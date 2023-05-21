using System;
using System.Collections.Generic;
using BetterOtherRoles.EnoFw.Kernel;
using BetterOtherRoles.Players;
using Reactor.Networking.Attributes;
using UnityEngine;
using Option = BetterOtherRoles.EnoFw.Kernel.CustomOption;

namespace BetterOtherRoles.EnoFw.Roles.Crewmate;

public class Medic : AbstractRole
{
    public static readonly Medic Instance = new();
    
    // Fields
    public static readonly Color ShieldColor = new Color32(0, 221, 255, byte.MaxValue);
    public PlayerControl Shielded;
    public PlayerControl FutureShielded;
    public bool UsedShield;
    public bool MeetingAfterShielding;

    public bool MedicCanSeeMurderAttempt => (string)ShowMurderAttempt is "medic" or "medic + shielded";
    public bool ShieldedCanSeeMurderAttempt => (string)ShowMurderAttempt is "medic + shielded";
    public bool SetShieldInstantly => (string)WhenSetShield is "instantly";
    public bool SetShieldAfterMeeting => (string)WhenSetShield is "after meeting";
    public bool ShowShieldInstantly => (string)WhenShowShield is "instantly";
    public bool ShowShieldAfterMeeting => (string)WhenShowShield is "after meeting";
    public bool EveryoneCanSeeShield => (string)ShowShield is "everyone";
    public bool MedicCanSeeShield => (string)ShowShield is "medic" or "shielded + medic";
    public bool ShieldedCanSeeShield => (string)ShowShield is "shielded + medic";
    
    // Options
    public readonly CustomOption ShowMurderAttempt;
    public readonly CustomOption WhenSetShield;
    public readonly CustomOption WhenShowShield;
    public readonly CustomOption ShowShield;

    public static Sprite ShieldButtonSprite => GetSprite("BetterOtherRoles.Resources.ShieldButton.png", 115f);

    private Medic() : base(nameof(Medic), "Medic")
    {
        Team = Teams.Crewmate;
        Color = new Color32(126, 251, 194, byte.MaxValue);
        CanTarget = true;
        
        SpawnRate = GetDefaultSpawnRateOption();
        
        ShowMurderAttempt = Tab.CreateStringList(
            $"{Name}{nameof(ShowMurderAttempt)}",
            Cs("Who can see murder attempt"),
            new List<string> { "nobody", "medic", "medic + shielded" },
            "nobody",
            SpawnRate);
        WhenSetShield = Tab.CreateStringList(
            $"{Name}{nameof(WhenSetShield)}",
            Cs("Shield will be activated"),
            new List<string> { "instantly", "after meeting" },
            "instantly",
            SpawnRate);
        WhenShowShield = Tab.CreateStringList(
            $"{Name}{nameof(WhenShowShield)}",
            Cs("Shield will be shown"),
            new List<string> { "never", "instantly", "after meeting" },
            "instantly",
            SpawnRate);
        ShowShield = Tab.CreateStringList(
            $"{Name}{nameof(ShowShield)}",
            Cs("Show shielded player"),
            new List<string> { "everyone", "shielded + medic", "medic" },
            "shielded + medic",
            WhenShowShield);
    }

    public bool LocalPlayerCanSeeShield
    {
        get
        {
            if (Helpers.shouldShowGhostInfo() || EveryoneCanSeeShield) return true;
            if (IsLocalPlayer && MedicCanSeeShield) return true;
            return CachedPlayer.LocalPlayer.PlayerControl == Shielded && ShieldedCanSeeShield;
        }
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        Shielded = null;
        FutureShielded = null;
        UsedShield = false;
        MeetingAfterShielding = false;
    }

    public static void MedicSetShielded(byte targetId)
    {
        var data = new Tuple<byte>(targetId);
        Rpc_MedicSetShielded(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.MedicSetShielded)]
    private static void Rpc_MedicSetShielded(PlayerControl sender, string rawData)
    {
        var targetId = Rpc.Deserialize<Tuple<byte>>(rawData).Item1;
        var target = Helpers.playerById(targetId);
        if (target == null) return;
        Instance.UsedShield = true;
        Instance.Shielded = target;
        Instance.FutureShielded = null;
    }

    public static void SetFutureShielded(byte targetId)
    {
        var data = new Tuple<byte>(targetId);
        Rpc_SetFutureShielded(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.SetFutureShielded)]
    private static void Rpc_SetFutureShielded(PlayerControl sender, string rawData)
    {
        var targetId = Rpc.Deserialize<Tuple<byte>>(rawData).Item1;
        var target = Helpers.playerById(targetId);
        if (target == null) return;
        Instance.UsedShield = true;
        Instance.FutureShielded = target;
    }

    public static void ShieldedMurderAttempt()
    {
        Rpc_ShieldedMurderAttempt(PlayerControl.LocalPlayer);
    }

    [MethodRpc((uint)Rpc.Role.ShieldedMurderAttempt)]
    private static void Rpc_ShieldedMurderAttempt(PlayerControl sender)
    {
        if (Instance.Shielded == null || !Instance.HasPlayer) return;

        var isShieldedAndShow =
            Instance.Shielded == CachedPlayer.LocalPlayer.PlayerControl && Instance.ShieldedCanSeeMurderAttempt;
        isShieldedAndShow = isShieldedAndShow && (Instance.MeetingAfterShielding || Instance.SetShieldAfterMeeting); // Dont show attempt, if shield is not shown yet
        var isMedicAndShow = Instance.IsLocalPlayer && Instance.MedicCanSeeMurderAttempt;

        if (isShieldedAndShow || isMedicAndShow || Helpers.shouldShowGhostInfo())
            Helpers.showFlash(Palette.ImpostorRed, duration: 0.5f, "Failed Murder Attempt on Shielded Player");
    }
}