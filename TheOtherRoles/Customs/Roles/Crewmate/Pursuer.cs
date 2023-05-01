using System.Collections.Generic;
using Hazel;
using Reactor.Networking.Attributes;
using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.Objects;
using TheOtherRoles.Players;
using TMPro;
using UnityEngine;
using Resources = TheOtherRoles.EnoFramework.Utils.Resources;

namespace TheOtherRoles.Customs.Roles.Crewmate;

[EnoSingleton]
public class Pursuer : CustomRole
{
    private Sprite? _blankSprite;

    private CustomButton? _blankButton;
    private TMP_Text? _blankButtonText;

    public readonly EnoFramework.CustomOption IsEnabled;
    public readonly EnoFramework.CustomOption BlankCooldown;
    public readonly EnoFramework.CustomOption BlankNumber;

    public readonly List<PlayerControl> BlankedPlayers = new();
    public int UsedBlanks;

    public Pursuer() : base(nameof(Pursuer), false)
    {
        Team = Teams.Crewmate;
        Color = new Color32(134, 153, 25, byte.MaxValue);

        IntroDescription = "Blank the Impostors";
        ShortDescription = "Blank the Impostors";

        IsEnabled = OptionsTab.CreateBool(
            $"{Name}{nameof(IsEnabled)}",
            Cs($"Enable {Name}"),
            true);

        BlankCooldown = OptionsTab.CreateFloatList(
            $"{Name}{nameof(BlankCooldown)}",
            Cs("Blank cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            IsEnabled,
            string.Empty,
            "s");
        BlankNumber = OptionsTab.CreateFloatList(
            $"{Name}{nameof(BlankNumber)}",
            Cs("Blank amount"),
            0f,
            10f,
            5f,
            1f,
            IsEnabled);
    }

    public Sprite GetBlankSprite()
    {
        if (_blankSprite == null)
        {
            _blankSprite = Resources.LoadSpriteFromResources("TheOtherRoles.Resources.PursuerButton.png", 115f);
        }

        return _blankSprite;
    }

    public override void CreateCustomButtons(HudManager hudManager)
    {
        base.CreateCustomButtons(hudManager);
        _blankButton = new CustomButton(
            OnBlankButtonClick,
            HasBlankButton,
            CouldUseBlankButton,
            ResetBlankButton,
            GetBlankSprite(),
            CustomButton.ButtonPositions.lowerRowRight,
            hudManager,
            "ActionQuaternary"
        );
        _blankButtonText = UnityEngine.Object.Instantiate(_blankButton.actionButton.cooldownTimerText,
            _blankButton.actionButton.cooldownTimerText.transform.parent);
        _blankButtonText.text = "";
        _blankButtonText.enableWordWrapping = false;
        _blankButtonText.transform.localScale = Vector3.one * 0.5f;
        _blankButtonText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
    }

    private void ResetBlankButton()
    {
        if (_blankButton == null) return;
        _blankButton.Timer = _blankButton.MaxTimer;
    }

    private bool CouldUseBlankButton()
    {
        if (_blankButtonText != null)
        {
            _blankButtonText.text = $"{BlankNumber - UsedBlanks}";
        }

        return UsedBlanks < BlankNumber && CachedPlayer.LocalPlayer.PlayerControl.CanMove && CurrentTarget != null;
    }

    private bool HasBlankButton()
    {
        return IsLocalPlayerAndAlive() && UsedBlanks < BlankNumber;
    }

    private void OnBlankButtonClick()
    {
        if (CurrentTarget == null || _blankButton == null) return;
        BlankPlayer(CachedPlayer.LocalPlayer, $"{CurrentTarget.PlayerId}");
        CurrentTarget = null;
        UsedBlanks++;
        _blankButton.Timer = _blankButton.MaxTimer;
        SoundEffectsManager.play("pursuerBlank");
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        UsedBlanks = 0;
        BlankedPlayers.Clear();
    }

    [MethodRpc((uint)Rpc.Id.PursuerBlankPlayer)]
    private static void BlankPlayer(PlayerControl sender, string rawData)
    {
        var targetId = byte.Parse(rawData);
        var target = Helpers.playerById(targetId);
        if (target == null || Singleton<Pursuer>.Instance.BlankedPlayers.Contains(target)) return;
        Singleton<Pursuer>.Instance.BlankedPlayers.Add(target);
    }

    [MethodRpc((uint)Rpc.Id.PursuerRemoveBlank)]
    public static void RemoveBlank(PlayerControl sender, string rawData)
    {
        var targetId = byte.Parse(rawData);
        var target = Helpers.playerById(targetId);
        if (target == null || !Singleton<Pursuer>.Instance.BlankedPlayers.Contains(target)) return;
        Singleton<Pursuer>.Instance.BlankedPlayers.Remove(target);
    }
}