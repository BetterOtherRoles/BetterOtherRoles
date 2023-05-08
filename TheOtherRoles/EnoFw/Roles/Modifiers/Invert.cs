using TheOtherRoles.EnoFw.Kernel;
using TheOtherRoles.EnoFw.Utils;
using UnityEngine;
using Option = TheOtherRoles.EnoFw.Kernel.CustomOption;

namespace TheOtherRoles.EnoFw.Roles.Modifiers;

public class Invert : AbstractMultipleModifier
{
    public static readonly Invert Instance = new();

    public readonly Option MeetingsOption;
    public int Meetings = 3;

    private Invert() : base(nameof(Invert), "Invert", Color.yellow)
    {
        MeetingsOption = CustomOptions.ModifierSettings.CreateFloatList(
            $"{Key}{nameof(MeetingsOption)}",
            Colors.Cs(Color, "Number of meetings inverted"),
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