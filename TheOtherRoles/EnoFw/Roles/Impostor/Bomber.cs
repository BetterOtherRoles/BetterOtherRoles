using System;
using Reactor.Networking.Attributes;
using TheOtherRoles.Objects;
using UnityEngine;

namespace TheOtherRoles.EnoFw.Roles.Impostor;

public static class Bomber
{
    public static PlayerControl bomber = null;
    public static Color color = Palette.ImpostorRed;

    public static Bomb bomb = null;
    public static bool isPlanted = false;
    public static bool isActive = false;
    public static float destructionTime = 20f;
    public static float destructionRange = 2f;
    public static float hearRange = 30f;
    public static float defuseDuration = 3f;
    public static float bombCooldown = 15f;
    public static float bombActiveAfter = 3f;

    private static Sprite buttonSprite;

    public static Sprite getButtonSprite()
    {
        if (buttonSprite) return buttonSprite;
        buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Bomb_Button_Plant.png", 115f);
        return buttonSprite;
    }

    public static void clearBomb(bool flag = true)
    {
        if (bomb != null)
        {
            UnityEngine.Object.Destroy(bomb.bomb);
            UnityEngine.Object.Destroy(bomb.background);
            bomb = null;
        }

        isPlanted = false;
        isActive = false;
        if (flag) SoundEffectsManager.stop("bombFuseBurning");
    }

    public static void clearAndReload()
    {
        clearBomb(false);
        bomber = null;
        bomb = null;
        isPlanted = false;
        isActive = false;
        destructionTime = CustomOptionHolder.bomberBombDestructionTime.getFloat();
        destructionRange = CustomOptionHolder.bomberBombDestructionRange.getFloat() / 10;
        hearRange = CustomOptionHolder.bomberBombHearRange.getFloat() / 10;
        defuseDuration = CustomOptionHolder.bomberDefuseDuration.getFloat();
        bombCooldown = CustomOptionHolder.bomberBombCooldown.getFloat();
        bombActiveAfter = CustomOptionHolder.bomberBombActiveAfter.getFloat();
        Bomb.clearBackgroundSprite();
    }

    public static void DefuseBomb()
    {
        Rpc_DefuseBomb(PlayerControl.LocalPlayer);
    }

    [MethodRpc((uint)Rpc.Role.DefuseBomb)]
    private static void Rpc_DefuseBomb(PlayerControl sender)
    {
        SoundEffectsManager.playAtPosition("bombDefused", Bomber.bomb.bomb.transform.position, range: Bomber.hearRange);
        clearBomb();
        HudManagerStartPatch.bomberButton.Timer = HudManagerStartPatch.bomberButton.MaxTimer;
        HudManagerStartPatch.bomberButton.isEffectActive = false;
        HudManagerStartPatch.bomberButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
    }

    public static void PlaceBomb(float x, float y)
    {
        var data = new Tuple<float, float>(x, y);
        Rpc_PlaceBomb(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.PlaceBomb)]
    private static void Rpc_PlaceBomb(PlayerControl sender, string rawData)
    {
        var (x, y) = Rpc.Deserialize<Tuple<float, float>>(rawData);
        if (bomber == null) return;
        var position = Vector3.zero;
        position.x = x;
        position.y = y;
        var _ = new Bomb(position);
    }
}