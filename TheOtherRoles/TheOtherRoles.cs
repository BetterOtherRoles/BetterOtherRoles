using HarmonyLib;
using System;
using TheOtherRoles.Utilities;
using TheOtherRoles.CustomGameModes;
using TheOtherRoles.EnoFw.Kernel;
using TheOtherRoles.EnoFw.Roles.Modifiers;
using TheOtherRoles.EnoFw.Roles.Neutral;

namespace TheOtherRoles;

[HarmonyPatch]
public static class TheOtherRoles
{
    public static readonly Random Rnd = new((int)DateTime.Now.Ticks);

    public static void clearAndReloadRoles() {
        foreach (var role in AbstractRole.AllRoles)
        {
            role.Value.ClearAndReload();
        }

        foreach (var modifier in AbstractModifier.AllModifiers)
        {
            modifier.Value.ClearAndReload();
        }

        // Modifier

        // Gamemodes
        HandleGuesser.clearAndReload();
        HideNSeek.clearAndReload();
    }
}
// Modifier