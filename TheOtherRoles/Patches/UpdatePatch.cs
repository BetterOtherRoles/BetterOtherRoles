using HarmonyLib;
using UnityEngine;
using TheOtherRoles.Objects;
using System.Collections.Generic;
using System.Linq;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;
using TheOtherRoles.CustomGameModes;
using AmongUs.GameOptions;
using TheOtherRoles.EnoFw.Roles.Crewmate;
using TheOtherRoles.EnoFw.Roles.Impostor;
using TheOtherRoles.EnoFw.Roles.Modifiers;
using TheOtherRoles.EnoFw.Roles.Neutral;

namespace TheOtherRoles.Patches {
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    class HudManagerUpdatePatch
    {
        private static Dictionary<byte, (string name, Color color)> TagColorDict = new();
        static void resetNameTagsAndColors() {
            var localPlayer = CachedPlayer.LocalPlayer.PlayerControl;
            var myData = CachedPlayer.LocalPlayer.Data;
            var amImpostor = myData.Role.IsImpostor;
            var morphTimerNotUp = Morphling.Instance.MorphTimer > 0f;
            var morphTargetNotNull = Morphling.Instance.MorphTarget != null;

            var dict = TagColorDict;
            dict.Clear();
            
            foreach (var data in GameData.Instance.AllPlayers.GetFastEnumerator())
            {
                var player = data.Object;
                string text = data.PlayerName;
                Color color;
                if (player != null)
                {
                    var playerName = text;
                    if (morphTimerNotUp && morphTargetNotNull && Morphling.Instance.Player == player) playerName = Morphling.Instance.MorphTarget.Data.PlayerName;
                    var nameText = player.cosmetics.nameText;
                
                    nameText.text = Helpers.hidePlayerName(localPlayer, player) ? "" : playerName;
                    nameText.color = color = amImpostor && data.Role.IsImpostor ? Palette.ImpostorRed : Color.white;
                    nameText.color = nameText.color.SetAlpha(Chameleon.Visibility(player.PlayerId));
                }
                else
                {
                    color = Color.white;
                }
                
                
                dict.Add(data.PlayerId, (text, color));
            }
            
            if (MeetingHud.Instance != null) 
            {
                foreach (PlayerVoteArea playerVoteArea in MeetingHud.Instance.playerStates)
                {
                    var data = dict[playerVoteArea.TargetPlayerId];
                    var text = playerVoteArea.NameText;
                    text.text = data.name;
                    text.color = data.color;
                }
            }
        }

        static void setPlayerNameColor(PlayerControl p, Color color) {
            p.cosmetics.nameText.color = color.SetAlpha(Chameleon.Visibility(p.PlayerId));
            if (MeetingHud.Instance != null)
                foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                    if (player.NameText != null && p.PlayerId == player.TargetPlayerId)
                        player.NameText.color = color;
        }

        static void setNameColors() {
            var localPlayer = CachedPlayer.LocalPlayer.PlayerControl;
            var localRole = RoleInfo.getRoleInfoForPlayer(localPlayer, false).FirstOrDefault();
            setPlayerNameColor(localPlayer, localRole.color);

            /*if (Jester.jester != null && Jester.jester == localPlayer)
                setPlayerNameColor(Jester.jester, Jester.color);
            else if (Mayor.mayor != null && Mayor.mayor == localPlayer)
                setPlayerNameColor(Mayor.mayor, Mayor.color);
            else if (Engineer.engineer != null && Engineer.engineer == localPlayer)
                setPlayerNameColor(Engineer.engineer, Engineer.color);
            else if (Sheriff.sheriff != null && Sheriff.sheriff == localPlayer) {
                setPlayerNameColor(Sheriff.sheriff, Sheriff.color);
                if (Deputy.deputy != null && Deputy.knowsSheriff) {
                    setPlayerNameColor(Deputy.deputy, Deputy.color);
                }
            } else*/
            if (Deputy.Instance.Player != null && Deputy.Instance.Player == localPlayer) {
                setPlayerNameColor(Deputy.Instance.Player, Deputy.Instance.Color);
                if (Sheriff.Instance.HasPlayer && Deputy.Instance.KnowsSheriff) {
                    setPlayerNameColor(Sheriff.Instance.Player, Sheriff.Instance.Color);
                }
            } /*else if (Portalmaker.portalmaker != null && Portalmaker.portalmaker == localPlayer)
                setPlayerNameColor(Portalmaker.portalmaker, Portalmaker.color);
            else if (Lighter.lighter != null && Lighter.lighter == localPlayer)
                setPlayerNameColor(Lighter.lighter, Lighter.color);
            else if (Detective.detective != null && Detective.detective == localPlayer)
                setPlayerNameColor(Detective.detective, Detective.color);
            else if (TimeMaster.timeMaster != null && TimeMaster.timeMaster == localPlayer)
                setPlayerNameColor(TimeMaster.timeMaster, TimeMaster.Instance.Color);
            else if (Medic.medic != null && Medic.medic == localPlayer)
                setPlayerNameColor(Medic.medic, Medic.color);
            else if (Shifter.Instance.Player != null && Shifter.Instance.Player == localPlayer)
                setPlayerNameColor(Shifter.Instance.Player, Shifter.color);
            else if (Swapper.swapper != null && Swapper.swapper == localPlayer)
                setPlayerNameColor(Swapper.swapper, Swapper.Instance.Color);
            else if (Seer.seer != null && Seer.seer == localPlayer)
                setPlayerNameColor(Seer.seer, Seer.color);
            else if (Hacker.hacker != null && Hacker.hacker == localPlayer)
                setPlayerNameColor(Hacker.hacker, Hacker.color);
            else if (Tracker.tracker != null && Tracker.tracker == localPlayer)
                setPlayerNameColor(Tracker.tracker, Tracker.Instance.Color);
            else if (Snitch.snitch != null && Snitch.snitch == localPlayer)
                setPlayerNameColor(Snitch.snitch, Snitch.color);*/
            else if (Jackal.Instance.Player != null && Jackal.Instance.Player == localPlayer) {
                // Jackal can see his sidekick
                setPlayerNameColor(Jackal.Instance.Player, Jackal.Instance.Color);
                if (Sidekick.Instance.Player != null) {
                    setPlayerNameColor(Sidekick.Instance.Player, Jackal.Instance.Color);
                }
                if (Jackal.Instance.FakeSidekick != null) {
                    setPlayerNameColor(Jackal.Instance.FakeSidekick, Jackal.Instance.Color);
                }
            }
            /*else if (Spy.spy != null && Spy.spy == localPlayer) {
                setPlayerNameColor(Spy.spy, Spy.color);
            } else if (SecurityGuard.securityGuard != null && SecurityGuard.securityGuard == localPlayer) {
                setPlayerNameColor(SecurityGuard.securityGuard, SecurityGuard.color);
            } else if (Arsonist.arsonist != null && Arsonist.arsonist == localPlayer) {
                setPlayerNameColor(Arsonist.arsonist, Arsonist.Instance.Color);
            } else if (Guesser.niceGuesser != null && Guesser.niceGuesser == localPlayer) {
                setPlayerNameColor(Guesser.niceGuesser, Guesser.color);
            } else if (Guesser.evilGuesser != null && Guesser.evilGuesser == localPlayer) {
                setPlayerNameColor(Guesser.evilGuesser, Palette.ImpostorRed);
            } else if (Vulture.vulture != null && Vulture.vulture == localPlayer) {
                setPlayerNameColor(Vulture.vulture, Vulture.Instance.Color);
            } else if (Medium.medium != null && Medium.medium == localPlayer) {
                setPlayerNameColor(Medium.medium, Medium.color);
            } else if (Trapper.trapper != null && Trapper.trapper == localPlayer) {
                setPlayerNameColor(Trapper.trapper, Trapper.color);
            } else if (Lawyer.lawyer != null && Lawyer.lawyer == localPlayer) {
                setPlayerNameColor(Lawyer.lawyer, Lawyer.color);
            } else if (Pursuer.pursuer != null && Pursuer.pursuer == localPlayer) {
                setPlayerNameColor(Pursuer.pursuer, Pursuer.color);
            }*/

            // No else if here, as a Lover of team Jackal needs the colors
            if (Sidekick.Instance.Player != null && Sidekick.Instance.Player == localPlayer) {
                // Sidekick can see the jackal
                setPlayerNameColor(Sidekick.Instance.Player, Sidekick.Instance.Color);
                if (Jackal.Instance.Player != null) {
                    setPlayerNameColor(Jackal.Instance.Player, Jackal.Instance.Color);
                }
            }

            // No else if here, as the Impostors need the Spy name to be colored
            if (Spy.Instance.Player != null && localPlayer.Data.Role.IsImpostor) {
                setPlayerNameColor(Spy.Instance.Player, Spy.Instance.Color);
            }
            if (Sidekick.Instance.Player != null && Sidekick.Instance.WasTeamRed && localPlayer.Data.Role.IsImpostor) {
                setPlayerNameColor(Sidekick.Instance.Player, Spy.Instance.Color);
            }
            if (Jackal.Instance.Player != null && Jackal.Instance.WasTeamRed && localPlayer.Data.Role.IsImpostor) {
                setPlayerNameColor(Jackal.Instance.Player, Spy.Instance.Color);
            }

            // Crewmate roles with no changes: Mini
            // Impostor roles with no changes: Morphling, Camouflager, Vampire, Godfather, Eraser, Janitor, Cleaner, Warlock, BountyHunter,  Witch and Mafioso
        }

        static void setNameTags() {
            // Mafia
            if (CachedPlayer.LocalPlayer != null && CachedPlayer.LocalPlayer.Data.Role.IsImpostor) {
                foreach (PlayerControl player in CachedPlayer.AllPlayers)
                    if (Godfather.Instance.Player != null && Godfather.Instance.Player == player)
                            player.cosmetics.nameText.text = player.Data.PlayerName + " (G)";
                    else if (Mafioso.Instance.Player != null && Mafioso.Instance.Player == player)
                            player.cosmetics.nameText.text = player.Data.PlayerName + " (M)";
                    else if (Janitor.Instance.Player != null && Janitor.Instance.Player == player)
                            player.cosmetics.nameText.text = player.Data.PlayerName + " (J)";
                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (Godfather.Instance.Player != null && Godfather.Instance.Player.PlayerId == player.TargetPlayerId)
                            player.NameText.text = Godfather.Instance.Player.Data.PlayerName + " (G)";
                        else if (Mafioso.Instance.Player != null && Mafioso.Instance.Player.PlayerId == player.TargetPlayerId)
                            player.NameText.text = Mafioso.Instance.Player.Data.PlayerName + " (M)";
                        else if (Janitor.Instance.Player != null && Janitor.Instance.Player.PlayerId == player.TargetPlayerId)
                            player.NameText.text = Janitor.Instance.Player.Data.PlayerName + " (J)";
            }

            // Lovers
            if (Lovers.Instance.Lover1 != null && Lovers.Instance.Lover2 != null && (Lovers.Instance.Lover1 == CachedPlayer.LocalPlayer.PlayerControl || Lovers.Instance.Lover2 == CachedPlayer.LocalPlayer.PlayerControl)) {
                string suffix = Helpers.cs(Lovers.Instance.Color, " ♥");
                Lovers.Instance.Lover1.cosmetics.nameText.text += suffix;
                Lovers.Instance.Lover2.cosmetics.nameText.text += suffix;

                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (Lovers.Instance.Lover1.PlayerId == player.TargetPlayerId || Lovers.Instance.Lover2.PlayerId == player.TargetPlayerId)
                            player.NameText.text += suffix;
            }

            // Lawyer or Prosecutor
            if ((Lawyer.Instance.Player != null && Lawyer.Instance.Target != null && Lawyer.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl)) {
                Color color = Lawyer.Instance.Color;
                PlayerControl target = Lawyer.Instance.Target;
                string suffix = Helpers.cs(color, " §");
                target.cosmetics.nameText.text += suffix;

                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (player.TargetPlayerId == target.PlayerId)
                            player.NameText.text += suffix;
            }

            // Former Thief
            if (Thief.Instance.FormerThief != null && (Thief.Instance.FormerThief == CachedPlayer.LocalPlayer.PlayerControl || CachedPlayer.LocalPlayer.PlayerControl.Data.IsDead)) {
                string suffix = Helpers.cs(Thief.Instance.Color, " $");
                Thief.Instance.FormerThief.cosmetics.nameText.text += suffix;
                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (player.TargetPlayerId == Thief.Instance.FormerThief.PlayerId)
                            player.NameText.text += suffix;
            }

            // Display lighter / darker color for all alive players
            if (CachedPlayer.LocalPlayer != null && MeetingHud.Instance != null && TORMapOptions.showLighterDarker) {
                foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates) {
                    var target = Helpers.playerById(player.TargetPlayerId);
                    if (target != null)  player.NameText.text += $" ({(Helpers.isLighterColor(target.Data.DefaultOutfit.ColorId) ? "L" : "D")})";
                }
            }
        }

