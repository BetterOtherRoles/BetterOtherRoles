using UnityEngine;

namespace TheOtherRoles.Customs.Roles.Crewmate;

public class Sheriff : CustomRole
{
    public static float cooldown = 30f;
    public static bool canKillNeutrals = false;
    public static bool spyCanDieToSheriff = false;

    public static PlayerControl currentTarget;

    public static PlayerControl formerDeputy; // Needed for keeping handcuffs + shifting
    public static PlayerControl formerSheriff; // When deputy gets promoted...

    public Sheriff() : base(nameof(Sheriff), new Color(248, 205, 70, byte.MaxValue), Teams.Crewmate)
    {
    }

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