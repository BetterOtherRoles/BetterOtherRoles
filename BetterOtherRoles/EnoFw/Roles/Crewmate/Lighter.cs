using BetterOtherRoles.EnoFw.Kernel;
using UnityEngine;
using Option = BetterOtherRoles.EnoFw.Kernel.CustomOption;

namespace BetterOtherRoles.EnoFw.Roles.Crewmate;

public class Lighter : AbstractRole
{
    public static readonly Lighter Instance = new();
    
    // Options
    public readonly CustomOption LightsOnVision;
    public readonly CustomOption LightsOffVision;
    public readonly CustomOption VisionWidth;

    private Lighter() : base(nameof(Lighter), "Lighter")
    {
        Team = Teams.Crewmate;
        Color = new Color32(238, 229, 190, byte.MaxValue);
        
        SpawnRate = GetDefaultSpawnRateOption();
        
        LightsOnVision = Tab.CreateFloatList(
            $"{Name}{nameof(LightsOnVision)}",
            Cs("Vision when lights are on"),
            0.25f,
            5f,
            1.5f,
            0.25f,
            SpawnRate);
        LightsOffVision = Tab.CreateFloatList(
            $"{Name}{nameof(LightsOffVision)}",
            Cs("Vision when lights are off"),
            0.25f,
            5f,
            0.5f,
            0.25f,
            SpawnRate);
        VisionWidth = Tab.CreateFloatList(
            $"{Name}{nameof(VisionWidth)}",
            Cs("Flashlight width"),
            0.1f,
            1f,
            0.3f,
            0.1f,
            SpawnRate);
    }
}