using System;
using TheOtherRoles.CustomGameModes;
using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.EnoFramework.Utils;
using TheOtherRoles.Objects;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;
using UnityEngine;
using Resources = TheOtherRoles.EnoFramework.Utils.Resources;

namespace TheOtherRoles.Customs.Roles.Impostor;

[EnoSingleton]
public class Vampire : CustomRole
{
    private Sprite? _biteSprite;
    private Sprite? _garlicSprite;

    private CustomButton? _biteButton;
    private CustomButton? _garlicButton;

    public readonly EnoFramework.CustomOption VampireDelay;
    public readonly EnoFramework.CustomOption VampireCooldown;
    public readonly EnoFramework.CustomOption CanKillNearGarlics;

    public bool LocalPlacedGarlic;
    public bool GarlicsActive = true;

    public PlayerControl? BittenTarget;
    public bool TargetNearGarlic;

    public Vampire() : base(nameof(Vampire))
    {
        Team = Teams.Impostor;
        Color = Palette.ImpostorRed;
        CanTarget = true;
        IncompatibleRoles.Add(typeof(Warlock));

        VampireDelay = OptionsTab.CreateFloatList(
            $"{nameof(VampireDelay)}",
            Colors.Cs(Color, "Vampire kill delay"),
            1f,
            20f,
            10f,
            1f,
            SpawnRate,
            string.Empty,
            "s");
        VampireCooldown = OptionsTab.CreateFloatList(
            nameof(VampireCooldown),
            Colors.Cs(Color, $"{Name} cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        CanKillNearGarlics = OptionsTab.CreateBool(
            $"{Name}{nameof(CanKillNearGarlics)},",
            Colors.Cs(Color, "Can kill near garlics"),
            true,
            SpawnRate);
    }

    public Sprite GetBiteSprite()
    {
        if (_biteSprite == null)
        {
            _biteSprite = Resources.LoadSpriteFromResources("TheOtherRoles.Resources.VampireButton.png", 115f);
        }

        return _biteSprite;
    }

    public Sprite GetGarlicSprite()
    {
        if (_garlicSprite == null)
        {
            _garlicSprite = Resources.LoadSpriteFromResources("TheOtherRoles.Resources.GarlicButton.png", 115f);
        }

        return _garlicSprite;
    }

    public override void CreateCustomButtons(HudManager hudManager)
    {
        base.CreateCustomButtons(hudManager);
        _biteButton = new CustomButton(
            OnBiteButtonClick,
            HasBiteButton,
            CouldUseBiteButton,
            ResetBiteButton,
            GetBiteSprite(),
            CustomButton.ButtonPositions.upperRowLeft,
            hudManager,
            "ActionSecondary",
            false,
            0f,
            OnBiteButtonEffectEnd
        );
        _garlicButton = new CustomButton(
            OnGarlicButtonClick,
            HasGarlicButton,
            CouldUseGarlicButton,
            ResetGarlicButton,
            GetGarlicSprite(),
            new Vector3(0, -0.06f, 0),
            hudManager,
            null,
            true
        );
    }

    private void ResetGarlicButton()
    {
    }

    private bool CouldUseGarlicButton()
    {
        return CachedPlayer.LocalPlayer.PlayerControl.CanMove && !LocalPlacedGarlic;
    }

    private bool HasGarlicButton()
    {
        return !LocalPlacedGarlic && !CachedPlayer.LocalPlayer.Data.IsDead && GarlicsActive && !HideNSeek.isHideNSeekGM;
    }

    private void OnGarlicButtonClick()
    {
        Utilities.EventUtility.StartEvent(EventUtility.EventTypes.Animation);
        LocalPlacedGarlic = true;
        var pos = CachedPlayer.LocalPlayer.transform.position;
        var buff = new byte[sizeof(float) * 2];
        Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, buff, 0 * sizeof(float), sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, buff, 1 * sizeof(float), sizeof(float));

        var writer = AmongUsClient.Instance.StartRpc(CachedPlayer.LocalPlayer.PlayerControl.NetId,
            (byte)CustomRPC.PlaceGarlic, Hazel.SendOption.Reliable);
        writer.WriteBytesAndSize(buff);
        writer.EndMessage();
        RPCProcedure.placeGarlic(buff);
        SoundEffectsManager.play("garlic");
    }

