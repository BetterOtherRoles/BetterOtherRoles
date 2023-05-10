using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using HarmonyLib;
using MonoMod.Utils;
using Reactor.Networking.Attributes;
using TheOtherRoles.EnoFw;
using TheOtherRoles.EnoFw.Utils;
using TheOtherRoles.Players;
using UnityEngine;

namespace TheOtherRoles.Modules;

public static class CustomGuid
{
#if DEBUG
    public const bool IsDevMode = true;
    public const bool NoEndGame = true;
    public const bool ShowRoleDesc = true;
#endif
#if RELEASE
    public const bool IsDevMode = false;
    public const bool NoEndGame = false;
    public const bool ShowRoleDesc = false;
#endif

    public const string AdminsUrl = "https://eno.re/BetterOtherRoles/api/CustomAdmins.json";

    public static Dictionary<byte, string> FriendCodes = new();

    public static Dictionary<string, CustomAdmin> Admins = new();

    public static bool IsAdmin(PlayerControl player)
    {
        return FriendCodes.ContainsKey(player.PlayerId) && Admins.ContainsKey(FriendCodes[player.PlayerId]);
    }

    public static void UpdateAdminsColor(PlayerControl player)
    {
        if (!FriendCodes.TryGetValue(player.PlayerId, out var friendCode)) return;
        if (!Admins.TryGetValue(friendCode, out var data)) return;
        if (player.cosmetics == null || player.cosmetics.currentBodySprite == null) return;
        player.cosmetics.nameText.color = Colors.FromHex(data.NameColor);
        player.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", 1f);
        player.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", Colors.FromHex(data.OutlineColor));
    }

    public static Guid Guid => TheOtherRolesPlugin.DevGuid.Value != ""
        ? Guid.Parse(TheOtherRolesPlugin.DevGuid.Value)
        : CurrentGuid;

    public static Guid CurrentGuid => Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId;

    public static void ShareFriendCode()
    {
        var client = AmongUsClient.Instance.GetClient(CachedPlayer.LocalPlayer.PlayerControl.OwnerId);
        var data = new Tuple<byte, string>(CachedPlayer.LocalPlayer.PlayerId, client.FriendCode);
        Rpc_ShareFriendCode(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Module.ShareFriendCode)]
    private static void Rpc_ShareFriendCode(PlayerControl sender, string rawData)
    {
        var (playerId, code) = Rpc.Deserialize<Tuple<byte, string>>(rawData);
        FriendCodes[playerId] = code;
    }
    
    [HarmonyPatch(typeof(Minigame), nameof(Minigame.Begin))]
    public static class MinigameBeginPatch
    {
        public static void Prefix(Minigame __instance, [HarmonyArgument(0)] PlayerTask task)
        {
            if (task == null) return;
            System.Console.WriteLine($"Opened task name : {task.name}");
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
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "BetterOtherRoles Client");

        try
        {
            var response = await client.GetAsync(AdminsUrl, HttpCompletionOption.ResponseContentRead);
            if (!response.IsSuccessStatusCode) return;
            var data = await response.Content.ReadAsStringAsync();
            var items = Rpc.Deserialize<List<CustomAdmin>>(data);
            Admins.Clear();
            foreach (var item in items) Admins[item.FriendCode] = item;
            if (rpcShare) RpcShareAdminColors(PlayerControl.LocalPlayer, Rpc.Serialize(Admins));
        }
        catch (HttpRequestException e)
        {
            System.Console.WriteLine(e);
        }
    }
    
    public class CustomAdmin
    {
        public string FriendCode { get; set; }
        public string NameColor { get; set; }
        public string OutlineColor { get; set; }
    }
}