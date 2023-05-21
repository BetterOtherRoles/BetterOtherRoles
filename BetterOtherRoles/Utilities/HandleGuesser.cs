using System;
using System.Linq;
using BetterOtherRoles.CustomGameModes;
using BetterOtherRoles.EnoFw;
using BetterOtherRoles.EnoFw.Libs.Reactor.Networking.Attributes;
using BetterOtherRoles.EnoFw.Roles.Crewmate;
using BetterOtherRoles.EnoFw.Roles.Impostor;
using BetterOtherRoles.EnoFw.Roles.Modifiers;
using BetterOtherRoles.EnoFw.Roles.Neutral;
using BetterOtherRoles.Patches;
using BetterOtherRoles.Players;
using UnityEngine;

namespace BetterOtherRoles.Utilities;

public static class HandleGuesser {
    private static Sprite targetSprite;
    public static bool isGuesserGm = false;
    public static bool hasMultipleShotsPerMeeting = false;
    public static bool killsThroughShield = true;
    public static bool evilGuesserCanGuessSpy = true;
    public static bool guesserCantGuessSnitch = false;

    public static Sprite getTargetSprite() {
        if (targetSprite) return targetSprite;
        targetSprite = Helpers.loadSpriteFromResources("BetterOtherRoles.Resources.TargetIcon.png", 150f);
        return targetSprite;
    }

    public static bool isGuesser(byte playerId) {
        if (isGuesserGm) return GuesserGM.isGuesser(playerId);
        return NiceGuesser.Instance.IsPlayer(playerId) || EvilGuesser.Instance.IsPlayer(playerId);
    }

    public static void clear(byte playerId) {
        if (isGuesserGm) GuesserGM.clear(playerId);
        else if (NiceGuesser.Instance.IsPlayer(playerId))
        {
            NiceGuesser.Instance.Clear(playerId);
        } else if (EvilGuesser.Instance.IsPlayer(playerId))
        {
            EvilGuesser.Instance.Clear(playerId);
        }
    }

    public static int remainingShots(byte playerId, bool shoot = false) {
        if (isGuesserGm) return GuesserGM.remainingShots(playerId, shoot);
        return NiceGuesser.Instance.IsPlayer(playerId) ? NiceGuesser.Instance.RemainingShots(playerId, shoot) : EvilGuesser.Instance.RemainingShots(playerId, shoot);
    }

    public static void clearAndReload() {
        NiceGuesser.Instance.ClearAndReload();
        EvilGuesser.Instance.ClearAndReload();
        GuesserGM.clearAndReload();
        isGuesserGm = TORMapOptions.gameMode == CustomGamemodes.Guesser;
        if (isGuesserGm)
        {
            guesserCantGuessSnitch = CustomOptions.GuesserGameModeCantGuessSnitchIfTasksDone;
            hasMultipleShotsPerMeeting = CustomOptions.GuesserGameModeHasMultipleShotsPerMeeting;
            killsThroughShield = CustomOptions.GuesserGameModeKillsThroughShield;
            evilGuesserCanGuessSpy = CustomOptions.GuesserGameModeEvilCanKillSpy;
        } else
        {
            guesserCantGuessSnitch = EvilGuesser.Instance.CantGuessSnitchIfTasksDone;
            hasMultipleShotsPerMeeting = NiceGuesser.Instance.MultipleShotsPerMeeting || EvilGuesser.Instance.MultipleShotsPerMeeting;
            killsThroughShield = NiceGuesser.Instance.KillTroughShield || EvilGuesser.Instance.KillTroughShield;
            evilGuesserCanGuessSpy = EvilGuesser.Instance.CanKillSpy;
        }
    }
    
