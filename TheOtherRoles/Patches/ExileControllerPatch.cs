using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using static TheOtherRoles.TheOtherRoles;
using TheOtherRoles.Objects;
using System;
using TheOtherRoles.EnoFw.Kernel;
using TheOtherRoles.EnoFw.Modules;
using TheOtherRoles.EnoFw.Roles.Crewmate;
using TheOtherRoles.EnoFw.Roles.Impostor;
using TheOtherRoles.EnoFw.Roles.Modifiers;
using TheOtherRoles.EnoFw.Roles.Neutral;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;
using UnityEngine;

namespace TheOtherRoles.Patches {
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
    [HarmonyPriority(Priority.First)]
    class ExileControllerBeginPatch {
        public static GameData.PlayerInfo lastExiled;
        public static void Prefix(ExileController __instance, [HarmonyArgument(0)]ref GameData.PlayerInfo exiled, [HarmonyArgument(1)]bool tie) {
            lastExiled = exiled;

            // Medic shield
            if (Medic.Instance.Player != null && AmongUsClient.Instance.AmHost && Medic.Instance.FutureShielded != null && !Medic.Instance.Player.Data.IsDead) { // We need to send the RPC from the host here, to make sure that the order of shifting and setting the shield is correct(for that reason the futureShifted and futureShielded are being synced)
                Medic.MedicSetShielded(Medic.Instance.FutureShielded.PlayerId);
            }
            if (Medic.Instance.UsedShield) Medic.Instance.MeetingAfterShielding = true;  // Has to be after the setting of the shield

            // Shifter shift
            if (Shifter.Instance.Player != null && AmongUsClient.Instance.AmHost && Shifter.Instance.FutureShift != null) { // We need to send the RPC from the host here, to make sure that the order of shifting and erasing is correct (for that reason the futureShifted and futureErased are being synced)
                Shifter.ShifterShift(Shifter.Instance.FutureShift.PlayerId);
            }
            Shifter.Instance.FutureShift = null;

            // Eraser erase
            if (Eraser.Instance.Player != null && AmongUsClient.Instance.AmHost) {  // We need to send the RPC from the host here, to make sure that the order of shifting and erasing is correct (for that reason the futureShifted and futureErased are being synced)
                foreach (var target in Eraser.Instance.FutureErased) {
                    if (target != null && target.canBeErased()) {
                        CommonRpc.ErasePlayerRoles(target.PlayerId);
                    }
                }
            }
            Eraser.Instance.FutureErased.Clear();

            // Trickster boxes
            if (Trickster.Instance.Player != null && JackInTheBox.hasJackInTheBoxLimitReached()) {
                JackInTheBox.convertToVents();
            }

            // Activate portals.
            Portal.meetingEndsUpdate();

            // Witch execute casted spells
            if (Witch.Instance.Player != null && Witch.Instance.FutureSpelled != null && AmongUsClient.Instance.AmHost) {
                bool exiledIsWitch = exiled != null && exiled.PlayerId == Witch.Instance.Player.PlayerId;
                bool witchDiesWithExiledLover = exiled != null && Lovers.Instance.Existing && Lovers.Instance.BothDie && (Lovers.Instance.Lover1.PlayerId == Witch.Instance.Player.PlayerId || Lovers.Instance.Lover2.PlayerId == Witch.Instance.Player.PlayerId) && (exiled.PlayerId == Lovers.Instance.Lover1.PlayerId || exiled.PlayerId == Lovers.Instance.Lover2.PlayerId);

                if ((witchDiesWithExiledLover || exiledIsWitch) && Witch.Instance.WitchVoteSaveTargets) Witch.Instance.FutureSpelled.Clear();
                foreach (PlayerControl target in Witch.Instance.FutureSpelled) {
                    if (target != null && !target.Data.IsDead && Helpers.checkMurderAttempt(Witch.Instance.Player, target, true) == MurderAttemptResult.PerformKill)
                    {
                        if (exiled != null && Lawyer.Instance.Player != null && (target == Lawyer.Instance.Player || target == Lovers.Instance.OtherLover(Lawyer.Instance.Player)) && Lawyer.Instance.Target != null && Lawyer.Instance.IsProsecutor && Lawyer.Instance.Target.PlayerId == exiled.PlayerId)
                            continue;
                        KernelRpc.UncheckedExilePlayer(target.PlayerId);
                        if (target == Lawyer.Instance.Target && Lawyer.Instance.Player != null) {
                            Lawyer.LawyerPromotesToPursuer();
                        }
                    }
                }
            }
            Witch.Instance.FutureSpelled.Clear();

            // SecurityGuard vents and cameras
            var allCameras = MapUtilities.CachedShipStatus.AllCameras.ToList();
            TORMapOptions.camerasToAdd.ForEach(camera => {
                camera.gameObject.SetActive(true);
                camera.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
                allCameras.Add(camera);
            });
            MapUtilities.CachedShipStatus.AllCameras = allCameras.ToArray();
            TORMapOptions.camerasToAdd = new List<SurvCamera>();

            foreach (Vent vent in TORMapOptions.ventsToSeal) {
                PowerTools.SpriteAnim animator = vent.GetComponent<PowerTools.SpriteAnim>(); 
                animator?.Stop();
                vent.EnterVentAnim = vent.ExitVentAnim = null;
                vent.myRend.sprite = animator == null ? SecurityGuard.StaticVentSealedSprite : SecurityGuard.AnimatedVentSealedSprite;
                if (SubmergedCompatibility.IsSubmerged && vent.Id == 0) vent.myRend.sprite = SecurityGuard.SubmergedCentralUpperSealedSprite;
                if (SubmergedCompatibility.IsSubmerged && vent.Id == 14) vent.myRend.sprite = SecurityGuard.SubmergedCentralLowerSealedSprite;
                vent.myRend.color = Color.white;
                vent.name = "SealedVent_" + vent.name;
            }
            TORMapOptions.ventsToSeal = new List<Vent>();

            EventUtility.meetingEndsUpdate();
        }
    }

