using System.Linq;
using System;
using System.Collections.Generic;
using TheOtherRoles.Players;
using static TheOtherRoles.TheOtherRoles;
using UnityEngine;
using TheOtherRoles.Utilities;
using TheOtherRoles.CustomGameModes;
using TheOtherRoles.Customs.Modifiers;
using TheOtherRoles.Customs.Roles.Crewmate;
using TheOtherRoles.Customs.Roles.Impostor;
using TheOtherRoles.Customs.Roles.Neutral;
using TheOtherRoles.EnoFramework.Kernel;

namespace TheOtherRoles;

class RoleInfo
{
    public Color color;
    public string name;
    public string introDescription;
    public string shortDescription;
    public RoleId roleId;
    public bool isNeutral;
    public bool isModifier;

    RoleInfo(string name, Color color, string introDescription, string shortDescription, RoleId roleId,
        bool isNeutral = false, bool isModifier = false)
    {
        this.color = color;
        this.name = name;
        this.introDescription = introDescription;
        this.shortDescription = shortDescription;
        this.roleId = roleId;
        this.isNeutral = isNeutral;
        this.isModifier = isModifier;
    }

    public static RoleInfo jester = new RoleInfo("Jester", Singleton<Jester>.Instance.Color, "Get voted out",
        "Get voted out", RoleId.Jester, true);

    public static RoleInfo mayor = new RoleInfo("Mayor", Singleton<Mayor>.Instance.Color, "Your vote counts twice",
        "Your vote counts twice", RoleId.Mayor);

    public static RoleInfo portalmaker = new RoleInfo("Portalmaker", Singleton<Portalmaker>.Instance.Color,
        "You can create portals", "You can create portals", RoleId.Portalmaker);

    public static RoleInfo engineer = new RoleInfo("Engineer", Singleton<Engineer>.Instance.Color, "Maintain important systems on the ship",
        "Repair the ship", RoleId.Engineer);

    public static RoleInfo sheriff = new RoleInfo("Sheriff", Singleton<Sheriff>.Instance.Color,
        "Shoot the <color=#FF1919FF>Impostors</color>", "Shoot the Impostors", RoleId.Sheriff);

    public static RoleInfo deputy = new RoleInfo("Deputy", Singleton<Deputy>.Instance.Color,
        "Handcuff the <color=#FF1919FF>Impostors</color>", "Handcuff the Impostors", RoleId.Deputy);

    public static RoleInfo lighter = new RoleInfo("Lighter", Singleton<Lighter>.Instance.Color, "Your light never goes out",
        "Your light never goes out", RoleId.Lighter);

    public static RoleInfo morphling = new RoleInfo("Morphling", Singleton<Morphling>.Instance.Color, "Change your look to not get caught",
        "Change your look", RoleId.Morphling);

    public static RoleInfo camouflager = new RoleInfo("Camouflager", Singleton<Camouflager>.Instance.Color,
        "Camouflage and kill the Crewmates", "Hide among others", RoleId.Camouflager);

    public static RoleInfo vampire = new RoleInfo("Vampire", Singleton<Vampire>.Instance.Color,
        "Kill the Crewmates with your bites", "Bite your enemies", RoleId.Vampire);

    public static RoleInfo eraser = new RoleInfo("Eraser", Singleton<Eraser>.Instance.Color, "Kill the Crewmates and erase their roles",
        "Erase the roles of your enemies", RoleId.Eraser);

    public static RoleInfo trickster = new RoleInfo("Trickster", Singleton<Trickster>.Instance.Color,
        "Use your jack-in-the-boxes to surprise others", "Surprise your enemies", RoleId.Trickster);

    public static RoleInfo cleaner = new RoleInfo("Cleaner", Singleton<Cleaner>.Instance.Color, "Kill everyone and leave no traces",
        "Clean up dead bodies", RoleId.Cleaner);

    public static RoleInfo warlock = new RoleInfo("Warlock", Singleton<Warlock>.Instance.Color,
        "Curse other players and kill everyone", "Curse and kill everyone", RoleId.Warlock);

    public static RoleInfo bountyHunter = new RoleInfo("Bounty Hunter", Singleton<BountyHunter>.Instance.Color, "Hunt your bounty down",
        "Hunt your bounty down", RoleId.BountyHunter);

    public static RoleInfo detective = new RoleInfo("Detective", Singleton<Detective>.Instance.Color,
        "Find the <color=#FF1919FF>Impostors</color> by examining footprints", "Examine footprints", RoleId.Detective);

    public static RoleInfo timeMaster = new RoleInfo("Time Master", Singleton<TimeMaster>.Instance.Color,
        "Save yourself with your time shield", "Use your time shield", RoleId.TimeMaster);

