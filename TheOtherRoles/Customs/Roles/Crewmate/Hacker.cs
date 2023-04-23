﻿using TheOtherRoles.Utilities;
using UnityEngine;

namespace TheOtherRoles.Customs.Roles.Crewmate;

public static class Hacker
{
    public static PlayerControl hacker;
    public static Minigame vitals = null;
    public static Minigame doorLog = null;
    public static Color color = new Color32(117, 250, 76, byte.MaxValue);

    public static float cooldown = 30f;
    public static float duration = 10f;
    public static float toolsNumber = 5f;
    public static bool onlyColorType = false;
    public static float hackerTimer = 0f;
    public static int rechargeTasksNumber = 2;
    public static int rechargedTasks = 2;
    public static int chargesVitals = 1;
    public static int chargesAdminTable = 1;
    public static bool cantMove = true;

    private static Sprite buttonSprite;
    private static Sprite vitalsSprite;
    private static Sprite logSprite;
    private static Sprite adminSprite;

    public static Sprite getButtonSprite()
    {
        if (buttonSprite) return buttonSprite;
        buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.HackerButton.png", 115f);
        return buttonSprite;
    }

    public static Sprite getVitalsSprite()
    {
        if (vitalsSprite) return vitalsSprite;
        vitalsSprite = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.VitalsButton]
            .Image;
        return vitalsSprite;
    }

    public static Sprite getLogSprite()
    {
        if (logSprite) return logSprite;
        logSprite = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.DoorLogsButton]
            .Image;
        return logSprite;
    }

    public static Sprite getAdminSprite()
    {
        byte mapId = GameOptionsManager.Instance.currentNormalGameOptions.MapId;
        UseButtonSettings button =
            FastDestroyableSingleton<HudManager>.Instance.UseButton
                .fastUseSettings[ImageNames.PolusAdminButton]; // Polus
        if (mapId == 0 || mapId == 3)
            button = FastDestroyableSingleton<HudManager>.Instance.UseButton
                .fastUseSettings[ImageNames.AdminMapButton]; // Skeld || Dleks
        else if (mapId == 1)
            button = FastDestroyableSingleton<HudManager>.Instance.UseButton
                .fastUseSettings[ImageNames.MIRAAdminButton]; // Mira HQ
        else if (mapId == 4)
            button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[
                ImageNames.AirshipAdminButton]; // Airship
        adminSprite = button.Image;
        return adminSprite;
    }

    public static void clearAndReload()
    {
        hacker = null;
        vitals = null;
        doorLog = null;
        hackerTimer = 0f;
        adminSprite = null;
        cooldown = CustomOptionHolder.hackerCooldown.getFloat();
        duration = CustomOptionHolder.hackerHackeringDuration.getFloat();
        onlyColorType = CustomOptionHolder.hackerOnlyColorType.getBool();
        toolsNumber = CustomOptionHolder.hackerToolsNumber.getFloat();
        rechargeTasksNumber = Mathf.RoundToInt(CustomOptionHolder.hackerRechargeTasksNumber.getFloat());
        rechargedTasks = Mathf.RoundToInt(CustomOptionHolder.hackerRechargeTasksNumber.getFloat());
        chargesVitals = Mathf.RoundToInt(CustomOptionHolder.hackerToolsNumber.getFloat()) / 2;
        chargesAdminTable = Mathf.RoundToInt(CustomOptionHolder.hackerToolsNumber.getFloat()) / 2;
        cantMove = CustomOptionHolder.hackerNoMove.getBool();
    }
}