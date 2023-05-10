using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using static TheOtherRoles.TheOtherRoles;
using static TheOtherRoles.GameHistory;
using TheOtherRoles.Objects;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;
using UnityEngine;
using TheOtherRoles.CustomGameModes;
using AmongUs.GameOptions;
using InnerNet;
using TheOtherRoles.EnoFw;
using TheOtherRoles.EnoFw.Kernel;
using TheOtherRoles.EnoFw.Modules;
using TheOtherRoles.EnoFw.Roles.Crewmate;
using TheOtherRoles.EnoFw.Roles.Impostor;
using TheOtherRoles.EnoFw.Roles.Modifiers;
using TheOtherRoles.EnoFw.Roles.Neutral;
using TheOtherRoles.Modules;

namespace TheOtherRoles.Patches
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    public static class PlayerControlFixedUpdatePatch
    {
        // Update functions

        static void setBasePlayerOutlines()
        {
            foreach (PlayerControl target in CachedPlayer.AllPlayers)
            {
                if (target == null || target.cosmetics?.currentBodySprite?.BodySprite == null) continue;

                var isMorphedMorphling = target == Morphling.Instance.Player && Morphling.Instance.MorphTarget != null &&
                                          Morphling.Instance.MorphTimer > 0f;
                var hasVisibleShield = false;
                var color = Medic.ShieldColor;
                if (Camouflager.Instance.CamouflageTimer <= 0f && Medic.Instance.Shielded != null &&
                    ((target == Medic.Instance.Shielded && !isMorphedMorphling) ||
                     (isMorphedMorphling && Morphling.Instance.MorphTarget == Medic.Instance.Shielded)))
                {
                    hasVisibleShield = Medic.Instance.LocalPlayerCanSeeShield;
                    // Make shield invisible till after the next meeting if the option is set (the medic can already see the shield)
                    hasVisibleShield = hasVisibleShield && (Medic.Instance.MeetingAfterShielding || !Medic.Instance.ShowShieldAfterMeeting || Medic.Instance.IsLocalPlayer || Helpers.shouldShowGhostInfo());
                }

                if (Camouflager.Instance.CamouflageTimer <= 0f && TORMapOptions.firstKillPlayer != null &&
                    TORMapOptions.shieldFirstKill &&
                    ((target == TORMapOptions.firstKillPlayer && !isMorphedMorphling) || (isMorphedMorphling &&
                        Morphling.Instance.MorphTarget == TORMapOptions.firstKillPlayer)))
                {
                    hasVisibleShield = true;
                    color = TORMapOptions.ShieldColor;
                }

                if (hasVisibleShield)
                {
                    target.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", 1f);
                    target.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", color);
                }
                else
                {
                    target.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", 0f);
                }
            }
        }

        public static void bendTimeUpdate()
        {
            if (TimeMaster.Instance.IsRewinding)
            {
                if (localPlayerPositions.Count > 0)
                {
                    // Set position
                    var next = localPlayerPositions[0];
                    if (next.Item2 == true)
                    {
                        // Exit current vent if necessary
                        if (CachedPlayer.LocalPlayer.PlayerControl.inVent)
                        {
                            foreach (Vent vent in MapUtilities.CachedShipStatus.AllVents)
                            {
                                bool canUse;
                                bool couldUse;
                                vent.CanUse(CachedPlayer.LocalPlayer.Data, out canUse, out couldUse);
                                if (canUse)
                                {
                                    CachedPlayer.LocalPlayer.PlayerPhysics.RpcExitVent(vent.Id);
                                    vent.SetButtons(false);
                                }
                            }
                        }

                        // Set position
                        CachedPlayer.LocalPlayer.transform.position = next.Item1;
                    }
                    else if (localPlayerPositions.Any(x => x.Item2 == true))
                    {
                        CachedPlayer.LocalPlayer.transform.position = next.Item1;
                    }

                    if (SubmergedCompatibility.IsSubmerged)
                    {
                        SubmergedCompatibility.ChangeFloor(next.Item1.y > -7);
                    }

                    localPlayerPositions.RemoveAt(0);

                    if (localPlayerPositions.Count > 1)
                        localPlayerPositions
                            .RemoveAt(0); // Skip every second position to rewinde twice as fast, but never skip the last position
                }
                else
                {
                    TimeMaster.Instance.IsRewinding = false;
                    CachedPlayer.LocalPlayer.PlayerControl.moveable = true;
                }
            }
            else
            {
                while (localPlayerPositions.Count >= Mathf.Round(TimeMaster.Instance.RewindTime / Time.fixedDeltaTime))
                    localPlayerPositions.RemoveAt(localPlayerPositions.Count - 1);
                localPlayerPositions.Insert(0,
                    new Tuple<Vector3, bool>(CachedPlayer.LocalPlayer.transform.position,
                        CachedPlayer.LocalPlayer.PlayerControl.CanMove)); // CanMove = CanMove
            }
        }

        static void medicSetTarget()
        {
            if (!Medic.Instance.IsLocalPlayer || Medic.Instance.UsedShield) return;
            Medic.Instance.SetTarget(Helpers.setTarget());
        }

        static void shifterSetTarget()
        {
            if (Shifter.Instance.Player == null || Shifter.Instance.Player != CachedPlayer.LocalPlayer.PlayerControl) return;
            Shifter.Instance.CurrentTarget = Helpers.setTarget();
            if (Shifter.Instance.FutureShift == null) Helpers.setPlayerOutline(Shifter.Instance.CurrentTarget, Color.yellow);
        }


        static void morphlingSetTarget()
        {
            if (Morphling.Instance.Player == null || Morphling.Instance.Player != CachedPlayer.LocalPlayer.PlayerControl) return;
            Morphling.Instance.SetTarget(Helpers.setTarget());
        }

        static void sheriffSetTarget()
        {
            if (!Sheriff.Instance.IsLocalPlayer) return;
            Sheriff.Instance.SetTarget(Helpers.setTarget());
        }

        static void deputySetTarget()
        {
            if (!Deputy.Instance.IsLocalPlayer) return;
            Deputy.Instance.SetTarget(Helpers.setTarget());
        }

        public static void deputyCheckPromotion(bool isMeeting = false)
        {
            // If LocalPlayer is Deputy, the Sheriff is disconnected and Deputy promotion is enabled, then trigger promotion
            if (!Deputy.Instance.IsLocalPlayer) return;
            if (Deputy.Instance.PromotedWhen == 0 || Deputy.Instance.Player.Data.IsDead ||
                Deputy.Instance.PromotedWhen == 2 && !isMeeting) return;
            if (Sheriff.Instance.IsDeadOrDisconnected)
            {
                Deputy.DeputyPromotes();
            }
        }

        static void trackerSetTarget()
        {
            if (Tracker.Instance.Player == null || Tracker.Instance.Player != CachedPlayer.LocalPlayer.PlayerControl) return;
            Tracker.Instance.SetTarget(Helpers.setTarget());
        }

        static void detectiveUpdateFootPrints()
        {
            if (!Detective.Instance.IsLocalPlayer) return;

            Detective.Instance.Timer -= Time.fixedDeltaTime;
            if (Detective.Instance.Timer <= 0f)
            {
                Detective.Instance.Timer = Detective.Instance.FootprintInterval;
                foreach (PlayerControl player in CachedPlayer.AllPlayers)
                {
                    if (player != null && player != CachedPlayer.LocalPlayer.PlayerControl && !player.Data.IsDead &&
                        !player.inVent)
                    {
                        FootprintHolder.Instance.MakeFootprint(player);
                    }
                }
            }
        }

        static void vampireSetTarget()
        {
            if (Vampire.Instance.Player == null || Vampire.Instance.Player != CachedPlayer.LocalPlayer.PlayerControl) return;

            PlayerControl target = null;
            if (Spy.Instance.Player != null || Sidekick.Instance.WasSpy || Jackal.Instance.WasSpy)
            {
                if (Spy.Instance.ImpostorsCanKillAnyone)
                {
                    target = Helpers.setTarget(false, true);
                }
                else
                {
                    target = Helpers.setTarget(true, true,
                        new List<PlayerControl>()
                        {
                            Spy.Instance.Player, Sidekick.Instance.WasTeamRed ? Sidekick.Instance.Player : null,
                            Jackal.Instance.WasTeamRed ? Jackal.Instance.Player : null
                        });
                }
            }
            else
            {
                target = Helpers.setTarget(true, true,
                    new List<PlayerControl>()
                        { Sidekick.Instance.WasImpostor ? Sidekick.Instance.Player : null, Jackal.Instance.WasImpostor ? Jackal.Instance.Player : null });
            }

            bool targetNearGarlic = false;
            if (target != null)
            {
                foreach (Garlic garlic in Garlic.garlics)
                {
                    if (Vector2.Distance(garlic.garlic.transform.position, target.transform.position) <= 1.91f)
                    {
                        targetNearGarlic = true;
                    }
                }
            }

            Vampire.Instance.TargetNearGarlic = targetNearGarlic;
            Vampire.Instance.SetTarget(target);
        }

        // A voir si fini.
        static void whispererSetTarget()
        {
            if (Whisperer.Instance.Player == null || Whisperer.Instance.Player != CachedPlayer.LocalPlayer.PlayerControl) return;

            if (Whisperer.Instance.WhisperVictim != null &&
                (Whisperer.Instance.WhisperVictim.Data.Disconnected || Whisperer.Instance.WhisperVictim.Data.IsDead))
            {
                Whisperer.Instance.ResetWhisper();
            }

            if (Whisperer.Instance.WhisperVictim == null)
            {
                Whisperer.Instance.SetTarget(Helpers.setTarget(false, true));
            }
            else
            {
                Whisperer.Instance.WhisperVictimTarget = Helpers.setTarget(targetingPlayer: Whisperer.Instance.WhisperVictim);
                Helpers.setPlayerOutline(Whisperer.Instance.WhisperVictimTarget, Whisperer.Instance.Color);
            }
        }

        static void undertakerSetTarget()
        {
            if (Undertaker.Instance.Player == null || Undertaker.Instance.Player != CachedPlayer.LocalPlayer.PlayerControl) return;
            
            if (Undertaker.Instance.CurrentDeadTarget != null)
            {
                Helpers.setDeadPlayerOutline(Undertaker.Instance.CurrentDeadTarget, null);
            }

            if (Undertaker.Instance.DraggedBody == null)
            {
                Undertaker.Instance.CurrentDeadTarget = Helpers.setDeadTarget(Undertaker.Instance.RealDragDistance);
                Helpers.setDeadPlayerOutline(Undertaker.Instance.CurrentDeadTarget, Undertaker.Instance.Color);
            }
        }

        static void undertakerCanDropTarget()
        {
            var component = Undertaker.Instance.DraggedBody;
            
            Undertaker.Instance.CanDropBody = false;
            
            if (component == null) return;
            
            if (component.enabled && Vector2.Distance(Undertaker.Instance.Player.GetTruePosition(), component.TruePosition) <= Undertaker.Instance.Player.MaxReportDistance && !PhysicsHelpers.AnythingBetween(CachedPlayer.LocalPlayer.PlayerControl.GetTruePosition(), component.TruePosition, Constants.ShipAndObjectsMask, false))
            {
                Undertaker.Instance.CanDropBody = true;
            }
        }


        static void undertakerUpdate()
        {
            var undertakerPlayer = Undertaker.Instance.Player;
            var bodyComponent = Undertaker.Instance.DraggedBody;

            if (undertakerPlayer == null || bodyComponent == null) return;

            var undertakerPos = undertakerPlayer.transform.position;
            var bodyLastPos = bodyComponent.transform.position;

            var direction = undertakerPlayer.gameObject.GetComponent<Rigidbody2D>().velocity.normalized;
            
            var newBodyPos = direction == Vector2.zero
                ? bodyLastPos
                : undertakerPos - (Vector3)(direction * (2f / 3f)) + (Vector3)bodyComponent.myCollider.offset;
            newBodyPos.z = undertakerPos.z + 0.005f;
            
            bodyComponent.transform.position.Set(newBodyPos.x, newBodyPos.y, newBodyPos.z);

            if (direction == Direction.right) newBodyPos += new Vector3(0.3f, 0, 0);
            if (direction == Direction.up) newBodyPos += new Vector3(0.15f, 0.2f, 0);
            if (direction == Direction.down) newBodyPos += new Vector3(0.15f, -0.2f, 0);
            if (direction == Direction.upleft) newBodyPos += new Vector3(0, 0.1f, 0);
            if (direction == Direction.upright) newBodyPos += new Vector3(0.3f, 0.1f, 0);
            if (direction == Direction.downright) newBodyPos += new Vector3(0.3f, -0.2f, 0);
            if (direction == Direction.downleft) newBodyPos += new Vector3(0f, -0.2f, 0);

            if (PhysicsHelpers.AnythingBetween(
                    undertakerPlayer.GetTruePosition(),
                    newBodyPos,
                    Constants.ShipAndObjectsMask,
                    false
                ))
            {
                newBodyPos = new Vector3(undertakerPos.x, undertakerPos.y, bodyLastPos.z);
            }

            if (undertakerPlayer.Data.IsDead)
            {
                if (undertakerPlayer.AmOwner)
                {
                    Undertaker.DropBody(newBodyPos.x, newBodyPos.y, newBodyPos.z);
                }

                return;
            }

            bodyComponent.transform.position = newBodyPos;

            if (!undertakerPlayer.AmOwner) return;

            Helpers.setDeadPlayerOutline(bodyComponent, Color.green);
        }

        static void jackalSetTarget()
        {
            if (Jackal.Instance.Player == null || Jackal.Instance.Player != CachedPlayer.LocalPlayer.PlayerControl) return;
            var untargetablePlayers = new List<PlayerControl>();
            if (Jackal.Instance.CanCreateSidekickFromImpostor)
            {
                // Only exclude sidekick from beeing targeted if the jackal can create sidekicks from impostors
                if (Sidekick.Instance.Player != null) untargetablePlayers.Add(Sidekick.Instance.Player);
            }

            if (Mini.Instance.Player != null && !Mini.Instance.IsGrownUp)
                untargetablePlayers.Add(Mini.Instance.Player); // Exclude Jackal from targeting the Mini unless it has grown up
            Jackal.Instance.SetTarget(Helpers.setTarget(untargetablePlayers: untargetablePlayers));
        }

        static void sidekickSetTarget()
        {
            if (Sidekick.Instance.Player == null || Sidekick.Instance.Player != CachedPlayer.LocalPlayer.PlayerControl) return;
            var untargetablePlayers = new List<PlayerControl>();
            if (Jackal.Instance.Player != null) untargetablePlayers.Add(Jackal.Instance.Player);
            if (Mini.Instance.Player != null && !Mini.Instance.IsGrownUp)
                untargetablePlayers.Add(Mini.Instance.Player); // Exclude Sidekick from targeting the Mini unless it has grown up
            Sidekick.Instance.CurrentTarget = Helpers.setTarget(untargetablePlayers: untargetablePlayers);
            if (Sidekick.Instance.CanKill) Helpers.setPlayerOutline(Sidekick.Instance.CurrentTarget, Palette.ImpostorRed);
        }

        static void sidekickCheckPromotion()
        {
            // If LocalPlayer is Sidekick, the Jackal is disconnected and Sidekick promotion is enabled, then trigger promotion
            if (Sidekick.Instance.Player == null || Sidekick.Instance.Player != CachedPlayer.LocalPlayer.PlayerControl) return;
            if (Sidekick.Instance.Player.Data.IsDead || !Sidekick.Instance.PromotesToJackal) return;
            if (Jackal.Instance.Player == null || Jackal.Instance.Player.Data.Disconnected == true)
            {
                Sidekick.SidekickPromotes();
            }
        }

        static void eraserSetTarget()
        {
            if (Eraser.Instance.Player == null || Eraser.Instance.Player != CachedPlayer.LocalPlayer.PlayerControl) return;

            List<PlayerControl> untargetables = new List<PlayerControl>();
            if (Spy.Instance.Player != null) untargetables.Add(Spy.Instance.Player);
            if (Sidekick.Instance.WasTeamRed) untargetables.Add(Sidekick.Instance.Player);
            if (Jackal.Instance.WasTeamRed) untargetables.Add(Jackal.Instance.Player);
            Eraser.Instance.CurrentTarget = Helpers.setTarget(onlyCrewmates: !Eraser.Instance.CanEraseAnyone,
                untargetablePlayers: Eraser.Instance.CanEraseAnyone ? new List<PlayerControl>() : untargetables);
            Helpers.setPlayerOutline(Eraser.Instance.CurrentTarget, Eraser.Instance.Color);
        }

        static void deputyUpdate()
        {
            if (CachedPlayer.LocalPlayer.PlayerControl == null ||
                !Deputy.Instance.HandcuffedKnows.ContainsKey(CachedPlayer.LocalPlayer.PlayerId)) return;

            if (Deputy.Instance.HandcuffedKnows[CachedPlayer.LocalPlayer.PlayerId] <= 0)
            {
                Deputy.Instance.HandcuffedKnows.Remove(CachedPlayer.LocalPlayer.PlayerId);
                // Resets the buttons
                Deputy.Instance.SetHandcuffedKnows(false);

                // Ghost info
                GhostInfos.ShareGhostInfo(GhostInfos.Types.HandcuffOver,
                    Rpc.Serialize(new Tuple<byte>(CachedPlayer.LocalPlayer.PlayerId)));
            }
        }

        static void engineerUpdate()
        {
            bool neutralHighlight = Engineer.Instance.HighlightVentsForNeutrals && Helpers.isNeutral(CachedPlayer.LocalPlayer.PlayerControl);
            bool impostorHighlight = Engineer.Instance.HighlightVentsForImpostors && CachedPlayer.LocalPlayer.Data.Role.IsImpostor;
            if ((neutralHighlight || impostorHighlight) && MapUtilities.CachedShipStatus?.AllVents != null)
            {
                foreach (Vent vent in MapUtilities.CachedShipStatus.AllVents)
                {
                    try
                    {
                        if (vent?.myRend?.material != null)
                        {
                            if (Engineer.Instance.HasPlayer && Engineer.Instance.Player.inVent)
                            {
                                vent.myRend.material.SetFloat("_Outline", 1f);
                                vent.myRend.material.SetColor("_OutlineColor", Engineer.Instance.Color);
                            }
                            else if (vent.myRend.material.GetColor("_AddColor") != Color.red)
                            {
                                vent.myRend.material.SetFloat("_Outline", 0);
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }

        static void impostorSetTarget()
        {
            if (!CachedPlayer.LocalPlayer.Data.Role.IsImpostor || !CachedPlayer.LocalPlayer.PlayerControl.CanMove ||
                CachedPlayer.LocalPlayer.Data.IsDead)
            {
                // !isImpostor || !canMove || isDead
                FastDestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(null);
                return;
            }

            PlayerControl target = null;
            if (Spy.Instance.Player != null || Sidekick.Instance.WasSpy || Jackal.Instance.WasSpy)
            {
                if (Spy.Instance.ImpostorsCanKillAnyone)
                {
                    target = Helpers.setTarget(false, true);
                }
                else
                {
                    target = Helpers.setTarget(true, true,
                        new List<PlayerControl>()
                        {
                            Spy.Instance.Player, Sidekick.Instance.WasTeamRed ? Sidekick.Instance.Player : null,
                            Jackal.Instance.WasTeamRed ? Jackal.Instance.Player : null
                        });
                }
            }
            else
            {
                target = Helpers.setTarget(true, true,
                    new List<PlayerControl>()
                        { Sidekick.Instance.WasImpostor ? Sidekick.Instance.Player : null, Jackal.Instance.WasImpostor ? Jackal.Instance.Player : null });
            }

            FastDestroyableSingleton<HudManager>.Instance.KillButton
                .SetTarget(target); // Includes Helpers.setPlayerOutline(target, Palette.ImpstorRed);
        }

        static void warlockSetTarget()
        {
            if (Warlock.Instance.Player == null || Warlock.Instance.Player != CachedPlayer.LocalPlayer.PlayerControl) return;

            if (Warlock.Instance.CurseVictim != null &&
                (Warlock.Instance.CurseVictim.Data.Disconnected || Warlock.Instance.CurseVictim.Data.IsDead))
            {
                // If the cursed victim is disconnected or dead reset the curse so a new curse can be applied
                Warlock.Instance.ResetCurse();
            }

            if (Warlock.Instance.CurseVictim == null)
            {
                Warlock.Instance.SetTarget(Helpers.setTarget());
            }
            else
            {
                Warlock.Instance.CurseVictimTarget = Helpers.setTarget(targetingPlayer: Warlock.Instance.CurseVictim);
                Helpers.setPlayerOutline(Warlock.Instance.CurseVictimTarget, Warlock.Instance.Color);
            }
        }

        static void ninjaUpdate()
        {
            if (Ninja.Instance.IsInvisible && Ninja.Instance.InvisibilityTimer <= 0 && Ninja.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl)
            {
                Ninja.SetInvisible(Ninja.Instance.Player.PlayerId, true);
            }

            if (Ninja.Instance.Arrow?.arrow != null)
            {
                if (Ninja.Instance.Player == null || Ninja.Instance.Player != CachedPlayer.LocalPlayer.PlayerControl ||
                    !Ninja.Instance.KnowsTargetLocation)
                {
                    Ninja.Instance.Arrow.arrow.SetActive(false);
                    return;
                }

                if (Ninja.Instance.MarkedTarget != null && !CachedPlayer.LocalPlayer.Data.IsDead)
                {
                    bool trackedOnMap = !Ninja.Instance.MarkedTarget.Data.IsDead;
                    Vector3 position = Ninja.Instance.MarkedTarget.transform.position;
                    if (!trackedOnMap)
                    {
                        // Check for dead body
                        DeadBody body = UnityEngine.Object.FindObjectsOfType<DeadBody>()
                            .FirstOrDefault(b => b.ParentId == Ninja.Instance.MarkedTarget.PlayerId);
                        if (body != null)
                        {
                            trackedOnMap = true;
                            position = body.transform.position;
                        }
                    }

                    Ninja.Instance.Arrow.Update(position);
                    Ninja.Instance.Arrow.arrow.SetActive(trackedOnMap);
                }
                else
                {
                    Ninja.Instance.Arrow.arrow.SetActive(false);
                }
            }
        }

        static void trackerUpdate()
        {
            // Handle player tracking
            if (Tracker.Instance.Arrow != null && Tracker.Instance.Arrow.arrow != null)
            {
                if (Tracker.Instance.Player == null || CachedPlayer.LocalPlayer.PlayerControl != Tracker.Instance.Player)
                {
                    Tracker.Instance.Arrow.arrow.SetActive(false);
                    return;
                }

                if (Tracker.Instance.Player != null && Tracker.Instance.Tracked != null &&
                    CachedPlayer.LocalPlayer.PlayerControl == Tracker.Instance.Player && !Tracker.Instance.Player.Data.IsDead)
                {
                    Tracker.Instance.TimeUntilUpdate -= Time.fixedDeltaTime;

                    if (Tracker.Instance.TimeUntilUpdate <= 0f)
                    {
                        bool trackedOnMap = !Tracker.Instance.Tracked.Data.IsDead;
                        Vector3 position = Tracker.Instance.Tracked.transform.position;
                        if (!trackedOnMap)
                        {
                            // Check for dead body
                            DeadBody body = UnityEngine.Object.FindObjectsOfType<DeadBody>()
                                .FirstOrDefault(b => b.ParentId == Tracker.Instance.Tracked.PlayerId);
                            if (body != null)
                            {
                                trackedOnMap = true;
                                position = body.transform.position;
                            }
                        }

                        Tracker.Instance.Arrow.Update(position);
                        Tracker.Instance.Arrow.arrow.SetActive(trackedOnMap);
                        Tracker.Instance.TimeUntilUpdate = Tracker.Instance.UpdateArrowInterval;
                    }
                    else
                    {
                        Tracker.Instance.Arrow.Update();
                    }
                }
            }

            // Handle corpses tracking
            if (Tracker.Instance.Player != null && Tracker.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl &&
                Tracker.Instance.CorpsesTrackingTimer >= 0f && !Tracker.Instance.Player.Data.IsDead)
            {
                bool arrowsCountChanged = Tracker.Instance.Arrows.Count != Tracker.Instance.DeadBodyPositions.Count;
                int index = 0;

                if (arrowsCountChanged)
                {
                    foreach (Arrow arrow in Tracker.Instance.Arrows) UnityEngine.Object.Destroy(arrow.arrow);
                    Tracker.Instance.Arrows.Clear();
                }

                foreach (Vector3 position in Tracker.Instance.DeadBodyPositions)
                {
                    if (arrowsCountChanged)
                    {
                        Tracker.Instance.Arrows.Add(new Arrow(Tracker.Instance.Color));
                        Tracker.Instance.Arrows[index].arrow.SetActive(true);
                    }

                    if (Tracker.Instance.Arrows[index] != null) Tracker.Instance.Arrows[index].Update(position);
                    index++;
                }
            }
            else if (Tracker.Instance.Arrows.Count > 0)
            {
                foreach (Arrow arrow in Tracker.Instance.Arrows) UnityEngine.Object.Destroy(arrow.arrow);
                Tracker.Instance.Arrows.Clear();
            }
        }

        public static void playerSizeUpdate(PlayerControl p)
        {
            // Set default player size
            CircleCollider2D collider = p.Collider.CastFast<CircleCollider2D>();

            p.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            collider.radius = Mini.DefaultColliderRadius;
            collider.offset = Mini.DefaultColliderOffset * Vector2.down;

            // Set adapted player size to Mini and Morphling
            if (Mini.Instance.Player == null || Camouflager.Instance.CamouflageTimer > 0f ||
                Mini.Instance.Player == Morphling.Instance.Player && Morphling.Instance.MorphTimer > 0) return;

            float growingProgress = Mini.Instance.GrowingProgress();
            float scale = growingProgress * 0.35f + 0.35f;
            float
                correctedColliderRadius =
                    Mini.DefaultColliderRadius * 0.7f /
                    scale; // scale / 0.7f is the factor by which we decrease the player size, hence we need to increase the collider size by 0.7f / scale

            if (p == Mini.Instance.Player)
            {
                p.transform.localScale = new Vector3(scale, scale, 1f);
                collider.radius = correctedColliderRadius;
            }

            if (Morphling.Instance.Player != null && p == Morphling.Instance.Player && Morphling.Instance.MorphTarget == Mini.Instance.Player &&
                Morphling.Instance.MorphTimer > 0f)
            {
                p.transform.localScale = new Vector3(scale, scale, 1f);
                collider.radius = correctedColliderRadius;
            }
        }

        public static void updatePlayerInfo()
        {
            Vector3 colorBlindTextMeetingInitialLocalPos = new Vector3(0.3384f, -0.16666f, -0.01f);
            Vector3 colorBlindTextMeetingInitialLocalScale = new Vector3(0.9f, 1f, 1f);
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                // Colorblind Text in Meeting
                PlayerVoteArea playerVoteArea =
                    MeetingHud.Instance?.playerStates?.FirstOrDefault(x => x.TargetPlayerId == p.PlayerId);
                if (playerVoteArea != null && playerVoteArea.ColorBlindName.gameObject.active)
                {
                    playerVoteArea.ColorBlindName.transform.localPosition =
                        colorBlindTextMeetingInitialLocalPos + new Vector3(0f, 0.4f, 0f);
                    playerVoteArea.ColorBlindName.transform.localScale = colorBlindTextMeetingInitialLocalScale * 0.8f;
                }

                // Colorblind Text During the round
                if (p.cosmetics.colorBlindText != null && p.cosmetics.showColorBlindText &&
                    p.cosmetics.colorBlindText.gameObject.active)
                {
                    p.cosmetics.colorBlindText.transform.localPosition = new Vector3(0, -1f, 0f);
                }

                p.cosmetics.nameText.transform.parent
                    .SetLocalZ(-0.0001f); // This moves both the name AND the colorblindtext behind objects (if the player is behind the object), like the rock on polus

                if (CustomGuid.IsDevMode ||
                    (Lawyer.Instance.KnowsTargetRole && CachedPlayer.LocalPlayer.PlayerControl == Lawyer.Instance.Player &&
                     p == Lawyer.Instance.Target) || p == CachedPlayer.LocalPlayer.PlayerControl ||
                    CachedPlayer.LocalPlayer.Data.IsDead)
                {
                    Transform playerInfoTransform = p.cosmetics.nameText.transform.parent.FindChild("Info");
                    TMPro.TextMeshPro playerInfo = playerInfoTransform != null
                        ? playerInfoTransform.GetComponent<TMPro.TextMeshPro>()
                        : null;
                    if (playerInfo == null)
                    {
                        playerInfo = UnityEngine.Object.Instantiate(p.cosmetics.nameText,
                            p.cosmetics.nameText.transform.parent);
                        playerInfo.transform.localPosition += Vector3.up * 0.225f;
                        playerInfo.fontSize *= 0.75f;
                        playerInfo.gameObject.name = "Info";
                        playerInfo.color = playerInfo.color.SetAlpha(1f);
                    }

                    Transform meetingInfoTransform = playerVoteArea != null
                        ? playerVoteArea.NameText.transform.parent.FindChild("Info")
                        : null;
                    TMPro.TextMeshPro meetingInfo = meetingInfoTransform != null
                        ? meetingInfoTransform.GetComponent<TMPro.TextMeshPro>()
                        : null;
                    if (meetingInfo == null && playerVoteArea != null)
                    {
                        meetingInfo = UnityEngine.Object.Instantiate(playerVoteArea.NameText,
                            playerVoteArea.NameText.transform.parent);
                        meetingInfo.transform.localPosition += Vector3.down * 0.2f;
                        meetingInfo.fontSize *= 0.60f;
                        meetingInfo.gameObject.name = "Info";
                    }

                    // Set player name higher to align in middle
                    if (meetingInfo != null && playerVoteArea != null)
                    {
                        var playerName = playerVoteArea.NameText;
                        playerName.transform.localPosition = new Vector3(0.3384f, 0.0311f, -0.1f);
                    }

                    var (tasksCompleted, tasksTotal) = TasksHandler.taskInfo(p.Data);
                    string roleNames = RoleInfo.GetRolesString(p, true, false);
                    string roleText = RoleInfo.GetRolesString(p, true, TORMapOptions.ghostsSeeModifier);
                    string taskInfo = tasksTotal > 0 ? $"<color=#FAD934FF>({tasksCompleted}/{tasksTotal})</color>" : "";

                    string playerInfoText = "";
                    string meetingInfoText = "";
                    if (p == CachedPlayer.LocalPlayer.PlayerControl)
                    {
                        if (p.Data.IsDead) roleNames = roleText;
                        playerInfoText = $"{roleNames}";
                        if (p == Swapper.Instance.Player)
                            playerInfoText = $"{roleNames}" + Helpers.cs(Swapper.Instance.Color, $" ({Swapper.Instance.Charges})");
                        if (HudManager.Instance.TaskPanel != null)
                        {
                            TMPro.TextMeshPro tabText = HudManager.Instance.TaskPanel.tab.transform
                                .FindChild("TabText_TMP").GetComponent<TMPro.TextMeshPro>();
                            tabText.SetText($"Tasks {taskInfo}");
                        }

                        meetingInfoText = $"{roleNames} {taskInfo}".Trim();
                    }
                    else if (TORMapOptions.ghostsSeeRoles && TORMapOptions.ghostsSeeInformation)
                    {
                        playerInfoText = $"{roleText} {taskInfo}".Trim();
                        meetingInfoText = playerInfoText;
                    }
                    else if (TORMapOptions.ghostsSeeInformation)
                    {
                        playerInfoText = $"{taskInfo}".Trim();
                        meetingInfoText = playerInfoText;
                    }
                    else if (TORMapOptions.ghostsSeeRoles || (Lawyer.Instance.KnowsTargetRole &&
                                                              CachedPlayer.LocalPlayer.PlayerControl == Lawyer.Instance.Player &&
                                                              p == Lawyer.Instance.Target))
                    {
                        playerInfoText = $"{roleText}";
                        meetingInfoText = playerInfoText;
                    }

                    playerInfo.text = playerInfoText;
                    playerInfo.gameObject.SetActive(p.Visible);
                    if (meetingInfo != null)
                        meetingInfo.text = MeetingHud.Instance.state == MeetingHud.VoteStates.Results
                            ? ""
                            : meetingInfoText;
                }
            }
        }

        public static void securityGuardSetTarget()
        {
            if (SecurityGuard.Instance.Player == null ||
                SecurityGuard.Instance.Player != CachedPlayer.LocalPlayer.PlayerControl ||
                MapUtilities.CachedShipStatus == null || MapUtilities.CachedShipStatus.AllVents == null) return;

            Vent target = null;
            Vector2 truePosition = CachedPlayer.LocalPlayer.PlayerControl.GetTruePosition();
            float closestDistance = float.MaxValue;
            for (int i = 0; i < MapUtilities.CachedShipStatus.AllVents.Length; i++)
            {
                Vent vent = MapUtilities.CachedShipStatus.AllVents[i];
                if (vent.gameObject.name.StartsWith("JackInTheBoxVent_") ||
                    vent.gameObject.name.StartsWith("SealedVent_") ||
                    vent.gameObject.name.StartsWith("FutureSealedVent_")) continue;
                if (SubmergedCompatibility.IsSubmerged && vent.Id == 9)
                    continue; // cannot seal submergeds exit only vent!
                float distance = Vector2.Distance(vent.transform.position, truePosition);
                if (distance <= vent.UsableDistance && distance < closestDistance)
                {
                    closestDistance = distance;
                    target = vent;
                }
            }

            SecurityGuard.Instance.VentTarget = target;
        }

        public static void securityGuardUpdate()
        {
            if (SecurityGuard.Instance.Player == null ||
                CachedPlayer.LocalPlayer.PlayerControl != SecurityGuard.Instance.Player ||
                SecurityGuard.Instance.Player.Data.IsDead) return;
            var (playerCompleted, _) = TasksHandler.taskInfo(SecurityGuard.Instance.Player.Data);
            if (playerCompleted == SecurityGuard.Instance.RechargedTasks)
            {
                SecurityGuard.Instance.RechargedTasks += SecurityGuard.Instance.CamRechargeTaskNumber;
                if (SecurityGuard.Instance.CamMaxCharges > SecurityGuard.Instance.UsedCharges) SecurityGuard.Instance.UsedCharges++;
            }
        }

        public static void arsonistSetTarget()
        {
            if (Arsonist.Instance.Player == null || Arsonist.Instance.Player != CachedPlayer.LocalPlayer.PlayerControl) return;
            List<PlayerControl> untargetables;
            if (Arsonist.Instance.DouseTarget != null)
            {
                untargetables = new();
                foreach (CachedPlayer cachedPlayer in CachedPlayer.AllPlayers)
                {
                    if (cachedPlayer.PlayerId != Arsonist.Instance.DouseTarget.PlayerId)
                    {
                        untargetables.Add(cachedPlayer);
                    }
                }
            }
            else untargetables = Arsonist.Instance.DousedPlayers;

            Arsonist.Instance.CurrentTarget = Helpers.setTarget(untargetablePlayers: untargetables);
            if (Arsonist.Instance.CurrentTarget != null) Helpers.setPlayerOutline(Arsonist.Instance.CurrentTarget, Arsonist.Instance.Color);
        }

        static void snitchUpdate()
        {
            if (Snitch.Instance.Player == null || !Snitch.Instance.NeedsUpdate) return;

            bool snitchIsDead = Snitch.Instance.Player.Data.IsDead;
            var (playerCompleted, playerTotal) = TasksHandler.taskInfo(Snitch.Instance.Player.Data);

            if (playerTotal == 0) return;

            PlayerControl local = CachedPlayer.LocalPlayer.PlayerControl;

            int numberOfTasks = playerTotal - playerCompleted;

            if (Snitch.Instance.IsRevealed && !Snitch.Instance.ArrowTargetsNone)
            {
                if (!Snitch.Instance.ArrowTargetsEvil &&
                    ((Snitch.Instance.InfoTargetEvilPlayers && Helpers.isEvil(local)) ||
                     (Snitch.Instance.InfoTargetKillingPlayers && Helpers.isKiller(local))))
                {
                    if (Snitch.Instance.Arrows.Count == 0) Snitch.Instance.Arrows.Add(new Arrow(Color.blue));

                    if (Snitch.Instance.Arrows.Count != 0 && Snitch.Instance.Arrows[0] != null)
                    {
                        Snitch.Instance.Arrows[0].arrow.SetActive(true);
                        Snitch.Instance.Arrows[0].Update(Snitch.Instance.Player.transform.position);
                    }
                }
                else if (!Snitch.Instance.ArrowTargetsSnitch && !snitchIsDead &&
                         CachedPlayer.LocalPlayer.PlayerControl == Snitch.Instance.Player &&
                         playerTotal - playerCompleted == 0)
                {
                    int arrowIndex = 0;

                    foreach (PlayerControl p in CachedPlayer.AllPlayers)
                    {
                        if (!p.Data.IsDead && ((Snitch.Instance.InfoTargetEvilPlayers && Helpers.isEvil(p)) ||
                                               (Snitch.Instance.InfoTargetKillingPlayers && Helpers.isKiller(p))))
                        {
                            if (arrowIndex >= Snitch.Instance.Arrows.Count)
                            {
                                Snitch.Instance.Arrows.Add(new Arrow(Palette.ImpostorRed));
                            }

                            if (arrowIndex < Snitch.Instance.Arrows.Count && Snitch.Instance.Arrows[arrowIndex] != null)
                            {
                                Snitch.Instance.Arrows[arrowIndex].arrow.SetActive(true);
                                Snitch.Instance.Arrows[arrowIndex].Update(p.transform.position, Palette.ImpostorRed);
                                // Snitch.localArrows[arrowIndex].Update(p.transform.position, (arrowForTeamJackal && Snitch.teamJackalUseDifferentArrowColor ? Jackal.Instance.Color : Palette.ImpostorRed));
                            }

                            arrowIndex++;
                        }
                    }
                }
            }

            if (Snitch.Instance.IsRevealed && ((Snitch.Instance.InfoTargetEvilPlayers && Helpers.isEvil(local)) ||
                                      (Snitch.Instance.InfoTargetKillingPlayers && Helpers.isKiller(local))))
            {
                if (Snitch.Instance.Text == null)
                {
                    Snitch.Instance.Text = GameObject.Instantiate(
                        FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText,
                        FastDestroyableSingleton<HudManager>.Instance.transform);
                    Snitch.Instance.Text.enableWordWrapping = false;
                    Snitch.Instance.Text.transform.localScale = Vector3.one * 0.75f;
                    Snitch.Instance.Text.transform.localPosition += new Vector3(0f, 1.8f, -69f);
                    Snitch.Instance.Text.gameObject.SetActive(true);
                }
                else
                {
                    if (playerCompleted == playerTotal)
                    {
                        Snitch.Instance.Text.text = $"The Snitch know who u are.";
                    }
                    else
                    {
                        Snitch.Instance.Text.text = $"The Snitch know who u are.";
                        Snitch.Instance.Text.text += $" ({playerCompleted} / {playerTotal})";
                    }

                    if (snitchIsDead) Snitch.Instance.Text.text = $"Snitch is dead !";
                }
            }

            if (snitchIsDead)
            {
                if (MeetingHud.Instance == null) Snitch.Instance.NeedsUpdate = false;
                return;
            }

            if (numberOfTasks <= Snitch.Instance.LeftTasksForReveal) Snitch.Instance.IsRevealed = true;
        }

        static void bountyHunterUpdate()
        {
            if (BountyHunter.Instance.Player == null ||
                CachedPlayer.LocalPlayer.PlayerControl != BountyHunter.Instance.Player) return;

            if (BountyHunter.Instance.Player.Data.IsDead)
            {
                if (BountyHunter.Instance.Arrow != null && BountyHunter.Instance.Arrow.arrow != null)
                    UnityEngine.Object.Destroy(BountyHunter.Instance.Arrow.arrow);
                BountyHunter.Instance.Arrow = null;
                if (BountyHunter.Instance.CooldownText != null && BountyHunter.Instance.CooldownText.gameObject != null)
                    UnityEngine.Object.Destroy(BountyHunter.Instance.CooldownText.gameObject);
                BountyHunter.Instance.CooldownText = null;
                BountyHunter.Instance.Bounty = null;
                foreach (PoolablePlayer p in TORMapOptions.playerIcons.Values)
                {
                    if (p != null && p.gameObject != null) p.gameObject.SetActive(false);
                }

                return;
            }

            BountyHunter.Instance.ArrowUpdateTimer -= Time.fixedDeltaTime;
            BountyHunter.Instance.BountyUpdateTimer -= Time.fixedDeltaTime;

            if (BountyHunter.Instance.Bounty == null || BountyHunter.Instance.BountyUpdateTimer <= 0f)
            {
                // Set new bounty
                BountyHunter.Instance.ArrowUpdateTimer = 0f; // Force arrow to update
                BountyHunter.Instance.BountyUpdateTimer = BountyHunter.Instance.BountyDuration;
                var possibleTargets = new List<PlayerControl>();

                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (p.Data.IsDead || p.Data.Disconnected) continue;
                    if (p.Data.Role.IsImpostor || p == Spy.Instance.Player) continue;
                    if (TORMapOptions.shieldFirstKill && TORMapOptions.firstKillPlayer == p) continue;
                    if (Jackal.Instance.Player == p && Jackal.Instance.WasTeamRed) continue;
                    if (Sidekick.Instance.Player == p && Sidekick.Instance.WasTeamRed) continue;
                    if (BountyHunter.Instance.Player.GetPartner() == p) continue;
                    if (p == Mini.Instance.Player && !Mini.Instance.IsGrownUp) continue;
                    if (p == BountyHunter.Instance.Bounty) continue;

                    possibleTargets.Add(p);
                }
                BountyHunter.Instance.Bounty = null;

                BountyHunter.Instance.Bounty = possibleTargets[TheOtherRoles.Rnd.Next(0, possibleTargets.Count)];
                if (BountyHunter.Instance.Bounty == null) return;

                // Ghost Info
                GhostInfos.ShareGhostInfo(GhostInfos.Types.BountyTarget,
                    Rpc.Serialize(new Tuple<byte>(BountyHunter.Instance.Bounty.PlayerId)));

                // Show poolable player
                if (FastDestroyableSingleton<HudManager>.Instance != null &&
                    FastDestroyableSingleton<HudManager>.Instance.UseButton != null)
                {
                    foreach (PoolablePlayer pp in TORMapOptions.playerIcons.Values) pp.gameObject.SetActive(false);
                    if (TORMapOptions.playerIcons.ContainsKey(BountyHunter.Instance.Bounty.PlayerId) &&
                        TORMapOptions.playerIcons[BountyHunter.Instance.Bounty.PlayerId].gameObject != null)
                        TORMapOptions.playerIcons[BountyHunter.Instance.Bounty.PlayerId].gameObject.SetActive(true);
                }
            }

            // Hide in meeting
            if (MeetingHud.Instance && TORMapOptions.playerIcons.ContainsKey(BountyHunter.Instance.Bounty.PlayerId) &&
                TORMapOptions.playerIcons[BountyHunter.Instance.Bounty.PlayerId].gameObject != null)
                TORMapOptions.playerIcons[BountyHunter.Instance.Bounty.PlayerId].gameObject.SetActive(false);

            // Update Cooldown Text
            if (BountyHunter.Instance.CooldownText != null)
            {
                BountyHunter.Instance.CooldownText.text = Mathf
                    .CeilToInt(Mathf.Clamp(BountyHunter.Instance.BountyUpdateTimer, 0, BountyHunter.Instance.BountyDuration)).ToString();
                BountyHunter.Instance.CooldownText.gameObject.SetActive(!MeetingHud.Instance); // Show if not in meeting
            }

            // Update Arrow
            if (BountyHunter.Instance.ShowArrow && BountyHunter.Instance.Bounty != null)
            {
                if (BountyHunter.Instance.Arrow == null) BountyHunter.Instance.Arrow = new Arrow(Color.red);
                if (BountyHunter.Instance.ArrowUpdateTimer <= 0f)
                {
                    BountyHunter.Instance.Arrow.Update(BountyHunter.Instance.Bounty.transform.position);
                    BountyHunter.Instance.ArrowUpdateTimer = BountyHunter.Instance.ArrowUpdateInterval;
                }

                BountyHunter.Instance.Arrow.Update();
            }
        }

        static void vultureUpdate()
        {
            if (Vulture.Instance.Player == null || CachedPlayer.LocalPlayer.PlayerControl != Vulture.Instance.Player || !Vulture.Instance.ShowArrows) return;
            if (Vulture.Instance.Player.Data.IsDead)
            {
                foreach (Arrow arrow in Vulture.Instance.Arrows) UnityEngine.Object.Destroy(arrow.arrow);
                Vulture.Instance.Arrows.Clear();
                return;
            }

            DeadBody[] deadBodies = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            bool arrowUpdate = Vulture.Instance.Arrows.Count != deadBodies.Count();
            int index = 0;

            if (arrowUpdate)
            {
                foreach (Arrow arrow in Vulture.Instance.Arrows) UnityEngine.Object.Destroy(arrow.arrow);
                Vulture.Instance.Arrows.Clear();
            }

            foreach (DeadBody db in deadBodies)
            {
                if (arrowUpdate)
                {
                    Vulture.Instance.Arrows.Add(new Arrow(Color.blue));
                    Vulture.Instance.Arrows[index].arrow.SetActive(true);
                }

                if (Vulture.Instance.Arrows[index] != null) Vulture.Instance.Arrows[index].Update(db.transform.position);
                index++;
            }
        }

        public static void mediumSetTarget()
        {
            if (!Medium.Instance.IsLocalPlayer || Medium.Instance.Player.Data.IsDead || MapUtilities.CachedShipStatus == null || MapUtilities.CachedShipStatus.AllVents == null) return;

            DeadPlayer target = null;
            Vector2 truePosition = CachedPlayer.LocalPlayer.PlayerControl.GetTruePosition();
            float closestDistance = float.MaxValue;
            var firstVent = MapUtilities.CachedShipStatus.AllVents.FirstOrDefault();
            if (!firstVent) return;
            float usableDistance = firstVent.UsableDistance;
            foreach (var (dp, ps) in Medium.Instance.DeadBodies)
            {
                float distance = Vector2.Distance(ps, truePosition);
                if (distance <= usableDistance && distance < closestDistance)
                {
                    closestDistance = distance;
                    target = dp;
                }
            }

            Medium.Instance.Target = target;
        }

        static void morphlingAndCamouflagerUpdate()
        {
            float oldCamouflageTimer = Camouflager.Instance.CamouflageTimer;
            float oldMorphTimer = Morphling.Instance.MorphTimer;
            Camouflager.Instance.CamouflageTimer = Mathf.Max(0f, Camouflager.Instance.CamouflageTimer - Time.fixedDeltaTime);
            Morphling.Instance.MorphTimer = Mathf.Max(0f, Morphling.Instance.MorphTimer - Time.fixedDeltaTime);


            // Camouflage reset and set Morphling look if necessary
            if (oldCamouflageTimer > 0f && Camouflager.Instance.CamouflageTimer <= 0f)
            {
                Camouflager.Instance.ResetCamouflage();
                if (Morphling.Instance.MorphTimer > 0f && Morphling.Instance.Player != null && Morphling.Instance.MorphTarget != null)
                {
                    PlayerControl target = Morphling.Instance.MorphTarget;
                    Morphling.Instance.Player.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId,
                        target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId,
                        target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId);
                }
            }

            // Morphling reset (only if camouflage is inactive)
            if (Camouflager.Instance.CamouflageTimer <= 0f && oldMorphTimer > 0f && Morphling.Instance.MorphTimer <= 0f &&
                Morphling.Instance.Player != null)
                Morphling.Instance.ResetMorph();
        }

        public static void lawyerUpdate()
        {
            if (Lawyer.Instance.Player == null || Lawyer.Instance.Player != CachedPlayer.LocalPlayer.PlayerControl) return;

            // Promote to Pursuer
            if (Lawyer.Instance.Target == null || !Lawyer.Instance.Target.Data.Disconnected || Lawyer.Instance.Player.Data.IsDead) return;
            Lawyer.LawyerPromotesToPursuer();
        }

        public static void hackerUpdate()
        {
            if (!Hacker.Instance.IsLocalPlayer || Hacker.Instance.Player.Data.IsDead) return;
            var (playerCompleted, _) = TasksHandler.taskInfo(Hacker.Instance.Player.Data);
            if (playerCompleted == Hacker.Instance.RechargedTasks)
            {
                Hacker.Instance.RechargedTasks += Hacker.Instance.RechargeTasksNumber;
                if (Hacker.Instance.MaxGadgetCharges > Hacker.Instance.VitalsCharges) Hacker.Instance.VitalsCharges++;
                if (Hacker.Instance.MaxGadgetCharges > Hacker.Instance.AdminCharges) Hacker.Instance.AdminCharges++;
            }
        }

        // For swapper swap charges        
        public static void swapperUpdate()
        {
            if (Swapper.Instance.Player == null || CachedPlayer.LocalPlayer.PlayerControl != Swapper.Instance.Player ||
                CachedPlayer.LocalPlayer.Data.IsDead) return;
            var (playerCompleted, _) = TasksHandler.taskInfo(CachedPlayer.LocalPlayer.Data);
            if (playerCompleted == Swapper.Instance.RechargedTasks)
            {
                Swapper.Instance.RechargedTasks += Swapper.Instance.RechargeTasksNumber;
                Swapper.Instance.UsedSwaps--;
            }
        }

        static void pursuerSetTarget()
        {
            if (Pursuer.Instance.Player == null || Pursuer.Instance.Player != CachedPlayer.LocalPlayer.PlayerControl) return;
            Pursuer.Instance.SetTarget(Helpers.setTarget());
        }

        static void witchSetTarget()
        {
            if (Witch.Instance.Player == null || Witch.Instance.Player != CachedPlayer.LocalPlayer.PlayerControl) return;
            List<PlayerControl> untargetables;
            if (Witch.Instance.SpellCastingTarget != null)
                untargetables = PlayerControl.AllPlayerControls.ToArray()
                    .Where(x => x.PlayerId != Witch.Instance.SpellCastingTarget.PlayerId)
                    .ToList(); // Don't switch the target from the the one you're currently casting a spell on
            else
            {
                untargetables =
                    new List<PlayerControl>(); // Also target players that have already been spelled, to hide spells that were blanks/blocked by shields
                if (Spy.Instance.Player != null && !Witch.Instance.CanSpellAnyone) untargetables.Add(Spy.Instance.Player);
                if (Sidekick.Instance.WasTeamRed && !Witch.Instance.CanSpellAnyone) untargetables.Add(Sidekick.Instance.Player);
                if (Jackal.Instance.WasTeamRed && !Witch.Instance.CanSpellAnyone) untargetables.Add(Jackal.Instance.Player);
            }

            Witch.Instance.SetTarget(Helpers.setTarget(onlyCrewmates: !Witch.Instance.CanSpellAnyone, untargetablePlayers: untargetables));
        }

        static void ninjaSetTarget()
        {
            if (Ninja.Instance.Player == null || Ninja.Instance.Player != CachedPlayer.LocalPlayer.PlayerControl) return;
            List<PlayerControl> untargetables = new List<PlayerControl>();
            if (Spy.Instance.Player != null && !Spy.Instance.ImpostorsCanKillAnyone) untargetables.Add(Spy.Instance.Player);
            if (Mini.Instance.Player != null && !Mini.Instance.IsGrownUp) untargetables.Add(Mini.Instance.Player);
            if (Sidekick.Instance.WasTeamRed && !Spy.Instance.ImpostorsCanKillAnyone) untargetables.Add(Sidekick.Instance.Player);
            if (Jackal.Instance.WasTeamRed && !Spy.Instance.ImpostorsCanKillAnyone) untargetables.Add(Jackal.Instance.Player);
            Ninja.Instance.CurrentTarget = Helpers.setTarget(onlyCrewmates: Spy.Instance.Player == null || !Spy.Instance.ImpostorsCanKillAnyone,
                untargetablePlayers: untargetables);
            Helpers.setPlayerOutline(Ninja.Instance.CurrentTarget, Ninja.Instance.Color);
        }

        static void thiefSetTarget()
        {
            if (Thief.Instance.Player == null || Thief.Instance.Player != CachedPlayer.LocalPlayer.PlayerControl) return;
            List<PlayerControl> untargetables = new List<PlayerControl>();
            if (Mini.Instance.Player != null && !Mini.Instance.IsGrownUp) untargetables.Add(Mini.Instance.Player);
            Thief.Instance.SetTarget(Helpers.setTarget(onlyCrewmates: false, untargetablePlayers: untargetables));
        }

        static void baitUpdate()
        {
            if (!Bait.Instance.Active.Any()) return;

            // Bait report
            foreach (var entry in new Dictionary<DeadPlayer, float>(Bait.Instance.Active))
            {
                Bait.Instance.Active[entry.Key] = entry.Value - Time.fixedDeltaTime;
                if (entry.Value > 0) continue;
                Bait.Instance.Active.Remove(entry.Key);
                if (entry.Key.killerIfExisting == null ||
                    entry.Key.killerIfExisting.PlayerId != CachedPlayer.LocalPlayer.PlayerId) continue;
                Helpers.handleVampireBiteOnBodyReport(); // Manually call Vampire handling, since the CmdReportDeadBody Prefix won't be called
                Helpers.handleWhispererKillOnBodyReport();
                Helpers.handleUndertakerDropOnBodyReport();
                KernelRpc.UncheckedCmdReportDeadBody(entry.Key.killerIfExisting.PlayerId, entry.Key.player.PlayerId);
            }
        }

        static void bloodyUpdate()
        {
            if (!Bloody.Instance.Active.Any()) return;
            foreach (var entry in new Dictionary<byte, float>(Bloody.Instance.Active))
            {
                PlayerControl player = Helpers.playerById(entry.Key);
                PlayerControl bloodyPlayer = Helpers.playerById(Bloody.Instance.BloodyKillerMap[player.PlayerId]);

                Bloody.Instance.Active[entry.Key] = entry.Value - Time.fixedDeltaTime;
                if (entry.Value <= 0 || player.Data.IsDead)
                {
                    Bloody.Instance.Active.Remove(entry.Key);
                    continue; // Skip the creation of the next blood drop, if the killer is dead or the time is up
                }

                new Bloodytrail(player, bloodyPlayer);
            }
        }

        // Mini set adapted button cooldown for Vampire, Sheriff, Jackal, Sidekick, Warlock, Cleaner
        public static void miniCooldownUpdate()
        {
            if (Mini.Instance.Player != null && CachedPlayer.LocalPlayer.PlayerControl == Mini.Instance.Player)
            {
                var multiplier = Mini.Instance.IsGrownUp ? 0.66f : 2f;
                HudManagerStartPatch.sheriffKillButton.MaxTimer = Sheriff.Instance.Cooldown * multiplier;
                HudManagerStartPatch.vampireKillButton.MaxTimer = Vampire.Instance.BiteCooldown * multiplier;
                HudManagerStartPatch.whispererKillButton.MaxTimer = Whisperer.Instance.WhisperCooldown * multiplier;
                HudManagerStartPatch.jackalKillButton.MaxTimer = Jackal.Instance.KillCooldown * multiplier;
                HudManagerStartPatch.sidekickKillButton.MaxTimer = Jackal.Instance.KillCooldown * multiplier;
                HudManagerStartPatch.warlockCurseButton.MaxTimer = Warlock.Instance.CurseCooldown * multiplier;
                HudManagerStartPatch.cleanerCleanButton.MaxTimer = Cleaner.Instance.CleanCooldown * multiplier;
                HudManagerStartPatch.witchSpellButton.MaxTimer =
                    (Witch.Instance.SpellCooldown + Witch.Instance.CurrentCooldownAddition) * multiplier;
                HudManagerStartPatch.ninjaButton.MaxTimer = Ninja.Instance.NinjaCooldown * multiplier;
                HudManagerStartPatch.thiefKillButton.MaxTimer = Thief.Instance.KillCooldown * multiplier;
            }
        }

        public static void trapperUpdate()
        {
            if (Trapper.Instance.Player == null || CachedPlayer.LocalPlayer.PlayerControl != Trapper.Instance.Player ||
                Trapper.Instance.Player.Data.IsDead) return;
            var (playerCompleted, _) = TasksHandler.taskInfo(Trapper.Instance.Player.Data);
            if (playerCompleted == Trapper.Instance.RechargedTasks)
            {
                Trapper.Instance.RechargedTasks += Trapper.Instance.RechargeTasksNumber;
                if (Trapper.Instance.MaxCharges > Trapper.Instance.Charges) Trapper.Instance.Charges++;
            }
        }

        static void hunterUpdate()
        {
            if (!HideNSeek.isHideNSeekGM) return;
            int minutes = (int)HideNSeek.timer / 60;
            int seconds = (int)HideNSeek.timer % 60;
            string suffix = $" {minutes:00}:{seconds:00}";

            if (HideNSeek.timerText == null)
            {
                RoomTracker roomTracker = FastDestroyableSingleton<HudManager>.Instance?.roomTracker;
                if (roomTracker != null)
                {
                    GameObject gameObject = UnityEngine.Object.Instantiate(roomTracker.gameObject);

                    gameObject.transform.SetParent(FastDestroyableSingleton<HudManager>.Instance.transform);
                    UnityEngine.Object.DestroyImmediate(gameObject.GetComponent<RoomTracker>());
                    HideNSeek.timerText = gameObject.GetComponent<TMPro.TMP_Text>();

                    // Use local position to place it in the player's view instead of the world location
                    gameObject.transform.localPosition = new Vector3(0, -1.8f, gameObject.transform.localPosition.z);
                    if (AmongUs.Data.DataManager.Settings.Gameplay.StreamerMode)
                        gameObject.transform.localPosition = new Vector3(0, 2f, gameObject.transform.localPosition.z);
                }
            }
            else
            {
                if (HideNSeek.isWaitingTimer)
                {
                    HideNSeek.timerText.text = "<color=#0000cc>" + suffix + "</color>";
                    HideNSeek.timerText.color = Color.blue;
                }
                else
                {
                    HideNSeek.timerText.text = "<color=#FF0000FF>" + suffix + "</color>";
                    HideNSeek.timerText.color = Color.red;
                }
            }

            if (HideNSeek.isHunted() && !Hunted.taskPunish && !HideNSeek.isWaitingTimer)
            {
                var (playerCompleted, playerTotal) = TasksHandler.taskInfo(CachedPlayer.LocalPlayer.Data);
                int numberOfTasks = playerTotal - playerCompleted;
                if (numberOfTasks == 0)
                {
                    CommonRpc.ShareTimer(HideNSeek.taskPunish);
                    Hunted.taskPunish = true;
                }
            }

            if (!HideNSeek.isHunter()) return;

            byte playerId = CachedPlayer.LocalPlayer.PlayerId;
            foreach (Arrow arrow in Hunter.localArrows) arrow.arrow.SetActive(false);
            if (Hunter.arrowActive)
            {
                int arrowIndex = 0;
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (!p.Data.IsDead && !p.Data.Role.IsImpostor)
                    {
                        if (arrowIndex >= Hunter.localArrows.Count)
                        {
                            Hunter.localArrows.Add(new Arrow(Color.blue));
                        }

                        if (arrowIndex < Hunter.localArrows.Count && Hunter.localArrows[arrowIndex] != null)
                        {
                            Hunter.localArrows[arrowIndex].arrow.SetActive(true);
                            Hunter.localArrows[arrowIndex].Update(p.transform.position, Color.blue);
                        }

                        arrowIndex++;
                    }
                }
            }
        }

        public static void Postfix(PlayerControl __instance)
        {
            if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
            {
                CustomGuid.UpdateAdminsColor(__instance);
                return;
            }
            if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return;

            // Mini and Morphling shrink
            playerSizeUpdate(__instance);

            // set position of colorblind text
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                //pc.cosmetics.colorBlindText.gameObject.transform.localPosition = new Vector3(0, 0, -0.0001f);
            }

            if (CachedPlayer.LocalPlayer.PlayerControl == __instance)
            {

                // Update first kill shield if is timer
                TORMapOptions.UpdateShield();
                // Update player outlines
                setBasePlayerOutlines();

                // Update Role Description
                Helpers.refreshRoleDescription(__instance);

                // Update Player Info
                updatePlayerInfo();

                // Time Master
                bendTimeUpdate();
                // Morphling
                morphlingSetTarget();
                // Medic
                medicSetTarget();
                // Shifter
                shifterSetTarget();
                // Sheriff
                sheriffSetTarget();
                // Deputy
                deputySetTarget();
                deputyUpdate();
                // Detective
                detectiveUpdateFootPrints();
                // Tracker
                trackerSetTarget();
                // Vampire
                vampireSetTarget();
                Garlic.UpdateAll();
                // Whisperer
                whispererSetTarget();
                // Undertaker
                undertakerSetTarget();
                undertakerCanDropTarget();
                undertakerUpdate();
                Trap.Update();
                // Eraser
                eraserSetTarget();
                // Engineer
                engineerUpdate();
                // Tracker
                trackerUpdate();
                // Jackal
                jackalSetTarget();
                // Sidekick
                sidekickSetTarget();
                // Impostor
                impostorSetTarget();
                // Warlock
                warlockSetTarget();
                // Check for deputy promotion on Sheriff disconnect
                deputyCheckPromotion();
                // Check for sidekick promotion on Jackal disconnect
                sidekickCheckPromotion();
                // SecurityGuard
                securityGuardSetTarget();
                securityGuardUpdate();
                // Arsonist
                arsonistSetTarget();
                // Snitch
                snitchUpdate();
                // BountyHunter
                bountyHunterUpdate();
                // Vulture
                vultureUpdate();
                // Medium
                mediumSetTarget();
                // Morphling and Camouflager
                morphlingAndCamouflagerUpdate();
                // Lawyer
                lawyerUpdate();
                // Pursuer
                pursuerSetTarget();
                // Witch
                witchSetTarget();
                // Ninja
                ninjaSetTarget();
                NinjaTrace.UpdateAll();
                ninjaUpdate();
                // Thief
                thiefSetTarget();

                hackerUpdate();
                swapperUpdate();
                // Hacker
                hackerUpdate();
                // Trapper
                trapperUpdate();

                // -- MODIFIER--
                // Bait
                baitUpdate();
                // Bloody
                bloodyUpdate();
                // mini (for the cooldowns)
                miniCooldownUpdate();
                // Chameleon (invis stuff, timers)
                Chameleon.update();
                Bomb.update();

                // -- GAME MODE --
                hunterUpdate();
            }
        }
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.WalkPlayerTo))]
    class PlayerPhysicsWalkPlayerToPatch
    {
        private static Vector2 offset = Vector2.zero;

        public static void Prefix(PlayerPhysics __instance)
        {
            bool correctOffset = Camouflager.Instance.CamouflageTimer <= 0f && (__instance.myPlayer == Mini.Instance.Player ||
                                                                       (Morphling.Instance.Player != null &&
                                                                        __instance.myPlayer == Morphling.Instance.Player &&
                                                                        Morphling.Instance.MorphTarget == Mini.Instance.Player &&
                                                                        Morphling.Instance.MorphTimer > 0f));
            correctOffset = correctOffset && !(Mini.Instance.Player == Morphling.Instance.Player && Morphling.Instance.MorphTimer > 0f);
            if (correctOffset)
            {
                float currentScaling = (Mini.Instance.GrowingProgress() + 1) * 0.5f;
                __instance.myPlayer.Collider.offset = currentScaling * Mini.DefaultColliderOffset * Vector2.down;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdReportDeadBody))]
    class PlayerControlCmdReportDeadBodyPatch
    {
        public static bool Prefix(PlayerControl __instance)
        {
            if (HideNSeek.isHideNSeekGM) return false;
            Helpers.handleVampireBiteOnBodyReport();
            Helpers.handleWhispererKillOnBodyReport();
            Helpers.handleUndertakerDropOnBodyReport();
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(CachedPlayer.LocalPlayer.PlayerControl.CmdReportDeadBody))]
    class BodyReportPatch
    {
        static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] GameData.PlayerInfo target)
        {
            // Medic or Detective report
            bool isMedicReport = Medic.Instance.IsLocalPlayer && __instance.PlayerId == Medic.Instance.Player.PlayerId;
            bool isDetectiveReport = Detective.Instance.IsLocalPlayer &&
                                     __instance.PlayerId == Detective.Instance.Player.PlayerId;
            if (isMedicReport || isDetectiveReport)
            {
                DeadPlayer deadPlayer =
                    deadPlayers?.Where(x => x.player?.PlayerId == target?.PlayerId)?.FirstOrDefault();

                if (deadPlayer != null && deadPlayer.killerIfExisting != null)
                {
                    float timeSinceDeath = ((float)(DateTime.UtcNow - deadPlayer.timeOfDeath).TotalMilliseconds);
                    string msg = "";

                    if (isMedicReport)
                    {
                        msg = $"Body Report: Killed {Math.Round(timeSinceDeath / 1000)}s ago!";
                    }
                    else
                    {
                        if (timeSinceDeath < Detective.Instance.ReportNameDuration * 1000)
                        {
                            msg =
                                $"Body Report: The killer appears to be {deadPlayer.killerIfExisting.Data.PlayerName}!";
                        }
                        else if (timeSinceDeath < Detective.Instance.ReportColorDuration * 1000)
                        {
                            var typeOfColor =
                                Helpers.isLighterColor(deadPlayer.killerIfExisting.Data.DefaultOutfit.ColorId)
                                    ? "lighter"
                                    : "darker";
                            msg = $"Body Report: The killer appears to be a {typeOfColor} color!";
                        }
                        else
                        {
                            msg = $"Body Report: The corpse is too old to gain information from!";
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(msg))
                    {
                        if (AmongUsClient.Instance.AmClient && FastDestroyableSingleton<HudManager>.Instance)
                        {
                            FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(
                                CachedPlayer.LocalPlayer.PlayerControl, msg);

                            // Ghost Info
                            GhostInfos.ShareGhostInfo(GhostInfos.Types.DetectiveOrMedicInfo,
                                Rpc.Serialize(new Tuple<byte, string>(CachedPlayer.LocalPlayer.PlayerId, msg)));
                        }

                        if (msg.IndexOf("who", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            FastDestroyableSingleton<Assets.CoreScripts.Telemetry>.Instance.SendWho();
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    public static class MurderPlayerPatch
    {
        public static bool resetToCrewmate = false;
        public static bool resetToDead = false;

        public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            // Allow everyone to murder players
            resetToCrewmate = !__instance.Data.Role.IsImpostor;
            resetToDead = __instance.Data.IsDead;
            __instance.Data.Role.TeamType = RoleTeamTypes.Impostor;
            __instance.Data.IsDead = false;
        }

        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            // Collect dead player info
            DeadPlayer deadPlayer = new DeadPlayer(target, DateTime.UtcNow, DeathReason.Kill, __instance);
            GameHistory.deadPlayers.Add(deadPlayer);

            // Reset killer to crewmate if resetToCrewmate
            if (resetToCrewmate) __instance.Data.Role.TeamType = RoleTeamTypes.Crewmate;
            if (resetToDead) __instance.Data.IsDead = true;

            // Remove fake tasks when player dies
            if (target.hasFakeTasks() || target == Lawyer.Instance.Player || target == Pursuer.Instance.Player || target == Thief.Instance.Player)
                target.clearAllTasks();

            // First kill (set before lover suicide)
            if (TORMapOptions.firstKillName == "") TORMapOptions.firstKillName = target.Data.PlayerName;

            // Lover suicide trigger on murder
            if ((Lovers.Instance.Lover1 != null && target == Lovers.Instance.Lover1) ||
                (Lovers.Instance.Lover2 != null && target == Lovers.Instance.Lover2))
            {
                PlayerControl otherLover = target == Lovers.Instance.Lover1 ? Lovers.Instance.Lover2 : Lovers.Instance.Lover1;
                if (otherLover != null && !otherLover.Data.IsDead && Lovers.Instance.BothDie)
                {
                    otherLover.MurderPlayer(otherLover);
                }
            }

            // Sidekick promotion trigger on murder
            if (Sidekick.Instance.PromotesToJackal && Sidekick.Instance.Player != null && !Sidekick.Instance.Player.Data.IsDead &&
                target == Jackal.Instance.Player && Jackal.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl)
            {
                Sidekick.SidekickPromotes();
            }

            // Pursuer promotion trigger on murder (the host sends the call such that everyone recieves the update before a possible game End)
            if (target == Lawyer.Instance.Target && AmongUsClient.Instance.AmHost && Lawyer.Instance.Player != null)
            {
                Lawyer.LawyerPromotesToPursuer();
            }

            // Seer show flash and add dead player position
            if (Seer.Instance.Player != null &&
                (CachedPlayer.LocalPlayer.PlayerControl == Seer.Instance.Player || Helpers.shouldShowGhostInfo()) &&
                !Seer.Instance.Player.Data.IsDead && Seer.Instance.Player != target && Seer.Instance.ShowDeathFlash)
            {
                Helpers.showFlash(new Color(42f / 255f, 187f / 255f, 245f / 255f), message: "Seer Info: Someone Died");
            }

            if (Seer.Instance.DeadBodyPositions != null) Seer.Instance.DeadBodyPositions.Add(target.transform.position);

            // Tracker store body positions
            if (Tracker.Instance.DeadBodyPositions != null) Tracker.Instance.DeadBodyPositions.Add(target.transform.position);

            // Medium add body
            if (Medium.Instance.DeadBodies != null)
            {
                Medium.Instance.FeatureDeadBodies.Add(new Tuple<DeadPlayer, Vector3>(deadPlayer, target.transform.position));
            }

            // Set bountyHunter cooldown
            if (BountyHunter.Instance.Player != null &&
                CachedPlayer.LocalPlayer.PlayerControl == BountyHunter.Instance.Player &&
                __instance == BountyHunter.Instance.Player)
            {
                if (target == BountyHunter.Instance.Bounty)
                {
                    BountyHunter.Instance.Player.SetKillTimer(BountyHunter.Instance.BountyKillCooldown);
                    BountyHunter.Instance.BountyUpdateTimer = 0f; // Force bounty update
                }
                else
                    BountyHunter.Instance.Player.SetKillTimer(
                        GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown +
                        BountyHunter.Instance.PunishmentTime);
            }

            // Mini Set Impostor Mini kill timer (Due to mini being a modifier, all "SetKillTimers" must have happened before this!)
            if (Mini.Instance.Player != null && __instance == Mini.Instance.Player && __instance == CachedPlayer.LocalPlayer.PlayerControl)
            {
                float multiplier = 1f;
                if (Mini.Instance.Player != null && CachedPlayer.LocalPlayer.PlayerControl == Mini.Instance.Player)
                    multiplier = Mini.Instance.IsGrownUp ? 0.66f : 2f;
                Mini.Instance.Player.SetKillTimer(__instance.killTimer * multiplier);
            }

            // Cleaner Button Sync
            if (Cleaner.Instance.Player != null && CachedPlayer.LocalPlayer.PlayerControl == Cleaner.Instance.Player &&
                __instance == Cleaner.Instance.Player && HudManagerStartPatch.cleanerCleanButton != null)
                HudManagerStartPatch.cleanerCleanButton.Timer = Cleaner.Instance.Player.killTimer;

            // Witch Button Sync
            if (Witch.Instance.TriggerBothCooldown && Witch.Instance.Player != null &&
                CachedPlayer.LocalPlayer.PlayerControl == Witch.Instance.Player && __instance == Witch.Instance.Player &&
                HudManagerStartPatch.witchSpellButton != null)
                HudManagerStartPatch.witchSpellButton.Timer = HudManagerStartPatch.witchSpellButton.MaxTimer;

            // Warlock Button Sync
            if (Warlock.Instance.Player != null && CachedPlayer.LocalPlayer.PlayerControl == Warlock.Instance.Player &&
                __instance == Warlock.Instance.Player && HudManagerStartPatch.warlockCurseButton != null)
            {
                if (Warlock.Instance.Player.killTimer > HudManagerStartPatch.warlockCurseButton.Timer)
                {
                    HudManagerStartPatch.warlockCurseButton.Timer = Warlock.Instance.Player.killTimer;
                }
            }

            // Whisperer Button Sync
            if (Whisperer.Instance.Player != null && CachedPlayer.LocalPlayer.PlayerControl == Whisperer.Instance.Player &&
                __instance == Whisperer.Instance.Player && HudManagerStartPatch.whispererKillButton != null)
            {
                HudManagerStartPatch.whispererKillButton.Timer = Whisperer.Instance.Player.killTimer;
            }

            // Ninja Button Sync
            if (Ninja.Instance.Player != null && CachedPlayer.LocalPlayer.PlayerControl == Ninja.Instance.Player &&
                __instance == Ninja.Instance.Player && HudManagerStartPatch.ninjaButton != null)
                HudManagerStartPatch.ninjaButton.Timer = HudManagerStartPatch.ninjaButton.MaxTimer;

            // Bait
            if (Bait.Instance.Is(target))
            {
                float reportDelay = (float)Rnd.Next((int)Bait.Instance.ReportDelayMin, (int)Bait.Instance.ReportDelayMax + 1);
                Bait.Instance.Active.Add(deadPlayer, reportDelay);

                if (Bait.Instance.ShowKillFlash && __instance == CachedPlayer.LocalPlayer.PlayerControl)
                    Helpers.showFlash(new Color(204f / 255f, 102f / 255f, 0f / 255f));
            }

            // Add Bloody Modifier
            if (Bloody.Instance.Is(target))
            {
                Bloody.SetBloody(__instance.PlayerId, target.PlayerId);
            }

            // VIP Modifier
            if (Vip.Instance.Is(target))
            {
                Color color = Color.yellow;
                if (Vip.Instance.ShowTeamColor)
                {
                    color = Color.white;
                    if (target.Data.Role.IsImpostor) color = Color.red;
                    else if (RoleInfo.getRoleInfoForPlayer(target, false).FirstOrDefault().isNeutral)
                        color = Color.blue;
                }

                Helpers.showFlash(color, 1.5f);
            }

            // HideNSeek
            if (HideNSeek.isHideNSeekGM)
            {
                int visibleCounter = 0;
                Vector3 bottomLeft = IntroCutsceneOnDestroyPatch.bottomLeft + new Vector3(-0.25f, -0.25f, 0);
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (!TORMapOptions.playerIcons.ContainsKey(p.PlayerId) || p.Data.Role.IsImpostor) continue;
                    if (p.Data.IsDead || p.Data.Disconnected)
                    {
                        TORMapOptions.playerIcons[p.PlayerId].gameObject.SetActive(false);
                    }
                    else
                    {
                        TORMapOptions.playerIcons[p.PlayerId].transform.localPosition =
                            bottomLeft + Vector3.right * visibleCounter * 0.35f;
                        visibleCounter++;
                    }
                }
            }

            // Snitch
            if (Snitch.Instance.Player != null && CachedPlayer.LocalPlayer.PlayerId == Snitch.Instance.Player.PlayerId &&
                MapBehaviourPatch.herePoints.Keys.Any(x => x.PlayerId == target.PlayerId))
            {
                foreach (var a in MapBehaviourPatch.herePoints.Where(x => x.Key.PlayerId == target.PlayerId))
                {
                    UnityEngine.Object.Destroy(a.Value);
                    MapBehaviourPatch.herePoints.Remove(a.Key);
                }
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
    class PlayerControlSetCoolDownPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] float time)
        {
            var killButton = FastDestroyableSingleton<HudManager>.Instance.KillButton;

            if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return true;
            if (GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown <= 0f) return false;
            float multiplier = 1f;
            float addition = 0f;
            if (Mini.Instance.Player != null && CachedPlayer.LocalPlayer.PlayerControl == Mini.Instance.Player)
                multiplier = Mini.Instance.IsGrownUp ? 0.66f : 2f;
            if (BountyHunter.Instance.Player != null &&
                CachedPlayer.LocalPlayer.PlayerControl == BountyHunter.Instance.Player)
                addition = BountyHunter.Instance.PunishmentTime;
            if (Undertaker.Instance.IsLocalPlayer && Undertaker.Instance.DraggedBody != null) return false;


            __instance.killTimer = Mathf.Clamp(time, 0f,
                GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown * multiplier + addition);
            killButton.SetCoolDown(__instance.killTimer,
                GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown * multiplier + addition);
            return false;
        }
    }

    [HarmonyPatch(typeof(KillAnimation), nameof(KillAnimation.CoPerformKill))]
    class KillAnimationCoPerformKillPatch
    {
        public static bool hideNextAnimation = false;

        public static void Prefix(KillAnimation __instance, [HarmonyArgument(0)] ref PlayerControl source,
            [HarmonyArgument(1)] ref PlayerControl target)
        {
            if (hideNextAnimation)
                source = target;
            hideNextAnimation = false;
        }
    }

    [HarmonyPatch(typeof(KillAnimation), nameof(KillAnimation.SetMovement))]
    class KillAnimationSetMovementPatch
    {
        private static int? colorId = null;

        public static void Prefix(PlayerControl source, bool canMove)
        {
            Color color = source.cosmetics.currentBodySprite.BodySprite.material.GetColor("_BodyColor");
            if (Morphling.Instance.Player != null && source.Data.PlayerId == Morphling.Instance.Player.PlayerId)
            {
                var index = Palette.PlayerColors.IndexOf(color);
                if (index != -1) colorId = index;
            }
        }

        public static void Postfix(PlayerControl source, bool canMove)
        {
            if (colorId.HasValue) source.RawSetColor(colorId.Value);
            colorId = null;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
    public static class ExilePlayerPatch
    {
        public static void Postfix(PlayerControl __instance)
        {
            // Collect dead player info
            DeadPlayer deadPlayer = new DeadPlayer(__instance, DateTime.UtcNow, DeathReason.Exile, null);
            GameHistory.deadPlayers.Add(deadPlayer);

            // Remove fake tasks when player dies
            if (__instance.hasFakeTasks() || __instance == Lawyer.Instance.Player || __instance == Pursuer.Instance.Player ||
                __instance == Thief.Instance.Player)
                __instance.clearAllTasks();

            // Lover suicide trigger on exile
            if ((Lovers.Instance.Lover1 != null && __instance == Lovers.Instance.Lover1) ||
                (Lovers.Instance.Lover2 != null && __instance == Lovers.Instance.Lover2))
            {
                PlayerControl otherLover = __instance == Lovers.Instance.Lover1 ? Lovers.Instance.Lover2 : Lovers.Instance.Lover1;
                if (otherLover != null && !otherLover.Data.IsDead && Lovers.Instance.BothDie)
                    otherLover.Exiled();
            }

            // Sidekick promotion trigger on exile
            if (Sidekick.Instance.PromotesToJackal && Sidekick.Instance.Player != null && !Sidekick.Instance.Player.Data.IsDead &&
                __instance == Jackal.Instance.Player && Jackal.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl)
            {
                Sidekick.SidekickPromotes();
            }

            // Pursuer promotion trigger on exile & suicide (the host sends the call such that everyone recieves the update before a possible game End)
            if (Lawyer.Instance.Player != null && __instance == Lawyer.Instance.Target)
            {
                if (AmongUsClient.Instance.AmHost &&
                    ((Lawyer.Instance.Target != Jester.Instance.Player && !Lawyer.Instance.IsProsecutor) || Lawyer.Instance.TargetWasGuessed))
                {
                    Lawyer.LawyerPromotesToPursuer();
                }

                if (!Lawyer.Instance.TargetWasGuessed && !Lawyer.Instance.IsProsecutor)
                {
                    if (Lawyer.Instance.Player != null) Lawyer.Instance.Player.Exiled();
                    if (Pursuer.Instance.Player != null) Pursuer.Instance.Player.Exiled();
                }
            }
        }
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
    public static class PlayerPhysicsFixedUpdate
    {
        public static void Postfix(PlayerPhysics __instance)
        {
            var shouldInvert = (Invert.Instance.Is(CachedPlayer.LocalPlayer) && Invert.Instance.Meetings > 0); // xor. if already invert, eventInvert will turn it off for 10s

            if (__instance.AmOwner && AmongUsClient.Instance && AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started && !CachedPlayer.LocalPlayer.Data.IsDead && GameData.Instance && __instance.myPlayer.CanMove)
            {
                if (shouldInvert) __instance.body.velocity *= -1;

                if (Undertaker.Instance.Player.PlayerId == __instance.myPlayer.PlayerId && Undertaker.Instance.DraggedBody != null)
                {
                    __instance.body.velocity *= 1f + (float)Undertaker.Instance.SpeedModifierWhenDragging / 100f;
                }
                    
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.IsFlashlightEnabled))]
    public static class IsFlashlightEnabledPatch
    {
        public static bool Prefix(ref bool __result)
        {
            if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek)
                return true;
            __result = false;
            if (!CachedPlayer.LocalPlayer.Data.IsDead && Lighter.Instance.Player != null &&
                Lighter.Instance.Player.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
            {
                __result = true;
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.AdjustLighting))]
    public static class AdjustLight
    {
        public static bool Prefix(PlayerControl __instance)
        {
            if (__instance == null || CachedPlayer.LocalPlayer == null || Lighter.Instance.Player == null) return true;

            bool hasFlashlight = !CachedPlayer.LocalPlayer.Data.IsDead &&
                                 Lighter.Instance.Player.PlayerId == CachedPlayer.LocalPlayer.PlayerId;
            __instance.SetFlashlightInputMethod();
            __instance.lightSource.SetupLightingForGameplay(hasFlashlight, Lighter.Instance.VisionWidth,
                __instance.TargetFlashlight.transform);

            return false;
        }
    }
}