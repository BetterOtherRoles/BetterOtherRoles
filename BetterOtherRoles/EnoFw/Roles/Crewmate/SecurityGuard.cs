using System;
using System.Linq;
using BetterOtherRoles.EnoFw.Kernel;
using BetterOtherRoles.Players;
using BetterOtherRoles.Utilities;
using PowerTools;
using UnityEngine;
using Object = UnityEngine.Object;
using Option = BetterOtherRoles.EnoFw.Kernel.CustomOption;

namespace BetterOtherRoles.EnoFw.Roles.Crewmate;

public class SecurityGuard : AbstractRole
{
    public static readonly SecurityGuard Instance = new();
    
    // Fields
    public int UsedScrews;
    public int UsedCharges;
    public int RechargedTasks;
    public Vent VentTarget;
    public Minigame Minigame;
    public int PlacedCameras;

    public int RemainingScrews => TotalScrews - UsedScrews;
    public int RemainingCharges => CamMaxCharges - UsedCharges;
    
    // Options
    public readonly CustomOption Cooldown;
    public readonly CustomOption TotalScrews;
    public readonly CustomOption CamPrice;
    public readonly CustomOption VentPrice;
    public readonly CustomOption CamDuration;
    public readonly CustomOption CamMaxCharges;
    public readonly CustomOption CamRechargeTaskNumber;
    public readonly CustomOption CanMoveDuringCam;

    public static Sprite CloseVentButtonSprite => GetSprite("BetterOtherRoles.Resources.CloseVentButton.png", 115f);
    public static Sprite PlaceCameraButtonSprite => GetSprite("BetterOtherRoles.Resources.PlaceCameraButton.png", 115f);

    private static float _lastPpu;
    private static Sprite _animatedVentSealedSprite;
    public static Sprite AnimatedVentSealedSprite
    {
        get
        {
            var ppu = 185f;
            if (SubmergedCompatibility.IsSubmerged) ppu = 120f;
            if (_lastPpu != ppu)
            {
                _animatedVentSealedSprite = null;
                _lastPpu = ppu;
            }

            if (_animatedVentSealedSprite != null) return _animatedVentSealedSprite;
            _animatedVentSealedSprite = GetSprite("BetterOtherRoles.Resources.AnimatedVentSealed.png", ppu);
            return _animatedVentSealedSprite;
        }
    }

    public static Sprite StaticVentSealedSprite => GetSprite("BetterOtherRoles.Resources.StaticVentSealed.png", 160f);

    public static Sprite SubmergedCentralUpperSealedSprite =>
        GetSprite("BetterOtherRoles.Resources.CentralUpperBlocked.png", 145f);

    public static Sprite SubmergedCentralLowerSealedSprite =>
        GetSprite("BetterOtherRoles.Resources.CentralLowerBlocked.png", 145f);

