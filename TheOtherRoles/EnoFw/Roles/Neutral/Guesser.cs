using System;
using System.Linq;
using Reactor.Networking.Attributes;
using TheOtherRoles.CustomGameModes;
using TheOtherRoles.EnoFw.Kernel;
using TheOtherRoles.EnoFw.Roles.Modifiers;
using TheOtherRoles.Patches;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;
using UnityEngine;
using Option = TheOtherRoles.EnoFw.Kernel.CustomOption;

namespace TheOtherRoles.EnoFw.Roles.Neutral;

public class Guesser : AbstractRole
{
    public static readonly Guesser Instance = new();
    
    // Fields
    public PlayerControl NiceGuesser;
    public PlayerControl EvilGuesser;
    public int EvilGuesserRemainingShots = 2;
    public int NiceGuesserRemainingShots = 2;
    
    // Options
    public readonly Option IsImpostorRate;
    public readonly Option NumberOfShots;
    public readonly Option MultipleShotsPerMeeting;
    public readonly Option KillTroughShield;
    public readonly Option CanKillSpy;
    public readonly Option SpawnBothRate;
    public readonly Option CantGuessSnitchIfTasksDone;

    private Guesser() : base(nameof(Guesser), "Guesser")
    {
        Team = Teams.Neutral;
        Color = new Color32(255, 255, 0, byte.MaxValue);
        
        SpawnRate = GetDefaultSpawnRateOption();
        
        IsImpostorRate = Tab.CreateFloatList(
            $"{Key}{nameof(IsImpostorRate)}",
            Cs("Chance that the guesser is an impostor"),
            0f,
            100f,
            50f,
            10f,
            SpawnRate,
            string.Empty,
            "%");
        NumberOfShots = Tab.CreateFloatList(
            $"{Key}{nameof(NumberOfShots)}",
            Cs("Number of shots"),
            1f,
            15f,
            1f,
            1f,
            SpawnRate);
        MultipleShotsPerMeeting = Tab.CreateBool(
            $"{Key}{nameof(MultipleShotsPerMeeting)}",
            Cs("Can guess multiple time per meeting"),
            false,
            SpawnRate);
        KillTroughShield = Tab.CreateBool(
            $"{Key}{nameof(KillTroughShield)}",
            Cs("Guess ignore medic shield"),
            false,
            SpawnRate);
        CanKillSpy = Tab.CreateBool(
            $"{Name}{nameof(CanKillSpy)}",
            Cs("Evil guesser can guess Spy"),
            false,
            IsImpostorRate);
        SpawnBothRate = Tab.CreateFloatList(
            $"{Key}{nameof(SpawnBothRate)}",
            Cs("Both guesser spawn rate"),
            0f,
            100f,
            50f,
            10f,
            IsImpostorRate,
            string.Empty,
            "%");
        CantGuessSnitchIfTasksDone = Tab.CreateBool(
            $"{Key}{nameof(CantGuessSnitchIfTasksDone)}",
            Cs("Can guess Snitch when tasks completed"),
            true,
            IsImpostorRate);
    }

    public bool IsGuesser(byte playerId)
    {
        return (NiceGuesser != null && NiceGuesser.PlayerId == playerId) ||
               (EvilGuesser != null && EvilGuesser.PlayerId == playerId);
    }

    public void Clear(byte playerId)
    {
        if (NiceGuesser != null && NiceGuesser.PlayerId == playerId) NiceGuesser = null;
        else if (EvilGuesser != null && EvilGuesser.PlayerId == playerId) EvilGuesser = null;
    }

    public int RemainingShots(byte playerId, bool shoot = false)
    {
        var remainingShots = EvilGuesserRemainingShots;
        if (NiceGuesser != null && NiceGuesser.PlayerId == playerId)
        {
            remainingShots = NiceGuesserRemainingShots;
            if (shoot) NiceGuesserRemainingShots = Mathf.Max(0, NiceGuesserRemainingShots - 1);
        } else if (shoot)
        {
            EvilGuesserRemainingShots = Mathf.Max(0, EvilGuesserRemainingShots - 1);
        }

        return remainingShots;
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        NiceGuesser = null;
        EvilGuesser = null;
        EvilGuesserRemainingShots = NumberOfShots;
        NiceGuesserRemainingShots = NumberOfShots;
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