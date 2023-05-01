using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using TheOtherRoles.Objects;
using System;
using TheOtherRoles.Customs.Modifiers;
using TheOtherRoles.Customs.Roles.Crewmate;
using TheOtherRoles.Customs.Roles.Impostor;
using TheOtherRoles.Customs.Roles.Neutral;
using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;
using UnityEngine;

namespace TheOtherRoles.Patches
{
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
    [HarmonyPriority(Priority.First)]
    class ExileControllerBeginPatch
    {
        public static GameData.PlayerInfo lastExiled;

        public static void Prefix(ExileController __instance, [HarmonyArgument(0)] ref GameData.PlayerInfo exiled,
            [HarmonyArgument(1)] bool tie)
        {
            lastExiled = exiled;

            // Medic shield
            if (Singleton<Medic>.Instance.Player != null && AmongUsClient.Instance.AmHost &&
                Singleton<Medic>.Instance.FutureShieldedPlayer != null && !Singleton<Medic>.Instance.Player.Data.IsDead)
            {
                // We need to send the RPC from the host here, to make sure that the order of shifting and setting the shield is correct(for that reason the futureShifted and futureShielded are being synced)
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                    CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.MedicSetShielded,
                    Hazel.SendOption.Reliable, -1);
                writer.Write(Singleton<Medic>.Instance.FutureShieldedPlayer.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.medicSetShielded(Singleton<Medic>.Instance.FutureShieldedPlayer.PlayerId);
            }

            if (Singleton<Medic>.Instance.UsedShield)
                Singleton<Medic>.Instance.MeetingAfterShielding = true; // Has to be after the setting of the shield

            // Shifter shift
            if (Shifter.shifter != null && AmongUsClient.Instance.AmHost && Shifter.futureShift != null)
            {
                // We need to send the RPC from the host here, to make sure that the order of shifting and erasing is correct (for that reason the futureShifted and futureErased are being synced)
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                    CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.ShifterShift,
                    Hazel.SendOption.Reliable, -1);
                writer.Write(Shifter.futureShift.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.shifterShift(Shifter.futureShift.PlayerId);
            }

            Shifter.futureShift = null;

            // Eraser erase
            if (Singleton<Eraser>.Instance.Player != null && AmongUsClient.Instance.AmHost &&
                Singleton<Eraser>.Instance.FutureErased != null)
            {
                // We need to send the RPC from the host here, to make sure that the order of shifting and erasing is correct (for that reason the futureShifted and futureErased are being synced)
                foreach (PlayerControl target in Singleton<Eraser>.Instance.FutureErased)
                {
                    if (target != null && target.canBeErased())
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                            CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.ErasePlayerRoles,
                            Hazel.SendOption.Reliable, -1);
                        writer.Write(target.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.erasePlayerRoles(target.PlayerId);
                    }
                }
            }

            Singleton<Eraser>.Instance.FutureErased = new List<PlayerControl>();

            // Trickster boxes
            if (Singleton<Trickster>.Instance.Player != null && JackInTheBox.hasJackInTheBoxLimitReached())
            {
                JackInTheBox.convertToVents();
            }

            // Activate portals.
            Portal.meetingEndsUpdate();

