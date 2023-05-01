using HarmonyLib;
using System;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;
using TheOtherRoles.Objects;
using System.Collections.Generic;
using System.Linq;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;
using TheOtherRoles.CustomGameModes;
using AmongUs.GameOptions;
using TheOtherRoles.Customs.Modifiers;
using TheOtherRoles.Customs.Roles.Crewmate;
using TheOtherRoles.Customs.Roles.Impostor;
using TheOtherRoles.Customs.Roles.Neutral;

namespace TheOtherRoles.Patches {
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    class HudManagerUpdatePatch
    {
        private static Dictionary<byte, (string name, Color color)> TagColorDict = new();
        static void resetNameTagsAndColors() {
            var localPlayer = CachedPlayer.LocalPlayer.PlayerControl;
            var myData = CachedPlayer.LocalPlayer.Data;
            var amImpostor = myData.Role.IsImpostor;
            var morphTimerNotUp = Morphling.morphTimer > 0f;
            var morphTargetNotNull = Morphling.morphTarget != null;

            var dict = TagColorDict;
            dict.Clear();
            
            foreach (var data in GameData.Instance.AllPlayers.GetFastEnumerator())
            {
                var player = data.Object;
                string text = data.PlayerName;
                Color color;
                if (player)
                {
                    var playerName = text;
                    if (morphTimerNotUp && morphTargetNotNull && Singleton<Morphling>.Instance.Player == player) playerName = Morphling.morphTarget.Data.PlayerName;
                    var nameText = player.cosmetics.nameText;
                
                    nameText.text = Helpers.hidePlayerName(localPlayer, player) ? "" : playerName;
                    nameText.color = color = amImpostor && data.Role.IsImpostor ? Palette.ImpostorRed : Color.white;
                    nameText.color = nameText.color.SetAlpha(Chameleon.visibility(player.PlayerId));
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
            p.cosmetics.nameText.color = color.SetAlpha(Chameleon.visibility(p.PlayerId));
            if (MeetingHud.Instance != null)
                foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                    if (player.NameText != null && p.PlayerId == player.TargetPlayerId)
                        player.NameText.color = color;
        }

        static void setNameColors() {
            var localPlayer = CachedPlayer.LocalPlayer.PlayerControl;
            var localRole = RoleInfo.getRoleInfoForPlayer(localPlayer, false).FirstOrDefault();
            setPlayerNameColor(localPlayer, localRole.color);

            /*if (Singleton<Jester>.Instance.Player != null && Singleton<Jester>.Instance.Player == localPlayer)
                setPlayerNameColor(Singleton<Jester>.Instance.Player, Singleton<Jester>.Instance.Color);
            else if (Singleton<Mayor>.Instance.Player != null && Singleton<Mayor>.Instance.Player == localPlayer)
                setPlayerNameColor(Singleton<Mayor>.Instance.Player, Singleton<Mayor>.Instance.Color);
            else if (Singleton<Engineer>.Instance.Player != null && Singleton<Engineer>.Instance.Player == localPlayer)
                setPlayerNameColor(Singleton<Engineer>.Instance.Player, Singleton<Engineer>.Instance.Color);
            else if (Singleton<Sheriff>.Instance.Player != null && Singleton<Sheriff>.Instance.Player == localPlayer) {
                setPlayerNameColor(Singleton<Sheriff>.Instance.Player, Singleton<Sheriff>.Instance.Color);
                if (Singleton<Deputy>.Instance.Player != null && Deputy.knowsSheriff) {
                    setPlayerNameColor(Singleton<Deputy>.Instance.Player, Deputy.color);
                }
            } else*/
            if (Singleton<Deputy>.Instance.Player != null && Singleton<Deputy>.Instance.Player == localPlayer) {
                setPlayerNameColor(Singleton<Deputy>.Instance.Player, Deputy.color);
                if (Singleton<Sheriff>.Instance.Player != null && Deputy.knowsSheriff) {
                    setPlayerNameColor(Singleton<Sheriff>.Instance.Player, Singleton<Sheriff>.Instance.Color);
                }
            } /*else if (Singleton<Portalmaker>.Instance.Player != null && Singleton<Portalmaker>.Instance.Player == localPlayer)
                setPlayerNameColor(Singleton<Portalmaker>.Instance.Player, Singleton<Portalmaker>.Instance.Color);
            else if (Singleton<Lighter>.Instance.Player != null && Singleton<Lighter>.Instance.Player == localPlayer)
                setPlayerNameColor(Singleton<Lighter>.Instance.Player, Singleton<Lighter>.Instance.Color);
            else if (Singleton<Detective>.Instance.Player != null && Singleton<Detective>.Instance.Player == localPlayer)
                setPlayerNameColor(Singleton<Detective>.Instance.Player, Singleton<Detective>.Instance.Color);
            else if (Singleton<TimeMaster>.Instance.Player != null && Singleton<TimeMaster>.Instance.Player == localPlayer)
                setPlayerNameColor(Singleton<TimeMaster>.Instance.Player, Singleton<TimeMaster>.Instance.Color);
            else if (Singleton<Medic>.Instance.Player != null && Singleton<Medic>.Instance.Player == localPlayer)
                setPlayerNameColor(Singleton<Medic>.Instance.Player, Singleton<Medic>.Instance.Color);
            else if (Shifter.shifter != null && Shifter.shifter == localPlayer)
                setPlayerNameColor(Shifter.shifter, Shifter.color);
            else if (Singleton<Swapper>.Instance.Player != null && Singleton<Swapper>.Instance.Player == localPlayer)
                setPlayerNameColor(Singleton<Swapper>.Instance.Player, Singleton<Swapper>.Instance.Color);
            else if (Singleton<Seer>.Instance.Player != null && Singleton<Seer>.Instance.Player == localPlayer)
                setPlayerNameColor(Singleton<Seer>.Instance.Player, Singleton<Seer>.Instance.Color);
            else if (Singleton<Hacker>.Instance.Player != null && Singleton<Hacker>.Instance.Player == localPlayer)
                setPlayerNameColor(Singleton<Hacker>.Instance.Player, Singleton<Hacker>.Instance.Color);
            else if (Singleton<Tracker>.Instance.Player != null && Singleton<Tracker>.Instance.Player == localPlayer)
                setPlayerNameColor(Singleton<Tracker>.Instance.Player, Singleton<Tracker>.Instance.Color);
            else if (Singleton<Snitch>.Instance.Player != null && Singleton<Snitch>.Instance.Player == localPlayer)
                setPlayerNameColor(Singleton<Snitch>.Instance.Player, Singleton<Snitch>.Instance.Color);*/
            else if (Singleton<Jackal>.Instance.Player != null && Singleton<Jackal>.Instance.Player == localPlayer) {
                // Jackal can see his sidekick
                setPlayerNameColor(Singleton<Jackal>.Instance.Player, Singleton<Jackal>.Instance.Color);
                if (Singleton<Sidekick>.Instance.Player != null) {
                    setPlayerNameColor(Singleton<Sidekick>.Instance.Player, Singleton<Jackal>.Instance.Color);
                }
                if (Jackal.fakeSidekick != null) {
                    setPlayerNameColor(Jackal.fakeSidekick, Singleton<Jackal>.Instance.Color);
                }
            }
            /*else if (Singleton<Spy>.Instance.Player != null && Singleton<Spy>.Instance.Player == localPlayer) {
                setPlayerNameColor(Singleton<Spy>.Instance.Player, Singleton<Spy>.Instance.Color);
            } else if (Singleton<SecurityGuard>.Instance.Player != null && Singleton<SecurityGuard>.Instance.Player == localPlayer) {
                setPlayerNameColor(Singleton<SecurityGuard>.Instance.Player, Singleton<SecurityGuard>.Instance.Color);
            } else if (Singleton<Arsonist>.Instance.Player != null && Singleton<Arsonist>.Instance.Player == localPlayer) {
                setPlayerNameColor(Singleton<Arsonist>.Instance.Player, Singleton<Arsonist>.Instance.Color);
            } else if (Singleton<NiceGuesser>.Instance.Player != null && Singleton<NiceGuesser>.Instance.Player == localPlayer) {
                setPlayerNameColor(Singleton<NiceGuesser>.Instance.Player, Guesser.color);
            } else if (Singleton<EvilGuesser>.Instance.Player != null && Singleton<EvilGuesser>.Instance.Player == localPlayer) {
                setPlayerNameColor(Singleton<EvilGuesser>.Instance.Player, Palette.ImpostorRed);
            } else if (Singleton<Vulture>.Instance.Player != null && Singleton<Vulture>.Instance.Player == localPlayer) {
                setPlayerNameColor(Singleton<Vulture>.Instance.Player, Singleton<Vulture>.Instance.Color);
            } else if (Singleton<Medium>.Instance.Player != null && Singleton<Medium>.Instance.Player == localPlayer) {
                setPlayerNameColor(Singleton<Medium>.Instance.Player, Singleton<Medium>.Instance.Color);
            } else if (Singleton<Trapper>.Instance.Player != null && Singleton<Trapper>.Instance.Player == localPlayer) {
                setPlayerNameColor(Singleton<Trapper>.Instance.Player, Singleton<Trapper>.Instance.Color);
            } else if (Singleton<Lawyer>.Instance.Player != null && Singleton<Lawyer>.Instance.Player == localPlayer) {
                setPlayerNameColor(Singleton<Lawyer>.Instance.Player, Singleton<Lawyer>.Instance.Color);
            } else if (Singleton<Pursuer>.Instance.Player != null && Singleton<Pursuer>.Instance.Player == localPlayer) {
                setPlayerNameColor(Singleton<Pursuer>.Instance.Player, Singleton<Pursuer>.Instance.Color);
            }*/

            // No else if here, as a Lover of team Jackal needs the colors
            if (Singleton<Sidekick>.Instance.Player != null && Singleton<Sidekick>.Instance.Player == localPlayer) {
                // Sidekick can see the jackal
                setPlayerNameColor(Singleton<Sidekick>.Instance.Player, Singleton<Sidekick>.Instance.Color);
                if (Singleton<Jackal>.Instance.Player != null) {
                    setPlayerNameColor(Singleton<Jackal>.Instance.Player, Singleton<Jackal>.Instance.Color);
                }
            }

            // No else if here, as the Impostors need the Spy name to be colored
            if (Singleton<Spy>.Instance.Player != null && localPlayer.Data.Role.IsImpostor) {
                setPlayerNameColor(Singleton<Spy>.Instance.Player, Singleton<Spy>.Instance.Color);
            }
            if (Singleton<Sidekick>.Instance.Player != null && Sidekick.wasTeamRed && localPlayer.Data.Role.IsImpostor) {
                setPlayerNameColor(Singleton<Sidekick>.Instance.Player, Singleton<Spy>.Instance.Color);
            }
            if (Singleton<Jackal>.Instance.Player != null && Jackal.wasTeamRed && localPlayer.Data.Role.IsImpostor) {
                setPlayerNameColor(Singleton<Jackal>.Instance.Player, Singleton<Spy>.Instance.Color);
            }

            // Crewmate roles with no changes: Mini
            // Impostor roles with no changes: Morphling, Camouflager, Vampire, Godfather, Eraser, Janitor, Cleaner, Warlock, BountyHunter,  Witch and Mafioso
        }

        static void setNameTags() {
            // Mafia
            if (CachedPlayer.LocalPlayer != null && CachedPlayer.LocalPlayer.Data.Role.IsImpostor) {
                foreach (PlayerControl player in CachedPlayer.AllPlayers)
                    if (Godfather.godfather != null && Godfather.godfather == player)
                            player.cosmetics.nameText.text = player.Data.PlayerName + " (G)";
                    else if (Mafioso.mafioso != null && Mafioso.mafioso == player)
                            player.cosmetics.nameText.text = player.Data.PlayerName + " (M)";
                    else if (Janitor.janitor != null && Janitor.janitor == player)
                            player.cosmetics.nameText.text = player.Data.PlayerName + " (J)";
                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (Godfather.godfather != null && Godfather.godfather.PlayerId == player.TargetPlayerId)
                            player.NameText.text = Godfather.godfather.Data.PlayerName + " (G)";
                        else if (Mafioso.mafioso != null && Mafioso.mafioso.PlayerId == player.TargetPlayerId)
                            player.NameText.text = Mafioso.mafioso.Data.PlayerName + " (M)";
                        else if (Janitor.janitor != null && Janitor.janitor.PlayerId == player.TargetPlayerId)
                            player.NameText.text = Janitor.janitor.Data.PlayerName + " (J)";
            }

            // Lovers
            if (Lovers.lover1 != null && Lovers.lover2 != null && (Lovers.lover1 == CachedPlayer.LocalPlayer.PlayerControl || Lovers.lover2 == CachedPlayer.LocalPlayer.PlayerControl)) {
                string suffix = Helpers.cs(Lovers.color, " ♥");
                Lovers.lover1.cosmetics.nameText.text += suffix;
                Lovers.lover2.cosmetics.nameText.text += suffix;

                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (Lovers.lover1.PlayerId == player.TargetPlayerId || Lovers.lover2.PlayerId == player.TargetPlayerId)
                            player.NameText.text += suffix;
            }

            // Lawyer or Prosecutor
            if ((Singleton<Lawyer>.Instance.Player != null && Singleton<Lawyer>.Instance.Target != null && Singleton<Lawyer>.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl)) {
                Color color = Singleton<Lawyer>.Instance.Color;
                PlayerControl target = Singleton<Lawyer>.Instance.Target;
                string suffix = Helpers.cs(color, " §");
                target.cosmetics.nameText.text += suffix;

                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (player.TargetPlayerId == target.PlayerId)
                            player.NameText.text += suffix;
            }

            // Former Thief
            if (Thief.formerThief != null && (Thief.formerThief == CachedPlayer.LocalPlayer.PlayerControl || CachedPlayer.LocalPlayer.PlayerControl.Data.IsDead)) {
                string suffix = Helpers.cs(Singleton<Thief>.Instance.Color, " $");
                Thief.formerThief.cosmetics.nameText.text += suffix;
                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (player.TargetPlayerId == Thief.formerThief.PlayerId)
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
            if (Medic.shielded == null) return;

            if (Medic.shielded.Data.IsDead || Singleton<Medic>.Instance.Player == null || Singleton<Medic>.Instance.Player.Data.IsDead) {
                Medic.shielded = null;
            }
        }

        static void timerUpdate() {
            var dt = Time.deltaTime;
            Singleton<Hacker>.Instance.PlayerTimer -= dt;
            Trickster.lightsOutTimer -= dt;
            Tracker.corpsesTrackingTimer -= dt;
            Ninja.invisibleTimer -= dt;
            HideNSeek.timer -= dt;
            foreach (byte key in Deputy.handcuffedKnows.Keys)
                Deputy.handcuffedKnows[key] -= dt;
        }

        public static void miniUpdate() {
            if (Mini.mini == null || Camouflager.camouflageTimer > 0f || Mini.mini == Singleton<Morphling>.Instance.Player && Morphling.morphTimer > 0f || Mini.mini == Singleton<Ninja>.Instance.Player && Ninja.isInvisble || SurveillanceMinigamePatch.nightVisionIsActive) return;
                
            float growingProgress = Mini.growingProgress();
            float scale = growingProgress * 0.35f + 0.35f;
            string suffix = "";
            if (growingProgress != 1f)
                suffix = " <color=#FAD934FF>(" + Mathf.FloorToInt(growingProgress * 18) + ")</color>"; 
            if (!Mini.isGrowingUpInMeeting && MeetingHud.Instance != null && Mini.ageOnMeetingStart != 0 && !(Mini.ageOnMeetingStart >= 18))
                suffix = " <color=#FAD934FF>(" + Mini.ageOnMeetingStart + ")</color>";

            Mini.mini.cosmetics.nameText.text += suffix;
            if (MeetingHud.Instance != null) {
                foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                    if (player.NameText != null && Mini.mini.PlayerId == player.TargetPlayerId)
                        player.NameText.text += suffix;
            }

            if (Singleton<Morphling>.Instance.Player != null && Morphling.morphTarget == Mini.mini && Morphling.morphTimer > 0f)
                Singleton<Morphling>.Instance.Player.cosmetics.nameText.text += suffix;
        }

        static void updateImpostorKillButton(HudManager __instance) {
            if (!CachedPlayer.LocalPlayer.Data.Role.IsImpostor) return;
            if (MeetingHud.Instance) {
                __instance.KillButton.Hide();
                return;
            }
            bool enabled = true;
            if (Singleton<Vampire>.Instance.Player != null && Singleton<Vampire>.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl)
                enabled = false;
            else if (Mafioso.mafioso != null && Mafioso.mafioso == CachedPlayer.LocalPlayer.PlayerControl && Godfather.godfather != null && !Godfather.godfather.Data.IsDead)
                enabled = false;
            else if (Janitor.janitor != null && Janitor.janitor == CachedPlayer.LocalPlayer.PlayerControl)
                enabled = false;
            
            if (enabled) __instance.KillButton.Show();
            else __instance.KillButton.Hide();

            if (Deputy.handcuffedKnows.ContainsKey(CachedPlayer.LocalPlayer.PlayerId) && Deputy.handcuffedKnows[CachedPlayer.LocalPlayer.PlayerId] > 0) __instance.KillButton.Hide();
        }

        static void updateReportButton(HudManager __instance) {
            if (Deputy.handcuffedKnows.ContainsKey(CachedPlayer.LocalPlayer.PlayerId) && Deputy.handcuffedKnows[CachedPlayer.LocalPlayer.PlayerId] > 0 || MeetingHud.Instance) __instance.ReportButton.Hide();
            else if (!__instance.ReportButton.isActiveAndEnabled) __instance.ReportButton.Show();
        }
         
        static void updateVentButton(HudManager __instance)
        {
            if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return;
            if (Deputy.handcuffedKnows.ContainsKey(CachedPlayer.LocalPlayer.PlayerId) && Deputy.handcuffedKnows[CachedPlayer.LocalPlayer.PlayerId] > 0 || MeetingHud.Instance) __instance.ImpostorVentButton.Hide();
            else if (CachedPlayer.LocalPlayer.PlayerControl.roleCanUseVents() && !__instance.ImpostorVentButton.isActiveAndEnabled) __instance.ImpostorVentButton.Show();

        }

        static void updateUseButton(HudManager __instance) {
            if (MeetingHud.Instance) __instance.UseButton.Hide();
        }

        static void updateSabotageButton(HudManager __instance) {
            if (MeetingHud.Instance || TORMapOptions.gameMode == CustomGamemodes.HideNSeek) __instance.SabotageButton.Hide();
        }

        static void updateMapButton(HudManager __instance) {
            if (Singleton<Trapper>.Instance.Player == null || !(CachedPlayer.LocalPlayer.PlayerId == Singleton<Trapper>.Instance.Player.PlayerId) || __instance == null || __instance.MapButton.HeldButtonSprite == null) return;
            __instance.MapButton.HeldButtonSprite.color = Trapper.playersOnMap.Any() ? Singleton<Trapper>.Instance.Color : Color.white;
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
