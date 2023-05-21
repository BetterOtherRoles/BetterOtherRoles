using BetterOtherRoles.EnoFw.Kernel;
using BetterOtherRoles.EnoFw.Utils;
using UnityEngine;
using Option = BetterOtherRoles.EnoFw.Kernel.CustomOption;

namespace BetterOtherRoles.EnoFw.Roles.Modifiers;

public class Vip : AbstractMultipleModifier
{
    public static readonly Vip Instance = new();

    public readonly CustomOption ShowTeamColor;

    private Vip() : base(nameof(Vip), "Vip", Color.yellow)
    {
        ShowTeamColor = CustomOptions.ModifierSettings.CreateBool(
            $"{Key}{nameof(ShowTeamColor)}",
            CustomOptions.Cs(Color, "Show team color"),
            false,
            SpawnRate);
    }

}