using HarmonyLib;
using static TheOtherRoles.TheOtherRoles;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Text;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;
using TheOtherRoles.CustomGameModes;
using TheOtherRoles.EnoFw.Kernel;
using TheOtherRoles.EnoFw.Roles.Crewmate;
using TheOtherRoles.EnoFw.Roles.Modifiers;
using TheOtherRoles.EnoFw.Roles.Neutral;
using TheOtherRoles.Modules;

namespace TheOtherRoles.Patches
{
    enum CustomGameOverReason
    {
        LoversWin = 10,
        TeamJackalWin = 11,
        MiniLose = 12,
        JesterWin = 13,
        ArsonistWin = 14,
        VultureWin = 15,
        ProsecutorWin = 16
    }

    enum WinCondition
    {
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

    static class AdditionalTempData
    {
        // Should be implemented using a proper GameOverReason in the future
        public static WinCondition winCondition = WinCondition.Default;
        public static List<WinCondition> additionalWinConditions = new List<WinCondition>();
        public static List<PlayerRoleInfo> playerRoles = new List<PlayerRoleInfo>();
        public static float timer = 0;

        public static void clear()
        {
            playerRoles.Clear();
            additionalWinConditions.Clear();
            winCondition = WinCondition.Default;
            timer = 0;
        }

