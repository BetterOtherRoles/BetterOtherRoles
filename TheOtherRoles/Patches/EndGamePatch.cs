using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Text;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;
using TheOtherRoles.CustomGameModes;
using TheOtherRoles.Customs.Modifiers;
using TheOtherRoles.Customs.Roles;
using TheOtherRoles.Customs.Roles.Crewmate;
using TheOtherRoles.Customs.Roles.Neutral;
using TheOtherRoles.EnoFramework.Kernel;

namespace TheOtherRoles.Patches {
    internal enum CustomGameOverReason {
        LoversWin = 10,
        TeamJackalWin = 11,
        MiniLose = 12,
        JesterWin = 13,
        ArsonistWin = 14,
        VultureWin = 15,
        ProsecutorWin = 16
    }

    internal enum WinCondition {
        Default,
        LoversTeamWin,
        LoversSoloWin,
        JesterWin,
        JackalWin,
        MiniLose,
        ArsonistWin,
        VultureWin,
        AdditionalLawyerBonusWin,
        AdditionalAlivePursuerWin,
        ProsecutorWin
    }

    static class AdditionalTempData {
        // Should be implemented using a proper GameOverReason in the future
        public static WinCondition winCondition = WinCondition.Default;
        public static List<WinCondition> additionalWinConditions = new List<WinCondition>();
        public static List<PlayerRoleInfo> playerRoles = new List<PlayerRoleInfo>();
        public static float timer = 0;

        public static void clear() {
            playerRoles.Clear();
            additionalWinConditions.Clear();
            winCondition = WinCondition.Default;
            timer = 0;
        }

