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

        TimeShieldCooldown = OptionsTab.CreateFloatList(
            $"{Name}{nameof(TimeShieldCooldown)}",
            Colors.Cs(Color, "Time shield cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        ShieldDuration = OptionsTab.CreateFloatList(
            $"{Name}{nameof(ShieldDuration)}",
            Colors.Cs(Color, "Time shield duration"),
            1f,
            20f,
            3f,
            1f,
            SpawnRate,
            string.Empty,
            "s");
        RewindTime = OptionsTab.CreateFloatList(
            $"{Name}{nameof(RewindTime)}",
            Colors.Cs(Color, "Rewind time"),
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

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        ShieldActive = false;
        IsRewinding = false;
    }
}