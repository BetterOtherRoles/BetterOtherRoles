using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using HarmonyLib;
using MonoMod.Utils;
using BetterOtherRoles.EnoFw.Libs.Reactor.Networking.Attributes;
using BetterOtherRoles.EnoFw;
using BetterOtherRoles.EnoFw.Modules;
using BetterOtherRoles.EnoFw.Utils;
using BetterOtherRoles.Patches;

namespace BetterOtherRoles.Modules;

public static class CustomGuid
{
    public static Guid Guid => BetterOtherRolesPlugin.DevGuid.Value != ""
        ? Guid.Parse(BetterOtherRolesPlugin.DevGuid.Value)
        : CurrentGuid;

    public static Guid CurrentGuid => Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId;
    
    [HarmonyPatch(typeof(Minigame), nameof(Minigame.Begin))]
    public static class MinigameBeginPatch
    {
        public static void Prefix(Minigame __instance, [HarmonyArgument(0)] PlayerTask task)
        {
            if (task == null) return;
            BetterOtherRolesPlugin.Logger.LogDebug($"Opened task name : {task.name}");
        }
    }
    /*
    
    [HarmonyPatch(typeof(FriendsListManager), nameof(FriendsListManager.CheckFriendCodeOnLogin))]
    public static class CheckFriendCodeOnLogin_Patch
    {
        public static void Postfix()
        {
            DestroyableSingleton<EOSManager>.Instance.editAccountUsername.gameObject.SetActive(true);
        }
    }

    [HarmonyPatch(typeof(EditAccountUsername), nameof(EditAccountUsername.RandomizeName))]
    public static class RandomizeName_Patch
    {
        public static void Postfix(EditAccountUsername __instance)
        {
            __instance.UsernameText.SetText("Eno", true);
        }
    }
    */
}