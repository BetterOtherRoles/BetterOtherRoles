using HarmonyLib;
using System;
using BetterOtherRoles.EnoFw.Roles.Modifiers;

namespace BetterOtherRoles.Patches {
    [HarmonyPatch]
    public static class AirShipSetAntiTpPosition {

        // Save the position of the player prior to starting the climb / gap platform
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.ClimbLadder))]
        public static void prefix() {
            AntiTeleport.Instance.Position = Players.CachedPlayer.LocalPlayer.transform.position;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MovingPlatformBehaviour), nameof(MovingPlatformBehaviour.UsePlatform))]
        public static void prefix2() {
            AntiTeleport.Instance.Position = Players.CachedPlayer.LocalPlayer.transform.position;
        }
    }
}
