using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using AmongUs.GameOptions;
using BetterOtherRoles.CustomGameModes;
using BetterOtherRoles.EnoFw;
using BetterOtherRoles.EnoFw.Kernel;
using BetterOtherRoles.EnoFw.Modules;
using BetterOtherRoles.EnoFw.Roles.Crewmate;
using BetterOtherRoles.EnoFw.Roles.Impostor;
using BetterOtherRoles.EnoFw.Roles.Modifiers;
using BetterOtherRoles.EnoFw.Roles.Neutral;
using BetterOtherRoles.Players;
using static BetterOtherRoles.RolesManager;

namespace BetterOtherRoles.Patches;

[HarmonyPatch(typeof(RoleOptionsCollectionV07), nameof(RoleOptionsCollectionV07.GetNumPerGame))]
class RoleOptionsDataGetNumPerGamePatch
{
    public static void Postfix(ref int __result)
    {
        if (CustomOptions.EnableRoles &&
            GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.Normal)
            __result = 0; // Deactivate Vanilla Roles if the mod roles are active
    }
}

[HarmonyPatch(typeof(IGameOptionsExtensions), nameof(IGameOptionsExtensions.GetAdjustedNumImpostors))]
class GameOptionsDataGetAdjustedNumImpostorsPatch
{
    public static void Postfix(ref int __result)
    {
        if (TORMapOptions.gameMode == CustomGamemodes.HideNSeek)
        {
            int impCount = Mathf.RoundToInt(CustomOptions.HideNSeekHunterCount);
            __result = impCount;
            ; // Set Imp Num
        }
        else if (GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.Normal)
        {
            // Ignore Vanilla impostor limits in TOR Games.
            __result = Mathf.Clamp(GameOptionsManager.Instance.CurrentGameOptions.NumImpostors, 1, 3);
        }
    }
}

[HarmonyPatch(typeof(GameOptionsData), nameof(GameOptionsData.Validate))]
class GameOptionsDataValidatePatch
{
    public static void Postfix(GameOptionsData __instance)
    {
        if (TORMapOptions.gameMode == CustomGamemodes.HideNSeek ||
            GameOptionsManager.Instance.CurrentGameOptions.GameMode != GameModes.Normal) return;
        __instance.NumImpostors = GameOptionsManager.Instance.CurrentGameOptions.NumImpostors;
    }
}

[HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
class RoleManagerSelectRolesPatch
{
    private static int crewValues;
    private static int impValues;
    private static bool isEvilGuesser;
    private static List<Tuple<byte, byte>> playerRoleMap = new();
    internal static Dictionary<byte, byte[]> blockedRolePairings = new ()
    {
        {
            (byte)RoleId.Vampire, new [] {(byte)RoleId.Warlock, (byte)RoleId.Whisperer}
        },
        {
            (byte)RoleId.Warlock, new []{(byte)RoleId.Vampire, (byte)RoleId.Whisperer}
        },
        {
            (byte)RoleId.Whisperer, new []{(byte)RoleId.Vampire, (byte)RoleId.Warlock}
        },
        {
            (byte)RoleId.Spy, new []{(byte)RoleId.Mini}
        },
        {
            (byte)RoleId.Mini, new []{(byte)RoleId.Spy}
        },
        {
            (byte)RoleId.Cleaner, new []{(byte)RoleId.Vulture}
        },
        {
            (byte)RoleId.Vulture, new []{(byte)RoleId.Cleaner}
        }
    };

    public static bool isGuesserGamemode => TORMapOptions.gameMode == CustomGamemodes.Guesser;

    public static void Postfix()
    {
        KernelRpc.ResetVariables();
        if (TORMapOptions.gameMode == CustomGamemodes.HideNSeek ||
            GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek)
            return; // Don't assign Roles in Hide N Seek
        if (CustomOptions.EnableRoles) // Don't assign Roles in Tutorial or if deactivated
            assignRoles();
    }

    private static void assignRoles()
    {
        var data = getRoleAssignmentData();
        assignSpecialRoles(
            data); // Assign special roles like mafia and lovers first as they assign a role to multiple players and the chances are independent of the ticket system
        selectFactionForFactionIndependentRoles(data);
        assignEnsuredRoles(data); // Assign roles that should always be in the game next
        assignDependentRoles(data); // Assign roles that may have a dependent role
        assignChanceRoles(data); // Assign roles that may or may not be in the game last
        assignRoleTargets(data); // Assign targets for Lawyer & Prosecutor
        if (isGuesserGamemode) assignGuesserGamemode();
        assignModifiers(); // Assign modifier
        // setRolesAgain();
    }

    public static RoleAssignmentData getRoleAssignmentData()
    {
        // Get the players that we want to assign the roles to. Crewmate and Neutral roles are assigned to natural crewmates. Impostor roles to impostors.
        List<PlayerControl> crewmates = PlayerControl.AllPlayerControls.ToArray().ToList()
            .OrderBy(x => Guid.NewGuid()).ToList();
        crewmates.RemoveAll(x => x.Data.Role.IsImpostor);
        List<PlayerControl> impostors = PlayerControl.AllPlayerControls.ToArray().ToList()
            .OrderBy(x => Guid.NewGuid()).ToList();
        impostors.RemoveAll(x => !x.Data.Role.IsImpostor);

        var crewmateMin = (int)CustomOptions.MinCrewmateRoles;
        var crewmateMax = (int)CustomOptions.MaxCrewmateRoles;
        var neutralMin = (int)CustomOptions.MinNeutralRoles;
        var neutralMax = (int)CustomOptions.MaxNeutralRoles;
        var impostorMin = (int)CustomOptions.MinImpostorRoles;
        var impostorMax = (int)CustomOptions.MaxImpostorRoles;

        // Automatically force everyone to get a role by setting crew Min / Max according to Neutral Settings
        if (CustomOptions.FillCrewmateRoles)
        {
            crewmateMax = crewmates.Count - neutralMin;
            crewmateMin = crewmates.Count - neutralMax;
        }

        // Make sure min is less or equal to max
        if (crewmateMin > crewmateMax) crewmateMin = crewmateMax;
        if (neutralMin > neutralMax) neutralMin = neutralMax;
        if (impostorMin > impostorMax) impostorMin = impostorMax;

        // Get the maximum allowed count of each role type based on the minimum and maximum option
        int crewCountSettings = Rnd.Next(crewmateMin, crewmateMax + 1);
        int neutralCountSettings = Rnd.Next(neutralMin, neutralMax + 1);
        int impCountSettings = Rnd.Next(impostorMin, impostorMax + 1);

        // Potentially lower the actual maximum to the assignable players
        int maxCrewmateRoles = Mathf.Min(crewmates.Count, crewCountSettings);
        int maxNeutralRoles = Mathf.Min(crewmates.Count, neutralCountSettings);
        int maxImpostorRoles = Mathf.Min(impostors.Count, impCountSettings);

        // Fill in the lists with the roles that should be assigned to players. Note that the special roles (like Mafia or Lovers) are NOT included in these lists
        Dictionary<byte, int> impSettings = new Dictionary<byte, int>();
        Dictionary<byte, int> neutralSettings = new Dictionary<byte, int>();
        Dictionary<byte, int> crewSettings = new Dictionary<byte, int>();

        impSettings.Add((byte)RoleId.Morphling, Morphling.Instance.SpawnRate / 10);
        impSettings.Add((byte)RoleId.Camouflager, Camouflager.Instance.SpawnRate / 10);
        impSettings.Add((byte)RoleId.Vampire, Vampire.Instance.SpawnRate / 10);
        impSettings.Add((byte)RoleId.Whisperer, Whisperer.Instance.SpawnRate / 10);
        impSettings.Add((byte)RoleId.Undertaker, Undertaker.Instance.SpawnRate / 10);
        impSettings.Add((byte)RoleId.Eraser, Eraser.Instance.SpawnRate / 10);
        impSettings.Add((byte)RoleId.Trickster, Trickster.Instance.SpawnRate / 10);
        impSettings.Add((byte)RoleId.Cleaner, Cleaner.Instance.SpawnRate / 10);
        impSettings.Add((byte)RoleId.Warlock, Warlock.Instance.SpawnRate / 10);
        impSettings.Add((byte)RoleId.BountyHunter, BountyHunter.Instance.SpawnRate / 10);
        impSettings.Add((byte)RoleId.Witch, Witch.Instance.SpawnRate / 10);
        impSettings.Add((byte)RoleId.Ninja, Ninja.Instance.SpawnRate / 10);
        impSettings.Add((byte)RoleId.Bomber, Bomber.Instance.SpawnRate / 10);
        if (!isGuesserGamemode)
        {
            impSettings.Add((byte)RoleId.EvilGuesser, EvilGuesser.Instance.SpawnRate / 10);
        }

        neutralSettings.Add((byte)RoleId.Jester, Jester.Instance.SpawnRate / 10);
        neutralSettings.Add((byte)RoleId.Arsonist, Arsonist.Instance.SpawnRate / 10);
        neutralSettings.Add((byte)RoleId.Jackal, Jackal.Instance.SpawnRate / 10);
        neutralSettings.Add((byte)RoleId.Vulture, Vulture.Instance.SpawnRate / 10);
        neutralSettings.Add((byte)RoleId.Thief, Thief.Instance.SpawnRate / 10);

        if (Rnd.Next(1, 101) <=
            Lawyer.Instance.IsProsecutorChance) // Lawyer or Prosecutor
            neutralSettings.Add((byte)RoleId.Prosecutor, Lawyer.Instance.SpawnRate / 10);
        else
            neutralSettings.Add((byte)RoleId.Lawyer, Lawyer.Instance.SpawnRate / 10);

        crewSettings.Add((byte)RoleId.Mayor, Mayor.Instance.SpawnRate / 10);
        crewSettings.Add((byte)RoleId.Portalmaker, Portalmaker.Instance.SpawnRate / 10);
        crewSettings.Add((byte)RoleId.Engineer, Engineer.Instance.SpawnRate / 10);
        crewSettings.Add((byte)RoleId.Lighter, Lighter.Instance.SpawnRate / 10);
        crewSettings.Add((byte)RoleId.Detective, Detective.Instance.SpawnRate / 10);
        crewSettings.Add((byte)RoleId.TimeMaster, TimeMaster.Instance.SpawnRate / 10);
        crewSettings.Add((byte)RoleId.Medic, Medic.Instance.SpawnRate / 10);
        crewSettings.Add((byte)RoleId.Swapper, Swapper.Instance.SpawnRate / 10);
        crewSettings.Add((byte)RoleId.Seer, Seer.Instance.SpawnRate / 10);
        crewSettings.Add((byte)RoleId.Hacker, Hacker.Instance.SpawnRate / 10);
        crewSettings.Add((byte)RoleId.Tracker, Tracker.Instance.SpawnRate / 10);
        crewSettings.Add((byte)RoleId.Snitch, Snitch.Instance.SpawnRate / 10);
        crewSettings.Add((byte)RoleId.Medium, Medium.Instance.SpawnRate / 10);
        crewSettings.Add((byte)RoleId.Trapper, Trapper.Instance.SpawnRate / 10);
        if (!isGuesserGamemode)
        {
            crewSettings.Add((byte)RoleId.NiceGuesser, NiceGuesser.Instance.SpawnRate / 10);
        }
        if (impostors.Count > 1)
        {
            // Only add Spy if more than 1 impostor as the spy role is otherwise useless
            crewSettings.Add((byte)RoleId.Spy, Spy.Instance.SpawnRate / 10);
        }

        crewSettings.Add((byte)RoleId.SecurityGuard, SecurityGuard.Instance.SpawnRate / 10);

        return new RoleAssignmentData
        {
            crewmates = crewmates,
            impostors = impostors,
            crewSettings = crewSettings,
            neutralSettings = neutralSettings,
            impSettings = impSettings,
            maxCrewmateRoles = maxCrewmateRoles,
            maxNeutralRoles = maxNeutralRoles,
            maxImpostorRoles = maxImpostorRoles
        };
    }

    private static void assignSpecialRoles(RoleAssignmentData data)
    {
        // Assign Mafia
        if (data.impostors.Count >= 3 && data.maxImpostorRoles >= 3 &&
            Rnd.Next(1, 101) <= CustomOptions.MafiaSpawnRate * 10)
        {
            setRoleToRandomPlayer((byte)RoleId.Godfather, data.impostors);
            setRoleToRandomPlayer((byte)RoleId.Janitor, data.impostors);
            setRoleToRandomPlayer((byte)RoleId.Mafioso, data.impostors);
            data.maxImpostorRoles -= 3;
        }
    }

    private static void selectFactionForFactionIndependentRoles(RoleAssignmentData data)
    {
        // Assign Sheriff
        if ((Deputy.Instance.SpawnRate > 0 && Sheriff.Instance.SpawnRate == 100) || Deputy.Instance.SpawnRate == 0)
            data.crewSettings.Add((byte)RoleId.Sheriff, Sheriff.Instance.SpawnRate / 10);


        crewValues = data.crewSettings.Values.ToList().Sum();
        impValues = data.impSettings.Values.ToList().Sum();
    }

    private static void assignEnsuredRoles(RoleAssignmentData data)
    {
        // Get all roles where the chance to occur is set to 100%
        List<byte> ensuredCrewmateRoles = data.crewSettings.Where(x => x.Value == 10).Select(x => x.Key).ToList();
        List<byte> ensuredNeutralRoles = data.neutralSettings.Where(x => x.Value == 10).Select(x => x.Key).ToList();
        List<byte> ensuredImpostorRoles = data.impSettings.Where(x => x.Value == 10).Select(x => x.Key).ToList();

        // Assign roles until we run out of either players we can assign roles to or run out of roles we can assign to players
        while (
            (data.impostors.Count > 0 && data.maxImpostorRoles > 0 && ensuredImpostorRoles.Count > 0) ||
            (data.crewmates.Count > 0 && (
                (data.maxCrewmateRoles > 0 && ensuredCrewmateRoles.Count > 0) ||
                (data.maxNeutralRoles > 0 && ensuredNeutralRoles.Count > 0)
            )))
        {
            Dictionary<RoleType, List<byte>> rolesToAssign = new Dictionary<RoleType, List<byte>>();
            if (data.crewmates.Count > 0 && data.maxCrewmateRoles > 0 && ensuredCrewmateRoles.Count > 0)
                rolesToAssign.Add(RoleType.Crewmate, ensuredCrewmateRoles);
            if (data.crewmates.Count > 0 && data.maxNeutralRoles > 0 && ensuredNeutralRoles.Count > 0)
                rolesToAssign.Add(RoleType.Neutral, ensuredNeutralRoles);
            if (data.impostors.Count > 0 && data.maxImpostorRoles > 0 && ensuredImpostorRoles.Count > 0)
                rolesToAssign.Add(RoleType.Impostor, ensuredImpostorRoles);

            // Randomly select a pool of roles to assign a role from next (Crewmate role, Neutral role or Impostor role) 
            // then select one of the roles from the selected pool to a player 
            // and remove the role (and any potentially blocked role pairings) from the pool(s)
            var roleType = rolesToAssign.Keys.ElementAt(Rnd.Next(0, rolesToAssign.Keys.Count()));
            var players = roleType == RoleType.Crewmate || roleType == RoleType.Neutral
                ? data.crewmates
                : data.impostors;
            var index = Rnd.Next(0, rolesToAssign[roleType].Count);
            var roleId = rolesToAssign[roleType][index];
            setRoleToRandomPlayer(rolesToAssign[roleType][index], players);
            rolesToAssign[roleType].RemoveAt(index);

            if (blockedRolePairings.TryGetValue(roleId, out var pairing))
            {
                foreach (var blockedRoleId in pairing)
                {
                    // Set chance for the blocked roles to 0 for chances less than 100%
                    if (data.impSettings.ContainsKey(blockedRoleId)) data.impSettings[blockedRoleId] = 0;
                    if (data.neutralSettings.ContainsKey(blockedRoleId)) data.neutralSettings[blockedRoleId] = 0;
                    if (data.crewSettings.ContainsKey(blockedRoleId)) data.crewSettings[blockedRoleId] = 0;
                    // Remove blocked roles even if the chance was 100%
                    foreach (var ensuredRolesList in rolesToAssign.Values)
                    {
                        ensuredRolesList.RemoveAll(x => x == blockedRoleId);
                    }
                }
            }

            // Adjust the role limit
            switch (roleType)
            {
                case RoleType.Crewmate:
                    data.maxCrewmateRoles--;
                    crewValues -= 10;
                    break;
                case RoleType.Neutral:
                    data.maxNeutralRoles--;
                    break;
                case RoleType.Impostor:
                    data.maxImpostorRoles--;
                    impValues -= 10;
                    break;
            }
        }
    }

    private static void assignDependentRoles(RoleAssignmentData data)
    {
        // Roles that prob have a dependent role
        bool sheriffFlag = Sheriff.Instance.DeputySpawnRate > 0
                           && Sheriff.Instance.SpawnRate > 0;
        
        if (!sheriffFlag) return; // assignDependentRoles is not needed

        int crew = data.crewmates.Count < data.maxCrewmateRoles
            ? data.crewmates.Count
            : data.maxCrewmateRoles; // Max number of crew loops
        int imp = data.impostors.Count < data.maxImpostorRoles
            ? data.impostors.Count
            : data.maxImpostorRoles; // Max number of imp loops
        int crewSteps = crew / data.crewSettings.Keys.Count(); // Avarage crewvalues deducted after each loop 
        int impSteps = imp / data.impSettings.Keys.Count(); // Avarage impvalues deducted after each loop

        // set to false if needed, otherwise we can skip the loop
        bool isSheriff = !sheriffFlag;

        // --- Simulate Crew & Imp ticket system ---
        while (crew > 0 && !isSheriff)
        {
            if (!isSheriff && Rnd.Next(crewValues) < Sheriff.Instance.SpawnRate / 10)
                isSheriff = true;
            crew--;
            crewValues -= crewSteps;
        }

        // --- Assign Main Roles if they won the lottery ---
        if (isSheriff && Sheriff.Instance.Player == null && data.crewmates.Count > 0 && data.maxCrewmateRoles > 0 &&
            sheriffFlag)
        {
            // Set Sheriff cause he won the lottery
            byte sheriff = setRoleToRandomPlayer((byte)RoleId.Sheriff, data.crewmates);
            data.crewmates.ToList().RemoveAll(x => x.PlayerId == sheriff);
            data.maxCrewmateRoles--;
        }

        // --- Assign Dependent Roles if main role exists ---
        if (Sheriff.Instance.Player != null)
        {
            // Deputy
            if (Deputy.Instance.SpawnRate == 100 && data.crewmates.Count > 0 && data.maxCrewmateRoles > 0)
            {
                // Force Deputy
                byte deputy = setRoleToRandomPlayer((byte)RoleId.Deputy, data.crewmates);
                data.crewmates.ToList().RemoveAll(x => x.PlayerId == deputy);
                data.maxCrewmateRoles--;
            }
            else if (Deputy.Instance.SpawnRate < 100) // Dont force, add Deputy to the ticket system
                data.crewSettings.Add((byte)RoleId.Deputy, Deputy.Instance.SpawnRate / 10);
        }

        data.crewSettings.TryAdd((byte)RoleId.Sheriff, 0);
    }

    private static void assignChanceRoles(RoleAssignmentData data)
    {
        // Get all roles where the chance to occur is set grater than 0% but not 100% and build a ticket pool based on their weight
        List<byte> crewmateTickets = data.crewSettings.Where(x => x.Value > 0 && x.Value < 10)
            .Select(x => Enumerable.Repeat(x.Key, x.Value)).SelectMany(x => x).ToList();
        List<byte> neutralTickets = data.neutralSettings.Where(x => x.Value > 0 && x.Value < 10)
            .Select(x => Enumerable.Repeat(x.Key, x.Value)).SelectMany(x => x).ToList();
        List<byte> impostorTickets = data.impSettings.Where(x => x.Value > 0 && x.Value < 10)
            .Select(x => Enumerable.Repeat(x.Key, x.Value)).SelectMany(x => x).ToList();

        // Assign roles until we run out of either players we can assign roles to or run out of roles we can assign to players
        while (
            (data.impostors.Count > 0 && data.maxImpostorRoles > 0 && impostorTickets.Count > 0) ||
            (data.crewmates.Count > 0 && (
                (data.maxCrewmateRoles > 0 && crewmateTickets.Count > 0) ||
                (data.maxNeutralRoles > 0 && neutralTickets.Count > 0)
            )))
        {
            Dictionary<RoleType, List<byte>> rolesToAssign = new Dictionary<RoleType, List<byte>>();
            if (data.crewmates.Count > 0 && data.maxCrewmateRoles > 0 && crewmateTickets.Count > 0)
                rolesToAssign.Add(RoleType.Crewmate, crewmateTickets);
            if (data.crewmates.Count > 0 && data.maxNeutralRoles > 0 && neutralTickets.Count > 0)
                rolesToAssign.Add(RoleType.Neutral, neutralTickets);
            if (data.impostors.Count > 0 && data.maxImpostorRoles > 0 && impostorTickets.Count > 0)
                rolesToAssign.Add(RoleType.Impostor, impostorTickets);

            // Randomly select a pool of role tickets to assign a role from next (Crewmate role, Neutral role or Impostor role) 
            // then select one of the roles from the selected pool to a player 
            // and remove all tickets of this role (and any potentially blocked role pairings) from the pool(s)
            var roleType = rolesToAssign.Keys.ElementAt(Rnd.Next(0, rolesToAssign.Keys.Count()));
            var players = roleType == RoleType.Crewmate || roleType == RoleType.Neutral
                ? data.crewmates
                : data.impostors;
            var index = Rnd.Next(0, rolesToAssign[roleType].Count);
            var roleId = rolesToAssign[roleType][index];
            setRoleToRandomPlayer(roleId, players);
            rolesToAssign[roleType].RemoveAll(x => x == roleId);

            if (blockedRolePairings.TryGetValue(roleId, out var pairing))
            {
                foreach (var blockedRoleId in pairing)
                {
                    // Remove tickets of blocked roles from all pools
                    crewmateTickets.RemoveAll(x => x == blockedRoleId);
                    neutralTickets.RemoveAll(x => x == blockedRoleId);
                    impostorTickets.RemoveAll(x => x == blockedRoleId);
                }
            }

            // Adjust the role limit
            switch (roleType)
            {
                case RoleType.Crewmate:
                    data.maxCrewmateRoles--;
                    break;
                case RoleType.Neutral:
                    data.maxNeutralRoles--;
                    break;
                case RoleType.Impostor:
                    data.maxImpostorRoles--;
                    break;
            }
        }
    }

    private static void assignRoleTargets(RoleAssignmentData data)
    {
        // Set Lawyer or Prosecutor Target
        if (Lawyer.Instance.Player != null)
        {
            var possibleTargets = new List<PlayerControl>();
            if (!Lawyer.Instance.IsProsecutor)
            {
                // Lawyer
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (!p.Data.IsDead && !p.Data.Disconnected && p != Lovers.Instance.Lover1 && p != Lovers.Instance.Lover2 &&
                        (p.Data.Role.IsImpostor || p == Jackal.Instance.Player ||
                         (Lawyer.Instance.TargetCanBeJester && p == Jester.Instance.Player)))
                        possibleTargets.Add(p);
                }
            }
            else
            {
                // Prosecutor
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (!p.Data.IsDead && !p.Data.Disconnected && p != Lovers.Instance.Lover1 && p != Lovers.Instance.Lover2 &&
                        p != Mini.Instance.Player && !p.Data.Role.IsImpostor && !Helpers.isNeutral(p) && p != Swapper.Instance.Player)
                        possibleTargets.Add(p);
                }
            }

            if (possibleTargets.Count == 0)
            {
                Lawyer.LawyerPromotesToPursuer();
            }
            else
            {
                var target = possibleTargets[RolesManager.Rnd.Next(0, possibleTargets.Count)];
                Lawyer.LawyerSetTarget(target.PlayerId);
            }
        }
    }

    private static void assignModifiers()
    {
        var modifierMin = (int)CustomOptions.MinModifiers;
        var modifierMax = (int)CustomOptions.MaxModifiers;
        if (modifierMin > modifierMax) modifierMin = modifierMax;
        int modifierCountSettings = Rnd.Next(modifierMin, modifierMax + 1);
        List<PlayerControl> players = PlayerControl.AllPlayerControls.ToArray().ToList();
        if (isGuesserGamemode && !CustomOptions.GuesserGameModeHaveModifier)
            players.RemoveAll(x => GuesserGM.isGuesser(x.PlayerId));
        int modifierCount = Mathf.Min(players.Count, modifierCountSettings);

        if (modifierCount == 0) return;

        List<RoleId> allModifiers = new List<RoleId>();
        List<RoleId> ensuredModifiers = new List<RoleId>();
        List<RoleId> chanceModifiers = new List<RoleId>();
        allModifiers.AddRange(new List<RoleId>
        {
            RoleId.Tiebreaker,
            RoleId.Mini,
            RoleId.Bait,
            RoleId.Bloody,
            RoleId.AntiTeleport,
            RoleId.Sunglasses,
            RoleId.Vip,
            RoleId.Invert,
            RoleId.Chameleon,
            RoleId.Shifter
        });

        if (Rnd.Next(1, 101) <= Lovers.Instance.SpawnRate)
        {
            // Assign lover
            bool isEvilLover = Rnd.Next(1, 101) <= Lovers.Instance.ImpostorRate;
            byte firstLoverId;
            List<PlayerControl> impPlayer = new List<PlayerControl>(players);
            List<PlayerControl> crewPlayer = new List<PlayerControl>(players);
            impPlayer.RemoveAll(x => !x.Data.Role.IsImpostor);
            crewPlayer.RemoveAll(x => x.Data.Role.IsImpostor || x == Lawyer.Instance.Player);

            if (isEvilLover) firstLoverId = setModifierToRandomPlayer((byte)RoleId.Lover, impPlayer);
            else firstLoverId = setModifierToRandomPlayer((byte)RoleId.Lover, crewPlayer);
            byte secondLoverId = setModifierToRandomPlayer((byte)RoleId.Lover, crewPlayer, 1);

            players.RemoveAll(x => x.PlayerId == firstLoverId || x.PlayerId == secondLoverId);
            modifierCount--;
        }

        foreach (RoleId m in allModifiers)
        {
            if (getSelectionForRoleId(m) == 10)
                ensuredModifiers.AddRange(Enumerable.Repeat(m, getSelectionForRoleId(m, true) / 10));
            else chanceModifiers.AddRange(Enumerable.Repeat(m, getSelectionForRoleId(m, true)));
        }

        assignModifiersToPlayers(ensuredModifiers, players, modifierCount); // Assign ensured modifier

        modifierCount -= ensuredModifiers.Count;
        if (modifierCount <= 0) return;
        int chanceModifierCount = Mathf.Min(modifierCount, chanceModifiers.Count);
        List<RoleId> chanceModifierToAssign = new List<RoleId>();
        while (chanceModifierCount > 0 && chanceModifiers.Count > 0)
        {
            var index = Rnd.Next(0, chanceModifiers.Count);
            RoleId modifierId = chanceModifiers[index];
            chanceModifierToAssign.Add(modifierId);

            int modifierSelection = getSelectionForRoleId(modifierId);
            while (modifierSelection > 0)
            {
                chanceModifiers.Remove(modifierId);
                modifierSelection--;
            }

            chanceModifierCount--;
        }

        assignModifiersToPlayers(chanceModifierToAssign, players, modifierCount); // Assign chance modifier
    }

    private static void assignGuesserGamemode()
    {
        List<PlayerControl> impPlayer = PlayerControl.AllPlayerControls.ToArray().ToList()
            .OrderBy(x => Guid.NewGuid()).ToList();
        List<PlayerControl> neutralPlayer = PlayerControl.AllPlayerControls.ToArray().ToList()
            .OrderBy(x => Guid.NewGuid()).ToList();
        List<PlayerControl> crewPlayer = PlayerControl.AllPlayerControls.ToArray().ToList()
            .OrderBy(x => Guid.NewGuid()).ToList();
        impPlayer.RemoveAll(x => !x.Data.Role.IsImpostor);
        neutralPlayer.RemoveAll(x => !Helpers.isNeutral(x));
        crewPlayer.RemoveAll(x => x.Data.Role.IsImpostor || Helpers.isNeutral(x));
        assignGuesserGamemodeToPlayers(crewPlayer,
            Mathf.RoundToInt(CustomOptions.GuesserGameModeCrewNumber));
        assignGuesserGamemodeToPlayers(neutralPlayer,
            Mathf.RoundToInt(CustomOptions.GuesserGameModeNeutralNumber),
            CustomOptions.GuesserForceJackalGuesser);
        assignGuesserGamemodeToPlayers(impPlayer,
            Mathf.RoundToInt(CustomOptions.GuesserGameModeImpostorNumber));
    }

    private static void assignGuesserGamemodeToPlayers(List<PlayerControl> playerList, int count,
        bool forceJackal = false)
    {
        for (int i = 0; i < count && playerList.Count > 0; i++)
        {
            var index = Rnd.Next(0, playerList.Count);
            if (forceJackal)
            {
                if (Jackal.Instance.Player != null)
                    index = playerList.FindIndex(x => x == Jackal.Instance.Player);
                forceJackal = false;
            }

            byte playerId = playerList[index].PlayerId;
            playerList.RemoveAt(index);
            CommonRpc.SetGuesserGm(playerId);
        }
    }

    private static byte setRoleToRandomPlayer(byte roleId, List<PlayerControl> playerList, bool removePlayer = true)
    {
        var index = Rnd.Next(0, playerList.Count);
        byte playerId = playerList[index].PlayerId;
        if (removePlayer) playerList.RemoveAt(index);

        playerRoleMap.Add(new Tuple<byte, byte>(playerId, roleId));

        KernelRpc.SetRole(roleId, playerId);
        return playerId;
    }

    private static byte setModifierToRandomPlayer(byte modifierId, List<PlayerControl> playerList, byte flag = 0)
    {
        if (playerList.Count == 0) return Byte.MaxValue;
        var index = Rnd.Next(0, playerList.Count);
        byte playerId = playerList[index].PlayerId;
        playerList.RemoveAt(index);
        KernelRpc.SetModifier(modifierId, playerId, flag);
        return playerId;
    }

    private static void assignModifiersToPlayers(List<RoleId> modifiers, List<PlayerControl> playerList,
        int modifierCount)
    {
        modifiers = modifiers.OrderBy(x => Rnd.Next()).ToList(); // randomize list

        while (modifierCount < modifiers.Count)
        {
            var index = Rnd.Next(0, modifiers.Count);
            modifiers.RemoveAt(index);
        }

        byte playerId;

        List<PlayerControl> crewPlayer = new List<PlayerControl>(playerList);
        crewPlayer.RemoveAll(x => x.Data.Role.IsImpostor || RoleInfo.getRoleInfoForPlayer(x).Any(r => r.isNeutral));
        if (modifiers.Contains(RoleId.Shifter))
        {
            var crewPlayerShifter = new List<PlayerControl>(crewPlayer);
            crewPlayerShifter.RemoveAll(x => x == Spy.Instance.Player);
            playerId = setModifierToRandomPlayer((byte)RoleId.Shifter, crewPlayerShifter);
            crewPlayer.RemoveAll(x => x.PlayerId == playerId);
            playerList.RemoveAll(x => x.PlayerId == playerId);
            modifiers.RemoveAll(x => x == RoleId.Shifter);
        }

        if (modifiers.Contains(RoleId.Sunglasses))
        {
            int sunglassesCount = 0;
            while (sunglassesCount < modifiers.FindAll(x => x == RoleId.Sunglasses).Count)
            {
                playerId = setModifierToRandomPlayer((byte)RoleId.Sunglasses, crewPlayer);
                crewPlayer.RemoveAll(x => x.PlayerId == playerId);
                playerList.RemoveAll(x => x.PlayerId == playerId);
                sunglassesCount++;
            }

            modifiers.RemoveAll(x => x == RoleId.Sunglasses);
        }

        foreach (RoleId modifier in modifiers)
        {
            if (playerList.Count == 0) break;
            playerId = setModifierToRandomPlayer((byte)modifier, playerList);
            playerList.RemoveAll(x => x.PlayerId == playerId);
        }
    }

    private static int getSelectionForRoleId(RoleId roleId, bool multiplyQuantity = false)
    {
        int selection = 0;
        switch (roleId)
        {
            case RoleId.Lover:
                selection = Lovers.Instance.SpawnRate;
                break;
            case RoleId.Tiebreaker:
                selection = Tiebreaker.Instance.SpawnRate;
                break;
            case RoleId.Mini:
                selection = Mini.Instance.SpawnRate;
                break;
            case RoleId.Bait:
                selection = Bait.Instance.SpawnRate;
                if (multiplyQuantity) selection *= Bait.Instance.Quantity;
                break;
            case RoleId.Bloody:
                selection = Bloody.Instance.SpawnRate;
                if (multiplyQuantity) selection *= Bloody.Instance.Quantity;
                break;
            case RoleId.AntiTeleport:
                selection = AntiTeleport.Instance.SpawnRate;
                if (multiplyQuantity) selection *= AntiTeleport.Instance.Quantity;
                break;
            case RoleId.Sunglasses:
                selection = Sunglasses.Instance.SpawnRate;
                if (multiplyQuantity) selection *= Sunglasses.Instance.Quantity;
                break;
            case RoleId.Vip:
                selection = Vip.Instance.SpawnRate;
                if (multiplyQuantity) selection *= Vip.Instance.Quantity;
                break;
            case RoleId.Invert:
                selection = Invert.Instance.SpawnRate;
                if (multiplyQuantity) selection *= Invert.Instance.Quantity;
                break;
            case RoleId.Chameleon:
                selection = Chameleon.Instance.SpawnRate;
                if (multiplyQuantity) selection *= Chameleon.Instance.Quantity;
                break;
            case RoleId.Shifter:
                selection = Shifter.Instance.SpawnRate;
                break;
        }

        return selection;
    }

    private static void setRolesAgain()
    {
        while (playerRoleMap.Any())
        {
            var roles = new Dictionary<byte, byte>();
            var amount = (byte)Math.Min(playerRoleMap.Count, 20);
            for (var i = 0; i < amount; i++)
            {
                var option = playerRoleMap[0];
                roles.Add(option.Item1, option.Item2);
                playerRoleMap.RemoveAt(0);
            }

            KernelRpc.WorkaroundSetRoles(roles);
        }
    }

    public class RoleAssignmentData
    {
        public List<PlayerControl> crewmates { get; set; }
        public List<PlayerControl> impostors { get; set; }
        public Dictionary<byte, int> impSettings = new Dictionary<byte, int>();
        public Dictionary<byte, int> neutralSettings = new Dictionary<byte, int>();
        public Dictionary<byte, int> crewSettings = new Dictionary<byte, int>();
        public int maxCrewmateRoles { get; set; }
        public int maxNeutralRoles { get; set; }
        public int maxImpostorRoles { get; set; }
    }

    private enum RoleType
    {
        Crewmate = 0,
        Neutral = 1,
        Impostor = 2
    }
}