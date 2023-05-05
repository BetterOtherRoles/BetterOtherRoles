using System.Linq;
using Reactor.Networking.Attributes;
using TheOtherRoles.Players;
using UnityEngine;

namespace TheOtherRoles.EnoFw.Roles.Impostor;

public static class Camouflager
{
    public static PlayerControl camouflager;
    public static Color color = Palette.ImpostorRed;

    public static float cooldown = 30f;
    public static float duration = 10f;
    public static float camouflageTimer = 0f;

    private static Sprite buttonSprite;

    public static Sprite getButtonSprite()
    {
        if (buttonSprite) return buttonSprite;
        buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.CamoButton.png", 115f);
        return buttonSprite;
    }

    public static void resetCamouflage()
    {
        camouflageTimer = 0f;
        foreach (PlayerControl p in CachedPlayer.AllPlayers)
        {
            if (p == Ninja.ninja && Ninja.isInvisble)
                continue;
            p.setDefaultLook();
        }
    }

    public static void clearAndReload()
    {
        resetCamouflage();
        camouflager = null;
        camouflageTimer = 0f;
        cooldown = CustomOptionHolder.camouflagerCooldown.getFloat();
        duration = CustomOptionHolder.camouflagerDuration.getFloat();
    }

    public static void CamouflagerCamouflage()
    {
        Rpc_CamouflagerCamouflage(PlayerControl.LocalPlayer);
    }

    [MethodRpc((uint)Rpc.Role.CamouflagerCamouflage)]
    private static void Rpc_CamouflagerCamouflage(PlayerControl sender)
    {
        if (camouflager == null) return;
        camouflageTimer = duration;
        foreach (var player in CachedPlayer.AllPlayers.Select(p => p.PlayerControl))
        {
            player.setLook("", 6, "", "", "", "");
        }
    }
}