    public static RoleInfo medic = new RoleInfo("Medic", Singleton<Medic>.Instance.Color, "Protect someone with your shield",
        "Protect other players", RoleId.Medic);

    public static RoleInfo swapper = new RoleInfo("Swapper", Singleton<Swapper>.Instance.Color,
        "Swap votes to exile the <color=#FF1919FF>Impostors</color>", "Swap votes", RoleId.Swapper);

    public static RoleInfo seer = new RoleInfo("Seer", Singleton<Seer>.Instance.Color, "You will see players die",
        "You will see players die", RoleId.Seer);

    public static RoleInfo hacker = new RoleInfo("Hacker", Singleton<Hacker>.Instance.Color,
        "Hack systems to find the <color=#FF1919FF>Impostors</color>", "Hack to find the Impostors", RoleId.Hacker);

    public static RoleInfo tracker = new RoleInfo("Tracker", Singleton<Tracker>.Instance.Color,
        "Track the <color=#FF1919FF>Impostors</color> down", "Track the Impostors down", RoleId.Tracker);

    public static RoleInfo snitch = new RoleInfo("Snitch", Singleton<Snitch>.Instance.Color,
        "Finish your tasks to find the <color=#FF1919FF>Impostors</color>", "Finish your tasks", RoleId.Snitch);

    public static RoleInfo jackal = new RoleInfo("Jackal", Singleton<Jackal>.Instance.Color,
        "Kill all Crewmates and <color=#FF1919FF>Impostors</color> to win", "Kill everyone", RoleId.Jackal, true);

    public static RoleInfo sidekick = new RoleInfo("Sidekick", Singleton<Sidekick>.Instance.Color, "Help your Jackal to kill everyone",
        "Help your Jackal to kill everyone", RoleId.Sidekick, true);

    public static RoleInfo spy = new RoleInfo("Spy", Singleton<Spy>.Instance.Color, "Confuse the <color=#FF1919FF>Impostors</color>",
        "Confuse the Impostors", RoleId.Spy);

    public static RoleInfo securityGuard = new RoleInfo("Security Guard", Singleton<SecurityGuard>.Instance.Color,
        "Seal vents and place cameras", "Seal vents and place cameras", RoleId.SecurityGuard);

    public static RoleInfo arsonist = new RoleInfo("Arsonist", Singleton<Arsonist>.Instance.Color, "Let them burn", "Let them burn",
        RoleId.Arsonist, true);

    public static RoleInfo goodGuesser = new RoleInfo("Nice Guesser", Singleton<NiceGuesser>.Instance.Color, "Guess and shoot",
        "Guess and shoot", RoleId.NiceGuesser);

    public static RoleInfo badGuesser = new RoleInfo("Evil Guesser", Singleton<EvilGuesser>.Instance.Color, "Guess and shoot",
        "Guess and shoot", RoleId.EvilGuesser);

    public static RoleInfo vulture = new RoleInfo("Vulture", Singleton<Vulture>.Instance.Color, "Eat corpses to win", "Eat dead bodies",
        RoleId.Vulture, true);

    public static RoleInfo medium = new RoleInfo("Medium", Singleton<Medium>.Instance.Color,
        "Question the souls of the dead to gain information", "Question the souls", RoleId.Medium);

    public static RoleInfo trapper = new RoleInfo("Trapper", Singleton<Trapper>.Instance.Color, "Place traps to find the Impostors",
        "Place traps", RoleId.Trapper);

    public static RoleInfo lawyer = new RoleInfo("Lawyer", Singleton<Lawyer>.Instance.Color, "Defend your client", "Defend your client",
        RoleId.Lawyer, true);

    public static RoleInfo prosecutor = new RoleInfo("Prosecutor", Singleton<Prosecutor>.Instance.Color, "Vote out your target",
        "Vote out your target", RoleId.Prosecutor, true);

    public static RoleInfo pursuer = new RoleInfo("Pursuer", Singleton<Pursuer>.Instance.Color, "Blank the Impostors",
        "Blank the Impostors", RoleId.Pursuer);

    public static RoleInfo impostor = new RoleInfo("Impostor", Palette.ImpostorRed,
        Helpers.cs(Palette.ImpostorRed, "Sabotage and kill everyone"), "Sabotage and kill everyone", RoleId.Impostor);

    public static RoleInfo crewmate = new RoleInfo("Crewmate", Color.white, "Find the Impostors", "Find the Impostors",
        RoleId.Crewmate);

    public static RoleInfo witch = new RoleInfo("Witch", Singleton<Witch>.Instance.Color, "Cast a spell upon your foes",
        "Cast a spell upon your foes", RoleId.Witch);

