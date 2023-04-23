﻿using UnityEngine;

namespace TheOtherRoles.Customs.Roles.Crewmate;

public static class TimeMaster {
    public static PlayerControl timeMaster;
    public static Color color = new Color32(112, 142, 239, byte.MaxValue);

    public static bool reviveDuringRewind = false;
    public static float rewindTime = 3f;
    public static float shieldDuration = 3f;
    public static float cooldown = 30f;

    public static bool shieldActive = false;
    public static bool isRewinding = false;

    private static Sprite buttonSprite;
    public static Sprite getButtonSprite() {
        if (buttonSprite) return buttonSprite;
        buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.TimeShieldButton.png", 115f);
        return buttonSprite;
    }

    public static void clearAndReload() {
        timeMaster = null;
        isRewinding = false;
        shieldActive = false;
        rewindTime = CustomOptionHolder.timeMasterRewindTime.getFloat();
        shieldDuration = CustomOptionHolder.timeMasterShieldDuration.getFloat();
        cooldown = CustomOptionHolder.timeMasterCooldown.getFloat();
    }
}
