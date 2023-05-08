using TheOtherRoles.EnoFw.Kernel;
using UnityEngine;
using Option = TheOtherRoles.EnoFw.Kernel.CustomOption;

namespace TheOtherRoles.EnoFw.Roles.Crewmate;

public class Detective : AbstractRole
{
    public static readonly Detective Instance = new();
    
    // Fields
    public float Timer = 6.2f;
    
    // Options
    public readonly Option AnonymousFootprint;
    public readonly Option FootprintInterval;
    public readonly Option FootprintDuration;
    public readonly Option ReportNameDuration;
    public readonly Option ReportColorDuration;

    private Detective() : base(nameof(Detective), "Detective")
    {
        Team = Teams.Crewmate;
        Color = new Color32(45, 106, 165, byte.MaxValue);

        SpawnRate = GetDefaultSpawnRateOption();
        
        AnonymousFootprint = Tab.CreateBool(
            $"{Key}{nameof(AnonymousFootprint)}",
            Cs("Anonymous footprints"),
            false,
            SpawnRate);
        FootprintInterval = Tab.CreateFloatList(
            $"{Key}{nameof(FootprintInterval)}",
            Cs("Footprint interval"),
            0.25f,
            10f,
            0.5f,
            0.25f,
            SpawnRate,
            string.Empty,
            "s");
        FootprintDuration = Tab.CreateFloatList(
            $"{Key}{nameof(FootprintDuration)}",
            Cs("Footprint duration"),
            0.25f,
            10f,
            5f,
            0.25f,
            SpawnRate,
            string.Empty,
            "s");
        ReportNameDuration = Tab.CreateFloatList(
            $"{Key}{nameof(ReportNameDuration)}",
            Cs("Time where detective reports will have name"),
            0f,
            60f,
            0f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        ReportColorDuration = Tab.CreateFloatList(
            $"{Key}{nameof(ReportColorDuration)}",
            Cs("Time where detective reports will have color type"),
            0f,
            120f,
            20f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        Timer = 6.2f;
    }
}