    public static RoleInfo ninja = new RoleInfo("Ninja", Singleton<Ninja>.Instance.Color, "Surprise and assassinate your foes",
        "Surprise and assassinate your foes", RoleId.Ninja);

    public static RoleInfo thief = new RoleInfo("Thief", Singleton<Thief>.Instance.Color, "Steal a killers role by killing them",
        "Steal a killers role", RoleId.Thief, true);

    public static RoleInfo bomber =
        new RoleInfo("Bomber", Singleton<Bomber>.Instance.Color, "Bomb all Crewmates", "Bomb all Crewmates", RoleId.Bomber);

    public static RoleInfo hunter = new RoleInfo("Hunter", Palette.ImpostorRed,
        Helpers.cs(Palette.ImpostorRed, "Seek and kill everyone"), "Seek and kill everyone", RoleId.Impostor);

    public static RoleInfo hunted = new RoleInfo("Hunted", Color.white, "Hide", "Hide", RoleId.Crewmate);


    // Modifier
    public static RoleInfo bloody = new RoleInfo("Bloody", Color.yellow, "Your killer leaves a bloody trail",
        "Your killer leaves a bloody trail", RoleId.Bloody, false, true);

    public static RoleInfo antiTeleport = new RoleInfo("Anti tp", Color.yellow, "You will not get teleported",
        "You will not get teleported", RoleId.AntiTeleport, false, true);

    public static RoleInfo tiebreaker = new RoleInfo("Tiebreaker", Color.yellow, "Your vote breaks the tie",
        "Break the tie", RoleId.Tiebreaker, false, true);

    public static RoleInfo bait = new RoleInfo("Bait", Color.yellow, "Bait your enemies", "Bait your enemies",
        RoleId.Bait, false, true);

    public static RoleInfo sunglasses = new RoleInfo("Sunglasses", Color.yellow, "You got the sunglasses",
        "Your vision is reduced", RoleId.Sunglasses, false, true);

    public static RoleInfo lover = new RoleInfo("Lover", Lovers.color, $"You are in love", $"You are in love",
        RoleId.Lover, false, true);

    public static RoleInfo mini = new RoleInfo("Mini", Color.yellow, "No one will harm you until you grow up",
        "No one will harm you", RoleId.Mini, false, true);

    public static RoleInfo vip = new RoleInfo("VIP", Color.yellow, "You are the VIP",
        "Everyone is notified when you die", RoleId.Vip, false, true);

    public static RoleInfo invert = new RoleInfo("Invert", Color.yellow, "Your movement is inverted",
        "Your movement is inverted", RoleId.Invert, false, true);

    public static RoleInfo chameleon = new RoleInfo("Chameleon", Color.yellow, "You're hard to see when not moving",
        "You're hard to see when not moving", RoleId.Chameleon, false, true);

    public static RoleInfo shifter = new RoleInfo("Shifter", Color.yellow, "Shift your role", "Shift your role",
        RoleId.Shifter, false, true);


    public static List<RoleInfo> allRoleInfos = new List<RoleInfo>()
    {
        impostor,
        morphling,
        camouflager,
        vampire,
        eraser,
        trickster,
        cleaner,
        warlock,
        bountyHunter,
        witch,
        ninja,
        bomber,
        goodGuesser,
        badGuesser,
        lover,
        jester,
        arsonist,
        jackal,
        sidekick,
        vulture,
        pursuer,
        lawyer,
        thief,
        prosecutor,
        crewmate,
        mayor,
        portalmaker,
        engineer,
        sheriff,
        deputy,
        lighter,
        detective,
        timeMaster,
        medic,
        swapper,
        seer,
        hacker,
        tracker,
        snitch,
        spy,
        securityGuard,
        bait,
        medium,
        trapper,
        bloody,
        antiTeleport,
        tiebreaker,
        sunglasses,
        mini,
        vip,
        invert,
        chameleon,
        shifter
    };

