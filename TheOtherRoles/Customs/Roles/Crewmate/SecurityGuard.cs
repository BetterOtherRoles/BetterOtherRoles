using System.Linq;
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
public class SecurityGuard : CustomRole
{
    private Sprite? _closeVentSprite;
    private Sprite? _placeCameraSprite;
    private Sprite? _animatedVentSealedSprite;
    private Sprite? _staticVentSealedSprite;
    private Sprite? _submergedCentralUpperVentSealedSprite;
    private Sprite? _submergedCentralLowerVentSealedSprite;
    private Sprite? _camSprite;
    private Sprite? _logSprite;

    private CustomButton? _screwButton;
    private TMP_Text? _screwText;
    private CustomButton? _cameraButton;
    private TMP_Text? _cameraText;

    public readonly EnoFramework.CustomOption Cooldown;
    public readonly EnoFramework.CustomOption TotalScrews;
    public readonly EnoFramework.CustomOption CamPrice;
    public readonly EnoFramework.CustomOption VentPrice;
    public readonly EnoFramework.CustomOption CamDuration;
    public readonly EnoFramework.CustomOption CamMaxCharges;
    public readonly EnoFramework.CustomOption CamRechargeTaskNumber;
    public readonly EnoFramework.CustomOption CanMoveDuringCam;

    public int UsedScrews;
    public int UsedCharges;
    public Vent? VentTarget;
    public Minigame? Minigame;
    public int PlacedCameras;
    private float _lastPpu;

    public int RemainingScrews => TotalScrews - UsedScrews;
    public int RemainingCharges => CamMaxCharges - UsedCharges;

