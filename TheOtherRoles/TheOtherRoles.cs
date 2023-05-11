using HarmonyLib;
using System;
using TheOtherRoles.Utilities;
using TheOtherRoles.CustomGameModes;
using TheOtherRoles.EnoFw.Kernel;
using TheOtherRoles.EnoFw.Roles.Crewmate;
using TheOtherRoles.EnoFw.Roles.Impostor;
using TheOtherRoles.EnoFw.Roles.Modifiers;
using TheOtherRoles.EnoFw.Roles.Neutral;

namespace TheOtherRoles;

[HarmonyPatch]
public static class TheOtherRoles
{
    public static readonly Random Rnd = new((int)DateTime.Now.Ticks);

    public static void LoadRoles()
    {
        Deputy.Instance.ClearAndReload();
        Detective.Instance.ClearAndReload();
        Engineer.Instance.ClearAndReload();
        Hacker.Instance.ClearAndReload();
        Lighter.Instance.ClearAndReload();
        Mayor.Instance.ClearAndReload();
        Medic.Instance.ClearAndReload();
        Medium.Instance.ClearAndReload();
        Portalmaker.Instance.ClearAndReload();
        Pursuer.Instance.ClearAndReload();
        SecurityGuard.Instance.ClearAndReload();
        Seer.Instance.ClearAndReload();
        Sheriff.Instance.ClearAndReload();
        Snitch.Instance.ClearAndReload();
        Spy.Instance.ClearAndReload();
        Swapper.Instance.ClearAndReload();
        TimeMaster.Instance.ClearAndReload();
        Tracker.Instance.ClearAndReload();
        Trapper.Instance.ClearAndReload();
        Bomber.Instance.ClearAndReload();
        BountyHunter.Instance.ClearAndReload();
        Camouflager.Instance.ClearAndReload();
        Cleaner.Instance.ClearAndReload();
        Eraser.Instance.ClearAndReload();
        Godfather.Instance.ClearAndReload();
        Janitor.Instance.ClearAndReload();
        Mafioso.Instance.ClearAndReload();
        Morphling.Instance.ClearAndReload();
        Ninja.Instance.ClearAndReload();
        Trickster.Instance.ClearAndReload();
        Undertaker.Instance.ClearAndReload();
        Vampire.Instance.ClearAndReload();
        Warlock.Instance.ClearAndReload();
        Whisperer.Instance.ClearAndReload();
        Witch.Instance.ClearAndReload();
        Arsonist.Instance.ClearAndReload();
        Fallen.Instance.ClearAndReload();
        Guesser.Instance.ClearAndReload();
        Jackal.Instance.ClearAndReload();
        Sidekick.Instance.ClearAndReload();
        Jester.Instance.ClearAndReload();
        Lawyer.Instance.ClearAndReload();
        Thief.Instance.ClearAndReload();
        Vulture.Instance.ClearAndReload();
        AntiTeleport.Instance.ClearAndReload();
        Bait.Instance.ClearAndReload();
        Bloody.Instance.ClearAndReload();
        Chameleon.Instance.ClearAndReload();
        Invert.Instance.ClearAndReload();
        Lovers.Instance.ClearAndReload();
        Mini.Instance.ClearAndReload();
        Shifter.Instance.ClearAndReload();
        Sunglasses.Instance.ClearAndReload();
        Tiebreaker.Instance.ClearAndReload();
        Vip.Instance.ClearAndReload();
    }

    public static void ClearAndReloadRoles() {
        LoadRoles();

        // Modifier

        // Gamemodes
        HandleGuesser.clearAndReload();
        HideNSeek.clearAndReload();
    }
}
// Modifier