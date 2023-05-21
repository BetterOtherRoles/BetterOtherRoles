using BetterOtherRoles.EnoFw.Kernel;
using UnityEngine;
using Option = BetterOtherRoles.EnoFw.Kernel.CustomOption;

namespace BetterOtherRoles.EnoFw.Roles.Impostor;

public class Janitor : AbstractRole
{
    public static readonly Janitor Instance = new();
    
    // Options
    public readonly CustomOption CleanCooldown;

    public static Sprite CleanButtonSprite => GetSprite("BetterOtherRoles.Resources.CleanButton.png", 115f);

    private Janitor() : base(nameof(Janitor), "Janitor", false)
    {
        Team = Teams.Impostor;
        Color = Palette.ImpostorRed;
        CanTarget = true;
        
        CleanCooldown = Tab.CreateFloatList(
            $"{Key}{nameof(CleanCooldown)}",
            Cs($"Clean cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            CustomOptions.MafiaSpawnRate,
            string.Empty,
            "s");
    }
}