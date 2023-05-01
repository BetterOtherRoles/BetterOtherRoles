using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.EnoFramework.Utils;
using UnityEngine;

namespace TheOtherRoles.Customs.Roles.Neutral;

[EnoSingleton]
public class Jester: CustomRole
{

    public readonly EnoFramework.CustomOption CanCallEmergencyMeeting;
    public readonly EnoFramework.CustomOption HasImpostorVision;

    public Jester() : base(nameof(Jester))
    {
        Team = Teams.Neutral;
        Color = new Color32(236, 98, 165, byte.MaxValue);

        IntroDescription = "Get voted out";
        ShortDescription = "Get voted out";

        CanCallEmergencyMeeting = OptionsTab.CreateBool(
            $"{Name}{nameof(CanCallEmergencyMeeting)}",
            Cs("Can call emergency meeting"),
            false,
            SpawnRate);
        HasImpostorVision = OptionsTab.CreateBool(
            $"{Name}{nameof(HasImpostorVision)}",
            Cs("Has impostor vision"),
            true,
            SpawnRate);
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        TriggerWin = false;
    }
}