using BetterOtherRoles.EnoFw.Kernel;
using BetterOtherRoles.Utilities;
using BetterOtherRoles.EnoFw.Libs.Reactor.Networking.Attributes;
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
        Rpc_EngineerUsedRepair(PlayerControl.LocalPlayer);
    }

    [MethodRpc((uint)Rpc.Role.EngineerUsedRepair)]
    private static void Rpc_EngineerUsedRepair(PlayerControl sender)
    {
        Instance.UsedFixes++;
        if (!Helpers.shouldShowGhostInfo()) return;
        Helpers.showFlash(Instance.Color, 0.5f, "Engineer Fix");
    }

    public static void EngineerFixSubmergedOxygen()
    {
        Rpc_EngineerFixSubmergedOxygen(PlayerControl.LocalPlayer);
    }

    [MethodRpc((uint)Rpc.Role.EngineerFixSubmergedOxygen)]
    private static void Rpc_EngineerFixSubmergedOxygen(PlayerControl sender)
    {
        SubmergedCompatibility.RepairOxygen();
    }

    public static void EngineerFixLights()
    {
        Rpc_EngineerFixLights(PlayerControl.LocalPlayer);
    }

    [MethodRpc((uint)Rpc.Role.EngineerFixLights)]
    private static void Rpc_EngineerFixLights(PlayerControl sender)
    {
        var switchSystem = MapUtilities.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
        switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
    }
}