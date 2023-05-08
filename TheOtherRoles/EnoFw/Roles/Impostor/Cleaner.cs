using TheOtherRoles.EnoFw.Kernel;
using UnityEngine;
using Option = TheOtherRoles.EnoFw.Kernel.CustomOption;

namespace TheOtherRoles.EnoFw.Roles.Impostor;

public class Cleaner : AbstractRole
{
    public static readonly Cleaner Instance = new();
    
    // Options
    public readonly Option CleanCooldown;

    public static Sprite CleanButtonSprite => GetSprite("TheOtherRoles.Resources.CleanButton.png", 115f);

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