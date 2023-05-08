using TheOtherRoles.EnoFw.Kernel;
using Option = TheOtherRoles.EnoFw.Kernel.CustomOption;

namespace TheOtherRoles.EnoFw.Roles.Crewmate;

public class Spy : AbstractRole
{
    public static readonly Spy Instance = new();
    
    // Options
    public readonly Option ImpostorsCanKillAnyone;
    public readonly Option CanEnterVents;
    public readonly Option HasImpostorVision;

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