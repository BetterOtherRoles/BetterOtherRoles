using System;
using Reactor.Networking.Attributes;
using TheOtherRoles.EnoFw.Kernel;
using UnityEngine;
using Option = TheOtherRoles.EnoFw.Kernel.CustomOption;

namespace TheOtherRoles.EnoFw.Roles.Crewmate;

public class Swapper : AbstractRole
{
    public static readonly Swapper Instance = new();
    
    // Fields
    public int UsedSwaps;
    public bool EnableSwap;
    public byte PlayerId1 = byte.MaxValue;
    public byte PlayerId2 = byte.MaxValue;
    public int RechargedTasks;
    public int Charges => NumberOfSwaps - UsedSwaps;
    
    // Options
    public readonly Option CanCallEmergencyMeeting;
    public readonly Option CanOnlySwapOthers;
    public readonly Option NumberOfSwaps;
    public readonly Option RechargeTasksNumber;

    public static Sprite CheckSprite => GetSprite("TheOtherRoles.Resources.SwapperCheck.png", 150f);

    private Swapper() : base(nameof(Swapper), "Swapper")
    {
        Team = Teams.Crewmate;
        Color = new Color32(134, 55, 86, byte.MaxValue);
        
        SpawnRate = GetDefaultSpawnRateOption();
        
        CanCallEmergencyMeeting = Tab.CreateBool(
            $"{Key}{nameof(CanCallEmergencyMeeting)}",
            Cs("Can call emergency meeting"),
            true,
            SpawnRate);
        CanOnlySwapOthers = Tab.CreateBool(
            $"{Key}{nameof(CanOnlySwapOthers)}",
            Cs("Can only swap others"),
            false,
            SpawnRate);
        NumberOfSwaps = Tab.CreateFloatList(
            $"{Key}{nameof(NumberOfSwaps)}",
            Cs("Initial swap charges"),
            0f,
            5f,
            1f,
            1f,
            SpawnRate);
        RechargeTasksNumber = Tab.CreateFloatList(
            $"{Key}{nameof(RechargeTasksNumber)}",
            Cs("Number of tasks needed for recharging"),
            1f,
            10f,
            2f,
            1f,
            SpawnRate);
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        PlayerId1 = byte.MaxValue;
        PlayerId2 = byte.MaxValue;
        UsedSwaps = 0;
        RechargedTasks = RechargeTasksNumber;
    }

    public static void SwapperSwap(byte pId1, byte pId2)
    {
        var data = new Tuple<byte, byte>(pId1, pId2);
        Rpc_SwapperSwap(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.SwapperSwap)]
    private static void Rpc_SwapperSwap(PlayerControl sender, string rawData)
    {
        var (pId1, pId2) = Rpc.Deserialize<Tuple<byte, byte>>(rawData);
        if (!MeetingHud.Instance) return;
        Instance.PlayerId1 = pId1;
        Instance.PlayerId2 = pId2;
    }
}