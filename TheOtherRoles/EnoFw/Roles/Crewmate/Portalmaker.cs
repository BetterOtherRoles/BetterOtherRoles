using System;
using Reactor.Networking.Attributes;
using TheOtherRoles.Objects;
using TheOtherRoles.Utilities;
using UnityEngine;

namespace TheOtherRoles.EnoFw.Roles.Crewmate;

public static class Portalmaker
{
    public static PlayerControl portalmaker;
    public static Color color = new Color32(69, 69, 169, byte.MaxValue);

    public static float cooldown;
    public static float usePortalCooldown;
    public static bool logOnlyHasColors;
    public static bool logShowsTime;
    public static bool canPortalFromAnywhere;

    private static Sprite placePortalButtonSprite;
    private static Sprite usePortalButtonSprite;
    private static Sprite usePortalSpecialButtonSprite1;
    private static Sprite usePortalSpecialButtonSprite2;
    private static Sprite logSprite;

    public static Sprite getPlacePortalButtonSprite()
    {
        if (placePortalButtonSprite) return placePortalButtonSprite;
        placePortalButtonSprite =
            Helpers.loadSpriteFromResources("TheOtherRoles.Resources.PlacePortalButton.png", 115f);
        return placePortalButtonSprite;
    }

    public static Sprite getUsePortalButtonSprite()
    {
        if (usePortalButtonSprite) return usePortalButtonSprite;
        usePortalButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.UsePortalButton.png", 115f);
        return usePortalButtonSprite;
    }

    public static Sprite getUsePortalSpecialButtonSprite(bool first)
    {
        if (first)
        {
            if (usePortalSpecialButtonSprite1) return usePortalSpecialButtonSprite1;
            usePortalSpecialButtonSprite1 =
                Helpers.loadSpriteFromResources("TheOtherRoles.Resources.UsePortalSpecialButton1.png", 115f);
            return usePortalSpecialButtonSprite1;
        }
        else
        {
            if (usePortalSpecialButtonSprite2) return usePortalSpecialButtonSprite2;
            usePortalSpecialButtonSprite2 =
                Helpers.loadSpriteFromResources("TheOtherRoles.Resources.UsePortalSpecialButton2.png", 115f);
            return usePortalSpecialButtonSprite2;
        }
    }

    public static Sprite getLogSprite()
    {
        if (logSprite) return logSprite;
        logSprite = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.DoorLogsButton]
            .Image;
        return logSprite;
    }

    public static void clearAndReload()
    {
        portalmaker = null;
        cooldown = CustomOptionHolder.portalmakerCooldown.getFloat();
        usePortalCooldown = CustomOptionHolder.portalmakerUsePortalCooldown.getFloat();
        logOnlyHasColors = CustomOptionHolder.portalmakerLogOnlyColorType.getBool();
        logShowsTime = CustomOptionHolder.portalmakerLogHasTime.getBool();
        canPortalFromAnywhere = CustomOptionHolder.portalmakerCanPortalFromAnywhere.getBool();
    }

    public static void UsePortal(byte playerId, byte exit)
    {
        var data = new Tuple<byte, byte>(playerId, exit);
        Rpc_UsePortal(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.UsePortal)]
    private static void Rpc_UsePortal(PlayerControl sender, string rawData)
    {
        var (playerId, exit) = Rpc.Deserialize<Tuple<byte, byte>>(rawData);
        Local_UsePortal(playerId, exit);
    }
    
    public static void Local_UsePortal(byte playerId, byte exit)
    {
        Portal.startTeleport(playerId, exit);
    }

    public static void PlacePortal(float x, float y)
    {
        var data = new Tuple<float, float>(x, y);
        Rpc_PlacePortal(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.PlacePortal)]
    private static void Rpc_PlacePortal(PlayerControl sender, string rawData)
    {
        var (x, y) = Rpc.Deserialize<Tuple<float, float>>(rawData);
        var position = Vector3.zero;
        position.x = x;
        position.y = y;
        var _ = new Portal(position);
    }
}