        internal class PlayerRoleInfo
        {
            public string PlayerName { get; set; }
            public List<RoleInfo> Roles { get; set; }
            public RoleInfo PreviousRole { get; set; }
            public int TasksCompleted { get; set; }
            public int TasksTotal { get; set; }
            public bool IsGuesser { get; set; }
            public int? Kills { get; set; }
        }
    }


    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    public class OnGameEndPatch
    {
        private static GameOverReason gameOverReason;

        public static void Prefix(AmongUsClient __instance, [HarmonyArgument(0)] ref EndGameResult endGameResult)
        {
            gameOverReason = endGameResult.GameOverReason;
            if ((int)endGameResult.GameOverReason >= 10) endGameResult.GameOverReason = GameOverReason.ImpostorByKill;

            // Reset zoomed out ghosts
            Helpers.toggleZoom(reset: true);
        }

        public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ref EndGameResult endGameResult)
        {
            AdditionalTempData.clear();

            foreach (PlayerControl playerControl in CachedPlayer.AllPlayers)
            {
                var roles = RoleInfo.getRoleInfoForPlayer(playerControl);
                var (tasksCompleted, tasksTotal) = TasksHandler.taskInfo(playerControl.Data);
                bool isGuesser = HandleGuesser.isGuesserGm && HandleGuesser.isGuesser(playerControl.PlayerId);
                int? killCount = GameHistory.deadPlayers.FindAll(x =>
                    x.killerIfExisting != null && x.killerIfExisting.PlayerId == playerControl.PlayerId).Count;
                if (killCount == 0 &&
                    !(new List<RoleInfo>()
                              { RoleInfo.sheriff, RoleInfo.jackal, RoleInfo.sidekick, RoleInfo.thief, RoleInfo.fallen }
                          .Contains(RoleInfo.getRoleInfoForPlayer(playerControl, false).FirstOrDefault()) ||
                      playerControl.Data.Role.IsImpostor))
                {
                    killCount = null;
                }

                RoleInfo previousRole = null;

                if (Thief.Instance.PlayerStolen == playerControl && !Thief.Instance.BecomePartner)
                    previousRole = RoleInfo.getRoleInfoForPlayer(Thief.Instance.FormerThief, false).FirstOrDefault();
                if (Thief.Instance.FormerThief == playerControl) previousRole = RoleInfo.thief;

                AdditionalTempData.playerRoles.Add(new AdditionalTempData.PlayerRoleInfo()
                {
                    PlayerName = playerControl.Data.PlayerName, Roles = roles, PreviousRole = previousRole,
                    TasksTotal = tasksTotal, TasksCompleted = tasksCompleted, IsGuesser = isGuesser, Kills = killCount
                });
            }

            // Remove Jester, Arsonist, Vulture, Jackal, former Jackals and Sidekick from winners (if they win, they'll be readded)
            List<PlayerControl> notWinners = new List<PlayerControl>();
            if (Jester.Instance.Player != null) notWinners.Add(Jester.Instance.Player);
            if (Sidekick.Instance.Player != null) notWinners.Add(Sidekick.Instance.Player);
            if (Jackal.Instance.Player != null) notWinners.Add(Jackal.Instance.Player);
            if (Arsonist.Instance.Player != null) notWinners.Add(Arsonist.Instance.Player);
            if (Vulture.Instance.Player != null) notWinners.Add(Vulture.Instance.Player);
            if (Lawyer.Instance.Player != null) notWinners.Add(Lawyer.Instance.Player);
            if (Pursuer.Instance.Player != null) notWinners.Add(Pursuer.Instance.Player);
            if (Thief.Instance.Player != null) notWinners.Add(Thief.Instance.Player);
            if (Fallen.Instance.Player != null) notWinners.Add(Fallen.Instance.Player);

            notWinners.AddRange(Jackal.Instance.FormerJackals);

            List<WinningPlayerData> winnersToRemove = new List<WinningPlayerData>();
            foreach (WinningPlayerData winner in TempData.winners.GetFastEnumerator())
            {
                if (notWinners.Any(x => x.Data.PlayerName == winner.PlayerName)) winnersToRemove.Add(winner);
            }

            foreach (var winner in winnersToRemove) TempData.winners.Remove(winner);

            bool jesterWin = Jester.Instance.Player != null &&
                             gameOverReason == (GameOverReason)CustomGameOverReason.JesterWin;
            bool arsonistWin = Arsonist.Instance.Player != null &&
                               gameOverReason == (GameOverReason)CustomGameOverReason.ArsonistWin;
            bool miniLose = Mini.Instance.Player != null &&
                            gameOverReason == (GameOverReason)CustomGameOverReason.MiniLose;
            bool loversWin = Lovers.Instance.ExistingAndAlive &&
                             (gameOverReason == (GameOverReason)CustomGameOverReason.LoversWin ||
                              (GameManager.Instance.DidHumansWin(gameOverReason) &&
                               !Lovers.Instance
                                   .ExistingWithKiller)); // Either they win if they are among the last 3 players, or they win if they are both Crewmates and both alive and the Crew wins (Team Imp/Jackal Lovers can only win solo wins)
            bool teamJackalWin = gameOverReason == (GameOverReason)CustomGameOverReason.TeamJackalWin &&
                                 ((Jackal.Instance.Player != null && !Jackal.Instance.Player.Data.IsDead) ||
                                  (Sidekick.Instance.Player != null && !Sidekick.Instance.Player.Data.IsDead));
            bool vultureWin = Vulture.Instance.Player != null &&
                              gameOverReason == (GameOverReason)CustomGameOverReason.VultureWin;
            bool prosecutorWin = Lawyer.Instance.Player != null &&
                                 gameOverReason == (GameOverReason)CustomGameOverReason.ProsecutorWin;

            bool isPursurerLose = jesterWin || arsonistWin || miniLose || vultureWin || teamJackalWin;

            // Mini lose
            if (miniLose)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                WinningPlayerData wpd = new WinningPlayerData(Mini.Instance.Player.Data);
                wpd.IsYou = false; // If "no one is the Mini", it will display the Mini, but also show defeat to everyone
                TempData.winners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.MiniLose;
            }

            // Jester win
            else if (jesterWin)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                WinningPlayerData wpd = new WinningPlayerData(Jester.Instance.Player.Data);
                TempData.winners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.JesterWin;
            }

