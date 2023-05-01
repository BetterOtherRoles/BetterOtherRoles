using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.EnoFramework.Utils;

namespace TheOtherRoles.Customs.Roles.Crewmate;

[EnoSingleton]
public class Spy : CustomRole
{
    public readonly EnoFramework.CustomOption ImpostorsCanKillAnyone;
    public readonly EnoFramework.CustomOption CanDieToSheriff;
    public readonly EnoFramework.CustomOption CanEnterVents;
    public readonly EnoFramework.CustomOption HasImpostorVision;

    public Spy() : base(nameof(Spy))
    {
        Team = Teams.Crewmate;
        Color = Palette.ImpostorRed;

        IntroDescription = $"Confuse the {Colors.Cs(Palette.ImpostorRed, "impostors")}";
        ShortDescription = "Confuse the Impostors";

        ImpostorsCanKillAnyone = OptionsTab.CreateBool(
            $"{Name}{nameof(ImpostorsCanKillAnyone)}",
            Cs("Impostors can kill anyone if there is a spy"),
            true,
            SpawnRate);
        CanDieToSheriff = OptionsTab.CreateBool(
            $"{Name}{nameof(CanDieToSheriff)}",
            Cs("Can be killed by sheriff"),
            false,
            SpawnRate);
        CanEnterVents = OptionsTab.CreateBool(
            $"{Name}{nameof(CanEnterVents)}",
            Cs("Can enter vents"),
            true,
            SpawnRate);
        HasImpostorVision = OptionsTab.CreateBool(
            $"{Name}{nameof(HasImpostorVision)}",
            Cs("Has impostor vision"),
            false,
            SpawnRate);
        
    }
}