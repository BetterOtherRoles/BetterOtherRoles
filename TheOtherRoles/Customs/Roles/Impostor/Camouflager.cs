using Reactor.Networking.Attributes;
using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.EnoFramework.Utils;
using TheOtherRoles.Objects;
using TheOtherRoles.Players;
using UnityEngine;
using Resources = TheOtherRoles.EnoFramework.Utils.Resources;

namespace TheOtherRoles.Customs.Roles.Impostor;

[EnoSingleton]
public class Camouflager : CustomRole
{
    private Sprite? _camoSprite;

    private CustomButton? _camoButton;

    public readonly EnoFramework.CustomOption CamoCooldown;
    public readonly EnoFramework.CustomOption CamoDuration;

    public float CamouflagerTimer;


    public Camouflager() : base(nameof(Camouflager))
    {
        Team = Teams.Impostor;
        Color = Palette.ImpostorRed;
        CanTarget = true;

        CamoCooldown = OptionsTab.CreateFloatList(
            nameof(CamoCooldown),
            Colors.Cs(Color, $"{Name} cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        CamoDuration = OptionsTab.CreateFloatList(
            nameof(CamoDuration),
            Colors.Cs(Color, $"{Name} duration"),
            5f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
    }

    private Sprite GetCamoSprite()
    {
        if (_camoSprite == null)
        {
            _camoSprite = Resources.LoadSpriteFromResources("TheOtherRoles.Resources.CamoButton.png", 115f);
        }

        return _camoSprite;
    }

    public void ResetCamouflage()
    {
        CamouflagerTimer = 0f;
        foreach (PlayerControl p in CachedPlayer.AllPlayers)
        {
            if (p == Singleton<Ninja>.Instance.Player && Singleton<Ninja>.Instance.IsInvisible) continue;
            p.setDefaultLook();
        }
    }

    public override void ClearAndReload()
    {
        ResetCamouflage();
        base.ClearAndReload();
    }

    public override void CreateCustomButtons(HudManager hudManager)
    {
        base.CreateCustomButtons(hudManager);
        _camoButton = new CustomButton(
            OnCamouflagerButtonClick,
            HasCamouflagerButton,
            CouldUseCamouflagerButton,
            ResetCamouflagerButton,
            GetCamoSprite(),
            CustomButton.ButtonPositions.upperRowLeft,
            hudManager,
            "ActionQuaternary",
            true,
            CamoDuration,
            OnCamouflagerButtonEffectEnd
        );
    }

    private void OnCamouflagerButtonEffectEnd()
    {
        if (_camoButton == null) return;
        _camoButton.Timer = _camoButton.MaxTimer;
        SoundEffectsManager.play("morphlingMorph");
    }

    private void ResetCamouflagerButton()
    {
        if (_camoButton == null) return;
        _camoButton.Timer = _camoButton.MaxTimer;
        _camoButton.isEffectActive = false;
        _camoButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
    }

    private bool CouldUseCamouflagerButton()
    {
        return CachedPlayer.LocalPlayer.PlayerControl.CanMove;
    }

    private bool HasCamouflagerButton()
    {
        return Player != null && Player == CachedPlayer.LocalPlayer.PlayerControl &&
               !CachedPlayer.LocalPlayer.Data.IsDead;
    }

    private void OnCamouflagerButtonClick()
    {
        if (_camoButton == null || Player == null || !Is(CachedPlayer.LocalPlayer)) return;
        CamouflagerCamouflage(CachedPlayer.LocalPlayer);
        SoundEffectsManager.play("morphlingMorph");
    }

    [MethodRpc((uint)Rpc.Id.CamouflagerCamouflage)]
    private static void CamouflagerCamouflage(PlayerControl sender)
    {
        if (Singleton<Camouflager>.Instance.Player == null) return;
        Singleton<Camouflager>.Instance.CamouflagerTimer = Singleton<Camouflager>.Instance.CamoDuration;
        foreach (PlayerControl player in CachedPlayer.AllPlayers)
        {
            player.setLook("", 6, "", "", "", "");
        }
    }
}