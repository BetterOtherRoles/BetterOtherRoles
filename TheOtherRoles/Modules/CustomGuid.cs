using System;
using System.Collections.Generic;
using System.Reflection;
using Reactor.Networking.Attributes;
using TheOtherRoles.Players;
using UnityEngine;

namespace TheOtherRoles.Modules;

public static class CustomGuid
{
    public const bool IsDevMode = false;
    
    public static Dictionary<byte, string> FriendCodes = new();

    public static Dictionary<string, Color> Admins = new()
    {
        {
            "seriesgone#6069", new Color32(66, 135, 245, byte.MaxValue)
        },
        {
            "divinegoal#1460", Color.white
        }
    };

    public static bool IsAdmin(PlayerControl player)
    {
        return FriendCodes.ContainsKey(player.PlayerId) && Admins.ContainsKey(FriendCodes[player.PlayerId]);
    }

    public static Color GetAdminColor(PlayerControl player)
    {
        var friendCode = FriendCodes.TryGetValue(player.PlayerId, out var code) ? code : null;
        if (friendCode == null) return Color.white;
        return Admins.TryGetValue(friendCode, out var color) ? color : Color.white;
    }
    
    public static Guid Guid => TheOtherRolesPlugin.DevGuid.Value != ""
        ? Guid.Parse(TheOtherRolesPlugin.DevGuid.Value)
        : CurrentGuid;

    public static Guid CurrentGuid => Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId;

    public static void ShareFriendCode()
    {
        var client = AmongUsClient.Instance.GetClient(CachedPlayer.LocalPlayer.PlayerControl.OwnerId);
        RpcShareFriendCode(CachedPlayer.LocalPlayer, client.FriendCode);
    }

    [MethodRpc((uint)CustomRpc.ShareFriendCode)]
    private static void RpcShareFriendCode(PlayerControl sender, string rawData)
    {
        FriendCodes[sender.PlayerId] = rawData;
    }
}