    public static void GuesserShoot(byte killerId, byte dyingTargetId, byte guessedTargetId, byte guessedRoleId)
    {
        var data = new Tuple<byte, byte, byte, byte>(killerId, dyingTargetId, guessedTargetId, guessedRoleId);
        Rpc_GuesserShoot(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.GuesserShoot)]
    private static void Rpc_GuesserShoot(PlayerControl sender, string rawData)
    {
        var (killerId, dyingTargetId, guessedTargetId, guessedRoleId) =
            Rpc.Deserialize<Tuple<byte, byte, byte, byte>>(rawData);

        var dyingTarget = Helpers.playerById(dyingTargetId);
        if (dyingTarget == null) return;
        if (Lawyer.Instance.Target != null && dyingTarget == Lawyer.Instance.Target)
            Lawyer.Instance.TargetWasGuessed = true; // Lawyer shouldn't be exiled with the client for guesses
        var dyingLoverPartner = Lovers.Instance.BothDie ? dyingTarget.GetPartner() : null; // Lover check
        if (Lawyer.Instance.Target != null && dyingLoverPartner == Lawyer.Instance.Target)
            Lawyer.Instance.TargetWasGuessed = true; // Lawyer shouldn't be exiled with the client for guesses
        dyingTarget.Exiled();
        var partnerId = dyingLoverPartner != null ? dyingLoverPartner.PlayerId : dyingTargetId;

        remainingShots(killerId, true);
        if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(dyingTarget.KillSfx, false, 0.8f);
        if (MeetingHud.Instance)
        {
            foreach (var pva in MeetingHud.Instance.playerStates)
            {
                if (pva.TargetPlayerId == dyingTargetId || pva.TargetPlayerId == partnerId)
                {
                    pva.SetDead(pva.DidReport, true);
                    pva.Overlay.gameObject.SetActive(true);
                }

                //Give players back their vote if target is shot dead
                if (pva.VotedFor != dyingTargetId || pva.VotedFor != partnerId) continue;
                pva.UnsetVote();
                var voteAreaPlayer = Helpers.playerById(pva.TargetPlayerId);
                if (!voteAreaPlayer.AmOwner) continue;
                MeetingHud.Instance.ClearVote();
            }

            if (AmongUsClient.Instance.AmHost)
                MeetingHud.Instance.CheckForEndVoting();
        }

        var guesser = Helpers.playerById(killerId);
        if (FastDestroyableSingleton<HudManager>.Instance != null && guesser != null)
            if (CachedPlayer.LocalPlayer.PlayerControl == dyingTarget)
            {
                FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(guesser.Data,
                    dyingTarget.Data);
                if (MeetingHudPatch.guesserUI != null) MeetingHudPatch.guesserUIExitButton.OnClick.Invoke();
            }
            else if (dyingLoverPartner != null && CachedPlayer.LocalPlayer.PlayerControl == dyingLoverPartner)
            {
                FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(dyingLoverPartner.Data,
                    dyingLoverPartner.Data);
                if (MeetingHudPatch.guesserUI != null) MeetingHudPatch.guesserUIExitButton.OnClick.Invoke();
            }

        // remove shoot button from targets for all guessers and close their guesserUI
        if (GuesserGM.isGuesser(PlayerControl.LocalPlayer.PlayerId) && PlayerControl.LocalPlayer != guesser &&
            !PlayerControl.LocalPlayer.Data.IsDead &&
            GuesserGM.remainingShots(PlayerControl.LocalPlayer.PlayerId) > 0 && MeetingHud.Instance)
        {
            MeetingHud.Instance.playerStates.ToList().ForEach(x =>
            {
                if (x.TargetPlayerId == dyingTarget.PlayerId && x.transform.FindChild("ShootButton") != null)
                    UnityEngine.Object.Destroy(x.transform.FindChild("ShootButton").gameObject);
            });
            if (dyingLoverPartner != null)
                MeetingHud.Instance.playerStates.ToList().ForEach(x =>
                {
                    if (x.TargetPlayerId == dyingLoverPartner.PlayerId && x.transform.FindChild("ShootButton") != null)
                        UnityEngine.Object.Destroy(x.transform.FindChild("ShootButton").gameObject);
                });

            if (MeetingHudPatch.guesserUI != null && MeetingHudPatch.guesserUIExitButton != null)
            {
                if (MeetingHudPatch.guesserCurrentTarget == dyingTarget.PlayerId)
                    MeetingHudPatch.guesserUIExitButton.OnClick.Invoke();
                else if (dyingLoverPartner != null &&
                         MeetingHudPatch.guesserCurrentTarget == dyingLoverPartner.PlayerId)
                    MeetingHudPatch.guesserUIExitButton.OnClick.Invoke();
            }
        }


        var guessedTarget = Helpers.playerById(guessedTargetId);
        if (!CachedPlayer.LocalPlayer.Data.IsDead || guessedTarget == null || guesser == null) return;
        var roleInfo = RoleInfo.allRoleInfos.FirstOrDefault(x => (byte)x.roleId == guessedRoleId);
        var msg =
            $"{guesser.Data.PlayerName} guessed the role {roleInfo?.name ?? ""} for {guessedTarget.Data.PlayerName}!";
        if (AmongUsClient.Instance.AmClient && FastDestroyableSingleton<HudManager>.Instance)
            FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(guesser, msg);
        if (msg.Contains("who", StringComparison.OrdinalIgnoreCase))
            FastDestroyableSingleton<Assets.CoreScripts.Telemetry>.Instance.SendWho();
    }
}