using System.Collections.Generic;
using Reactor.Networking.Attributes;
using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.EnoFramework.Utils;
using TheOtherRoles.Objects;
using TheOtherRoles.Players;
using TMPro;
using UnityEngine;
using Resources = TheOtherRoles.EnoFramework.Utils.Resources;

namespace TheOtherRoles.Customs.Roles.Crewmate;

[EnoSingleton]
public class Deputy : CustomRole
{
    private Sprite? _handcuffSprite;
    private Sprite? _handcuffedSprite;

    private CustomButton? _handcuffButton;
    private TMP_Text? _handcuffButtonText;
    private CustomButton? _killButton;

    public readonly EnoFramework.CustomOption NumberOfHandcuffs;
    public readonly EnoFramework.CustomOption HandcuffCooldown;
    public readonly EnoFramework.CustomOption HandcuffDuration;
    public readonly EnoFramework.CustomOption KillCooldown;
    public readonly EnoFramework.CustomOption PromotedWhen;
    public readonly EnoFramework.CustomOption CanKillNeutrals;

    public bool KillButtonEnabled;
    public int UsedHandcuffs;
    public readonly List<PlayerControl> HandcuffedPlayers = new();

    public Deputy() : base(nameof(Deputy))
    {
        Team = Teams.Crewmate;
        Color = new Color32(248, 205, 70, byte.MaxValue);
        CanTarget = true;

        NumberOfHandcuffs = OptionsTab.CreateFloatList(
            $"{Name}{nameof(NumberOfHandcuffs)}",
            Colors.Cs(Color, "Number of handcuffs"),
            0f,
            15f,
            3f,
            1f,
            SpawnRate);
        HandcuffCooldown = OptionsTab.CreateFloatList(
            $"{Name}{nameof(HandcuffCooldown)}",
            Colors.Cs(Color, "Handcuff cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        HandcuffDuration = OptionsTab.CreateFloatList(
            $"{Name}{nameof(HandcuffDuration)}",
            Colors.Cs(Color, "Handcuff duration"),
            2f,
            60f,
            10f,
            1f,
            SpawnRate,
            string.Empty,
            "s");
        PromotedWhen = OptionsTab.CreateStringList(
            $"{Name}{nameof(PromotedWhen)}",
            Colors.Cs(Color, "Can kill if sheriff dies"),
            new List<string> { "no", "yes (immediately)", "yes (after meeting)" },
            "no",
            SpawnRate);
        KillCooldown = OptionsTab.CreateFloatList(
            $"{Name}{nameof(KillCooldown)}",
            Colors.Cs(Color, "Kill cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            PromotedWhen,
            string.Empty,
            "s");
        CanKillNeutrals = OptionsTab.CreateBool(
            $"{Name}{nameof(CanKillNeutrals)}",
            Colors.Cs(Color, "Can kill neutral roles"),
            true,
            PromotedWhen);
    }

    public Sprite GetHandcuffSprite()
    {
        if (_handcuffSprite == null)
        {
            _handcuffSprite =
                Resources.LoadSpriteFromResources("TheOtherRoles.Resources.DeputyHandcuffButton.png", 115f);
        }

        return _handcuffSprite;
    }

    // Can be used to enable / disable the handcuff effect on the target's buttons
    public Sprite GetHandcuffedSprite()
    {
        if (_handcuffedSprite == null)
        {
            _handcuffedSprite = Resources.LoadSpriteFromResources("TheOtherRoles.Resources.DeputyHandcuffed.png", 115f);
        }

        return _handcuffedSprite;
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
        _handcuffButton = new CustomButton(
            OnHandcuffButtonClick,
            HasHandcuffButton,
            CouldUseHandcuffButton,
            ResetHandcuffButton,
            GetHandcuffSprite(),
            CustomButton.ButtonPositions.lowerRowRight,
            hudManager,
            "ActionQuaternary"
        );
        _handcuffButtonText = UnityEngine.Object.Instantiate(
            _handcuffButton.actionButton.cooldownTimerText,
            _handcuffButton.actionButton.cooldownTimerText.transform.parent
        );
        _handcuffButtonText.text = "";
        _handcuffButtonText.enableWordWrapping = false;
        _handcuffButtonText.transform.localScale = Vector3.one * 0.5f;
        _handcuffButtonText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
    }

    private void ResetHandcuffButton()
    {
        if (_handcuffButton == null) return;
        _handcuffButton.Timer = _handcuffButton.MaxTimer;
    }

    private bool CouldUseHandcuffButton()
    {
        if (_handcuffButtonText != null)
        {
            _handcuffButtonText.text = $"{NumberOfHandcuffs - UsedHandcuffs}";
        }

        return Player != null && Is(CachedPlayer.LocalPlayer) && CurrentTarget != null;
    }

    private bool HasHandcuffButton()
    {
        return Player != null && Is(CachedPlayer.LocalPlayer) && !CachedPlayer.LocalPlayer.Data.IsDead;
    }

    private void OnHandcuffButtonClick()
    {
        if (_handcuffButton == null || Player == null || CurrentTarget == null) return;
        AddHandcuff(CurrentTarget);
        CurrentTarget = null;
        _handcuffButton.Timer = _handcuffButton.MaxTimer;
        SoundEffectsManager.play("deputyHandcuff");
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

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        HandcuffedPlayers.Clear();
        KillButtonEnabled = false;
        UsedHandcuffs = 0;
    }

    public static void AddHandcuff(PlayerControl target)
    {
        RpcAddHandcuff(CachedPlayer.LocalPlayer, $"{target.PlayerId}");
    }

    public static void RemoveHandcuff(PlayerControl target)
    {
        RpcRemoveHandcuff(CachedPlayer.LocalPlayer, $"{target.PlayerId}");
    }

    [MethodRpc((uint)Rpc.Id.DeputyAddHandcuff)]
    private static void RpcAddHandcuff(PlayerControl sender, string rawData)
    {
        var targetId = byte.Parse(rawData);
        var target = Helpers.playerById(targetId);
        if (target == null) return;
        if (!Singleton<Deputy>.Instance.HandcuffedPlayers.Contains(target))
        {
            Singleton<Deputy>.Instance.HandcuffedPlayers.Add(target);
        }

        Singleton<Deputy>.Instance.UsedHandcuffs++;
    }

    [MethodRpc((uint)Rpc.Id.DeputyRemoveHandcuff)]
    private static void RpcRemoveHandcuff(PlayerControl sender, string rawData)
    {
        var targetId = byte.Parse(rawData);
        var target = Helpers.playerById(targetId);
        if (target == null) return;
        if (!Singleton<Deputy>.Instance.HandcuffedPlayers.Contains(target)) return;
        Singleton<Deputy>.Instance.HandcuffedPlayers.Remove(target);
    }
}