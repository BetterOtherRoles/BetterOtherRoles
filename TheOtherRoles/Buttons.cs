using HarmonyLib;
using Hazel;
using System;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;
using TheOtherRoles.Objects;
using System.Linq;
using System.Collections.Generic;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;
using TheOtherRoles.CustomGameModes;
using TheOtherRoles.Customs.Modifiers;
using TheOtherRoles.Customs.Roles.Crewmate;
using TheOtherRoles.Customs.Roles.Impostor;
using TheOtherRoles.Customs.Roles.Neutral;

namespace TheOtherRoles
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    static class HudManagerStartPatch
    {
        private static bool initialized = false;

        private static CustomButton engineerRepairButton;
        private static CustomButton janitorCleanButton;
        public static CustomButton sheriffKillButton;
        private static CustomButton deputyHandcuffButton;
        private static CustomButton timeMasterShieldButton;
        private static CustomButton medicShieldButton;
        public static CustomButton shifterShiftButton;
        private static CustomButton morphlingButton;
        private static CustomButton camouflagerButton;
        private static CustomButton portalmakerPlacePortalButton;
        private static CustomButton usePortalButton;
        private static CustomButton portalmakerMoveToPortalButton;
        private static CustomButton hackerButton;
        public static CustomButton hackerVitalsButton;
        public static CustomButton hackerAdminTableButton;
        private static CustomButton trackerTrackPlayerButton;
        private static CustomButton trackerTrackCorpsesButton;
        public static CustomButton vampireKillButton;
        public static CustomButton garlicButton;
        public static CustomButton jackalKillButton;
        public static CustomButton sidekickKillButton;
        private static CustomButton jackalSidekickButton;
        private static CustomButton eraserButton;
        private static CustomButton placeJackInTheBoxButton;        
        private static CustomButton lightsOutButton;
        public static CustomButton cleanerCleanButton;
        public static CustomButton warlockCurseButton;
        public static CustomButton securityGuardButton;
        public static CustomButton securityGuardCamButton;
        public static CustomButton arsonistButton;
        public static CustomButton vultureEatButton;
        public static CustomButton mediumButton;
        public static CustomButton pursuerButton;
        public static CustomButton witchSpellButton;
        public static CustomButton ninjaButton;
        public static CustomButton mayorMeetingButton;
        public static CustomButton thiefKillButton;
        public static CustomButton trapperButton;
        public static CustomButton bomberButton;
        public static CustomButton defuseButton;
        public static CustomButton zoomOutButton;
        private static CustomButton hunterLighterButton;
        private static CustomButton hunterAdminTableButton;
        private static CustomButton hunterArrowButton;
        private static CustomButton huntedShieldButton;

        public static Dictionary<byte, List<CustomButton>> deputyHandcuffedButtons = null;
        public static PoolablePlayer targetDisplay;

        public static TMPro.TMP_Text securityGuardButtonScrewsText;
        public static TMPro.TMP_Text securityGuardChargesText;
        public static TMPro.TMP_Text deputyButtonHandcuffsText;
        public static TMPro.TMP_Text pursuerButtonBlanksText;
        public static TMPro.TMP_Text hackerAdminTableChargesText;
        public static TMPro.TMP_Text hackerVitalsChargesText;
        public static TMPro.TMP_Text trapperChargesText;
        public static TMPro.TMP_Text portalmakerButtonText1;
        public static TMPro.TMP_Text portalmakerButtonText2;
        public static TMPro.TMP_Text huntedShieldCountText;

        public static void setCustomButtonCooldowns() {
            if (!initialized) {
                try {
                    createButtonsPostfix(HudManager.Instance);
                } 
                catch {
                    TheOtherRolesPlugin.Logger.LogWarning("Button cooldowns not set, either the gamemode does not require them or there's something wrong.");
                    return;
                }
            }
            engineerRepairButton.MaxTimer = 0f;
            janitorCleanButton.MaxTimer = Janitor.cooldown;
            sheriffKillButton.MaxTimer = Sheriff.cooldown;
            deputyHandcuffButton.MaxTimer = Deputy.handcuffCooldown;
            timeMasterShieldButton.MaxTimer = TimeMaster.cooldown;
            medicShieldButton.MaxTimer = 0f;
            shifterShiftButton.MaxTimer = 0f;
            morphlingButton.MaxTimer = Morphling.cooldown;
            camouflagerButton.MaxTimer = Camouflager.cooldown;
            portalmakerPlacePortalButton.MaxTimer = Portalmaker.cooldown;
            usePortalButton.MaxTimer = Portalmaker.usePortalCooldown;
            portalmakerMoveToPortalButton.MaxTimer = Portalmaker.usePortalCooldown;
            hackerButton.MaxTimer = Hacker.cooldown;
            hackerVitalsButton.MaxTimer = Hacker.cooldown;
            hackerAdminTableButton.MaxTimer = Hacker.cooldown;
            vampireKillButton.MaxTimer = Vampire.cooldown;
            trackerTrackPlayerButton.MaxTimer = 0f;
            garlicButton.MaxTimer = 0f;
            jackalKillButton.MaxTimer = Jackal.cooldown;
            sidekickKillButton.MaxTimer = Sidekick.cooldown;
            jackalSidekickButton.MaxTimer = Jackal.createSidekickCooldown;
            eraserButton.MaxTimer = Eraser.cooldown;
            placeJackInTheBoxButton.MaxTimer = Trickster.placeBoxCooldown;
            lightsOutButton.MaxTimer = Trickster.lightsOutCooldown;
            cleanerCleanButton.MaxTimer = Cleaner.cooldown;
            warlockCurseButton.MaxTimer = Warlock.cooldown;
            securityGuardButton.MaxTimer = SecurityGuard.cooldown;
            securityGuardCamButton.MaxTimer = SecurityGuard.cooldown;
            arsonistButton.MaxTimer = Arsonist.cooldown;
            vultureEatButton.MaxTimer = Vulture.cooldown;
            mediumButton.MaxTimer = Medium.cooldown;
            pursuerButton.MaxTimer = Pursuer.cooldown;
            trackerTrackCorpsesButton.MaxTimer = Tracker.corpsesTrackingCooldown;
            witchSpellButton.MaxTimer = Witch.cooldown;
            ninjaButton.MaxTimer = Ninja.cooldown;
            thiefKillButton.MaxTimer = Thief.cooldown;
            mayorMeetingButton.MaxTimer = GameManager.Instance.LogicOptions.GetEmergencyCooldown();
            trapperButton.MaxTimer = Trapper.cooldown;
            bomberButton.MaxTimer = Bomber.bombCooldown;
            hunterLighterButton.MaxTimer = Hunter.lightCooldown;
            hunterAdminTableButton.MaxTimer = Hunter.AdminCooldown;
            hunterArrowButton.MaxTimer = Hunter.ArrowCooldown;
            huntedShieldButton.MaxTimer = Hunted.shieldCooldown;
            defuseButton.MaxTimer = 0f;
            defuseButton.Timer = 0f;

            timeMasterShieldButton.EffectDuration = TimeMaster.shieldDuration;
            hackerButton.EffectDuration = Hacker.duration;
            hackerVitalsButton.EffectDuration = Hacker.duration;
            hackerAdminTableButton.EffectDuration = Hacker.duration;
            vampireKillButton.EffectDuration = Vampire.delay;
            camouflagerButton.EffectDuration = Camouflager.duration;
            morphlingButton.EffectDuration = Morphling.duration;
            lightsOutButton.EffectDuration = Trickster.lightsOutDuration;
            arsonistButton.EffectDuration = Arsonist.duration;
            mediumButton.EffectDuration = Medium.duration;
            trackerTrackCorpsesButton.EffectDuration = Tracker.corpsesTrackingDuration;
            witchSpellButton.EffectDuration = Witch.spellCastingDuration;
            securityGuardCamButton.EffectDuration = SecurityGuard.duration;
            hunterLighterButton.EffectDuration = Hunter.lightDuration;
            hunterArrowButton.EffectDuration = Hunter.ArrowDuration;
            huntedShieldButton.EffectDuration = Hunted.shieldDuration;
            defuseButton.EffectDuration = Bomber.defuseDuration;
            bomberButton.EffectDuration = Bomber.destructionTime + Bomber.bombActiveAfter;
            // Already set the timer to the max, as the button is enabled during the game and not available at the start
            lightsOutButton.Timer = lightsOutButton.MaxTimer;
            zoomOutButton.MaxTimer = 0f;
        }

        public static void resetTimeMasterButton() {
            timeMasterShieldButton.Timer = timeMasterShieldButton.MaxTimer;
            timeMasterShieldButton.isEffectActive = false;
            timeMasterShieldButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
            SoundEffectsManager.stop("timemasterShield");
        }

        public static void resetHuntedRewindButton() {
            huntedShieldButton.Timer = huntedShieldButton.MaxTimer;
            huntedShieldButton.isEffectActive = false;
            huntedShieldButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
            SoundEffectsManager.stop("timemasterShield");
        }

        private static void addReplacementHandcuffedButton(CustomButton button, Vector3? positionOffset = null, Func<bool> couldUse = null)
        {
            Vector3 positionOffsetValue = positionOffset ?? button.PositionOffset;  // For non custom buttons, we can set these manually.
            positionOffsetValue.z = -0.1f;
            couldUse = couldUse ?? button.CouldUse;
            CustomButton replacementHandcuffedButton = new CustomButton(() => { }, () => { return true; }, couldUse, () => { }, Deputy.getHandcuffedButtonSprite(), positionOffsetValue, button.hudManager, button.actionName,
                true, Deputy.handcuffDuration, () => { }, button.mirror);
            replacementHandcuffedButton.Timer = replacementHandcuffedButton.EffectDuration;
            replacementHandcuffedButton.actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
            replacementHandcuffedButton.isEffectActive = true;
            if (deputyHandcuffedButtons.ContainsKey(CachedPlayer.LocalPlayer.PlayerId))
                deputyHandcuffedButtons[CachedPlayer.LocalPlayer.PlayerId].Add(replacementHandcuffedButton);
            else
                deputyHandcuffedButtons.Add(CachedPlayer.LocalPlayer.PlayerId, new List<CustomButton> { replacementHandcuffedButton });
        }
        
        // Disables / Enables all Buttons (except the ones disabled in the Deputy class), and replaces them with new buttons.
        public static void setAllButtonsHandcuffedStatus(bool handcuffed, bool reset = false)
        {
            if (reset) {
                deputyHandcuffedButtons = new Dictionary<byte, List<CustomButton>>();
                return;
            }
            if (handcuffed && !deputyHandcuffedButtons.ContainsKey(CachedPlayer.LocalPlayer.PlayerId))
            {
                int maxI = CustomButton.buttons.Count;
                for (int i = 0; i < maxI; i++)
                {
                    try
                    {
                        if (CustomButton.buttons[i].HasButton())  // For each custombutton the player has
                        {
                            addReplacementHandcuffedButton(CustomButton.buttons[i]);  // The new buttons are the only non-handcuffed buttons now!
                        }
                        CustomButton.buttons[i].isHandcuffed = true;
                    }
                    catch (NullReferenceException)
                    {
                        System.Console.WriteLine("[WARNING] NullReferenceException from MeetingEndedUpdate().HasButton(), if theres only one warning its fine");  // Note: idk what this is good for, but i copied it from above /gendelo
                    }
                }
                // Non Custom (Vanilla) Buttons. The Originals are disabled / hidden in UpdatePatch.cs already, just need to replace them. Can use any button, as we replace onclick etc anyways.
                // Kill Button if enabled for the Role
                if (FastDestroyableSingleton<HudManager>.Instance.KillButton.isActiveAndEnabled) addReplacementHandcuffedButton(arsonistButton, CustomButton.ButtonPositions.upperRowRight, couldUse: () => { return FastDestroyableSingleton<HudManager>.Instance.KillButton.currentTarget != null; });
                // Vent Button if enabled
                if (CachedPlayer.LocalPlayer.PlayerControl.roleCanUseVents()) addReplacementHandcuffedButton(arsonistButton, CustomButton.ButtonPositions.upperRowCenter, couldUse: () => { return FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.currentTarget != null; });
                // Report Button
                addReplacementHandcuffedButton(arsonistButton, (!CachedPlayer.LocalPlayer.Data.Role.IsImpostor) ? new Vector3(-1f, -0.06f, 0): CustomButton.ButtonPositions.lowerRowRight, () => { return FastDestroyableSingleton<HudManager>.Instance.ReportButton.graphic.color == Palette.EnabledColor; });
            }
            else if (!handcuffed && deputyHandcuffedButtons.ContainsKey(CachedPlayer.LocalPlayer.PlayerId))  // Reset to original. Disables the replacements, enables the original buttons.
            {
                foreach (CustomButton replacementButton in deputyHandcuffedButtons[CachedPlayer.LocalPlayer.PlayerId])
                {
                    replacementButton.HasButton = () => { return false; };
                    replacementButton.Update(); // To make it disappear properly.
                    CustomButton.buttons.Remove(replacementButton);
                }
                deputyHandcuffedButtons.Remove(CachedPlayer.LocalPlayer.PlayerId);

                foreach (CustomButton button in CustomButton.buttons)
                {
                    button.isHandcuffed = false;
                }
            }
        }

        private static void setButtonTargetDisplay(PlayerControl target, CustomButton button = null, Vector3? offset=null) {
            if (target == null || button == null) {
                if (targetDisplay != null) {  // Reset the poolable player
                    targetDisplay.gameObject.SetActive(false);
                    GameObject.Destroy(targetDisplay.gameObject);
                    targetDisplay = null;
                }
                return;
            }
            // Add poolable player to the button so that the target outfit is shown
            button.actionButton.cooldownTimerText.transform.localPosition = new Vector3(0, 0, -1f);  // Before the poolable player
            targetDisplay = UnityEngine.Object.Instantiate<PoolablePlayer>(Patches.IntroCutsceneOnDestroyPatch.playerPrefab, button.actionButton.transform);
            GameData.PlayerInfo data = target.Data;
            target.SetPlayerMaterialColors(targetDisplay.cosmetics.currentBodySprite.BodySprite);
            targetDisplay.SetSkin(data.DefaultOutfit.SkinId, data.DefaultOutfit.ColorId);
            targetDisplay.SetHat(data.DefaultOutfit.HatId, data.DefaultOutfit.ColorId);
            targetDisplay.cosmetics.nameText.text = "";  // Hide the name!
            targetDisplay.transform.localPosition = new Vector3(0f, 0.22f, -0.01f);
            if (offset != null) targetDisplay.transform.localPosition += (Vector3)offset;
            targetDisplay.transform.localScale = Vector3.one * 0.33f;
            targetDisplay.setSemiTransparent(false);
            targetDisplay.gameObject.SetActive(true);
        }

        public static void Postfix(HudManager __instance) {
            initialized = false;

            try {
                createButtonsPostfix(__instance);
            } catch { }
        }
         
        public static void createButtonsPostfix(HudManager __instance) {
            // get map id, or raise error to wait...
            var mapId = GameOptionsManager.Instance.currentNormalGameOptions.MapId;

            // Time Master Rewind Time
            timeMasterShieldButton = new CustomButton(
                () => {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.TimeMasterShield, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.timeMasterShield();
                    SoundEffectsManager.play("timemasterShield");
                },
                () => { return TimeMaster.timeMaster != null && TimeMaster.timeMaster == CachedPlayer.LocalPlayer.PlayerControl && !CachedPlayer.LocalPlayer.Data.IsDead; },
                () => { return CachedPlayer.LocalPlayer.PlayerControl.CanMove; },
                () => {
                    timeMasterShieldButton.Timer = timeMasterShieldButton.MaxTimer;
                    timeMasterShieldButton.isEffectActive = false;
                    timeMasterShieldButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                TimeMaster.getButtonSprite(),
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                "ActionQuaternary", 
                true,
                TimeMaster.shieldDuration,
                () => {
                    timeMasterShieldButton.Timer = timeMasterShieldButton.MaxTimer;
                    SoundEffectsManager.stop("timemasterShield");

                }
            );


            // Shifter shift
            shifterShiftButton = new CustomButton(
                () => {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.SetFutureShifted, Hazel.SendOption.Reliable, -1);
                    writer.Write(Shifter.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.setFutureShifted(Shifter.currentTarget.PlayerId);
                    SoundEffectsManager.play("shifterShift");
                },
                () => { return Shifter.shifter != null && Shifter.shifter == CachedPlayer.LocalPlayer.PlayerControl && Shifter.futureShift == null && !CachedPlayer.LocalPlayer.Data.IsDead; },
                () => { return Shifter.currentTarget && Shifter.futureShift == null && CachedPlayer.LocalPlayer.PlayerControl.CanMove; },
                () => { },
                Shifter.getButtonSprite(),
                new Vector3(0, 1f, 0),
                __instance,
                null,
                true
            );

            // Hacker button
            hackerButton = new CustomButton(
                () => {
                    Hacker.hackerTimer = Hacker.duration;
                    SoundEffectsManager.play("hackerHack");
                },
                () => { return Hacker.hacker != null && Hacker.hacker == CachedPlayer.LocalPlayer.PlayerControl && !CachedPlayer.LocalPlayer.Data.IsDead; },
                () => { return true; },
                () => {
                    hackerButton.Timer = hackerButton.MaxTimer;
                    hackerButton.isEffectActive = false;
                    hackerButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Hacker.getButtonSprite(),
                CustomButton.ButtonPositions.upperRowRight,
                __instance,
                "ActionQuaternary",
                true,
                0f,
                () => { hackerButton.Timer = hackerButton.MaxTimer;}
            );

            hackerAdminTableButton = new CustomButton(
               () => {
                   if (!MapBehaviour.Instance || !MapBehaviour.Instance.isActiveAndEnabled) {
                       HudManager __instance = FastDestroyableSingleton<HudManager>.Instance;
                       __instance.InitMap();
                       MapBehaviour.Instance.ShowCountOverlay(allowedToMove: true, showLivePlayerPosition: true, includeDeadBodies: true);
                   }
                   if (Hacker.cantMove) CachedPlayer.LocalPlayer.PlayerControl.moveable = false;
                   CachedPlayer.LocalPlayer.NetTransform.Halt(); // Stop current movement 
                   Hacker.chargesAdminTable--;
               },
               () => { return Hacker.hacker != null && Hacker.hacker == CachedPlayer.LocalPlayer.PlayerControl && !CachedPlayer.LocalPlayer.Data.IsDead;},
               () => {
                   if (hackerAdminTableChargesText != null) hackerAdminTableChargesText.text = $"{Hacker.chargesAdminTable} / {Hacker.toolsNumber}";
                   return Hacker.chargesAdminTable > 0; 
               },
               () => {
                   hackerAdminTableButton.Timer = hackerAdminTableButton.MaxTimer;
                   hackerAdminTableButton.isEffectActive = false;
                   hackerAdminTableButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
               },
               Hacker.getAdminSprite(),
               CustomButton.ButtonPositions.lowerRowRight,
               __instance,
               "ActionSecondary",
               true,
               0f,
               () => { 
                   hackerAdminTableButton.Timer = hackerAdminTableButton.MaxTimer;
                   if (!hackerVitalsButton.isEffectActive) CachedPlayer.LocalPlayer.PlayerControl.moveable = true;
                   if (MapBehaviour.Instance && MapBehaviour.Instance.isActiveAndEnabled) MapBehaviour.Instance.Close();
               },
               GameOptionsManager.Instance.currentNormalGameOptions.MapId == 3,
               "ADMIN"
           );

            // Hacker Admin Table Charges
            hackerAdminTableChargesText = GameObject.Instantiate(hackerAdminTableButton.actionButton.cooldownTimerText, hackerAdminTableButton.actionButton.cooldownTimerText.transform.parent);
            hackerAdminTableChargesText.text = "";
            hackerAdminTableChargesText.enableWordWrapping = false;
            hackerAdminTableChargesText.transform.localScale = Vector3.one * 0.5f;
            hackerAdminTableChargesText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            hackerVitalsButton = new CustomButton(
               () => {
                   if (GameOptionsManager.Instance.currentNormalGameOptions.MapId != 1) {
                       if (Hacker.vitals == null) {
                           var e = UnityEngine.Object.FindObjectsOfType<SystemConsole>().FirstOrDefault(x => x.gameObject.name.Contains("panel_vitals"));
                           if (e == null || Camera.main == null) return;
                           Hacker.vitals = UnityEngine.Object.Instantiate(e.MinigamePrefab, Camera.main.transform, false);
                       }
                       Hacker.vitals.transform.SetParent(Camera.main.transform, false);
                       Hacker.vitals.transform.localPosition = new Vector3(0.0f, 0.0f, -50f);
                       Hacker.vitals.Begin(null);
                   } else {
                       if (Hacker.doorLog == null) {
                           var e = UnityEngine.Object.FindObjectsOfType<SystemConsole>().FirstOrDefault(x => x.gameObject.name.Contains("SurvLogConsole"));
                           if (e == null || Camera.main == null) return;
                           Hacker.doorLog = UnityEngine.Object.Instantiate(e.MinigamePrefab, Camera.main.transform, false);
                       }
                       Hacker.doorLog.transform.SetParent(Camera.main.transform, false);
                       Hacker.doorLog.transform.localPosition = new Vector3(0.0f, 0.0f, -50f);
                       Hacker.doorLog.Begin(null);
                   }

                   if (Hacker.cantMove) CachedPlayer.LocalPlayer.PlayerControl.moveable = false;
                   CachedPlayer.LocalPlayer.NetTransform.Halt(); // Stop current movement 

                   Hacker.chargesVitals--;
               },
               () => { return Hacker.hacker != null && Hacker.hacker == CachedPlayer.LocalPlayer.PlayerControl && !CachedPlayer.LocalPlayer.Data.IsDead && GameOptionsManager.Instance.currentNormalGameOptions.MapId != 0 && GameOptionsManager.Instance.currentNormalGameOptions.MapId != 3; },
               () => {
                   if (hackerVitalsChargesText != null) hackerVitalsChargesText.text = $"{Hacker.chargesVitals} / {Hacker.toolsNumber}";
                   hackerVitalsButton.actionButton.graphic.sprite = GameOptionsManager.Instance.currentNormalGameOptions.MapId == 1 ? Hacker.getLogSprite() : Hacker.getVitalsSprite();
                   hackerVitalsButton.actionButton.OverrideText(GameOptionsManager.Instance.currentNormalGameOptions.MapId == 1 ? "DOORLOG" : "VITALS");
                   return Hacker.chargesVitals > 0;
               },
               () => {
                   hackerVitalsButton.Timer = hackerVitalsButton.MaxTimer;
                   hackerVitalsButton.isEffectActive = false;
                   hackerVitalsButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
               },
               Hacker.getVitalsSprite(),
               CustomButton.ButtonPositions.lowerRowCenter,
               __instance,
               "ActionSecondary",
               true,
               0f,
               () => { 
                   hackerVitalsButton.Timer = hackerVitalsButton.MaxTimer;
                   if(!hackerAdminTableButton.isEffectActive) CachedPlayer.LocalPlayer.PlayerControl.moveable = true;
                   if (Minigame.Instance) {
                       if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 1) Hacker.doorLog.ForceClose();
                       else Hacker.vitals.ForceClose();
                   }
               },
               false,
              GameOptionsManager.Instance.currentNormalGameOptions.MapId == 1 ? "DOORLOG" : "VITALS"
           );

            // Hacker Vitals Charges
            hackerVitalsChargesText = GameObject.Instantiate(hackerVitalsButton.actionButton.cooldownTimerText, hackerVitalsButton.actionButton.cooldownTimerText.transform.parent);
            hackerVitalsChargesText.text = "";
            hackerVitalsChargesText.enableWordWrapping = false;
            hackerVitalsChargesText.transform.localScale = Vector3.one * 0.5f;
            hackerVitalsChargesText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // Tracker button
            trackerTrackPlayerButton = new CustomButton(
                () => {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.TrackerUsedTracker, Hazel.SendOption.Reliable, -1);
                    writer.Write(Tracker.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.trackerUsedTracker(Tracker.currentTarget.PlayerId);
                    SoundEffectsManager.play("trackerTrackPlayer");
                },
                () => { return Tracker.tracker != null && Tracker.tracker == CachedPlayer.LocalPlayer.PlayerControl && !CachedPlayer.LocalPlayer.Data.IsDead; },
                () => { return CachedPlayer.LocalPlayer.PlayerControl.CanMove && Tracker.currentTarget != null && !Tracker.usedTracker; },
                () => { if(Tracker.resetTargetAfterMeeting) Tracker.resetTracked(); },
                Tracker.getButtonSprite(),
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                "ActionQuaternary"
            );

            trackerTrackCorpsesButton = new CustomButton(
                () => { Tracker.corpsesTrackingTimer = Tracker.corpsesTrackingDuration;
                            SoundEffectsManager.play("trackerTrackCorpses"); },
                () => { return Tracker.tracker != null && Tracker.tracker == CachedPlayer.LocalPlayer.PlayerControl && !CachedPlayer.LocalPlayer.Data.IsDead && Tracker.canTrackCorpses; },
                () => { return CachedPlayer.LocalPlayer.PlayerControl.CanMove; },
                () => {
                    trackerTrackCorpsesButton.Timer = trackerTrackCorpsesButton.MaxTimer;
                    trackerTrackCorpsesButton.isEffectActive = false;
                    trackerTrackCorpsesButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Tracker.getTrackCorpsesButtonSprite(),
                CustomButton.ButtonPositions.lowerRowCenter,
                __instance,
                "ActionSecondary",
                true,
                Tracker.corpsesTrackingDuration,
                () => {
                    trackerTrackCorpsesButton.Timer = trackerTrackCorpsesButton.MaxTimer;
                }
            );

            portalmakerPlacePortalButton = new CustomButton(
                () => {
                    portalmakerPlacePortalButton.Timer = portalmakerPlacePortalButton.MaxTimer;

                    var pos = CachedPlayer.LocalPlayer.transform.position;
                    byte[] buff = new byte[sizeof(float) * 2];
                    Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, buff, 0 * sizeof(float), sizeof(float));
                    Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, buff, 1 * sizeof(float), sizeof(float));

                    MessageWriter writer = AmongUsClient.Instance.StartRpc(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.PlacePortal, Hazel.SendOption.Reliable);
                    writer.WriteBytesAndSize(buff);
                    writer.EndMessage();
                    RPCProcedure.placePortal(buff);
                    SoundEffectsManager.play("tricksterPlaceBox");
                },
                () => { return Portalmaker.portalmaker != null && Portalmaker.portalmaker == CachedPlayer.LocalPlayer.PlayerControl && !CachedPlayer.LocalPlayer.Data.IsDead && Portal.secondPortal == null; },
                () => { return CachedPlayer.LocalPlayer.PlayerControl.CanMove && Portal.secondPortal == null; },
                () => { portalmakerPlacePortalButton.Timer = portalmakerPlacePortalButton.MaxTimer; },
                Portalmaker.getPlacePortalButtonSprite(),
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                "ActionQuaternary"
            );

            usePortalButton = new CustomButton(
                () => {
                    bool didTeleport = false;
                    Vector3 exit = Portal.findExit(CachedPlayer.LocalPlayer.transform.position);
                    Vector3 entry = Portal.findEntry(CachedPlayer.LocalPlayer.transform.position);

                    bool portalMakerSoloTeleport = !Portal.locationNearEntry(CachedPlayer.LocalPlayer.transform.position);
                    if (portalMakerSoloTeleport) {
                        exit = Portal.firstPortal.portalGameObject.transform.position;
                        entry = CachedPlayer.LocalPlayer.transform.position;
                    }

                    CachedPlayer.LocalPlayer.NetTransform.RpcSnapTo(entry);

                    if (!CachedPlayer.LocalPlayer.Data.IsDead) {  // Ghosts can portal too, but non-blocking and only with a local animation
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.UsePortal, Hazel.SendOption.Reliable, -1);
                        writer.Write((byte)CachedPlayer.LocalPlayer.PlayerId);
                        writer.Write(portalMakerSoloTeleport ? (byte)1 : (byte)0);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                    }
                    RPCProcedure.usePortal(CachedPlayer.LocalPlayer.PlayerId, portalMakerSoloTeleport ? (byte)1 : (byte)0);
                    usePortalButton.Timer = usePortalButton.MaxTimer;
                    portalmakerMoveToPortalButton.Timer = usePortalButton.MaxTimer;
                    SoundEffectsManager.play("portalUse");
                    FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Portal.teleportDuration, new Action<float>((p) => { // Delayed action
                        CachedPlayer.LocalPlayer.PlayerControl.moveable = false;
                        CachedPlayer.LocalPlayer.NetTransform.Halt();
                        if (p >= 0.5f && p <= 0.53f && !didTeleport && !MeetingHud.Instance) {
                            if (SubmergedCompatibility.IsSubmerged) {
                                SubmergedCompatibility.ChangeFloor(exit.y > -7);
                            }
                            CachedPlayer.LocalPlayer.NetTransform.RpcSnapTo(exit);
                            didTeleport = true;
                        }
                        if (p == 1f) {
                            CachedPlayer.LocalPlayer.PlayerControl.moveable = true;
                        }
                    })));
                    },
                () => {
                    if (CachedPlayer.LocalPlayer.PlayerControl == Portalmaker.portalmaker && Portal.bothPlacedAndEnabled)
                        portalmakerButtonText1.text = Portal.locationNearEntry(CachedPlayer.LocalPlayer.transform.position) || !Portalmaker.canPortalFromAnywhere ? "" : "1. " + Portal.firstPortal.room;
                    return Portal.bothPlacedAndEnabled; },
                () => { return CachedPlayer.LocalPlayer.PlayerControl.CanMove && (Portal.locationNearEntry(CachedPlayer.LocalPlayer.transform.position) || Portalmaker.canPortalFromAnywhere && CachedPlayer.LocalPlayer.PlayerControl == Portalmaker.portalmaker) && !Portal.isTeleporting; },
                () => { usePortalButton.Timer = usePortalButton.MaxTimer; },
                Portalmaker.getUsePortalButtonSprite(),
                new Vector3(0.9f, -0.06f, 0),
                __instance,
                "UsePortal",
                mirror: true
            );

            portalmakerMoveToPortalButton = new CustomButton(
                () => {
                    bool didTeleport = false;
                    Vector3 exit = Portal.secondPortal.portalGameObject.transform.position;

                    if (!CachedPlayer.LocalPlayer.Data.IsDead) {  // Ghosts can portal too, but non-blocking and only with a local animation
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.UsePortal, Hazel.SendOption.Reliable, -1);
                        writer.Write((byte)CachedPlayer.LocalPlayer.PlayerId);
                        writer.Write((byte)2);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                    }
                    RPCProcedure.usePortal(CachedPlayer.LocalPlayer.PlayerId, 2);
                    usePortalButton.Timer = usePortalButton.MaxTimer;
                    portalmakerMoveToPortalButton.Timer = usePortalButton.MaxTimer;
                    SoundEffectsManager.play("portalUse");
                    FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Portal.teleportDuration, new Action<float>((p) => { // Delayed action
                        CachedPlayer.LocalPlayer.PlayerControl.moveable = false;
                        CachedPlayer.LocalPlayer.NetTransform.Halt();
                        if (p >= 0.5f && p <= 0.53f && !didTeleport && !MeetingHud.Instance) {
                            if (SubmergedCompatibility.IsSubmerged) {
                                SubmergedCompatibility.ChangeFloor(exit.y > -7);
                            }
                            CachedPlayer.LocalPlayer.NetTransform.RpcSnapTo(exit);
                            didTeleport = true;
                        }
                        if (p == 1f) {
                            CachedPlayer.LocalPlayer.PlayerControl.moveable = true;
                        }
                    })));
                },
                () => { return Portalmaker.canPortalFromAnywhere && Portal.bothPlacedAndEnabled && CachedPlayer.LocalPlayer.PlayerControl == Portalmaker.portalmaker; },
                () => { return CachedPlayer.LocalPlayer.PlayerControl.CanMove && !Portal.locationNearEntry(CachedPlayer.LocalPlayer.transform.position) && !Portal.isTeleporting; },
                () => { portalmakerMoveToPortalButton.Timer = usePortalButton.MaxTimer; },
                Portalmaker.getUsePortalButtonSprite(),
                new Vector3(0.9f, 1f, 0),
                __instance,
                "PortalMakerTeleportation",
                mirror: true
            );


            portalmakerButtonText1 = GameObject.Instantiate(usePortalButton.actionButton.cooldownTimerText, usePortalButton.actionButton.cooldownTimerText.transform.parent);
            portalmakerButtonText1.text = "";
            portalmakerButtonText1.enableWordWrapping = false;
            portalmakerButtonText1.transform.localScale = Vector3.one * 0.5f;
            portalmakerButtonText1.transform.localPosition += new Vector3(-0.05f, 0.55f, -1f);

            portalmakerButtonText2 = GameObject.Instantiate(portalmakerMoveToPortalButton.actionButton.cooldownTimerText, portalmakerMoveToPortalButton.actionButton.cooldownTimerText.transform.parent);
            portalmakerButtonText2.text = "";
            portalmakerButtonText2.enableWordWrapping = false;
            portalmakerButtonText2.transform.localScale = Vector3.one * 0.5f;
            portalmakerButtonText2.transform.localPosition += new Vector3(-0.05f, 0.55f, -1f);

            // Security Guard button
            securityGuardButton = new CustomButton(
                () => {
                    if (SecurityGuard.ventTarget != null) { // Seal vent
                        MessageWriter writer = AmongUsClient.Instance.StartRpc(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.SealVent, Hazel.SendOption.Reliable);
                        writer.WritePacked(SecurityGuard.ventTarget.Id);
                        writer.EndMessage();
                        RPCProcedure.sealVent(SecurityGuard.ventTarget.Id);
                        SecurityGuard.ventTarget = null;
                        
                    } else if (GameOptionsManager.Instance.currentNormalGameOptions.MapId != 1 && !SubmergedCompatibility.IsSubmerged) { // Place camera if there's no vent and it's not MiraHQ or Submerged
                        var pos = CachedPlayer.LocalPlayer.transform.position;
                        byte[] buff = new byte[sizeof(float) * 2];
                        Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, buff, 0*sizeof(float), sizeof(float));
                        Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, buff, 1*sizeof(float), sizeof(float));

                        MessageWriter writer = AmongUsClient.Instance.StartRpc(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.PlaceCamera, Hazel.SendOption.Reliable);
                        writer.WriteBytesAndSize(buff);
                        writer.EndMessage();
                        RPCProcedure.placeCamera(buff); 
                    }
                    SoundEffectsManager.play("securityGuardPlaceCam");  // Same sound used for both types (cam or vent)!
                    securityGuardButton.Timer = securityGuardButton.MaxTimer;
                },
                () => { return SecurityGuard.securityGuard != null && SecurityGuard.securityGuard == CachedPlayer.LocalPlayer.PlayerControl && !CachedPlayer.LocalPlayer.Data.IsDead && SecurityGuard.remainingScrews >= Mathf.Min(SecurityGuard.ventPrice, SecurityGuard.camPrice); },
                () => {
                    securityGuardButton.actionButton.graphic.sprite = (SecurityGuard.ventTarget == null && GameOptionsManager.Instance.currentNormalGameOptions.MapId != 1 && !SubmergedCompatibility.IsSubmerged) ? SecurityGuard.getPlaceCameraButtonSprite() : SecurityGuard.getCloseVentButtonSprite(); 
                    if (securityGuardButtonScrewsText != null) securityGuardButtonScrewsText.text = $"{SecurityGuard.remainingScrews}/{SecurityGuard.totalScrews}";

                    if (SecurityGuard.ventTarget != null)
                        return SecurityGuard.remainingScrews >= SecurityGuard.ventPrice && CachedPlayer.LocalPlayer.PlayerControl.CanMove;
                    return GameOptionsManager.Instance.currentNormalGameOptions.MapId != 1 && !SubmergedCompatibility.IsSubmerged && SecurityGuard.remainingScrews >= SecurityGuard.camPrice && CachedPlayer.LocalPlayer.PlayerControl.CanMove;
                },
                () => { securityGuardButton.Timer = securityGuardButton.MaxTimer; },
                SecurityGuard.getPlaceCameraButtonSprite(),
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                "ActionQuaternary"
            );
            
            // Security Guard button screws counter
            securityGuardButtonScrewsText = GameObject.Instantiate(securityGuardButton.actionButton.cooldownTimerText, securityGuardButton.actionButton.cooldownTimerText.transform.parent);
            securityGuardButtonScrewsText.text = "";
            securityGuardButtonScrewsText.enableWordWrapping = false;
            securityGuardButtonScrewsText.transform.localScale = Vector3.one * 0.5f;
            securityGuardButtonScrewsText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            securityGuardCamButton = new CustomButton(
                () => {
                    if (GameOptionsManager.Instance.currentNormalGameOptions.MapId != 1) {
                        if (SecurityGuard.minigame == null) {
                            byte mapId = GameOptionsManager.Instance.currentNormalGameOptions.MapId;
                            var e = UnityEngine.Object.FindObjectsOfType<SystemConsole>().FirstOrDefault(x => x.gameObject.name.Contains("Surv_Panel"));
                            if (mapId == 0 || mapId == 3) e = UnityEngine.Object.FindObjectsOfType<SystemConsole>().FirstOrDefault(x => x.gameObject.name.Contains("SurvConsole"));
                            else if (mapId == 4) e = UnityEngine.Object.FindObjectsOfType<SystemConsole>().FirstOrDefault(x => x.gameObject.name.Contains("task_cams"));
                            if (e == null || Camera.main == null) return;
                            SecurityGuard.minigame = UnityEngine.Object.Instantiate(e.MinigamePrefab, Camera.main.transform, false);
                        }
                        SecurityGuard.minigame.transform.SetParent(Camera.main.transform, false);
                        SecurityGuard.minigame.transform.localPosition = new Vector3(0.0f, 0.0f, -50f);
                        SecurityGuard.minigame.Begin(null);
                    } else {
                        if (SecurityGuard.minigame == null) {
                            var e = UnityEngine.Object.FindObjectsOfType<SystemConsole>().FirstOrDefault(x => x.gameObject.name.Contains("SurvLogConsole"));
                            if (e == null || Camera.main == null) return;
                            SecurityGuard.minigame = UnityEngine.Object.Instantiate(e.MinigamePrefab, Camera.main.transform, false);
                        }
                        SecurityGuard.minigame.transform.SetParent(Camera.main.transform, false);
                        SecurityGuard.minigame.transform.localPosition = new Vector3(0.0f, 0.0f, -50f);
                        SecurityGuard.minigame.Begin(null);
                    }
                    SecurityGuard.charges--;

                    if (SecurityGuard.cantMove) CachedPlayer.LocalPlayer.PlayerControl.moveable = false;
                    CachedPlayer.LocalPlayer.NetTransform.Halt(); // Stop current movement 
                },
                () => { return SecurityGuard.securityGuard != null && SecurityGuard.securityGuard == CachedPlayer.LocalPlayer.PlayerControl && !CachedPlayer.LocalPlayer.Data.IsDead && SecurityGuard.remainingScrews < Mathf.Min(SecurityGuard.ventPrice, SecurityGuard.camPrice)
                               && !SubmergedCompatibility.IsSubmerged; },
                () => {
                    if (securityGuardChargesText != null) securityGuardChargesText.text = $"{SecurityGuard.charges} / {SecurityGuard.maxCharges}";
                    securityGuardCamButton.actionButton.graphic.sprite = GameOptionsManager.Instance.currentNormalGameOptions.MapId == 1 ? SecurityGuard.getLogSprite() : SecurityGuard.getCamSprite();
                    securityGuardCamButton.actionButton.OverrideText(GameOptionsManager.Instance.currentNormalGameOptions.MapId == 1 ? "DOORLOG" : "SECURITY");
                    return CachedPlayer.LocalPlayer.PlayerControl.CanMove && SecurityGuard.charges > 0;
                },
                () => {
                    securityGuardCamButton.Timer = securityGuardCamButton.MaxTimer;
                    securityGuardCamButton.isEffectActive = false;
                    securityGuardCamButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                SecurityGuard.getCamSprite(),
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                "ActionSecondary",
                true,
                0f,
                () => {
                    securityGuardCamButton.Timer = securityGuardCamButton.MaxTimer;
                    if (Minigame.Instance) {
                        SecurityGuard.minigame.ForceClose();
                    }
                    CachedPlayer.LocalPlayer.PlayerControl.moveable = true;
                },
                false,
                GameOptionsManager.Instance.currentNormalGameOptions.MapId == 1 ? "DOORLOG" : "SECURITY"
            );

            // Security Guard cam button charges
            securityGuardChargesText = GameObject.Instantiate(securityGuardCamButton.actionButton.cooldownTimerText, securityGuardCamButton.actionButton.cooldownTimerText.transform.parent);
            securityGuardChargesText.text = "";
            securityGuardChargesText.enableWordWrapping = false;
            securityGuardChargesText.transform.localScale = Vector3.one * 0.5f;
            securityGuardChargesText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // Medium button
            mediumButton = new CustomButton(
                () => {
                    if (Medium.target != null) {
                        Medium.soulTarget = Medium.target;
                        mediumButton.HasEffect = true;
                        SoundEffectsManager.play("mediumAsk");
                    }
                },
                () => { return Medium.medium != null && Medium.medium == CachedPlayer.LocalPlayer.PlayerControl && !CachedPlayer.LocalPlayer.Data.IsDead; },
                () => {
                    if (mediumButton.isEffectActive && Medium.target != Medium.soulTarget) {
                        Medium.soulTarget = null;
                        mediumButton.Timer = 0f;
                        mediumButton.isEffectActive = false;
                    }
                    return Medium.target != null && CachedPlayer.LocalPlayer.PlayerControl.CanMove;
                },
                () => {
                    mediumButton.Timer = mediumButton.MaxTimer;
                    mediumButton.isEffectActive = false;
                    Medium.soulTarget = null;
                },
                Medium.getQuestionSprite(),
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                "ActionQuaternary",
                true,
                Medium.duration,
                () => {
                    mediumButton.Timer = mediumButton.MaxTimer;
                    if (Medium.target == null || Medium.target.player == null) return;
                    string msg = Medium.getInfo(Medium.target.player, Medium.target.killerIfExisting);
                    FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(CachedPlayer.LocalPlayer.PlayerControl, msg);

                    // Ghost Info
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.ShareGhostInfo, Hazel.SendOption.Reliable, -1);
                    writer.Write(Medium.target.player.PlayerId);
                    writer.Write((byte)RPCProcedure.GhostInfoTypes.MediumInfo);
                    writer.Write(msg);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);

                    // Remove soul
                    if (Medium.oneTimeUse) {
                        float closestDistance = float.MaxValue;
                        SpriteRenderer target = null;

                        foreach ((DeadPlayer db, Vector3 ps) in Medium.deadBodies) {
                            if (db == Medium.target) {
                                Tuple<DeadPlayer, Vector3> deadBody = Tuple.Create(db, ps);
                                Medium.deadBodies.Remove(deadBody);
                                break;
                            }

                        }
                        foreach (SpriteRenderer rend in Medium.souls) {
                            float distance = Vector2.Distance(rend.transform.position, CachedPlayer.LocalPlayer.PlayerControl.GetTruePosition());
                            if (distance < closestDistance) {
                                closestDistance = distance;
                                target = rend;
                            }
                        }

                        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(5f, new Action<float>((p) => {
                            if (target != null) {
                                var tmp = target.color;
                                tmp.a = Mathf.Clamp01(1 - p);
                                target.color = tmp;
                            }
                            if (p == 1f && target != null && target.gameObject != null) UnityEngine.Object.Destroy(target.gameObject);
                        })));

                        Medium.souls.Remove(target);
                    }
                    SoundEffectsManager.stop("mediumAsk");
                }
            );

            // Pursuer button
            pursuerButton = new CustomButton(
                () => {
                    if (Pursuer.target != null) {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.SetBlanked, Hazel.SendOption.Reliable, -1);
                        writer.Write(Pursuer.target.PlayerId);
                        writer.Write(Byte.MaxValue);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.setBlanked(Pursuer.target.PlayerId, Byte.MaxValue);

                        Pursuer.target = null;

                        Pursuer.blanks++;
                        pursuerButton.Timer = pursuerButton.MaxTimer;
                        SoundEffectsManager.play("pursuerBlank");
                    }

                },
                () => { return Pursuer.pursuer != null && Pursuer.pursuer == CachedPlayer.LocalPlayer.PlayerControl && !CachedPlayer.LocalPlayer.Data.IsDead && Pursuer.blanks < Pursuer.blanksNumber; },
                () => {
                    if (pursuerButtonBlanksText != null) pursuerButtonBlanksText.text = $"{Pursuer.blanksNumber - Pursuer.blanks}";

                    return Pursuer.blanksNumber > Pursuer.blanks && CachedPlayer.LocalPlayer.PlayerControl.CanMove && Pursuer.target != null;
                },
                () => { pursuerButton.Timer = pursuerButton.MaxTimer; },
                Pursuer.getTargetSprite(),
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                "ActionQuaternary"
            );

            // Pursuer button blanks left
            pursuerButtonBlanksText = GameObject.Instantiate(pursuerButton.actionButton.cooldownTimerText, pursuerButton.actionButton.cooldownTimerText.transform.parent);
            pursuerButtonBlanksText.text = "";
            pursuerButtonBlanksText.enableWordWrapping = false;
            pursuerButtonBlanksText.transform.localScale = Vector3.one * 0.5f;
            pursuerButtonBlanksText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // Trapper button
            trapperButton = new CustomButton(
                () => {


                    var pos = CachedPlayer.LocalPlayer.transform.position;
                    byte[] buff = new byte[sizeof(float) * 2];
                    Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, buff, 0 * sizeof(float), sizeof(float));
                    Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, buff, 1 * sizeof(float), sizeof(float));

                    MessageWriter writer = AmongUsClient.Instance.StartRpc(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.SetTrap, Hazel.SendOption.Reliable);
                    writer.WriteBytesAndSize(buff);
                    writer.EndMessage();
                    RPCProcedure.setTrap(buff);

                    SoundEffectsManager.play("trapperTrap");
                    trapperButton.Timer = trapperButton.MaxTimer;
                },
                () => { return Trapper.trapper != null && Trapper.trapper == CachedPlayer.LocalPlayer.PlayerControl && !CachedPlayer.LocalPlayer.Data.IsDead; },
                () => {
                    if (trapperChargesText != null) trapperChargesText.text = $"{Trapper.charges} / {Trapper.maxCharges}";
                    return CachedPlayer.LocalPlayer.PlayerControl.CanMove && Trapper.charges > 0;
                },
                () => { trapperButton.Timer = trapperButton.MaxTimer; },
                Trapper.getButtonSprite(),
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                "ActionQuaternary"
            );

            // Bomber button
            bomberButton = new CustomButton(
                () => {
                    if (Helpers.checkMurderAttempt(Bomber.bomber, Bomber.bomber) != MurderAttemptResult.BlankKill) {
                        var pos = CachedPlayer.LocalPlayer.transform.position;
                        byte[] buff = new byte[sizeof(float) * 2];
                        Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, buff, 0 * sizeof(float), sizeof(float));
                        Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, buff, 1 * sizeof(float), sizeof(float));

                        MessageWriter writer = AmongUsClient.Instance.StartRpc(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.PlaceBomb, Hazel.SendOption.Reliable);
                        writer.WriteBytesAndSize(buff);
                        writer.EndMessage();
                        RPCProcedure.placeBomb(buff);

                        SoundEffectsManager.play("trapperTrap");
                    }

                    bomberButton.Timer = bomberButton.MaxTimer;
                    Bomber.isPlanted = true;
                },
                () => { return Bomber.bomber != null && Bomber.bomber == CachedPlayer.LocalPlayer.PlayerControl && !CachedPlayer.LocalPlayer.Data.IsDead; },
                () => { return CachedPlayer.LocalPlayer.PlayerControl.CanMove && !Bomber.isPlanted; },
                () => { bomberButton.Timer = bomberButton.MaxTimer; },
                Bomber.getButtonSprite(),
                CustomButton.ButtonPositions.upperRowLeft,
                __instance,
                "ActionQuaternary",
                true,
                Bomber.destructionTime,
                () => {
                    bomberButton.Timer = bomberButton.MaxTimer;
                    bomberButton.isEffectActive = false;
                    bomberButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                }
            );

            defuseButton = new CustomButton(
                () => {
                    defuseButton.HasEffect = true;
                },
                () => {
                    if (shifterShiftButton.HasButton())
                        defuseButton.PositionOffset = new Vector3(0f, 2f, 0f);
                    else
                        defuseButton.PositionOffset = new Vector3(0f, 1f, 0f);
                    return Bomber.bomb != null && Bomb.canDefuse && !CachedPlayer.LocalPlayer.Data.IsDead; },
                () => {
                    if (defuseButton.isEffectActive && !Bomb.canDefuse) {
                        defuseButton.Timer = 0f;
                        defuseButton.isEffectActive = false;
                    }
                    return CachedPlayer.LocalPlayer.PlayerControl.CanMove; 
                },
                () => {
                    defuseButton.Timer = 0f;
                    defuseButton.isEffectActive = false;
                },
                Bomb.getDefuseSprite(),
                new Vector3(0f, 1f, 0),
                __instance,
                "defuseBomb",
                true,
                Bomber.defuseDuration,
                () => {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.DefuseBomb, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.defuseBomb();

                    defuseButton.Timer = 0f;
                    Bomb.canDefuse = false;
                },
                true
            );

            thiefKillButton = new CustomButton(
                () => {
                    PlayerControl thief = Thief.thief;
                    PlayerControl target = Thief.currentTarget;
                    var result = Helpers.checkMurderAttempt(thief, target);
                    if (result == MurderAttemptResult.BlankKill) {
                        thiefKillButton.Timer = thiefKillButton.MaxTimer;
                        return;
                    }

                    if (Thief.suicideFlag) {
                        // Suicide
                        MessageWriter writer2 = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.UncheckedMurderPlayer, Hazel.SendOption.Reliable, -1);
                        writer2.Write(thief.PlayerId);
                        writer2.Write(thief.PlayerId);
                        writer2.Write(0);
                        RPCProcedure.uncheckedMurderPlayer(thief.PlayerId, thief.PlayerId, 0);
                        AmongUsClient.Instance.FinishRpcImmediately(writer2);
                        Thief.thief.clearAllTasks();
                    }

                    // Steal role if survived.
                    if (!Thief.thief.Data.IsDead && result == MurderAttemptResult.PerformKill) {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.ThiefStealsRole, Hazel.SendOption.Reliable, -1);
                        writer.Write(target.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.thiefStealsRole(target.PlayerId);
                    }
                    // Kill the victim (after becoming their role - so that no win is triggered for other teams)
                    if (result == MurderAttemptResult.PerformKill) {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.UncheckedMurderPlayer, Hazel.SendOption.Reliable, -1);
                        writer.Write(thief.PlayerId);
                        writer.Write(target.PlayerId);
                        writer.Write(byte.MaxValue);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.uncheckedMurderPlayer(thief.PlayerId, target.PlayerId, byte.MaxValue);
                    }
                },
               () => { return Thief.thief != null && CachedPlayer.LocalPlayer.PlayerControl == Thief.thief && !CachedPlayer.LocalPlayer.Data.IsDead; },
               () => { return Thief.currentTarget != null && CachedPlayer.LocalPlayer.PlayerControl.CanMove; },
               () => { thiefKillButton.Timer = thiefKillButton.MaxTimer; },
               __instance.KillButton.graphic.sprite,
               CustomButton.ButtonPositions.upperRowRight,
               __instance,
               "ActionSecondary"
               );

            // Trapper Charges
            trapperChargesText = GameObject.Instantiate(trapperButton.actionButton.cooldownTimerText, trapperButton.actionButton.cooldownTimerText.transform.parent);
            trapperChargesText.text = "";
            trapperChargesText.enableWordWrapping = false;
            trapperChargesText.transform.localScale = Vector3.one * 0.5f;
            trapperChargesText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            zoomOutButton = new CustomButton(
                () => { Helpers.toggleZoom();
                },
                () => { if (CachedPlayer.LocalPlayer.PlayerControl == null || !CachedPlayer.LocalPlayer.Data.IsDead || CachedPlayer.LocalPlayer.Data.Role.IsImpostor) return false;
                    var (playerCompleted, playerTotal) = TasksHandler.taskInfo(CachedPlayer.LocalPlayer.Data);
                    int numberOfLeftTasks = playerTotal - playerCompleted;
                    return numberOfLeftTasks <= 0 || !CustomOptionHolder.finishTasksBeforeHauntingOrZoomingOut.getBool();
                },
                () => { return true; },
                () => { return; },
                Helpers.loadSpriteFromResources("TheOtherRoles.Resources.MinusButton.png", 150f),  // Invisible button!
                new Vector3(0.4f, 2.8f, 0),
                __instance,
                "ZoomOut"
                );
            zoomOutButton.Timer = 0f;


            hunterLighterButton = new CustomButton(
                () => {
                    Hunter.lightActive.Add(CachedPlayer.LocalPlayer.PlayerId);
                    SoundEffectsManager.play("lighterLight");

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.ShareTimer, Hazel.SendOption.Reliable, -1);
                    writer.Write(Hunter.lightPunish);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.shareTimer(Hunter.lightPunish);
                },
                () => { return HideNSeek.isHunter() && !CachedPlayer.LocalPlayer.Data.IsDead; },
                () => { return true; },
                () => {
                    hunterLighterButton.Timer = 30f;
                    hunterLighterButton.isEffectActive = false;
                    hunterLighterButton.actionButton.graphic.color = Palette.EnabledColor;
                },
                Hunter.getLightSprite(),
                CustomButton.ButtonPositions.upperRowFarLeft,
                __instance,
                "ActionQuaternary",
                true,
                Hunter.lightDuration,
                () => {
                    Hunter.lightActive.Remove(CachedPlayer.LocalPlayer.PlayerId);
                    hunterLighterButton.Timer = hunterLighterButton.MaxTimer;
                    SoundEffectsManager.play("lighterLight");
                }
            );

            hunterAdminTableButton = new CustomButton(
               () => {
                   if (!MapBehaviour.Instance || !MapBehaviour.Instance.isActiveAndEnabled) {
                       HudManager __instance = FastDestroyableSingleton<HudManager>.Instance;
                       __instance.InitMap();
                       MapBehaviour.Instance.ShowCountOverlay(allowedToMove: true, showLivePlayerPosition: true, includeDeadBodies: false);
                   }

                   CachedPlayer.LocalPlayer.NetTransform.Halt(); // Stop current movement 

                   MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.ShareTimer, Hazel.SendOption.Reliable, -1);
                   writer.Write(Hunter.AdminPunish); 
                   AmongUsClient.Instance.FinishRpcImmediately(writer);
                   RPCProcedure.shareTimer(Hunter.AdminPunish);
               },
               () => { return HideNSeek.isHunter() && !CachedPlayer.LocalPlayer.Data.IsDead; },
               () => { return true; },
               () => {
                   hunterAdminTableButton.Timer = hunterAdminTableButton.MaxTimer;
                   hunterAdminTableButton.isEffectActive = false;
                   hunterAdminTableButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
               },
               Hacker.getAdminSprite(),
               CustomButton.ButtonPositions.lowerRowCenter,
               __instance,
               "HunterAdmin",
               true,
               Hunter.AdminDuration,
               () => {
                   hunterAdminTableButton.Timer = hunterAdminTableButton.MaxTimer;
                   if (MapBehaviour.Instance && MapBehaviour.Instance.isActiveAndEnabled) MapBehaviour.Instance.Close();
               },
               false,
               "ADMIN"
            );

            hunterArrowButton = new CustomButton(
                () => {
                    Hunter.arrowActive = true;
                    SoundEffectsManager.play("trackerTrackPlayer");

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.ShareTimer, Hazel.SendOption.Reliable, -1);
                    writer.Write(Hunter.ArrowPunish);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.shareTimer(Hunter.ArrowPunish);
                },
                () => { return HideNSeek.isHunter() && !CachedPlayer.LocalPlayer.Data.IsDead; },
                () => { return true; },
                () => {
                    hunterArrowButton.Timer = 30f;
                    hunterArrowButton.isEffectActive = false;
                    hunterArrowButton.actionButton.graphic.color = Palette.EnabledColor;
                },
                Hunter.getArrowSprite(),
                CustomButton.ButtonPositions.upperRowLeft,
                __instance,
                "HunterArrow",
                true,
                Hunter.ArrowDuration,
                () => {
                    Hunter.arrowActive = false;
                    hunterArrowButton.Timer = hunterArrowButton.MaxTimer;
                    SoundEffectsManager.play("trackerTrackPlayer");
                }
            );

            huntedShieldButton = new CustomButton(
                () => {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.HuntedShield, Hazel.SendOption.Reliable, -1);
                    writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.huntedShield(CachedPlayer.LocalPlayer.PlayerId);
                    SoundEffectsManager.play("timemasterShield");

                    Hunted.shieldCount--;
                },
                () => { return HideNSeek.isHunted() && !CachedPlayer.LocalPlayer.Data.IsDead; },
                () => {
                    if (huntedShieldCountText != null) huntedShieldCountText.text = $"{Hunted.shieldCount}";
                    return CachedPlayer.LocalPlayer.PlayerControl.CanMove && Hunted.shieldCount > 0;
                },
                () => {
                    huntedShieldButton.Timer = huntedShieldButton.MaxTimer;
                    huntedShieldButton.isEffectActive = false;
                    huntedShieldButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                TimeMaster.getButtonSprite(),
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                "ActionQuaternary",
                true,
                Hunted.shieldDuration,
                () => {
                    huntedShieldButton.Timer = huntedShieldButton.MaxTimer;
                    SoundEffectsManager.stop("timemasterShield");

                }
            );

            huntedShieldCountText = GameObject.Instantiate(huntedShieldButton.actionButton.cooldownTimerText, huntedShieldButton.actionButton.cooldownTimerText.transform.parent);
            huntedShieldCountText.text = "";
            huntedShieldCountText.enableWordWrapping = false;
            huntedShieldCountText.transform.localScale = Vector3.one * 0.5f;
            huntedShieldCountText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // Set the default (or settings from the previous game) timers / durations when spawning the buttons
            initialized = true;
            setCustomButtonCooldowns();
            deputyHandcuffedButtons = new Dictionary<byte, List<CustomButton>>();
            
        }
    }
}
