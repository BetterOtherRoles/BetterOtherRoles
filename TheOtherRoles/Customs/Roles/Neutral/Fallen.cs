using TheOtherRoles.EnoFramework.Kernel;
using UnityEngine;

namespace TheOtherRoles.Customs.Roles.Neutral;

[EnoSingleton]
public class Fallen : CustomRole
{
    public Fallen() : base(nameof(Sidekick), false)
    {
        Team = Teams.Neutral;
        Color = new Color32(71, 99, 45, byte.MaxValue);

        IntroDescription = "You no longer have power";
        ShortDescription = "You no longer have power";
    }
}