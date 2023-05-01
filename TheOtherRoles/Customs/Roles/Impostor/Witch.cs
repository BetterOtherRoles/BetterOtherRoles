using System.Collections.Generic;
using TheOtherRoles.Customs.Modifiers;
using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.EnoFramework.Utils;
using TheOtherRoles.Objects;
using TheOtherRoles.Players;
using UnityEngine;
using Resources = TheOtherRoles.EnoFramework.Utils.Resources;

namespace TheOtherRoles.Customs.Roles.Impostor;

[EnoSingleton]
public class Witch : CustomRole
{
    private Sprite? _spellSprite;
    private Sprite? _spelledOverlaySprite;

    private CustomButton? _spellButton;

    public readonly EnoFramework.CustomOption SpellCooldown;
    public readonly EnoFramework.CustomOption SpellCastingDuration;
    public readonly EnoFramework.CustomOption AdditionalCooldown;
    public readonly EnoFramework.CustomOption CanSpellAnyone;
    public readonly EnoFramework.CustomOption TriggerBothCooldown;
    public readonly EnoFramework.CustomOption WitchVoteSaveTargets;

    public float CurrentCooldownAddition;
    public PlayerControl? SpellCastingTarget;
    public List<PlayerControl> FutureSpelled = new();

    public Witch() : base(nameof(Witch))
    {
        Team = Teams.Impostor;
        Color = Palette.ImpostorRed;
        CanTarget = true;

        IntroDescription = "Cast a spell upon your foes";
        ShortDescription = "Cast a spell upon your foes";

        SpellCooldown = OptionsTab.CreateFloatList(
            $"{Name}{nameof(SpellCooldown)}",
            Cs($"Spell cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        SpellCastingDuration = OptionsTab.CreateFloatList(
            $"{Name}{nameof(SpellCastingDuration)}",
            Cs($"Spell casting duration"),
            0f,
            10f,
            1f,
            1f,
            SpawnRate,
            string.Empty,
            "s");
        AdditionalCooldown = OptionsTab.CreateFloatList(
            $"{Name}{nameof(AdditionalCooldown)}",
            Cs($"Spell additional cooldown"),
            0f,
            60f,
            10f,
            5f,
            SpawnRate,
            string.Empty,
            "s");
        CanSpellAnyone = OptionsTab.CreateBool(
            $"{Name}{nameof(CanSpellAnyone)}",
            Cs("Can spell anyone"),
            false,
            SpawnRate);
        TriggerBothCooldown = OptionsTab.CreateBool(
            $"{Name}{nameof(TriggerBothCooldown)}",
            Cs("Trigger both cooldown"),
            false,
            SpawnRate);
        WitchVoteSaveTargets = OptionsTab.CreateBool(
            $"{Name}{nameof(WitchVoteSaveTargets)}",
            Cs("Voting the witch saves all targets"),
            false,
            SpawnRate);
    }

    public Sprite GetSpellSprite()
    {
        if (_spellSprite == null)
        {
            _spellSprite = Resources.LoadSpriteFromResources("TheOtherRoles.Resources.SpellButton.png", 115f);
        }

        return _spellSprite;
    }

    public Sprite GetSpelledOverlaySprite()
    {
        if (_spelledOverlaySprite == null)
        {
            _spelledOverlaySprite =
                Resources.LoadSpriteFromResources("TheOtherRoles.Resources.SpellButtonMeeting.png", 225f);
        }

        return _spelledOverlaySprite;
    }

    public override void CreateCustomButtons(HudManager hudManager)
    {
        base.CreateCustomButtons(hudManager);
        _spellButton = new CustomButton(
            OnSpellButtonClick,
            HasSpellButton,
            CouldUseSpellButton,
            ResetSpellButton,
            GetSpellSprite(),
            CustomButton.ButtonPositions.upperRowLeft,
            hudManager,
            "ActionQuaternary",
            true,
            SpellCastingDuration,
            OnSpellButtonEffectEnd
        );
    }

    private void OnSpellButtonEffectEnd()
    {
        if (SpellCastingTarget == null || _spellButton == null || Player == null || CurrentTarget == null) return;
        var attempt = Helpers.checkMurderAttempt(Player, SpellCastingTarget);
        if (attempt == MurderAttemptResult.PerformKill)
        {
            var writer = AmongUsClient.Instance.StartRpcImmediately(
                CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.SetFutureSpelled,
                Hazel.SendOption.Reliable, -1);
            writer.Write(CurrentTarget.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.setFutureSpelled(CurrentTarget.PlayerId);
        }

        if (attempt is MurderAttemptResult.BlankKill or MurderAttemptResult.PerformKill)
        {
            CurrentCooldownAddition += AdditionalCooldown;
            _spellButton.MaxTimer = SpellCooldown + CurrentCooldownAddition;
            Patches.PlayerControlFixedUpdatePatch
                .miniCooldownUpdate(); // Modifies the MaxTimer if the witch is the mini
            _spellButton.Timer = _spellButton.MaxTimer;
            if (TriggerBothCooldown)
            {
                var multiplier = Mini.mini != null && CachedPlayer.LocalPlayer.PlayerControl == Mini.mini
                    ? Mini.isGrownUp() ? 0.66f : 2f
                    : 1f;
                Player.killTimer = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown * multiplier;
            }
        }
        else
        {
            _spellButton.Timer = 0f;
        }

        SpellCastingTarget = null;
    }

    private void ResetSpellButton()
    {
        if (_spellButton == null) return;
        _spellButton.Timer = _spellButton.MaxTimer;
        _spellButton.isEffectActive = false;
        SpellCastingTarget = null;
    }

    private bool CouldUseSpellButton()
    {
        if (_spellButton == null) return false;
        if (_spellButton.isEffectActive && SpellCastingTarget != CurrentTarget)
        {
            SpellCastingTarget = null;
            _spellButton.Timer = 0f;
            _spellButton.isEffectActive = false;
        }

        return CachedPlayer.LocalPlayer.PlayerControl.CanMove && CurrentTarget != null;
    }

    private bool HasSpellButton()
    {
        return Player != null && Player == CachedPlayer.LocalPlayer.PlayerControl &&
               !CachedPlayer.LocalPlayer.Data.IsDead;
    }

    private void OnSpellButtonClick()
    {
        if (CurrentTarget == null) return;
        SpellCastingTarget = CurrentTarget;
        SoundEffectsManager.play("witchSpell");
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        FutureSpelled.Clear();
        SpellCastingTarget = null;
        CurrentCooldownAddition = 0f;
    }
}