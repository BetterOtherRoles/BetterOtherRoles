﻿using UnityEngine;

namespace TheOtherRoles.EnoFw.Roles.Impostor;

public static class Vampire
{
    public static PlayerControl vampire;
    public static Color color = Palette.ImpostorRed;

    public static float delay = 10f;
    public static float cooldown = 30f;
    public static bool canKillNearGarlics = true;
    public static bool localPlacedGarlic = false;
    public static bool garlicsActive = true;

    public static PlayerControl currentTarget;
    public static PlayerControl bitten;
    public static bool targetNearGarlic = false;

    private static Sprite buttonSprite;

    public static Sprite getButtonSprite()
    {
        if (buttonSprite) return buttonSprite;
        buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.VampireButton.png", 115f);
        return buttonSprite;
    }

    private static Sprite garlicButtonSprite;

    public static Sprite getGarlicButtonSprite()
    {
        if (garlicButtonSprite) return garlicButtonSprite;
        garlicButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.GarlicButton.png", 115f);
        return garlicButtonSprite;
    }

    public static void clearAndReload()
    {
        vampire = null;
        bitten = null;
        targetNearGarlic = false;
        localPlacedGarlic = false;
        currentTarget = null;
        garlicsActive = CustomOptionHolder.vampireSpawnRate.getSelection() > 0;
        delay = CustomOptionHolder.vampireKillDelay.getFloat();
        cooldown = CustomOptionHolder.vampireCooldown.getFloat();
        canKillNearGarlics = CustomOptionHolder.vampireCanKillNearGarlics.getBool();
    }
}