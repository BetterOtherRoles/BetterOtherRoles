using System;
using System.Collections.Generic;
using BetterOtherRoles.EnoFw.Kernel;
using BetterOtherRoles.Objects;
using UnityEngine;
using Option = BetterOtherRoles.EnoFw.Kernel.CustomOption;

namespace BetterOtherRoles.EnoFw.Roles.Crewmate;

public class Trapper : AbstractRole
{
    public static readonly Trapper Instance = new();
    
    // Fields
    public readonly List<PlayerControl> PlayersOnMap = new();
    public int RechargedTasks;
    public int Charges;
    
    // Options
    public readonly CustomOption PlaceTrapCooldown;
    public readonly CustomOption MaxCharges;
    public readonly CustomOption RechargeTasksNumber;
    public readonly CustomOption TrapNeededTriggerToReveal;
    public readonly CustomOption AnonymousMap;
    public readonly CustomOption InfoType;
    public readonly CustomOption TrapDuration;

    public static Sprite TrapButtonSprite => GetSprite("BetterOtherRoles.Resources.Trapper_Place_Button.png", 115f);

    private Trapper() : base(nameof(Trapper), "Trapper")
    {
        Team = Teams.Crewmate;
        Color = new Color32(110, 57, 105, byte.MaxValue);
        
        SpawnRate = GetDefaultSpawnRateOption();
        
        PlaceTrapCooldown = Tab.CreateFloatList(
            $"{Key}{nameof(PlaceTrapCooldown)}",
            Cs("Place trap cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        MaxCharges = Tab.CreateFloatList(
            $"{Key}{nameof(MaxCharges)}",
            Cs("Max traps charges"),
            1f,
            15f,
            5f,
            1f,
            SpawnRate);
        RechargeTasksNumber = Tab.CreateFloatList(
            $"{Key}{nameof(RechargeTasksNumber)}",
            Cs("Number of tasks needed for recharging"),
            1f,
            15f,
            2f,
            1f,
            SpawnRate);
        TrapNeededTriggerToReveal = Tab.CreateFloatList(
            $"{Key}{nameof(TrapNeededTriggerToReveal)}",
            Cs("Trap needed trigger to reveal"),
            2f,
            10f,
            3f,
            1f,
            SpawnRate);
        AnonymousMap = Tab.CreateBool(
            $"{Key}{nameof(AnonymousMap)}",
            Cs("Show anonymous map"),
            true,
            SpawnRate);
        InfoType = Tab.CreateStringList(
            $"{Key}{nameof(InfoType)}",
            Cs("Trap information type"),
            new List<string> { "role", "name", "good/evil" },
            "good/evil",
            SpawnRate);
        TrapDuration = Tab.CreateFloatList(
            $"{Key}{nameof(TrapDuration)}",
            Cs("Trap duration"),
            1f,
            15f,
            5f,
            1f,
            SpawnRate,
            string.Empty,
            "s");
    }

    public bool InfoTypeRole => (string)InfoType is "role";
    public bool InfoTypeName => (string)InfoType is "name";
    public bool InfoTypeGoodOrEvil => (string)InfoType is "good/evil";

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        RechargedTasks = 0;
        Charges = 0;
        PlayersOnMap.Clear();
    }

    public static void TriggerTrap(byte playerId, byte trapId)
    {
        var data = new Tuple<byte, byte>(playerId, trapId);
        RpcManager.Instance.Send((uint)Rpc.Role.TriggerTrap, data);
    }

    [BindRpc((uint)Rpc.Role.TriggerTrap)]
    public static void Rpc_TriggerTrap(Tuple<byte, byte> data)
    {
        var (playerId, trapId) = data;
        Trap.triggerTrap(playerId, trapId);
    }

    public static void SetTrap(float x, float y, float z)
    {
        var data = new Tuple<float, float, float>(x, y, z);
        RpcManager.Instance.Send((uint)Rpc.Role.SetTrap, data, false);
    }

    [BindRpc((uint)Rpc.Role.SetTrap)]
    public static void Rpc_SetTrap(Tuple<float, float, float> xyz)
    {
        if (Instance.Player == null) return;
        var (x, y, z) = xyz;
        Instance.Charges--;
        var position = new Vector3(x, y, z);
        var _ = new Trap(position);
    }
}