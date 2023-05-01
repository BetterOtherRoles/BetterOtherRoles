using AmongUs.GameOptions;
using HarmonyLib;
using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.EnoFramework.Utils;
using TheOtherRoles.Players;
using UnityEngine;

namespace TheOtherRoles.Customs.Roles.Crewmate;

[EnoSingleton]
public class Lighter : CustomRole
{
    public readonly EnoFramework.CustomOption LightsOnVision;
    public readonly EnoFramework.CustomOption LightsOffVision;
    public readonly EnoFramework.CustomOption VisionWidth;

    public Lighter() : base(nameof(Lighter))
    {
        Team = Teams.Crewmate;
        Color = new Color32(238, 229, 190, byte.MaxValue);

        IntroDescription = "Your light never goes out";
        ShortDescription = "Your light never goes out";

        LightsOnVision = OptionsTab.CreateFloatList(
            $"{Name}{nameof(LightsOnVision)}",
            Cs("Vision when lights are on"),
            0.25f,
            5f,
            1.5f,
            0.25f,
            SpawnRate);
        LightsOffVision = OptionsTab.CreateFloatList(
            $"{Name}{nameof(LightsOffVision)}",
            Cs("Vision when lights are off"),
            0.25f,
            5f,
            0.5f,
            0.25f,
            SpawnRate);
        VisionWidth = OptionsTab.CreateFloatList(
            $"{Name}{nameof(VisionWidth)}",
            Cs("Flashlight width"),
            0.1f,
            1f,
            0.3f,
            0.1f,
            SpawnRate);
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.IsFlashlightEnabled))]
    private static class PlayerControlIsFlashlightEnabledPatch
    {
        public static bool Prefix(ref bool __result)
        {
            if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek)
                return true;
            __result = !CachedPlayer.LocalPlayer.Data.IsDead && Singleton<Lighter>.Instance.Player != null &&
                       Singleton<Lighter>.Instance.Player.PlayerId == CachedPlayer.LocalPlayer.PlayerId;

            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.AdjustLighting))]
    private static class PlayerControlAdjustLightPatch
    {
        public static bool Prefix(PlayerControl __instance)
        {
            if (__instance == null || CachedPlayer.LocalPlayer == null ||
                Singleton<Lighter>.Instance.Player == null) return true;

            var hasFlashlight = !CachedPlayer.LocalPlayer.Data.IsDead &&
                                Singleton<Lighter>.Instance.Player.PlayerId == CachedPlayer.LocalPlayer.PlayerId;
            __instance.SetFlashlightInputMethod();
            __instance.lightSource.SetupLightingForGameplay(hasFlashlight, Singleton<Lighter>.Instance.VisionWidth,
                __instance.TargetFlashlight.transform);

            return false;
        }
    }
}