    private void OnBiteButtonEffectEnd()
    {
        if (_biteButton == null) return;
        _biteButton.Timer = _biteButton.MaxTimer;
    }

    private void ResetBiteButton()
    {
        if (_biteButton == null) return;
        _biteButton.Timer = _biteButton.MaxTimer;
        _biteButton.isEffectActive = false;
        _biteButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
    }

    private bool CouldUseBiteButton()
    {
        if (_biteButton == null) return false;
        if (TargetNearGarlic && CanKillNearGarlics)
        {
            _biteButton.actionButton.graphic.sprite = _biteButton.hudManager.KillButton.graphic.sprite;
            _biteButton.showButtonText = true;
        }
        else
        {
            _biteButton.actionButton.graphic.sprite = GetBiteSprite();
            _biteButton.showButtonText = false;
        }

        return CurrentTarget != null && CachedPlayer.LocalPlayer.PlayerControl.CanMove &&
               (!TargetNearGarlic || CanKillNearGarlics);
    }

    private bool HasBiteButton()
    {
        return Player != null && Player == CachedPlayer.LocalPlayer.PlayerControl &&
               !CachedPlayer.LocalPlayer.Data.IsDead;
    }

    private void OnBiteButtonClick()
    {
        if (Player == null || CurrentTarget == null || _biteButton == null) return;
        var murder = Helpers.checkMurderAttempt(Player, CurrentTarget);
        switch (murder)
        {
            case MurderAttemptResult.PerformKill when TargetNearGarlic:
            {
                var writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
                    (byte)CustomRPC.UncheckedMurderPlayer, Hazel.SendOption.Reliable, -1);
                writer.Write(Player.PlayerId);
                writer.Write(CurrentTarget.PlayerId);
                writer.Write(byte.MaxValue);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.uncheckedMurderPlayer(Player.PlayerId, CurrentTarget.PlayerId, byte.MaxValue);

                _biteButton.HasEffect = false; // Block effect on this click
                _biteButton.Timer = _biteButton.MaxTimer;
                break;
            }
            case MurderAttemptResult.PerformKill:
            {
                BittenTarget = CurrentTarget;
                // Notify players about bitten
                var writer = AmongUsClient.Instance.StartRpcImmediately(
                    CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.VampireSetBitten,
                    Hazel.SendOption.Reliable, -1);
                writer.Write(BittenTarget.PlayerId);
                writer.Write((byte)0);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.vampireSetBitten(BittenTarget.PlayerId, 0);

                var lastTimer = (byte)(float)VampireDelay;
                FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(VampireDelay,
                    new Action<float>((p) =>
                    {
                        // Delayed action
                        if (p <= 1f)
                        {
                            var timer = (byte)_biteButton.Timer;
                            if (timer != lastTimer)
                            {
                                lastTimer = timer;
                                var writer = AmongUsClient.Instance.StartRpcImmediately(
                                    CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.ShareGhostInfo,
                                    Hazel.SendOption.Reliable, -1);
                                writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                                writer.Write((byte)RPCProcedure.GhostInfoTypes.VampireTimer);
                                writer.Write(timer);
                                AmongUsClient.Instance.FinishRpcImmediately(writer);
                            }
                        }

                        if (p == 1f)
                        {
                            // Perform kill if possible and reset bitten (regardless whether the kill was successful or not)
                            Helpers.checkMurderAttemptAndKill(Player, BittenTarget, showAnimation: false);
                            var writer = AmongUsClient.Instance.StartRpcImmediately(
                                CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.VampireSetBitten,
                                Hazel.SendOption.Reliable, -1);
                            writer.Write(byte.MaxValue);
                            writer.Write(byte.MaxValue);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.vampireSetBitten(byte.MaxValue, byte.MaxValue);
                        }
                    })));
                SoundEffectsManager.play("vampireBite");

                _biteButton.HasEffect = true; // Trigger effect on this click
                break;
            }
            case MurderAttemptResult.BlankKill:
                _biteButton.Timer = _biteButton.MaxTimer;
                _biteButton.HasEffect = false;
                break;
            default:
                _biteButton.HasEffect = false;
                break;
        }
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        BittenTarget = null;
        TargetNearGarlic = false;
        LocalPlacedGarlic = false;
        GarlicsActive = SpawnRate;
    }
}