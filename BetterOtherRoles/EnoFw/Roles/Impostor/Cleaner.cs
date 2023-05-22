using BetterOtherRoles.EnoFw.Kernel;
using UnityEngine;
using Option = BetterOtherRoles.EnoFw.Kernel.CustomOption;

namespace BetterOtherRoles.EnoFw.Roles.Impostor;

public class Cleaner : AbstractRole
{
    public static readonly Cleaner Instance = new();
    
    // Options
    public readonly CustomOption CleanCooldown;

    public static Sprite CleanButtonSprite => GetSprite("BetterOtherRoles.Resources.CleanButton.png", 115f);

    private Cleaner() : base(nameof(Cleaner), "Cleaner")
    {
        Team = Teams.Impostor;
        Color = Palette.ImpostorRed;
        CanTarget = true;
        
        SpawnRate = GetDefaultSpawnRateOption();
        
        CleanCooldown = Tab.CreateFloatList(
            $"{Key}{nameof(CleanCooldown)}",
            Cs($"Clean cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
    }
}