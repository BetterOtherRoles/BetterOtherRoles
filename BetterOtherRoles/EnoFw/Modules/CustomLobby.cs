using BetterOtherRoles.EnoFw.Modules.BorApi;
using BetterOtherRoles.Patches;
using UnityEngine;

namespace BetterOtherRoles.EnoFw.Modules;

public static class CustomLobby
{
    public static bool IsAdmin(PlayerControl player)
    {
        var info = GetPublicAccountInfo(player);
        return info is { IsAdmin: true };
    }

    public static string GetFriendCode(PlayerControl player)
    {
        return !GameStartManagerPatch.PlayerVersions.TryGetValue(player.OwnerId, out var version) ? null : version.FriendCode;
    }

    public static PublicAccountInfo GetPublicAccountInfo(PlayerControl player)
    {
        var friendCode = GetFriendCode(player);
        if (friendCode == null) return null;
        return !BorClient.Instance.PublicAccountInfos.TryGetValue(friendCode, out var accountInfo) ? null : accountInfo;
    }

    public static void OnLobbyPlayerUpdate(PlayerControl player)
    {
        if (player.cosmetics == null || player.cosmetics.currentBodySprite == null) return;
        var accountInfo = GetPublicAccountInfo(player);
        if (accountInfo == null) return;
        if (accountInfo.LobbyNameColor != Color.clear)
        {
            player.cosmetics.nameText.color = accountInfo.LobbyNameColor;
        }

        if (accountInfo.LobbyOutlineColor != Color.clear)
        {
            player.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", 1f);
            player.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", accountInfo.LobbyOutlineColor);
        }
    }
}