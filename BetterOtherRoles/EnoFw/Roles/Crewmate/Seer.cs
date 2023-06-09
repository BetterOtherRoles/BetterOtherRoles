﻿using System.Collections.Generic;
using BetterOtherRoles.EnoFw.Kernel;
using UnityEngine;
using Option = BetterOtherRoles.EnoFw.Kernel.CustomOption;

namespace BetterOtherRoles.EnoFw.Roles.Crewmate;

public class Seer : AbstractRole
{
    public static readonly Seer Instance = new();
    
    // Fields
    public readonly List<Vector3> DeadBodyPositions = new();

    // Options
    public readonly CustomOption ShowDeathFlash;
    public readonly CustomOption ShowDeathSouls;
    public readonly CustomOption LimitSoulsDuration;
    public readonly CustomOption SoulsDuration;

    public static Sprite SoulSprite => GetSprite("BetterOtherRoles.Resources.Soul.png", 500f);

    private Seer() : base(nameof(Seer), "Seer")
    {
        Team = Teams.Crewmate;
        Color = new Color32(97, 178, 108, byte.MaxValue);
        
        SpawnRate = GetDefaultSpawnRateOption();
        
        ShowDeathFlash = Tab.CreateBool(
            $"{Key}{nameof(ShowDeathFlash)}",
            Cs("Show flash on player death"),
            true,
            SpawnRate);
        ShowDeathSouls = Tab.CreateBool(
            $"{Key}{nameof(ShowDeathSouls)}",
            Cs("Show souls of death players"),
            true,
            SpawnRate);
        LimitSoulsDuration = Tab.CreateBool(
            $"{Key}{nameof(LimitSoulsDuration)}",
            Cs("Limit souls duration"),
            false,
            ShowDeathSouls);
        SoulsDuration = Tab.CreateFloatList(
            $"{Key}{nameof(SoulsDuration)}",
            Cs("Souls duration"),
            0f,
            120f,
            15f,
            5f,
            LimitSoulsDuration,
            string.Empty,
            "s");
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        DeadBodyPositions.Clear();
    }
}