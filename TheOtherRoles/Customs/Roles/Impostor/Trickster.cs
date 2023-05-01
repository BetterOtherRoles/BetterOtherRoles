using Reactor.Networking.Attributes;
using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.EnoFramework.Utils;
using TheOtherRoles.Objects;
using TheOtherRoles.Players;
using UnityEngine;
using Resources = TheOtherRoles.EnoFramework.Utils.Resources;

namespace TheOtherRoles.Customs.Roles.Impostor;

[EnoSingleton]
public class Trickster : CustomRole
{
    private Sprite? _placeBoxSprite;
    private Sprite? _lightOutSprite;
    private Sprite? _boxVentSprite;

    private CustomButton? _placeBoxButton;
    private CustomButton? _lightOutButton;

    public readonly EnoFramework.CustomOption PlaceBoxCooldown;
    public readonly EnoFramework.CustomOption LightsOutCooldown;
    public readonly EnoFramework.CustomOption LightsOutDuration;

    public float LightsOutTimer;

    public Trickster() : base(nameof(Trickster))
    {
        Team = Teams.Impostor;
        Color = Palette.ImpostorRed;
        CanTarget = true;

        IntroDescription = "Use your jack-in-the-boxes to surprise others";
        ShortDescription = "Surprise your enemies";

        PlaceBoxCooldown = OptionsTab.CreateFloatList(
            $"{Name}{nameof(PlaceBoxCooldown)}",
            Cs("Place box cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        LightsOutCooldown = OptionsTab.CreateFloatList(
            $"{Name}{nameof(LightsOutCooldown)}",
            Cs("Lights out cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        LightsOutDuration = OptionsTab.CreateFloatList(
            $"{Name}{nameof(LightsOutDuration)}",
            Cs("Lights out duration"),
            5f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
    }

    public Sprite GetPlaceBoxSprite()
    {
        if (_placeBoxSprite == null)
        {
            _placeBoxSprite =
                Resources.LoadSpriteFromResources("TheOtherRoles.Resources.PlaceJackInTheBoxButton.png", 115f);
        }

        return _placeBoxSprite;
    }

    public Sprite GetLightOutSprite()
    {
        if (_lightOutSprite == null)
        {
            _lightOutSprite =
                Resources.LoadSpriteFromResources("TheOtherRoles.Resources.LightsOutButton.png", 115f);
        }

        return _lightOutSprite;
    }

    public Sprite GetBoxVentSprite()
    {
        if (_boxVentSprite == null)
        {
            _boxVentSprite =
                Resources.LoadSpriteFromResources("TheOtherRoles.Resources.TricksterVentButton.png", 115f);
        }

        return _boxVentSprite;
    }

    public override void CreateCustomButtons(HudManager hudManager)
    {
        base.CreateCustomButtons(hudManager);
        _placeBoxButton = new CustomButton(
            OnPlaceBoxButtonClick,
            HasPlaceBoxButton,
            CouldUsePlaceBoxButton,
            ResetPlaceBoxButton,
            GetPlaceBoxSprite(),
            CustomButton.ButtonPositions.upperRowLeft,
            hudManager,
            "ActionQuaternary"
        );
        _lightOutButton = new CustomButton(
            OnLightOutButtonClick,
            HasLightOutButton,
            CouldUseLightOutButton,
            ResetLightOutButton,
            GetLightOutSprite(),
            CustomButton.ButtonPositions.upperRowLeft,
            hudManager,
            "ActionQuaternary",
            true,
            LightsOutDuration,
            OnLightOutButtonEffectEnd
        );
    }

    private void OnLightOutButtonEffectEnd()
    {
        if (_lightOutButton == null) return;
        _lightOutButton.Timer = _lightOutButton.MaxTimer;
        SoundEffectsManager.play("lighterLight");
    }

    private void ResetLightOutButton()
    {
        if (_lightOutButton == null) return;
        _lightOutButton.Timer = _lightOutButton.MaxTimer;
        _lightOutButton.isEffectActive = false;
        _lightOutButton.actionButton.graphic.color = Palette.EnabledColor;
    }

    private bool CouldUseLightOutButton()
    {
        return CachedPlayer.LocalPlayer.PlayerControl.CanMove && JackInTheBox.hasJackInTheBoxLimitReached() &&
               JackInTheBox.boxesConvertedToVents;
    }

    private bool HasLightOutButton()
    {
        return Player != null && Player == CachedPlayer.LocalPlayer.PlayerControl &&
               !CachedPlayer.LocalPlayer.Data.IsDead && JackInTheBox.hasJackInTheBoxLimitReached() &&
               JackInTheBox.boxesConvertedToVents;
    }

    private void OnLightOutButtonClick()
    {
        LightsOut(CachedPlayer.LocalPlayer);
        SoundEffectsManager.play("lighterLight");
    }

    private void ResetPlaceBoxButton()
    {
        if (_placeBoxButton == null) return;
        _placeBoxButton.Timer = _placeBoxButton.MaxTimer;
    }

    private bool CouldUsePlaceBoxButton()
    {
        return CachedPlayer.LocalPlayer.PlayerControl.CanMove && !JackInTheBox.hasJackInTheBoxLimitReached();
    }

    private bool HasPlaceBoxButton()
    {
        return Player != null && Player == CachedPlayer.LocalPlayer.PlayerControl &&
               !CachedPlayer.LocalPlayer.Data.IsDead && !JackInTheBox.hasJackInTheBoxLimitReached();
    }

    private void OnPlaceBoxButtonClick()
    {
        if (_placeBoxButton == null) return;
        _placeBoxButton.Timer = _placeBoxButton.MaxTimer;
        var pos = CachedPlayer.LocalPlayer.transform.position;
        PlaceBox(CachedPlayer.LocalPlayer, $"{pos.x}|{pos.y}");
        SoundEffectsManager.play("tricksterPlaceBox");
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        LightsOutTimer = 0f;
        JackInTheBox.UpdateStates();
    }
    
    [MethodRpc((uint)Rpc.Id.TricksterPlaceBox)]
    private static void PlaceBox(PlayerControl sender, string rawData)
    {
        var rawPos = rawData.Split("|");
        var position = Vector3.zero;
        position.x = float.Parse(rawPos[0]);
        position.y = float.Parse(rawPos[1]);
        var _ = new JackInTheBox(position);
    }
    
    [MethodRpc((uint) Rpc.Id.TricksterLightsOut)]
    private static void LightsOut(PlayerControl sender)
    {
        Singleton<Trickster>.Instance.LightsOutTimer = Singleton<Trickster>.Instance.LightsOutDuration;
        if (Helpers.hasImpVision(GameData.Instance.GetPlayerById(CachedPlayer.LocalPlayer.PlayerId)))
        {
            var _ = new CustomMessage("Lights are out", Singleton<Trickster>.Instance.LightsOutDuration);
        }
    }
}