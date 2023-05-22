using BetterOtherRoles.EnoFw.Kernel;
using BetterOtherRoles.EnoFw.Utils;
using UnityEngine;
using Option = BetterOtherRoles.EnoFw.Kernel.CustomOption;

namespace BetterOtherRoles.EnoFw.Roles.Modifiers;

public class Invert : AbstractMultipleModifier
{
    public static readonly Invert Instance = new();

    public readonly CustomOption MeetingsOption;
    public int Meetings = 3;

    private Invert() : base(nameof(Invert), "Invert", Color.yellow)
    {
        MeetingsOption = CustomOptions.ModifierSettings.CreateFloatList(
            $"{Key}{nameof(MeetingsOption)}",
            CustomOptions.Cs(Color, "Number of meetings inverted"),
            1f,
            15f,
            3f,
            1f,
            SpawnRate);
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        Meetings = MeetingsOption;
    }
}