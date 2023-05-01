using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.EnoFramework.Utils;
using UnityEngine;

namespace TheOtherRoles.Customs.Roles.Neutral;

[EnoSingleton]
public class ExJackal : CustomRole
{
    public ExJackal() : base(nameof(Sidekick), false)
    {
        Team = Teams.Neutral;
        Color = new Color32(0, 180, 235, byte.MaxValue);

        IntroDescription = $"Kill all Crewmates and {Colors.Cs(Palette.ImpostorRed, "impostors")} to win";
        ShortDescription = "Kill everyone";
    }
}