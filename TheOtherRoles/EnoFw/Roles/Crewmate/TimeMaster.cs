using System;
using Reactor.Networking.Attributes;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;
using UnityEngine;

namespace TheOtherRoles.EnoFw.Roles.Crewmate;

public static class TimeMaster
{
    public static PlayerControl timeMaster;
    public static Color color = new Color32(112, 142, 239, byte.MaxValue);

    public static bool reviveDuringRewind = false;
    public static float rewindTime = 3f;
    public static float shieldDuration = 3f;
    public static float cooldown = 30f;

    public static bool shieldActive = false;
    public static bool isRewinding = false;

    private static Sprite buttonSprite;

    public static Sprite getButtonSprite()
    {
        if (buttonSprite) return buttonSprite;
        buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.TimeShieldButton.png", 115f);
        return buttonSprite;
    }

    public static void clearAndReload()
    {
        timeMaster = null;
        isRewinding = false;
        shieldActive = false;
        rewindTime = CustomOptionHolder.timeMasterRewindTime.getFloat();
        shieldDuration = CustomOptionHolder.timeMasterShieldDuration.getFloat();
        cooldown = CustomOptionHolder.timeMasterCooldown.getFloat();
    }

    public static void TimeMasterRewindTime()
    {
        Rpc_TimeMasterRewindTime(PlayerControl.LocalPlayer);
    }

    [MethodRpc((uint)Rpc.Role.TimeMasterRewindTime)]
    private static void Rpc_TimeMasterRewindTime(PlayerControl sender)
    {
        shieldActive = true;
        SoundEffectsManager.stop("timemasterShield");
        if (timeMaster != null && timeMaster == CachedPlayer.LocalPlayer.PlayerControl)
        {
            HudManagerStartPatch.resetTimeMasterButton();
        }
        FastDestroyableSingleton<HudManager>.Instance.FullScreen.color = new Color(0f, 0.5f, 0.8f, 0.3f);
        FastDestroyableSingleton<HudManager>.Instance.FullScreen.enabled = true;
        FastDestroyableSingleton<HudManager>.Instance.FullScreen.gameObject.SetActive(true);
        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(TimeMaster.rewindTime / 2, new Action<float>((p) => {
            if (p == 1f) FastDestroyableSingleton<HudManager>.Instance.FullScreen.enabled = false;
        })));
        if (timeMaster == null || CachedPlayer.LocalPlayer.PlayerControl == timeMaster) return;
        isRewinding = true;
        if (MapBehaviour.Instance)
            MapBehaviour.Instance.Close();
        if (Minigame.Instance)
            Minigame.Instance.ForceClose();
        CachedPlayer.LocalPlayer.PlayerControl.moveable = false;
    }

    public static void TimeMasterShield()
    {
        Rpc_TimeMasterShield(PlayerControl.LocalPlayer);
    }

    [MethodRpc((uint)Rpc.Role.TimeMasterShield)]
    private static void Rpc_TimeMasterShield(PlayerControl sender)
    {
        shieldActive = true;
        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(shieldDuration, new Action<float>(p => {
            if (p == 1f) shieldActive = false;
        })));
    }
}