    private static Sprite _cameraButtonSprite;
    public static Sprite CameraButtonSprite
    {
        get
        {
            if (_cameraButtonSprite != null) return _cameraButtonSprite;
            _cameraButtonSprite = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.CamsButton].Image;
            return _cameraButtonSprite;
        }
    }

    private static Sprite _doorLogButtonSprite;
    public static Sprite DoorLogButtonSprite
    {
        get
        {
            if (_doorLogButtonSprite != null) return _doorLogButtonSprite;
            _doorLogButtonSprite = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.DoorLogsButton].Image;
            return _doorLogButtonSprite;
        }
    }


    private SecurityGuard() : base(nameof(SecurityGuard), "Security guard")
    {
        Team = Teams.Crewmate;
        Color = new Color32(195, 178, 95, byte.MaxValue);
        
        SpawnRate = GetDefaultSpawnRateOption();
        
        Cooldown = Tab.CreateFloatList(
            $"{Name}{nameof(Cooldown)}",
            Cs("Cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        TotalScrews = Tab.CreateFloatList(
            $"{Name}{nameof(TotalScrews)}",
            Cs("Number of screws"),
            1f,
            15f,
            7f,
            1f,
            SpawnRate);
        CamPrice = Tab.CreateFloatList(
            $"{Name}{nameof(CamPrice)}",
            Cs("Number of screws per camera"),
            1f,
            15f,
            2f,
            1f,
            SpawnRate);
        VentPrice = Tab.CreateFloatList(
            $"{Name}{nameof(VentPrice)}",
            Cs("Number of screws per vent"),
            1f,
            15f,
            2f,
            1f,
            SpawnRate);
        CamDuration = Tab.CreateFloatList(
            $"{Name}{nameof(CamDuration)}",
            Cs("Portable camera duration"),
            2.5f,
            60f,
            10f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        CamMaxCharges = Tab.CreateFloatList(
            $"{Name}{nameof(CamMaxCharges)}",
            Cs("Portable camera max charges"),
            1f,
            30f,
            5f,
            1f,
            SpawnRate);
        CamRechargeTaskNumber = Tab.CreateFloatList(
            $"{Name}{nameof(CamRechargeTaskNumber)}",
            Cs("Number of tasks needed for recharging"),
            1f,
            10f,
            3f,
            1f,
            SpawnRate);
        CanMoveDuringCam = Tab.CreateBool(
            $"{Name}{nameof(CanMoveDuringCam)}",
            Cs("Can move while using cameras"),
            false,
            SpawnRate);
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        VentTarget = null;
        Minigame = null;
        UsedCharges = 0;
        UsedScrews = 0;
        PlacedCameras = 0;
        RechargedTasks = 0;
    }

    public static void SealVent(int ventId)
    {
        RpcManager.Instance.Send((uint)Rpc.Role.SealVent, ventId, false);
    }

    [BindRpc((uint)Rpc.Role.SealVent)]
    public static void Rpc_SealVent(int ventId)
    {
        var vent = MapUtilities.CachedShipStatus.AllVents.FirstOrDefault(x => x != null && x.Id == ventId);
        if (vent == null) return;

        Instance.UsedScrews += Instance.VentPrice;
        if (CachedPlayer.LocalPlayer.PlayerControl == Instance.Player) {
            var animator = vent.GetComponent<SpriteAnim>(); 
            if (animator != null) animator.Stop();
            vent.EnterVentAnim = vent.ExitVentAnim = null;
            vent.myRend.sprite = animator == null ? StaticVentSealedSprite : AnimatedVentSealedSprite;
            if (SubmergedCompatibility.IsSubmerged && vent.Id == 0) vent.myRend.sprite = SubmergedCentralUpperSealedSprite;
            if (SubmergedCompatibility.IsSubmerged && vent.Id == 14) vent.myRend.sprite = SubmergedCentralLowerSealedSprite;
            vent.myRend.color = new Color(1f, 1f, 1f, 0.5f);
            vent.name = "FutureSealedVent_" + vent.name;
        }

        TORMapOptions.ventsToSeal.Add(vent);
    }

    public static void PlaceCamera(float x, float y, float z)
    {
        var data = new Tuple<float, float, float>(x, y, z);
        RpcManager.Instance.Send((uint)Rpc.Role.PlaceCamera, data, false);
    }

    [BindRpc((uint)Rpc.Role.PlaceCamera)]
    public static void Rpc_PlaceCamera(Tuple<float, float, float> xyz)
    {
        var referenceCamera = Object.FindObjectOfType<SurvCamera>(); 
        if (referenceCamera == null) return; // Mira HQ

        var (x, y, z) = xyz;
        Instance.UsedScrews += Instance.CamPrice;
        Instance.PlacedCameras++;

        var camera = Object.Instantiate(referenceCamera);
        camera.transform.position = new Vector3(x, y, z + 0.005f);
        camera.CamName = $"Security Camera {Instance.PlacedCameras}";
        camera.Offset = new Vector3(0f, 0f, camera.Offset.z);

        if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 2 ||
            GameOptionsManager.Instance.currentNormalGameOptions.MapId == 4)
        {
            camera.transform.localRotation = new Quaternion(0, 0, 1, 1); // Polus and Airship 
        }
        
        if (SubmergedCompatibility.IsSubmerged) {
            // remove 2d box collider of console, so that no barrier can be created. (irrelevant for now, but who knows... maybe we need it later)
            var fixConsole = camera.transform.FindChild("FixConsole");
            if (fixConsole != null) {
                var boxCollider = fixConsole.GetComponent<BoxCollider2D>();
                if (boxCollider != null) Object.Destroy(boxCollider);
            }
        }
        
        if (CachedPlayer.LocalPlayer.PlayerControl == Instance.Player) {
            camera.gameObject.SetActive(true);
            camera.gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f);
        } else {
            camera.gameObject.SetActive(false);
        }
        TORMapOptions.camerasToAdd.Add(camera);
    }
}