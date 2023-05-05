using System;
using System.Collections.Generic;
using Reactor.Networking.Attributes;
using UnityEngine;

namespace TheOtherRoles.EnoFw.Roles.Impostor;

public static class Eraser
{
    public static PlayerControl eraser;
    public static Color color = Palette.ImpostorRed;

    public static List<byte> alreadyErased = new List<byte>();

    public static List<PlayerControl> futureErased = new();
    public static PlayerControl currentTarget;
    public static float cooldown = 30f;
    public static bool canEraseAnyone = false;

    private static Sprite buttonSprite;

    public static Sprite getButtonSprite()
    {
        if (buttonSprite) return buttonSprite;
        buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.EraserButton.png", 115f);
        return buttonSprite;
    }

    public static void clearAndReload()
    {
        eraser = null;
        futureErased.Clear();
        currentTarget = null;
        cooldown = CustomOptionHolder.eraserCooldown.getFloat();
        canEraseAnyone = CustomOptionHolder.eraserCanEraseAnyone.getBool();
        alreadyErased = new List<byte>();
    }

    public static void SetFutureErased(byte playerId)
    {
        var data = new Tuple<byte>(playerId);
        Rpc_SetFutureErased(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.SetFutureErased)]
    private static void Rpc_SetFutureErased(PlayerControl sender, string rawData)
    {
        var playerId = Rpc.Deserialize<Tuple<byte>>(rawData).Item1;
        var player = Helpers.playerById(playerId);
        if (player == null) return;
        futureErased.Add(player);
    }
}