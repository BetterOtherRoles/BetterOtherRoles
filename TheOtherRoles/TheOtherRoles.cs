using HarmonyLib;
using System;
using TheOtherRoles.Utilities;
using TheOtherRoles.CustomGameModes;
using TheOtherRoles.EnoFw.Roles.Crewmate;
using TheOtherRoles.EnoFw.Roles.Impostor;
using TheOtherRoles.EnoFw.Roles.Modifiers;
using TheOtherRoles.EnoFw.Roles.Neutral;

namespace TheOtherRoles
{
    [HarmonyPatch]
    public static class TheOtherRoles
    {
        public static Random rnd = new((int)DateTime.Now.Ticks);

        public static void clearAndReloadRoles() {
            Jester.clearAndReload();
            Mayor.clearAndReload();
            Portalmaker.clearAndReload();
            Engineer.clearAndReload();
            Sheriff.clearAndReload();
            Deputy.ClearAndReload();
            Lighter.clearAndReload();
            Godfather.clearAndReload();
            Mafioso.clearAndReload();
            Janitor.clearAndReload();
            Detective.clearAndReload();
            TimeMaster.clearAndReload();
            Medic.clearAndReload();
            Shifter.clearAndReload();
            Swapper.clearAndReload();
            Lovers.clearAndReload();
            Seer.clearAndReload();
            Morphling.clearAndReload();
            Camouflager.clearAndReload();
            Hacker.clearAndReload();
            Tracker.clearAndReload();
            Vampire.clearAndReload();
            Snitch.clearAndReload();
            Jackal.clearAndReload();
            Sidekick.clearAndReload();
            Eraser.clearAndReload();
            Spy.clearAndReload();
            Trickster.clearAndReload();
            Cleaner.clearAndReload();
            Warlock.clearAndReload();
            SecurityGuard.clearAndReload();
            Arsonist.clearAndReload();
            BountyHunter.clearAndReload();
            Vulture.clearAndReload();
            Medium.clearAndReload();
            Lawyer.clearAndReload();
            Pursuer.clearAndReload();
            Witch.clearAndReload();
            Ninja.clearAndReload();
            Thief.clearAndReload();
            Fallen.clearAndReload();
            Trapper.clearAndReload();
            Bomber.clearAndReload();
            Whisperer.clearAndReload();
            Undertaker.clearAndReload();

            // Modifier
            Bait.clearAndReload();
            Bloody.clearAndReload();
            AntiTeleport.clearAndReload();
            Tiebreaker.clearAndReload();
            Sunglasses.clearAndReload();
            Mini.clearAndReload();
            Vip.clearAndReload();
            Invert.clearAndReload();
            Chameleon.clearAndReload();

            // Gamemodes
            HandleGuesser.clearAndReload();
            HideNSeek.clearAndReload();
        }
    }
    // Modifier
}
