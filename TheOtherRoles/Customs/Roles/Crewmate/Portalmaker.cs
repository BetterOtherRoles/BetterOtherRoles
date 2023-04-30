using System;
using Reactor.Networking.Attributes;
using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.Objects;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;
using TMPro;
using UnityEngine;
using Resources = TheOtherRoles.EnoFramework.Utils.Resources;

namespace TheOtherRoles.Customs.Roles.Crewmate;

[EnoSingleton]
public class Portalmaker : CustomRole
{
    private Sprite? _placePortalSprite;
    private Sprite? _usePortalSprite;
    private Sprite? _usePortalSpecialFirstSprite;
    private Sprite? _usePortalSpecialSecondSprite;
    private Sprite? _portalLogSprite;

    private CustomButton? _placePortalButton;
    private CustomButton? _usePortalButton;
    private CustomButton? _usePortalSpecialButton;
    private TMP_Text? _portalSpecialFirstText;
    private TMP_Text? _portalSpecialSecondText;

    public readonly EnoFramework.CustomOption PortalCooldown;
    public readonly EnoFramework.CustomOption UsePortalCooldown;
    public readonly EnoFramework.CustomOption LogOnlyColorType;
    public readonly EnoFramework.CustomOption LogHasTime;
    public readonly EnoFramework.CustomOption CanPortalFromAnywhere;

