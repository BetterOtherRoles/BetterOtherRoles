using System.Collections.Generic;
using Reactor.Networking.Attributes;
using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.Objects;
using TheOtherRoles.Players;
using TMPro;
using UnityEngine;
using Resources = TheOtherRoles.EnoFramework.Utils.Resources;

namespace TheOtherRoles.Customs.Roles.Crewmate;

[EnoSingleton]
public class Trapper : CustomRole
{
    private Sprite? _placeTrapSprite;

    private CustomButton? _placeTrapButton;
    private TMP_Text? _placeTrapButtonText;

    public readonly EnoFramework.CustomOption PlaceTrapCooldown;
    public readonly EnoFramework.CustomOption MaxCharges;
    public readonly EnoFramework.CustomOption RechargeTasksNumber;
    public readonly EnoFramework.CustomOption TrapNeededTriggerToReveal;
    public readonly EnoFramework.CustomOption AnonymousMap;
    public readonly EnoFramework.CustomOption InfoType;
    public readonly EnoFramework.CustomOption TrapDuration;

    public readonly List<PlayerControl> PlayersOnMap = new();
    public int Charges;

    public Trapper() : base(nameof(Trapper))
    {
        Team = Teams.Crewmate;
        Color = new Color32(110, 57, 105, byte.MaxValue);

        PlaceTrapCooldown = OptionsTab.CreateFloatList(
            $"{Name}{nameof(PlaceTrapCooldown)}",
            Cs("Place trap cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        MaxCharges = OptionsTab.CreateFloatList(
            $"{Name}{nameof(MaxCharges)}",
            Cs("Max traps charges"),
            1f,
            15f,
            5f,
            1f,
            SpawnRate);
        RechargeTasksNumber = OptionsTab.CreateFloatList(
            $"{Name}{nameof(RechargeTasksNumber)}",
            Cs("Number of tasks needed for recharging"),
            1f,
            15f,
            2f,
            1f,
            SpawnRate);
        TrapNeededTriggerToReveal = OptionsTab.CreateFloatList(
            $"{Name}{nameof(TrapNeededTriggerToReveal)}",
            Cs("Trap needed trigger to reveal"),
            2f,
            10f,
            3f,
            1f,
            SpawnRate);
        AnonymousMap = OptionsTab.CreateBool(
            $"{Name}{nameof(AnonymousMap)}",
            Cs("Show anonymous map"),
            true,
            SpawnRate);
        InfoType = OptionsTab.CreateStringList(
            $"{Name}{nameof(InfoType)}",
            Cs("Trap information type"),
            new List<string> { "role", "name", "good/evil" });
        TrapDuration = OptionsTab.CreateFloatList(
            $"{Name}{nameof(TrapDuration)}",
            Cs("Trap duration"),
            1f,
            15f,
            5f,
            1f,
            SpawnRate,
            string.Empty,
            "s");
    }

    public Sprite GetPlaceTrapSprite()
    {
        if (_placeTrapSprite == null)
        {
            _placeTrapSprite =
                Resources.LoadSpriteFromResources("TheOtherRoles.Resources.Trapper_Place_Button.png", 115f);
        }

        return _placeTrapSprite;
    }

    public override void CreateCustomButtons(HudManager hudManager)
    {
        base.CreateCustomButtons(hudManager);
        _placeTrapButton = new CustomButton(
            OnPlaceTrapButtonClick,
            IsLocalPlayerAndAlive,
            CouldUsePlaceTrapButton,
            ResetPlaceTrapButton,
            GetPlaceTrapSprite(),
            CustomButton.ButtonPositions.lowerRowRight,
            hudManager,
            "ActionQuaternary"
        );
        _placeTrapButtonText = UnityEngine.Object.Instantiate(_placeTrapButton.actionButton.cooldownTimerText,
            _placeTrapButton.actionButton.cooldownTimerText.transform.parent);
        _placeTrapButtonText.text = "";
        _placeTrapButtonText.enableWordWrapping = false;
        _placeTrapButtonText.transform.localScale = Vector3.one * 0.5f;
        _placeTrapButtonText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
    }

    private void ResetPlaceTrapButton()
    {
        if (_placeTrapButton == null) return;
        _placeTrapButton.Timer = _placeTrapButton.MaxTimer;
    }

    private bool CouldUsePlaceTrapButton()
    {
        if (_placeTrapButtonText != null)
        {
            _placeTrapButtonText.text = $"{Charges} / {(int)MaxCharges}";
        }

        return Charges > 0 && CachedPlayer.LocalPlayer.PlayerControl.CanMove;
    }

    private void OnPlaceTrapButtonClick()
    {
        if (_placeTrapButton == null) return;
        var pos = CachedPlayer.LocalPlayer.transform.position;
        PlaceTrap(CachedPlayer.LocalPlayer, $"{pos.x}|{pos.y}");
        SoundEffectsManager.play("trapperTrap");
        _placeTrapButton.Timer = _placeTrapButton.MaxTimer;
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        PlayersOnMap.Clear();
        Charges = 0;
    }

    [MethodRpc((uint)Rpc.Id.TrapperPlaceTrap)]
    private static void PlaceTrap(PlayerControl sender, string rawData)
    {
        if (Singleton<Trapper>.Instance.Player == null) return;
        var rawPos = rawData.Split("|");
        var position = Vector3.zero;
        position.x = float.Parse(rawPos[0]);
        position.y = float.Parse(rawPos[1]);
        var _ = new Trap(position);
    }
}