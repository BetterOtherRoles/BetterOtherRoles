using Reactor.Networking.Attributes;
using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.EnoFramework.Utils;
using TheOtherRoles.Objects;
using TheOtherRoles.Players;
using UnityEngine;
using Resources = TheOtherRoles.EnoFramework.Utils.Resources;

namespace TheOtherRoles.Customs.Roles.Neutral;

[EnoSingleton(1)]
public class Jackal : CustomRole
{
    private Sprite? _sidekickSprite;

    private CustomButton? _killButton;
    private CustomButton? _sidekickButton;

    public readonly EnoFramework.CustomOption KillCooldown;
    public readonly EnoFramework.CustomOption HasImpostorVision;
    public readonly EnoFramework.CustomOption CanUseVents;
    public readonly EnoFramework.CustomOption CanCreateSidekick;
    public readonly EnoFramework.CustomOption CreateSidekickCooldown;
    public readonly EnoFramework.CustomOption SidekickPromoteToJackal;
    public readonly EnoFramework.CustomOption CanCreateSidekickWhenPromoted;

    public PlayerControl? FutureSidekick;
    public bool IsSidekickPromoted;

    public Jackal() : base(nameof(Jackal))
    {
        Team = Teams.Neutral;
        Color = new Color32(0, 180, 235, byte.MaxValue);
        CanTarget = true;
        IsSidekickPromoted = false;
        
        IntroDescription = $"Kill all Crewmates and {Colors.Cs(Palette.ImpostorRed, "impostors")} to win";
        ShortDescription = "Kill everyone";
        
        KillCooldown = OptionsTab.CreateFloatList(
            $"{Name}{nameof(KillCooldown)}",
            Cs("Kill cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        HasImpostorVision = OptionsTab.CreateBool(
            $"{Name}{nameof(HasImpostorVision)}",
            Cs("Has impostor vision"),
            true,
            SpawnRate);
        CanUseVents = OptionsTab.CreateBool(
            $"{Name}{nameof(CanUseVents)}",
            Cs("Can use vents"),
            true,
            SpawnRate);
        CanCreateSidekick = OptionsTab.CreateBool(
            $"{Name}{nameof(CanCreateSidekick)}",
            Cs("Can create a sidekick"),
            false,
            SpawnRate);
        CreateSidekickCooldown = OptionsTab.CreateFloatList(
            $"{Name}{nameof(CreateSidekickCooldown)}",
            Cs("Create a sidekick cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            CanCreateSidekick,
            string.Empty,
            "s");
        SidekickPromoteToJackal = OptionsTab.CreateBool(
            $"{Name}{nameof(SidekickPromoteToJackal)}",
            Cs("Sidekick gets promoted to jackal on jackal death"),
            false,
            CanCreateSidekick);
        CanCreateSidekickWhenPromoted = OptionsTab.CreateBool(
            $"{Name}{nameof(CanCreateSidekickWhenPromoted)}",
            Cs("Jackal promoted from sidekick can create a sidekick"),
            false,
            CanCreateSidekick);
    }

    public Sprite GetSidekickSprite()
    {
        if (_sidekickSprite == null)
        {
            _sidekickSprite = Resources.LoadSpriteFromResources("TheOtherRoles.Resources.SidekickButton.png", 115f);
        }

        return _sidekickSprite;
    }

    public override void CreateCustomButtons(HudManager hudManager)
    {
        base.CreateCustomButtons(hudManager);
        _killButton = new CustomButton(
            OnKillButtonClick,
            HasKillButton,
            CouldUseKillButton,
            ResetKillButton,
            hudManager.KillButton.graphic.sprite,
            CustomButton.ButtonPositions.upperRowRight,
            hudManager,
            "ActionSecondary"
        );
        _sidekickButton = new CustomButton(
            OnSidekickButtonClick,
            HasSidekickButton,
            CouldUseSidekickButton,
            ResetSidekickButton,
            GetSidekickSprite(),
            CustomButton.ButtonPositions.lowerRowCenter,
            hudManager,
            "ActionQuaternary"
        );
    }

    private void ResetSidekickButton()
    {
        if (_sidekickButton == null) return;
        _sidekickButton.Timer = _sidekickButton.MaxTimer;
    }

    private bool CouldUseSidekickButton()
    {
        return CanCreateSidekick && CurrentTarget != null && CachedPlayer.LocalPlayer.PlayerControl.CanMove;
    }

    private bool HasSidekickButton()
    {
        if (IsSidekickPromoted && !CanCreateSidekickWhenPromoted) return false;
        return CanCreateSidekick && Player != null && Is(CachedPlayer.LocalPlayer) &&
               !CachedPlayer.LocalPlayer.Data.IsDead && FutureSidekick == null;
    }

    private void OnSidekickButtonClick()
    {
        if (CurrentTarget == null) return;
        CreateSidekick(CachedPlayer.LocalPlayer, $"{CurrentTarget.PlayerId}");
        SoundEffectsManager.play("jackalSidekick");
    }
    
    [MethodRpc((uint)Rpc.Id.JackalCreateSidekick)]
    private static void CreateSidekick(PlayerControl sender, string rawData)
    {
        if (Singleton<Jackal>.Instance.Player == null) return;
        var playerId = byte.Parse(rawData);
        var player = Helpers.playerById(playerId);
        if (player == null) return;
        Singleton<Jackal>.Instance.FutureSidekick = player;
    }

    private void ResetKillButton()
    {
        if (_killButton == null) return;
        _killButton.Timer = _killButton.MaxTimer;
    }

    private bool CouldUseKillButton()
    {
        return CurrentTarget != null && CachedPlayer.LocalPlayer.PlayerControl.CanMove;
    }

    private bool HasKillButton()
    {
        return Player != null && Is(CachedPlayer.LocalPlayer) && !CachedPlayer.LocalPlayer.Data.IsDead;
    }

    private void OnKillButtonClick()
    {
        if (_killButton == null || Player == null || CurrentTarget == null) return;
        if (Helpers.checkMurderAttemptAndKill(Player, CurrentTarget) == MurderAttemptResult.SuppressKill) return;
        _killButton.Timer = _killButton.MaxTimer;
        CurrentTarget = null;
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        FutureSidekick = null;
        IsSidekickPromoted = false;
    }
}