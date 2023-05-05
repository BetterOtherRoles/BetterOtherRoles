using System;
using System.Linq;
using Reactor.Networking.Attributes;
using TheOtherRoles.Objects;
using TheOtherRoles.Players;
using UnityEngine;

namespace TheOtherRoles.EnoFw.Roles.Impostor;

public static class Vampire
{
    public static PlayerControl vampire;
    public static Color color = Palette.ImpostorRed;

    public static float delay = 10f;
    public static float cooldown = 30f;
    public static bool canKillNearGarlics = true;
    public static bool localPlacedGarlic = false;
    public static bool garlicsActive = true;

    public static PlayerControl currentTarget;
    public static PlayerControl bitten;
    public static bool targetNearGarlic = false;

    private static Sprite buttonSprite;

    public static Sprite getButtonSprite()
    {
        if (buttonSprite) return buttonSprite;
        buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.VampireButton.png", 115f);
        return buttonSprite;
    }

    private static Sprite garlicButtonSprite;

    public static Sprite getGarlicButtonSprite()
    {
        if (garlicButtonSprite) return garlicButtonSprite;
        garlicButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.GarlicButton.png", 115f);
        return garlicButtonSprite;
    }

    public static void clearAndReload()
    {
        vampire = null;
        bitten = null;
        targetNearGarlic = false;
        localPlacedGarlic = false;
        currentTarget = null;
        garlicsActive = CustomOptionHolder.vampireSpawnRate.getSelection() > 0;
        delay = CustomOptionHolder.vampireKillDelay.getFloat();
        cooldown = CustomOptionHolder.vampireCooldown.getFloat();
        canKillNearGarlics = CustomOptionHolder.vampireCanKillNearGarlics.getBool();
    }

    public static void VampireSetBitten(byte targetId, bool performReset)
    {
        var data = new Tuple<byte, bool>(targetId, performReset);
        Rpc_VampireSetBitten(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.VampireSetBitten)]
    private static void Rpc_VampireSetBitten(PlayerControl sender, string rawData)
    {
        var (targetId, performReset) = Rpc.Deserialize<Tuple<byte, bool>>(rawData);
        if (performReset)
        {
            bitten = null;
            return;
        }
        if (vampire == null) return;
        var player = Helpers.playerById(targetId);
        if (player.Data.IsDead) return;
        bitten = player;
    }

    public static void PlaceGarlic(float x, float y)
    {
        var data = new Tuple<float, float>(x, y);
        Rpc_PlaceGarlic(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.PlaceGarlic)]
    private static void Rpc_PlaceGarlic(PlayerControl sender, string rawData)
    {
        var (x, y) = Rpc.Deserialize<Tuple<float, float>>(rawData);
        var position = Vector3.zero;
        position.x = x;
        position.y = y;
        var _ = new Garlic(position);
    }
}