            // Witch execute casted spells
            if (Singleton<Witch>.Instance.Player != null && AmongUsClient.Instance.AmHost)
            {
                bool exiledIsWitch = exiled != null && exiled.PlayerId == Singleton<Witch>.Instance.Player.PlayerId;
                bool witchDiesWithExiledLover = exiled != null && Lovers.existing() && Lovers.bothDie &&
                                                (Lovers.lover1.PlayerId == Singleton<Witch>.Instance.Player.PlayerId ||
                                                 Lovers.lover2.PlayerId == Singleton<Witch>.Instance.Player.PlayerId) &&
                                                (exiled.PlayerId == Lovers.lover1.PlayerId ||
                                                 exiled.PlayerId == Lovers.lover2.PlayerId);

                if ((witchDiesWithExiledLover || exiledIsWitch) && Singleton<Witch>.Instance.WitchVoteSaveTargets)
                    Singleton<Witch>.Instance.FutureSpelled.Clear();
                foreach (PlayerControl target in Singleton<Witch>.Instance.FutureSpelled)
                {
                    if (target != null && !target.Data.IsDead &&
                        Helpers.checkMurderAttempt(Singleton<Witch>.Instance.Player, target, true) ==
                        MurderAttemptResult.PerformKill)
                    {
                        if (exiled != null && Singleton<Prosecutor>.Instance.Player != null &&
                            (target == Singleton<Prosecutor>.Instance.Player ||
                             target == Lovers.otherLover(Singleton<Prosecutor>.Instance.Player)) &&
                            Singleton<Prosecutor>.Instance.Target != null &&
                            Singleton<Prosecutor>.Instance.Target.PlayerId == exiled.PlayerId)
                            continue;
                        var writer = AmongUsClient.Instance.StartRpcImmediately(
                            CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.UncheckedExilePlayer,
                            Hazel.SendOption.Reliable, -1);
                        writer.Write(target.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.uncheckedExilePlayer(target.PlayerId);
                        if (target != Singleton<Lawyer>.Instance.Target || Singleton<Lawyer>.Instance.Player == null)
                            continue;
                        var writer2 = AmongUsClient.Instance.StartRpcImmediately(
                            CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.LawyerPromotesToPursuer,
                            Hazel.SendOption.Reliable, -1);
                        AmongUsClient.Instance.FinishRpcImmediately(writer2);
                        RPCProcedure.lawyerPromotesToPursuer();
                    }
                }
            }

            Singleton<Witch>.Instance.FutureSpelled.Clear();

            // SecurityGuard vents and cameras
            var allCameras = MapUtilities.CachedShipStatus.AllCameras.ToList();
            TORMapOptions.camerasToAdd.ForEach(camera =>
            {
                camera.gameObject.SetActive(true);
                camera.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
                allCameras.Add(camera);
            });
            MapUtilities.CachedShipStatus.AllCameras = allCameras.ToArray();
            TORMapOptions.camerasToAdd = new List<SurvCamera>();

            foreach (Vent vent in TORMapOptions.ventsToSeal)
            {
                PowerTools.SpriteAnim animator = vent.GetComponent<PowerTools.SpriteAnim>();
                animator?.Stop();
                vent.EnterVentAnim = vent.ExitVentAnim = null;
                vent.myRend.sprite = animator == null
                    ? Singleton<SecurityGuard>.Instance.GetStaticVentSealedSprite()
                    : Singleton<SecurityGuard>.Instance.GetAnimatedVentSealedSprite();
                if (SubmergedCompatibility.IsSubmerged && vent.Id == 0)
                    vent.myRend.sprite = Singleton<SecurityGuard>.Instance.GetSubmergedCentralUpperSealedSprite();
                if (SubmergedCompatibility.IsSubmerged && vent.Id == 14)
                    vent.myRend.sprite = Singleton<SecurityGuard>.Instance.GetSubmergedCentralLowerSealedSprite();
                vent.myRend.color = Color.white;
                vent.name = "SealedVent_" + vent.name;
            }

            TORMapOptions.ventsToSeal = new List<Vent>();

            EventUtility.meetingEndsUpdate();
        }
    }

    [HarmonyPatch]
    class ExileControllerWrapUpPatch
    {
        [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
        class BaseExileControllerPatch
        {
            public static void Postfix(ExileController __instance)
            {
                WrapUpPostfix(__instance.exiled);
            }
        }

        [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
        class AirshipExileControllerPatch
        {
            public static void Postfix(AirshipExileController __instance)
            {
                WrapUpPostfix(__instance.exiled);
            }
        }

        // Workaround to add a "postfix" to the destroying of the exile controller (i.e. cutscene) and SpwanInMinigame of submerged
        [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Destroy),
            new Type[] { typeof(GameObject) })]
        public static void Prefix(GameObject obj)
        {
            if (!SubmergedCompatibility.IsSubmerged) return;
            if (obj.name.Contains("ExileCutscene"))
            {
                WrapUpPostfix(ExileControllerBeginPatch.lastExiled);
            }
            else if (obj.name.Contains("SpawnInMinigame"))
            {
                AntiTeleport.setPosition();
                Chameleon.lastMoved.Clear();
            }
        }