    public Portalmaker() : base(nameof(Portalmaker))
    {
        Team = Teams.Crewmate;
        Color = new Color32(69, 69, 169, byte.MaxValue);

        PortalCooldown = OptionsTab.CreateFloatList(
            $"{Name}{nameof(PortalCooldown)}",
            Cs("Portal cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        UsePortalCooldown = OptionsTab.CreateFloatList(
            $"{Name}{nameof(UsePortalCooldown)}",
            Cs("Use portal cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        LogOnlyColorType = OptionsTab.CreateBool(
            $"{Name}{nameof(LogOnlyColorType)}",
            Cs("Log only display color type"),
            true,
            SpawnRate);
        LogHasTime = OptionsTab.CreateBool(
            $"{Name}{nameof(LogHasTime)}",
            Cs("Log display time"),
            true,
            SpawnRate);
        CanPortalFromAnywhere = OptionsTab.CreateBool(
            $"{Name}{nameof(CanPortalFromAnywhere)}",
            Cs("Can use portal from everywhere"),
            true,
            SpawnRate);
    }

    public Sprite GetPlacePortalSprite()
    {
        if (_placePortalSprite == null)
        {
            _placePortalSprite =
                Resources.LoadSpriteFromResources("TheOtherRoles.Resources.PlacePortalButton.png", 115f);
        }

        return _placePortalSprite;
    }

    public Sprite GetUsePortalSprite()
    {
        if (_usePortalSprite == null)
        {
            _usePortalSprite = Resources.LoadSpriteFromResources("TheOtherRoles.Resources.UsePortalButton.png", 115f);
        }

        return _usePortalSprite;
    }

    public Sprite UsePortalSpecialFirstSprite()
    {
        if (_usePortalSpecialFirstSprite == null)
        {
            _usePortalSpecialFirstSprite =
                Resources.LoadSpriteFromResources("TheOtherRoles.Resources.UsePortalSpecialButton1.png", 115f);
        }

        return _usePortalSpecialFirstSprite;
    }

    public Sprite UsePortalSpecialSecondSprite()
    {
        if (_usePortalSpecialSecondSprite == null)
        {
            _usePortalSpecialSecondSprite =
                Resources.LoadSpriteFromResources("TheOtherRoles.Resources.UsePortalSpecialButton2.png", 115f);
        }

        return _usePortalSpecialSecondSprite;
    }

    public Sprite GetLogSprite()
    {
        if (_portalLogSprite == null)
        {
            _portalLogSprite = FastDestroyableSingleton<HudManager>.Instance.UseButton
                .fastUseSettings[ImageNames.DoorLogsButton]
                .Image;
        }

        return _portalLogSprite;
    }

    public override void CreateCustomButtons(HudManager hudManager)
    {
        base.CreateCustomButtons(hudManager);
        _placePortalButton = new CustomButton(
            OnPlacePortalButtonClick,
            HasPlacePortalButton,
            CouldUsePlacePortalButton,
            ResetPlacePortalButton,
            GetPlacePortalSprite(),
            CustomButton.ButtonPositions.lowerRowRight,
            hudManager,
            "ActionQuaternary"
        );
        _usePortalButton = new CustomButton(
            OnUsePortalButtonClick,
            HasUsePortalButton,
            CouldUseUsePortalButton,
            ResetUsePortalButton,
            GetUsePortalSprite(),
            new Vector3(0.9f, -0.06f, 0),
            hudManager,
            "UsePortal",
            mirror: true
        );
        _usePortalSpecialButton = new CustomButton(
            OnUsePortalSpecialButtonClick,
            HasUsePortalSpecialButton,
            CouldUseUsePortalSpecialButton,
            ResetUsePortalSpecialButton,
            GetUsePortalSprite(),
            new Vector3(0.9f, 1f, 0),
            hudManager,
            "PortalMakerTeleportation",
            mirror: true
        );

        _portalSpecialFirstText = UnityEngine.Object.Instantiate(_usePortalButton.actionButton.cooldownTimerText,
            _usePortalButton.actionButton.cooldownTimerText.transform.parent);
        _portalSpecialFirstText.text = "";
        _portalSpecialFirstText.enableWordWrapping = false;
        _portalSpecialFirstText.transform.localScale = Vector3.one * 0.5f;
        _portalSpecialFirstText.transform.localPosition += new Vector3(-0.05f, 0.55f, -1f);

        _portalSpecialSecondText = UnityEngine.Object.Instantiate(
            _usePortalSpecialButton.actionButton.cooldownTimerText,
            _usePortalSpecialButton.actionButton.cooldownTimerText.transform.parent);
        _portalSpecialSecondText.text = "";
        _portalSpecialSecondText.enableWordWrapping = false;
        _portalSpecialSecondText.transform.localScale = Vector3.one * 0.5f;
        _portalSpecialSecondText.transform.localPosition += new Vector3(-0.05f, 0.55f, -1f);
    }

    private void ResetUsePortalSpecialButton()
    {
        if (_usePortalSpecialButton == null || _usePortalButton == null) return;
        _usePortalSpecialButton.Timer = _usePortalButton.MaxTimer;
    }

    private bool CouldUseUsePortalSpecialButton()
    {
        return CachedPlayer.LocalPlayer.PlayerControl.CanMove &&
               !Portal.locationNearEntry(CachedPlayer.LocalPlayer.transform.position) && !Portal.isTeleporting;
    }

    private bool HasUsePortalSpecialButton()
    {
        return CanPortalFromAnywhere && Portal.bothPlacedAndEnabled && IsLocalPlayer();
    }

    private void OnUsePortalSpecialButtonClick()
    {
        if (Portal.secondPortal == null || _usePortalButton == null || _usePortalSpecialButton == null) return;
        var didTeleport = false;
        var exit = Portal.secondPortal.portalGameObject.transform.position;

        if (!CachedPlayer.LocalPlayer.Data.IsDead)
        {
            // Ghosts can portal too, but non-blocking and only with a local animation
            RpcUsePortal(CachedPlayer.LocalPlayer, $"{CachedPlayer.LocalPlayer.PlayerId}|{(byte)2}");
        }
        else
        {
            UsePortal(CachedPlayer.LocalPlayer.PlayerId, 2);
        }

        _usePortalButton.Timer = _usePortalButton.MaxTimer;
        _usePortalSpecialButton.Timer = _usePortalButton.MaxTimer;
        SoundEffectsManager.play("portalUse");
        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Portal.teleportDuration,
            new Action<float>((p) =>
            {
                // Delayed action
                CachedPlayer.LocalPlayer.PlayerControl.moveable = false;
                CachedPlayer.LocalPlayer.NetTransform.Halt();
                switch (p)
                {
                    case >= 0.5f and <= 0.53f when !didTeleport && !MeetingHud.Instance:
                    {
                        if (SubmergedCompatibility.IsSubmerged)
                        {
                            SubmergedCompatibility.ChangeFloor(exit.y > -7);
                        }

                        CachedPlayer.LocalPlayer.NetTransform.RpcSnapTo(exit);
                        didTeleport = true;
                        break;
                    }
                    case 1f:
                        CachedPlayer.LocalPlayer.PlayerControl.moveable = true;
                        break;
                }
            })));
    }

    private void ResetUsePortalButton()
    {
        if (_usePortalButton == null) return;
        _usePortalButton.Timer = _usePortalButton.MaxTimer;
    }

    private bool CouldUseUsePortalButton()
    {
        return CachedPlayer.LocalPlayer.PlayerControl.CanMove &&
               (Portal.locationNearEntry(CachedPlayer.LocalPlayer.transform.position) ||
                CanPortalFromAnywhere && IsLocalPlayer()) && !Portal.isTeleporting;
    }

