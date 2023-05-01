using System.Collections.Generic;
using Reactor.Networking.Attributes;
using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.EnoFramework.Utils;
using TheOtherRoles.Objects;
using TheOtherRoles.Players;
using UnityEngine;
using Resources = TheOtherRoles.EnoFramework.Utils.Resources;

namespace TheOtherRoles.Customs.Roles.Crewmate;

[EnoSingleton]
public class Medic : CustomRole
{
    private Sprite? _shieldSprite;

    private CustomButton? _shieldButton;

    public readonly EnoFramework.CustomOption ShowMurderAttemptToMedic;
    public readonly EnoFramework.CustomOption ShowMurderAttemptToShielded;
    public readonly EnoFramework.CustomOption WhenSetShield;
    public readonly EnoFramework.CustomOption WhenShowShield;
    public readonly EnoFramework.CustomOption ShowShield;
    public readonly Color ShieldColor;

    public PlayerControl? ShieldedPlayer;
    public PlayerControl? FutureShieldedPlayer;
    public bool UsedShield;
    public bool VisibleShield;
    public bool MeetingAfterShielding;

    public bool IsShielded(PlayerControl player)
    {
        if (Player == null || Player.Data.IsDead) return false;
        return ShieldedPlayer == player;
    }

    public Medic() : base(nameof(Medic))
    {
        Team = Teams.Crewmate;
        Color = new Color32(126, 251, 194, byte.MaxValue);
        CanTarget = true;
        ShieldColor = new Color32(0, 221, 255, byte.MaxValue);

        IntroDescription = "Protect someone with your shield";
        ShortDescription = "Protect other players";

        ShowMurderAttemptToMedic = OptionsTab.CreateBool(
            $"{Name}{nameof(ShowMurderAttemptToMedic)}",
            Cs("Show murder attempt to medic"),
            false,
            SpawnRate);
        ShowMurderAttemptToShielded = OptionsTab.CreateBool(
            $"{Name}{nameof(ShowMurderAttemptToShielded)}",
            Cs("Show murder attempt to shielded"),
            false,
            SpawnRate);
        WhenSetShield = OptionsTab.CreateStringList(
            $"{Name}{nameof(WhenSetShield)}",
            Cs("Shield will be activated"),
            new List<string> { "instantly", "after meeting" },
            "instantly",
            SpawnRate);
        WhenShowShield = OptionsTab.CreateStringList(
            $"{Name}{nameof(WhenShowShield)}",
            Cs("Shield will be shown"),
            new List<string> { "never", "instantly", "after meeting" },
            "instantly",
            SpawnRate);
        ShowShield = OptionsTab.CreateStringList(
            $"{Name}{nameof(ShowShield)}",
            Cs("Show shielded player"),
            new List<string> { "everyone", "shielded + medic", "medic" },
            "shielded + medic",
            WhenShowShield);
    }

    public Sprite GetShieldSprite()
    {
        if (_shieldSprite == null)
        {
            _shieldSprite = Resources.LoadSpriteFromResources("TheOtherRoles.Resources.ShieldButton.png", 115f);
        }

        return _shieldSprite;
    }

    public override void CreateCustomButtons(HudManager hudManager)
    {
        base.CreateCustomButtons(hudManager);
        _shieldButton = new CustomButton(
            OnShieldButtonClick,
            HasShieldButton,
            CouldUseShieldButton,
            () => { },
            GetShieldSprite(),
            CustomButton.ButtonPositions.lowerRowRight,
            hudManager,
            "ActionQuaternary"
        );
    }

    private bool CouldUseShieldButton()
    {
        return !UsedShield && CurrentTarget != null && CachedPlayer.LocalPlayer.PlayerControl.CanMove;
    }

    private bool HasShieldButton()
    {
        return Player != null && Is(CachedPlayer.LocalPlayer) && !CachedPlayer.LocalPlayer.Data.IsDead;
    }

    private void OnShieldButtonClick()
    {
        if (_shieldButton == null || Player == null || CurrentTarget == null) return;
        _shieldButton.Timer = 0f;
        ShieldPlayer(CachedPlayer.LocalPlayer, $"{CurrentTarget.PlayerId}");
        SoundEffectsManager.play("medicShield");
    }

    public override void OnMeetingVotingComplete(MeetingHud meetingHud, byte[] states, GameData.PlayerInfo exiled,
        bool tie)
    {
        base.OnMeetingVotingComplete(meetingHud, states, exiled, tie);
        if (FutureShieldedPlayer == null) return;
        ShieldedPlayer = FutureShieldedPlayer;
        FutureShieldedPlayer = null;
        if (WhenShowShield == "after meeting")
        {
            VisibleShield = true;
        }
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        ShieldedPlayer = null;
        FutureShieldedPlayer = null;
        UsedShield = false;
        MeetingAfterShielding = false;
    }

    [MethodRpc((uint)Rpc.Id.MedicShieldPlayer)]
    private static void ShieldPlayer(PlayerControl sender, string rawData)
    {
        if (Singleton<Medic>.Instance.UsedShield) return;
        var targetId = byte.Parse(rawData);
        var target = Helpers.playerById(targetId);
        if (target == null) return;
        Singleton<Medic>.Instance.UsedShield = true;
        if (Singleton<Medic>.Instance.WhenSetShield == "instantly")
        {
            Singleton<Medic>.Instance.ShieldedPlayer = target;
            if (Singleton<Medic>.Instance.WhenShowShield == "instantly")
            {
                Singleton<Medic>.Instance.VisibleShield = true;
            }
        }
        else
        {
            Singleton<Medic>.Instance.FutureShieldedPlayer = target;
        }
    }
}