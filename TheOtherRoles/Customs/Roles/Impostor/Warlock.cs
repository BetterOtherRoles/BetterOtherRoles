using System;
using TheOtherRoles.Customs.Modifiers;
using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.EnoFramework.Utils;
using TheOtherRoles.Objects;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;
using UnityEngine;
using Resources = TheOtherRoles.EnoFramework.Utils.Resources;

namespace TheOtherRoles.Customs.Roles.Impostor;

[EnoSingleton]
public class Warlock : CustomRole
{
    private Sprite? _curseSprite;
    private Sprite? _curseKillSprite;

    private CustomButton? _curseButton;

    public readonly EnoFramework.CustomOption CurseCooldown;
    public readonly EnoFramework.CustomOption RootDuration;

    public PlayerControl? CurseVictim;
    public PlayerControl? CurseVictimTarget;

    public Warlock() : base(nameof(Warlock))
    {
        Team = Teams.Impostor;
        Color = Palette.ImpostorRed;
        CanTarget = true;
        IncompatibleRoles.Add(typeof(Vampire));

        IntroDescription = "Curse other players and kill everyone";
        ShortDescription = "Curse and kill everyone";

        CurseCooldown = OptionsTab.CreateFloatList(
            $"{Name}{nameof(CurseCooldown)}",
            Cs($"{Name} cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        RootDuration = OptionsTab.CreateFloatList(
            $"{Name}{nameof(RootDuration)}",
            Cs("Root duration"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
    }

    public Sprite GetCurseSprite()
    {
        if (_curseSprite == null)
        {
            _curseSprite = Resources.LoadSpriteFromResources("TheOtherRoles.Resources.CurseButton.png", 115f);
        }

        return _curseSprite;
    }

    public Sprite GetCurseKillSprite()
    {
        if (_curseKillSprite == null)
        {
            _curseKillSprite = Resources.LoadSpriteFromResources("TheOtherRoles.Resources.CurseKillButton.png", 115f);
        }

        return _curseKillSprite;
    }

    public override void CreateCustomButtons(HudManager hudManager)
    {
        base.CreateCustomButtons(hudManager);
        _curseButton = new CustomButton(
            OnCurseButtonClick,
            HasCurseButton,
            CouldUseCurseButton,
            ResetCurseButton,
            GetCurseSprite(),
            CustomButton.ButtonPositions.upperRowLeft,
            hudManager,
            "ActionQuaternary"
        );
    }

    private void ResetCurseButton()
    {
        if (_curseButton == null) return;
        _curseButton.Timer = _curseButton.MaxTimer;
        _curseButton.Sprite = GetCurseSprite();
        CurseVictim = null;
        CurseVictimTarget = null;
    }

    private bool CouldUseCurseButton()
    {
        return ((CurseVictim == null && CurrentTarget != null) || (CurseVictim != null && CurseVictimTarget != null)) &&
               CachedPlayer.LocalPlayer.PlayerControl.CanMove;
    }

    private bool HasCurseButton()
    {
        return Player != null && Player == CachedPlayer.LocalPlayer.PlayerControl &&
               !CachedPlayer.LocalPlayer.Data.IsDead;
    }

    private void OnCurseButtonClick()
    {
        if (_curseButton == null || Player == null) return;
        if (CurseVictim == null)
        {
            if (CurrentTarget == null) return;
            // Apply Curse
            CurseVictim = CurrentTarget;
            _curseButton.Sprite = GetCurseKillSprite();
            _curseButton.Timer = 1f;
            SoundEffectsManager.play("warlockCurse");

            // Ghost Info
            var writer = AmongUsClient.Instance.StartRpcImmediately(
                CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.ShareGhostInfo,
                Hazel.SendOption.Reliable, -1);
            writer.Write(CachedPlayer.LocalPlayer.PlayerId);
            writer.Write((byte)RPCProcedure.GhostInfoTypes.WarlockTarget);
            writer.Write(CurseVictim.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        else if (CurseVictim != null && CurseVictimTarget != null)
        {
            var murder = Helpers.checkMurderAttemptAndKill(Player, CurseVictimTarget, showAnimation: false);
            if (murder == MurderAttemptResult.SuppressKill) return;

            // If blanked or killed
            if (RootDuration > 0f)
            {
                AntiTeleport.position = CachedPlayer.LocalPlayer.transform.position;
                CachedPlayer.LocalPlayer.PlayerControl.moveable = false;
                CachedPlayer.LocalPlayer.NetTransform
                    .Halt(); // Stop current movement so the warlock is not just running straight into the next object
                FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(RootDuration,
                    new Action<float>(p =>
                    {
                        // Delayed action
                        if (p == 1f)
                        {
                            CachedPlayer.LocalPlayer.PlayerControl.moveable = true;
                        }
                    })));
            }

            CurseVictim = null;
            CurseVictimTarget = null;
            _curseButton.Sprite = GetCurseSprite();
            Player.killTimer = _curseButton.Timer = _curseButton.MaxTimer;

            var writer = AmongUsClient.Instance.StartRpcImmediately(
                CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.ShareGhostInfo,
                Hazel.SendOption.Reliable, -1);
            writer.Write(CachedPlayer.LocalPlayer.PlayerId);
            writer.Write((byte)RPCProcedure.GhostInfoTypes.WarlockTarget);
            writer.Write(byte.MaxValue); // This will set it to null!
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
    }

    public void ResetCurse()
    {
        if (_curseButton != null)
        {
            _curseButton.Timer = _curseButton.MaxTimer;
            _curseButton.Sprite = GetCurseSprite();
            _curseButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
        }

        CurrentTarget = null;
        CurseVictim = null;
        CurseVictimTarget = null;
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        CurseVictim = null;
        CurseVictimTarget = null;
    }
}