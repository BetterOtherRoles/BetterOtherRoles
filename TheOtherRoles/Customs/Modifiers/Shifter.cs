using TheOtherRoles.Customs.Roles;
using TheOtherRoles.Customs.Roles.Crewmate;
using UnityEngine;

namespace TheOtherRoles.Customs.Modifiers;

public static class Shifter
{
    public static PlayerControl? shifter;

    public static PlayerControl? futureShift;
    public static PlayerControl? currentTarget;

    private static Sprite buttonSprite;

    public static Sprite getButtonSprite()
    {
        if (buttonSprite) return buttonSprite;
        buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.ShiftButton.png", 115f);
        return buttonSprite;
    }

    public static void ShiftRole(PlayerControl player1, PlayerControl player2)
    {
        var player1Role = CustomRole.GetRoleByPlayer(player1);
        var player2Role = CustomRole.GetRoleByPlayer(player2);
        if (player1Role == null || player2Role == null) return;
        player1Role.Player = player2;
        player2Role.Player = player1;
    }

    public static void clearAndReload()
    {
        shifter = null;
        currentTarget = null;
        futureShift = null;
    }
}