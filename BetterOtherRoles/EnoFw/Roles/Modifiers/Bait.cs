using System.Collections.Generic;
using BetterOtherRoles.EnoFw.Kernel;
using BetterOtherRoles.EnoFw.Utils;
using UnityEngine;
using Option = BetterOtherRoles.EnoFw.Kernel.CustomOption;

namespace BetterOtherRoles.EnoFw.Roles.Modifiers;

public class Bait : AbstractMultipleModifier
{
    public static readonly Bait Instance = new();
    
    public readonly Dictionary<DeadPlayer, float> Active = new();

    public readonly CustomOption ReportDelayMinOption;
    public readonly CustomOption ReportDelayMaxOption;
    public readonly CustomOption ShowKillFlash;

    public float ReportDelayMin { get; private set; }
    public float ReportDelayMax { get; private set; }

    private Bait() : base(nameof(Bait), "Bait", Color.yellow)
    {
        ReportDelayMinOption = CustomOptions.ModifierSettings.CreateFloatList(
            $"{Key}{nameof(ReportDelayMinOption)}",
            CustomOptions.Cs(Color, "Report delay min"),
            0f,
            10f,
            0f,
            1f,
            SpawnRate,
            string.Empty,
            "s");
        ReportDelayMaxOption = CustomOptions.ModifierSettings.CreateFloatList(
            $"{Key}{nameof(ReportDelayMaxOption)}",
            CustomOptions.Cs(Color, "Report delay max"),
            0f,
            10f,
            0f,
            1f,
            SpawnRate,
            string.Empty,
            "s");
        ShowKillFlash = CustomOptions.ModifierSettings.CreateBool(
            $"{Key}{nameof(ShowKillFlash)}",
            CustomOptions.Cs(Color, "Warn the killer with a flash"),
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