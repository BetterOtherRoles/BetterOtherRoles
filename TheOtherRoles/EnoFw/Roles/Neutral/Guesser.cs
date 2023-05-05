using System;
using System.Linq;
using Reactor.Networking.Attributes;
using TheOtherRoles.CustomGameModes;
using TheOtherRoles.EnoFw.Roles.Modifiers;
using TheOtherRoles.Patches;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;
using UnityEngine;

namespace TheOtherRoles.EnoFw.Roles.Neutral;

public static class Guesser
{
    public static PlayerControl niceGuesser;
    public static PlayerControl evilGuesser;
    public static Color color = new Color32(255, 255, 0, byte.MaxValue);

    public static int remainingShotsEvilGuesser = 2;
    public static int remainingShotsNiceGuesser = 2;

    public static bool isGuesser(byte playerId)
    {
        if ((niceGuesser != null && niceGuesser.PlayerId == playerId) ||
            (evilGuesser != null && evilGuesser.PlayerId == playerId)) return true;
        return false;
    }

    public static void clear(byte playerId)
    {
        if (niceGuesser != null && niceGuesser.PlayerId == playerId) niceGuesser = null;
        else if (evilGuesser != null && evilGuesser.PlayerId == playerId) evilGuesser = null;
    }

    public static int remainingShots(byte playerId, bool shoot = false)
    {
        int remainingShots = remainingShotsEvilGuesser;
        if (niceGuesser != null && niceGuesser.PlayerId == playerId)
        {
            remainingShots = remainingShotsNiceGuesser;
            if (shoot) remainingShotsNiceGuesser = Mathf.Max(0, remainingShotsNiceGuesser - 1);
        }
        else if (shoot)
        {
            remainingShotsEvilGuesser = Mathf.Max(0, remainingShotsEvilGuesser - 1);
        }

        return remainingShots;
    }

    public static void clearAndReload()
    {
        niceGuesser = null;
        evilGuesser = null;
        remainingShotsEvilGuesser = Mathf.RoundToInt(CustomOptionHolder.guesserNumberOfShots.getFloat());
        remainingShotsNiceGuesser = Mathf.RoundToInt(CustomOptionHolder.guesserNumberOfShots.getFloat());
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
        if (Lawyer.target != null && dyingTarget == Lawyer.target)
            Lawyer.targetWasGuessed = true; // Lawyer shouldn't be exiled with the client for guesses
        var dyingLoverPartner = Lovers.bothDie ? dyingTarget.getPartner() : null; // Lover check
        if (Lawyer.target != null && dyingLoverPartner == Lawyer.target)
            Lawyer.targetWasGuessed = true; // Lawyer shouldn't be exiled with the client for guesses
        dyingTarget.Exiled();
        var partnerId = dyingLoverPartner != null ? dyingLoverPartner.PlayerId : dyingTargetId;

        HandleGuesser.remainingShots(killerId, true);
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