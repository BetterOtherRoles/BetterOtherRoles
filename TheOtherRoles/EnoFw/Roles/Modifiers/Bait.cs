﻿using System.Collections.Generic;
using TheOtherRoles.EnoFw.Kernel;
using TheOtherRoles.EnoFw.Utils;
using UnityEngine;
using Option = TheOtherRoles.EnoFw.Kernel.CustomOption;

namespace TheOtherRoles.EnoFw.Roles.Modifiers;

public class Bait : AbstractMultipleModifier
{
    public static readonly Bait Instance = new();
    
    public readonly Dictionary<DeadPlayer, float> Active = new();

    public readonly Option ReportDelayMinOption;
    public readonly Option ReportDelayMaxOption;
    public readonly Option ShowKillFlash;

    public float ReportDelayMin { get; private set; }
    public float ReportDelayMax { get; private set; }

    private Bait() : base(nameof(Bait), "Bait", Color.yellow)
    {
        ReportDelayMinOption = CustomOptions.ModifierSettings.CreateFloatList(
            $"{Key}{nameof(ReportDelayMinOption)}",
            Colors.Cs(Color, "Report delay min"),
            0f,
            10f,
            0f,
            1f,
            SpawnRate,
            string.Empty,
            "s");
        ReportDelayMaxOption = CustomOptions.ModifierSettings.CreateFloatList(
            $"{Key}{nameof(ReportDelayMaxOption)}",
            Colors.Cs(Color, "Report delay max"),
            0f,
            10f,
            0f,
            1f,
            SpawnRate,
            string.Empty,
            "s");
        ShowKillFlash = CustomOptions.ModifierSettings.CreateBool(
            $"{Key}{nameof(ShowKillFlash)}",
            Colors.Cs(Color, "Warn the killer with a flash"),
            false,
            SpawnRate);
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        Active.Clear();
        ReportDelayMin = ReportDelayMinOption;
        ReportDelayMax = ReportDelayMaxOption;
        if (ReportDelayMin > ReportDelayMax) ReportDelayMin = ReportDelayMax;
    }
}