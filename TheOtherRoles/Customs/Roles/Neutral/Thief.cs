﻿using UnityEngine;

namespace TheOtherRoles.Customs.Roles.Neutral;

public static class Thief {
    public static PlayerControl thief;
    public static Color color = new Color32(71, 99, 45, byte.MaxValue);
    public static PlayerControl currentTarget;
    public static PlayerControl formerThief;

    public static float cooldown = 30f;

    public static bool suicideFlag = false;  // Used as a flag for suicide

    public static bool hasImpostorVision;
    public static bool canUseVents;
    public static bool canKillSheriff;

    public static void clearAndReload() {
        thief = null;
        suicideFlag = false;
        currentTarget = null;
        formerThief = null;
        hasImpostorVision = CustomOptionHolder.thiefHasImpVision.getBool();
        cooldown = CustomOptionHolder.thiefCooldown.getFloat();
        canUseVents = CustomOptionHolder.thiefCanUseVents.getBool();
        canKillSheriff = CustomOptionHolder.thiefCanKillSheriff.getBool();
    }
}