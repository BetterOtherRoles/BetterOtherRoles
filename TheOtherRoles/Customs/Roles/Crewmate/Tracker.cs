using System.Collections.Generic;
using System.Linq;
using Reactor.Networking.Attributes;
using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.EnoFramework.Utils;
using TheOtherRoles.Objects;
using TheOtherRoles.Players;
using UnityEngine;
using Resources = TheOtherRoles.EnoFramework.Utils.Resources;

namespace TheOtherRoles.Customs.Roles.Crewmate;

[EnoSingleton]
public class Tracker : CustomRole
{
    private Sprite? _trackSprite;
    private Sprite? _trackCorpseSprite;

    private CustomButton? _trackButton;
    private CustomButton? _trackCorpseButton;

    public readonly EnoFramework.CustomOption UpdateArrowInterval;
    public readonly EnoFramework.CustomOption ResetTargetAfterMeeting;
    public readonly EnoFramework.CustomOption CanTrackCorpses;
    public readonly EnoFramework.CustomOption CorpsesTrackingCooldown;
    public readonly EnoFramework.CustomOption CorpsesTrackingDuration;

    public readonly List<Arrow> Arrows = new();
    public readonly List<Vector3> DeadBodyPositions = new();
    public PlayerControl? TrackedTarget;
    public float TimeUntilUpdate;
    public float CorpseTrackingTimer;
    public Arrow Arrow = new Arrow(Color.blue);

    public Tracker() : base(nameof(Tracker))
    {
        Team = Teams.Crewmate;
        Color = new Color32(100, 58, 220, byte.MaxValue);
        CanTarget = true;

        UpdateArrowInterval = OptionsTab.CreateFloatList(
            $"{Name}{nameof(UpdateArrowInterval)}",
            Colors.Cs(Color, "Update interval"),
            1f,
            30f,
            5f,
            1f,
            SpawnRate,
            string.Empty,
            "s");
        ResetTargetAfterMeeting = OptionsTab.CreateBool(
            $"{Name}{nameof(ResetTargetAfterMeeting)}",
            Colors.Cs(Color, "Reset target after meeting"),
            true,
            SpawnRate);
        CanTrackCorpses = OptionsTab.CreateBool(
            $"{Name}{nameof(CanTrackCorpses)}",
            Colors.Cs(Color, "Can track dead bodies"),
            false,
            SpawnRate);
        CorpsesTrackingCooldown = OptionsTab.CreateFloatList(
            $"{Name}{nameof(CorpsesTrackingCooldown)}",
            Colors.Cs(Color, "Corpses tracking cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            CanTrackCorpses,
            string.Empty,
            "s");
        CorpsesTrackingDuration = OptionsTab.CreateFloatList(
            $"{Name}{nameof(CorpsesTrackingDuration)}",
            Colors.Cs(Color, "Corpses tracking duration"),
            2.5f,
            30f,
            5f,
            2.5f,
            CanTrackCorpses,
            string.Empty,
            "s");
    }

    public Sprite GetTrackSprite()
    {
        if (_trackSprite == null)
        {
            _trackSprite = Resources.LoadSpriteFromResources("TheOtherRoles.Resources.PathfindButton.png", 115f);
        }

        return _trackSprite;
    }

    public Sprite GetTrackCorpsesSprite()
    {
        if (_trackCorpseSprite == null)
        {
            _trackCorpseSprite = Resources.LoadSpriteFromResources("TheOtherRoles.Resources.TrackerButton.png", 115f);
        }

        return _trackCorpseSprite;
    }

    public override void CreateCustomButtons(HudManager hudManager)
    {
        base.CreateCustomButtons(hudManager);
        _trackButton = new CustomButton(
            OnTrackButtonClick,
            IsLocalPlayerAndAlive,
            CouldUseTrackButton,
            ResetTrackButton,
            GetTrackSprite(),
            CustomButton.ButtonPositions.lowerRowRight,
            hudManager,
            "ActionQuaternary"
        );
        _trackCorpseButton = new CustomButton(
            OnTrackCorpseButtonClick,
            HasTrackCorpseButton,
            CanUseTrackCorpseButton,
            ResetTrackCorpseButton,
            GetTrackCorpsesSprite(),
            CustomButton.ButtonPositions.lowerRowCenter,
            hudManager,
            "ActionSecondary",
            true,
            CorpsesTrackingDuration,
            OnTrackCorpseEffectEnd
        );
    }

    private void OnTrackCorpseEffectEnd()
    {
        if (_trackCorpseButton == null) return;
        _trackCorpseButton.Timer = _trackCorpseButton.MaxTimer;
    }

    private void ResetTrackCorpseButton()
    {
        if (_trackCorpseButton == null) return;
        _trackCorpseButton.Timer = _trackCorpseButton.MaxTimer;
        _trackCorpseButton.isEffectActive = false;
        _trackCorpseButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
    }

    private bool CanUseTrackCorpseButton()
    {
        return CachedPlayer.LocalPlayer.PlayerControl.CanMove;
    }

    private bool HasTrackCorpseButton()
    {
        return IsLocalPlayerAndAlive() && CanTrackCorpses;
    }

    private void OnTrackCorpseButtonClick()
    {
        CorpseTrackingTimer = CorpsesTrackingDuration;
        SoundEffectsManager.play("trackerTrackCorpses");
    }

    private void ResetTrackButton()
    {
        if (ResetTargetAfterMeeting)
        {
            ResetTracked();
        }
    }

    private bool CouldUseTrackButton()
    {
        return CachedPlayer.LocalPlayer.PlayerControl.CanMove && CurrentTarget != null && TrackedTarget == null;
    }

    private void OnTrackButtonClick()
    {
        if (CurrentTarget == null) return;
        TrackPlayer(CachedPlayer.LocalPlayer, $"{CurrentTarget.PlayerId}");
        SoundEffectsManager.play("trackerTrackPlayer");
    }

    [MethodRpc((uint)Rpc.Id.TrackerTrackPlayer)]
    private static void TrackPlayer(PlayerControl sender, string rawData)
    {
        var targetId = byte.Parse(rawData);
        var target = Helpers.playerById(targetId);
        if (target == null) return;
        Singleton<Tracker>.Instance.TrackedTarget = target;
    }

    public void ResetTracked()
    {
        CurrentTarget = TrackedTarget = null;
        if (Arrow.arrow != null) UnityEngine.Object.Destroy(Arrow.arrow);
        Arrow = new Arrow(Color.blue);
        if (Arrow.arrow != null) Arrow.arrow.SetActive(true);
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        ResetTracked();
        TimeUntilUpdate = 0f;
        foreach (var arrow in Arrows.Where(arrow => arrow.arrow != null))
        {
            UnityEngine.Object.Destroy(arrow.arrow);
        }

        Arrows.Clear();
        DeadBodyPositions.Clear();
        CorpseTrackingTimer = 0f;
    }
}