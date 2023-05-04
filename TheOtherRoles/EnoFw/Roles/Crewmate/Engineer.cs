using Reactor.Networking.Attributes;
using TheOtherRoles.Utilities;
using UnityEngine;

namespace TheOtherRoles.EnoFw.Roles.Crewmate;

public static class Engineer
{
    public static PlayerControl engineer;
    public static Color color = new Color32(0, 40, 245, byte.MaxValue);
    private static Sprite buttonSprite;

    public static int remainingFixes = 1;
    public static bool highlightForImpostors = true;
    public static bool highlightForTeamJackal = true;

    public static Sprite getButtonSprite()
    {
        if (buttonSprite) return buttonSprite;
        buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.RepairButton.png", 115f);
        return buttonSprite;
    }

    public static void clearAndReload()
    {
        engineer = null;
        remainingFixes = Mathf.RoundToInt(CustomOptionHolder.engineerNumberOfFixes.getFloat());
        highlightForImpostors = CustomOptionHolder.engineerHighlightForImpostors.getBool();
        highlightForTeamJackal = CustomOptionHolder.engineerHighlightForTeamJackal.getBool();
    }

    public static void EngineerUsedRepair()
    {
        Rpc_EngineerUsedRepair(PlayerControl.LocalPlayer);
    }

    [MethodRpc((uint)Rpc.Role.EngineerUsedRepair)]
    private static void Rpc_EngineerUsedRepair(PlayerControl sender)
    {
        remainingFixes--;
        if (!Helpers.shouldShowGhostInfo()) return;
        Helpers.showFlash(color, 0.5f, "Engineer Fix");
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