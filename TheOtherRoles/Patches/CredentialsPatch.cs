using HarmonyLib;
using System;
using TheOtherRoles;
using TheOtherRoles.CustomGameModes;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;
using UnityEngine;

namespace TheOtherRoles.Patches
{
    [HarmonyPatch]
    public static class CredentialsPatch
    {
        private const string FullCredentialsText = "<color=#cc9e41>Better</color><color=#ff351f>OtherRoles</color>";
        private static readonly string FullVersionText = $"<color=#cc9e41>v{TheOtherRolesPlugin.Version}</color>";

        private static readonly string FullCredentialsVersion =
            $"<size=130%>{FullCredentialsText}</size> {FullVersionText}\n<size=60%>Based on <color=#ff351f>TheOtherRoles</color></size>";

        [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
        private static class VersionShowerPatch
        {
            static void Postfix(VersionShower __instance)
            {
                var amongUsLogo = GameObject.Find("bannerLogo_AmongUs");
                if (amongUsLogo == null) return;

                var credentials = UnityEngine.Object.Instantiate(__instance.text, amongUsLogo.transform, true);
                credentials.transform.position = new Vector3(0, 0.08f, 0);
                credentials.SetText($"<size=150%>{FullCredentialsText}\n{FullVersionText}</size>");
                credentials.alignment = TMPro.TextAlignmentOptions.Center;
                credentials.fontSize *= 0.75f;
            }
        }

        [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
        internal static class PingTrackerPatch
        {
            public static GameObject ModStamp;

            static void Postfix(PingTracker __instance)
            {
                __instance.text.alignment = TMPro.TextAlignmentOptions.TopRight;
                if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
                {
                    string gameModeText = $"";
                    if (HideNSeek.isHideNSeekGM) gameModeText = "Hide 'N Seek";
                    else if (HandleGuesser.isGuesserGm) gameModeText = "Guesser";
                    if (gameModeText != "") gameModeText = Helpers.cs(Color.yellow, gameModeText) + "\n";
                    __instance.text.text = $"{FullCredentialsVersion}\n{gameModeText}" + __instance.text.text;
                    if (CachedPlayer.LocalPlayer.Data.IsDead || (!(CachedPlayer.LocalPlayer.PlayerControl == null) &&
                                                                 (CachedPlayer.LocalPlayer.PlayerControl ==
                                                                  Lovers.lover1 ||
                                                                  CachedPlayer.LocalPlayer.PlayerControl ==
                                                                  Lovers.lover2)))
                    {
                        var transform = __instance.transform;
                        var localPosition = transform.localPosition;
                        localPosition = new Vector3(3.45f, localPosition.y, localPosition.z);
                        transform.localPosition = localPosition;
                    }
                    else
                    {
                        var transform = __instance.transform;
                        var localPosition = transform.localPosition;
                        localPosition = new Vector3(4.2f, localPosition.y, localPosition.z);
                        transform.localPosition = localPosition;
                    }
                }
                else
                {
                    var gameModeText = TORMapOptions.gameMode switch
                    {
                        CustomGamemodes.HideNSeek => "Hide 'N Seek",
                        CustomGamemodes.Guesser => "Guesser",
                        _ => ""
                    };
                    if (gameModeText != "") gameModeText = Helpers.cs(Color.yellow, gameModeText) + "\n";

                    __instance.text.text = $"{FullCredentialsVersion}\n  {gameModeText}\n {__instance.text.text}";
                    var transform = __instance.transform;
                    var localPosition = transform.localPosition;
                    localPosition = new Vector3(3.5f, localPosition.y, localPosition.z);
                    transform.localPosition = localPosition;
                }
            }
        }

        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
        public static class LogoPatch
        {
            public static SpriteRenderer renderer;
            public static Sprite bannerSprite;
            public static Sprite horseBannerSprite;
            public static Sprite banner2Sprite;
            private static PingTracker instance;

            static void Postfix(PingTracker __instance)
            {
                var amongUsLogo = GameObject.Find("bannerLogo_AmongUs");
                if (amongUsLogo != null)
                {
                    amongUsLogo.transform.localScale *= 0.6f;
                    amongUsLogo.transform.position += Vector3.up * 0.25f;
                }

                var torLogo = new GameObject("bannerLogo_TOR");
                torLogo.transform.position = Vector3.up;
                renderer = torLogo.AddComponent<SpriteRenderer>();
                loadSprites();
                renderer.sprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Banner.png", 300f);

                instance = __instance;
                loadSprites();
                // renderer.sprite = TORMapOptions.enableHorseMode ? horseBannerSprite : bannerSprite;
                renderer.sprite = EventUtility.isEnabled ? banner2Sprite : bannerSprite;
            }

            public static void loadSprites()
            {
                if (bannerSprite == null)
                    bannerSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Banner.png", 300f);
                if (banner2Sprite == null)
                    banner2Sprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Banner2.png", 300f);
                if (horseBannerSprite == null)
                    horseBannerSprite =
                        Helpers.loadSpriteFromResources("TheOtherRoles.Resources.bannerTheHorseRoles.png", 300f);
            }

            public static void updateSprite()
            {
                loadSprites();
                if (renderer != null)
                {
                    float fadeDuration = 1f;
                    instance.StartCoroutine(Effects.Lerp(fadeDuration, new Action<float>((p) =>
                    {
                        renderer.color = new Color(1, 1, 1, 1 - p);
                        if (p == 1)
                        {
                            renderer.sprite = TORMapOptions.enableHorseMode ? horseBannerSprite : bannerSprite;
                            instance.StartCoroutine(Effects.Lerp(fadeDuration,
                                new Action<float>((p) => { renderer.color = new Color(1, 1, 1, p); })));
                        }
                    })));
                }
            }
        }
    }
}