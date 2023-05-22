using BetterOtherRoles.EnoFw.Kernel;

namespace BetterOtherRoles.EnoFw.Roles.Impostor;

public class Mafioso : AbstractRole
{
    public static readonly Mafioso Instance = new();

    private Mafioso() : base(nameof(Mafioso), "Mafioso", false)
    {
        Team = Teams.Impostor;
        Color = Palette.ImpostorRed;
        CanTarget = false;
    }
}