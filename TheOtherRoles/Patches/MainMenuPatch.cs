using System;
using System.Linq;
using AmongUs.Data;
using Assets.InnerNet;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using TheOtherRoles.Modules;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.UI.Button;
using Object = UnityEngine.Object;

namespace TheOtherRoles.Patches {
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public class MainMenuPatch {
        private static GameObject bottomTemplate;
        private static AnnouncementPopUp popUp;

        private static void Prefix(MainMenuManager __instance) {
            CustomHatLoader.LaunchHatFetcher();
            var template = GameObject.Find("ExitGameButton");
            if (template == null) return;

            var buttonGithub = Object.Instantiate(template, null);
            var localPosition = buttonGithub.transform.localPosition;
            localPosition = new Vector3(localPosition.x, localPosition.y + 0.6f, localPosition.z);
            buttonGithub.transform.localPosition = localPosition;

            var textGithub = buttonGithub.transform.GetChild(0).GetComponent<TMPro.TMP_Text>();
            __instance.StartCoroutine(Effects.Lerp(0.1f, new System.Action<float>((p) => {
                textGithub.SetText("Github");
            })));

            var passiveButtonGithub = buttonGithub.GetComponent<PassiveButton>();
            var passiveSpriteGithub = buttonGithub.GetComponent<SpriteRenderer>();

            passiveButtonGithub.OnClick = new Button.ButtonClickedEvent();
            passiveButtonGithub.OnClick.AddListener((System.Action)(() => Application.OpenURL("https://discord.gg/77RkMJHWsM")));

            Color discordColor = new Color32(88, 101, 242, byte.MaxValue);
            passiveSpriteGithub.color = textGithub.color = discordColor;
            passiveButtonGithub.OnMouseOut.AddListener((System.Action)delegate {
                passiveSpriteGithub.color = textGithub.color = discordColor;
            });


            bottomTemplate = GameObject.Find("InventoryButton");

            // TOR credits button
            if (bottomTemplate == null) return;
            var creditsButton = Object.Instantiate(bottomTemplate, bottomTemplate.transform.parent);
            var passiveCreditsButton = creditsButton.GetComponent<PassiveButton>();
            var spriteCreditsButton = creditsButton.GetComponent<SpriteRenderer>();

            spriteCreditsButton.sprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.CreditsButton.png", 75f);

            passiveCreditsButton.OnClick = new ButtonClickedEvent();

            passiveCreditsButton.OnClick.AddListener((Action)delegate {
                // do stuff
                if (popUp != null) Object.Destroy(popUp);
                popUp = Object.Instantiate(Object.FindObjectOfType<AnnouncementPopUp>(true));
                popUp.gameObject.SetActive(true);
                
                __instance.StartCoroutine(Effects.Lerp(0.01f, new Action<float>((p) =>
                {
                    if (p != 1) return;
                    var backup = DataManager.Player.Announcements.allAnnouncements;
                    popUp.Init(false);
                    DataManager.Player.Announcements.allAnnouncements = new List<Announcement>();
                    DataManager.Player.Announcements.allAnnouncements.Insert(0, ModCredits.GetBetterOtherRolesCredits());
                    foreach (var item in popUp.visibleAnnouncements) Object.Destroy(item);
                    foreach (var item in Object.FindObjectsOfType<AnnouncementPanel>()) {
                        if (item != popUp.ErrorPanel) Object.Destroy(item.gameObject);
                    }
                    popUp.CreateAnnouncementList();
                    popUp.visibleAnnouncements[0].PassiveButton.OnClick.RemoveAllListeners();
                    DataManager.Player.Announcements.allAnnouncements = backup;
                    var titleText = GameObject.Find("Title_Text").GetComponent<TMPro.TextMeshPro>();
                    if (titleText != null) titleText.text = "";
                })));
            });
            
        }

        public static void Postfix(MainMenuManager __instance) {
            __instance.StartCoroutine(Effects.Lerp(0.01f, new Action<float>((p) =>
            {
                if (p != 1) return;
                bottomTemplate = GameObject.Find("InventoryButton");
                foreach (Transform tf in bottomTemplate.transform.parent.GetComponentsInChildren<Transform>())
                {
                    var localPosition = tf.localPosition;
                    localPosition = new Vector2(localPosition.x * 0.8f, localPosition.y);
                    tf.localPosition = localPosition;
                }
            })));

        }

        public static void addSceneChangeCallbacks() {
            SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>)((scene, _) => {
                if (!scene.name.Equals("MatchMaking", StringComparison.Ordinal)) return;
                TORMapOptions.gameMode = CustomGamemodes.Classic;
                // Add buttons For Guesser Mode, Hide N Seek in this scene.
                // find "HostLocalGameButton"
                var template = GameObject.FindObjectOfType<HostLocalGameButton>();
                var gameButton = template.transform.FindChild("CreateGameButton");
                var gameButtonPassiveButton = gameButton.GetComponentInChildren<PassiveButton>();

                var guesserButton = GameObject.Instantiate<Transform>(gameButton, gameButton.parent);
                guesserButton.transform.localPosition += new Vector3(0f, -0.5f);
                var guesserButtonText = guesserButton.GetComponentInChildren<TMPro.TextMeshPro>();
                var guesserButtonPassiveButton = guesserButton.GetComponentInChildren<PassiveButton>();
                
                guesserButtonPassiveButton.OnClick = new Button.ButtonClickedEvent();
                guesserButtonPassiveButton.OnClick.AddListener((System.Action)(() => {
                    TORMapOptions.gameMode = CustomGamemodes.Guesser;
                    template.OnClick();
                }));

                var hideNSeekButton = GameObject.Instantiate(gameButton, gameButton.parent);
                hideNSeekButton.transform.localPosition += new Vector3(1.7f, -0.5f);
                var hideNSeekButtonText = hideNSeekButton.GetComponentInChildren<TMPro.TextMeshPro>();
                var hideNSeekButtonPassiveButton = hideNSeekButton.GetComponentInChildren<PassiveButton>();
                
                hideNSeekButtonPassiveButton.OnClick = new Button.ButtonClickedEvent();
                hideNSeekButtonPassiveButton.OnClick.AddListener((System.Action)(() => {
                    TORMapOptions.gameMode = CustomGamemodes.HideNSeek;
                    template.OnClick();
                }));

                template.StartCoroutine(Effects.Lerp(0.1f, new Action<float>(_ => {
                    guesserButtonText.SetText("TOR Guesser");
                    hideNSeekButtonText.SetText("TOR Hide N Seek");
                 })));
            }));
        }
    }
}