        static void WrapUpPostfix(GameData.PlayerInfo exiled)
        {
            // Prosecutor win condition
            if (exiled != null && Singleton<Prosecutor>.Instance.Player != null &&
                Singleton<Prosecutor>.Instance.Target != null &&
                Singleton<Prosecutor>.Instance.Target.PlayerId == exiled.PlayerId &&
                !Singleton<Prosecutor>.Instance.Player.Data.IsDead)
            {
                Singleton<Prosecutor>.Instance.TriggerWin = true;
            }

            // Mini exile lose condition
            else if (exiled != null && Mini.mini != null && Mini.mini.PlayerId == exiled.PlayerId &&
                     !Mini.isGrownUp() && !Mini.mini.Data.Role.IsImpostor &&
                     !RoleInfo.getRoleInfoForPlayer(Mini.mini).Any(x => x.isNeutral))
            {
                Mini.triggerMiniLose = true;
            }
            // Jester win condition
            else if (exiled != null && Singleton<Jester>.Instance.Player != null &&
                     Singleton<Jester>.Instance.Player.PlayerId == exiled.PlayerId)
            {
                Singleton<Jester>.Instance.TriggerWin = true;
            }


            // Reset custom button timers where necessary
            CustomButton.MeetingEndedUpdate();

            // Mini set adapted cooldown
            if (Mini.mini != null && CachedPlayer.LocalPlayer.PlayerControl == Mini.mini &&
                Mini.mini.Data.Role.IsImpostor)
            {
                var multiplier = Mini.isGrownUp() ? 0.66f : 2f;
                Mini.mini.SetKillTimer(GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown * multiplier);
            }

            // Seer spawn souls
            if (Singleton<Seer>.Instance.IsLocalPlayer())
            {
                foreach (var pos in Singleton<Seer>.Instance.DeadBodyPositions)
                {
                    GameObject soul = new GameObject();
                    //soul.transform.position = pos;
                    soul.transform.position = new Vector3(pos.x, pos.y, pos.y / 1000 - 1f);
                    soul.layer = 5;
                    var rend = soul.AddComponent<SpriteRenderer>();
                    soul.AddSubmergedComponent(SubmergedCompatibility.Classes.ElevatorMover);
                    rend.sprite = Singleton<Seer>.Instance.GetSoulSprite();

                    if (Singleton<Seer>.Instance.LimitSoulsDuration)
                    {
                        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(
                            Singleton<Seer>.Instance.SoulsDuration,
                            new Action<float>((p) =>
                            {
                                if (rend != null)
                                {
                                    var tmp = rend.color;
                                    tmp.a = Mathf.Clamp01(1 - p);
                                    rend.color = tmp;
                                }

                                if (p == 1f && rend != null && rend.gameObject != null)
                                    UnityEngine.Object.Destroy(rend.gameObject);
                            })));
                    }
                }

                Singleton<Seer>.Instance.DeadBodyPositions.Clear();
            }

            // Tracker reset deadBodyPositions
            Singleton<Tracker>.Instance.DeadBodyPositions.Clear();

            // Arsonist deactivate dead poolable players
            if (Singleton<Arsonist>.Instance.IsLocalPlayer())
            {
                int visibleCounter = 0;
                Vector3 newBottomLeft = IntroCutsceneOnDestroyPatch.bottomLeft;
                var BottomLeft = newBottomLeft + new Vector3(-0.25f, -0.25f, 0);
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (!TORMapOptions.playerIcons.ContainsKey(p.PlayerId)) continue;
                    if (p.Data.IsDead || p.Data.Disconnected)
                    {
                        TORMapOptions.playerIcons[p.PlayerId].gameObject.SetActive(false);
                    }
                    else
                    {
                        TORMapOptions.playerIcons[p.PlayerId].transform.localPosition =
                            newBottomLeft + Vector3.right * visibleCounter * 0.35f;
                        visibleCounter++;
                    }
                }
            }

