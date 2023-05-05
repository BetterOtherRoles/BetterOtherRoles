﻿using Reactor.Networking.Attributes;
using UnityEngine;

namespace TheOtherRoles.EnoFw.Roles.Neutral;

public static class Sidekick
{
    public static PlayerControl sidekick;
    public static Color color = new Color32(0, 180, 235, byte.MaxValue);

    public static PlayerControl currentTarget;

    public static bool wasTeamRed;
    public static bool wasImpostor;
    public static bool wasSpy;

    public static float cooldown = 30f;
    public static bool canUseVents = true;
    public static bool canKill = true;
    public static bool promotesToJackal = true;
    public static bool hasImpostorVision;

    public static void clearAndReload()
    {
        sidekick = null;
        currentTarget = null;
        cooldown = CustomOptionHolder.jackalKillCooldown.getFloat();
        canUseVents = CustomOptionHolder.sidekickCanUseVents.getBool();
        canKill = CustomOptionHolder.sidekickCanKill.getBool();
        promotesToJackal = CustomOptionHolder.sidekickPromotesToJackal.getBool();
        hasImpostorVision = CustomOptionHolder.jackalAndSidekickHaveImpostorVision.getBool();
        wasTeamRed = wasImpostor = wasSpy = false;
    }

    public static void SidekickPromotes()
    {
        Rpc_SidekickPromotes(PlayerControl.LocalPlayer);
    }

    [MethodRpc((uint)Rpc.Role.SidekickPromotes)]
    private static void Rpc_SidekickPromotes(PlayerControl sender)
    {
        Local_SidekickPromotes();
    }

    public static void Local_SidekickPromotes()
    {
        Jackal.removeCurrentJackal();
        Jackal.jackal = sidekick;
        Jackal.canCreateSidekick = Jackal.jackalPromotedFromSidekickCanCreateSidekick;
        Jackal.wasTeamRed = wasTeamRed;
        Jackal.wasSpy = wasSpy;
        Jackal.wasImpostor = wasImpostor;
        clearAndReload();
    }
}