    public SecurityGuard() : base(nameof(SecurityGuard))
    {
        Team = Teams.Crewmate;
        Color = new Color32(195, 178, 95, byte.MaxValue);
        DisplayName = "Security guard";

        Cooldown = OptionsTab.CreateFloatList(
            $"{Name}{nameof(Cooldown)}",
            Cs("Cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        TotalScrews = OptionsTab.CreateFloatList(
            $"{Name}{nameof(TotalScrews)}",
            Cs("Number of screws"),
            1f,
            15f,
            7f,
            1f,
            SpawnRate);
        CamPrice = OptionsTab.CreateFloatList(
            $"{Name}{nameof(CamPrice)}",
            Cs("Number of screws per camera"),
            1f,
            15f,
            2f,
            1f,
            SpawnRate);
        VentPrice = OptionsTab.CreateFloatList(
            $"{Name}{nameof(VentPrice)}",
            Cs("Number of screws per vent"),
            1f,
            15f,
            2f,
            1f,
            SpawnRate);
        CamDuration = OptionsTab.CreateFloatList(
            $"{Name}{nameof(CamDuration)}",
            Cs("Portable camera duration"),
            2.5f,
            60f,
            10f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        CamMaxCharges = OptionsTab.CreateFloatList(
            $"{Name}{nameof(CamMaxCharges)}",
            Cs("Portable camera max charges"),
            1f,
            30f,
            5f,
            1f,
            SpawnRate);
        CamRechargeTaskNumber = OptionsTab.CreateFloatList(
            $"{Name}{nameof(CamRechargeTaskNumber)}",
            Cs("Number of tasks needed for recharging"),
            1f,
            10f,
            3f,
            1f,
            SpawnRate);
        CanMoveDuringCam = OptionsTab.CreateBool(
            $"{Name}{nameof(CanMoveDuringCam)}",
            Cs("Can move during using cameras"),
            false);
    }

    public Sprite GetCloseVentSprite()
    {
        if (_closeVentSprite == null)
        {
            _closeVentSprite = Resources.LoadSpriteFromResources("TheOtherRoles.Resources.CloseVentButton.png", 115f);
        }

        return _closeVentSprite;
    }

    public Sprite GetPlaceCameraSprite()
    {
        if (_placeCameraSprite == null)
        {
            _placeCameraSprite =
                Resources.LoadSpriteFromResources("TheOtherRoles.Resources.PlaceCameraButton.png", 115f);
        }

        return _placeCameraSprite;
    }

    public Sprite GetAnimatedVentSealedSprite()
    {
        var ppu = 185f;
        if (SubmergedCompatibility.IsSubmerged) ppu = 120f;
        if (_lastPpu != ppu)
        {
            _animatedVentSealedSprite = null;
            _lastPpu = ppu;
        }

        if (_animatedVentSealedSprite == null)
        {
            _animatedVentSealedSprite =
                Resources.LoadSpriteFromResources("TheOtherRoles.Resources.AnimatedVentSealed.png", ppu);
        }

        return _animatedVentSealedSprite;
    }

    public Sprite GetStaticVentSealedSprite()
    {
        if (_staticVentSealedSprite == null)
        {
            _staticVentSealedSprite =
                Resources.LoadSpriteFromResources("TheOtherRoles.Resources.StaticVentSealed.png", 160f);
        }

        return _staticVentSealedSprite;
    }

    public Sprite GetSubmergedCentralUpperSealedSprite()
    {
        if (_submergedCentralUpperVentSealedSprite == null)
        {
            _submergedCentralUpperVentSealedSprite =
                Resources.LoadSpriteFromResources("TheOtherRoles.Resources.CentralUpperBlocked.png", 145f);
        }

        return _submergedCentralUpperVentSealedSprite;
    }

    public Sprite GetSubmergedCentralLowerSealedSprite()
    {
        if (_submergedCentralLowerVentSealedSprite == null)
        {
            _submergedCentralLowerVentSealedSprite =
                Resources.LoadSpriteFromResources("TheOtherRoles.Resources.CentralLowerBlocked.png", 145f);
        }

        return _submergedCentralLowerVentSealedSprite;
    }

    public Sprite GetCamSprite()
    {
        if (_camSprite == null)
        {
            _camSprite = FastDestroyableSingleton<HudManager>.Instance.UseButton
                .fastUseSettings[ImageNames.CamsButton]
                .Image;
        }

        return _camSprite;
    }

    public Sprite GetLogSprite()
    {
        if (_logSprite == null)
        {
            _logSprite = FastDestroyableSingleton<HudManager>.Instance.UseButton
                .fastUseSettings[ImageNames.DoorLogsButton]
                .Image;
        }

        return _logSprite;
    }

    public override void CreateCustomButtons(HudManager hudManager)
    {
        base.CreateCustomButtons(hudManager);
        _screwButton = new CustomButton(
            OnScrewButtonClick,
            HasScrewButton,
            CouldUseScrewButton,
            ResetScrewButton,
            GetPlaceCameraSprite(),
            CustomButton.ButtonPositions.lowerRowRight,
            hudManager,
            "ActionQuaternary"
        );
        _screwText = UnityEngine.Object.Instantiate(_screwButton.actionButton.cooldownTimerText,
            _screwButton.actionButton.cooldownTimerText.transform.parent);
        _screwText.text = "";
        _screwText.enableWordWrapping = false;
        _screwText.transform.localScale = Vector3.one * 0.5f;
        _screwText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

        _cameraButton = new CustomButton(
            OnCameraButtonClick,
            HasCameraButton,
            CouldUseCameraButton,
            ResetCameraButton,
            GetCamSprite(),
            CustomButton.ButtonPositions.lowerRowRight,
            hudManager,
            "ActionSecondary",
            true,
            0f,
            OnCameraButtonEffectEnd,
            false,
            GameOptionsManager.Instance.currentNormalGameOptions.MapId == 1 ? "DOORLOG" : "SECURITY"
        );

        _cameraText = UnityEngine.Object.Instantiate(_cameraButton.actionButton.cooldownTimerText,
            _cameraButton.actionButton.cooldownTimerText.transform.parent);
        _cameraText.text = "";
        _cameraText.enableWordWrapping = false;
        _cameraText.transform.localScale = Vector3.one * 0.5f;
        _cameraText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
    }

    private void OnCameraButtonEffectEnd()
    {
        if (_cameraButton == null) return;
        _cameraButton.Timer = _cameraButton.MaxTimer;
        if (Minigame.Instance && Minigame != null)
        {
            Minigame.ForceClose();
        }

        CachedPlayer.LocalPlayer.PlayerControl.moveable = true;
    }

    private void ResetCameraButton()
    {
        if (_cameraButton == null) return;
        _cameraButton.Timer = _cameraButton.MaxTimer;
        _cameraButton.isEffectActive = false;
        _cameraButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
    }

    private bool CouldUseCameraButton()
    {
        if (_cameraButton == null) return false;
        if (_cameraText != null)
        {
            _cameraText.text = $"{RemainingCharges} / {(int)CamMaxCharges}";
        }

        _cameraButton.actionButton.graphic.sprite =
            GameOptionsManager.Instance.currentNormalGameOptions.MapId == 1
                ? GetLogSprite()
                : GetCamSprite();
        _cameraButton.actionButton.OverrideText(
            GameOptionsManager.Instance.currentNormalGameOptions.MapId == 1 ? "DOORLOG" : "SECURITY");
        return CachedPlayer.LocalPlayer.PlayerControl.CanMove && RemainingCharges > 0;
    }

    private bool HasCameraButton()
    {
        return IsLocalPlayerAndAlive() && RemainingScrews <
                                       Mathf.Min(VentPrice, CamPrice)
                                       && !SubmergedCompatibility.IsSubmerged;
    }

    private void OnCameraButtonClick()
    {
        if (Camera.main == null) return;
        if (GameOptionsManager.Instance.currentNormalGameOptions.MapId != 1)
        {
            if (Minigame == null)
            {
                var mapId = GameOptionsManager.Instance.currentNormalGameOptions.MapId;
                var e = UnityEngine.Object.FindObjectsOfType<SystemConsole>()
                    .FirstOrDefault(x => x.gameObject.name.Contains("Surv_Panel"));
                switch (mapId)
                {
                    case 0 or 3:
                        e = UnityEngine.Object.FindObjectsOfType<SystemConsole>()
                            .FirstOrDefault(x => x.gameObject.name.Contains("SurvConsole"));
                        break;
                    case 4:
                        e = UnityEngine.Object.FindObjectsOfType<SystemConsole>()
                            .FirstOrDefault(x => x.gameObject.name.Contains("task_cams"));
                        break;
                }

                if (e == null) return;
                Minigame = UnityEngine.Object.Instantiate(e.MinigamePrefab, Camera.main.transform, false);
            }

            var transform = Minigame.transform;
            transform.SetParent(Camera.main.transform, false);
            transform.localPosition = new Vector3(0.0f, 0.0f, -50f);
            Minigame.Begin(null);
        }
        else
        {
            if (Minigame == null)
            {
                var e = UnityEngine.Object.FindObjectsOfType<SystemConsole>()
                    .FirstOrDefault(x => x.gameObject.name.Contains("SurvLogConsole"));
                if (e == null) return;
                Minigame = UnityEngine.Object.Instantiate(e.MinigamePrefab, Camera.main.transform, false);
            }

            var transform = Minigame.transform;
            transform.SetParent(Camera.main.transform, false);
            transform.localPosition = new Vector3(0.0f, 0.0f, -50f);
            Minigame.Begin(null);
        }

        UsedCharges++;

        if (!CanMoveDuringCam) CachedPlayer.LocalPlayer.PlayerControl.moveable = false;
        CachedPlayer.LocalPlayer.NetTransform.Halt(); // Stop current movement 
    }

    private void ResetScrewButton()
    {
        if (_screwButton == null) return;
        _screwButton.Timer = _screwButton.MaxTimer;
    }

    private bool CouldUseScrewButton()
    {
        if (_screwButton == null) return false;
        _screwButton.actionButton.graphic.sprite =
            (VentTarget == null &&
             GameOptionsManager.Instance.currentNormalGameOptions.MapId != 1 &&
             !SubmergedCompatibility.IsSubmerged)
                ? GetPlaceCameraSprite()
                : GetCloseVentSprite();
        if (_screwText != null)
        {
            _screwText.text = $"{RemainingScrews}/{(int)TotalScrews}";
        }

        if (!CachedPlayer.LocalPlayer.PlayerControl.CanMove) return false;

        if (VentTarget != null)
        {
            return RemainingScrews >= VentPrice;
        }

        return GameOptionsManager.Instance.currentNormalGameOptions.MapId != 1 &&
               !SubmergedCompatibility.IsSubmerged && RemainingScrews >= CamPrice &&
               CachedPlayer.LocalPlayer.PlayerControl.CanMove;
    }

    private bool HasScrewButton()
    {
        return IsLocalPlayerAndAlive() && RemainingScrews >= Mathf.Min(VentPrice, CamPrice);
    }

    private void OnScrewButtonClick()
    {
        if (_screwButton == null) return;
        if (VentTarget != null)
        {
            SealVent(CachedPlayer.LocalPlayer, $"{VentTarget.Id}");
            VentTarget = null;
        }
        else if (GameOptionsManager.Instance.currentNormalGameOptions.MapId != 1 &&
                 !SubmergedCompatibility.IsSubmerged)
        {
            // Place camera if there's no vent and it's not MiraHQ or Submerged
            var pos = CachedPlayer.LocalPlayer.transform.position;
            PlaceCamera(CachedPlayer.LocalPlayer, $"{pos.x}|{pos.y}");
        }

        SoundEffectsManager.play("securityGuardPlaceCam"); // Same sound used for both types (cam or vent)!
        _screwButton.Timer = _screwButton.MaxTimer;
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        UsedScrews = 0;
        UsedCharges = 0;
        VentTarget = null;
        Minigame = null;
        PlacedCameras = 0;
    }

    [MethodRpc((uint)Rpc.Id.SecurityGuardPlaceCamera)]
    private static void PlaceCamera(PlayerControl sender, string rawData)
    {
        var rawPos = rawData.Split("|");
        var position = Vector3.zero;
        position.x = float.Parse(rawPos[0]);
        position.y = float.Parse(rawPos[1]);

        var referenceCamera = UnityEngine.Object.FindObjectOfType<SurvCamera>();
        if (referenceCamera == null) return; // Mira HQ
        Singleton<SecurityGuard>.Instance.UsedScrews += Singleton<SecurityGuard>.Instance.CamPrice;
        Singleton<SecurityGuard>.Instance.PlacedCameras++;

        var camera = UnityEngine.Object.Instantiate(referenceCamera);
        camera.transform.position = new Vector3(position.x, position.y, referenceCamera.transform.position.z - 1f);
        camera.CamName = $"Security Camera {Singleton<SecurityGuard>.Instance.PlacedCameras}";
        camera.Offset = new Vector3(0f, 0f, camera.Offset.z);
        if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 2 ||
            GameOptionsManager.Instance.currentNormalGameOptions.MapId == 4)
        {
            camera.transform.localRotation = new Quaternion(0, 0, 1, 1); // Polus and Airship 
        }

        if (SubmergedCompatibility.IsSubmerged)
        {
            // remove 2d box collider of console, so that no barrier can be created. (irrelevant for now, but who knows... maybe we need it later)
            var fixConsole = camera.transform.FindChild("FixConsole");
            if (fixConsole != null)
            {
                var boxCollider = fixConsole.GetComponent<BoxCollider2D>();
                if (boxCollider != null) UnityEngine.Object.Destroy(boxCollider);
            }
        }

        if (Singleton<SecurityGuard>.Instance.IsLocalPlayer())
        {
            camera.gameObject.SetActive(true);
            camera.gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f);
        }
        else
        {
            camera.gameObject.SetActive(false);
        }

        TORMapOptions.camerasToAdd.Add(camera);
    }

