using TheOtherRoles.EnoFramework.Kernel;
using UnityEngine;

namespace TheOtherRoles.Customs.Roles.Neutral;

[EnoSingleton]
public class ExJackal : CustomRole
{
    public ExJackal() : base(nameof(Sidekick), false)
    {
        Team = Teams.Neutral;
        Color = new Color32(0, 180, 235, byte.MaxValue);
    }
}