            // Arsonist win
            else if (arsonistWin)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                WinningPlayerData wpd = new WinningPlayerData(Arsonist.Instance.Player.Data);
                TempData.winners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.ArsonistWin;
            }

            // Vulture win
            else if (vultureWin)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                WinningPlayerData wpd = new WinningPlayerData(Vulture.Instance.Player.Data);
                TempData.winners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.VultureWin;
            }

            // Jester win
            else if (prosecutorWin)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                WinningPlayerData wpd = new WinningPlayerData(Lawyer.Instance.Player.Data);
                TempData.winners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.ProsecutorWin;
            }

            // Lovers win conditions
            else if (loversWin)
            {
                // Double win for lovers, crewmates also win
                if (!Lovers.Instance.ExistingWithKiller)
                {
                    AdditionalTempData.winCondition = WinCondition.LoversTeamWin;
                    TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                    foreach (PlayerControl p in CachedPlayer.AllPlayers)
                    {
                        if (p == null) continue;
                        if (Lovers.Instance.Is(p))
                            TempData.winners.Add(new WinningPlayerData(p.Data));
                        else if (p == Pursuer.Instance.Player && !Pursuer.Instance.Player.Data.IsDead)
                            TempData.winners.Add(new WinningPlayerData(p.Data));
                        else if (p != Jester.Instance.Player && p != Jackal.Instance.Player &&
                                 p != Sidekick.Instance.Player && p != Arsonist.Instance.Player &&
                                 p != Vulture.Instance.Player && !Jackal.Instance.FormerJackals.Contains(p) &&
                                 !p.Data.Role.IsImpostor)
                            TempData.winners.Add(new WinningPlayerData(p.Data));
                    }
                }
                // Lovers solo win
                else
                {
                    AdditionalTempData.winCondition = WinCondition.LoversSoloWin;
                    TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                    TempData.winners.Add(new WinningPlayerData(Lovers.Instance.Lover1.Data));
                    TempData.winners.Add(new WinningPlayerData(Lovers.Instance.Lover2.Data));
                }
            }

            // Jackal win condition (should be implemented using a proper GameOverReason in the future)
            else if (teamJackalWin)
            {
                // Jackal wins if nobody except jackal is alive
                AdditionalTempData.winCondition = WinCondition.JackalWin;
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                WinningPlayerData wpd = new WinningPlayerData(Jackal.Instance.Player.Data);
                wpd.IsImpostor = false;
                TempData.winners.Add(wpd);
                // If there is a sidekick. The sidekick also wins
                if (Sidekick.Instance.Player != null)
                {
                    WinningPlayerData wpdSidekick = new WinningPlayerData(Sidekick.Instance.Player.Data);
                    wpdSidekick.IsImpostor = false;
                    TempData.winners.Add(wpdSidekick);
                }

                foreach (var player in Jackal.Instance.FormerJackals)
                {
                    WinningPlayerData wpdFormerJackal = new WinningPlayerData(player.Data);
                    wpdFormerJackal.IsImpostor = false;
                    TempData.winners.Add(wpdFormerJackal);
                }
            }

            // Possible Additional winner: Lawyer
            if (Lawyer.Instance.Player != null && Lawyer.Instance.Target != null &&
                (!Lawyer.Instance.Target.Data.IsDead || Lawyer.Instance.Target == Jester.Instance.Player) &&
                !Pursuer.Instance.NotAckedExiled && !Lawyer.Instance.IsProsecutor)
            {
                WinningPlayerData winningClient = null;
                foreach (WinningPlayerData winner in TempData.winners.GetFastEnumerator())
                {
                    if (winner.PlayerName == Lawyer.Instance.Target.Data.PlayerName)
                        winningClient = winner;
                }

                if (winningClient != null)
                {
                    // The Lawyer wins if the client is winning (and alive, but if he wasn't the Lawyer shouldn't exist anymore)
                    if (TempData.winners.ToArray().All(x => x.PlayerName != Lawyer.Instance.Player.Data.PlayerName))
                        TempData.winners.Add(new WinningPlayerData(Lawyer.Instance.Player.Data));
                    AdditionalTempData.additionalWinConditions.Add(WinCondition
                        .AdditionalLawyerBonusWin); // The Lawyer wins together with the client
                }
            }

            // Possible Additional winner: Pursuer
            if (Pursuer.Instance.Player != null && !Pursuer.Instance.Player.Data.IsDead &&
                !Pursuer.Instance.NotAckedExiled && !isPursurerLose &&
                !TempData.winners.ToArray().Any(x => x.IsImpostor))
            {
                if (TempData.winners.ToArray().All(x => x.PlayerName != Pursuer.Instance.Player.Data.PlayerName))
                    TempData.winners.Add(new WinningPlayerData(Pursuer.Instance.Player.Data));
                AdditionalTempData.additionalWinConditions.Add(WinCondition.AdditionalAlivePursuerWin);
            }

            AdditionalTempData.timer = ((float)(DateTime.UtcNow - HideNSeek.startTime).TotalMilliseconds) / 1000;

            // Reset Settings
            if (TORMapOptions.gameMode == CustomGamemodes.HideNSeek) ShipStatusPatch.resetVanillaSettings();
            KernelRpc.Local_ResetVariables();
            EventUtility.gameEndsUpdate();
        }
    }

    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
    public class EndGameManagerSetUpPatch
    {
        public static void Postfix(EndGameManager __instance)
        {
            // Delete and readd PoolablePlayers always showing the name and role of the player
            foreach (PoolablePlayer pb in __instance.transform.GetComponentsInChildren<PoolablePlayer>())
            {
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
            for (int i = 0; i < list.Count; i++)
            {
                WinningPlayerData winningPlayerData2 = list[i];
                int num2 = (i % 2 == 0) ? -1 : 1;
                int num3 = (i + 1) / 2;
                float num4 = (float)num3 / (float)num;
                float num5 = Mathf.Lerp(1f, 0.75f, num4);
                float num6 = (float)((i == 0) ? -8 : -1);
                PoolablePlayer poolablePlayer =
                    UnityEngine.Object.Instantiate<PoolablePlayer>(__instance.PlayerPrefab, __instance.transform);
                poolablePlayer.transform.localPosition = new Vector3(1f * (float)num2 * (float)num3 * num5,
                    FloatRange.SpreadToEdges(-1.125f, 0f, num3, num), num6 + (float)num3 * 0.01f) * 0.9f;
                float num7 = Mathf.Lerp(1f, 0.65f, num4) * 0.9f;
                Vector3 vector = new Vector3(num7, num7, 1f);
                poolablePlayer.transform.localScale = vector;
                poolablePlayer.UpdateFromPlayerOutfit((GameData.PlayerOutfit)winningPlayerData2,
                    PlayerMaterial.MaskType.ComplexUI, winningPlayerData2.IsDead, true);
                if (winningPlayerData2.IsDead)
                {
                    poolablePlayer.cosmetics.currentBodySprite.BodySprite.sprite =
                        poolablePlayer.cosmetics.currentBodySprite.GhostSprite;
                    poolablePlayer.SetDeadFlipX(i % 2 == 0);
                }
                else
                {
                    poolablePlayer.SetFlipX(i % 2 == 0);
                }

                poolablePlayer.cosmetics.nameText.color = Color.white;
                poolablePlayer.cosmetics.nameText.transform.localScale =
                    new Vector3(1f / vector.x, 1f / vector.y, 1f / vector.z);
                poolablePlayer.cosmetics.nameText.transform.localPosition = new Vector3(
                    poolablePlayer.cosmetics.nameText.transform.localPosition.x,
                    poolablePlayer.cosmetics.nameText.transform.localPosition.y, -15f);
                poolablePlayer.cosmetics.nameText.text = winningPlayerData2.PlayerName;

                foreach (var data in AdditionalTempData.playerRoles)
                {
                    if (data.PlayerName != winningPlayerData2.PlayerName) continue;
                    var roles =
                        poolablePlayer.cosmetics.nameText.text +=
                            $"\n{string.Join("\n", data.Roles.Select(x => Helpers.cs(x.color, x.name)))}";
                }
            }

            // Additional code
            GameObject bonusText = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
            bonusText.transform.position = new Vector3(__instance.WinText.transform.position.x,
                __instance.WinText.transform.position.y - 0.5f, __instance.WinText.transform.position.z);
            bonusText.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            TMPro.TMP_Text textRenderer = bonusText.GetComponent<TMPro.TMP_Text>();
            textRenderer.text = "";

            if (AdditionalTempData.winCondition == WinCondition.JesterWin)
            {
                textRenderer.text = "Jester Wins";
                textRenderer.color = Jester.Instance.Color;
            }
            else if (AdditionalTempData.winCondition == WinCondition.ArsonistWin)
            {
                textRenderer.text = "Arsonist Wins";
                textRenderer.color = Arsonist.Instance.Color;
            }
            else if (AdditionalTempData.winCondition == WinCondition.VultureWin)
            {
                textRenderer.text = "Vulture Wins";
                textRenderer.color = Vulture.Instance.Color;
            }
            else if (AdditionalTempData.winCondition == WinCondition.ProsecutorWin)
            {
                textRenderer.text = "Prosecutor Wins";
                textRenderer.color = Lawyer.Instance.Color;
            }
            else if (AdditionalTempData.winCondition == WinCondition.LoversTeamWin)
            {
                textRenderer.text = "Lovers And Crewmates Win";
                textRenderer.color = Lovers.Instance.Color;
                __instance.BackgroundBar.material.SetColor("_Color", Lovers.Instance.Color);
            }
            else if (AdditionalTempData.winCondition == WinCondition.LoversSoloWin)
            {
                textRenderer.text = "Lovers Win";
                textRenderer.color = Lovers.Instance.Color;
                __instance.BackgroundBar.material.SetColor("_Color", Lovers.Instance.Color);
            }
            else if (AdditionalTempData.winCondition == WinCondition.JackalWin)
            {
                textRenderer.text = "Team Jackal Wins";
                textRenderer.color = Jackal.Instance.Color;
            }
            else if (AdditionalTempData.winCondition == WinCondition.MiniLose)
            {
                textRenderer.text = "Mini died";
                textRenderer.color = Mini.Instance.Color;
            }

            foreach (WinCondition cond in AdditionalTempData.additionalWinConditions)
            {
                if (cond == WinCondition.AdditionalLawyerBonusWin)
                {
                    textRenderer.text += $"\n{Helpers.cs(Lawyer.Instance.Color, "The Lawyer wins with the client")}";
                }
                else if (cond == WinCondition.AdditionalAlivePursuerWin)
                {
                    textRenderer.text += $"\n{Helpers.cs(Pursuer.Instance.Color, "The Pursuer survived")}";
                }
            }

            if (TORMapOptions.showRoleSummary || HideNSeek.isHideNSeekGM)
            {
                var position = Camera.main.ViewportToWorldPoint(new Vector3(0f, 1f, Camera.main.nearClipPlane));
                GameObject roleSummary = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
                roleSummary.transform.position =
                    new Vector3(__instance.Navigation.ExitButton.transform.position.x + 0.1f, position.y - 0.1f, -214f);
                roleSummary.transform.localScale = new Vector3(1f, 1f, 1f);

                var roleSummaryText = new StringBuilder();
                if (HideNSeek.isHideNSeekGM)
                {
                    int minutes = (int)AdditionalTempData.timer / 60;
                    int seconds = (int)AdditionalTempData.timer % 60;
                    roleSummaryText.AppendLine($"<color=#FAD934FF>Time: {minutes:00}:{seconds:00}</color> \n");
                }

                roleSummaryText.AppendLine("Players and roles at the end of the game:");
                foreach (AdditionalTempData.PlayerRoleInfo data in AdditionalTempData.playerRoles)
                {
                    var roles = string.Join(" ", data.Roles.Select(x => Helpers.cs(x.color, x.name)));
                    var previousRole = data.PreviousRole != null
                        ? $"{Helpers.cs(data.PreviousRole.color, data.PreviousRole.name)} => "
                        : "";
                    if (previousRole != "")
                    {
                        string[] splittedRoles = roles.Split(" ");
                        splittedRoles[splittedRoles.Length - 1] =
                            previousRole + splittedRoles[splittedRoles.Length - 1];
                        roles = string.Join(" ", splittedRoles);
                    }

                    if (data.IsGuesser) roles += " (Guesser)";
                    var taskInfo = data.TasksTotal > 0
                        ? $" - <color=#FAD934FF>({data.TasksCompleted}/{data.TasksTotal})</color>"
                        : "";
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
    class CheckEndCriteriaPatch
    {
        public static bool Prefix(ShipStatus __instance)
        {
            if (!GameData.Instance || (CustomGuid.IsDevMode && CustomGuid.NoEndGame)) return false;

            if (DestroyableSingleton<TutorialManager>
                .InstanceExists) // InstanceExists | Don't check Custom Criteria when in Tutorial
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

        private static bool CheckAndEndGameForMiniLose(ShipStatus __instance)
        {
            if (Mini.Instance.TriggerMiniLose)
            {
                //__instance.enabled = false;
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.MiniLose, false);
                return true;
            }

            return false;
        }

        private static bool CheckAndEndGameForJesterWin(ShipStatus __instance)
        {
            if (Jester.Instance.TriggerJesterWin)
            {
                //__instance.enabled = false;
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.JesterWin, false);
                return true;
            }

            return false;
        }

        private static bool CheckAndEndGameForArsonistWin(ShipStatus __instance)
        {
            if (Arsonist.Instance.TriggerArsonistWin)
            {
                //__instance.enabled = false;
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.ArsonistWin, false);
                return true;
            }

            return false;
        }

        private static bool CheckAndEndGameForVultureWin(ShipStatus __instance)
        {
            if (Vulture.Instance.TriggerVultureWin)
            {
                //__instance.enabled = false;
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.VultureWin, false);
                return true;
            }

            return false;
        }

        private static bool CheckAndEndGameForSabotageWin(ShipStatus __instance)
        {
            if (MapUtilities.Systems == null) return false;
            var systemType = MapUtilities.Systems.ContainsKey(SystemTypes.LifeSupp)
                ? MapUtilities.Systems[SystemTypes.LifeSupp]
                : null;
            if (systemType != null)
            {
                LifeSuppSystemType lifeSuppSystemType = systemType.TryCast<LifeSuppSystemType>();
                if (lifeSuppSystemType != null && lifeSuppSystemType.Countdown < 0f)
                {
                    EndGameForSabotage(__instance);
                    lifeSuppSystemType.Countdown = 10000f;
                    return true;
                }
            }

            var systemType2 = MapUtilities.Systems.ContainsKey(SystemTypes.Reactor)
                ? MapUtilities.Systems[SystemTypes.Reactor]
                : null;
            if (systemType2 == null)
            {
                systemType2 = MapUtilities.Systems.ContainsKey(SystemTypes.Laboratory)
                    ? MapUtilities.Systems[SystemTypes.Laboratory]
                    : null;
            }

            if (systemType2 != null)
            {
                ICriticalSabotage criticalSystem = systemType2.TryCast<ICriticalSabotage>();
                if (criticalSystem != null && criticalSystem.Countdown < 0f)
                {
                    EndGameForSabotage(__instance);
                    criticalSystem.ClearSabotage();
                    return true;
                }
            }

            return false;
        }

        private static bool CheckAndEndGameForTaskWin(ShipStatus __instance)
        {
            if (HideNSeek.isHideNSeekGM && !HideNSeek.taskWinPossible) return false;
            var totalTasks = 0;
            var completedTasks = 0;
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.Data.Role.IsImpostor) continue;
                if (Helpers.isNeutral(player)) continue;
                if (Lovers.Instance.ExistingWithKiller && Lovers.Instance.ExistingAndAlive &&
                    Lovers.Instance.Is(player)) continue;
                var (tasksCompleted, tasksTotal) = TasksHandler.taskInfo(player.Data);
                totalTasks += tasksTotal;
                completedTasks += tasksCompleted;
            }

            if (totalTasks == 0 || completedTasks < totalTasks) return false;
            
            //__instance.enabled = false;
            GameManager.Instance.RpcEndGame(GameOverReason.HumansByTask, false);
            return true;
        }

        private static bool CheckAndEndGameForProsecutorWin(ShipStatus __instance)
        {
            if (Lawyer.Instance.TriggerProsecutorWin)
            {
                //__instance.enabled = false;
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.ProsecutorWin, false);
                return true;
            }

            return false;
        }

        private static bool CheckAndEndGameForLoverWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.TeamLoversAlive == 2 && statistics.TotalAlive <= 3)
            {
                //__instance.enabled = false;
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.LoversWin, false);
                return true;
            }

            return false;
        }

        private static bool CheckAndEndGameForJackalWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.TeamJackalAlive >= statistics.TotalAlive - statistics.TeamJackalAlive &&
                statistics.TeamImpostorsAlive == 0 &&
                !(statistics.TeamJackalHasAliveLover && statistics.TeamLoversAlive == 2))
            {
                //__instance.enabled = false;
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.TeamJackalWin, false);
                return true;
            }

            return false;
        }

        private static bool CheckAndEndGameForImpostorWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (HideNSeek.isHideNSeekGM)
                if ((0 != statistics.TotalAlive - statistics.TeamImpostorsAlive))
                    return false;

            if (statistics.TeamImpostorsAlive >= statistics.TotalAlive - statistics.TeamImpostorsAlive &&
                statistics.TeamJackalAlive == 0 &&
                !(statistics.TeamImpostorHasAliveLover && statistics.TeamLoversAlive == 2))
            {
                //__instance.enabled = false;
                GameOverReason endReason;
                switch (TempData.LastDeathReason)
                {
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

        private static bool CheckAndEndGameForCrewmateWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (HideNSeek.isHideNSeekGM && HideNSeek.timer <= 0 && !HideNSeek.isWaitingTimer)
            {
                //__instance.enabled = false;
                GameManager.Instance.RpcEndGame(GameOverReason.HumansByVote, false);
                return true;
            }

            if (statistics.TeamImpostorsAlive == 0 && statistics.TeamJackalAlive == 0)
            {
                //__instance.enabled = false;
                GameManager.Instance.RpcEndGame(GameOverReason.HumansByVote, false);
                return true;
            }

            return false;
        }

        private static void EndGameForSabotage(ShipStatus __instance)
        {
            //__instance.enabled = false;
            GameManager.Instance.RpcEndGame(GameOverReason.ImpostorBySabotage, false);
            return;
        }
    }

    internal class PlayerStatistics
    {
        public int TeamImpostorsAlive { get; set; }
        public int TeamJackalAlive { get; set; }
        public int TeamLoversAlive { get; set; }
        public int TotalAlive { get; set; }
        public bool TeamImpostorHasAliveLover { get; set; }
        public bool TeamJackalHasAliveLover { get; set; }

        public PlayerStatistics(ShipStatus __instance)
        {
            GetPlayerCounts();
        }

        private bool isLover(GameData.PlayerInfo p)
        {
            return Lovers.Instance.Is(p.PlayerId);
        }

        private void GetPlayerCounts()
        {
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

                        bool lover = isLover(playerInfo);
                        if (lover) numLoversAlive++;

                        if (playerInfo.Role.IsImpostor)
                        {
                            numImpostorsAlive++;
                            if (lover) impLover = true;
                        }

                        if (Jackal.Instance.Player != null && Jackal.Instance.Player.PlayerId == playerInfo.PlayerId)
                        {
                            numJackalAlive++;
                            if (lover) jackalLover = true;
                        }

                        if (Sidekick.Instance.Player != null &&
                            Sidekick.Instance.Player.PlayerId == playerInfo.PlayerId)
                        {
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