    [MethodRpc((uint)Rpc.Id.SecurityGuardSealVent)]
    private static void SealVent(PlayerControl sender, string rawData)
    {
        var ventId = int.Parse(rawData);
        var vent = MapUtilities.CachedShipStatus.AllVents.FirstOrDefault(v => v != null && v.Id == ventId);
        if (vent == null) return;
        Singleton<SecurityGuard>.Instance.UsedScrews += Singleton<SecurityGuard>.Instance.VentPrice;
        if (Singleton<SecurityGuard>.Instance.IsLocalPlayer())
        {
            var animator = vent.GetComponent<PowerTools.SpriteAnim>();
            if (animator != null) animator.Stop();
            vent.EnterVentAnim = vent.ExitVentAnim = null;
            vent.myRend.sprite = animator == null
                ? Singleton<SecurityGuard>.Instance.GetStaticVentSealedSprite()
                : Singleton<SecurityGuard>.Instance.GetAnimatedVentSealedSprite();
            if (SubmergedCompatibility.IsSubmerged)
            {
                vent.myRend.sprite = vent.Id switch
                {
                    0 => Singleton<SecurityGuard>.Instance.GetSubmergedCentralUpperSealedSprite(),
                    14 => Singleton<SecurityGuard>.Instance.GetSubmergedCentralLowerSealedSprite(),
                    _ => vent.myRend.sprite
                };
            }

            vent.myRend.color = new Color(1f, 1f, 1f, 0.5f);
            vent.name = "FutureSealedVent_" + vent.name;
        }

        TORMapOptions.ventsToSeal.Add(vent);
    }
}