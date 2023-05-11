using HarmonyLib;
using System;
using UnityEngine;
using TheOtherRoles.Objects;
using System.Linq;
using System.Collections.Generic;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;
using TheOtherRoles.CustomGameModes;
using TheOtherRoles.EnoFw;
using TheOtherRoles.EnoFw.Kernel;
using TheOtherRoles.EnoFw.Modules;
using TheOtherRoles.EnoFw.Roles.Crewmate;
using TheOtherRoles.EnoFw.Roles.Neutral;
using TheOtherRoles.EnoFw.Roles.Impostor;
using TheOtherRoles.EnoFw.Roles.Modifiers;


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
        private static CustomButton shifterShiftButton;
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
        public static CustomButton whispererKillButton;
        public static CustomButton undertakerDragButton;
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

        public static void setCustomButtonCooldowns()
        {
            if (!initialized)
            {
                try
                {
                    createButtonsPostfix(HudManager.Instance);
                }
                catch
                {
                    TheOtherRolesPlugin.Logger.LogWarning(
                        "Button cooldowns not set, either the gamemode does not require them or there's something wrong.");
                    return;
                }
            }

            engineerRepairButton.MaxTimer = 0f;
            janitorCleanButton.MaxTimer = Janitor.Instance.CleanCooldown;
            sheriffKillButton.MaxTimer = Sheriff.Instance.Cooldown;
            deputyHandcuffButton.MaxTimer = Deputy.Instance.HandcuffCooldown;
            timeMasterShieldButton.MaxTimer = TimeMaster.Instance.TimeShieldCooldown;
            medicShieldButton.MaxTimer = 0f;
            shifterShiftButton.MaxTimer = 0f;
            morphlingButton.MaxTimer = Morphling.Instance.MorphCooldown;
            camouflagerButton.MaxTimer = Camouflager.Instance.CamoCooldown;
            portalmakerPlacePortalButton.MaxTimer = Portalmaker.Instance.PortalCooldown;
            usePortalButton.MaxTimer = Portalmaker.Instance.UsePortalCooldown;
            portalmakerMoveToPortalButton.MaxTimer = Portalmaker.Instance.UsePortalCooldown;
            hackerButton.MaxTimer = Hacker.Instance.HackCooldown;
            hackerVitalsButton.MaxTimer = Hacker.Instance.HackCooldown;
            hackerAdminTableButton.MaxTimer = Hacker.Instance.HackCooldown;
            vampireKillButton.MaxTimer = Vampire.Instance.BiteCooldown;
            whispererKillButton.MaxTimer = Whisperer.Instance.WhisperCooldown;
            undertakerDragButton.MaxTimer = Undertaker.Instance.DragCooldown;
            trackerTrackPlayerButton.MaxTimer = 0f;
            garlicButton.MaxTimer = 0f;
            jackalKillButton.MaxTimer = Jackal.Instance.KillCooldown;
            sidekickKillButton.MaxTimer = Jackal.Instance.KillCooldown;
            jackalSidekickButton.MaxTimer = Jackal.Instance.CreateSidekickCooldown;
            eraserButton.MaxTimer = Eraser.Instance.EraseCooldown;
            placeJackInTheBoxButton.MaxTimer = Trickster.Instance.PlaceBoxCooldown;
            lightsOutButton.MaxTimer = Trickster.Instance.LightsOutCooldown;
            cleanerCleanButton.MaxTimer = Cleaner.Instance.CleanCooldown;
            warlockCurseButton.MaxTimer = Warlock.Instance.CurseCooldown;
            securityGuardButton.MaxTimer = SecurityGuard.Instance.Cooldown;
            securityGuardCamButton.MaxTimer = SecurityGuard.Instance.Cooldown;
            arsonistButton.MaxTimer = Arsonist.Instance.DouseCooldown;
            vultureEatButton.MaxTimer = Vulture.Instance.EatCooldown;
            mediumButton.MaxTimer = Medium.Instance.QuestionCooldown;
            pursuerButton.MaxTimer = Pursuer.Instance.BlankCooldown;
            trackerTrackCorpsesButton.MaxTimer = Tracker.Instance.CorpsesTrackingCooldown;
            witchSpellButton.MaxTimer = Witch.Instance.SpellCooldown;
            ninjaButton.MaxTimer = Ninja.Instance.NinjaCooldown;
            thiefKillButton.MaxTimer = Thief.Instance.KillCooldown;
            mayorMeetingButton.MaxTimer = GameManager.Instance.LogicOptions.GetEmergencyCooldown();
            trapperButton.MaxTimer = Trapper.Instance.PlaceTrapCooldown;
            bomberButton.MaxTimer = Bomber.Instance.BombCooldown;
            hunterLighterButton.MaxTimer = Hunter.lightCooldown;
            hunterAdminTableButton.MaxTimer = Hunter.AdminCooldown;
            hunterArrowButton.MaxTimer = Hunter.ArrowCooldown;
            huntedShieldButton.MaxTimer = Hunted.shieldCooldown;
            defuseButton.MaxTimer = 0f;
            defuseButton.Timer = 0f;

            timeMasterShieldButton.EffectDuration = TimeMaster.Instance.ShieldDuration;
            hackerButton.EffectDuration = Hacker.Instance.HackingDuration;
            hackerVitalsButton.EffectDuration = Hacker.Instance.HackingDuration;
            hackerAdminTableButton.EffectDuration = Hacker.Instance.HackingDuration;
            vampireKillButton.EffectDuration = Vampire.Instance.BiteDelay;
            whispererKillButton.EffectDuration = Whisperer.Instance.WhisperDelay;
            camouflagerButton.EffectDuration = Camouflager.Instance.CamoDuration;
            morphlingButton.EffectDuration = Morphling.Instance.MorphDuration;
            lightsOutButton.EffectDuration = Trickster.Instance.LightsOutDuration;
            arsonistButton.EffectDuration = Arsonist.Instance.DouseDuration;
            mediumButton.EffectDuration = Medium.Instance.QuestionDuration;
            trackerTrackCorpsesButton.EffectDuration = Tracker.Instance.CorpsesTrackingDuration;
            witchSpellButton.EffectDuration = Witch.Instance.SpellCastingDuration;
            securityGuardCamButton.EffectDuration = SecurityGuard.Instance.CamDuration;
            hunterLighterButton.EffectDuration = Hunter.lightDuration;
            hunterArrowButton.EffectDuration = Hunter.ArrowDuration;
            huntedShieldButton.EffectDuration = Hunted.shieldDuration;
            defuseButton.EffectDuration = Bomber.Instance.BombDefuseDuration;
            bomberButton.EffectDuration = (float)Bomber.Instance.BombDestructionTime + (float)Bomber.Instance.BombActivationTime;
            // Already set the timer to the max, as the button is enabled during the game and not available at the start
            lightsOutButton.Timer = lightsOutButton.MaxTimer;
            zoomOutButton.MaxTimer = 0f;
        }

        public static void resetTimeMasterButton()
        {
            timeMasterShieldButton.Timer = timeMasterShieldButton.MaxTimer;
            timeMasterShieldButton.isEffectActive = false;
            timeMasterShieldButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
            SoundEffectsManager.stop("timemasterShield");
        }

        public static void resetHuntedRewindButton()
        {
            huntedShieldButton.Timer = huntedShieldButton.MaxTimer;
            huntedShieldButton.isEffectActive = false;
            huntedShieldButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
            SoundEffectsManager.stop("timemasterShield");
        }

        private static void addReplacementHandcuffedButton(CustomButton button, Vector3? positionOffset = null,
            Func<bool> couldUse = null)
        {
            Vector3 positionOffsetValue =
                positionOffset ?? button.PositionOffset; // For non custom buttons, we can set these manually.
            positionOffsetValue.z = -0.1f;
            couldUse = couldUse ?? button.CouldUse;
            CustomButton replacementHandcuffedButton = new CustomButton(() => { }, () => { return true; }, couldUse,
                () => { }, Deputy.HandcuffedButtonSprite, positionOffsetValue, button.hudManager, button.actionName,
                true, Deputy.Instance.HandcuffDuration, () => { }, button.mirror);
            replacementHandcuffedButton.Timer = replacementHandcuffedButton.EffectDuration;
            replacementHandcuffedButton.actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
            replacementHandcuffedButton.isEffectActive = true;
            if (deputyHandcuffedButtons.ContainsKey(CachedPlayer.LocalPlayer.PlayerId))
                deputyHandcuffedButtons[CachedPlayer.LocalPlayer.PlayerId].Add(replacementHandcuffedButton);
            else
                deputyHandcuffedButtons.Add(CachedPlayer.LocalPlayer.PlayerId,
                    new List<CustomButton> { replacementHandcuffedButton });
        }

        // Disables / Enables all Buttons (except the ones disabled in the Deputy class), and replaces them with new buttons.
        public static void setAllButtonsHandcuffedStatus(bool handcuffed, bool reset = false)
        {
            if (reset)
            {
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
                        if (CustomButton.buttons[i].HasButton()) // For each custombutton the player has
                        {
                            addReplacementHandcuffedButton(CustomButton
                                .buttons[i]); // The new buttons are the only non-handcuffed buttons now!
                        }

                        CustomButton.buttons[i].isHandcuffed = true;
                    }
                    catch (NullReferenceException)
                    {
                        System.Console.WriteLine(
                            "[WARNING] NullReferenceException from MeetingEndedUpdate().HasButton(), if theres only one warning its fine"); // Note: idk what this is good for, but i copied it from above /gendelo
                    }
                }

                // Non Custom (Vanilla) Buttons. The Originals are disabled / hidden in UpdatePatch.cs already, just need to replace them. Can use any button, as we replace onclick etc anyways.
                // Kill Button if enabled for the Role
                if (FastDestroyableSingleton<HudManager>.Instance.KillButton.isActiveAndEnabled)
                    addReplacementHandcuffedButton(arsonistButton, CustomButton.ButtonPositions.upperRowRight,
                        couldUse: () =>
                        {
                            return FastDestroyableSingleton<HudManager>.Instance.KillButton.currentTarget != null;
                        });
                // Vent Button if enabled
                if (CachedPlayer.LocalPlayer.PlayerControl.roleCanUseVents())
                    addReplacementHandcuffedButton(arsonistButton, CustomButton.ButtonPositions.upperRowCenter,
                        couldUse: () =>
                        {
                            return FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.currentTarget !=
                                   null;
                        });
                // Report Button
                addReplacementHandcuffedButton(arsonistButton,
                    (!CachedPlayer.LocalPlayer.Data.Role.IsImpostor)
                        ? new Vector3(-1f, -0.06f, 0)
                        : CustomButton.ButtonPositions.lowerRowRight,
                    () =>
                    {
                        return FastDestroyableSingleton<HudManager>.Instance.ReportButton.graphic.color ==
                               Palette.EnabledColor;
                    });
            }
            else if (!handcuffed &&
                     deputyHandcuffedButtons.ContainsKey(CachedPlayer.LocalPlayer
                         .PlayerId)) // Reset to original. Disables the replacements, enables the original buttons.
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

        private static void setButtonTargetDisplay(PlayerControl target, CustomButton button = null,
            Vector3? offset = null)
        {
            if (target == null || button == null)
            {
                if (targetDisplay != null)
                {
                    // Reset the poolable player
                    targetDisplay.gameObject.SetActive(false);
                    GameObject.Destroy(targetDisplay.gameObject);
                    targetDisplay = null;
                }

                return;
            }

            // Add poolable player to the button so that the target outfit is shown
            button.actionButton.cooldownTimerText.transform.localPosition =
                new Vector3(0, 0, -1f); // Before the poolable player
            targetDisplay =
                UnityEngine.Object.Instantiate<PoolablePlayer>(Patches.IntroCutsceneOnDestroyPatch.playerPrefab,
                    button.actionButton.transform);
            GameData.PlayerInfo data = target.Data;
            target.SetPlayerMaterialColors(targetDisplay.cosmetics.currentBodySprite.BodySprite);
            targetDisplay.SetSkin(data.DefaultOutfit.SkinId, data.DefaultOutfit.ColorId);
            targetDisplay.SetHat(data.DefaultOutfit.HatId, data.DefaultOutfit.ColorId);
            targetDisplay.cosmetics.nameText.text = ""; // Hide the name!
            targetDisplay.transform.localPosition = new Vector3(0f, 0.22f, -0.01f);
            if (offset != null) targetDisplay.transform.localPosition += (Vector3)offset;
            targetDisplay.transform.localScale = Vector3.one * 0.33f;
            targetDisplay.setSemiTransparent(false);
            targetDisplay.gameObject.SetActive(true);
        }

        public static void Postfix(HudManager __instance)
        {
            initialized = false;

            try
            {
                createButtonsPostfix(__instance);
            }
            catch
            {
            }
        }

        public static void createButtonsPostfix(HudManager __instance)
        {
            // get map id, or raise error to wait...
            var mapId = GameOptionsManager.Instance.currentNormalGameOptions.MapId;

            // Engineer Repair
            engineerRepairButton = new CustomButton(
                () =>
                {
                    engineerRepairButton.Timer = 0f;
                    Engineer.EngineerUsedRepair();
                    SoundEffectsManager.play("engineerRepair");
                    foreach (PlayerTask task in CachedPlayer.LocalPlayer.PlayerControl.myTasks.GetFastEnumerator())
                    {
                        if (task.TaskType == TaskTypes.FixLights)
                        {
                            Engineer.EngineerFixLights();
                        }
                        else if (task.TaskType == TaskTypes.RestoreOxy)
                        {
                            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.LifeSupp, 0 | 64);
                            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.LifeSupp, 1 | 64);
                        }
                        else if (task.TaskType == TaskTypes.ResetReactor)
                        {
                            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Reactor, 16);
                        }
                        else if (task.TaskType == TaskTypes.ResetSeismic)
                        {
                            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Laboratory, 16);
                        }
                        else if (task.TaskType == TaskTypes.FixComms)
                        {
                            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Comms, 16 | 0);
                            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Comms, 16 | 1);
                        }
                        else if (task.TaskType == TaskTypes.StopCharles)
                        {
                            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Reactor, 0 | 16);
                            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Reactor, 1 | 16);
                        }
                        else if (SubmergedCompatibility.IsSubmerged &&
                                 task.TaskType == SubmergedCompatibility.RetrieveOxygenMask)
                        {
                            Engineer.EngineerFixSubmergedOxygen();
                        }
                    }
                },
                () =>
                {
                    return Engineer.Instance.IsLocalPlayer && Engineer.Instance.RemainingFixes > 0 &&
                           !CachedPlayer.LocalPlayer.Data.IsDead;
                },
                () =>
                {
                    bool sabotageActive = false;
                    foreach (PlayerTask task in CachedPlayer.LocalPlayer.PlayerControl.myTasks.GetFastEnumerator())
                        if (task.TaskType == TaskTypes.FixLights || task.TaskType == TaskTypes.RestoreOxy ||
                            task.TaskType == TaskTypes.ResetReactor || task.TaskType == TaskTypes.ResetSeismic ||
                            task.TaskType == TaskTypes.FixComms || task.TaskType == TaskTypes.StopCharles
                            || SubmergedCompatibility.IsSubmerged &&
                            task.TaskType == SubmergedCompatibility.RetrieveOxygenMask)
                            sabotageActive = true;
                    return sabotageActive && Engineer.Instance.RemainingFixes > 0 &&
                           CachedPlayer.LocalPlayer.PlayerControl.CanMove;
                },
                () => { },
                Engineer.RepairButtonSprite,
                CustomButton.ButtonPositions.upperRowRight,
                __instance,
                "ActionQuaternary"
            );

            // Janitor Clean
            janitorCleanButton = new CustomButton(
                () =>
                {
                    foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(
                                 CachedPlayer.LocalPlayer.PlayerControl.GetTruePosition(),
                                 CachedPlayer.LocalPlayer.PlayerControl.MaxReportDistance, Constants.PlayersOnlyMask))
                    {
                        if (collider2D.tag != "DeadBody") continue;
                        var component = collider2D.GetComponent<DeadBody>();
                        if (!component || component.Reported) continue;
                        var truePosition = CachedPlayer.LocalPlayer.PlayerControl.GetTruePosition();
                        var truePosition2 = component.TruePosition;
                        if (Vector2.Distance(truePosition2, truePosition) <=
                            CachedPlayer.LocalPlayer.PlayerControl.MaxReportDistance &&
                            CachedPlayer.LocalPlayer.PlayerControl.CanMove &&
                            !PhysicsHelpers.AnythingBetween(truePosition, truePosition2, Constants.ShipAndObjectsMask,
                                false))
                        {
                            var playerInfo = GameData.Instance.GetPlayerById(component.ParentId);
                            CommonRpc.CleanBody(playerInfo.PlayerId, Janitor.Instance.Player.PlayerId);
                            janitorCleanButton.Timer = janitorCleanButton.MaxTimer;
                            SoundEffectsManager.play("cleanerClean");

                            break;
                        }
                    }
                },
                () =>
                {
                    return Janitor.Instance.Player != null && Janitor.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl &&
                           !CachedPlayer.LocalPlayer.Data.IsDead;
                },
                () =>
                {
                    return __instance.ReportButton.graphic.color == Palette.EnabledColor &&
                           CachedPlayer.LocalPlayer.PlayerControl.CanMove;
                },
                () => { janitorCleanButton.Timer = janitorCleanButton.MaxTimer; },
                Janitor.CleanButtonSprite,
                CustomButton.ButtonPositions.upperRowLeft,
                __instance,
                "ActionQuaternary"
            );

            // Sheriff Kill
            sheriffKillButton = new CustomButton(
                () =>
                {
                    MurderAttemptResult murderAttemptResult =
                        Helpers.checkMurderAttempt(Sheriff.Instance.Player, Sheriff.Instance.CurrentTarget);
                    if (murderAttemptResult == MurderAttemptResult.SuppressKill) return;

                    if (murderAttemptResult == MurderAttemptResult.PerformKill)
                    {
                        byte targetId = 0;
                        if ((Sheriff.Instance.CurrentTarget.Data.Role.IsImpostor &&
                             (Sheriff.Instance.CurrentTarget != Mini.Instance.Player || Mini.Instance.IsGrownUp)) ||
                            (Sheriff.Instance.SpyCanDieToSheriff && Spy.Instance.Player == Sheriff.Instance.CurrentTarget) ||
                            (Sheriff.Instance.CanKillNeutrals && Helpers.isNeutral(Sheriff.Instance.CurrentTarget)) ||
                            (Jackal.Instance.Player == Sheriff.Instance.CurrentTarget ||
                             Sidekick.Instance.Player == Sheriff.Instance.CurrentTarget))
                        {
                            targetId = Sheriff.Instance.CurrentTarget.PlayerId;
                        }
                        else
                        {
                            targetId = CachedPlayer.LocalPlayer.PlayerId;
                        }

                        KernelRpc.UncheckedMurderPlayer(Sheriff.Instance.Player.PlayerId, targetId, true);
                    }

                    sheriffKillButton.Timer = sheriffKillButton.MaxTimer;
                    Sheriff.Instance.CurrentTarget = null;
                },
                () => { return Sheriff.Instance.IsLocalPlayer && !CachedPlayer.LocalPlayer.Data.IsDead; },
                () => { return Sheriff.Instance.CurrentTarget && CachedPlayer.LocalPlayer.PlayerControl.CanMove; },
                () => { sheriffKillButton.Timer = sheriffKillButton.MaxTimer; },
                __instance.KillButton.graphic.sprite,
                CustomButton.ButtonPositions.upperRowRight,
                __instance,
                "ActionSecondary"
            );

            // Deputy Handcuff
            deputyHandcuffButton = new CustomButton(
                () =>
                {
                    byte targetId = 0;
                    targetId = Sheriff.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl
                        ? Sheriff.Instance.CurrentTarget.PlayerId
                        : Deputy.Instance.CurrentTarget
                            .PlayerId; // If the deputy is now the sheriff, sheriffs target, else deputies target
                    Deputy.DeputyUsedHandcuffs(targetId);
                    Deputy.Instance.CurrentTarget = null;
                    deputyHandcuffButton.Timer = deputyHandcuffButton.MaxTimer;
                    SoundEffectsManager.play("deputyHandcuff");
                },
                () =>
                {
                    return (Deputy.Instance.IsLocalPlayer || Sheriff.Instance.IsLocalPlayer &&
                        Sheriff.Instance.Player == Sheriff.Instance.FormerDeputy &&
                        Deputy.Instance.KeepsHandcuffsOnPromotion) && !CachedPlayer.LocalPlayer.Data.IsDead;
                },
                () =>
                {
                    if (deputyButtonHandcuffsText != null)
                        deputyButtonHandcuffsText.text = $"{Deputy.Instance.RemainingHandcuffs}";
                    return ((Deputy.Instance.IsLocalPlayer && Deputy.Instance.CurrentTarget ||
                             Sheriff.Instance.IsLocalPlayer &&
                             Sheriff.Instance.Player == Sheriff.Instance.FormerDeputy &&
                             Sheriff.Instance.CurrentTarget != null) && Deputy.Instance.RemainingHandcuffs > 0 &&
                            CachedPlayer.LocalPlayer.PlayerControl.CanMove);
                },
                () => { deputyHandcuffButton.Timer = deputyHandcuffButton.MaxTimer; },
                Deputy.HandcuffButtonSprite,
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                "ActionQuaternary"
            );
            // Deputy Handcuff button handcuff counter
            deputyButtonHandcuffsText = GameObject.Instantiate(deputyHandcuffButton.actionButton.cooldownTimerText,
                deputyHandcuffButton.actionButton.cooldownTimerText.transform.parent);
            deputyButtonHandcuffsText.text = "";
            deputyButtonHandcuffsText.enableWordWrapping = false;
            deputyButtonHandcuffsText.transform.localScale = Vector3.one * 0.5f;
            deputyButtonHandcuffsText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // Time Master Rewind Time
            timeMasterShieldButton = new CustomButton(
                () =>
                {
                    TimeMaster.TimeMasterShield();
                    SoundEffectsManager.play("timemasterShield");
                },
                () =>
                {
                    return TimeMaster.Instance.Player != null &&
                           TimeMaster.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl &&
                           !CachedPlayer.LocalPlayer.Data.IsDead;
                },
                () => { return CachedPlayer.LocalPlayer.PlayerControl.CanMove; },
                () =>
                {
                    timeMasterShieldButton.Timer = timeMasterShieldButton.MaxTimer;
                    timeMasterShieldButton.isEffectActive = false;
                    timeMasterShieldButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                TimeMaster.TimeShieldButtonSprite,
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                "ActionQuaternary",
                true,
                TimeMaster.Instance.ShieldDuration,
                () =>
                {
                    timeMasterShieldButton.Timer = timeMasterShieldButton.MaxTimer;
                    SoundEffectsManager.stop("timemasterShield");
                }
            );

            // Medic Shield
            medicShieldButton = new CustomButton(
                () =>
                {
                    medicShieldButton.Timer = 0f;
                    if (Medic.Instance.SetShieldAfterMeeting)
                        Medic.SetFutureShielded(Medic.Instance.CurrentTarget.PlayerId);
                    else
                        Medic.MedicSetShielded(Medic.Instance.CurrentTarget.PlayerId);
                    Medic.Instance.MeetingAfterShielding = false;
                    SoundEffectsManager.play("medicShield");
                },
                () => { return Medic.Instance.IsLocalPlayer && !CachedPlayer.LocalPlayer.Data.IsDead; },
                () =>
                {
                    return !Medic.Instance.UsedShield && Medic.Instance.CurrentTarget &&
                           CachedPlayer.LocalPlayer.PlayerControl.CanMove;
                },
                () => { },
                Medic.ShieldButtonSprite,
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                "ActionQuaternary"
            );

            TORMapOptions.ShieldExpireButton = new CustomButton(
                () => { },
                () => TORMapOptions.firstKillPlayer != null &&
                      TORMapOptions.firstKillPlayer == CachedPlayer.LocalPlayer.PlayerControl &&
                      !TORMapOptions.removeShieldOnFirstMeeting &&
                      CustomOptions.ShowShieldIndicator,
                () => true,
                () => { },
                TORMapOptions.GetShieldSprite(),
                new Vector3(0, 1f, 0),
                __instance,
                null,
                mirror: true
            );


            // Shifter shift
            shifterShiftButton = new CustomButton(
                () =>
                {
                    Shifter.SetFutureShifted(Shifter.Instance.CurrentTarget.PlayerId);
                    SoundEffectsManager.play("shifterShift");
                },
                () =>
                {
                    return Shifter.Instance.Player != null && Shifter.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl &&
                           Shifter.Instance.FutureShift == null && !CachedPlayer.LocalPlayer.Data.IsDead;
                },
                () =>
                {
                    return Shifter.Instance.CurrentTarget && Shifter.Instance.FutureShift == null &&
                           CachedPlayer.LocalPlayer.PlayerControl.CanMove;
                },
                () => { },
                Shifter.ShiftButtonSprite,
                new Vector3(0, 1f, 0),
                __instance,
                null,
                true
            );

            // Morphling morph

            morphlingButton = new CustomButton(
                () =>
                {
                    if (Morphling.Instance.SampledTarget != null)
                    {
                        Morphling.MorphlingMorph(Morphling.Instance.SampledTarget.PlayerId);
                        Morphling.Instance.SampledTarget = null;
                        morphlingButton.EffectDuration = Morphling.Instance.MorphDuration;
                        SoundEffectsManager.play("morphlingMorph");
                    }
                    else if (Morphling.Instance.CurrentTarget != null)
                    {
                        Morphling.Instance.SampledTarget = Morphling.Instance.CurrentTarget;
                        morphlingButton.Sprite = Morphling.MorphButtonSprite;
                        morphlingButton.EffectDuration = 1f;
                        SoundEffectsManager.play("morphlingSample");

                        // Add poolable player to the button so that the target outfit is shown
                        setButtonTargetDisplay(Morphling.Instance.SampledTarget, morphlingButton);
                    }
                },
                () =>
                {
                    return Morphling.Instance.Player != null &&
                           Morphling.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl &&
                           !CachedPlayer.LocalPlayer.Data.IsDead;
                },
                () =>
                {
                    return (Morphling.Instance.CurrentTarget || Morphling.Instance.SampledTarget) &&
                           CachedPlayer.LocalPlayer.PlayerControl.CanMove;
                },
                () =>
                {
                    morphlingButton.Timer = morphlingButton.MaxTimer;
                    morphlingButton.Sprite = Morphling.SampleButtonSprite;
                    morphlingButton.isEffectActive = false;
                    morphlingButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                    Morphling.Instance.SampledTarget = null;
                    setButtonTargetDisplay(null);
                },
                Morphling.SampleButtonSprite,
                CustomButton.ButtonPositions.upperRowLeft,
                __instance,
                "ActionQuaternary",
                true,
                Morphling.Instance.MorphDuration,
                () =>
                {
                    if (Morphling.Instance.SampledTarget == null)
                    {
                        morphlingButton.Timer = morphlingButton.MaxTimer;
                        morphlingButton.Sprite = Morphling.SampleButtonSprite;
                        SoundEffectsManager.play("morphlingMorph");

                        // Reset the poolable player
                        setButtonTargetDisplay(null);
                    }
                }
            );

            // Camouflager camouflage
            camouflagerButton = new CustomButton(
                () =>
                {
                    Camouflager.CamouflagerCamouflage();
                    SoundEffectsManager.play("morphlingMorph");
                },
                () =>
                {
                    return Camouflager.Instance.Player != null &&
                           Camouflager.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl &&
                           !CachedPlayer.LocalPlayer.Data.IsDead;
                },
                () => { return CachedPlayer.LocalPlayer.PlayerControl.CanMove; },
                () =>
                {
                    camouflagerButton.Timer = camouflagerButton.MaxTimer;
                    camouflagerButton.isEffectActive = false;
                    camouflagerButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Camouflager.CamouflageButtonSprite,
                CustomButton.ButtonPositions.upperRowLeft,
                __instance,
                "ActionQuaternary",
                true,
                Camouflager.Instance.CamoDuration,
                () =>
                {
                    camouflagerButton.Timer = camouflagerButton.MaxTimer;
                    SoundEffectsManager.play("morphlingMorph");
                }
            );

            // Hacker button
            hackerButton = new CustomButton(
                () =>
                {
                    Hacker.Instance.Timer = Hacker.Instance.HackingDuration;
                    SoundEffectsManager.play("hackerHack");
                },
                () => { return Hacker.Instance.IsLocalPlayer && !CachedPlayer.LocalPlayer.Data.IsDead; },
                () => { return true; },
                () =>
                {
                    hackerButton.Timer = hackerButton.MaxTimer;
                    hackerButton.isEffectActive = false;
                    hackerButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Hacker.HackButtonSprite,
                CustomButton.ButtonPositions.upperRowRight,
                __instance,
                "ActionQuaternary",
                true,
                0f,
                () => { hackerButton.Timer = hackerButton.MaxTimer; }
            );

            hackerAdminTableButton = new CustomButton(
                () =>
                {
                    if (!MapBehaviour.Instance || !MapBehaviour.Instance.isActiveAndEnabled)
                    {
                        HudManager __instance = FastDestroyableSingleton<HudManager>.Instance;
                        __instance.InitMap();
                        MapBehaviour.Instance.ShowCountOverlay(allowedToMove: true, showLivePlayerPosition: true,
                            includeDeadBodies: true);
                    }

                    if (!Hacker.Instance.CanMoveDuringGadget) CachedPlayer.LocalPlayer.PlayerControl.moveable = false;
                    CachedPlayer.LocalPlayer.NetTransform.Halt(); // Stop current movement 
                    Hacker.Instance.AdminCharges--;
                },
                () => { return Hacker.Instance.IsLocalPlayer && !CachedPlayer.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (hackerAdminTableChargesText != null)
                        hackerAdminTableChargesText.text =
                            $"{Hacker.Instance.AdminCharges} / {Hacker.Instance.MaxGadgetCharges}";
                    return Hacker.Instance.AdminCharges > 0;
                },
                () =>
                {
                    hackerAdminTableButton.Timer = hackerAdminTableButton.MaxTimer;
                    hackerAdminTableButton.isEffectActive = false;
                    hackerAdminTableButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Hacker.AdminButtonSprite,
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                "ActionSecondary",
                true,
                0f,
                () =>
                {
                    hackerAdminTableButton.Timer = hackerAdminTableButton.MaxTimer;
                    if (!hackerVitalsButton.isEffectActive) CachedPlayer.LocalPlayer.PlayerControl.moveable = true;
                    if (MapBehaviour.Instance && MapBehaviour.Instance.isActiveAndEnabled)
                        MapBehaviour.Instance.Close();
                },
                GameOptionsManager.Instance.currentNormalGameOptions.MapId == 3,
                "ADMIN"
            );

            // Hacker Admin Table Charges
            hackerAdminTableChargesText = GameObject.Instantiate(hackerAdminTableButton.actionButton.cooldownTimerText,
                hackerAdminTableButton.actionButton.cooldownTimerText.transform.parent);
            hackerAdminTableChargesText.text = "";
            hackerAdminTableChargesText.enableWordWrapping = false;
            hackerAdminTableChargesText.transform.localScale = Vector3.one * 0.5f;
            hackerAdminTableChargesText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            hackerVitalsButton = new CustomButton(
                () =>
                {
                    if (GameOptionsManager.Instance.currentNormalGameOptions.MapId != 1)
                    {
                        if (Hacker.Instance.Vitals == null)
                        {
                            var e = UnityEngine.Object.FindObjectsOfType<SystemConsole>()
                                .FirstOrDefault(x => x.gameObject.name.Contains("panel_vitals"));
                            if (e == null || Camera.main == null) return;
                            Hacker.Instance.Vitals =
                                UnityEngine.Object.Instantiate(e.MinigamePrefab, Camera.main.transform, false);
                        }

                        Hacker.Instance.Vitals.transform.SetParent(Camera.main.transform, false);
                        Hacker.Instance.Vitals.transform.localPosition = new Vector3(0.0f, 0.0f, -50f);
                        Hacker.Instance.Vitals.Begin(null);
                    }
                    else
                    {
                        if (Hacker.Instance.DoorLog == null)
                        {
                            var e = UnityEngine.Object.FindObjectsOfType<SystemConsole>()
                                .FirstOrDefault(x => x.gameObject.name.Contains("SurvLogConsole"));
                            if (e == null || Camera.main == null) return;
                            Hacker.Instance.DoorLog =
                                UnityEngine.Object.Instantiate(e.MinigamePrefab, Camera.main.transform, false);
                        }

                        Hacker.Instance.DoorLog.transform.SetParent(Camera.main.transform, false);
                        Hacker.Instance.DoorLog.transform.localPosition = new Vector3(0.0f, 0.0f, -50f);
                        Hacker.Instance.DoorLog.Begin(null);
                    }

                    if (!Hacker.Instance.CanMoveDuringGadget) CachedPlayer.LocalPlayer.PlayerControl.moveable = false;
                    CachedPlayer.LocalPlayer.NetTransform.Halt(); // Stop current movement 

                    Hacker.Instance.VitalsCharges--;
                },
                () =>
                {
                    return Hacker.Instance.IsLocalPlayer && !CachedPlayer.LocalPlayer.Data.IsDead &&
                           GameOptionsManager.Instance.currentNormalGameOptions.MapId != 0 &&
                           GameOptionsManager.Instance.currentNormalGameOptions.MapId != 3;
                },
                () =>
                {
                    if (hackerVitalsChargesText != null)
                        hackerVitalsChargesText.text =
                            $"{Hacker.Instance.VitalsCharges} / {Hacker.Instance.MaxGadgetCharges}";
                    hackerVitalsButton.actionButton.graphic.sprite =
                        GameOptionsManager.Instance.currentNormalGameOptions.MapId == 1
                            ? Hacker.DoorLogButtonSprite
                            : Hacker.VitalsButtonSprite;
                    hackerVitalsButton.actionButton.OverrideText(
                        GameOptionsManager.Instance.currentNormalGameOptions.MapId == 1 ? "DOORLOG" : "VITALS");
                    return Hacker.Instance.VitalsCharges > 0;
                },
                () =>
                {
                    hackerVitalsButton.Timer = hackerVitalsButton.MaxTimer;
                    hackerVitalsButton.isEffectActive = false;
                    hackerVitalsButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Hacker.VitalsButtonSprite,
                CustomButton.ButtonPositions.lowerRowCenter,
                __instance,
                "ActionSecondary",
                true,
                0f,
                () =>
                {
                    hackerVitalsButton.Timer = hackerVitalsButton.MaxTimer;
                    if (!hackerAdminTableButton.isEffectActive) CachedPlayer.LocalPlayer.PlayerControl.moveable = true;
                    if (Minigame.Instance)
                    {
                        if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 1)
                            Hacker.Instance.DoorLog.ForceClose();
                        else Hacker.Instance.Vitals.ForceClose();
                    }
                },
                false,
                GameOptionsManager.Instance.currentNormalGameOptions.MapId == 1 ? "DOORLOG" : "VITALS"
            );

            // Hacker Vitals Charges
            hackerVitalsChargesText = GameObject.Instantiate(hackerVitalsButton.actionButton.cooldownTimerText,
                hackerVitalsButton.actionButton.cooldownTimerText.transform.parent);
            hackerVitalsChargesText.text = "";
            hackerVitalsChargesText.enableWordWrapping = false;
            hackerVitalsChargesText.transform.localScale = Vector3.one * 0.5f;
            hackerVitalsChargesText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // Tracker button
            trackerTrackPlayerButton = new CustomButton(
                () =>
                {
                    Tracker.TrackerUsedTracker(Tracker.Instance.CurrentTarget.PlayerId);
                    SoundEffectsManager.play("trackerTrackPlayer");
                },
                () =>
                {
                    return Tracker.Instance.IsAliveLocalPlayer;
                },
                () =>
                {
                    return CachedPlayer.LocalPlayer.PlayerControl.CanMove && Tracker.Instance.CurrentTarget != null &&
                           !Tracker.Instance.UsedTracker;
                },
                () =>
                {
                    if (Tracker.Instance.ResetTargetAfterMeeting) Tracker.Instance.ResetTracked();
                },
                Tracker.TrackButtonSprite,
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                "ActionQuaternary"
            );

            trackerTrackCorpsesButton = new CustomButton(
                () =>
                {
                    Tracker.Instance.CorpsesTrackingTimer = Tracker.Instance.CorpsesTrackingDuration;
                    SoundEffectsManager.play("trackerTrackCorpses");
                },
                () =>
                {
                    return Tracker.Instance.Player != null && Tracker.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl &&
                           !CachedPlayer.LocalPlayer.Data.IsDead && Tracker.Instance.CanTrackCorpses;
                },
                () => { return CachedPlayer.LocalPlayer.PlayerControl.CanMove; },
                () =>
                {
                    trackerTrackCorpsesButton.Timer = trackerTrackCorpsesButton.MaxTimer;
                    trackerTrackCorpsesButton.isEffectActive = false;
                    trackerTrackCorpsesButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Tracker.TrackCorpsesButtonSprite,
                CustomButton.ButtonPositions.lowerRowCenter,
                __instance,
                "ActionSecondary",
                true,
                Tracker.Instance.CorpsesTrackingDuration,
                () => { trackerTrackCorpsesButton.Timer = trackerTrackCorpsesButton.MaxTimer; }
            );

            whispererKillButton = new CustomButton(
                () =>
                {
                    MurderAttemptResult murder =
                        Helpers.checkMurderAttempt(Whisperer.Instance.Player, Whisperer.Instance.CurrentTarget);
                    if (murder == MurderAttemptResult.PerformKill)
                    {
                        Whisperer.Instance.WhisperVictim = Whisperer.Instance.CurrentTarget;

                        SoundEffectsManager.play("warlockCurse");

                        byte lastTimer = (byte)(float)Whisperer.Instance.WhisperDelay;
                        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Whisperer.Instance.WhisperDelay,
                            new Action<float>((p) =>
                            {
                                // Delayed action

                                Whisperer.Instance.WhisperVictimToKill = Whisperer.Instance.WhisperVictimTarget != null
                                    ? Whisperer.Instance.WhisperVictimTarget
                                    : Whisperer.Instance.WhisperVictim;

                                if (p <= 1f)
                                {
                                    byte timer = (byte)whispererKillButton.Timer;
                                    if (timer != lastTimer)
                                    {
                                        lastTimer = timer;
                                        GhostInfos.ShareGhostInfo(GhostInfos.Types.WhispererTimerAndTarget,
                                            Rpc.Serialize(new Tuple<byte, byte, float>(Whisperer.Instance.WhisperVictim.PlayerId,
                                                Whisperer.Instance.WhisperVictimToKill.PlayerId, timer)));
                                    }
                                }

                                if (p == 1f)
                                {
                                    // Perform kill if possible and reset bitten (regardless whether the kill was successful or not)

                                    if (Whisperer.Instance.WhisperVictimToKill != null &&
                                        Whisperer.Instance.WhisperVictimToKill != Medic.Instance.Shielded &&
                                        (!TORMapOptions.shieldFirstKill || Whisperer.Instance.WhisperVictimToKill !=
                                            TORMapOptions.firstKillPlayer))
                                        Helpers.checkMurderAttemptAndKill(Whisperer.Instance.Player,
                                            Whisperer.Instance.WhisperVictimToKill, showAnimation: false);
                                    else
                                        Helpers.checkMurderAttemptAndKill(Whisperer.Instance.Player, Whisperer.Instance.WhisperVictim,
                                            showAnimation: false);

                                    // & reset anyway.

                                    Whisperer.Instance.CurrentTarget = null;
                                    Whisperer.Instance.WhisperVictim = null;
                                    Whisperer.Instance.WhisperVictimTarget = null;
                                    Whisperer.Instance.WhisperVictimToKill = null;

                                    whispererKillButton.Timer = whispererKillButton.MaxTimer;
                                }
                            })));

                        whispererKillButton.HasEffect = true; // Trigger effect on this click
                        Whisperer.Instance.Player.killTimer =
                            GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
                    }
                    else if (murder == MurderAttemptResult.BlankKill)
                    {
                        whispererKillButton.Timer = whispererKillButton.MaxTimer;
                        Whisperer.Instance.Player.killTimer =
                            GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
                        whispererKillButton.HasEffect = false;
                    }
                    else
                    {
                        whispererKillButton.HasEffect = false;
                    }
                },
                () =>
                {
                    return Whisperer.Instance.Player != null &&
                           Whisperer.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl &&
                           !CachedPlayer.LocalPlayer.Data.IsDead;
                },
                () =>
                {
                    whispererKillButton.actionButton.graphic.sprite = Whisperer.WhisperButtonSprite;
                    whispererKillButton.showButtonText = false;

                    return Whisperer.Instance.CurrentTarget != null && Whisperer.Instance.WhisperVictim == null && CachedPlayer.LocalPlayer.PlayerControl.CanMove;
                },
                () =>
                {
                    whispererKillButton.Timer = whispererKillButton.MaxTimer;
                    whispererKillButton.isEffectActive = false;
                    whispererKillButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                    Whisperer.Instance.WhisperVictim = null;
                    Whisperer.Instance.WhisperVictimTarget = null;
                },
                Whisperer.WhisperButtonSprite,
                CustomButton.ButtonPositions.upperRowLeft,
                __instance,
                "ActionSecondary",
                false,
                0f,
                () => { whispererKillButton.Timer = whispererKillButton.MaxTimer; }
            );


            undertakerDragButton = new CustomButton(
                () =>
                {
                    var bodyComponent = Undertaker.Instance.CurrentDeadTarget;

                    if (Undertaker.Instance.DraggedBody == null && bodyComponent != null)
                    {
                        Undertaker.DragBody(bodyComponent.ParentId);
                    }
                    else if (Undertaker.Instance.DraggedBody != null)
                    {
                        var position = Undertaker.Instance.DraggedBody.transform.position;
                        Undertaker.DropBody(position.x, position.y, position.z);
                    }
                }, // Action OnClick
                () =>
                {
                    return Undertaker.Instance.Player != null &&
                           Undertaker.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl &&
                           !CachedPlayer.LocalPlayer.Data.IsDead;
                }, // Bool HasButton
                () =>
                {
                    if (Undertaker.Instance.DraggedBody != null)
                    {
                        undertakerDragButton.Sprite = Undertaker.DropButtonSprite;
                    }
                    else
                    {
                        undertakerDragButton.Sprite = Undertaker.DragButtonSprite;
                    }

                    return ((Undertaker.Instance.CurrentDeadTarget != null && Undertaker.Instance.DraggedBody == null) 
                            || (Undertaker.Instance.DraggedBody != null && Undertaker.Instance.CanDropBody)) 
                           && CachedPlayer.LocalPlayer.PlayerControl.CanMove;
                }, // Bool CouldUse
                () => { }, // Action OnMeetingEnds
                Undertaker.DragButtonSprite, // Sprite sprite,
                CustomButton.ButtonPositions.upperRowLeft, // Vector3 PositionOffset
                __instance, // HudManager hudManager
                "ActionQuaternary", // String actionName,
                false, // bool HasEffect
                0f, // Float EffectDuration
                () => { }, // Action OnEffectEnds
                false, // Bool mirror = false
                "" // String buttonText = ""
            );

            vampireKillButton = new CustomButton(
                () =>
                {
                    MurderAttemptResult murder = Helpers.checkMurderAttempt(Vampire.Instance.Player, Vampire.Instance.CurrentTarget);
                    if (murder == MurderAttemptResult.PerformKill)
                    {
                        if (Vampire.Instance.TargetNearGarlic)
                        {
                            KernelRpc.UncheckedMurderPlayer(Vampire.Instance.Player.PlayerId, Vampire.Instance.CurrentTarget.PlayerId,
                                true);

                            vampireKillButton.HasEffect = false; // Block effect on this click
                            vampireKillButton.Timer = vampireKillButton.MaxTimer;
                        }
                        else
                        {
                            Vampire.Instance.Bitten = Vampire.Instance.CurrentTarget;
                            // Notify players about bitten
                            Vampire.VampireSetBitten(Vampire.Instance.Bitten.PlayerId, false);

                            byte lastTimer = (byte)(float)Vampire.Instance.BiteDelay;
                            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Vampire.Instance.BiteDelay,
                                new Action<float>((p) =>
                                {
                                    // Delayed action
                                    if (p <= 1f)
                                    {
                                        byte timer = (byte)vampireKillButton.Timer;
                                        if (timer != lastTimer)
                                        {
                                            lastTimer = timer;
                                            GhostInfos.ShareGhostInfo(GhostInfos.Types.VampireTimer,
                                                Rpc.Serialize(new Tuple<float>(timer)));
                                        }
                                    }

                                    if (p == 1f)
                                    {
                                        // Perform kill if possible and reset bitten (regardless whether the kill was successful or not)
                                        Helpers.checkMurderAttemptAndKill(Vampire.Instance.Player, Vampire.Instance.Bitten,
                                            showAnimation: false);
                                        Vampire.VampireSetBitten(byte.MaxValue, true);
                                    }
                                })));
                            SoundEffectsManager.play("vampireBite");

                            vampireKillButton.HasEffect = true; // Trigger effect on this click
                        }
                    }
                    else if (murder == MurderAttemptResult.BlankKill)
                    {
                        vampireKillButton.Timer = vampireKillButton.MaxTimer;
                        vampireKillButton.HasEffect = false;
                    }
                    else
                    {
                        vampireKillButton.HasEffect = false;
                    }
                },
                () =>
                {
                    return Vampire.Instance.Player != null && Vampire.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl &&
                           !CachedPlayer.LocalPlayer.Data.IsDead;
                },
                () =>
                {
                    if (Vampire.Instance.TargetNearGarlic && Vampire.Instance.CanKillNearGarlics)
                    {
                        vampireKillButton.actionButton.graphic.sprite = __instance.KillButton.graphic.sprite;
                        vampireKillButton.showButtonText = true;
                    }
                    else
                    {
                        vampireKillButton.actionButton.graphic.sprite = Vampire.BiteButtonSprite;
                        vampireKillButton.showButtonText = false;
                    }

                    return Vampire.Instance.CurrentTarget != null && CachedPlayer.LocalPlayer.PlayerControl.CanMove &&
                           (!Vampire.Instance.TargetNearGarlic || Vampire.Instance.CanKillNearGarlics);
                },
                () =>
                {
                    vampireKillButton.Timer = vampireKillButton.MaxTimer;
                    vampireKillButton.isEffectActive = false;
                    vampireKillButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Vampire.BiteButtonSprite,
                CustomButton.ButtonPositions.upperRowLeft,
                __instance,
                "ActionSecondary",
                false,
                0f,
                () => { vampireKillButton.Timer = vampireKillButton.MaxTimer; }
            );

            garlicButton = new CustomButton(
                () =>
                {
                    Vampire.Instance.LocalPlacedGarlic = true;
                    var pos = CachedPlayer.LocalPlayer.transform.position;
                    Vampire.PlaceGarlic(pos.x, pos.y, pos.z);
                    SoundEffectsManager.play("garlic");
                },
                () =>
                {
                    return !Vampire.Instance.LocalPlacedGarlic && !CachedPlayer.LocalPlayer.Data.IsDead &&
                           Vampire.Instance.GarlicsActive && !HideNSeek.isHideNSeekGM;
                },
                () => { return CachedPlayer.LocalPlayer.PlayerControl.CanMove && !Vampire.Instance.LocalPlacedGarlic; },
                () => { },
                Vampire.GarlicButtonSprite,
                new Vector3(0, -0.06f, 0),
                __instance,
                null,
                true
            );

            portalmakerPlacePortalButton = new CustomButton(
                () =>
                {
                    portalmakerPlacePortalButton.Timer = portalmakerPlacePortalButton.MaxTimer;
                    var pos = CachedPlayer.LocalPlayer.transform.position;
                    Portalmaker.PlacePortal(pos.x, pos.y, pos.z);
                    SoundEffectsManager.play("tricksterPlaceBox");
                },
                () =>
                {
                    return Portalmaker.Instance.IsLocalPlayer && !CachedPlayer.LocalPlayer.Data.IsDead &&
                           Portal.secondPortal == null;
                },
                () => { return CachedPlayer.LocalPlayer.PlayerControl.CanMove && Portal.secondPortal == null; },
                () => { portalmakerPlacePortalButton.Timer = portalmakerPlacePortalButton.MaxTimer; },
                Portalmaker.PlacePortalButtonSprite,
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                "ActionQuaternary"
            );

            usePortalButton = new CustomButton(
                () =>
                {
                    bool didTeleport = false;
                    Vector3 exit = Portal.findExit(CachedPlayer.LocalPlayer.transform.position);
                    Vector3 entry = Portal.findEntry(CachedPlayer.LocalPlayer.transform.position);

                    bool portalMakerSoloTeleport =
                        !Portal.locationNearEntry(CachedPlayer.LocalPlayer.transform.position);
                    if (portalMakerSoloTeleport)
                    {
                        exit = Portal.firstPortal.portalGameObject.transform.position;
                        entry = CachedPlayer.LocalPlayer.transform.position;
                    }

                    CachedPlayer.LocalPlayer.NetTransform.RpcSnapTo(entry);

                    if (!CachedPlayer.LocalPlayer.Data.IsDead)
                    {
                        // Ghosts can portal too, but non-blocking and only with a local animation
                        Portalmaker.UsePortal(CachedPlayer.LocalPlayer.PlayerId,
                            portalMakerSoloTeleport ? (byte)1 : (byte)0);
                    }
                    else
                    {
                        Portalmaker.Local_UsePortal(CachedPlayer.LocalPlayer.PlayerId,
                            portalMakerSoloTeleport ? (byte)1 : (byte)0);
                    }

                    usePortalButton.Timer = usePortalButton.MaxTimer;
                    portalmakerMoveToPortalButton.Timer = usePortalButton.MaxTimer;
                    SoundEffectsManager.play("portalUse");
                    FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Portal.teleportDuration,
                        new Action<float>((p) =>
                        {
                            // Delayed action
                            CachedPlayer.LocalPlayer.PlayerControl.moveable = false;
                            CachedPlayer.LocalPlayer.NetTransform.Halt();
                            if (p >= 0.5f && p <= 0.53f && !didTeleport && !MeetingHud.Instance)
                            {
                                if (SubmergedCompatibility.IsSubmerged)
                                {
                                    SubmergedCompatibility.ChangeFloor(exit.y > -7);
                                }

                                CachedPlayer.LocalPlayer.NetTransform.RpcSnapTo(exit);
                                didTeleport = true;
                            }

                            if (p == 1f)
                            {
                                CachedPlayer.LocalPlayer.PlayerControl.moveable = true;
                            }
                        })));
                },
                () =>
                {
                    if (Portalmaker.Instance.IsLocalPlayer && Portal.bothPlacedAndEnabled)
                        portalmakerButtonText1.text =
                            Portal.locationNearEntry(CachedPlayer.LocalPlayer.transform.position) ||
                            !Portalmaker.Instance.CanPortalFromAnywhere
                                ? ""
                                : "1. " + Portal.firstPortal.room;
                    return Portal.bothPlacedAndEnabled;
                },
                () =>
                {
                    return CachedPlayer.LocalPlayer.PlayerControl.CanMove &&
                           (Portal.locationNearEntry(CachedPlayer.LocalPlayer.transform.position) ||
                            Portalmaker.Instance.CanPortalFromAnywhere && Portalmaker.Instance.IsLocalPlayer) &&
                           !Portal.isTeleporting;
                },
                () => { usePortalButton.Timer = usePortalButton.MaxTimer; },
                Portalmaker.UsePortalButtonSprite,
                new Vector3(0.9f, -0.06f, 0),
                __instance,
                "UsePortal",
                mirror: true
            );

            portalmakerMoveToPortalButton = new CustomButton(
                () =>
                {
                    bool didTeleport = false;
                    Vector3 exit = Portal.secondPortal.portalGameObject.transform.position;

                    if (!CachedPlayer.LocalPlayer.Data.IsDead)
                    {
                        // Ghosts can portal too, but non-blocking and only with a local animation
                        Portalmaker.UsePortal(CachedPlayer.LocalPlayer.PlayerId, 2);
                    }
                    else
                    {
                        Portalmaker.Local_UsePortal(CachedPlayer.LocalPlayer.PlayerId, 2);
                    }

                    usePortalButton.Timer = usePortalButton.MaxTimer;
                    portalmakerMoveToPortalButton.Timer = usePortalButton.MaxTimer;
                    SoundEffectsManager.play("portalUse");
                    FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Portal.teleportDuration,
                        new Action<float>((p) =>
                        {
                            // Delayed action
                            CachedPlayer.LocalPlayer.PlayerControl.moveable = false;
                            CachedPlayer.LocalPlayer.NetTransform.Halt();
                            if (p >= 0.5f && p <= 0.53f && !didTeleport && !MeetingHud.Instance)
                            {
                                if (SubmergedCompatibility.IsSubmerged)
                                {
                                    SubmergedCompatibility.ChangeFloor(exit.y > -7);
                                }

                                CachedPlayer.LocalPlayer.NetTransform.RpcSnapTo(exit);
                                didTeleport = true;
                            }

                            if (p == 1f)
                            {
                                CachedPlayer.LocalPlayer.PlayerControl.moveable = true;
                            }
                        })));
                },
                () =>
                {
                    return Portalmaker.Instance.CanPortalFromAnywhere && Portal.bothPlacedAndEnabled &&
                           Portalmaker.Instance.IsLocalPlayer;
                },
                () =>
                {
                    return CachedPlayer.LocalPlayer.PlayerControl.CanMove &&
                           !Portal.locationNearEntry(CachedPlayer.LocalPlayer.transform.position) &&
                           !Portal.isTeleporting;
                },
                () => { portalmakerMoveToPortalButton.Timer = usePortalButton.MaxTimer; },
                Portalmaker.UsePortalButtonSprite,
                new Vector3(0.9f, 1f, 0),
                __instance,
                "PortalMakerTeleportation",
                mirror: true
            );


            portalmakerButtonText1 = GameObject.Instantiate(usePortalButton.actionButton.cooldownTimerText,
                usePortalButton.actionButton.cooldownTimerText.transform.parent);
            portalmakerButtonText1.text = "";
            portalmakerButtonText1.enableWordWrapping = false;
            portalmakerButtonText1.transform.localScale = Vector3.one * 0.5f;
            portalmakerButtonText1.transform.localPosition += new Vector3(-0.05f, 0.55f, -1f);

            portalmakerButtonText2 =
                GameObject.Instantiate(portalmakerMoveToPortalButton.actionButton.cooldownTimerText,
                    portalmakerMoveToPortalButton.actionButton.cooldownTimerText.transform.parent);
            portalmakerButtonText2.text = "";
            portalmakerButtonText2.enableWordWrapping = false;
            portalmakerButtonText2.transform.localScale = Vector3.one * 0.5f;
            portalmakerButtonText2.transform.localPosition += new Vector3(-0.05f, 0.55f, -1f);


            // Jackal Sidekick Button
            jackalSidekickButton = new CustomButton(
                () =>
                {
                    Jackal.JackalCreatesSidekick(Jackal.Instance.CurrentTarget.PlayerId);
                    SoundEffectsManager.play("jackalSidekick");
                },
                () =>
                {
                    return Jackal.Instance.CanCreateSidekick && Jackal.Instance.Player != null &&
                           Jackal.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl &&
                           !CachedPlayer.LocalPlayer.Data.IsDead;
                },
                () =>
                {
                    return Jackal.Instance.CanCreateSidekick && Jackal.Instance.CurrentTarget != null &&
                           CachedPlayer.LocalPlayer.PlayerControl.CanMove;
                },
                () => { jackalSidekickButton.Timer = jackalSidekickButton.MaxTimer; },
                Jackal.SidekickButtonSprite,
                CustomButton.ButtonPositions.lowerRowCenter,
                __instance,
                "ActionQuaternary"
            );

            // Jackal Kill
            jackalKillButton = new CustomButton(
                () =>
                {
                    if (Helpers.checkMurderAttemptAndKill(Jackal.Instance.Player, Jackal.Instance.CurrentTarget) ==
                        MurderAttemptResult.SuppressKill) return;

                    jackalKillButton.Timer = jackalKillButton.MaxTimer;
                    Jackal.Instance.CurrentTarget = null;
                },
                () =>
                {
                    return Jackal.Instance.Player != null && Jackal.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl &&
                           !CachedPlayer.LocalPlayer.Data.IsDead;
                },
                () => { return Jackal.Instance.CurrentTarget && CachedPlayer.LocalPlayer.PlayerControl.CanMove; },
                () => { jackalKillButton.Timer = jackalKillButton.MaxTimer; },
                __instance.KillButton.graphic.sprite,
                CustomButton.ButtonPositions.upperRowRight,
                __instance,
                "ActionSecondary"
            );

            // Sidekick Kill
            sidekickKillButton = new CustomButton(
                () =>
                {
                    if (Helpers.checkMurderAttemptAndKill(Sidekick.Instance.Player, Sidekick.Instance.CurrentTarget) ==
                        MurderAttemptResult.SuppressKill) return;
                    sidekickKillButton.Timer = sidekickKillButton.MaxTimer;
                    Sidekick.Instance.CurrentTarget = null;
                },
                () =>
                {
                    return Sidekick.Instance.CanKill && Sidekick.Instance.Player != null &&
                           Sidekick.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl &&
                           !CachedPlayer.LocalPlayer.Data.IsDead;
                },
                () => { return Sidekick.Instance.CurrentTarget && CachedPlayer.LocalPlayer.PlayerControl.CanMove; },
                () => { sidekickKillButton.Timer = sidekickKillButton.MaxTimer; },
                __instance.KillButton.graphic.sprite,
                CustomButton.ButtonPositions.upperRowRight,
                __instance,
                "ActionSecondary"
            );

            // Eraser erase button
            eraserButton = new CustomButton(
                () =>
                {
                    eraserButton.MaxTimer += 10;
                    eraserButton.Timer = eraserButton.MaxTimer;
                    Eraser.SetFutureErased(Eraser.Instance.CurrentTarget.PlayerId);
                    SoundEffectsManager.play("eraserErase");
                },
                () =>
                {
                    return Eraser.Instance.Player != null && Eraser.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl &&
                           !CachedPlayer.LocalPlayer.Data.IsDead;
                },
                () => { return CachedPlayer.LocalPlayer.PlayerControl.CanMove && Eraser.Instance.CurrentTarget != null; },
                () => { eraserButton.Timer = eraserButton.MaxTimer; },
                Eraser.EraseButtonSprite,
                CustomButton.ButtonPositions.upperRowLeft,
                __instance,
                "ActionQuaternary"
            );

            placeJackInTheBoxButton = new CustomButton(
                () =>
                {
                    placeJackInTheBoxButton.Timer = placeJackInTheBoxButton.MaxTimer;

                    var pos = CachedPlayer.LocalPlayer.transform.position;
                    Trickster.PlaceJackInTheBox(pos.x, pos.y);
                    SoundEffectsManager.play("tricksterPlaceBox");
                },
                () =>
                {
                    return Trickster.Instance.Player != null &&
                           Trickster.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl &&
                           !CachedPlayer.LocalPlayer.Data.IsDead && !JackInTheBox.hasJackInTheBoxLimitReached();
                },
                () =>
                {
                    return CachedPlayer.LocalPlayer.PlayerControl.CanMove &&
                           !JackInTheBox.hasJackInTheBoxLimitReached();
                },
                () => { placeJackInTheBoxButton.Timer = placeJackInTheBoxButton.MaxTimer; },
                Trickster.PlaceBoxButtonSprite,
                CustomButton.ButtonPositions.upperRowLeft,
                __instance,
                "ActionQuaternary"
            );

            lightsOutButton = new CustomButton(
                () =>
                {
                    Trickster.LightsOut();
                    SoundEffectsManager.play("lighterLight");
                },
                () =>
                {
                    return Trickster.Instance.Player != null &&
                           Trickster.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl &&
                           !CachedPlayer.LocalPlayer.Data.IsDead && JackInTheBox.hasJackInTheBoxLimitReached() &&
                           JackInTheBox.boxesConvertedToVents;
                },
                () =>
                {
                    return CachedPlayer.LocalPlayer.PlayerControl.CanMove &&
                           JackInTheBox.hasJackInTheBoxLimitReached() && JackInTheBox.boxesConvertedToVents;
                },
                () =>
                {
                    lightsOutButton.Timer = lightsOutButton.MaxTimer;
                    lightsOutButton.isEffectActive = false;
                    lightsOutButton.actionButton.graphic.color = Palette.EnabledColor;
                },
                Trickster.LightsOutButtonSprite,
                CustomButton.ButtonPositions.upperRowLeft,
                __instance,
                "ActionQuaternary",
                true,
                Trickster.Instance.LightsOutDuration,
                () =>
                {
                    lightsOutButton.Timer = lightsOutButton.MaxTimer;
                    SoundEffectsManager.play("lighterLight");
                }
            );

            // Cleaner Clean
            cleanerCleanButton = new CustomButton(
                () =>
                {
                    foreach (var collider2D in Physics2D.OverlapCircleAll(
                                 CachedPlayer.LocalPlayer.PlayerControl.GetTruePosition(),
                                 CachedPlayer.LocalPlayer.PlayerControl.MaxReportDistance, Constants.PlayersOnlyMask))
                    {
                        if (collider2D.tag != "DeadBody") continue;
                        DeadBody component = collider2D.GetComponent<DeadBody>();
                        if (!component || component.Reported) continue;
                        var truePosition = CachedPlayer.LocalPlayer.PlayerControl.GetTruePosition();
                        var truePosition2 = component.TruePosition;
                        if (Vector2.Distance(truePosition2, truePosition) <=
                            CachedPlayer.LocalPlayer.PlayerControl.MaxReportDistance &&
                            CachedPlayer.LocalPlayer.PlayerControl.CanMove &&
                            !PhysicsHelpers.AnythingBetween(truePosition, truePosition2, Constants.ShipAndObjectsMask,
                                false))
                        {
                            var playerInfo = GameData.Instance.GetPlayerById(component.ParentId);
                            CommonRpc.CleanBody(playerInfo.PlayerId, Cleaner.Instance.Player.PlayerId);

                            Cleaner.Instance.Player.killTimer = cleanerCleanButton.Timer = cleanerCleanButton.MaxTimer;
                            SoundEffectsManager.play("cleanerClean");
                            break;
                        }
                    }
                },
                () =>
                {
                    return Cleaner.Instance.Player != null && Cleaner.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl &&
                           !CachedPlayer.LocalPlayer.Data.IsDead;
                },
                () =>
                {
                    return __instance.ReportButton.graphic.color == Palette.EnabledColor &&
                           CachedPlayer.LocalPlayer.PlayerControl.CanMove;
                },
                () => { cleanerCleanButton.Timer = cleanerCleanButton.MaxTimer; },
                Cleaner.CleanButtonSprite,
                CustomButton.ButtonPositions.upperRowLeft,
                __instance,
                "ActionQuaternary"
            );

            // Warlock curse
            warlockCurseButton = new CustomButton(
                () =>
                {
                    if (Warlock.Instance.CurseVictim == null)
                    {
                        // Apply Curse
                        Warlock.Instance.CurseVictim = Warlock.Instance.CurrentTarget;
                        warlockCurseButton.Sprite = Warlock.CurseKillButtonSprite;
                        warlockCurseButton.Timer = 1f;
                        SoundEffectsManager.play("warlockCurse");

                        // Ghost Info
                        GhostInfos.ShareGhostInfo(GhostInfos.Types.WarlockTarget,
                            Rpc.Serialize(new Tuple<byte>(Warlock.Instance.CurseVictim.PlayerId)));
                    }
                    else if (Warlock.Instance.CurseVictim != null && Warlock.Instance.CurseVictimTarget != null)
                    {
                        MurderAttemptResult murder = Helpers.checkMurderAttemptAndKill(Warlock.Instance.Player,
                            Warlock.Instance.CurseVictimTarget, showAnimation: false);
                        if (murder == MurderAttemptResult.SuppressKill) return;

                        // If blanked or killed
                        if (Warlock.Instance.RootDuration > 0)
                        {
                            AntiTeleport.Instance.Position = CachedPlayer.LocalPlayer.transform.position;
                            CachedPlayer.LocalPlayer.PlayerControl.moveable = false;
                            CachedPlayer.LocalPlayer.NetTransform
                                .Halt(); // Stop current movement so the warlock is not just running straight into the next object
                            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Warlock.Instance.RootDuration,
                                new Action<float>((p) =>
                                {
                                    // Delayed action
                                    if (p == 1f)
                                    {
                                        CachedPlayer.LocalPlayer.PlayerControl.moveable = true;
                                    }
                                })));
                        }

                        Warlock.Instance.CurseVictim = null;
                        Warlock.Instance.CurseVictimTarget = null;
                        warlockCurseButton.Sprite = Warlock.CurseButtonSprite;
                        Warlock.Instance.Player.killTimer = warlockCurseButton.Timer = warlockCurseButton.MaxTimer;

                        GhostInfos.ShareGhostInfo(GhostInfos.Types.WarlockTarget,
                            Rpc.Serialize(new Tuple<byte>(byte.MaxValue)));
                    }
                },
                () =>
                {
                    return Warlock.Instance.Player != null && Warlock.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl &&
                           !CachedPlayer.LocalPlayer.Data.IsDead;
                },
                () =>
                {
                    return ((Warlock.Instance.CurseVictim == null && Warlock.Instance.CurseVictimTarget != null) ||
                            (Warlock.Instance.CurseVictim != null && Warlock.Instance.CurseVictimTarget != null)) &&
                           CachedPlayer.LocalPlayer.PlayerControl.CanMove;
                },
                () =>
                {
                    warlockCurseButton.Timer = warlockCurseButton.MaxTimer;
                    warlockCurseButton.Sprite = Warlock.CurseButtonSprite;
                    Warlock.Instance.CurseVictim = null;
                    Warlock.Instance.CurseVictimTarget = null;
                },
                Warlock.CurseButtonSprite,
                CustomButton.ButtonPositions.upperRowLeft,
                __instance,
                "ActionQuaternary"
            );

            // Security Guard button
            securityGuardButton = new CustomButton(
                () =>
                {
                    if (SecurityGuard.Instance.VentTarget != null)
                    {
                        // Seal vent
                        SecurityGuard.SealVent(SecurityGuard.Instance.VentTarget.Id);
                        SecurityGuard.Instance.VentTarget = null;
                    }
                    else if (GameOptionsManager.Instance.currentNormalGameOptions.MapId != 1 &&
                             !SubmergedCompatibility.IsSubmerged)
                    {
                        // Place camera if there's no vent and it's not MiraHQ or Submerged
                        var pos = CachedPlayer.LocalPlayer.transform.position;
                        SecurityGuard.PlaceCamera(pos.x, pos.y, pos.z);
                    }

                    SoundEffectsManager.play("securityGuardPlaceCam"); // Same sound used for both types (cam or vent)!
                    securityGuardButton.Timer = securityGuardButton.MaxTimer;
                },
                () =>
                {
                    return SecurityGuard.Instance.Player != null &&
                           SecurityGuard.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl &&
                           !CachedPlayer.LocalPlayer.Data.IsDead && SecurityGuard.Instance.RemainingScrews >=
                           Mathf.Min(SecurityGuard.Instance.VentPrice, SecurityGuard.Instance.CamPrice);
                },
                () =>
                {
                    securityGuardButton.actionButton.graphic.sprite =
                        (SecurityGuard.Instance.VentTarget == null &&
                         GameOptionsManager.Instance.currentNormalGameOptions.MapId != 1 &&
                         !SubmergedCompatibility.IsSubmerged)
                            ? SecurityGuard.PlaceCameraButtonSprite
                            : SecurityGuard.CloseVentButtonSprite;
                    if (securityGuardButtonScrewsText != null)
                        securityGuardButtonScrewsText.text =
                            $"{SecurityGuard.Instance.RemainingScrews}/{(int)SecurityGuard.Instance.TotalScrews}";

                    if (SecurityGuard.Instance.VentTarget != null)
                        return SecurityGuard.Instance.RemainingScrews >= SecurityGuard.Instance.VentPrice &&
                               CachedPlayer.LocalPlayer.PlayerControl.CanMove;
                    return GameOptionsManager.Instance.currentNormalGameOptions.MapId != 1 &&
                           !SubmergedCompatibility.IsSubmerged &&
                           SecurityGuard.Instance.RemainingScrews >= SecurityGuard.Instance.CamPrice &&
                           CachedPlayer.LocalPlayer.PlayerControl.CanMove;
                },
                () => { securityGuardButton.Timer = securityGuardButton.MaxTimer; },
                SecurityGuard.PlaceCameraButtonSprite,
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                "ActionQuaternary"
            );

            // Security Guard button screws counter
            securityGuardButtonScrewsText = GameObject.Instantiate(securityGuardButton.actionButton.cooldownTimerText,
                securityGuardButton.actionButton.cooldownTimerText.transform.parent);
            securityGuardButtonScrewsText.text = "";
            securityGuardButtonScrewsText.enableWordWrapping = false;
            securityGuardButtonScrewsText.transform.localScale = Vector3.one * 0.5f;
            securityGuardButtonScrewsText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            securityGuardCamButton = new CustomButton(
                () =>
                {
                    if (GameOptionsManager.Instance.currentNormalGameOptions.MapId != 1)
                    {
                        if (SecurityGuard.Instance.Minigame == null)
                        {
                            byte mapId = GameOptionsManager.Instance.currentNormalGameOptions.MapId;
                            var e = UnityEngine.Object.FindObjectsOfType<SystemConsole>()
                                .FirstOrDefault(x => x.gameObject.name.Contains("Surv_Panel"));
                            if (mapId == 0 || mapId == 3)
                                e = UnityEngine.Object.FindObjectsOfType<SystemConsole>()
                                    .FirstOrDefault(x => x.gameObject.name.Contains("SurvConsole"));
                            else if (mapId == 4)
                                e = UnityEngine.Object.FindObjectsOfType<SystemConsole>()
                                    .FirstOrDefault(x => x.gameObject.name.Contains("task_cams"));
                            if (e == null || Camera.main == null) return;
                            SecurityGuard.Instance.Minigame =
                                UnityEngine.Object.Instantiate(e.MinigamePrefab, Camera.main.transform, false);
                        }

                        SecurityGuard.Instance.Minigame.transform.SetParent(Camera.main.transform, false);
                        SecurityGuard.Instance.Minigame.transform.localPosition = new Vector3(0.0f, 0.0f, -50f);
                        SecurityGuard.Instance.Minigame.Begin(null);
                    }
                    else
                    {
                        if (SecurityGuard.Instance.Minigame == null)
                        {
                            var e = UnityEngine.Object.FindObjectsOfType<SystemConsole>()
                                .FirstOrDefault(x => x.gameObject.name.Contains("SurvLogConsole"));
                            if (e == null || Camera.main == null) return;
                            SecurityGuard.Instance.Minigame =
                                UnityEngine.Object.Instantiate(e.MinigamePrefab, Camera.main.transform, false);
                        }

                        SecurityGuard.Instance.Minigame.transform.SetParent(Camera.main.transform, false);
                        SecurityGuard.Instance.Minigame.transform.localPosition = new Vector3(0.0f, 0.0f, -50f);
                        SecurityGuard.Instance.Minigame.Begin(null);
                    }

                    SecurityGuard.Instance.UsedCharges++;

                    if (!SecurityGuard.Instance.CanMoveDuringCam) CachedPlayer.LocalPlayer.PlayerControl.moveable = false;
                    CachedPlayer.LocalPlayer.NetTransform.Halt(); // Stop current movement 
                },
                () =>
                {
                    return SecurityGuard.Instance.Player != null &&
                           SecurityGuard.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl &&
                           !CachedPlayer.LocalPlayer.Data.IsDead && SecurityGuard.Instance.RemainingScrews <
                           Mathf.Min(SecurityGuard.Instance.VentPrice, SecurityGuard.Instance.CamPrice)
                           && !SubmergedCompatibility.IsSubmerged;
                },
                () =>
                {
                    if (securityGuardChargesText != null)
                        securityGuardChargesText.text = $"{SecurityGuard.Instance.UsedCharges} / {(int)SecurityGuard.Instance.CamMaxCharges}";
                    securityGuardCamButton.actionButton.graphic.sprite =
                        GameOptionsManager.Instance.currentNormalGameOptions.MapId == 1
                            ? SecurityGuard.DoorLogButtonSprite
                            : SecurityGuard.CameraButtonSprite;
                    securityGuardCamButton.actionButton.OverrideText(
                        GameOptionsManager.Instance.currentNormalGameOptions.MapId == 1 ? "DOORLOG" : "SECURITY");
                    return CachedPlayer.LocalPlayer.PlayerControl.CanMove && SecurityGuard.Instance.UsedCharges > 0;
                },
                () =>
                {
                    securityGuardCamButton.Timer = securityGuardCamButton.MaxTimer;
                    securityGuardCamButton.isEffectActive = false;
                    securityGuardCamButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                SecurityGuard.CameraButtonSprite,
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                "ActionSecondary",
                true,
                0f,
                () =>
                {
                    securityGuardCamButton.Timer = securityGuardCamButton.MaxTimer;
                    if (Minigame.Instance)
                    {
                        SecurityGuard.Instance.Minigame.ForceClose();
                    }

                    CachedPlayer.LocalPlayer.PlayerControl.moveable = true;
                },
                false,
                GameOptionsManager.Instance.currentNormalGameOptions.MapId == 1 ? "DOORLOG" : "SECURITY"
            );

            // Security Guard cam button charges
            securityGuardChargesText = GameObject.Instantiate(securityGuardCamButton.actionButton.cooldownTimerText,
                securityGuardCamButton.actionButton.cooldownTimerText.transform.parent);
            securityGuardChargesText.text = "";
            securityGuardChargesText.enableWordWrapping = false;
            securityGuardChargesText.transform.localScale = Vector3.one * 0.5f;
            securityGuardChargesText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // Arsonist button
            arsonistButton = new CustomButton(
                () =>
                {
                    bool dousedEveryoneAlive = Arsonist.Instance.DousedEveryoneAlive();
                    if (dousedEveryoneAlive)
                    {
                        Arsonist.ArsonistWin();
                        arsonistButton.HasEffect = false;
                    }
                    else if (Arsonist.Instance.CurrentTarget != null)
                    {
                        Arsonist.Instance.DouseTarget = Arsonist.Instance.CurrentTarget;
                        arsonistButton.HasEffect = true;
                        SoundEffectsManager.play("arsonistDouse");
                    }
                },
                () =>
                {
                    return Arsonist.Instance.Player != null && Arsonist.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl &&
                           !CachedPlayer.LocalPlayer.Data.IsDead;
                },
                () =>
                {
                    bool dousedEveryoneAlive = Arsonist.Instance.DousedEveryoneAlive();
                    if (dousedEveryoneAlive) arsonistButton.actionButton.graphic.sprite = Arsonist.IgniteButtonSprite;

                    if (arsonistButton.isEffectActive && Arsonist.Instance.DouseTarget != Arsonist.Instance.CurrentTarget)
                    {
                        Arsonist.Instance.DouseTarget = null;
                        arsonistButton.Timer = 0f;
                        arsonistButton.isEffectActive = false;
                    }

                    return CachedPlayer.LocalPlayer.PlayerControl.CanMove &&
                           (dousedEveryoneAlive || Arsonist.Instance.CurrentTarget != null);
                },
                () =>
                {
                    arsonistButton.Timer = arsonistButton.MaxTimer;
                    arsonistButton.isEffectActive = false;
                    Arsonist.Instance.DouseTarget = null;
                },
                Arsonist.DouseButtonSprite,
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                "ActionQuaternary",
                true,
                Arsonist.Instance.DouseDuration,
                () =>
                {
                    if (Arsonist.Instance.DouseTarget != null) Arsonist.Instance.DousedPlayers.Add(Arsonist.Instance.DouseTarget);

                    arsonistButton.Timer = Arsonist.Instance.DousedEveryoneAlive() ? 0 : arsonistButton.MaxTimer;

                    foreach (PlayerControl p in Arsonist.Instance.DousedPlayers)
                    {
                        if (TORMapOptions.playerIcons.ContainsKey(p.PlayerId))
                        {
                            TORMapOptions.playerIcons[p.PlayerId].setSemiTransparent(false);
                        }
                    }

                    // Ghost Info
                    GhostInfos.ShareGhostInfo(GhostInfos.Types.ArsonistDouse,
                        Rpc.Serialize(new Tuple<byte>(Arsonist.Instance.DouseTarget.PlayerId)));
                    Arsonist.Instance.DouseTarget = null;
                }
            );

            // Vulture Eat
            vultureEatButton = new CustomButton(
                () =>
                {
                    foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(
                                 CachedPlayer.LocalPlayer.PlayerControl.GetTruePosition(),
                                 CachedPlayer.LocalPlayer.PlayerControl.MaxReportDistance, Constants.PlayersOnlyMask))
                    {
                        if (collider2D.tag == "DeadBody")
                        {
                            var component = collider2D.GetComponent<DeadBody>();
                            if (component && !component.Reported)
                            {
                                var truePosition = CachedPlayer.LocalPlayer.PlayerControl.GetTruePosition();
                                var truePosition2 = component.TruePosition;
                                if (Vector2.Distance(truePosition2, truePosition) <=
                                    CachedPlayer.LocalPlayer.PlayerControl.MaxReportDistance &&
                                    CachedPlayer.LocalPlayer.PlayerControl.CanMove &&
                                    !PhysicsHelpers.AnythingBetween(truePosition, truePosition2,
                                        Constants.ShipAndObjectsMask, false))
                                {
                                    var playerInfo = GameData.Instance.GetPlayerById(component.ParentId);
                                    CommonRpc.CleanBody(playerInfo.PlayerId, Vulture.Instance.Player.PlayerId);

                                    vultureEatButton.Timer = vultureEatButton.MaxTimer;
                                    SoundEffectsManager.play("vultureEat");
                                    break;
                                }
                            }
                        }
                    }
                },
                () =>
                {
                    return Vulture.Instance.Player != null && Vulture.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl &&
                           !CachedPlayer.LocalPlayer.Data.IsDead;
                },
                () =>
                {
                    return __instance.ReportButton.graphic.color == Palette.EnabledColor &&
                           CachedPlayer.LocalPlayer.PlayerControl.CanMove;
                },
                () => { vultureEatButton.Timer = vultureEatButton.MaxTimer; },
                Vulture.EatButtonSprite,
                CustomButton.ButtonPositions.lowerRowCenter,
                __instance,
                "ActionQuaternary"
            );

            // Medium button
            mediumButton = new CustomButton(
                () =>
                {
                    if (Medium.Instance.Target != null)
                    {
                        Medium.Instance.SoulTarget = Medium.Instance.Target;
                        mediumButton.HasEffect = true;
                        SoundEffectsManager.play("mediumAsk");
                    }
                },
                () => { return Medium.Instance.IsLocalPlayer && !CachedPlayer.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (mediumButton.isEffectActive && Medium.Instance.Target != Medium.Instance.SoulTarget)
                    {
                        Medium.Instance.SoulTarget = null;
                        mediumButton.Timer = 0f;
                        mediumButton.isEffectActive = false;
                    }

                    return Medium.Instance.Target != null && CachedPlayer.LocalPlayer.PlayerControl.CanMove;
                },
                () =>
                {
                    mediumButton.Timer = mediumButton.MaxTimer;
                    mediumButton.isEffectActive = false;
                    Medium.Instance.SoulTarget = null;
                },
                Medium.QuestionButtonSprite,
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                "ActionQuaternary",
                true,
                Medium.Instance.QuestionDuration,
                () =>
                {
                    mediumButton.Timer = mediumButton.MaxTimer;
                    if (!Medium.Instance.HasPlayer || Medium.Instance.Target.player == null) return;
                    string msg = Medium.getInfo(Medium.Instance.Target.player, Medium.Instance.Target.killerIfExisting);
                    FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(CachedPlayer.LocalPlayer.PlayerControl,
                        msg);

                    // Ghost Info
                    GhostInfos.ShareGhostInfo(GhostInfos.Types.MediumInfo,
                        Rpc.Serialize(new Tuple<byte, string>(Medium.Instance.Target.player.PlayerId, msg)));

                    // Remove soul
                    if (Medium.Instance.OneQuestionPerSoul)
                    {
                        float closestDistance = float.MaxValue;
                        SpriteRenderer target = null;

                        foreach ((DeadPlayer db, Vector3 ps) in Medium.Instance.DeadBodies)
                        {
                            if (db == Medium.Instance.Target)
                            {
                                Tuple<DeadPlayer, Vector3> deadBody = Tuple.Create(db, ps);
                                Medium.Instance.DeadBodies.Remove(deadBody);
                                break;
                            }
                        }

                        foreach (SpriteRenderer rend in Medium.Instance.Souls)
                        {
                            float distance = Vector2.Distance(rend.transform.position,
                                CachedPlayer.LocalPlayer.PlayerControl.GetTruePosition());
                            if (distance < closestDistance)
                            {
                                closestDistance = distance;
                                target = rend;
                            }
                        }

                        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(5f,
                            new Action<float>((p) =>
                            {
                                if (target != null)
                                {
                                    var tmp = target.color;
                                    tmp.a = Mathf.Clamp01(1 - p);
                                    target.color = tmp;
                                }

                                if (p == 1f && target != null && target.gameObject != null)
                                    UnityEngine.Object.Destroy(target.gameObject);
                            })));

                        Medium.Instance.Souls.Remove(target);
                    }

                    SoundEffectsManager.stop("mediumAsk");
                }
            );

            // Pursuer button
            pursuerButton = new CustomButton(
                () =>
                {
                    if (Pursuer.Instance.CurrentTarget != null)
                    {
                        Pursuer.SetBlanked(Pursuer.Instance.CurrentTarget.PlayerId, true);
                        Pursuer.Instance.CurrentTarget = null;
                        Pursuer.Instance.UsedBlanks++;
                        pursuerButton.Timer = pursuerButton.MaxTimer;
                        SoundEffectsManager.play("pursuerBlank");
                    }
                },
                () =>
                {
                    return Pursuer.Instance.IsAliveLocalPlayer &&
                           Pursuer.Instance.UsedBlanks < Pursuer.Instance.BlankNumber;
                },
                () =>
                {
                    if (pursuerButtonBlanksText != null)
                        pursuerButtonBlanksText.text = $"{Pursuer.Instance.BlankNumber - Pursuer.Instance.UsedBlanks}";

                    return Pursuer.Instance.BlankNumber > Pursuer.Instance.UsedBlanks &&
                           CachedPlayer.LocalPlayer.PlayerControl.CanMove && Pursuer.Instance.CurrentTarget != null;
                },
                () => { pursuerButton.Timer = pursuerButton.MaxTimer; },
                Pursuer.BlankButtonSprite,
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                "ActionQuaternary"
            );

            // Pursuer button blanks left
            pursuerButtonBlanksText = GameObject.Instantiate(pursuerButton.actionButton.cooldownTimerText,
                pursuerButton.actionButton.cooldownTimerText.transform.parent);
            pursuerButtonBlanksText.text = "";
            pursuerButtonBlanksText.enableWordWrapping = false;
            pursuerButtonBlanksText.transform.localScale = Vector3.one * 0.5f;
            pursuerButtonBlanksText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);


            // Witch Spell button
            witchSpellButton = new CustomButton(
                () =>
                {
                    if (Witch.Instance.CurrentTarget != null)
                    {
                        Witch.Instance.SpellCastingTarget = Witch.Instance.CurrentTarget;
                        SoundEffectsManager.play("witchSpell");
                    }
                },
                () =>
                {
                    return Witch.Instance.Player != null && Witch.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl &&
                           !CachedPlayer.LocalPlayer.Data.IsDead;
                },
                () =>
                {
                    if (witchSpellButton.isEffectActive && Witch.Instance.SpellCastingTarget != Witch.Instance.CurrentTarget)
                    {
                        Witch.Instance.SpellCastingTarget = null;
                        witchSpellButton.Timer = 0f;
                        witchSpellButton.isEffectActive = false;
                    }

                    return CachedPlayer.LocalPlayer.PlayerControl.CanMove && Witch.Instance.CurrentTarget != null;
                },
                () =>
                {
                    witchSpellButton.Timer = witchSpellButton.MaxTimer;
                    witchSpellButton.isEffectActive = false;
                    Witch.Instance.SpellCastingTarget = null;
                },
                Witch.SpellButtonSprite,
                CustomButton.ButtonPositions.upperRowLeft,
                __instance,
                "ActionQuaternary",
                true,
                Witch.Instance.SpellCastingDuration,
                () =>
                {
                    if (Witch.Instance.SpellCastingTarget == null) return;
                    MurderAttemptResult attempt = Helpers.checkMurderAttempt(Witch.Instance.Player, Witch.Instance.SpellCastingTarget);
                    if (attempt == MurderAttemptResult.PerformKill)
                    {
                        Witch.SetFutureSpelled(Witch.Instance.CurrentTarget.PlayerId);
                    }

                    if (attempt == MurderAttemptResult.BlankKill || attempt == MurderAttemptResult.PerformKill)
                    {
                        Witch.Instance.CurrentCooldownAddition += Witch.Instance.AdditionalCooldown;
                        witchSpellButton.MaxTimer = Witch.Instance.SpellCooldown + Witch.Instance.CurrentCooldownAddition;
                        Patches.PlayerControlFixedUpdatePatch
                            .miniCooldownUpdate(); // Modifies the MaxTimer if the witch is the mini
                        witchSpellButton.Timer = witchSpellButton.MaxTimer;
                        if (Witch.Instance.TriggerBothCooldown)
                        {
                            float multiplier =
                                (Mini.Instance.Player != null && CachedPlayer.LocalPlayer.PlayerControl == Mini.Instance.Player)
                                    ? (Mini.Instance.IsGrownUp ? 0.66f : 2f)
                                    : 1f;
                            Witch.Instance.Player.killTimer = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown *
                                                    multiplier;
                        }
                    }
                    else
                    {
                        witchSpellButton.Timer = 0f;
                    }

                    Witch.Instance.SpellCastingTarget = null;
                }
            );

            // Ninja mark and assassinate button 
            ninjaButton = new CustomButton(
                () =>
                {
                    if (Ninja.Instance.MarkedTarget != null)
                    {
                        // Murder attempt with teleport
                        MurderAttemptResult attempt = Helpers.checkMurderAttempt(Ninja.Instance.Player, Ninja.Instance.MarkedTarget);
                        if (attempt == MurderAttemptResult.PerformKill)
                        {
                            // Create first trace before killing
                            var pos = CachedPlayer.LocalPlayer.transform.position;
                            Ninja.PlaceNinjaTrace(pos.x, pos.y, pos.z);
                            Ninja.SetInvisible(Ninja.Instance.Player.PlayerId, false);

                            // Perform Kill
                            if (SubmergedCompatibility.IsSubmerged)
                            {
                                SubmergedCompatibility.ChangeFloor(Ninja.Instance.MarkedTarget.transform.localPosition.y > -7);
                            }

                            KernelRpc.UncheckedMurderPlayer(CachedPlayer.LocalPlayer.PlayerId,
                                Ninja.Instance.MarkedTarget.PlayerId, true);
                            // Create Second trace after killing
                            pos = Ninja.Instance.MarkedTarget.transform.position;
                            Ninja.PlaceNinjaTrace(pos.x, pos.y, pos.z);
                        }

                        if (attempt == MurderAttemptResult.BlankKill || attempt == MurderAttemptResult.PerformKill)
                        {
                            ninjaButton.Timer = ninjaButton.MaxTimer;
                            Ninja.Instance.Player.killTimer = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
                        }
                        else if (attempt == MurderAttemptResult.SuppressKill)
                        {
                            ninjaButton.Timer = 0f;
                        }

                        Ninja.Instance.MarkedTarget = null;
                        return;
                    }

                    if (Ninja.Instance.CurrentTarget != null)
                    {
                        Ninja.Instance.MarkedTarget = Ninja.Instance.CurrentTarget;
                        ninjaButton.Timer = 5f;
                        SoundEffectsManager.play("warlockCurse");

                        // Ghost Info
                        GhostInfos.ShareGhostInfo(GhostInfos.Types.NinjaMarked,
                            Rpc.Serialize(new Tuple<byte>(Ninja.Instance.MarkedTarget.PlayerId)));
                    }
                },
                () =>
                {
                    return Ninja.Instance.Player != null && Ninja.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl &&
                           !CachedPlayer.LocalPlayer.Data.IsDead;
                },
                () =>
                {
                    // CouldUse
                    ninjaButton.Sprite = Ninja.Instance.MarkedTarget != null
                        ? Ninja.AssassinateButtonSprite
                        : Ninja.MarkButtonSprite;
                    return (Ninja.Instance.CurrentTarget != null || Ninja.Instance.MarkedTarget != null) &&
                           CachedPlayer.LocalPlayer.PlayerControl.CanMove;
                },
                () =>
                {
                    // on meeting ends
                    ninjaButton.Timer = ninjaButton.MaxTimer;
                    Ninja.Instance.MarkedTarget = null;
                },
                Ninja.MarkButtonSprite,
                CustomButton.ButtonPositions.upperRowLeft,
                __instance,
                "ActionQuaternary"
            );

            mayorMeetingButton = new CustomButton(
                () =>
                {
                    CachedPlayer.LocalPlayer.NetTransform.Halt(); // Stop current movement 
                    Mayor.Instance.UsedRemoteMeetings++;
                    Helpers
                        .handleVampireBiteOnBodyReport(); // Manually call Vampire handling, since the CmdReportDeadBody Prefix won't be called
                    // Helpers.handleWhispererKillOnBodyReport();
                    // Helpers.handleUndertakerDropOnBodyReport();
                    KernelRpc.UncheckedCmdReportDeadBody(CachedPlayer.LocalPlayer.PlayerId, null);
                    mayorMeetingButton.Timer = 1f;
                },
                () =>
                {
                    return Mayor.Instance.IsLocalPlayer && !CachedPlayer.LocalPlayer.Data.IsDead &&
                           Mayor.Instance.HasRemoteMeetingButton;
                },
                () =>
                {
                    mayorMeetingButton.actionButton.OverrideText(
                        "Emergency (" + Mayor.Instance.RemoteMeetingsLeft + ")");
                    bool sabotageActive = false;
                    foreach (PlayerTask task in CachedPlayer.LocalPlayer.PlayerControl.myTasks.GetFastEnumerator())
                        if (task.TaskType == TaskTypes.FixLights || task.TaskType == TaskTypes.RestoreOxy ||
                            task.TaskType == TaskTypes.ResetReactor || task.TaskType == TaskTypes.ResetSeismic ||
                            task.TaskType == TaskTypes.FixComms || task.TaskType == TaskTypes.StopCharles
                            || SubmergedCompatibility.IsSubmerged &&
                            task.TaskType == SubmergedCompatibility.RetrieveOxygenMask)
                            sabotageActive = true;
                    return !sabotageActive && CachedPlayer.LocalPlayer.PlayerControl.CanMove &&
                           Mayor.Instance.RemoteMeetingsLeft > 0;
                },
                () => { mayorMeetingButton.Timer = mayorMeetingButton.MaxTimer; },
                Mayor.MeetingButtonSprite,
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                "ActionQuaternary",
                true,
                0f,
                () => { },
                false,
                "Meeting"
            );

            // Trapper button
            trapperButton = new CustomButton(
                () =>
                {
                    var pos = CachedPlayer.LocalPlayer.transform.position;
                    Trapper.SetTrap(pos.x, pos.y, pos.z);

                    SoundEffectsManager.play("trapperTrap");
                    trapperButton.Timer = trapperButton.MaxTimer;
                },
                () =>
                {
                    return Trapper.Instance.Player != null && Trapper.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl &&
                           !CachedPlayer.LocalPlayer.Data.IsDead;
                },
                () =>
                {
                    if (trapperChargesText != null)
                        trapperChargesText.text = $"{Trapper.Instance.Charges} / {(int)Trapper.Instance.MaxCharges}";
                    return CachedPlayer.LocalPlayer.PlayerControl.CanMove && Trapper.Instance.Charges > 0;
                },
                () => { trapperButton.Timer = trapperButton.MaxTimer; },
                Trapper.TrapButtonSprite,
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                "ActionQuaternary"
            );

            // Bomber button
            bomberButton = new CustomButton(
                () =>
                {
                    if (Helpers.checkMurderAttempt(Bomber.Instance.Player, Bomber.Instance.Player) != MurderAttemptResult.BlankKill)
                    {
                        var pos = CachedPlayer.LocalPlayer.transform.position;
                        Bomber.PlaceBomb(pos.x, pos.y, pos.y);
                        SoundEffectsManager.play("trapperTrap");
                    }

                    bomberButton.Timer = bomberButton.MaxTimer;
                    Bomber.Instance.IsPlanted = true;
                },
                () =>
                {
                    return Bomber.Instance.Player != null && Bomber.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl &&
                           !CachedPlayer.LocalPlayer.Data.IsDead;
                },
                () => { return CachedPlayer.LocalPlayer.PlayerControl.CanMove && !Bomber.Instance.IsPlanted; },
                () => { bomberButton.Timer = bomberButton.MaxTimer; },
                Bomber.PlantBombButtonSprite,
                CustomButton.ButtonPositions.upperRowLeft,
                __instance,
                "ActionQuaternary",
                true,
                Bomber.Instance.BombDestructionTime,
                () =>
                {
                    bomberButton.Timer = bomberButton.MaxTimer;
                    bomberButton.isEffectActive = false;
                    bomberButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                }
            );

            defuseButton = new CustomButton(
                () => { defuseButton.HasEffect = true; },
                () =>
                {
                    if (shifterShiftButton.HasButton())
                        defuseButton.PositionOffset = new Vector3(0f, 2f, 0f);
                    else
                        defuseButton.PositionOffset = new Vector3(0f, 1f, 0f);
                    return Bomber.Instance.Bomb != null && Bomb.canDefuse && !CachedPlayer.LocalPlayer.Data.IsDead;
                },
                () =>
                {
                    if (defuseButton.isEffectActive && !Bomb.canDefuse)
                    {
                        defuseButton.Timer = 0f;
                        defuseButton.isEffectActive = false;
                    }

                    return CachedPlayer.LocalPlayer.PlayerControl.CanMove;
                },
                () =>
                {
                    defuseButton.Timer = 0f;
                    defuseButton.isEffectActive = false;
                },
                Bomb.getDefuseSprite(),
                new Vector3(0f, 1f, 0),
                __instance,
                "defuseBomb",
                true,
                Bomber.Instance.BombDefuseDuration,
                () =>
                {
                    Bomber.DefuseBomb();

                    defuseButton.Timer = 0f;
                    Bomb.canDefuse = false;
                },
                true
            );

            thiefKillButton = new CustomButton(
                () =>
                {
                    PlayerControl thief = Thief.Instance.Player;
                    PlayerControl target = Thief.Instance.CurrentTarget;
                    var result = Helpers.checkMurderAttempt(thief, target);
                    if (result == MurderAttemptResult.BlankKill)
                    {
                        thiefKillButton.Timer = thiefKillButton.MaxTimer;
                        return;
                    }

                    if (Thief.Instance.SuicideFlag)
                    {
                        // Suicide
                        KernelRpc.UncheckedMurderPlayer(Thief.Instance.Player.PlayerId, Thief.Instance.Player.PlayerId, false);
                        Thief.Instance.Player.clearAllTasks();
                    }

                    // Kill the victim (after becoming their role - so that no win is triggered for other teams)
                    if (result == MurderAttemptResult.PerformKill)
                    {
                        Thief.ThiefStealsRole(target.PlayerId);
                    }
                },
                () =>
                {
                    return Thief.Instance.Player != null && CachedPlayer.LocalPlayer.PlayerControl == Thief.Instance.Player &&
                           !CachedPlayer.LocalPlayer.Data.IsDead;
                },
                () => { return Thief.Instance.CurrentTarget != null && CachedPlayer.LocalPlayer.PlayerControl.CanMove; },
                () => { thiefKillButton.Timer = thiefKillButton.MaxTimer; },
                __instance.KillButton.graphic.sprite,
                CustomButton.ButtonPositions.upperRowRight,
                __instance,
                "ActionSecondary"
            );

            // Trapper Charges
            trapperChargesText = GameObject.Instantiate(trapperButton.actionButton.cooldownTimerText,
                trapperButton.actionButton.cooldownTimerText.transform.parent);
            trapperChargesText.text = "";
            trapperChargesText.enableWordWrapping = false;
            trapperChargesText.transform.localScale = Vector3.one * 0.5f;
            trapperChargesText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            zoomOutButton = new CustomButton(
                () => { Helpers.toggleZoom(); },
                () =>
                {
                    if (CachedPlayer.LocalPlayer.PlayerControl == null || !CachedPlayer.LocalPlayer.Data.IsDead ||
                        CachedPlayer.LocalPlayer.Data.Role.IsImpostor) return false;
                    var (playerCompleted, playerTotal) = TasksHandler.taskInfo(CachedPlayer.LocalPlayer.Data);
                    int numberOfLeftTasks = playerTotal - playerCompleted;
                    return numberOfLeftTasks <= 0 ||
                           !CustomOptions.FinishTasksBeforeHauntingOrZoomingOut;
                },
                () => { return true; },
                () => { return; },
                Helpers.loadSpriteFromResources("TheOtherRoles.Resources.MinusButton.png", 150f), // Invisible button!
                new Vector3(0.4f, 2.8f, 0),
                __instance,
                "ZoomOut"
            );
            zoomOutButton.Timer = 0f;


            hunterLighterButton = new CustomButton(
                () =>
                {
                    Hunter.lightActive.Add(CachedPlayer.LocalPlayer.PlayerId);
                    SoundEffectsManager.play("lighterLight");
                    CommonRpc.ShareTimer(Hunter.lightPunish);
                },
                () => { return HideNSeek.isHunter() && !CachedPlayer.LocalPlayer.Data.IsDead; },
                () => { return true; },
                () =>
                {
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
                () =>
                {
                    Hunter.lightActive.Remove(CachedPlayer.LocalPlayer.PlayerId);
                    hunterLighterButton.Timer = hunterLighterButton.MaxTimer;
                    SoundEffectsManager.play("lighterLight");
                }
            );

            hunterAdminTableButton = new CustomButton(
                () =>
                {
                    if (!MapBehaviour.Instance || !MapBehaviour.Instance.isActiveAndEnabled)
                    {
                        HudManager __instance = FastDestroyableSingleton<HudManager>.Instance;
                        __instance.InitMap();
                        MapBehaviour.Instance.ShowCountOverlay(allowedToMove: true, showLivePlayerPosition: true,
                            includeDeadBodies: false);
                    }

                    CachedPlayer.LocalPlayer.NetTransform.Halt(); // Stop current movement 
                    CommonRpc.ShareTimer(Hunter.AdminPunish);
                },
                () => { return HideNSeek.isHunter() && !CachedPlayer.LocalPlayer.Data.IsDead; },
                () => { return true; },
                () =>
                {
                    hunterAdminTableButton.Timer = hunterAdminTableButton.MaxTimer;
                    hunterAdminTableButton.isEffectActive = false;
                    hunterAdminTableButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Hacker.AdminButtonSprite,
                CustomButton.ButtonPositions.lowerRowCenter,
                __instance,
                "HunterAdmin",
                true,
                Hunter.AdminDuration,
                () =>
                {
                    hunterAdminTableButton.Timer = hunterAdminTableButton.MaxTimer;
                    if (MapBehaviour.Instance && MapBehaviour.Instance.isActiveAndEnabled)
                        MapBehaviour.Instance.Close();
                },
                false,
                "ADMIN"
            );

            hunterArrowButton = new CustomButton(
                () =>
                {
                    Hunter.arrowActive = true;
                    SoundEffectsManager.play("trackerTrackPlayer");
                    CommonRpc.ShareTimer(Hunter.ArrowPunish);
                },
                () => { return HideNSeek.isHunter() && !CachedPlayer.LocalPlayer.Data.IsDead; },
                () => { return true; },
                () =>
                {
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
                () =>
                {
                    Hunter.arrowActive = false;
                    hunterArrowButton.Timer = hunterArrowButton.MaxTimer;
                    SoundEffectsManager.play("trackerTrackPlayer");
                }
            );

            huntedShieldButton = new CustomButton(
                () =>
                {
                    CommonRpc.HuntedShield(CachedPlayer.LocalPlayer.PlayerId);
                    SoundEffectsManager.play("timemasterShield");

                    Hunted.shieldCount--;
                },
                () => { return HideNSeek.isHunted() && !CachedPlayer.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (huntedShieldCountText != null) huntedShieldCountText.text = $"{Hunted.shieldCount}";
                    return CachedPlayer.LocalPlayer.PlayerControl.CanMove && Hunted.shieldCount > 0;
                },
                () =>
                {
                    huntedShieldButton.Timer = huntedShieldButton.MaxTimer;
                    huntedShieldButton.isEffectActive = false;
                    huntedShieldButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                TimeMaster.TimeShieldButtonSprite,
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                "ActionQuaternary",
                true,
                Hunted.shieldDuration,
                () =>
                {
                    huntedShieldButton.Timer = huntedShieldButton.MaxTimer;
                    SoundEffectsManager.stop("timemasterShield");
                }
            );

            huntedShieldCountText = GameObject.Instantiate(huntedShieldButton.actionButton.cooldownTimerText,
                huntedShieldButton.actionButton.cooldownTimerText.transform.parent);
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