    private bool HasUsePortalButton()
    {
        if (IsLocalPlayer() && Portal.bothPlacedAndEnabled && _portalSpecialFirstText != null)
        {
            _portalSpecialFirstText.text =
                Portal.locationNearEntry(CachedPlayer.LocalPlayer.transform.position) || !CanPortalFromAnywhere
                    ? ""
                    : $"1. {Portal.firstPortal?.room}";
        }

        return Portal.bothPlacedAndEnabled;
    }

    private void OnUsePortalButtonClick()
    {
        if (Portal.firstPortal == null || Portal.secondPortal == null || _usePortalButton == null) return;
        var didTeleport = false;
        var position = CachedPlayer.LocalPlayer.transform.position;
        Vector3 exit = Portal.findExit(position);
        Vector3 entry = Portal.findEntry(position);

        var portalMakerSoloTeleport = !Portal.locationNearEntry(position);
        if (portalMakerSoloTeleport)
        {
            exit = Portal.firstPortal.portalGameObject.transform.position;
            entry = CachedPlayer.LocalPlayer.transform.position;
        }

        CachedPlayer.LocalPlayer.NetTransform.RpcSnapTo(entry);

        if (!CachedPlayer.LocalPlayer.Data.IsDead)
        {
            // Ghosts can portal too, but non-blocking and only with a local animation
            RpcUsePortal(CachedPlayer.LocalPlayer,
                $"{CachedPlayer.LocalPlayer.PlayerId}|{(portalMakerSoloTeleport ? (byte)1 : (byte)0)}");
        }
        else
        {
            UsePortal(CachedPlayer.LocalPlayer.PlayerId, portalMakerSoloTeleport ? (byte)1 : (byte)0);
        }

        _usePortalButton.Timer = _usePortalButton.MaxTimer;
        if (_usePortalSpecialButton != null)
        {
            _usePortalSpecialButton.Timer = _usePortalButton.MaxTimer;
        }

        SoundEffectsManager.play("portalUse");
        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Portal.teleportDuration,
            new Action<float>((p) =>
            {
                // Delayed action
                CachedPlayer.LocalPlayer.PlayerControl.moveable = false;
                CachedPlayer.LocalPlayer.NetTransform.Halt();
                switch (p)
                {
                    case >= 0.5f and <= 0.53f when !didTeleport && !MeetingHud.Instance:
                    {
                        if (SubmergedCompatibility.IsSubmerged)
                        {
                            SubmergedCompatibility.ChangeFloor(exit.y > -7);
                        }

                        CachedPlayer.LocalPlayer.NetTransform.RpcSnapTo(exit);
                        didTeleport = true;
                        break;
                    }
                    case 1f:
                        CachedPlayer.LocalPlayer.PlayerControl.moveable = true;
                        break;
                }
            })));
    }

    private void ResetPlacePortalButton()
    {
        if (_placePortalButton == null) return;
        _placePortalButton.Timer = _placePortalButton.MaxTimer;
    }

    private bool CouldUsePlacePortalButton()
    {
        return CachedPlayer.LocalPlayer.PlayerControl.CanMove && Portal.secondPortal == null;
    }

    private bool HasPlacePortalButton()
    {
        return IsLocalPlayerAndAlive() && Portal.secondPortal == null;
    }

    private void OnPlacePortalButtonClick()
    {
        if (_placePortalButton == null) return;
        _placePortalButton.Timer = _placePortalButton.MaxTimer;
        var pos = CachedPlayer.LocalPlayer.transform.position;
        PlacePortal(CachedPlayer.LocalPlayer, $"{pos.x}|{pos.y}");
        SoundEffectsManager.play("tricksterPlaceBox");
    }

    private static void UsePortal(byte playerId, byte exit)
    {
        Portal.startTeleport(playerId, exit);
    }

    [MethodRpc((uint)Rpc.Id.PortalMakerPlacePortal)]
    private static void PlacePortal(PlayerControl sender, string rawData)
    {
        var rawPos = rawData.Split("|");
        var position = Vector3.zero;
        position.x = float.Parse(rawPos[0]);
        position.y = float.Parse(rawPos[1]);
        var _ = new Portal(position);
    }

    [MethodRpc((uint)Rpc.Id.PortalMakerUsePortal)]
    private static void RpcUsePortal(PlayerControl sender, string rawData)
    {
        var data = rawData.Split("|");
        var playerId = byte.Parse(data[0]);
        var exit = byte.Parse(data[1]);
        UsePortal(playerId, exit);
    }
}