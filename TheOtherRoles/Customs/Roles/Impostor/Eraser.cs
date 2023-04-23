using System.Collections.Generic;
using Hazel;
using Reactor.Networking.Attributes;
using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.EnoFramework.Utils;
using TheOtherRoles.Objects;
using TheOtherRoles.Players;
using UnityEngine;
using Resources = TheOtherRoles.EnoFramework.Utils.Resources;

namespace TheOtherRoles.Customs.Roles.Impostor;

[EnoSingleton]
public class Eraser : CustomRole
{
    private Sprite? _eraseSprite;

    private CustomButton? _eraseButton;

    public List<byte> AlreadyErased = new();
    public List<PlayerControl> FutureErased = new();

    public readonly EnoFramework.CustomOption EraseCooldown;
    public readonly EnoFramework.CustomOption CanEraseAnyone;

    public static float cooldown = 30f;
    public static bool canEraseAnyone = false;

    public Eraser() : base(nameof(Eraser))
    {
        Team = Teams.Impostor;
        Color = Palette.ImpostorRed;
        CanTarget = true;
        
        EraseCooldown = OptionsTab.CreateFloatList(
            nameof(EraseCooldown),
            Colors.Cs(Color, $"{Name} cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        CanEraseAnyone = OptionsTab.CreateBool(
            $"{Name}{nameof(CanEraseAnyone)}",
            Colors.Cs(Color, "Can erase anyone"),
            false,
            SpawnRate);
    }

    public Sprite GetEraseSprite()
    {
        if (_eraseSprite == null)
        {
            _eraseSprite = Resources.LoadSpriteFromResources("TheOtherRoles.Resources.EraserButton.png", 115f);
        }

        return _eraseSprite;
    }

    public override void CreateCustomButtons(HudManager hudManager)
    {
        base.CreateCustomButtons(hudManager);
        _eraseButton = new CustomButton(
            OnEraseButtonClick,
            HasEraseButton,
            CouldUseEraseButton,
            ResetEraseButton,
            GetEraseSprite(),
            CustomButton.ButtonPositions.upperRowLeft,
            hudManager,
            "ActionQuaternary"
        );
    }

    private void ResetEraseButton()
    {
        if (_eraseButton == null) return;
        _eraseButton.Timer = _eraseButton.MaxTimer;
    }

    private bool CouldUseEraseButton()
    {
        return CachedPlayer.LocalPlayer.PlayerControl.CanMove && CurrentTarget != null;
    }

    private bool HasEraseButton()
    {
        return Player != null && Player == CachedPlayer.LocalPlayer.PlayerControl && !CachedPlayer.LocalPlayer.Data.IsDead;
    }

    private void OnEraseButtonClick()
    {
        if (_eraseButton == null || CurrentTarget == null) return;
        _eraseButton.MaxTimer += 10;
        _eraseButton.Timer = _eraseButton.MaxTimer;
        SetFutureErased(CachedPlayer.LocalPlayer, $"{CurrentTarget.PlayerId}");
        SoundEffectsManager.play("eraserErase");
    }
    
    public override void ClearAndReload()
    {
        base.ClearAndReload();
        FutureErased.Clear();
        AlreadyErased.Clear();
    }

    [MethodRpc((uint) Rpc.Id.EraserSetFutureErased)]
    private static void SetFutureErased(PlayerControl sender, string rawData)
    {
        var playerId = byte.Parse(rawData);
        var player = Helpers.playerById(playerId);
        Singleton<Eraser>.Instance.FutureErased.Add(player);
    }
}