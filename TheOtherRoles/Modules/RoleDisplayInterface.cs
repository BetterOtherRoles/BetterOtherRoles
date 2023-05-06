using System;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;


namespace TheOtherRoles.Modules;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public class RoleDisplayInterface
{
    public static bool RoleDescriptionIsOpen => roleDescriptionDisplay != null && roleDescriptionDisplay.gameObject != null;
    private static TMPro.TextMeshPro[] roleDescriptionTMPs = Array.Empty<TextMeshPro>();
    public static Minigame roleDescriptionDisplay;
    
    public static void OpenRoleDescription(HudManager hudManager) {
            if (hudManager.FullScreen == null || MapBehaviour.Instance && MapBehaviour.Instance.IsOpen
                /*|| AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started*/
                || GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return;
            

            if (Camera.main == null || Minigame.Instance != null) return;
            
            
            if (roleDescriptionDisplay == null) 
            {
                // var vitalsObj = GameObject.FindObjectsOfType<SystemConsole>().ToList().Find(console => console.name == "panel_vitals");
                var vitalsObj = UnityEngine.Object.FindObjectsOfType<SystemConsole>().FirstOrDefault(x => x.gameObject.name.Contains("panel_vitals"));

                if (vitalsObj != null)
                {
                    var hud = UnityEngine.Object.Instantiate(vitalsObj.MinigamePrefab, Camera.main.transform, false).Cast<VitalsMinigame>();
                    
                    // var allRoleInfo = RoleInfo.getRoleInfoForPlayer(CachedPlayer.LocalPlayer);

                    // roleDescriptionTMPs = new TMPro.TextMeshPro[allRoleInfo.Count];

                    // for (var index = 0; index < allRoleInfo.Count; index++)
                    // {
                    //     roleDescriptionTMPs[index].text = allRoleInfo[index].name;
                    // }

                    roleDescriptionDisplay = hud;
                    roleDescriptionDisplay.gameObject.name = "hudroleinfo";
                }
            }

            var transform = roleDescriptionDisplay.transform;

            transform.SetParent(Camera.main.transform, false);
            transform.localPosition = new Vector3(0.0f, (-0.66f * 2), -100);
            roleDescriptionDisplay.Begin(null);
            
            // var backgroundRenderer = roleDescriptionDisplay.GetComponent<SpriteRenderer>();
            // backgroundRenderer.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            // backgroundRenderer.enabled = true;
            
            // if (roleDescriptionTMPs[0])
            // {
            //   for (int index = 0; index < roleDescriptionTMPs.Count(); index++) {
            //      roleDescriptionTMPs[index] = UnityEngine.Object.Instantiate(hudManager.KillButton.cooldownTimerText, hudManager.transform);
            //      roleDescriptionTMPs[index].alignment = TMPro.TextAlignmentOptions.TopLeft;
            //      roleDescriptionTMPs[index].enableWordWrapping = false;
            //      roleDescriptionTMPs[index].transform.localScale = Vector3.one * 0.25f;
            //      roleDescriptionTMPs[index].transform.localPosition += new Vector3(-4f + 3f * index, 1.8f, -500f);
            //      roleDescriptionTMPs[index].gameObject.SetActive(true);
            //  }
            // }
        }
        public static void CloseRoleDescription() {
            if (Minigame.Instance != null) {
                roleDescriptionDisplay.ForceClose();
            }

            foreach (var tmp in roleDescriptionTMPs)
                if (tmp) tmp.gameObject.Destroy();

        }

        public static void ToggleRoleDescription(HudManager hudManager) {
            if (RoleDescriptionIsOpen) CloseRoleDescription();
            else OpenRoleDescription(hudManager);
        }
        
        static PassiveButton toggleRoleInfoButton;
        static GameObject toggleRoleInfoButtonObject;
        [HarmonyPostfix]
        public static void Postfix(HudManager __instance) {
            if (!toggleRoleInfoButton || !toggleRoleInfoButtonObject) {
                // add a special button for RoleInfo display:
                toggleRoleInfoButtonObject = UnityEngine.Object.Instantiate(__instance.MapButton.gameObject, __instance.MapButton.transform.parent);
                toggleRoleInfoButtonObject.transform.localPosition = __instance.MapButton.transform.localPosition + new Vector3(0, 0, -500f);
                SpriteRenderer roleInfoRenderer = toggleRoleInfoButtonObject.GetComponent<SpriteRenderer>();
                roleInfoRenderer.sprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.CurrentSettingsButton.png", 180f);
                toggleRoleInfoButton = toggleRoleInfoButtonObject.GetComponent<PassiveButton>();
                toggleRoleInfoButton.OnClick.RemoveAllListeners();
                toggleRoleInfoButton.OnClick.AddListener((Action)(() => ToggleRoleDescription(__instance)));

            }
            toggleRoleInfoButtonObject.SetActive(__instance.MapButton.gameObject.active && !(MapBehaviour.Instance && MapBehaviour.Instance.IsOpen) && GameOptionsManager.Instance.currentGameOptions.GameMode != GameModes.HideNSeek);
            toggleRoleInfoButtonObject.transform.localPosition = __instance.MapButton.transform.localPosition + new Vector3(0, (-0.66f * 2), -500f);
        }
}