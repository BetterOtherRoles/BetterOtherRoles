using UnityEngine;

namespace TheOtherRoles.EnoFw.Roles.Crewmate;

public static class Sheriff
{
    public static PlayerControl sheriff;
    public static Color color = new Color32(248, 205, 70, byte.MaxValue);

    public static float cooldown = 30f;
    public static bool canKillNeutrals = false;
    public static bool spyCanDieToSheriff = false;

    public static PlayerControl currentTarget;

    public static PlayerControl formerDeputy; // Needed for keeping handcuffs + shifting
    public static PlayerControl formerSheriff; // When deputy gets promoted...

    public static void replaceCurrentSheriff(PlayerControl deputy)
    {
        if (!formerSheriff) formerSheriff = sheriff;
        sheriff = deputy;
        currentTarget = null;
        cooldown = CustomOptionHolder.sheriffCooldown.getFloat();
    }

    public static void clearAndReload()
    {
        sheriff = null;
        currentTarget = null;
        formerDeputy = null;
        formerSheriff = null;
        cooldown = CustomOptionHolder.sheriffCooldown.getFloat();
        canKillNeutrals = CustomOptionHolder.sheriffCanKillNeutrals.getBool();
        spyCanDieToSheriff = CustomOptionHolder.spyCanDieToSheriff.getBool();
    }
}