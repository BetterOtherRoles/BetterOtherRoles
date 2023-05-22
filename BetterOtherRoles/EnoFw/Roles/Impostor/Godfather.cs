using BetterOtherRoles.EnoFw.Kernel;

namespace BetterOtherRoles.EnoFw.Roles.Impostor;

public class Godfather : AbstractRole
{
    public static readonly Godfather Instance = new();

    private Godfather() : base(nameof(Godfather), "Godfather", false)
    {
        Team = Teams.Impostor;
        Color = Palette.ImpostorRed;
        CanTarget = true;
    }
}