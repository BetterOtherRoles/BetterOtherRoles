using TheOtherRoles.EnoFw.Kernel;
using TheOtherRoles.EnoFw.Utils;
using UnityEngine;
using Option = TheOtherRoles.EnoFw.Kernel.CustomOption;

namespace TheOtherRoles.EnoFw.Roles.Modifiers;

public class Vip : AbstractMultipleModifier
{
    public static readonly Vip Instance = new();

    public readonly Option ShowTeamColor;

    private Vip() : base(nameof(Vip), "Vip", Color.yellow)
    {
        ShowTeamColor = CustomOptions.ModifierSettings.CreateBool(
            $"{Key}{nameof(ShowTeamColor)}",
            Colors.Cs(Color, "Show team color"),
            false,
            SpawnRate);
    }

}