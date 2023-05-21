using System;
using System.Linq;
using AmongUs.GameOptions;
using BetterOtherRoles.EnoFw.Libs.Reactor.Utilities.Extensions;
using BetterOtherRoles.Players;
using BetterOtherRoles.Utilities;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace BetterOtherRoles.EnoFw.Modules;

public class RoleDisplayInterface
{
    private static bool RoleDescriptionIsOpen =>
        roleDescriptionDisplay != null && roleDescriptionDisplay.gameObject != null;

    private static TextMeshPro[] roleDescriptionTMPs = null;
    private static Minigame roleDescriptionDisplay;

    private static void OpenRoleDescription(HudManager hudManager)
    {
        if (hudManager.FullScreen == null || MapBehaviour.Instance && MapBehaviour.Instance.IsOpen
                                          /*|| AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started*/
                                          || GameOptionsManager.Instance.currentGameOptions.GameMode ==
                                          GameModes.HideNSeek) return;

        if (Camera.main == null || Minigame.Instance != null) return;

        if (roleDescriptionDisplay == null) Initialize(hudManager);

        // for (var index = 0; index < roleDescriptionTMPs.Length; index++)
        // {
        //     roleDescriptionTMPs[index] = UnityEngine.Object.Instantiate(hudManager.KillButton.cooldownTimerText, hudManager.transform);
        //     roleDescriptionTMPs[index].alignment = TMPro.TextAlignmentOptions.TopLeft;
        //     roleDescriptionTMPs[index].enableWordWrapping = false;
        //     roleDescriptionTMPs[index].transform.localScale = Vector3.one * 0.25f;
        //     roleDescriptionTMPs[index].transform.localPosition += new Vector3(-4f + 3f * index, 1.8f, -500f);
        //     roleDescriptionTMPs[index].gameObject.SetActive(true);
        // }

        var transform = roleDescriptionDisplay.transform;

        transform.SetParent(Camera.main.transform, false);
        transform.localPosition = new Vector3(0.0f, (-0.66f * 2), -100);
        roleDescriptionDisplay.Begin(null);

        // var backgroundRenderer = roleDescriptionDisplay.GetComponent<SpriteRenderer>();
        // backgroundRenderer.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        // backgroundRenderer.enabled = true;
    }

    private static void Initialize(HudManager hudManager)
    {
        BetterOtherRolesPlugin.Logger.LogMessage($"Initialize roledescription UI");
        // var vitalsObj = GameObject.FindObjectsOfType<SystemConsole>().ToList().Find(console => console.name == "panel_vitals");
        var vitalsObj = UnityEngine.Object.FindObjectsOfType<SystemConsole>()
            .FirstOrDefault(x => x.gameObject.name.Contains("panel_vitals"));
        var allRoleInfo = RoleInfo.getRoleInfoForPlayer(CachedPlayer.LocalPlayer);
        var isGuesser = HandleGuesser.isGuesserGm && HandleGuesser.isGuesser(CachedPlayer.LocalPlayer.PlayerId);

        roleDescriptionTMPs ??= new TMPro.TextMeshPro[allRoleInfo.Count + (isGuesser ? 1 : 0)];

        BetterOtherRolesPlugin.Logger.LogMessage($"Number of roles: {allRoleInfo.Count}");
        BetterOtherRolesPlugin.Logger.LogMessage($"IsGuesser: {isGuesser}");
        BetterOtherRolesPlugin.Logger.LogMessage($"Array Length: {roleDescriptionTMPs.Length}");

        if (vitalsObj == null || Camera.main == null) return;

        roleDescriptionDisplay = UnityEngine.Object.Instantiate(vitalsObj.MinigamePrefab, Camera.main.transform, false)
            .Cast<VitalsMinigame>();
        roleDescriptionDisplay.gameObject.name = "hudroleinfo";

        // for (var index = 0; index < allRoleInfo.Count; index++)
        // {
        //     roleDescriptionTMPs[index].text = allRoleInfo[index].name;
        //     TheOtherRolesPlugin.Logger.LogMessage($"Add Role to TMPs.list.");
        // }
        //             
        // if (isGuesser) roleDescriptionTMPs.Last().text = "isGuesser";
        // TheOtherRolesPlugin.Logger.LogMessage($"Add guesser to TMPs.list.");
    }

    private static void CloseRoleDescription()
    {
        if (Minigame.Instance != null)
        {
            roleDescriptionDisplay.Close();
        }

        foreach (var tmp in roleDescriptionTMPs)
            if (tmp)
                tmp.gameObject.Destroy();
    }

    private static void ToggleRoleDescription(HudManager hudManager)
    {
        if (RoleDescriptionIsOpen) CloseRoleDescription();
        else OpenRoleDescription(hudManager);
    }

    [HarmonyPrefix]
    public static void Prefix2(HudManager __instance)
    {
        if (!roleDescriptionTMPs[0]) return;

        foreach (var tmp in roleDescriptionTMPs) tmp.text = "";
    }

    private static PassiveButton toggleRoleInfoButton;
    private static GameObject toggleRoleInfoButtonObject;

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    internal class HudManagerUpdatePatch
    {
        private static void Postfix(HudManager __instance)
        {
            if (!DevMode.ShowRoleDescription) return;

            if (!toggleRoleInfoButton || !toggleRoleInfoButtonObject)
            {
                // add a special button for RoleInfo display:
                toggleRoleInfoButtonObject = UnityEngine.Object.Instantiate(__instance.MapButton.gameObject,
                    __instance.MapButton.transform.parent);
                toggleRoleInfoButtonObject.transform.localPosition =
                    __instance.MapButton.transform.localPosition + new Vector3(0, 0, -500f);
                SpriteRenderer roleInfoRenderer = toggleRoleInfoButtonObject.GetComponent<SpriteRenderer>();
                roleInfoRenderer.sprite =
                    Helpers.loadSpriteFromResources("BetterOtherRoles.Resources.CurrentSettingsButton.png", 180f);
                toggleRoleInfoButton = toggleRoleInfoButtonObject.GetComponent<PassiveButton>();
                toggleRoleInfoButton.OnClick.RemoveAllListeners();
                toggleRoleInfoButton.OnClick.AddListener((Action)(() => ToggleRoleDescription(__instance)));
            }

            toggleRoleInfoButtonObject.SetActive(__instance.MapButton.gameObject.active &&
                                                 !(MapBehaviour.Instance && MapBehaviour.Instance.IsOpen) &&
                                                 GameOptionsManager.Instance.currentGameOptions.GameMode !=
                                                 GameModes.HideNSeek);
            toggleRoleInfoButtonObject.transform.localPosition =
                __instance.MapButton.transform.localPosition + new Vector3(0, (-0.66f * 2), -500f);
        }
    }
}