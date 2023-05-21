using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using BetterOtherRoles.EnoFw.Kernel;
using BetterOtherRoles.EnoFw.Roles.Crewmate;
using BetterOtherRoles.EnoFw.Utils;
using UnityEngine;

namespace BetterOtherRoles.EnoFw.Modules;
/*
public static class RoleAssignment
{
    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    internal class RoleManagerSelectRolesPatch
    {
        private static void Postfix()
        {
            KernelRpc.ResetVariables();
            // Don't assign Roles in Hide N Seek
            if (TORMapOptions.gameMode == CustomGamemodes.HideNSeek ||
                GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return;
            if (CustomOptions.EnableRoles)
            {
                AssignRoles();
            }
        }
    }

    private static void AssignRoles()
    {
        var data = new AssignmentData();
        var pools = new AssignmentPools(data);
    }

    private class AssignmentPools
    {
        private readonly AssignmentData _data;

        private readonly List<AbstractRole> _crewmateRoles;
        private readonly List<AbstractRole> _neutralRoles;
        private readonly List<AbstractRole> _impostorRoles;

        public readonly Dictionary<byte, string> RolesToAssign = new();

        private readonly List<string> _assignedDependantRoles = new();

        public AssignmentPools(AssignmentData data)
        {
            _data = data;
            _crewmateRoles = AbstractRole.AllRoles
                .Select(r => r.Value)
                .Where(r => r.Team == AbstractRole.Teams.Crewmate && r.IsAssignable && r.SpawnRate > 0)
                .ToList();
            _neutralRoles = AbstractRole.AllRoles
                .Select(r => r.Value)
                .Where(r => r.Team == AbstractRole.Teams.Neutral && r.IsAssignable && r.SpawnRate > 0)
                .ToList();
            _impostorRoles = AbstractRole.AllRoles
                .Select(r => r.Value)
                .Where(r => r.Team == AbstractRole.Teams.Impostor && r.IsAssignable && r.SpawnRate > 0)
                .ToList();

            while (_data.CrewmateRolesCount > 0 && _crewmateRoles.Count > 0 && _data.Crewmates.Count > 0)
            {
                var role = _crewmateRoles.PickOneRandom();
                if (_data.CrewmateRolesCount >= _data.Crewmates.Count || BetterOtherRoles.Rnd.Next(0, 100) <= role.SpawnRate)
                {
                    var player = _data.Crewmates.PickOneRandom();
                    RolesToAssign.Add(player.PlayerId, role.Name);
                    _data.CrewmateRolesCount--;
                }
            }

            while (_data.NeutralRolesCount > 0 && _neutralRoles.Count > 0 && _data.Neutrals.Count > 0)
            {
                var role = _neutralRoles.PickOneRandom();
                if (_data.NeutralRolesCount >= _data.Neutrals.Count || BetterOtherRoles.Rnd.Next(0, 100) <= role.SpawnRate)
                {
                    var player = _data.Neutrals.PickOneRandom();
                    RolesToAssign.Add(player.PlayerId, role.Name);
                    _data.NeutralRolesCount--;
                }
            }
            
            while (_data.ImpostorRolesCount > 0 && _impostorRoles.Count > 0 && _data.Impostors.Count > 0)
            {
                var role = _impostorRoles.PickOneRandom();
                if (_data.ImpostorRolesCount >= _data.Impostors.Count || BetterOtherRoles.Rnd.Next(0, 100) <= role.SpawnRate)
                {
                    var player = _data.Impostors.PickOneRandom();
                    RolesToAssign.Add(player.PlayerId, role.Name);
                    _data.ImpostorRolesCount--;
                }
            }
            
            CheckDependantRoles();
        }

        private void CheckDependantRoles()
        {
            if (RolesToAssign.ContainsValue(Deputy.Instance.Name) &&
                !RolesToAssign.ContainsValue(Sheriff.Instance.Name))
            {
                var key = RolesToAssign.FirstOrDefault(i => i.Value == Deputy.Instance.Name).Key;
                RolesToAssign[key] = Sheriff.Instance.Name;
            }
        }
    }

    private class AssignmentData
    {
        public readonly List<PlayerControl> Crewmates;
        public readonly List<PlayerControl> Neutrals;
        public readonly List<PlayerControl> Impostors;

        public int CrewmateRolesCount;
        public int NeutralRolesCount;
        public int ImpostorRolesCount;

        public AssignmentData()
        {
            var crewmates = PlayerControl.AllPlayerControls
                .ToArray()
                .Where(p => !p.Data.Role.IsImpostor)
                .OrderBy(_ => BetterOtherRoles.Rnd.Next())
                .ToList();
            var impostors = PlayerControl.AllPlayerControls
                .ToArray()
                .Where(p => p.Data.Role.IsImpostor)
                .OrderBy(_ => BetterOtherRoles.Rnd.Next())
                .ToList();

            var minCrewmateRoles = (int)CustomOptions.MinCrewmateRoles;
            var maxCrewmateRoles = (int)CustomOptions.MaxCrewmateRoles;
            var minNeutralRoles = (int)CustomOptions.MinNeutralRoles;
            var maxNeutralRoles = (int)CustomOptions.MaxNeutralRoles;
            var minImpostorRoles = (int)CustomOptions.MinImpostorRoles;
            var maxImpostorRoles = (int)CustomOptions.MaxImpostorRoles;

            if (minCrewmateRoles > maxCrewmateRoles) minCrewmateRoles = maxCrewmateRoles;
            if (minNeutralRoles > maxNeutralRoles) minNeutralRoles = maxNeutralRoles;
            if (minImpostorRoles > maxImpostorRoles) minImpostorRoles = maxImpostorRoles;

            var crewmateRolesCount = BetterOtherRoles.Rnd.Next(minCrewmateRoles, maxCrewmateRoles);
            var neutralRolesCount = BetterOtherRoles.Rnd.Next(minNeutralRoles, maxNeutralRoles);
            var impostorRolesCount = BetterOtherRoles.Rnd.Next(minImpostorRoles, maxImpostorRoles);

            if (impostors.Count < impostorRolesCount) impostorRolesCount = impostors.Count;
            while (crewmates.Count < neutralRolesCount * 2) neutralRolesCount--;

            var neutrals = crewmates.PickRandom(neutralRolesCount);

            if (CustomOptions.FillCrewmateRoles)
            {
                crewmateRolesCount = crewmates.Count;
            }

            Crewmates = crewmates;
            Neutrals = neutrals;
            Impostors = impostors;

            CrewmateRolesCount = crewmateRolesCount;
            NeutralRolesCount = neutralRolesCount;
            ImpostorRolesCount = impostorRolesCount;
        }
    }

    [HarmonyPatch(typeof(RoleOptionsCollectionV07), nameof(RoleOptionsCollectionV07.GetNumPerGame))]
    internal class RoleOptionsDataGetNumPerGamePatch
    {
        private static void Postfix(ref int __result)
        {
            if (CustomOptions.EnableRoles &&
                GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.Normal)
                __result = 0; // Deactivate Vanilla Roles if the mod roles are active
        }
    }

    [HarmonyPatch(typeof(IGameOptionsExtensions), nameof(IGameOptionsExtensions.GetAdjustedNumImpostors))]
    internal class GameOptionsDataGetAdjustedNumImpostorsPatch
    {
        private static void Postfix(ref int __result)
        {
            if (TORMapOptions.gameMode == CustomGamemodes.HideNSeek)
            {
                int impCount = Mathf.RoundToInt(CustomOptions.HideNSeekHunterCount);
                // Set Imp Num
                __result = impCount;
            }
            else if (GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.Normal)
            {
                // Ignore Vanilla impostor limits in TOR Games.
                __result = Mathf.Clamp(GameOptionsManager.Instance.CurrentGameOptions.NumImpostors, 1, 3);
            }
        }
    }

    [HarmonyPatch(typeof(GameOptionsData), nameof(GameOptionsData.Validate))]
    internal class GameOptionsDataValidatePatch
    {
        private static void Postfix(GameOptionsData __instance)
        {
            if (TORMapOptions.gameMode == CustomGamemodes.HideNSeek ||
                GameOptionsManager.Instance.CurrentGameOptions.GameMode != GameModes.Normal) return;
            __instance.NumImpostors = GameOptionsManager.Instance.CurrentGameOptions.NumImpostors;
        }
    }
}
*/