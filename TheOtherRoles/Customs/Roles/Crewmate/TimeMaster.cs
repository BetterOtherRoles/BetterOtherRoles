using System;
using Reactor.Networking.Attributes;
using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.EnoFramework.Utils;
using TheOtherRoles.Objects;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;
using UnityEngine;
using Resources = TheOtherRoles.EnoFramework.Utils.Resources;

namespace TheOtherRoles.Customs.Roles.Crewmate;

[EnoSingleton]
public class TimeMaster : CustomRole
{
    private Sprite? _timeShieldSprite;

    private CustomButton? _timeShieldButton;

    public readonly EnoFramework.CustomOption TimeShieldCooldown;
    public readonly EnoFramework.CustomOption ShieldDuration;
    public readonly EnoFramework.CustomOption RewindTime;

    public bool ShieldActive;
    public bool IsRewinding;

    public TimeMaster() : base(nameof(TimeMaster))
    {
        Team = Teams.Crewmate;
        Color = new Color32(112, 142, 239, byte.MaxValue);

        IntroDescription = "Save yourself with your time shield";
        ShortDescription = "Use your time shield";

        TimeShieldCooldown = OptionsTab.CreateFloatList(
            $"{Name}{nameof(TimeShieldCooldown)}",
            Cs("Time shield cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        ShieldDuration = OptionsTab.CreateFloatList(
            $"{Name}{nameof(ShieldDuration)}",
            Cs("Time shield duration"),
            1f,
            20f,
            3f,
            1f,
            SpawnRate,
            string.Empty,
            "s");
        RewindTime = OptionsTab.CreateFloatList(
            $"{Name}{nameof(RewindTime)}",
            Cs("Rewind time"),
            1f,
            10f,
            3f,
            1f,
            SpawnRate,
            string.Empty,
            "s");
    }

    public Sprite GetTimeShieldSprite()
    {
        if (_timeShieldSprite == null)
        {
            _timeShieldSprite = Resources.LoadSpriteFromResources("TheOtherRoles.Resources.TimeShieldButton.png", 115f);
        }

        return _timeShieldSprite;
    }

    public override void CreateCustomButtons(HudManager hudManager)
    {
        base.CreateCustomButtons(hudManager);
        _timeShieldButton = new CustomButton(
            OnTimeShieldButtonClick,
            HasTimeShieldButton,
            CouldUseTimeShieldButton,
            ResetTimeShieldButton,
            GetTimeShieldSprite(),
            CustomButton.ButtonPositions.lowerRowRight,
            hudManager,
            "ActionQuaternary",
            true,
            ShieldDuration,
            OnTimeShieldButtonEffectEnd
        );
    }

    private void OnTimeShieldButtonEffectEnd()
    {
        if (_timeShieldButton == null) return;
        _timeShieldButton.Timer = _timeShieldButton.MaxTimer;
        SoundEffectsManager.stop("timemasterShield");
    }

    private void ResetTimeShieldButton()
    {
        if (_timeShieldButton == null) return;
        _timeShieldButton.Timer = _timeShieldButton.MaxTimer;
        _timeShieldButton.isEffectActive = false;
        _timeShieldButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
    }

    private bool CouldUseTimeShieldButton()
    {
        return CachedPlayer.LocalPlayer.PlayerControl.CanMove;
    }

    private bool HasTimeShieldButton()
    {
        return Player != null && Is(CachedPlayer.LocalPlayer) && !CachedPlayer.LocalPlayer.Data.IsDead;
    }

    private void OnTimeShieldButtonClick()
    {
        Shield(CachedPlayer.LocalPlayer);
        SoundEffectsManager.play("timemasterShield");
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        ShieldActive = false;
        IsRewinding = false;
    }
    
    [MethodRpc((uint)Rpc.Id.TimeMasterShield)]
    private static void Shield(PlayerControl sender)
    {
        Singleton<TimeMaster>.Instance.ShieldActive = true;
        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(
            Singleton<TimeMaster>.Instance.ShieldDuration, new Action<float>(p =>
            {
                if (p == 1f) Singleton<TimeMaster>.Instance.ShieldActive = false;
            })));
    }

    [MethodRpc((uint)Rpc.Id.TimeMasterRewindTime)]
    public static void StartRewindTime(PlayerControl sender)
    {
        Singleton<TimeMaster>.Instance.ShieldActive = false;
        SoundEffectsManager.stop("timemasterShield");
        if (Singleton<TimeMaster>.Instance.IsLocalPlayer())
        {
            Singleton<TimeMaster>.Instance.ResetTimeMasterButton();
        }
        FastDestroyableSingleton<HudManager>.Instance.FullScreen.color = new Color(0f, 0.5f, 0.8f, 0.3f);
        FastDestroyableSingleton<HudManager>.Instance.FullScreen.enabled = true;
        FastDestroyableSingleton<HudManager>.Instance.FullScreen.gameObject.SetActive(true);
        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Singleton<TimeMaster>.Instance.RewindTime / 2f,
            new Action<float>((p) =>
            {
                if (p == 1f) FastDestroyableSingleton<HudManager>.Instance.FullScreen.enabled = false;
            })));

        if (Singleton<TimeMaster>.Instance.IsLocalPlayer()) return; // Time Master himself does not rewind

        Singleton<TimeMaster>.Instance.IsRewinding = true;

        if (MapBehaviour.Instance)
            MapBehaviour.Instance.Close();
        if (Minigame.Instance)
            Minigame.Instance.ForceClose();
        CachedPlayer.LocalPlayer.PlayerControl.moveable = false;
    }

    private void ResetTimeMasterButton()
    {
        if (_timeShieldButton == null) return;
        _timeShieldButton.Timer = _timeShieldButton.MaxTimer;
        _timeShieldButton.isEffectActive = false;
        _timeShieldButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
        SoundEffectsManager.stop("timemasterShield");
    }
}