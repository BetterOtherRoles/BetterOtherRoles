using System;
using BetterOtherRoles.EnoFw.Kernel;
using BetterOtherRoles.Players;
using BetterOtherRoles.Utilities;
using Reactor.Networking.Attributes;
using UnityEngine;
using Option = BetterOtherRoles.EnoFw.Kernel.CustomOption;

namespace BetterOtherRoles.EnoFw.Roles.Crewmate;

public class TimeMaster : AbstractRole
{
    public static readonly TimeMaster Instance = new();
    
    // Fields
    public bool ShieldActive;
    public bool IsRewinding;
    
    // Options
    public readonly CustomOption TimeShieldCooldown;
    public readonly CustomOption ShieldDuration;
    public readonly CustomOption RewindTime;

    public static Sprite TimeShieldButtonSprite => GetSprite("BetterOtherRoles.Resources.TimeShieldButton.png", 115f);

    private TimeMaster() : base(nameof(TimeMaster), "Time master")
    {
        Team = Teams.Crewmate;
        Color = new Color32(112, 142, 239, byte.MaxValue);
        
        SpawnRate = GetDefaultSpawnRateOption();
        
        TimeShieldCooldown = Tab.CreateFloatList(
            $"{Key}{nameof(TimeShieldCooldown)}",
            Cs("Time shield cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        ShieldDuration = Tab.CreateFloatList(
            $"{Key}{nameof(ShieldDuration)}",
            Cs("Time shield duration"),
            1f,
            20f,
            3f,
            1f,
            SpawnRate,
            string.Empty,
            "s");
        RewindTime = Tab.CreateFloatList(
            $"{Key}{nameof(RewindTime)}",
            Cs("Rewind time"),
            1f,
            10f,
            3f,
            1f,
            SpawnRate,
            string.Empty,
            "s");
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        IsRewinding = false;
        ShieldActive = false;
    }

    public static void TimeMasterRewindTime()
    {
        Rpc_TimeMasterRewindTime(PlayerControl.LocalPlayer);
    }

    [MethodRpc((uint)Rpc.Role.TimeMasterRewindTime)]
    private static void Rpc_TimeMasterRewindTime(PlayerControl sender)
    {
        Instance.ShieldActive = true;
        SoundEffectsManager.stop("timemasterShield");
        if (Instance.IsLocalPlayer)
        {
            HudManagerStartPatch.resetTimeMasterButton();
        }
        FastDestroyableSingleton<HudManager>.Instance.FullScreen.color = new Color(0f, 0.5f, 0.8f, 0.3f);
        FastDestroyableSingleton<HudManager>.Instance.FullScreen.enabled = true;
        FastDestroyableSingleton<HudManager>.Instance.FullScreen.gameObject.SetActive(true);
        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp((float)Instance.RewindTime / 2, new Action<float>((p) => {
            if (p == 1f) FastDestroyableSingleton<HudManager>.Instance.FullScreen.enabled = false;
        })));
        if (!Instance.IsLocalPlayer) return;
        Instance.IsRewinding = true;
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
        Instance.ShieldActive = true;
        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Instance.ShieldDuration, new Action<float>(p => {
            if (p == 1f) Instance.ShieldActive = false;
        })));
    }
}