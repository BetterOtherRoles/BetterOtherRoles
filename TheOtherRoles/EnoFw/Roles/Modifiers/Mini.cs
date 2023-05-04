﻿using System;
using UnityEngine;

namespace TheOtherRoles.EnoFw.Roles.Modifiers;

public static class Mini
{
    public static PlayerControl mini;
    public static Color color = Color.yellow;
    public const float defaultColliderRadius = 0.2233912f;
    public const float defaultColliderOffset = 0.3636057f;

    public static float growingUpDuration = 400f;
    public static bool isGrowingUpInMeeting = true;
    public static DateTime timeOfGrowthStart = DateTime.UtcNow;
    public static DateTime timeOfMeetingStart = DateTime.UtcNow;
    public static float ageOnMeetingStart = 0f;
    public static bool triggerMiniLose = false;

    public static void clearAndReload()
    {
        mini = null;
        triggerMiniLose = false;
        growingUpDuration = CustomOptionHolder.modifierMiniGrowingUpDuration.getFloat();
        isGrowingUpInMeeting = CustomOptionHolder.modifierMiniGrowingUpInMeeting.getBool();
        timeOfGrowthStart = DateTime.UtcNow;
    }

    public static float growingProgress()
    {
        float timeSinceStart = (float)(DateTime.UtcNow - timeOfGrowthStart).TotalMilliseconds;
        return Mathf.Clamp(timeSinceStart / (growingUpDuration * 1000), 0f, 1f);
    }

    public static bool isGrownUp()
    {
        return growingProgress() == 1f;
    }
}