using TheOtherRoles.EnoFw.Kernel;
using UnityEngine;

namespace TheOtherRoles.EnoFw.Roles.Neutral;

public class Fallen : AbstractRole
{
    public static readonly Fallen Instance = new();

    private Fallen() : base(nameof(Fallen), "Fallen", false)
    {
        Team = Teams.Neutral;
        Color = new Color32(71, 99, 45, byte.MaxValue);
    }
}