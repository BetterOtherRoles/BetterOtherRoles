using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using HarmonyLib;
using MonoMod.Utils;
using Reactor.Networking.Attributes;
using TheOtherRoles.EnoFw;
using TheOtherRoles.EnoFw.Modules;
using TheOtherRoles.EnoFw.Utils;
using TheOtherRoles.Patches;

namespace TheOtherRoles.Modules;

public static class CustomGuid
{
#if DEBUG
    public const bool IsDevMode = true;
    public const bool NoEndGame = false;
    public const bool ShowRoleDesc = false;
#endif
#if RELEASE
    public const bool IsDevMode = false;
    public const bool NoEndGame = false;
    public const bool ShowRoleDesc = false;
#endif

    private static readonly Dictionary<string, CustomAdmin> Admins = new();

    public static bool IsAdmin(PlayerControl player)
    {
        if (!GameStartManagerPatch.PlayerVersions.TryGetValue(player.OwnerId, out var version)) return false;
        return Admins.ContainsKey(version.FriendCode) && Admins[version.FriendCode].ModAdmin;
    }

    public static void UpdateAdminsColor(PlayerControl player)
    {
        if (player.cosmetics == null || player.cosmetics.currentBodySprite == null) return;
        if (!GameStartManagerPatch.PlayerVersions.TryGetValue(player.OwnerId, out var version)) return;
        if (!Admins.TryGetValue(version.FriendCode, out var data)) return;
        if (data.NameColor != "") player.cosmetics.nameText.color = Colors.FromHex(data.NameColor);
        if (data.OutlineColor != "")
        {
            player.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", 1f);
            player.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", Colors.FromHex(data.OutlineColor));
        }
    }

    public static Guid Guid => TheOtherRolesPlugin.DevGuid.Value != ""
        ? Guid.Parse(TheOtherRolesPlugin.DevGuid.Value)
        : CurrentGuid;

    public static Guid CurrentGuid => Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId;
    
    [HarmonyPatch(typeof(Minigame), nameof(Minigame.Begin))]
    public static class MinigameBeginPatch
    {
        public static void Prefix(Minigame __instance, [HarmonyArgument(0)] PlayerTask task)
        {
            if (task == null) return;
            TheOtherRolesPlugin.Logger.LogDebug($"Opened task name : {task.name}");
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

    [MethodRpc((uint)Rpc.Kernel.ShareAdminColors)]
    private static void RpcShareAdminColors(PlayerControl sender, string rawData)
    {
        if (sender.AmOwner) return;
        Admins.Clear();
        Admins.AddRange(Rpc.Deserialize<Dictionary<string, CustomAdmin>>(rawData));
    }

    public static async void FetchAdmins(bool rpcShare = false)
    {
        var items = await ExternalResources.Get<List<CustomAdmin>>("CustomAdmins.json");
        Admins.Clear();
        foreach (var item in items) Admins[item.FriendCode] = item;
        if (rpcShare) RpcShareAdminColors(PlayerControl.LocalPlayer, Rpc.Serialize(Admins));
    }
    
    public class CustomAdmin
    {
        public string FriendCode { get; set; }
        public string NameColor { get; set; }
        public string OutlineColor { get; set; }

        public bool ModAdmin { get; set; } = false;
    }
}