        internal class PlayerRoleInfo {
            public string PlayerName { get; set; }
            public List<RoleInfo> Roles {get;set;}
            public int TasksCompleted  {get;set;}
            public int TasksTotal  {get;set;}
            public bool IsGuesser {get; set;}
            public int? Kills {get; set;}
        }
    }


    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    public class OnGameEndPatch {
        private static GameOverReason gameOverReason;
        public static void Prefix(AmongUsClient __instance, [HarmonyArgument(0)]ref EndGameResult endGameResult) {
            gameOverReason = endGameResult.GameOverReason;
            if ((int)endGameResult.GameOverReason >= 10) endGameResult.GameOverReason = GameOverReason.ImpostorByKill;

            // Reset zoomed out ghosts
            Helpers.toggleZoom(true);
        }

        public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)]ref EndGameResult endGameResult) {
            AdditionalTempData.clear();

            foreach(var playerControl in CachedPlayer.AllPlayers) {
                var roles = RoleInfo.getRoleInfoForPlayer(playerControl);
                var (tasksCompleted, tasksTotal) = TasksHandler.taskInfo(playerControl.Data);
                var isGuesser = HandleGuesser.isGuesserGm && HandleGuesser.isGuesser(playerControl.PlayerId);
                int? killCount = GameHistory.deadPlayers.FindAll(x => x.killerIfExisting != null && x.killerIfExisting.PlayerId == playerControl.PlayerId).Count;
                if (killCount == 0 && !(new List<RoleInfo> { RoleInfo.sheriff, RoleInfo.jackal, RoleInfo.sidekick, RoleInfo.thief }.Contains(RoleInfo.getRoleInfoForPlayer(playerControl, false).FirstOrDefault()) || playerControl.Data.Role.IsImpostor)) {
                    killCount = null;
                }
                AdditionalTempData.playerRoles.Add(new AdditionalTempData.PlayerRoleInfo() { PlayerName = playerControl.Data.PlayerName, Roles = roles, TasksTotal = tasksTotal, TasksCompleted = tasksCompleted, IsGuesser = isGuesser, Kills = killCount });
            }

            // Remove Jester, Arsonist, Vulture, Jackal, former Jackals and Sidekick from winners (if they win, they'll be readded)
            var notWinners = new List<PlayerControl>();
            foreach (var neutralRole in CustomRole.NeutralRoles)
            {
                if (neutralRole.Player != null) notWinners.Add(neutralRole.Player);
            }

            List<WinningPlayerData> winnersToRemove = new List<WinningPlayerData>();
            foreach (WinningPlayerData winner in TempData.winners.GetFastEnumerator()) {
                if (notWinners.Any(x => x.Data.PlayerName == winner.PlayerName)) winnersToRemove.Add(winner);
            }
            foreach (var winner in winnersToRemove) TempData.winners.Remove(winner);

            var jesterWin = Singleton<Jester>.Instance.Player != null && gameOverReason == (GameOverReason)CustomGameOverReason.JesterWin;
            var arsonistWin = Singleton<Arsonist>.Instance.Player != null && gameOverReason == (GameOverReason)CustomGameOverReason.ArsonistWin;
            var miniLose = Mini.mini != null && gameOverReason == (GameOverReason)CustomGameOverReason.MiniLose;
            var loversWin = Lovers.existingAndAlive() && (gameOverReason == (GameOverReason)CustomGameOverReason.LoversWin || (GameManager.Instance.DidHumansWin(gameOverReason) && !Lovers.existingWithKiller())); // Either they win if they are among the last 3 players, or they win if they are both Crewmates and both alive and the Crew wins (Team Imp/Jackal Lovers can only win solo wins)
            var teamJackalWin = gameOverReason == (GameOverReason)CustomGameOverReason.TeamJackalWin && ((Singleton<Jackal>.Instance.Player != null && !Singleton<Jackal>.Instance.Player.Data.IsDead) || (Singleton<Sidekick>.Instance.Player != null && !Singleton<Sidekick>.Instance.Player.Data.IsDead));
            var vultureWin = Singleton<Vulture>.Instance.Player != null && gameOverReason == (GameOverReason)CustomGameOverReason.VultureWin;
            var lawyerWin = Singleton<Lawyer>.Instance.Player != null && gameOverReason == (GameOverReason)CustomGameOverReason.ProsecutorWin;
            var prosecutorWin = Singleton<Prosecutor>.Instance.Player != null && gameOverReason == (GameOverReason)CustomGameOverReason.ProsecutorWin;

            var isPursuerLose = jesterWin || arsonistWin || miniLose || vultureWin || teamJackalWin;

            // Mini lose
            if (miniLose && Mini.mini != null) {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                var wpd = new WinningPlayerData(Mini.mini.Data)
                {
                    IsYou = false // If "no one is the Mini", it will display the Mini, but also show defeat to everyone
                };
                TempData.winners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.MiniLose;  
            }

            // Jester win
            else if (jesterWin && Singleton<Jester>.Instance.Player != null) {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                var wpd = new WinningPlayerData(Singleton<Jester>.Instance.Player.Data);
                TempData.winners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.JesterWin;
            }

            // Arsonist win
            else if (arsonistWin && Singleton<Arsonist>.Instance.Player != null) {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                var wpd = new WinningPlayerData(Singleton<Arsonist>.Instance.Player.Data);
                TempData.winners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.ArsonistWin;
            }

            // Vulture win
            else if (vultureWin && Singleton<Vulture>.Instance.Player != null) {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                var wpd = new WinningPlayerData(Singleton<Vulture>.Instance.Player.Data);
                TempData.winners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.VultureWin;
            }
            
            // Lawyer win
            else if (lawyerWin && Singleton<Lawyer>.Instance.Player != null) {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                var wpd = new WinningPlayerData(Singleton<Lawyer>.Instance.Player.Data);
                TempData.winners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.ProsecutorWin;
            }

            // Prosecutor win
            else if (prosecutorWin && Singleton<Prosecutor>.Instance.Player != null) {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                var wpd = new WinningPlayerData(Singleton<Prosecutor>.Instance.Player.Data);
                TempData.winners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.ProsecutorWin;
            }

            // Lovers win conditions
            else if (loversWin) {
                // Double win for lovers, crewmates also win
                if (!Lovers.existingWithKiller()) {
                    AdditionalTempData.winCondition = WinCondition.LoversTeamWin;
                    TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                    foreach (PlayerControl p in CachedPlayer.AllPlayers) {
                        if (p == null) continue;
                        if (p == Lovers.lover1 || p == Lovers.lover2)
                            TempData.winners.Add(new WinningPlayerData(p.Data));
                        else if (p == Singleton<Pursuer>.Instance.Player && !Singleton<Pursuer>.Instance.Player.Data.IsDead)
                            TempData.winners.Add(new WinningPlayerData(p.Data));
                        else if (p != Singleton<Jester>.Instance.Player && p != Singleton<Jackal>.Instance.Player && p != Singleton<Sidekick>.Instance.Player && p != Singleton<Arsonist>.Instance.Player && p != Singleton<Vulture>.Instance.Player && !p.Data.Role.IsImpostor)
                            TempData.winners.Add(new WinningPlayerData(p.Data));
                    }
                }
                // Lovers solo win
                else {
                    AdditionalTempData.winCondition = WinCondition.LoversSoloWin;
                    TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                    TempData.winners.Add(new WinningPlayerData(Lovers.lover1.Data));
                    TempData.winners.Add(new WinningPlayerData(Lovers.lover2.Data));
                }
            }
            
            // Jackal win condition (should be implemented using a proper GameOverReason in the future)
            else if (teamJackalWin && Singleton<Jackal>.Instance.Player != null) {
                // Jackal wins if nobody except jackal is alive
                AdditionalTempData.winCondition = WinCondition.JackalWin;
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                var wpd = new WinningPlayerData(Singleton<Jackal>.Instance.Player.Data)
                { 
                    IsImpostor = false
                };
                TempData.winners.Add(wpd);
                if (Singleton<ExJackal>.Instance.Player != null)
                {
                    var wpdExJackal = new WinningPlayerData(Singleton<ExJackal>.Instance.Player.Data) 
                    { 
                        IsImpostor = false 
                    };
                    TempData.winners.Add(wpdExJackal);
                }
                // If there is a sidekick. The sidekick also wins
                if (Singleton<Sidekick>.Instance.Player != null) {
                    var wpdSidekick = new WinningPlayerData(Singleton<Sidekick>.Instance.Player.Data) 
                    { 
                        IsImpostor = false 
                    };
                    TempData.winners.Add(wpdSidekick);
                }
            }

            // Possible Additional winner: Lawyer
            if (Singleton<Lawyer>.Instance.Player != null && Singleton<Lawyer>.Instance.Target != null && (!Singleton<Lawyer>.Instance.Target.Data.IsDead || Singleton<Lawyer>.Instance.Target == Singleton<Jester>.Instance.Player)) {
                WinningPlayerData winningClient = null;
                foreach (var winner in TempData.winners.GetFastEnumerator()) {
                    if (winner.PlayerName == Singleton<Lawyer>.Instance.Target.Data.PlayerName)
                        winningClient = winner;
                }
                if (winningClient != null) { // The Lawyer wins if the client is winning (and alive, but if he wasn't the Lawyer shouldn't exist anymore)
                    if (TempData.winners.ToArray().All(x => x.PlayerName != Singleton<Lawyer>.Instance.Player.Data.PlayerName))
                        TempData.winners.Add(new WinningPlayerData(Singleton<Lawyer>.Instance.Player.Data));
                    AdditionalTempData.additionalWinConditions.Add(WinCondition.AdditionalLawyerBonusWin); // The Lawyer wins together with the client
                } 
            }

            // Possible Additional winner: Pursuer
            if (Singleton<Pursuer>.Instance.Player != null && !Singleton<Pursuer>.Instance.Player.Data.IsDead && !isPursuerLose && !TempData.winners.ToArray().Any(x => x.IsImpostor)) {
                if (TempData.winners.ToArray().All(x => x.PlayerName != Singleton<Pursuer>.Instance.Player.Data.PlayerName))
                    TempData.winners.Add(new WinningPlayerData(Singleton<Pursuer>.Instance.Player.Data));
                AdditionalTempData.additionalWinConditions.Add(WinCondition.AdditionalAlivePursuerWin);
            }

            AdditionalTempData.timer = ((float)(DateTime.UtcNow - HideNSeek.startTime).TotalMilliseconds) / 1000;

            // Reset Settings
            if (TORMapOptions.gameMode == CustomGamemodes.HideNSeek) ShipStatusPatch.resetVanillaSettings();
            RPCProcedure.resetVariables();
            EventUtility.gameEndsUpdate();
        }
    }

    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
    public class EndGameManagerSetUpPatch {
        public static void Postfix(EndGameManager __instance) {
            // Delete and readd PoolablePlayers always showing the name and role of the player
            foreach (PoolablePlayer pb in __instance.transform.GetComponentsInChildren<PoolablePlayer>()) {
                UnityEngine.Object.Destroy(pb.gameObject);
            }
            int num = Mathf.CeilToInt(7.5f);
            List<WinningPlayerData> list = TempData.winners.ToArray().ToList().OrderBy(delegate(WinningPlayerData b)
            {
                if (!b.IsYou)
                {
                    return 0;
                }
                return -1;
            }).ToList<WinningPlayerData>();
            for (int i = 0; i < list.Count; i++) {
                WinningPlayerData winningPlayerData2 = list[i];
                int num2 = (i % 2 == 0) ? -1 : 1;
                int num3 = (i + 1) / 2;
                float num4 = (float)num3 / (float)num;
                float num5 = Mathf.Lerp(1f, 0.75f, num4);
                float num6 = (float)((i == 0) ? -8 : -1);
                PoolablePlayer poolablePlayer = UnityEngine.Object.Instantiate<PoolablePlayer>(__instance.PlayerPrefab, __instance.transform);
                poolablePlayer.transform.localPosition = new Vector3(1f * (float)num2 * (float)num3 * num5, FloatRange.SpreadToEdges(-1.125f, 0f, num3, num), num6 + (float)num3 * 0.01f) * 0.9f;
                float num7 = Mathf.Lerp(1f, 0.65f, num4) * 0.9f;
                Vector3 vector = new Vector3(num7, num7, 1f);
                poolablePlayer.transform.localScale = vector;
                poolablePlayer.UpdateFromPlayerOutfit((GameData.PlayerOutfit) winningPlayerData2, PlayerMaterial.MaskType.ComplexUI, winningPlayerData2.IsDead, true);
                if (winningPlayerData2.IsDead) {
                    poolablePlayer.cosmetics.currentBodySprite.BodySprite.sprite = poolablePlayer.cosmetics.currentBodySprite.GhostSprite;
                    poolablePlayer.SetDeadFlipX(i % 2 == 0);
                } else {
                    poolablePlayer.SetFlipX(i % 2 == 0);
                }

                poolablePlayer.cosmetics.nameText.color = Color.white;
                poolablePlayer.cosmetics.nameText.transform.localScale = new Vector3(1f / vector.x, 1f / vector.y, 1f / vector.z);
                var localPosition = poolablePlayer.cosmetics.nameText.transform.localPosition;
                localPosition = new Vector3(localPosition.x, localPosition.y, -15f);
                poolablePlayer.cosmetics.nameText.transform.localPosition = localPosition;
                poolablePlayer.cosmetics.nameText.text = winningPlayerData2.PlayerName;

                foreach(var data in AdditionalTempData.playerRoles) {
                    if (data.PlayerName != winningPlayerData2.PlayerName) continue;
                    var roles = 
                    poolablePlayer.cosmetics.nameText.text += $"\n{string.Join("\n", data.Roles.Select(x => Helpers.cs(x.color, x.name)))}";
                }
            }

            // Additional code
            GameObject bonusText = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
            var position1 = __instance.WinText.transform.position;
            bonusText.transform.position = new Vector3(position1.x, position1.y - 0.5f, position1.z);
            bonusText.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            TMPro.TMP_Text textRenderer = bonusText.GetComponent<TMPro.TMP_Text>();
            textRenderer.text = "";

            if (AdditionalTempData.winCondition == WinCondition.JesterWin) {
                textRenderer.text = "Jester Wins";
                textRenderer.color = Singleton<Jester>.Instance.Color;
            }
            else if (AdditionalTempData.winCondition == WinCondition.ArsonistWin) {
                textRenderer.text = "Arsonist Wins";
                textRenderer.color = Singleton<Arsonist>.Instance.Color;
            }
            else if (AdditionalTempData.winCondition == WinCondition.VultureWin) {
                textRenderer.text = "Vulture Wins";
                textRenderer.color = Singleton<Vulture>.Instance.Color;
            }
            else if (AdditionalTempData.winCondition == WinCondition.ProsecutorWin) {
                textRenderer.text = "Prosecutor Wins";
                textRenderer.color = Singleton<Lawyer>.Instance.Color;
            }
            else if (AdditionalTempData.winCondition == WinCondition.LoversTeamWin) {
                textRenderer.text = "Lovers And Crewmates Win";
                textRenderer.color = Lovers.color;
                __instance.BackgroundBar.material.SetColor("_Color", Lovers.color);
            } 
            else if (AdditionalTempData.winCondition == WinCondition.LoversSoloWin) {
                textRenderer.text = "Lovers Win";
                textRenderer.color = Lovers.color;
                __instance.BackgroundBar.material.SetColor("_Color", Lovers.color);
            }
            else if (AdditionalTempData.winCondition == WinCondition.JackalWin) {
                textRenderer.text = "Team Jackal Wins";
                textRenderer.color = Singleton<Jackal>.Instance.Color;
            }
            else if (AdditionalTempData.winCondition == WinCondition.MiniLose) {
                textRenderer.text = "Mini died";
                textRenderer.color = Mini.color;
            }

            foreach (WinCondition cond in AdditionalTempData.additionalWinConditions) {
                if (cond == WinCondition.AdditionalLawyerBonusWin) {
                    textRenderer.text += $"\n{Helpers.cs(Singleton<Lawyer>.Instance.Color, "The Lawyer wins with the client")}";
                } else if (cond == WinCondition.AdditionalAlivePursuerWin) {
                    textRenderer.text += $"\n{Helpers.cs(Singleton<Pursuer>.Instance.Color, "The Pursuer survived")}";
                }
            }

            if (TORMapOptions.showRoleSummary || HideNSeek.isHideNSeekGM) {
                var position = Camera.main.ViewportToWorldPoint(new Vector3(0f, 1f, Camera.main.nearClipPlane));
                GameObject roleSummary = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
                roleSummary.transform.position = new Vector3(__instance.Navigation.ExitButton.transform.position.x + 0.1f, position.y - 0.1f, -214f); 
                roleSummary.transform.localScale = new Vector3(1f, 1f, 1f);

                var roleSummaryText = new StringBuilder();
                if (HideNSeek.isHideNSeekGM) {
                    int minutes = (int)AdditionalTempData.timer / 60;
                    int seconds = (int)AdditionalTempData.timer % 60;
                    roleSummaryText.AppendLine($"<color=#FAD934FF>Time: {minutes:00}:{seconds:00}</color> \n");
                }
                roleSummaryText.AppendLine("Players and roles at the end of the game:");
                foreach(var data in AdditionalTempData.playerRoles) {
                    var roles = string.Join(" ", data.Roles.Select(x => Helpers.cs(x.color, x.name)));
                    if (data.IsGuesser) roles += " (Guesser)";
                    var taskInfo = data.TasksTotal > 0 ? $" - <color=#FAD934FF>({data.TasksCompleted}/{data.TasksTotal})</color>" : "";
                    if (data.Kills != null) taskInfo += $" - <color=#FF0000FF>(Kills: {data.Kills})</color>";
                    roleSummaryText.AppendLine($"{data.PlayerName} - {roles}{taskInfo}"); 
                }
                TMPro.TMP_Text roleSummaryTextMesh = roleSummary.GetComponent<TMPro.TMP_Text>();
                roleSummaryTextMesh.alignment = TMPro.TextAlignmentOptions.TopLeft;
                roleSummaryTextMesh.color = Color.white;
                roleSummaryTextMesh.fontSizeMin = 1.5f;
                roleSummaryTextMesh.fontSizeMax = 1.5f;
                roleSummaryTextMesh.fontSize = 1.5f;
                
                var roleSummaryTextMeshRectTransform = roleSummaryTextMesh.GetComponent<RectTransform>();
                roleSummaryTextMeshRectTransform.anchoredPosition = new Vector2(position.x + 3.5f, position.y - 0.1f);
                roleSummaryTextMesh.text = roleSummaryText.ToString();
            }
            AdditionalTempData.clear();
        }
    }

    [HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))] 
    class CheckEndCriteriaPatch {
        public static bool Prefix(ShipStatus __instance) {
            if (!GameData.Instance) return false;
            if (DestroyableSingleton<TutorialManager>.InstanceExists) // InstanceExists | Don't check Custom Criteria when in Tutorial
                return true;
            var statistics = new PlayerStatistics(__instance);
            if (CheckAndEndGameForMiniLose(__instance)) return false;
            if (CheckAndEndGameForJesterWin(__instance)) return false;
            if (CheckAndEndGameForArsonistWin(__instance)) return false;
            if (CheckAndEndGameForVultureWin(__instance)) return false;
            if (CheckAndEndGameForSabotageWin(__instance)) return false;
            if (CheckAndEndGameForTaskWin(__instance)) return false;
            if (CheckAndEndGameForProsecutorWin(__instance)) return false;
            if (CheckAndEndGameForLoverWin(__instance, statistics)) return false;
            if (CheckAndEndGameForJackalWin(__instance, statistics)) return false;
            if (CheckAndEndGameForImpostorWin(__instance, statistics)) return false;
            if (CheckAndEndGameForCrewmateWin(__instance, statistics)) return false;
            return false;
        }

        private static bool CheckAndEndGameForMiniLose(ShipStatus __instance) {
            if (Mini.triggerMiniLose) {
                //__instance.enabled = false;
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.MiniLose, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForJesterWin(ShipStatus __instance) {
            if (Singleton<Jester>.Instance.TriggerWin) {
                //__instance.enabled = false;
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.JesterWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForArsonistWin(ShipStatus __instance) {
            if (Singleton<Arsonist>.Instance.TriggerWin) {
                //__instance.enabled = false;
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.ArsonistWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForVultureWin(ShipStatus __instance) {
            if (Singleton<Vulture>.Instance.TriggerWin) {
                //__instance.enabled = false;
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.VultureWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForSabotageWin(ShipStatus __instance) {
            if (MapUtilities.Systems == null) return false;
            var systemType = MapUtilities.Systems.ContainsKey(SystemTypes.LifeSupp) ? MapUtilities.Systems[SystemTypes.LifeSupp] : null;
            if (systemType != null) {
                LifeSuppSystemType lifeSuppSystemType = systemType.TryCast<LifeSuppSystemType>();
                if (lifeSuppSystemType != null && lifeSuppSystemType.Countdown < 0f) {
                    EndGameForSabotage(__instance);
                    lifeSuppSystemType.Countdown = 10000f;
                    return true;
                }
            }
            var systemType2 = MapUtilities.Systems.ContainsKey(SystemTypes.Reactor) ? MapUtilities.Systems[SystemTypes.Reactor] : null;
            if (systemType2 == null) {
                systemType2 = MapUtilities.Systems.ContainsKey(SystemTypes.Laboratory) ? MapUtilities.Systems[SystemTypes.Laboratory] : null;
            }
            if (systemType2 != null) {
                ICriticalSabotage criticalSystem = systemType2.TryCast<ICriticalSabotage>();
                if (criticalSystem != null && criticalSystem.Countdown < 0f) {
                    EndGameForSabotage(__instance);
                    criticalSystem.ClearSabotage();
                    return true;
                }
            }
            return false;
        }

        private static bool CheckAndEndGameForTaskWin(ShipStatus __instance) {
            if (HideNSeek.isHideNSeekGM && !HideNSeek.taskWinPossible) return false;
            if (GameData.Instance.TotalTasks > 0 && GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks) {
                //__instance.enabled = false;
                GameManager.Instance.RpcEndGame(GameOverReason.HumansByTask, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForProsecutorWin(ShipStatus __instance) {
            if (Singleton<Lawyer>.Instance.TriggerWin || Singleton<Prosecutor>.Instance.TriggerWin) {
                //__instance.enabled = false;
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.ProsecutorWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForLoverWin(ShipStatus __instance, PlayerStatistics statistics) {
            if (statistics.TeamLoversAlive == 2 && statistics.TotalAlive <= 3) {
                //__instance.enabled = false;
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.LoversWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForJackalWin(ShipStatus __instance, PlayerStatistics statistics) {
            if (statistics.TeamJackalAlive >= statistics.TotalAlive - statistics.TeamJackalAlive && statistics.TeamImpostorsAlive == 0 && !(statistics.TeamJackalHasAliveLover && statistics.TeamLoversAlive == 2)) {
                //__instance.enabled = false;
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.TeamJackalWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForImpostorWin(ShipStatus __instance, PlayerStatistics statistics) {
            if (HideNSeek.isHideNSeekGM) 
                if ((0 != statistics.TotalAlive - statistics.TeamImpostorsAlive)) return false;

            if (statistics.TeamImpostorsAlive >= statistics.TotalAlive - statistics.TeamImpostorsAlive && statistics.TeamJackalAlive == 0 && !(statistics.TeamImpostorHasAliveLover && statistics.TeamLoversAlive == 2)) {
                //__instance.enabled = false;
                GameOverReason endReason;
                switch (TempData.LastDeathReason) {
                    case DeathReason.Exile:
                        endReason = GameOverReason.ImpostorByVote;
                        break;
                    case DeathReason.Kill:
                        endReason = GameOverReason.ImpostorByKill;
                        break;
                    default:
                        endReason = GameOverReason.ImpostorByVote;
                        break;
                }
                GameManager.Instance.RpcEndGame(endReason, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForCrewmateWin(ShipStatus __instance, PlayerStatistics statistics) {
            if (HideNSeek.isHideNSeekGM && HideNSeek.timer <= 0 && !HideNSeek.isWaitingTimer) {
                //__instance.enabled = false;
                GameManager.Instance.RpcEndGame(GameOverReason.HumansByVote, false);
                return true;
            }
            if (statistics.TeamImpostorsAlive == 0 && statistics.TeamJackalAlive == 0) {
                //__instance.enabled = false;
                GameManager.Instance.RpcEndGame(GameOverReason.HumansByVote, false);
                return true;
            }
            return false;
        }

        private static void EndGameForSabotage(ShipStatus __instance) {
            //__instance.enabled = false;
            GameManager.Instance.RpcEndGame(GameOverReason.ImpostorBySabotage, false);
            return;
        }

    }

    internal class PlayerStatistics {
        public int TeamImpostorsAlive {get;set;}
        public int TeamJackalAlive {get;set;}
        public int TeamLoversAlive {get;set;}
        public int TotalAlive {get;set;}
        public bool TeamImpostorHasAliveLover {get;set;}
        public bool TeamJackalHasAliveLover {get;set;}

        public PlayerStatistics(ShipStatus __instance) {
            GetPlayerCounts();
        }

        private bool isLover(GameData.PlayerInfo p) {
            return (Lovers.lover1 != null && Lovers.lover1.PlayerId == p.PlayerId) || (Lovers.lover2 != null && Lovers.lover2.PlayerId == p.PlayerId);
        }

        private void GetPlayerCounts() {
            int numJackalAlive = 0;
            int numImpostorsAlive = 0;
            int numLoversAlive = 0;
            int numTotalAlive = 0;
            bool impLover = false;
            bool jackalLover = false;

            foreach (var playerInfo in GameData.Instance.AllPlayers.GetFastEnumerator())
            {
                if (!playerInfo.Disconnected)
                {
                    if (!playerInfo.IsDead)
                    {
                        numTotalAlive++;

                        var lover = isLover(playerInfo);
                        if (lover) numLoversAlive++;

                        if (playerInfo.Role.IsImpostor) {
                            numImpostorsAlive++;
                            if (lover) impLover = true;
                        }
                        if (Singleton<Jackal>.Instance.Player != null && Singleton<Jackal>.Instance.Player.PlayerId == playerInfo.PlayerId) {
                            numJackalAlive++;
                            if (lover) jackalLover = true;
                        }
                        if (Singleton<ExJackal>.Instance.Player != null && Singleton<ExJackal>.Instance.Player.PlayerId == playerInfo.PlayerId) {
                            numJackalAlive++;
                            if (lover) jackalLover = true;
                        }
                        if (Singleton<Sidekick>.Instance.Player != null && Singleton<Sidekick>.Instance.Player.PlayerId == playerInfo.PlayerId) {
                            numJackalAlive++;
                            if (lover) jackalLover = true;
                        }
                    }
                }
            }

            TeamJackalAlive = numJackalAlive;
            TeamImpostorsAlive = numImpostorsAlive;
            TeamLoversAlive = numLoversAlive;
            TotalAlive = numTotalAlive;
            TeamImpostorHasAliveLover = impLover;
            TeamJackalHasAliveLover = jackalLover;
        }
    }
}
