using UnityEngine;

namespace TheOtherRoles.EnoFw.Roles.Neutral;

public static class Fallen
{
    public static PlayerControl fallen;
    public static Color color = Thief.color;

    public static void clearAndReload()
    {
        fallen = null;
    }
}