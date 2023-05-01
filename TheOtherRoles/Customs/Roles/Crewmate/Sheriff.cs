using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.EnoFramework.Utils;
using TheOtherRoles.Objects;
using TheOtherRoles.Players;
using UnityEngine;

namespace TheOtherRoles.Customs.Roles.Crewmate;

[EnoSingleton]
public class Sheriff : CustomRole
{
    private CustomButton? _killButton;

    public readonly EnoFramework.CustomOption KillCooldown;
    public readonly EnoFramework.CustomOption CanKillNeutrals;

    public Sheriff() : base(nameof(Sheriff))
    {
        Team = Teams.Crewmate;
        Color = new Color32(248, 205, 70, byte.MaxValue);
        CanTarget = true;

        IntroDescription = $"Shoot the {Colors.Cs(Palette.ImpostorRed, "impostors")}";
        ShortDescription = "Shoot the impostors";

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
        CanKillNeutrals = OptionsTab.CreateBool(
            $"{Name}{nameof(CanKillNeutrals)}",
            Cs("Can kill neutral roles"),
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
        if (_killButton == null || Player == null || CurrentTarget == null) return;
        var result = Helpers.checkMurderAttempt(Player, CurrentTarget);
        if (result == MurderAttemptResult.SuppressKill) return;
        if (result == MurderAttemptResult.PerformKill)
        {
            var targetRole = GetRoleByPlayer(CurrentTarget);
            if ((targetRole is { IsNeutral: true } && CanKillNeutrals) || CurrentTarget.Data.Role.IsImpostor)
            {
                Rpc.UncheckedMurderPlayer(Player, CurrentTarget, true);
            }
            else
            {
                Rpc.UncheckedMurderPlayer(Player, Player, true);
            }
        }

        _killButton.Timer = _killButton.MaxTimer;
        CurrentTarget = null;
    }
}