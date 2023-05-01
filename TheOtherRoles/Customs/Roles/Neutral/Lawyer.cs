using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.EnoFramework.Utils;
using UnityEngine;

namespace TheOtherRoles.Customs.Roles.Neutral;

[EnoSingleton]
public class Lawyer : CustomRole
{
    public readonly EnoFramework.CustomOption Vision;
    public readonly EnoFramework.CustomOption KnowsTargetRole;
    public readonly EnoFramework.CustomOption CanCallEmergencyMeeting;
    public readonly EnoFramework.CustomOption TargetCanBeJester;
    
    public PlayerControl? Target;

    public Lawyer() : base(nameof(Lawyer))
    {
        Team = Teams.Neutral;
        Color = new Color32(134, 153, 25, byte.MaxValue);
        IncompatibleRoles.Add(typeof(Prosecutor));

        IntroDescription = "Defend your client";
        ShortDescription = "Defend your client";

        Vision = OptionsTab.CreateFloatList(
            $"{Name}{Vision}",
            Cs("Vision"),
            0.25f,
            3f,
            1f,
            0.25f,
            SpawnRate);
        KnowsTargetRole = OptionsTab.CreateBool(
            $"{Name}{KnowsTargetRole}",
            Cs("Knows target role"),
            false,
            SpawnRate);
        CanCallEmergencyMeeting = OptionsTab.CreateBool(
            $"{Name}{CanCallEmergencyMeeting}",
            Cs("Can call emergency meeting"),
            false,
            SpawnRate);
        TargetCanBeJester = OptionsTab.CreateBool(
            $"{Name}{TargetCanBeJester}",
            Cs("Target can be jester"),
            false,
            SpawnRate);
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        Target = null;
    }
}