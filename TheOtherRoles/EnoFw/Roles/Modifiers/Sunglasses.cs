using TheOtherRoles.EnoFw.Kernel;
using TheOtherRoles.EnoFw.Utils;
using UnityEngine;
using Option = TheOtherRoles.EnoFw.Kernel.CustomOption;

namespace TheOtherRoles.EnoFw.Roles.Modifiers;

public class Sunglasses : AbstractMultipleModifier
{
    public static readonly Sunglasses Instance = new();

    public readonly Option Vision;

    private Sunglasses() : base(nameof(Sunglasses), "Sunglasses", Color.yellow)
    {
        Vision = CustomOptions.ModifierSettings.CreateFloatList(
            $"{Key}{nameof(Vision)}",
            Colors.Cs(Color, "Vision with sunglasses"),
            -50f,
            -10f,
            -30f,
            10f,
            SpawnRate,
            string.Empty,
            "%");
    }

}