    [HarmonyPatch]
    class ExileControllerWrapUpPatch {

        [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
        class BaseExileControllerPatch {
            public static void Postfix(ExileController __instance) {
                WrapUpPostfix(__instance.exiled);
            }
        }

        [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
        class AirshipExileControllerPatch {
            public static void Postfix(AirshipExileController __instance) {
                WrapUpPostfix(__instance.exiled);
            }
        }

        // Workaround to add a "postfix" to the destroying of the exile controller (i.e. cutscene) and SpwanInMinigame of submerged
        [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Destroy), new Type[] { typeof(GameObject) })]
        public static void Prefix(GameObject obj) {
            if (!SubmergedCompatibility.IsSubmerged) return;
            if (obj.name.Contains("ExileCutscene")) {
                WrapUpPostfix(ExileControllerBeginPatch.lastExiled);
            } else if (obj.name.Contains("SpawnInMinigame")) {
                AntiTeleport.Instance.SetPosition();
                Chameleon.Instance.LastMoved.Clear();
            }
        }

        static void WrapUpPostfix(GameData.PlayerInfo exiled) {
            // Prosecutor win condition
            if (exiled != null && Lawyer.Instance.Player != null && Lawyer.Instance.Target != null && Lawyer.Instance.IsProsecutor && Lawyer.Instance.Target.PlayerId == exiled.PlayerId && !Lawyer.Instance.Player.Data.IsDead)
                Lawyer.Instance.TriggerProsecutorWin = true;

            // Mini exile lose condition
            else if (exiled != null && Mini.Instance.Player != null && Mini.Instance.Player.PlayerId == exiled.PlayerId && !Mini.Instance.IsGrownUp && !Mini.Instance.Player.Data.Role.IsImpostor && !RoleInfo.getRoleInfoForPlayer(Mini.Instance.Player).Any(x => x.isNeutral)) {
                Mini.Instance.TriggerMiniLose = true;
            }
            // Jester win condition
            else if (exiled != null && Jester.Instance.HasPlayer && Jester.Instance.Player.PlayerId == exiled.PlayerId) {
                Jester.Instance.TriggerJesterWin = true;
            }
            // Prosecutor win condition
            else if (exiled != null && Lawyer.Instance.Player != null && Lawyer.Instance.Target != null && Lawyer.Instance.IsProsecutor && Lawyer.Instance.Target.PlayerId == exiled.PlayerId && !Lawyer.Instance.Player.Data.IsDead)
                Lawyer.Instance.TriggerProsecutorWin = true;


            // Reset custom button timers where necessary
            CustomButton.MeetingEndedUpdate();

            // Mini set adapted cooldown
            if (Mini.Instance.Player != null && CachedPlayer.LocalPlayer.PlayerControl == Mini.Instance.Player && Mini.Instance.Player.Data.Role.IsImpostor) {
                var multiplier = Mini.Instance.IsGrownUp ? 0.66f : 2f;
                Mini.Instance.Player.SetKillTimer(GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown * multiplier);
            }

            // Seer spawn souls
            if (Seer.Instance.DeadBodyPositions != null && Seer.Instance.Player != null && CachedPlayer.LocalPlayer.PlayerControl == Seer.Instance.Player && Seer.Instance.ShowDeathSouls) {
                foreach (Vector3 pos in Seer.Instance.DeadBodyPositions) {
                    GameObject soul = new GameObject();
                    //soul.transform.position = pos;
                    soul.transform.position = new Vector3(pos.x, pos.y, pos.y / 1000 - 1f);
                    soul.layer = 5;
                    var rend = soul.AddComponent<SpriteRenderer>();
                    soul.AddSubmergedComponent(SubmergedCompatibility.Classes.ElevatorMover);
                    rend.sprite = Seer.SoulSprite;
                    
                    if(Seer.Instance.LimitSoulsDuration) {
                        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Seer.Instance.SoulsDuration, new Action<float>((p) => {
                            if (rend != null) {
                                var tmp = rend.color;
                                tmp.a = Mathf.Clamp01(1 - p);
                                rend.color = tmp;
                            }    
                            if (p == 1f && rend != null && rend.gameObject != null) UnityEngine.Object.Destroy(rend.gameObject);
                        })));
                    }
                }
                Seer.Instance.DeadBodyPositions.Clear();
            }

