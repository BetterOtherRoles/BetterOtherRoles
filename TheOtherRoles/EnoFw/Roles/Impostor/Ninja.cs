using System;
using AmongUs.Data;
using Reactor.Networking.Attributes;
using TheOtherRoles.Objects;
using TheOtherRoles.Players;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TheOtherRoles.EnoFw.Roles.Impostor;

public static class Ninja
{
    public static PlayerControl ninja;
    public static Color color = Palette.ImpostorRed;

    public static PlayerControl ninjaMarked;
    public static PlayerControl currentTarget;
    public static float cooldown = 30f;
    public static float traceTime = 1f;
    public static bool knowsTargetLocation;
    public static float invisibleDuration = 5f;

    public static float invisibleTimer;
    public static bool isInvisble;
    private static Sprite markButtonSprite;
    private static Sprite killButtonSprite;
    public static Arrow arrow = new Arrow(Color.black);

    public static Sprite getMarkButtonSprite()
    {
        if (markButtonSprite) return markButtonSprite;
        markButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.NinjaMarkButton.png", 115f);
        return markButtonSprite;
    }

    public static Sprite getKillButtonSprite()
    {
        if (killButtonSprite) return killButtonSprite;
        killButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.NinjaAssassinateButton.png", 115f);
        return killButtonSprite;
    }

    public static void clearAndReload()
    {
        ninja = null;
        currentTarget = ninjaMarked = null;
        cooldown = CustomOptionHolder.ninjaCooldown.getFloat();
        knowsTargetLocation = CustomOptionHolder.ninjaKnowsTargetLocation.getBool();
        traceTime = CustomOptionHolder.ninjaTraceTime.getFloat();
        invisibleDuration = CustomOptionHolder.ninjaInvisibleDuration.getFloat();
        invisibleTimer = 0f;
        isInvisble = false;
        if (arrow?.arrow != null) Object.Destroy(arrow.arrow);
        arrow = new Arrow(Color.black);
        if (arrow.arrow != null) arrow.arrow.SetActive(false);
    }

    public static void SetInvisible(byte playerId, bool visible)
    {
        var data = new Tuple<byte, bool>(playerId, visible);
        Rpc_SetInvisible(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.SetInvisible)]
    private static void Rpc_SetInvisible(PlayerControl sender, string rawData)
    {
        var (playerId, visible) = Rpc.Deserialize<Tuple<byte, bool>>(rawData);
        
        var target = Helpers.playerById(playerId);
        if (target == null) return;
        if (visible)
        {
            target.cosmetics.currentBodySprite.BodySprite.color = Color.white;
            target.cosmetics.colorBlindText.gameObject.SetActive(DataManager.Settings.Accessibility.ColorBlindMode);
            target.cosmetics.colorBlindText.color = target.cosmetics.colorBlindText.color.SetAlpha(1f);
            if (Camouflager.camouflageTimer <= 0) target.setDefaultLook();
            isInvisble = false;
            return;
        }

        target.setLook("", 6, "", "", "", "");
        var invisibleColor = Color.clear;           
        if (CachedPlayer.LocalPlayer.Data.Role.IsImpostor || CachedPlayer.LocalPlayer.Data.IsDead) color.a = 0.1f;
        target.cosmetics.currentBodySprite.BodySprite.color = invisibleColor;
        target.cosmetics.colorBlindText.color = target.cosmetics.colorBlindText.color.SetAlpha(color.a);
        target.cosmetics.colorBlindText.gameObject.SetActive(false);
        invisibleTimer = invisibleDuration;
        isInvisble = true;
    }

    public static void PlaceNinjaTrace(float x, float y)
    {
        var data = new Tuple<float, float>(x, y);
        Rpc_PlaceNinjaTrace(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.PlaceNinjaTrace)]
    private static void Rpc_PlaceNinjaTrace(PlayerControl sender, string rawData)
    {
        var (x, y) = Rpc.Deserialize<Tuple<float, float>>(rawData);
        var position = Vector3.zero;
        position.x = x;
        position.y = y;
        var _ = new NinjaTrace(position, traceTime);
        if (CachedPlayer.LocalPlayer.PlayerControl == ninja) return;
        ninjaMarked = null;
    }
}