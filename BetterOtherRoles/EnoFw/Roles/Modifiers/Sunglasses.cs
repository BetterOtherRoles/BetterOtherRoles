using BetterOtherRoles.EnoFw.Kernel;
using BetterOtherRoles.EnoFw.Utils;
using UnityEngine;
using Option = BetterOtherRoles.EnoFw.Kernel.CustomOption;

namespace BetterOtherRoles.EnoFw.Roles.Modifiers;

public class Sunglasses : AbstractMultipleModifier
{
    public static readonly Sunglasses Instance = new();

    public readonly CustomOption Vision;

    private Sunglasses() : base(nameof(Sunglasses), "Sunglasses", Color.yellow)
    {
        Vision = CustomOptions.ModifierSettings.CreateFloatList(
            $"{Key}{nameof(Vision)}",
            CustomOptions.Cs(Color, "Vision with sunglasses"),
            -50f,
            -10f,
            -30f,
            10f,
            SpawnRate,
            string.Empty,
            "%");
    }

}