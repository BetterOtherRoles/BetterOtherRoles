using System;
using Reactor.Networking.Attributes;
using TheOtherRoles.Objects;
using TheOtherRoles.Players;
using UnityEngine;

namespace TheOtherRoles.EnoFw.Roles.Impostor;

public static class Trickster
{
    public static PlayerControl trickster;
    public static Color color = Palette.ImpostorRed;
    public static float placeBoxCooldown = 30f;
    public static float lightsOutCooldown = 30f;
    public static float lightsOutDuration = 10f;
    public static float lightsOutTimer = 0f;

    private static Sprite placeBoxButtonSprite;
    private static Sprite lightOutButtonSprite;
    private static Sprite tricksterVentButtonSprite;

    public static Sprite getPlaceBoxButtonSprite()
    {
        if (placeBoxButtonSprite) return placeBoxButtonSprite;
        placeBoxButtonSprite =
            Helpers.loadSpriteFromResources("TheOtherRoles.Resources.PlaceJackInTheBoxButton.png", 115f);
        return placeBoxButtonSprite;
    }

    public static Sprite getLightsOutButtonSprite()
    {
        if (lightOutButtonSprite) return lightOutButtonSprite;
        lightOutButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.LightsOutButton.png", 115f);
        return lightOutButtonSprite;
    }

    public static Sprite getTricksterVentButtonSprite()
    {
        if (tricksterVentButtonSprite) return tricksterVentButtonSprite;
        tricksterVentButtonSprite =
            Helpers.loadSpriteFromResources("TheOtherRoles.Resources.TricksterVentButton.png", 115f);
        return tricksterVentButtonSprite;
    }

    public static void clearAndReload()
    {
        trickster = null;
        lightsOutTimer = 0f;
        placeBoxCooldown = CustomOptionHolder.tricksterPlaceBoxCooldown.getFloat();
        lightsOutCooldown = CustomOptionHolder.tricksterLightsOutCooldown.getFloat();
        lightsOutDuration = CustomOptionHolder.tricksterLightsOutDuration.getFloat();
        JackInTheBox.UpdateStates(); // if the role is erased, we might have to update the state of the created objects
    }

    public static void LightsOut()
    {
        Rpc_LightsOut(PlayerControl.LocalPlayer);
    }

    [MethodRpc((uint)Rpc.Role.LightsOut)]
    private static void Rpc_LightsOut(PlayerControl sender)
    {
        lightsOutTimer = lightsOutDuration;
        // If the local player is impostor indicate lights out
        if (!Helpers.hasImpVision(GameData.Instance.GetPlayerById(CachedPlayer.LocalPlayer.PlayerId))) return;
        var _ = new CustomMessage("Lights are out", lightsOutDuration);
    }

    public static void PlaceJackInTheBox(float x, float y)
    {
        var data = new Tuple<float, float>(x, y);
        Rpc_PlaceJackInTheBox(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.PlaceJackInTheBox)]
    private static void Rpc_PlaceJackInTheBox(PlayerControl sender, string rawData)
    {
        var (x, y) = Rpc.Deserialize<Tuple<float, float>>(rawData);
        var position = Vector3.zero;
        position.x = x;
        position.y = y;
        var _ = new JackInTheBox(position);
    }
}