    public static List<RoleInfo> getRoleInfoForPlayer(PlayerControl p, bool showModifier = true)
    {
        List<RoleInfo> infos = new List<RoleInfo>();
        if (p == null) return infos;

        // Modifier
        if (showModifier)
        {
            // after dead modifier
            if (!CustomOptionHolder.modifiersAreHidden.getBool() || PlayerControl.LocalPlayer.Data.IsDead ||
                AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Ended)
            {
                if (Bait.bait.Any(x => x.PlayerId == p.PlayerId)) infos.Add(bait);
                if (Bloody.bloody.Any(x => x.PlayerId == p.PlayerId)) infos.Add(bloody);
                if (Vip.vip.Any(x => x.PlayerId == p.PlayerId)) infos.Add(vip);
            }

            if (p == Lovers.lover1 || p == Lovers.lover2) infos.Add(lover);
            if (p == Tiebreaker.tiebreaker) infos.Add(tiebreaker);
            if (AntiTeleport.antiTeleport.Any(x => x.PlayerId == p.PlayerId)) infos.Add(antiTeleport);
            if (Sunglasses.sunglasses.Any(x => x.PlayerId == p.PlayerId)) infos.Add(sunglasses);
            if (p == Mini.mini) infos.Add(mini);
            if (Invert.invert.Any(x => x.PlayerId == p.PlayerId)) infos.Add(invert);
            if (Chameleon.chameleon.Any(x => x.PlayerId == p.PlayerId)) infos.Add(chameleon);
            if (p == Shifter.shifter) infos.Add(shifter);
        }

        int count = infos.Count; // Save count after modifiers are added so that the role count can be checked

        // Special roles
        if (p == Singleton<Jester>.Instance.Player) infos.Add(jester);
        if (p == Singleton<Mayor>.Instance.Player) infos.Add(mayor);
        if (p == Singleton<Portalmaker>.Instance.Player) infos.Add(portalmaker);
        if (p == Singleton<Engineer>.Instance.Player) infos.Add(engineer);
        if (p == Singleton<Sheriff>.Instance.Player) infos.Add(sheriff);
        if (p == Singleton<Deputy>.Instance.Player) infos.Add(deputy);
        if (p == Singleton<Lighter>.Instance.Player) infos.Add(lighter);
        if (p == Singleton<Morphling>.Instance.Player) infos.Add(morphling);
        if (p == Singleton<Camouflager>.Instance.Player) infos.Add(camouflager);
        if (p == Singleton<Vampire>.Instance.Player) infos.Add(vampire);
        if (p == Singleton<Eraser>.Instance.Player) infos.Add(eraser);
        if (p == Singleton<Trickster>.Instance.Player) infos.Add(trickster);
        if (p == Singleton<Cleaner>.Instance.Player) infos.Add(cleaner);
        if (p == Singleton<Warlock>.Instance.Player) infos.Add(warlock);
        if (p == Singleton<Witch>.Instance.Player) infos.Add(witch);
        if (p == Singleton<Ninja>.Instance.Player) infos.Add(ninja);
        if (p == Singleton<Bomber>.Instance.Player) infos.Add(bomber);
        if (p == Singleton<Detective>.Instance.Player) infos.Add(detective);
        if (p == Singleton<TimeMaster>.Instance.Player) infos.Add(timeMaster);
        if (p == Singleton<Medic>.Instance.Player) infos.Add(medic);
        if (p == Singleton<Swapper>.Instance.Player) infos.Add(swapper);
        if (p == Singleton<Seer>.Instance.Player) infos.Add(seer);
        if (p == Singleton<Hacker>.Instance.Player) infos.Add(hacker);
        if (p == Singleton<Tracker>.Instance.Player) infos.Add(tracker);
        if (p == Singleton<Snitch>.Instance.Player) infos.Add(snitch);
        if (p == Singleton<Jackal>.Instance.Player) infos.Add(jackal);
        if (p == Singleton<Sidekick>.Instance.Player) infos.Add(sidekick);
        if (p == Singleton<Spy>.Instance.Player) infos.Add(spy);
        if (p == Singleton<SecurityGuard>.Instance.Player) infos.Add(securityGuard);
        if (p == Singleton<Arsonist>.Instance.Player) infos.Add(arsonist);
        if (p == Singleton<NiceGuesser>.Instance.Player) infos.Add(goodGuesser);
        if (p == Singleton<EvilGuesser>.Instance.Player) infos.Add(badGuesser);
        if (p == Singleton<BountyHunter>.Instance.Player) infos.Add(bountyHunter);
        if (p == Singleton<Vulture>.Instance.Player) infos.Add(vulture);
        if (p == Singleton<Medium>.Instance.Player) infos.Add(medium);
        if (p == Singleton<Lawyer>.Instance.Player) infos.Add(lawyer);
        if (p == Singleton<Prosecutor>.Instance.Player) infos.Add(prosecutor);
        if (p == Singleton<Trapper>.Instance.Player) infos.Add(trapper);
        if (p == Singleton<Pursuer>.Instance.Player) infos.Add(pursuer);
        if (p == Singleton<Thief>.Instance.Player) infos.Add(thief);

        // Default roles (just impostor, just crewmate, or hunter / hunted for hide n seek
        if (infos.Count == count)
        {
            if (p.Data.Role.IsImpostor)
                infos.Add(TORMapOptions.gameMode == CustomGamemodes.HideNSeek ? RoleInfo.hunter : RoleInfo.impostor);
            else
                infos.Add(TORMapOptions.gameMode == CustomGamemodes.HideNSeek ? RoleInfo.hunted : RoleInfo.crewmate);
        }

        return infos;
    }

