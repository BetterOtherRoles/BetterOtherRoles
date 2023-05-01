using TheOtherRoles.EnoFramework.Kernel;
using UnityEngine;

namespace TheOtherRoles.Customs.Roles.Neutral;

[EnoSingleton]
public class Sidekick : CustomRole
{
    public Sidekick() : base(nameof(Sidekick), false)
    {
        Team = Teams.Neutral;
        Color = new Color32(0, 180, 235, byte.MaxValue);

        IntroDescription = "Help your Jackal to kill everyone";
        ShortDescription = "Help your Jackal to kill everyone";
    }

    public void PromoteToJackal()
    {
        if (!Singleton<Jackal>.Instance.SidekickPromoteToJackal) return;
        Singleton<ExJackal>.Instance.Player = Singleton<Jackal>.Instance.Player;
        Singleton<Jackal>.Instance.Player = Player;
        Singleton<Jackal>.Instance.IsSidekickPromoted = true;
        Player = null;
    }
}