            // Deputy check Promotion, see if the sheriff still exists. The promotion will be after the meeting.
            if (Singleton<Deputy>.Instance.Player != null)
            {
                PlayerControlFixedUpdatePatch.deputyCheckPromotion(isMeeting: true);
            }

            // Force Bounty Hunter Bounty Update
            if (Singleton<BountyHunter>.Instance.IsLocalPlayer())
                Singleton<BountyHunter>.Instance.BountyUpdateTimer = 0f;

            // Medium spawn souls
            if (Singleton<Medium>.Instance.IsLocalPlayer())
            {
                foreach (var sr in Singleton<Medium>.Instance.Souls) UnityEngine.Object.Destroy(sr.gameObject);
                Singleton<Medium>.Instance.Souls.Clear();

                foreach ((DeadPlayer db, Vector3 ps) in Singleton<Medium>.Instance.FeatureDeadBodies)
                {
                    GameObject s = new GameObject();
                    //s.transform.position = ps;
                    s.transform.position = new Vector3(ps.x, ps.y, ps.y / 1000 - 1f);
                    s.layer = 5;
                    var rend = s.AddComponent<SpriteRenderer>();
                    s.AddSubmergedComponent(SubmergedCompatibility.Classes.ElevatorMover);
                    rend.sprite = Singleton<Medium>.Instance.GetSoulSprite();
                    Singleton<Medium>.Instance.Souls.Add(rend);
                }

                Singleton<Medium>.Instance.DeadBodies.Clear();
                Singleton<Medium>.Instance.DeadBodies.AddRange(Singleton<Medium>.Instance.FeatureDeadBodies);
                Singleton<Medium>.Instance.FeatureDeadBodies.Clear();
            }

            // AntiTeleport set position
            AntiTeleport.setPosition();

            // Invert add meeting
            if (Invert.meetings > 0) Invert.meetings--;

            Chameleon.lastMoved.Clear();

            foreach (var trap in Trap.traps) trap.triggerable = false;
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(
                GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown / 2 + 2, new System.Action<float>(
                    (p) =>
                    {
                        if (p == 1f)
                            foreach (var trap in Trap.traps)
                                trap.triggerable = true;
                    })));
        }
    }

    [HarmonyPatch(typeof(SpawnInMinigame),
        nameof(SpawnInMinigame.Close))] // Set position of AntiTp players AFTER they have selected a spawn.
    class AirshipSpawnInPatch
    {
        static void Postfix()
        {
            AntiTeleport.setPosition();
            Chameleon.lastMoved.Clear();
        }
    }

    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString),
        new Type[] { typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
    class ExileControllerMessagePatch
    {
        static void Postfix(ref string __result, [HarmonyArgument(0)] StringNames id)
        {
            try
            {
                if (ExileController.Instance != null && ExileController.Instance.exiled != null)
                {
                    PlayerControl player = Helpers.playerById(ExileController.Instance.exiled.Object.PlayerId);
                    if (player == null) return;
                    // Exile role text
                    if (id == StringNames.ExileTextPN || id == StringNames.ExileTextSN ||
                        id == StringNames.ExileTextPP || id == StringNames.ExileTextSP)
                    {
                        __result = player.Data.PlayerName + " was The " + String.Join(" ",
                            RoleInfo.getRoleInfoForPlayer(player, false).Select(x => x.name).ToArray());
                    }

                    // Hide number of remaining impostors on Jester win
                    if (id == StringNames.ImpostorsRemainP || id == StringNames.ImpostorsRemainS)
                    {
                        if (Singleton<Jester>.Instance.Player != null &&
                            player.PlayerId == Singleton<Jester>.Instance.Player.PlayerId) __result = "";
                    }

                    if (Tiebreaker.isTiebreak) __result += " (Tiebreaker)";
                    Tiebreaker.isTiebreak = false;
                }
            }
            catch
            {
                // pass - Hopefully prevent leaving while exiling to softlock game
            }
        }
    }
}