        static void updateShielded() {
            if (Medic.Instance.Shielded == null) return;

            if (Medic.Instance.Shielded.Data.IsDead || !Medic.Instance.HasPlayer || Medic.Instance.Player.Data.IsDead) {
                Medic.Instance.Shielded = null;
            }
        }

        static void timerUpdate() {
            var dt = Time.deltaTime;
            Hacker.Instance.Timer -= dt;
            Trickster.Instance.LightsOutTimer -= dt;
            Tracker.Instance.CorpsesTrackingTimer -= dt;
            Ninja.Instance.InvisibilityTimer -= dt;
            HideNSeek.timer -= dt;
            foreach (byte key in Deputy.Instance.HandcuffedKnows.Keys)
                Deputy.Instance.HandcuffedKnows[key] -= dt;
        }

        public static void miniUpdate() {
            if (Mini.Instance.Player == null || Camouflager.Instance.CamouflageTimer > 0f || Mini.Instance.Player == Morphling.Instance.Player && Morphling.Instance.MorphTimer > 0f || Mini.Instance.Player == Ninja.Instance.Player && Ninja.Instance.IsInvisible || SurveillanceMinigamePatch.nightVisionIsActive) return;
                
            float growingProgress = Mini.Instance.GrowingProgress();
            float scale = growingProgress * 0.35f + 0.35f;
            string suffix = "";
            if (growingProgress != 1f)
                suffix = " <color=#FAD934FF>(" + Mathf.FloorToInt(growingProgress * 18) + ")</color>"; 
            if (!Mini.Instance.IsGrowingUpInMeeting && MeetingHud.Instance != null && Mini.Instance.AgeOnMeetingStart != 0 && !(Mini.Instance.AgeOnMeetingStart >= 18))
                suffix = " <color=#FAD934FF>(" + Mini.Instance.AgeOnMeetingStart + ")</color>";

            Mini.Instance.Player.cosmetics.nameText.text += suffix;
            if (MeetingHud.Instance != null) {
                foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                    if (player.NameText != null && Mini.Instance.Player.PlayerId == player.TargetPlayerId)
                        player.NameText.text += suffix;
            }

            if (Morphling.Instance.Player != null && Morphling.Instance.MorphTarget == Mini.Instance.Player && Morphling.Instance.MorphTimer > 0f)
                Morphling.Instance.Player.cosmetics.nameText.text += suffix;
        }

