using System;
using System.Linq;
using PowerTools;
using Reactor.Networking.Attributes;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TheOtherRoles.EnoFw.Roles.Crewmate;

public static class SecurityGuard
{
    public static PlayerControl securityGuard;
    public static Color color = new Color32(195, 178, 95, byte.MaxValue);

    public static float cooldown = 30f;
    public static int remainingScrews = 7;
    public static int totalScrews = 7;
    public static int ventPrice = 1;
    public static int camPrice = 2;
    public static int placedCameras;
    public static float duration = 10f;
    public static int maxCharges = 5;
    public static int rechargeTasksNumber = 3;
    public static int rechargedTasks = 3;
    public static int charges = 1;
    public static bool cantMove = true;
    public static Vent ventTarget;
    public static Minigame minigame;

    private static Sprite closeVentButtonSprite;

    public static Sprite getCloseVentButtonSprite()
    {
        if (closeVentButtonSprite) return closeVentButtonSprite;
        closeVentButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.CloseVentButton.png", 115f);
        return closeVentButtonSprite;
    }

    private static Sprite placeCameraButtonSprite;

    public static Sprite getPlaceCameraButtonSprite()
    {
        if (placeCameraButtonSprite) return placeCameraButtonSprite;
        placeCameraButtonSprite =
            Helpers.loadSpriteFromResources("TheOtherRoles.Resources.PlaceCameraButton.png", 115f);
        return placeCameraButtonSprite;
    }

    private static Sprite animatedVentSealedSprite;
    private static float lastPPU;

    public static Sprite getAnimatedVentSealedSprite()
    {
        float ppu = 185f;
        if (SubmergedCompatibility.IsSubmerged) ppu = 120f;
        if (lastPPU != ppu)
        {
            animatedVentSealedSprite = null;
            lastPPU = ppu;
        }

        if (animatedVentSealedSprite) return animatedVentSealedSprite;
        animatedVentSealedSprite =
            Helpers.loadSpriteFromResources("TheOtherRoles.Resources.AnimatedVentSealed.png", ppu);
        return animatedVentSealedSprite;
    }

    private static Sprite staticVentSealedSprite;

    public static Sprite getStaticVentSealedSprite()
    {
        if (staticVentSealedSprite) return staticVentSealedSprite;
        staticVentSealedSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.StaticVentSealed.png", 160f);
        return staticVentSealedSprite;
    }

    private static Sprite submergedCentralUpperVentSealedSprite;

    public static Sprite getSubmergedCentralUpperSealedSprite()
    {
        if (submergedCentralUpperVentSealedSprite) return submergedCentralUpperVentSealedSprite;
        submergedCentralUpperVentSealedSprite =
            Helpers.loadSpriteFromResources("TheOtherRoles.Resources.CentralUpperBlocked.png", 145f);
        return submergedCentralUpperVentSealedSprite;
    }

    private static Sprite submergedCentralLowerVentSealedSprite;

    public static Sprite getSubmergedCentralLowerSealedSprite()
    {
        if (submergedCentralLowerVentSealedSprite) return submergedCentralLowerVentSealedSprite;
        submergedCentralLowerVentSealedSprite =
            Helpers.loadSpriteFromResources("TheOtherRoles.Resources.CentralLowerBlocked.png", 145f);
        return submergedCentralLowerVentSealedSprite;
    }

    private static Sprite camSprite;

    public static Sprite getCamSprite()
    {
        if (camSprite) return camSprite;
        camSprite = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.CamsButton]
            .Image;
        return camSprite;
    }

    private static Sprite logSprite;

    public static Sprite getLogSprite()
    {
        if (logSprite) return logSprite;
        logSprite = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.DoorLogsButton]
            .Image;
        return logSprite;
    }

    public static void clearAndReload()
    {
        securityGuard = null;
        ventTarget = null;
        minigame = null;
        duration = CustomOptionHolder.securityGuardCamDuration.getFloat();
        maxCharges = Mathf.RoundToInt(CustomOptionHolder.securityGuardCamMaxCharges.getFloat());
        rechargeTasksNumber = Mathf.RoundToInt(CustomOptionHolder.securityGuardCamRechargeTasksNumber.getFloat());
        rechargedTasks = Mathf.RoundToInt(CustomOptionHolder.securityGuardCamRechargeTasksNumber.getFloat());
        charges = Mathf.RoundToInt(CustomOptionHolder.securityGuardCamMaxCharges.getFloat()) / 2;
        placedCameras = 0;
        cooldown = CustomOptionHolder.securityGuardCooldown.getFloat();
        totalScrews = remainingScrews = Mathf.RoundToInt(CustomOptionHolder.securityGuardTotalScrews.getFloat());
        camPrice = Mathf.RoundToInt(CustomOptionHolder.securityGuardCamPrice.getFloat());
        ventPrice = Mathf.RoundToInt(CustomOptionHolder.securityGuardVentPrice.getFloat());
        cantMove = CustomOptionHolder.securityGuardNoMove.getBool();
    }

    public static void SealVent(int ventId)
    {
        var data = new Tuple<int>(ventId);
        Rpc_SealVent(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.SealVent)]
    private static void Rpc_SealVent(PlayerControl sender, string rawData)
    {
        var ventId = Rpc.Deserialize<Tuple<int>>(rawData).Item1;
        var vent = MapUtilities.CachedShipStatus.AllVents.FirstOrDefault(x => x != null && x.Id == ventId);
        if (vent == null) return;

        remainingScrews -= ventPrice;
        if (CachedPlayer.LocalPlayer.PlayerControl == securityGuard) {
            var animator = vent.GetComponent<SpriteAnim>(); 
            if (animator != null) animator.Stop();
            vent.EnterVentAnim = vent.ExitVentAnim = null;
            vent.myRend.sprite = animator == null ? getStaticVentSealedSprite() : getAnimatedVentSealedSprite();
            if (SubmergedCompatibility.IsSubmerged && vent.Id == 0) vent.myRend.sprite = getSubmergedCentralUpperSealedSprite();
            if (SubmergedCompatibility.IsSubmerged && vent.Id == 14) vent.myRend.sprite = getSubmergedCentralLowerSealedSprite();
            vent.myRend.color = new Color(1f, 1f, 1f, 0.5f);
            vent.name = "FutureSealedVent_" + vent.name;
        }

        TORMapOptions.ventsToSeal.Add(vent);
    }

    public static void PlaceCamera(float x, float y)
    {
        var data = new Tuple<float, float>(x, y);
        Rpc_PlaceCamera(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.PlaceCamera)]
    private static void Rpc_PlaceCamera(PlayerControl sender, string rawDData)
    {
        var referenceCamera = Object.FindObjectOfType<SurvCamera>(); 
        if (referenceCamera == null) return; // Mira HQ
        
        var (x, y) = Rpc.Deserialize<Tuple<float, float>>(rawDData);
        remainingScrews -= camPrice;
        placedCameras++;

        var camera = Object.Instantiate(referenceCamera);
        camera.transform.position = new Vector3(x, y, referenceCamera.transform.position.z - 1f);
        camera.CamName = $"Security Camera {placedCameras}";
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
        
        if (CachedPlayer.LocalPlayer.PlayerControl == securityGuard) {
            camera.gameObject.SetActive(true);
            camera.gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f);
        } else {
            camera.gameObject.SetActive(false);
        }
        TORMapOptions.camerasToAdd.Add(camera);
    }
}