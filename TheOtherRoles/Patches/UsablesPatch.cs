using HarmonyLib;
using System;
using UnityEngine;
using PowerTools;
using System.Linq;
using static TheOtherRoles.GameHistory;
using static TheOtherRoles.TORMapOptions;
using System.Collections.Generic;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;
using TheOtherRoles.CustomGameModes;
using Reactor.Utilities.Extensions;
using AmongUs.GameOptions;
using TheOtherRoles.EnoFw;
using TheOtherRoles.EnoFw.Kernel;
using TheOtherRoles.EnoFw.Roles.Crewmate;
using TheOtherRoles.EnoFw.Roles.Impostor;
using TheOtherRoles.EnoFw.Roles.Modifiers;
using TheOtherRoles.EnoFw.Roles.Neutral;
using TheOtherRoles.Modules;

namespace TheOtherRoles.Patches {
    // HACK ¯\_(ツ)_/¯ but it works !
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
    public static class VentStartPatch
    {
        public class VentKeybind: MonoBehaviour
        {
            private void Update()
            {
                if (FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.enabled && Rewired.ReInput.players.GetPlayer(0).GetButtonDown("UseVent"))
                {
                    FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.DoClick();
                }
            }
        }

        static VentStartPatch()
        {
            ClassInjector.RegisterTypeInIl2Cpp<VentKeybind>();
        }
        
        public static void Postfix()
        {
            new GameObject().AddComponent<VentKeybind>();
        }
    }