        static void updateImpostorKillButton(HudManager __instance) {
            if (!CachedPlayer.LocalPlayer.Data.Role.IsImpostor) return;
            if (MeetingHud.Instance) {
                __instance.KillButton.Hide();
                return;
            }
            
            if(Undertaker.Instance.Player && CachedPlayer.LocalPlayer.PlayerControl == Undertaker.Instance.Player && Undertaker.Instance.DraggedBody != null) 
            {
                __instance.KillButton.graphic.color = Palette.DisabledClear;
                __instance.KillButton.buttonLabelText.color = Palette.DisabledClear;
                __instance.KillButton.cooldownTimerText.color = Palette.DisabledClear;
                __instance.KillButton.graphic.material.SetFloat(Shader.PropertyToID("_Desat"), 1f);
                return;
            }

            bool enabled = true;
            if (Vampire.Instance.Player != null && Vampire.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl)
                enabled = false;
            else if (Mafioso.Instance.Player != null && Mafioso.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl && Godfather.Instance.Player != null && !Godfather.Instance.Player.Data.IsDead)
                enabled = false;
            else if (Janitor.Instance.Player != null && Janitor.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl)
                enabled = false;
            
            if (enabled) __instance.KillButton.Show();
            else __instance.KillButton.Hide();

            if (Deputy.Instance.HandcuffedKnows.ContainsKey(CachedPlayer.LocalPlayer.PlayerId) && Deputy.Instance.HandcuffedKnows[CachedPlayer.LocalPlayer.PlayerId] > 0) __instance.KillButton.Hide();
        }

