using BetterOtherRoles.EnoFw.Kernel;
using Option = BetterOtherRoles.EnoFw.Kernel.CustomOption;

namespace BetterOtherRoles.EnoFw.Roles.Crewmate;

public class Spy : AbstractRole
{
    public static readonly Spy Instance = new();
    
    // Options
    public readonly CustomOption ImpostorsCanKillAnyone;
    public readonly CustomOption CanEnterVents;
    public readonly CustomOption HasImpostorVision;

    private Spy() : base(nameof(Spy), "Spy")
    {
        Team = Teams.Crewmate;
        Color = Palette.ImpostorRed;
        
        SpawnRate = GetDefaultSpawnRateOption();
        
        ImpostorsCanKillAnyone = Tab.CreateBool(
            $"{Key}{nameof(ImpostorsCanKillAnyone)}",
            Cs("Impostors can kill anyone if there is a spy"),
            true,
            SpawnRate);
        CanEnterVents = Tab.CreateBool(
            $"{Key}{nameof(CanEnterVents)}",
            Cs("Can enter vents"),
            true,
            SpawnRate);
        HasImpostorVision = Tab.CreateBool(
            $"{Key}{nameof(HasImpostorVision)}",
            Cs("Has impostor vision"),
            false,
            SpawnRate);
    }
}