    [HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
    public static class VentCanUsePatch
    {
        public static bool Prefix(Vent __instance, ref float __result, [HarmonyArgument(0)] GameData.PlayerInfo pc, [HarmonyArgument(1)] ref bool canUse, [HarmonyArgument(2)] ref bool couldUse) {
            if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return true;
            float num = float.MaxValue;
            PlayerControl playerObject = pc.Object;

            bool roleCouldUse = playerObject.roleCanUseVents();

            if (__instance.name.StartsWith("SealedVent_")) {
                canUse = couldUse = false;
                __result = num;
                return false;
            }

            // Submerged Compatability if needed:
            if (SubmergedCompatibility.IsSubmerged) {
                // as submerged does, only change stuff for vents 9 and 14 of submerged. Code partially provided by AlexejheroYTB
                if (SubmergedCompatibility.getInTransition()) {
                    __result = float.MaxValue;
                    return canUse = couldUse = false;
                }                
                switch (__instance.Id) {
                    case 9:  // Cannot enter vent 9 (Engine Room Exit Only Vent)!
                        if (CachedPlayer.LocalPlayer.PlayerControl.inVent) break;
                        __result = float.MaxValue;
                        return canUse = couldUse = false;                    
                    case 14: // Lower Central
                        __result = float.MaxValue;
                        couldUse = roleCouldUse && !pc.IsDead && (playerObject.CanMove || playerObject.inVent);
                        canUse = couldUse;
                        if (canUse) {
                            Vector3 center = playerObject.Collider.bounds.center;
                            Vector3 position = __instance.transform.position;
                            __result = Vector2.Distance(center, position);
                            canUse &= __result <= __instance.UsableDistance;
                        }
                        return false;
                }
            }

            var usableDistance = __instance.UsableDistance;
            if (__instance.name.StartsWith("JackInTheBoxVent_")) {
                if(Trickster.Instance.Player != CachedPlayer.LocalPlayer.PlayerControl) {
                    // Only the Trickster can use the Jack-In-The-Boxes!
                    canUse = false;
                    couldUse = false;
                    __result = num;
                    return false; 
                } else {
                    // Reduce the usable distance to reduce the risk of gettings stuck while trying to jump into the box if it's placed near objects
                    usableDistance = 0.4f; 
                }
            }

            couldUse = (playerObject.inVent || roleCouldUse) && !pc.IsDead && (playerObject.CanMove || playerObject.inVent);
            canUse = couldUse;
            if (canUse)
            {
                Vector3 center = playerObject.Collider.bounds.center;
                Vector3 position = __instance.transform.position;
                num = Vector2.Distance(center, position);
                canUse &= (num <= usableDistance && (!PhysicsHelpers.AnythingBetween(playerObject.Collider, center, position, Constants.ShipOnlyMask, false) || __instance.name.StartsWith("JackInTheBoxVent_")));
            }
            __result = num;
            return false;
        }
    }

    [HarmonyPatch(typeof(VentButton), nameof(VentButton.DoClick))]
    class VentButtonDoClickPatch {
        static  bool Prefix(VentButton __instance) {
            // Manually modifying the VentButton to use Vent.Use again in order to trigger the Vent.Use prefix patch
		    if (__instance.currentTarget != null && !Deputy.Instance.HandcuffedKnows.ContainsKey(CachedPlayer.LocalPlayer.PlayerId)) __instance.currentTarget.Use();
            return false;
        }
    }

    [HarmonyPatch(typeof(Vent), nameof(Vent.Use))]
    public static class VentUsePatch {
        public static bool Prefix(Vent __instance) {
            if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return true;
            // Deputy handcuff disables the vents
            if (Deputy.Instance.HandcuffedPlayers.Contains(CachedPlayer.LocalPlayer.PlayerId)) {
                Deputy.Instance.SetHandcuffedKnows();
                return false;
            }
            if (Trapper.Instance.PlayersOnMap.Contains(CachedPlayer.LocalPlayer.PlayerControl)) return false;

            bool canUse;
            bool couldUse;
            __instance.CanUse(CachedPlayer.LocalPlayer.Data, out canUse, out couldUse);
            bool canMoveInVents = CachedPlayer.LocalPlayer.PlayerControl != Spy.Instance.Player && !Trapper.Instance.PlayersOnMap.Contains(CachedPlayer.LocalPlayer.PlayerControl);
            if (!canUse) return false; // No need to execute the native method as using is disallowed anyways

            bool isEnter = !CachedPlayer.LocalPlayer.PlayerControl.inVent;
            
            if (__instance.name.StartsWith("JackInTheBoxVent_")) {
                __instance.SetButtons(isEnter && canMoveInVents);
                KernelRpc.UseUncheckedVent(__instance.Id, CachedPlayer.LocalPlayer.PlayerId, isEnter);
                SoundEffectsManager.play("tricksterUseBoxVent");
                return false;
            }

            if(isEnter) {
                CachedPlayer.LocalPlayer.PlayerPhysics.RpcEnterVent(__instance.Id);
            } else {
                CachedPlayer.LocalPlayer.PlayerPhysics.RpcExitVent(__instance.Id);
            }
            __instance.SetButtons(isEnter && canMoveInVents);
            return false;
        }
    }

    internal class VisibleVentPatches
    {
        public static int ShipAndObjectsMask = LayerMask.GetMask(new string[]
        {
            "Ship",
            "Objects"
        });
        
        [HarmonyPatch(typeof(Vent), nameof(Vent.EnterVent))] //EnterVent
        public static class EnterVentPatch
        {
            public static bool Prefix(Vent __instance, PlayerControl pc)
            {
                if (!__instance.EnterVentAnim)
                {
                    return false;
                }
                
                var truePosition = PlayerControl.LocalPlayer.GetTruePosition();
                
                Vector2 vector = pc.GetTruePosition() - truePosition;
                var magnitude = vector.magnitude;
                if (pc.AmOwner || magnitude < CachedPlayer.LocalPlayer.PlayerControl.lightSource.viewDistance &&
                    !PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude,
                        ShipAndObjectsMask))
                {
                    __instance.GetComponent<SpriteAnim>().Play(__instance.EnterVentAnim, 1f);
                }
                
                if (pc.AmOwner && Constants.ShouldPlaySfx()) //ShouldPlaySfx
                {
                    SoundManager.Instance.StopSound(ShipStatus.Instance.VentEnterSound);
                    SoundManager.Instance.PlaySound(ShipStatus.Instance.VentEnterSound, false, 1f).pitch =
                        UnityEngine.Random.Range(0.8f, 1.2f);
                }
                
                return false;
            }
        }
    
        [HarmonyPatch(typeof(Vent), nameof(Vent.ExitVent))] //ExitVent
        public static class ExitVentPatch
        {
            public static bool Prefix(Vent __instance, PlayerControl pc)
            {
                if (!__instance.ExitVentAnim)
                {
                    return false;
                }
        
                var truePosition = PlayerControl.LocalPlayer.GetTruePosition();
        
                Vector2 vector = pc.GetTruePosition() - truePosition;
                var magnitude = vector.magnitude;
                if (pc.AmOwner || magnitude < CachedPlayer.LocalPlayer.PlayerControl.lightSource.viewDistance &&
                    !PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude,
                        ShipAndObjectsMask))
                {
                    __instance.GetComponent<SpriteAnim>().Play(__instance.ExitVentAnim, 1f);
                }
        
                if (pc.AmOwner && Constants.ShouldPlaySfx()) //ShouldPlaySfx
                {
                    SoundManager.Instance.StopSound(ShipStatus.Instance.VentEnterSound);
                    SoundManager.Instance.PlaySound(ShipStatus.Instance.VentEnterSound, false, 1f).pitch =
                        UnityEngine.Random.Range(0.8f, 1.2f);
                }
        
                return false;
            }
        }
    }

    [HarmonyPatch(typeof(Vent), nameof(Vent.TryMoveToVent))]
    public static class TryMoveToVentPatch {
        public static bool Prefix(Vent otherVent) {
            return !Trapper.Instance.PlayersOnMap.Contains(CachedPlayer.LocalPlayer.PlayerControl);
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    class VentButtonVisibilityPatch {
        static void Postfix(PlayerControl __instance) {
            if (__instance.AmOwner && __instance.roleCanUseVents() && FastDestroyableSingleton<HudManager>.Instance.ReportButton.isActiveAndEnabled) {
                FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.Show();
            }
        }
    }

    [HarmonyPatch(typeof(VentButton), nameof(VentButton.SetTarget))]
    class VentButtonSetTargetPatch {
        static Sprite defaultVentSprite = null;

        static bool Prefix(VentButton __instance)
        {
            if (Undertaker.Instance.Player != null && Undertaker.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl && Undertaker.Instance.DraggedBody != null && Undertaker.Instance.DisableVentWhileDragging) return false;

            return true;
        }

        static void Postfix(VentButton __instance) {

            // Trickster render special vent button
            if (Trickster.Instance.Player != null && Trickster.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl) {
                if (defaultVentSprite == null) defaultVentSprite = __instance.graphic.sprite;
                bool isSpecialVent = __instance.currentTarget != null && __instance.currentTarget.gameObject != null && __instance.currentTarget.gameObject.name.StartsWith("JackInTheBoxVent_");
                __instance.graphic.sprite = isSpecialVent ?  Trickster.TricksterVentButtonSprite : defaultVentSprite;
                __instance.buttonLabelText.enabled = !isSpecialVent;
            }
        }
    }

    [HarmonyPatch(typeof(KillButton), nameof(KillButton.SetTarget))]
    class KillButtonSetTargetPatch
    {
        static bool Prefix(KillButton __instance)
        {
            if (Undertaker.Instance.Player != null && Undertaker.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl && Undertaker.Instance.DraggedBody != null) return false;

            return true;
        }
    }

    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    class KillButtonDoClickPatch {
        public static bool Prefix(KillButton __instance) {
            if (__instance.isActiveAndEnabled && __instance.currentTarget && !__instance.isCoolingDown && !CachedPlayer.LocalPlayer.Data.IsDead && CachedPlayer.LocalPlayer.PlayerControl.CanMove) {
                // Deputy handcuff update.
                if (Deputy.Instance.HandcuffedPlayers.Contains(CachedPlayer.LocalPlayer.PlayerId)) {
                    Deputy.Instance.SetHandcuffedKnows();
                    return false;
                }
                
                // Use an unchecked kill command, to allow shorter kill cooldowns etc. without getting kicked
                MurderAttemptResult res = Helpers.checkMurderAttemptAndKill(CachedPlayer.LocalPlayer.PlayerControl, __instance.currentTarget);
                // Handle blank kill
                if (res == MurderAttemptResult.BlankKill) {
                    CachedPlayer.LocalPlayer.PlayerControl.killTimer = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
                    if (CachedPlayer.LocalPlayer.PlayerControl == Cleaner.Instance.Player)
                        Cleaner.Instance.Player.killTimer = HudManagerStartPatch.cleanerCleanButton.Timer = HudManagerStartPatch.cleanerCleanButton.MaxTimer;
                    else if (CachedPlayer.LocalPlayer.PlayerControl == Warlock.Instance.Player)
                        Warlock.Instance.Player.killTimer = HudManagerStartPatch.warlockCurseButton.Timer = HudManagerStartPatch.warlockCurseButton.MaxTimer;
                    else if (CachedPlayer.LocalPlayer.PlayerControl == Mini.Instance.Player && Mini.Instance.Player.Data.Role.IsImpostor)
                        Mini.Instance.Player.SetKillTimer(GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown * (Mini.Instance.IsGrownUp ? 0.66f : 2f));
                    else if (CachedPlayer.LocalPlayer.PlayerControl == Witch.Instance.Player)
                        Witch.Instance.Player.killTimer = HudManagerStartPatch.witchSpellButton.Timer = HudManagerStartPatch.witchSpellButton.MaxTimer;
                    else if (CachedPlayer.LocalPlayer.PlayerControl == Ninja.Instance.Player)
                        Ninja.Instance.Player.killTimer = HudManagerStartPatch.ninjaButton.Timer = HudManagerStartPatch.ninjaButton.MaxTimer;
                }
                __instance.SetTarget(null);
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(SabotageButton), nameof(SabotageButton.Refresh))]
    class SabotageButtonRefreshPatch {
        static void Postfix() {
            // Mafia disable sabotage button for Janitor and sometimes for Mafioso
            bool blockSabotageJanitor = (Janitor.Instance.Player != null && Janitor.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl);
            bool blockSabotageMafioso = (Mafioso.Instance.Player != null && Mafioso.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl && Godfather.Instance.Player != null && !Godfather.Instance.Player.Data.IsDead);
            if (blockSabotageJanitor || blockSabotageMafioso) {
                FastDestroyableSingleton<HudManager>.Instance.SabotageButton.SetDisabled();
            }
        }
    }

    [HarmonyPatch(typeof(ReportButton), nameof(ReportButton.DoClick))]
    class ReportButtonDoClickPatch {
        public static bool Prefix(ReportButton __instance) {
            if (__instance.isActiveAndEnabled && Deputy.Instance.HandcuffedPlayers.Contains(CachedPlayer.LocalPlayer.PlayerId) && __instance.graphic.color == Palette.EnabledColor) Deputy.Instance.SetHandcuffedKnows();
            if (Undertaker.Instance.Player != null && Undertaker.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl && Undertaker.Instance.DraggedBody != null && Undertaker.Instance.DisableReportWhileDragging) return false;
            return !Deputy.Instance.HandcuffedKnows.ContainsKey(CachedPlayer.LocalPlayer.PlayerId);
        }
    }

    [HarmonyPatch(typeof(DeadBody), nameof(DeadBody.OnClick))]
    class DeadBodyOnClickPatch {
        public static bool Prefix(DeadBody __instance) {
            // Deputy handcuff disables the vents
            if (Deputy.Instance.HandcuffedPlayers.Contains(CachedPlayer.LocalPlayer.PlayerId)) {
                Deputy.Instance.SetHandcuffedKnows();
                return false;
            }

            if (Undertaker.Instance.Player != null && Undertaker.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl && Undertaker.Instance.DraggedBody != null)
            {
                PlayerControl undertakerPlayer = Undertaker.Instance.Player;

                if (Vector2.Distance(undertakerPlayer.GetTruePosition() - new Vector2(-0.2f, -0.22f),  __instance.TruePosition) <= Undertaker.Instance.RealDragDistance + 0.1f)
                {
                    Undertaker.DragBody(__instance.ParentId);
                    
                    return false;
                }

            }
            
            return true;
        } 
    }

    [HarmonyPatch(typeof(EmergencyMinigame), nameof(EmergencyMinigame.Update))]
    class EmergencyMinigameUpdatePatch {
        static void Postfix(EmergencyMinigame __instance) {
            var roleCanCallEmergency = true;
            var statusText = "";

            // Deactivate emergency button for Swapper
            if (Swapper.Instance.Player != null && Swapper.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl && !Swapper.Instance.CanCallEmergencyMeeting) {
                roleCanCallEmergency = false;
                statusText = "The Swapper can't start an emergency meeting";
            }
            // Potentially deactivate emergency button for Jester
            if (Jester.Instance.HasPlayer && Jester.Instance.IsLocalPlayer && !Jester.Instance.CanCallEmergency) {
                roleCanCallEmergency = false;
                statusText = "The Jester can't start an emergency meeting";
            }
            // Potentially deactivate emergency button for Lawyer/Prosecutor
            if (Lawyer.Instance.Player != null && Lawyer.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl && !Lawyer.Instance.CanCallEmergencyMeeting) {
                roleCanCallEmergency = false;
                statusText = "The Lawyer can't start an emergency meeting";
                if (Lawyer.Instance.IsProsecutor) statusText = "The Prosecutor can't start an emergency meeting";
            }

            if (!roleCanCallEmergency) {
                __instance.StatusText.text = statusText;
                __instance.NumberText.text = string.Empty;
                __instance.ClosedLid.gameObject.SetActive(true);
                __instance.OpenLid.gameObject.SetActive(false);
                __instance.ButtonActive = false;
                return;
            }

            // Handle max number of meetings
            if (__instance.state == 1) {
                int localRemaining = CachedPlayer.LocalPlayer.PlayerControl.RemainingEmergencies;
                int teamRemaining = Mathf.Max(0, maxNumberOfMeetings - meetingsCount);
                int remaining = Mathf.Min(localRemaining, Mayor.Instance.IsLocalPlayer ? 1 : teamRemaining);
                __instance.NumberText.text = $"{localRemaining.ToString()} and the ship has {teamRemaining.ToString()}";
                __instance.ButtonActive = remaining > 0;
                __instance.ClosedLid.gameObject.SetActive(!__instance.ButtonActive);
                __instance.OpenLid.gameObject.SetActive(__instance.ButtonActive);
				return;
			}
        }
    }


    [HarmonyPatch(typeof(Console), nameof(Console.CanUse))]
    public static class ConsoleCanUsePatch {
        public static bool Prefix(ref float __result, Console __instance, [HarmonyArgument(0)] GameData.PlayerInfo pc, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse) {
            canUse = couldUse = false;
            if (Swapper.Instance.Player != null && Swapper.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl)
                return !__instance.TaskTypes.Any(x => x == TaskTypes.FixLights || x == TaskTypes.FixComms);
            if (__instance.AllowImpostor) return true;
            if (!Helpers.hasFakeTasks(pc.Object)) return true;
            __result = float.MaxValue;
            return false;
        }
    }

    [HarmonyPatch(typeof(TuneRadioMinigame), nameof(TuneRadioMinigame.Begin))]
    class CommsMinigameBeginPatch {
        static void Postfix(TuneRadioMinigame __instance) {
            // Block Swapper from fixing comms. Still looking for a better way to do this, but deleting the task doesn't seem like a viable option since then the camera, admin table, ... work while comms are out
            if (Swapper.Instance.Player != null && Swapper.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl) {
                __instance.Close();
            }
        }
    }

    [HarmonyPatch(typeof(SwitchMinigame), nameof(SwitchMinigame.Begin))]
    class LightsMinigameBeginPatch {
        static void Postfix(SwitchMinigame __instance) {
            // Block Swapper from fixing lights. One could also just delete the PlayerTask, but I wanted to do it the same way as with coms for now.
            if (Swapper.Instance.Player != null && Swapper.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl) {
                __instance.Close();
            }
        }
    }

    [HarmonyPatch]
    class VitalsMinigamePatch {
        private static List<TMPro.TextMeshPro> hackerTexts = new List<TMPro.TextMeshPro>();

        [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Begin))]
        class VitalsMinigameStartPatch {
            static void Postfix(VitalsMinigame __instance) {
                if (CustomGuid.ShowRoleDesc && __instance.gameObject.name == "hudroleinfo")
                {
                    foreach (var vitalsPanel in __instance.vitals)
                    {
                        vitalsPanel.gameObject.SetActive(false);
                    }
                    return;
                }
                
                if (Hacker.Instance.IsLocalPlayer) {
                    hackerTexts = new List<TMPro.TextMeshPro>();
                    foreach (VitalsPanel panel in __instance.vitals) {
                        TMPro.TextMeshPro text = UnityEngine.Object.Instantiate(__instance.SabText, panel.transform);
                        hackerTexts.Add(text);
                        UnityEngine.Object.DestroyImmediate(text.GetComponent<AlphaBlink>());
                        text.gameObject.SetActive(false);
                        text.transform.localScale = Vector3.one * 0.75f;
                        text.transform.localPosition = new Vector3(-0.75f, -0.23f, 0f);
                    
                    }
                }

                //Fix Visor in Vitals
                foreach (VitalsPanel panel in __instance.vitals) {
                    if (panel.PlayerIcon != null && panel.PlayerIcon.cosmetics.skin != null) {
                         panel.PlayerIcon.cosmetics.skin.transform.position = new Vector3(0, 0, 0f);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Update))]
        class VitalsMinigameUpdatePatch {

            static void Postfix(VitalsMinigame __instance) {
                // Hacker show time since death
                
                if (Hacker.Instance.IsLocalPlayer && Hacker.Instance.Timer > 0) {
                    for (int k = 0; k < __instance.vitals.Length; k++) {
                        VitalsPanel vitalsPanel = __instance.vitals[k];
                        GameData.PlayerInfo player = vitalsPanel.PlayerInfo;

                        // Hacker update
                        if (vitalsPanel.IsDead) {
                            DeadPlayer deadPlayer = deadPlayers?.Where(x => x.player?.PlayerId == player?.PlayerId)?.FirstOrDefault();
                            if (deadPlayer != null && k < hackerTexts.Count && hackerTexts[k] != null) {
                                float timeSinceDeath = ((float)(DateTime.UtcNow - deadPlayer.timeOfDeath).TotalMilliseconds);
                                hackerTexts[k].gameObject.SetActive(true);
                                hackerTexts[k].text = Math.Round(timeSinceDeath / 1000) + "s";
                            }
                        }
                    }
                } else {
                    foreach (TMPro.TextMeshPro text in hackerTexts)
                        if (text != null && text.gameObject != null)
                            text.gameObject.SetActive(false);
                }
            }
        }
    }

    [HarmonyPatch]
    class AdminPanelPatch {
        static Dictionary<SystemTypes, List<Color>> players = new Dictionary<SystemTypes, System.Collections.Generic.List<Color>>();

        [HarmonyPatch(typeof(MapCountOverlay), nameof(MapCountOverlay.Update))]
        class MapCountOverlayUpdatePatch {
            static bool Prefix(MapCountOverlay __instance) {
                // Save colors for the Hacker
                __instance.timer += Time.deltaTime;
                if (__instance.timer < 0.1f)
                {
                    return false;
                }
                __instance.timer = 0f;
                players = new Dictionary<SystemTypes, List<Color>>();
                bool commsActive = false;
                    foreach (PlayerTask task in CachedPlayer.LocalPlayer.PlayerControl.myTasks.GetFastEnumerator())
                        if (task.TaskType == TaskTypes.FixComms) commsActive = true;       


                if (!__instance.isSab && commsActive)
                {
                    __instance.isSab = true;
                    __instance.BackgroundColor.SetColor(Palette.DisabledGrey);
                    __instance.SabotageText.gameObject.SetActive(true);
                    return false;
                }
                if (__instance.isSab && !commsActive)
                {
                    __instance.isSab = false;
                    __instance.BackgroundColor.SetColor(Color.green);
                    __instance.SabotageText.gameObject.SetActive(false);
                }

                for (int i = 0; i < __instance.CountAreas.Length; i++)
                {
                    CounterArea counterArea = __instance.CountAreas[i];
                    List<Color> roomColors = new List<Color>();
                    players.Add(counterArea.RoomType, roomColors);

                    if (!commsActive)
                    {
                        PlainShipRoom plainShipRoom = MapUtilities.CachedShipStatus.FastRooms[counterArea.RoomType];

                        if (plainShipRoom != null && plainShipRoom.roomArea) {


                            HashSet<int> hashSet = new HashSet<int>();
                            int num = plainShipRoom.roomArea.OverlapCollider(__instance.filter, __instance.buffer);
                            int num2 = 0;
                            for (int j = 0; j < num; j++) {
                                Collider2D collider2D = __instance.buffer[j];
                                if (collider2D.CompareTag("DeadBody") && __instance.includeDeadBodies) {
                                    num2++;
                                    DeadBody bodyComponent = collider2D.GetComponent<DeadBody>();
                                    if (bodyComponent) {
                                        GameData.PlayerInfo playerInfo = GameData.Instance.GetPlayerById(bodyComponent.ParentId);
                                        if (playerInfo != null) {
                                            var color = Palette.PlayerColors[playerInfo.DefaultOutfit.ColorId];
                                            if (Hacker.Instance.OnlyColorType)
                                                color = Helpers.isLighterColor(playerInfo.DefaultOutfit.ColorId) ? Palette.PlayerColors[7] : Palette.PlayerColors[6];
                                            roomColors.Add(color);
                                        }
                                    }
                                } else {
                                    PlayerControl component = collider2D.GetComponent<PlayerControl>();
                                    if (component && component.Data != null && !component.Data.Disconnected && !component.Data.IsDead && (__instance.showLivePlayerPosition || !component.AmOwner) && hashSet.Add((int)component.PlayerId)) {
                                        num2++;
                                        if (component?.cosmetics?.currentBodySprite?.BodySprite?.material != null) {
                                            Color color = component.cosmetics.currentBodySprite.BodySprite.material.GetColor("_BodyColor");
                                            if (Hacker.Instance.OnlyColorType) {
                                                var id = Mathf.Max(0, Palette.PlayerColors.IndexOf(color));
                                                color = Helpers.isLighterColor((byte)id) ? Palette.PlayerColors[7] : Palette.PlayerColors[6];
                                            }
                                            roomColors.Add(color);
                                        }
                                    }
                                }
                            }

                            counterArea.UpdateCount(num2);
                        }
                        else
                        {
                            Debug.LogWarning("Couldn't find counter for:" + counterArea.RoomType);
                        }
                    }
                    else
                    {
                        counterArea.UpdateCount(0);
                    }
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(CounterArea), nameof(CounterArea.UpdateCount))]
        class CounterAreaUpdateCountPatch {
            private static Material defaultMat;
            private static Material newMat;
            static void Postfix(CounterArea __instance) {
                // Hacker display saved colors on the admin panel
                bool showHackerInfo = Hacker.Instance.IsLocalPlayer && Hacker.Instance.Timer > 0;
                if (players.ContainsKey(__instance.RoomType)) {
                    List<Color> colors = players[__instance.RoomType];
                    int i = -1;
                    foreach (var icon in __instance.myIcons.GetFastEnumerator())
                    {
                        i += 1;
                        SpriteRenderer renderer = icon.GetComponent<SpriteRenderer>();

                        if (renderer != null) {
                            if (defaultMat == null) defaultMat = renderer.material;
                            if (newMat == null) newMat = UnityEngine.Object.Instantiate<Material>(defaultMat);
                            if (showHackerInfo && colors.Count > i) {
                                renderer.material = newMat;
                                var color = colors[i];
                                renderer.material.SetColor("_BodyColor", color);
                                var id = Palette.PlayerColors.IndexOf(color);
                                if (id < 0) {
                                    renderer.material.SetColor("_BackColor", color);
                                } else {
                                    renderer.material.SetColor("_BackColor", Palette.ShadowColors[id]);
                                }
                                renderer.material.SetColor("_VisorColor", Palette.VisorColor);
                            } else {
                                renderer.material = defaultMat;
                            }
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch]
    class SurveillanceMinigamePatch {
        private static int page = 0;
        private static float timer = 0f;

        public static List<GameObject> nightVisionOverlays = null;
        private static Sprite overlaySprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.NightVisionOverlay.png", 350f);
        public static bool nightVisionIsActive = false;
        private static bool isLightsOut;

        [HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Begin))]
        class SurveillanceMinigameBeginPatch {
            public static void Postfix(SurveillanceMinigame __instance) {
                // Add securityGuard cameras
                page = 0;
                timer = 0;
                if (MapUtilities.CachedShipStatus.AllCameras.Length > 4 && __instance.FilteredRooms.Length > 0) {
                    __instance.textures = __instance.textures.ToList().Concat(new RenderTexture[MapUtilities.CachedShipStatus.AllCameras.Length - 4]).ToArray();
                    for (int i = 4; i < MapUtilities.CachedShipStatus.AllCameras.Length; i++) {
                        SurvCamera surv = MapUtilities.CachedShipStatus.AllCameras[i];
                        Camera camera = UnityEngine.Object.Instantiate<Camera>(__instance.CameraPrefab);
                        camera.transform.SetParent(__instance.transform);
                        camera.transform.position = new Vector3(surv.transform.position.x, surv.transform.position.y, 8f);
                        camera.orthographicSize = 2.35f;
                        RenderTexture temporary = RenderTexture.GetTemporary(256, 256, 16, (RenderTextureFormat)0);
                        __instance.textures[i] = temporary;
                        camera.targetTexture = temporary;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Update))]
        class SurveillanceMinigameUpdatePatch {
            public static bool Prefix(SurveillanceMinigame __instance) {
                // Update normal and securityGuard cameras
                timer += Time.deltaTime;
                int numberOfPages = Mathf.CeilToInt(MapUtilities.CachedShipStatus.AllCameras.Length / 4f);

                bool update = false;

                if (timer > 3f || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) {
                    update = true;
                    timer = 0f;
                    page = (page + 1) % numberOfPages;
                } else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Q)) {
                    page = (page + numberOfPages - 1) % numberOfPages;
                    update = true;
                    timer = 0f;
                }

                if ((__instance.isStatic || update) && !PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(CachedPlayer.LocalPlayer.PlayerControl)) {
                    __instance.isStatic = false;
                    for (int i = 0; i < __instance.ViewPorts.Length; i++) {
                        __instance.ViewPorts[i].sharedMaterial = __instance.DefaultMaterial;
                        __instance.SabText[i].gameObject.SetActive(false);
                        if (page * 4 + i < __instance.textures.Length)
                            __instance.ViewPorts[i].material.SetTexture("_MainTex", __instance.textures[page * 4 + i]);
                        else
                            __instance.ViewPorts[i].sharedMaterial = __instance.StaticMaterial;
                    }
                } else if (!__instance.isStatic && PlayerTask.PlayerHasTaskOfType<HudOverrideTask>(CachedPlayer.LocalPlayer.PlayerControl)) {
                    __instance.isStatic = true;
                    for (int j = 0; j < __instance.ViewPorts.Length; j++) {
                        __instance.ViewPorts[j].sharedMaterial = __instance.StaticMaterial;
                        __instance.SabText[j].gameObject.SetActive(true);
                    }
                }

                nightVisionUpdate(SkeldCamsMinigame: __instance);
                return false;
            }
        }

        [HarmonyPatch(typeof(PlanetSurveillanceMinigame), nameof(PlanetSurveillanceMinigame.Update))]
        class PlanetSurveillanceMinigameUpdatePatch {
            public static void Postfix(PlanetSurveillanceMinigame __instance) {
                if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                    __instance.NextCamera(1);
                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Q))
                    __instance.NextCamera(-1);

                nightVisionUpdate(SwitchCamsMinigame: __instance);
            }
        }

        [HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.OnDestroy))]
        class SurveillanceMinigameDestroyPatch {
            public static void Prefix() {
                resetNightVision();
            }
        }

        [HarmonyPatch(typeof(PlanetSurveillanceMinigame), nameof(PlanetSurveillanceMinigame.OnDestroy))]
        class PlanetSurveillanceMinigameDestroyPatch {
            public static void Prefix() {
                resetNightVision();
            }
        }


        private static void nightVisionUpdate(SurveillanceMinigame SkeldCamsMinigame = null, PlanetSurveillanceMinigame SwitchCamsMinigame = null) {
            if (nightVisionOverlays == null) {
                GameObject closeButton = null;
                List<MeshRenderer> viewPorts = new();
                Transform viewablesTransform = null;
                if (SkeldCamsMinigame != null) {
                    closeButton = SkeldCamsMinigame.Viewables.transform.Find("CloseButton").gameObject;
                    foreach (var rend in SkeldCamsMinigame.ViewPorts) viewPorts.Add(rend);
                    viewablesTransform = SkeldCamsMinigame.Viewables.transform;
                } else if (SwitchCamsMinigame != null) {
                    closeButton = SwitchCamsMinigame.Viewables.transform.Find("CloseButton").gameObject;
                    viewPorts.Add(SwitchCamsMinigame.ViewPort);
                    viewablesTransform = SwitchCamsMinigame.Viewables.transform;
                } else return;

                nightVisionOverlays = new List<GameObject>();

                foreach (var renderer in viewPorts) {
                    GameObject overlayObject = GameObject.Instantiate(closeButton, viewablesTransform);
                    overlayObject.transform.position = new Vector3(renderer.transform.position.x, renderer.transform.position.y, overlayObject.transform.position.z);
                    overlayObject.transform.localScale = (SkeldCamsMinigame != null) ? new Vector3(0.91f, 0.612f, 1f) : new Vector3(2.124f, 1.356f, 1f);
                    overlayObject.layer = closeButton.layer;
                    var overlayRenderer = overlayObject.GetComponent<SpriteRenderer>();
                    overlayRenderer.sprite = overlaySprite;
                    overlayObject.SetActive(false);
                    GameObject.Destroy(overlayObject.GetComponent<CircleCollider2D>());
                    nightVisionOverlays.Add(overlayObject);
                }
            }


            isLightsOut = CachedPlayer.LocalPlayer.PlayerControl.myTasks.ToArray().Any(x => x.name.Contains("FixLightsTask"));
            bool ignoreNightVision = CustomOptions.CamerasNightVisionIfImpostor && Helpers.hasImpVision(GameData.Instance.GetPlayerById(CachedPlayer.LocalPlayer.PlayerId)) || CachedPlayer.LocalPlayer.Data.IsDead;
            bool nightVisionEnabled = CustomOptions.CamerasNightVision;

            if (isLightsOut && !nightVisionIsActive && nightVisionEnabled && !ignoreNightVision) {  // only update when something changed!
                foreach (PlayerControl pc in CachedPlayer.AllPlayers) {
                    if (pc == Ninja.Instance.Player && Ninja.Instance.InvisibilityTimer > 0f) {
                        continue;
                    }
                    pc.setLook("", 11, "", "", "", "", false);
                }
                foreach (var overlayObject in nightVisionOverlays) {
                    overlayObject.SetActive(true);
                }
                // Dead Bodies
                foreach (DeadBody deadBody in GameObject.FindObjectsOfType<DeadBody>()) {
                    SpriteRenderer component = deadBody.bodyRenderers.FirstOrDefault();
                    component.material.SetColor("_BackColor", Palette.ShadowColors[11]);
                    component.material.SetColor("_BodyColor", Palette.PlayerColors[11]);
                }
                nightVisionIsActive = true;
            } else if (!isLightsOut && nightVisionIsActive) {
                resetNightVision();
            }
        }

        private static void resetNightVision() {
            foreach (var go in nightVisionOverlays) {
                go.Destroy();
            }
            nightVisionOverlays = null;

            if (nightVisionIsActive) {
                nightVisionIsActive = false;
                foreach (PlayerControl pc in CachedPlayer.AllPlayers) {
                    if (Camouflager.Instance.CamouflageTimer > 0) {
                        pc.setLook("", 6, "", "", "", "", false);
                    } else if (pc == Morphling.Instance.Player && Morphling.Instance.MorphTimer > 0) {
                        PlayerControl target = Morphling.Instance.MorphTarget;
                        Morphling.Instance.Player.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId, false);
                    } else if (pc == Ninja.Instance.Player && Ninja.Instance.InvisibilityTimer > 0f) {
                        continue;
                    } else {
                        Helpers.setDefaultLook(pc, false);
                    }
                    // Dead Bodies
                    foreach (DeadBody deadBody in GameObject.FindObjectsOfType<DeadBody>()) {
                        var colorId = GameData.Instance.GetPlayerById(deadBody.ParentId).Object.Data.DefaultOutfit.ColorId;
                        SpriteRenderer component = deadBody.bodyRenderers.FirstOrDefault();
                        component.material.SetColor("_BackColor", Palette.ShadowColors[colorId]);
                        component.material.SetColor("_BodyColor", Palette.PlayerColors[colorId]);
                    }
                }
            }

        }

        public static void enforceNightVision(PlayerControl player) {
            if (isLightsOut && nightVisionOverlays != null && nightVisionIsActive) {
                player.setLook("", 11, "", "", "", "", false);
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetPlayerMaterialColors))]
        public static void Postfix(PlayerControl __instance, SpriteRenderer rend) {
            if (!nightVisionIsActive) return;
            foreach (DeadBody deadBody in GameObject.FindObjectsOfType<DeadBody>()) {
                foreach (SpriteRenderer component in new SpriteRenderer[2] { deadBody.bodyRenderers.FirstOrDefault(), deadBody.bloodSplatter }) { 
                    component.material.SetColor("_BackColor", Palette.ShadowColors[11]);
                    component.material.SetColor("_BodyColor", Palette.PlayerColors[11]);
                }
            }
        }
    }

    [HarmonyPatch(typeof(MedScanMinigame), nameof(MedScanMinigame.FixedUpdate))]
    class MedScanMinigameFixedUpdatePatch {
        static void Prefix(MedScanMinigame __instance) {
            if (TORMapOptions.allowParallelMedBayScans) {
                __instance.medscan.CurrentUser = CachedPlayer.LocalPlayer.PlayerId;
                __instance.medscan.UsersList.Clear();
            }
        }
    }

    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowSabotageMap))]
    class ShowSabotageMapPatch {
        static bool Prefix(MapBehaviour __instance) {
            if (HideNSeek.isHideNSeekGM)
                return HideNSeek.canSabotage;
            return true;
        }
    }

}
