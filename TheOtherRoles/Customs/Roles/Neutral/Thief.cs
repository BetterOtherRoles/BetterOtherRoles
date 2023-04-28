using Reactor.Networking.Attributes;
using TheOtherRoles.Customs.Roles.Crewmate;
using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.EnoFramework.Utils;
using TheOtherRoles.Objects;
using TheOtherRoles.Players;
using UnityEngine;

namespace TheOtherRoles.Customs.Roles.Neutral;

[EnoSingleton]
public class Thief : CustomRole
{
    private CustomButton? _killButton;

    public readonly EnoFramework.CustomOption KillCooldown;
    public readonly EnoFramework.CustomOption HasImpostorVision;
    public readonly EnoFramework.CustomOption CanUseVents;
    public readonly EnoFramework.CustomOption CanKillSheriff;

    public Thief() : base(nameof(Thief))
    {
        Team = Teams.Neutral;
        Color = new Color32(71, 99, 45, byte.MaxValue);
        CanTarget = true;

        KillCooldown = OptionsTab.CreateFloatList(
            $"{Name}{nameof(KillCooldown)}",
            Colors.Cs(Color, "Kill cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        HasImpostorVision = OptionsTab.CreateBool(
            $"{Name}{nameof(HasImpostorVision)}",
            Colors.Cs(Color, "Has impostor vision"),
            false,
            SpawnRate);
        CanUseVents = OptionsTab.CreateBool(
            $"{Name}{nameof(CanUseVents)}",
            Colors.Cs(Color, "Can use vents"),
            true,
            SpawnRate);
        CanKillSheriff = OptionsTab.CreateBool(
            $"{Name}{nameof(CanKillSheriff)}",
            Colors.Cs(Color, "Can kill sheriff"),
            true,
            SpawnRate);
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
        if (Player == null || CurrentTarget == null || _killButton == null) return;
        var result = Helpers.checkMurderAttempt(Player, CurrentTarget);
        if (result == MurderAttemptResult.BlankKill)
        {
            _killButton.Timer = _killButton.MaxTimer;
            return;
        }

        if (!CurrentTarget.Data.Role.IsImpostor || !Singleton<Jackal>.Instance.Is(CurrentTarget) ||
            (!CanKillSheriff && Singleton<Sheriff>.Instance.Is(CurrentTarget)))
        {
            Rpc.UncheckedMurderPlayer(Player, Player, false);
            Player.clearAllTasks();
            return;
        }

        if (!Player.Data.IsDead && result == MurderAttemptResult.PerformKill)
        {
            StealRole(CachedPlayer.LocalPlayer, $"{CurrentTarget.PlayerId}");
        }

        if (result == MurderAttemptResult.PerformKill)
        {
            Rpc.UncheckedMurderPlayer(Player, CurrentTarget, true);
        }
    }

    [MethodRpc((uint)Rpc.Id.ThiefStealRole)]
    private static void StealRole(PlayerControl sender, string rawData)
    {
        if (Singleton<Thief>.Instance.Player == null) return;
        var targetId = byte.Parse(rawData);
        var target = Helpers.playerById(targetId);
        if (target == null) return;
        var targetRole = GetRoleByPlayer(target);
        if (targetRole == null) return;
        targetRole.Player = Singleton<Thief>.Instance.Player;
        Singleton<Thief>.Instance.Player = target;
    }
}