            // Tracker reset deadBodyPositions
            Tracker.Instance.DeadBodyPositions.Clear();

            // Arsonist deactivate dead poolable players
            if (Arsonist.Instance.Player != null && Arsonist.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl) {
                int visibleCounter = 0;
                Vector3 newBottomLeft = IntroCutsceneOnDestroyPatch.bottomLeft;
                var BottomLeft = newBottomLeft + new Vector3(-0.25f, -0.25f, 0);
                foreach (PlayerControl p in CachedPlayer.AllPlayers) {
                    if (!TORMapOptions.playerIcons.ContainsKey(p.PlayerId)) continue;
                    if (p.Data.IsDead || p.Data.Disconnected) {
                        TORMapOptions.playerIcons[p.PlayerId].gameObject.SetActive(false);
                    } else {
                        TORMapOptions.playerIcons[p.PlayerId].transform.localPosition = newBottomLeft + Vector3.right * visibleCounter * 0.35f;
                        visibleCounter++;
                    }
                }
            }

            // Deputy check Promotion, see if the sheriff still exists. The promotion will be after the meeting.
            if (Deputy.Instance.Player != null)
            {
                PlayerControlFixedUpdatePatch.deputyCheckPromotion(isMeeting: true);
            }

            // Force Bounty Hunter Bounty Update
            if (BountyHunter.Instance.Player != null && BountyHunter.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl)
                BountyHunter.Instance.BountyUpdateTimer = 0f;

            // Medium spawn souls
            if (Medium.Instance.IsLocalPlayer) {
                if (Medium.Instance.Souls != null) {
                    foreach (SpriteRenderer sr in Medium.Instance.Souls) UnityEngine.Object.Destroy(sr.gameObject);
                    Medium.Instance.Souls.Clear();
                }

                if (Medium.Instance.FeatureDeadBodies != null) {
                    foreach (var (db, ps) in Medium.Instance.FeatureDeadBodies) {
                        var s = new GameObject();
                        //s.transform.position = ps;
                        s.transform.position = new Vector3(ps.x, ps.y, ps.y / 1000 - 1f);
                        s.layer = 5;
                        var rend = s.AddComponent<SpriteRenderer>();
                        s.AddSubmergedComponent(SubmergedCompatibility.Classes.ElevatorMover);
                        rend.sprite = Medium.SoulSprite;
                        Medium.Instance.Souls.Add(rend);
                    }
                    Medium.Instance.DeadBodies.AddRange(Medium.Instance.FeatureDeadBodies);
                    Medium.Instance.FeatureDeadBodies.Clear();
                }
            }

            // AntiTeleport set position
            AntiTeleport.Instance.SetPosition();

            // Invert add meeting
            if (Invert.Instance.Meetings > 0) Invert.Instance.Meetings--;

            Chameleon.Instance.LastMoved.Clear();

            foreach (Trap trap in Trap.traps) trap.triggerable = false;
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown / 2 + 2, new Action<float>((p) => {
            if (p == 1f) foreach (Trap trap in Trap.traps) trap.triggerable = true;
            })));
        }
    }

    [HarmonyPatch(typeof(SpawnInMinigame), nameof(SpawnInMinigame.Close))]  // Set position of AntiTp players AFTER they have selected a spawn.
    class AirshipSpawnInPatch {
        static void Postfix() {
            AntiTeleport.Instance.SetPosition();
            Chameleon.Instance.LastMoved.Clear();
        }
    }

    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new Type[] { typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
    class ExileControllerMessagePatch {
        static void Postfix(ref string __result, [HarmonyArgument(0)]StringNames id) {
            try {
                if (ExileController.Instance != null && ExileController.Instance.exiled != null) {
                    PlayerControl player = Helpers.playerById(ExileController.Instance.exiled.Object.PlayerId);
                    if (player == null) return;
                    // Exile role text
                    if (id == StringNames.ExileTextPN || id == StringNames.ExileTextSN || id == StringNames.ExileTextPP || id == StringNames.ExileTextSP) {
                        __result = player.Data.PlayerName + " was The " + String.Join(" ", RoleInfo.getRoleInfoForPlayer(player, false).Select(x => x.name).ToArray());
                    }
                    // Hide number of remaining impostors on Jester win
                    if (id == StringNames.ImpostorsRemainP || id == StringNames.ImpostorsRemainS) {
                        if (Jester.Instance.HasPlayer && player.PlayerId == Jester.Instance.Player.PlayerId) __result = "";
                    }
                    if (Tiebreaker.Instance.IsTiebreak) __result += " (Tiebreaker)";
                    Tiebreaker.Instance.IsTiebreak = false;
                }
            } catch {
                // pass - Hopefully prevent leaving while exiling to softlock game
            }
        }
    }
}
