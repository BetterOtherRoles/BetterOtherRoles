﻿using System.Collections.Generic;
using System.Linq;
using TheOtherRoles.EnoFw.Kernel;
using TheOtherRoles.Objects;
using UnityEngine;
using Option = TheOtherRoles.EnoFw.Kernel.CustomOption;

namespace TheOtherRoles.EnoFw.Roles.Neutral;

public class Vulture : AbstractRole
{
    public static readonly Vulture Instance = new();
    
    // Fields
    public readonly List<Arrow> Arrows = new();
    public int EatenBodies;
    public bool TriggerVultureWin;

    // Options
    public readonly Option EatCooldown;
    public readonly Option EatNumberToWin;
    public readonly Option CanUseVents;
    public readonly Option ShowArrows;

    public static Sprite EatButtonSprite => GetSprite("TheOtherRoles.Resources.VultureButton.png", 115f);

    private Vulture() : base(nameof(Vulture), "Vulture")
    {
        Team = Teams.Neutral;
        Color = new Color32(139, 69, 19, byte.MaxValue);
        
        SpawnRate = GetDefaultSpawnRateOption();
        
        EatCooldown = Tab.CreateFloatList(
            $"{Key}{nameof(EatCooldown)}",
            Cs("Eat cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        EatNumberToWin = Tab.CreateFloatList(
            $"{Key}{nameof(EatNumberToWin)}",
            Cs("Number of corpses needed to be eaten"),
            1f,
            10f,
            4f,
            1f,
            SpawnRate,
            string.Empty,
            "s");
        CanUseVents = Tab.CreateBool(
            $"{Key}{nameof(CanUseVents)}",
            Cs("Can use vents"),
            true,
            SpawnRate);
        ShowArrows = Tab.CreateBool(
            $"{Key}{nameof(ShowArrows)}",
            Cs("Show arrows pointing towards the corpses"),
            true,
            SpawnRate);
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        EatenBodies = 0;
        TriggerVultureWin = false;
        foreach (var arrow in Arrows.Where(arrow => arrow.arrow != null))
        {
            UnityEngine.Object.Destroy(arrow.arrow);
        }
        Arrows.Clear();
    }
}