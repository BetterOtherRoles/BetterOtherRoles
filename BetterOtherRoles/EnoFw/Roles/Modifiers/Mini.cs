using System;
using BetterOtherRoles.EnoFw.Kernel;
using BetterOtherRoles.EnoFw.Utils;
using UnityEngine;
using Option = BetterOtherRoles.EnoFw.Kernel.CustomOption;

namespace BetterOtherRoles.EnoFw.Roles.Modifiers;

public class Mini : AbstractSimpleModifier
{
    public const float DefaultColliderRadius = 0.2233912f;
    public const float DefaultColliderOffset = 0.3636057f;
    
    public static readonly Mini Instance = new();

    public DateTime TimeOfGrowthStart = DateTime.UtcNow;
    public DateTime TimeOfMeetingStart = DateTime.UtcNow;
    public float AgeOnMeetingStart;
    public bool TriggerMiniLose;

    public readonly CustomOption GrowingUpDuration;
    public readonly CustomOption IsGrowingUpInMeeting;

    private Mini() : base(nameof(Mini), "Mini", Color.yellow)
    {
        GrowingUpDuration = CustomOptions.ModifierSettings.CreateFloatList(
            $"{Key}{nameof(GrowingUpDuration)}",
            CustomOptions.Cs(Color, "Growing up duration"),
            60f,
            1200f,
            400f,
            10f,
            SpawnRate,
            string.Empty,
            "s");
        IsGrowingUpInMeeting = CustomOptions.ModifierSettings.CreateBool(
            $"{Key}{nameof(IsGrowingUpInMeeting)}",
            CustomOptions.Cs(Color, "Grows up in meeting"),
            true,
            SpawnRate);
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        TriggerMiniLose = false;
        TimeOfGrowthStart = DateTime.UtcNow;
    }

    public float GrowingProgress()
    {
        var timeSinceStart = (float)(DateTime.UtcNow - TimeOfGrowthStart).TotalMilliseconds;
        return Mathf.Clamp(timeSinceStart / (GrowingUpDuration * 1000f), 0f, 1f);
    }

    public bool IsGrownUp => GrowingProgress() >= 1f;
}