        static void updateReportButton(HudManager __instance) {
            if (Deputy.Instance.HandcuffedKnows.ContainsKey(CachedPlayer.LocalPlayer.PlayerId) && Deputy.Instance.HandcuffedKnows[CachedPlayer.LocalPlayer.PlayerId] > 0 || MeetingHud.Instance) 
            { 
                __instance.ReportButton.Hide();
            } 
            
            else if (!__instance.ReportButton.isActiveAndEnabled) 
            {
                __instance.ReportButton.Show();
            } 
            /*
            if(Undertaker.undertaker != null && CachedPlayer.LocalPlayer.PlayerControl == Undertaker.undertaker && Undertaker.Instance.DraggedBody != null && Undertaker.disableReportButton) 
            {
                __instance.ReportButton.graphic.color = Palette.DisabledClear;
                __instance.ReportButton.buttonLabelText.color = Palette.DisabledClear;
                __instance.ReportButton.graphic.material.SetFloat(Shader.PropertyToID("_Desat"), 1f);
            }
            */
        }
         
        static void updateVentButton(HudManager __instance)
        {
            if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return;
            if (Deputy.Instance.HandcuffedKnows.ContainsKey(CachedPlayer.LocalPlayer.PlayerId) && Deputy.Instance.HandcuffedKnows[CachedPlayer.LocalPlayer.PlayerId] > 0 || MeetingHud.Instance) __instance.ImpostorVentButton.Hide();
            else if (CachedPlayer.LocalPlayer.PlayerControl.roleCanUseVents() && !__instance.ImpostorVentButton.isActiveAndEnabled) __instance.ImpostorVentButton.Show();

        }