    public static String GetRolesString(PlayerControl p, bool useColors, bool showModifier = true,
        bool suppressGhostInfo = false)
    {
        var roleName = string.Join(" ",
            getRoleInfoForPlayer(p, showModifier).Select(x => useColors ? Helpers.cs(x.color, x.name) : x.name)
                .ToArray());
        if (Singleton<Lawyer>.Instance.Target != null && p.PlayerId == Singleton<Lawyer>.Instance.Target.PlayerId &&
            CachedPlayer.LocalPlayer.PlayerControl != Singleton<Lawyer>.Instance.Target)
            roleName += (useColors ? Helpers.cs(Singleton<Pursuer>.Instance.Color, " §") : " §");
        if (HandleGuesser.isGuesserGm && HandleGuesser.isGuesser(p.PlayerId)) roleName += " (Guesser)";
        if (suppressGhostInfo || p == null) return roleName;
        if (p == Shifter.shifter &&
            (CachedPlayer.LocalPlayer.PlayerControl == Shifter.shifter || Helpers.shouldShowGhostInfo()) &&
            Shifter.futureShift != null)
            roleName += Helpers.cs(Color.yellow, " ← " + Shifter.futureShift.Data.PlayerName);
        if (p == Singleton<Vulture>.Instance.Player &&
            (CachedPlayer.LocalPlayer.PlayerControl == Singleton<Vulture>.Instance.Player ||
             Helpers.shouldShowGhostInfo()))
            roleName = roleName + Helpers.cs(Singleton<Vulture>.Instance.Color,
                $" ({Singleton<Vulture>.Instance.EatNumberToWin - Singleton<Vulture>.Instance.EatenBodies} left)");
        if (!Helpers.shouldShowGhostInfo()) return roleName;
        if (Singleton<Eraser>.Instance.FutureErased.Contains(p))
            roleName = Helpers.cs(Color.gray, "(erased) ") + roleName;
        if (Singleton<Vampire>.Instance.Player != null && !Singleton<Vampire>.Instance.Player.Data.IsDead &&
            Singleton<Vampire>.Instance.BittenTarget == p && !p.Data.IsDead)
            roleName = Helpers.cs(Singleton<Vampire>.Instance.Color,
                $"(bitten {(int)HudManagerStartPatch.vampireKillButton.Timer + 1}) ") + roleName;
        if (Singleton<Deputy>.Instance.HandcuffedPlayers.Contains(p))
            roleName = Helpers.cs(Color.gray, "(cuffed) ") + roleName;
        if (p == Singleton<Warlock>.Instance.CurseVictim)
            roleName = Helpers.cs(Singleton<Warlock>.Instance.Color, "(cursed) ") + roleName;
        if (p == Singleton<Ninja>.Instance.MarkedTarget)
            roleName = Helpers.cs(Singleton<Ninja>.Instance.Color, "(marked) ") + roleName;
        if (Singleton<Pursuer>.Instance.BlankedPlayers.Contains(p) && !p.Data.IsDead)
            roleName = Helpers.cs(Singleton<Pursuer>.Instance.Color, "(blanked) ") + roleName;
        if (Singleton<Witch>.Instance.FutureSpelled.Contains(p) &&
            !MeetingHud.Instance) // This is already displayed in meetings!
            roleName = Helpers.cs(Singleton<Witch>.Instance.Color, "☆ ") + roleName;
        if (Singleton<BountyHunter>.Instance.Bounty == p)
            roleName = Helpers.cs(Singleton<BountyHunter>.Instance.Color, "(bounty) ") + roleName;
        if (Singleton<Arsonist>.Instance.DousedPlayers.Contains(p))
            roleName = Helpers.cs(Singleton<Arsonist>.Instance.Color, "♨ ") + roleName;
        if (p == Singleton<Arsonist>.Instance.Player)
            roleName += Helpers.cs(Singleton<Arsonist>.Instance.Color,
                $" ({CachedPlayer.AllPlayers.Count(x => { return x.PlayerControl != Singleton<Arsonist>.Instance.Player && x.Data is { IsDead: false, Disconnected: false } && Singleton<Arsonist>.Instance.DousedPlayers.All(y => y.PlayerId != x.PlayerId); })} left)");
        return roleName;
    }
}