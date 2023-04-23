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

        CanCallEmergencyMeeting = OptionsTab.CreateBool(
            $"{Name}{nameof(CanCallEmergencyMeeting)}",
            Colors.Cs(Color, "Can call emergency meeting"),
            false,
            SpawnRate);
        HasImpostorVision = OptionsTab.CreateBool(
            $"{Name}{nameof(HasImpostorVision)}",
            Colors.Cs(Color, "Has impostor vision"),
            true,
            SpawnRate);
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        TriggerWin = false;
    }
}