        static void updateUseButton(HudManager __instance) {
            if (MeetingHud.Instance) __instance.UseButton.Hide();
        }

        static void updateSabotageButton(HudManager __instance) {
            if (MeetingHud.Instance || TORMapOptions.gameMode == CustomGamemodes.HideNSeek) __instance.SabotageButton.Hide();
        }

        static void updateMapButton(HudManager __instance) {
            if (Trapper.Instance.Player == null || !(CachedPlayer.LocalPlayer.PlayerId == Trapper.Instance.Player.PlayerId) || __instance == null || __instance.MapButton.HeldButtonSprite == null) return;
            __instance.MapButton.HeldButtonSprite.color = Trapper.Instance.PlayersOnMap.Any() ? Trapper.Instance.Color : Color.white;
        }

        static void Postfix(HudManager __instance)
        {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;
            
            EventUtility.Update();

            CustomButton.HudUpdate();
            resetNameTagsAndColors();
            setNameColors();
            updateShielded();
            setNameTags();

            // Impostors
            updateImpostorKillButton(__instance);
            // Timer updates
            timerUpdate();
            // Mini
            miniUpdate();

            // Deputy Sabotage, Use and Vent Button Disabling
            updateReportButton(__instance);
            updateVentButton(__instance);
            // Meeting hide buttons if needed (used for the map usage, because closing the map would show buttons)
            updateSabotageButton(__instance);
            updateUseButton(__instance);
            updateMapButton(__instance);
            if (!MeetingHud.Instance) __instance.AbilityButton?.Update();
        }
    }
}
