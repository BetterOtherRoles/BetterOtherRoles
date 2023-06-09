﻿using BetterOtherRoles.EnoFw.Kernel;
using BetterOtherRoles.EnoFw.Roles.Crewmate;
using BetterOtherRoles.Players;
using UnityEngine;
using Option = BetterOtherRoles.EnoFw.Kernel.CustomOption;

namespace BetterOtherRoles.EnoFw.Roles.Neutral;

public class Lawyer : AbstractRole
{
    public static readonly Lawyer Instance = new();
    
    // Fields
    public PlayerControl Target;
    public PlayerControl FormerLawyer;
    public bool TriggerProsecutorWin;
    public bool IsProsecutor;
    public bool TargetWasGuessed;

    // Options
    public readonly CustomOption IsProsecutorChance;
    public readonly CustomOption Vision;
    public readonly CustomOption KnowsTargetRole;
    public readonly CustomOption CanCallEmergencyMeeting;
    public readonly CustomOption TargetCanBeJester;

    private Lawyer() : base(nameof(Lawyer), "Lawyer")
    {
        Team = Teams.Neutral;
        Color = new Color32(134, 153, 25, byte.MaxValue);
        
        SpawnRate = GetDefaultSpawnRateOption();
        
        IsProsecutorChance = Tab.CreateFloatList(
            $"{Key}{nameof(IsProsecutorChance)}",
            Cs("Chance to be Prosecutor"),
            0f,
            100f,
            50f,
            10f,
            SpawnRate,
            string.Empty,
            "%");
        Vision = Tab.CreateFloatList(
            $"{Key}{nameof(Vision)}",
            Cs("Vision"),
            0.25f,
            3f,
            1f,
            0.25f,
            SpawnRate);
        KnowsTargetRole = Tab.CreateBool(
            $"{Key}{nameof(KnowsTargetRole)}",
            Cs("Knows target role"),
            false,
            SpawnRate);
        CanCallEmergencyMeeting = Tab.CreateBool(
            $"{Key}{nameof(CanCallEmergencyMeeting)}",
            Cs("Can call emergency meeting"),
            false,
            SpawnRate);
        TargetCanBeJester = Tab.CreateBool(
            $"{Key}{nameof(TargetCanBeJester)}",
            Cs("Target can be jester"),
            false,
            SpawnRate);
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        ClearAndReloadButKeepTarget();
        Target = null;
        TargetWasGuessed = false;
    }

    public void ClearAndReloadButKeepTarget()
    {
        base.ClearAndReload();
        FormerLawyer = null;
        IsProsecutor = false;
        TriggerProsecutorWin = false;
    }

    public static void LawyerPromotesToPursuer()
    {
        RpcManager.Instance.Send((uint)Rpc.Role.LawyerPromotesToPursuer);
    }

    [BindRpc((uint)Rpc.Role.LawyerPromotesToPursuer)]
    public static void Rpc_LawyerPromotesToPursuer()
    {
        var player = Instance.Player;
        var client = Instance.Target;
        Instance.ClearAndReloadButKeepTarget();

        Pursuer.Instance.Player = player;

        if (player.PlayerId != CachedPlayer.LocalPlayer.PlayerId || client == null) return;
        var playerInfoTransform = client.cosmetics.nameText.transform.parent.FindChild("Info");
        var playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
        if (playerInfo != null) playerInfo.text = "";
    }

    public static void LawyerSetTarget(byte playerId)
    {
        RpcManager.Instance.Send((uint)Rpc.Role.LawyerSetTarget, playerId);
    }

    [BindRpc((uint)Rpc.Role.LawyerSetTarget)]
    public static void Rpc_LawyerSetTarget(byte playerId)
    {
        Instance.Target = Helpers.playerById(playerId);
    }
}