using BetterOtherRoles.EnoFw.Kernel;
using BetterOtherRoles.Utilities;
using UnityEngine;
using Option = BetterOtherRoles.EnoFw.Kernel.CustomOption;

namespace BetterOtherRoles.EnoFw.Roles.Crewmate;

public class Engineer : AbstractRole
{
    public static readonly Engineer Instance = new();
    
    // Fields
    public int UsedFixes;
    public int RemainingFixes => NumberOfFixes - UsedFixes;
    
    // Options
    public readonly CustomOption NumberOfFixes;
    public readonly CustomOption HighlightVentsForImpostors;
    public readonly CustomOption HighlightVentsForNeutrals;

    public static Sprite RepairButtonSprite => GetSprite("BetterOtherRoles.Resources.RepairButton.png", 115f);

    private Engineer() : base(nameof(Engineer), "Engineer")
    {
        Team = Teams.Crewmate;
        Color = new Color32(0, 40, 245, byte.MaxValue);
        
        SpawnRate = GetDefaultSpawnRateOption();
        
        NumberOfFixes = Tab.CreateFloatList(
            $"{Name}{nameof(NumberOfFixes)}",
            Cs("Number of sabotage fixes"),
            1f,
            3f,
            1f,
            1f,
            SpawnRate);
        HighlightVentsForImpostors = Tab.CreateBool(
            $"{Name}{nameof(HighlightVentsForImpostors)}",
            Cs("Impostors see vents highlighted"),
            true,
            SpawnRate);
        HighlightVentsForNeutrals = Tab.CreateBool(
            $"{Name}{nameof(HighlightVentsForNeutrals)}",
            Cs("Neutrals see vents highlighted"),
            false,
            SpawnRate);
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        UsedFixes = 0;
    }

    public static void EngineerUsedRepair()
    {
        RpcManager.Instance.Send((uint)Rpc.Role.EngineerUsedRepair);
    }

    [BindRpc((uint)Rpc.Role.EngineerUsedRepair)]
    public static void Rpc_EngineerUsedRepair()
    {
        Instance.UsedFixes++;
        if (!Helpers.shouldShowGhostInfo()) return;
        Helpers.showFlash(Instance.Color, 0.5f, "Engineer Fix");
    }

    public static void EngineerFixSubmergedOxygen()
    {
        RpcManager.Instance.Send((uint)Rpc.Role.EngineerFixSubmergedOxygen);
    }

    [BindRpc((uint)Rpc.Role.EngineerFixSubmergedOxygen)]
    public static void Rpc_EngineerFixSubmergedOxygen()
    {
        SubmergedCompatibility.RepairOxygen();
    }

    public static void EngineerFixLights()
    {
        RpcManager.Instance.Send((uint)Rpc.Role.EngineerFixLights);
    }

    [BindRpc((uint)Rpc.Role.EngineerFixLights)]
    public static void Rpc_EngineerFixLights()
    {
        var switchSystem = MapUtilities.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
        switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
    }
}