using System;
using System.Collections.Generic;
using System.Linq;
using BetterOtherRoles.EnoFw.Utils;
using BetterOtherRoles.Players;
using HarmonyLib;
using UnityEngine;

namespace BetterOtherRoles.EnoFw.Kernel;

public static class CustomOptionsPatch
{
    private static float timer = 1f;

    private static bool SetNames(Dictionary<string, string> gameObjectNameDisplayNameMap)
    {
        foreach (var entry in gameObjectNameDisplayNameMap.Where(entry => GameObject.Find(entry.Key) != null))
        {
            // Settings setup has already been performed, fixing the title of the tab and returning
            GameObject.Find(entry.Key).transform.FindChild("GameGroup").FindChild("Text")
                .GetComponent<TMPro.TextMeshPro>().SetText(entry.Value);
            return true;
        }

        return false;
    }

    private static GameOptionsMenu GetMenu(GameObject setting, string settingName)
    {
        var menu = setting.transform.FindChild("GameGroup").FindChild("SliderInner").GetComponent<GameOptionsMenu>();
        setting.name = settingName;

        return menu;
    }

    private static SpriteRenderer GetTabHighlight(GameObject tab, string tabName, string tabSpritePath)
    {
        var tabHighlight = tab.transform.FindChild("Hat Button").FindChild("Tab Background")
            .GetComponent<SpriteRenderer>();
        tab.transform.FindChild("Hat Button").FindChild("Icon").GetComponent<SpriteRenderer>().sprite =
            Helpers.loadSpriteFromResources(tabSpritePath, 100f);
        tab.name = "tabName";

        return tabHighlight;
    }

    private static void SetListener(Dictionary<GameObject, SpriteRenderer> settingsHighlightMap, int index)
    {
        foreach (var entry in settingsHighlightMap)
        {
            entry.Key.SetActive(false);
            entry.Value.enabled = false;
        }

        settingsHighlightMap.ElementAt(index).Key.SetActive(true);
        settingsHighlightMap.ElementAt(index).Value.enabled = true;
    }

    private static void DestroyOptions(List<List<OptionBehaviour>> optionBehavioursList)
    {
        foreach (var option in optionBehavioursList.SelectMany(optionBehaviours => optionBehaviours))
        {
            UnityEngine.Object.Destroy(option.gameObject);
        }
    }

    private static void SetOptions(
        IReadOnlyList<GameOptionsMenu> menus,
        IReadOnlyList<List<OptionBehaviour>> options,
        IReadOnlyList<GameObject> settings)
    {
        if (menus.Count != options.Count || options.Count != settings.Count)
        {
            BetterOtherRolesPlugin.Logger.LogError("List counts are not equal");
            return;
        }

        for (var i = 0; i < menus.Count; i++)
        {
            menus[i].Children = options[i].ToArray();
            settings[i].gameObject.SetActive(false);
        }
    }

    private static void CreateCustomTabs(GameOptionsMenu gameOptionsMenu)
    {
        var tabKeys = CustomOption.Tab.Tabs.ToDictionary(
            customSetting => customSetting.Key,
            customSetting => customSetting.Title);
        var isReturn = SetNames(tabKeys);
        if (isReturn) return;

        var template = UnityEngine.Object.FindObjectsOfType<StringOption>().FirstOrDefault();
        var boolTemplate = UnityEngine.Object.FindObjectsOfType<ToggleOption>().FirstOrDefault();
        if (template == null || boolTemplate == null) return;

        var gameSettings = GameObject.Find("Game Settings");
        var gameSettingMenu = UnityEngine.Object.FindObjectsOfType<GameSettingMenu>().FirstOrDefault();
        if (gameSettingMenu == null) return;

        var customSettings = new Dictionary<string, GameObject>();
        var customMenus = new Dictionary<string, GameOptionsMenu>();

        for (var index = 0; index < CustomOption.Tab.Tabs.Count; index++)
        {
            var settingsTabInfo = CustomOption.Tab.Tabs[index];
            GameObject setting;
            if (index == 0)
            {
                setting = UnityEngine.Object.Instantiate(gameSettings, gameSettings.transform.parent);
            }
            else
            {
                var previousInfo = CustomOption.Tab.Tabs[index - 1];
                var previousSetting = customSettings[previousInfo.Key];
                setting = UnityEngine.Object.Instantiate(gameSettings, previousSetting.transform.parent);
            }

            customMenus[settingsTabInfo.Key] = GetMenu(setting, settingsTabInfo.Key);
            customSettings[settingsTabInfo.Key] = setting;
        }

        var roleTab = GameObject.Find("RoleTab");
        var gameTab = GameObject.Find("GameTab");

        var customTabs = new Dictionary<string, GameObject>();
        var customTabHighlights = new Dictionary<string, SpriteRenderer>();
        for (var index = 0; index < CustomOption.Tab.Tabs.Count; index++)
        {
            var tabInfo = CustomOption.Tab.Tabs[index];
            GameObject tab;
            if (index == 0)
            {
                tab = UnityEngine.Object.Instantiate(roleTab, roleTab.transform.parent);
            }
            else
            {
                var previousInfo = CustomOption.Tab.Tabs[index - 1];
                var previousTab = customTabs[previousInfo.Key];
                tab = UnityEngine.Object.Instantiate(roleTab, previousTab.transform);
            }

            customTabs[tabInfo.Key] = tab;
            var tabHighlight = GetTabHighlight(tab, $"{tabInfo.Key}Tab", tabInfo.IconPath);
            customTabHighlights[tabInfo.Key] = tabHighlight;
        }

        gameTab.transform.position += Vector3.left * 3f;
        roleTab.transform.position += Vector3.left * 3f;
        for (var index = 0; index < CustomOption.Tab.Tabs.Count; index++)
        {
            var tabInfo = CustomOption.Tab.Tabs[index];
            var tab = customTabs[tabInfo.Key];
            if (index == 0)
            {
                tab.transform.position += Vector3.left * 2f;
            }
            else
            {
                tab.transform.localPosition += Vector3.right * 1f;
            }
        }

        var tabs = new List<GameObject> { gameTab, roleTab };
        tabs.AddRange(customTabs.Select(ct => ct.Value));

        var settingsHighlightMap = new Dictionary<GameObject, SpriteRenderer>
        {
            [gameSettingMenu.RegularGameSettings] = gameSettingMenu.GameSettingsHightlight,
            [gameSettingMenu.RolesSettings.gameObject] = gameSettingMenu.RolesSettingsHightlight,
        };
        foreach (var cs in customSettings)
        {
            settingsHighlightMap[cs.Value.gameObject] = customTabHighlights[cs.Key];
        }

        for (var i = 0; i < tabs.Count; i++)
        {
            var button = tabs[i].GetComponentInChildren<PassiveButton>();
            if (button == null) continue;
            var copiedIndex = i;
            button.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            button.OnClick.AddListener((Action)(() => { SetListener(settingsHighlightMap, copiedIndex); }));
        }

        DestroyOptions(
            customMenus.Select(cm => cm.Value.GetComponentsInChildren<OptionBehaviour>().ToList())
                .ToList());

        var customOptions = new Dictionary<string, List<OptionBehaviour>>();

        var menus = new Dictionary<string, Transform>();
        var optionBehaviours =
            new Dictionary<string, List<OptionBehaviour>>();

        foreach (var cst in CustomOption.Tab.Tabs)
        {
            customOptions[cst.Key] = new List<OptionBehaviour>();
            menus[cst.Key] = customMenus[cst.Key].transform;
            optionBehaviours[cst.Key] = customOptions[cst.Key];
        }

        foreach (var cst in CustomOption.Tab.Tabs)
        {
            foreach (var setting in cst.Settings.Where(o => o.AllowedGameModes.Contains(CustomOption.CurrentGameMode)))
            {
                if (setting.OptionBehaviour == null)
                {
                    if (setting.Type == CustomOption.OptionType.Boolean)
                    {
                        var boolOption = UnityEngine.Object.Instantiate(boolTemplate, menus[cst.Key]);
                        optionBehaviours[cst.Key].Add(boolOption);
                        boolOption.OnValueChanged = new Action<OptionBehaviour>((_) => { });
                        boolOption.TitleText.text = setting.DisplayNameWithIndentation;
                        boolOption.CheckMark.enabled = boolOption.oldValue = setting;
                        
                        setting.OptionBehaviour = boolOption;
                    }
                    else
                    {
                        var stringOption = UnityEngine.Object.Instantiate(template, menus[cst.Key]);
                        optionBehaviours[cst.Key].Add(stringOption);
                        stringOption.OnValueChanged = new Action<OptionBehaviour>((_) => { });
                        stringOption.TitleText.text = setting.DisplayNameWithIndentation;
                        stringOption.Value = stringOption.oldValue = setting.SelectionIndex;
                        stringOption.ValueText.text = setting.StringSelections[setting.SelectionIndex];

                        setting.OptionBehaviour = stringOption;
                    }
                }

                setting.OptionBehaviour.gameObject.SetActive(true);
            }
        }

        SetOptions(customMenus.Values.ToList(), optionBehaviours.Values.ToList(), customSettings.Values.ToList());
    }

    private static bool CustomOptionEnable(StringOption stringOption)
    {
        var option = CustomOption.Tab.Options.FirstOrDefault(option => option.OptionBehaviour == stringOption);
        if (option == null) return true;
        stringOption.OnValueChanged = new Action<OptionBehaviour>(_ => { });
        stringOption.TitleText.text = option.DisplayNameWithIndentation;
        stringOption.Value = stringOption.oldValue = option.SelectionIndex;
        stringOption.ValueText.text = option.StringSelections[option.SelectionIndex];

        return false;
    }

    private static bool CustomOptionEnable(ToggleOption boolOption)
    {
        var option = CustomOption.Tab.Options.FirstOrDefault(option => option.OptionBehaviour == boolOption);
        if (option == null) return true;
        boolOption.OnValueChanged = new Action<OptionBehaviour>(_ => { });
        boolOption.TitleText.text = option.DisplayNameWithIndentation;
        boolOption.CheckMark.enabled = boolOption.oldValue = option;

        return false;
    }

    private static bool CustomOptionIncrease(StringOption stringOption)
    {
        var option = CustomOption.Tab.Options.FirstOrDefault(option => option.OptionBehaviour == stringOption);
        if (option == null) return true;
        option.UpdateSelection(option.SelectionIndex + 1);
        return false;
    }

    private static bool CustomOptionToggle(ToggleOption boolOption)
    {
        var option = CustomOption.Tab.Options.FirstOrDefault(option => option.OptionBehaviour == boolOption);
        if (option == null) return true;
        option.UpdateSelection(option.SelectionIndex + 1);
        return false;
    }

    private static bool CustomOptionDecrease(StringOption stringOption)
    {
        var option = CustomOption.Tab.Options.FirstOrDefault(option => option.OptionBehaviour == stringOption);
        if (option == null) return true;
        option.UpdateSelection(option.SelectionIndex - 1);
        return false;
    }

    private static void ShareCustomOptions()
    {
        CustomOption.Tab.ShareCustomOptions();
    }

    private static void CustomOptionMenuUpdate(GameOptionsMenu optionsMenu)
    {
        var gameSettingMenu = UnityEngine.Object.FindObjectsOfType<GameSettingMenu>().FirstOrDefault();
        if (gameSettingMenu != null && (gameSettingMenu.RegularGameSettings.active ||
                                        gameSettingMenu.RolesSettings.gameObject.active)) return;
        optionsMenu.GetComponentInParent<Scroller>().ContentYBounds.max = -0.5F + optionsMenu.Children.Length * 0.55F;
        timer += Time.deltaTime;
        if (timer < 0.1f) return;
        timer = 0f;

        foreach (var cst in CustomOption.Tab.Tabs)
        {
            var offset = 2.75f;
            foreach (var setting in cst.Settings)
            {
                if (setting.OptionBehaviour == null || setting.OptionBehaviour.gameObject == null) continue;
                var enabled = true;
                var parent = setting.Parent;
                while (parent != null && enabled)
                {
                    enabled = parent.SelectionIndex != 0;
                    parent = parent.Parent;
                }

                setting.OptionBehaviour.gameObject.SetActive(enabled);
                if (!enabled) continue;
                offset -= setting.IsHeader ? 0.75f : 0.5f;
                var transform = setting.OptionBehaviour.transform;
                var localPosition = transform.localPosition;
                transform.localPosition = new Vector3(localPosition.x, offset, localPosition.z);
            }
        }
    }

    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
    public static class GameOptionsMenuStartPatch
    {
        public static void Postfix(GameOptionsMenu __instance)
        {
            CreateCustomTabs(__instance);
            AdaptTasksCount(__instance);
        }
    }
    
    private static void AdaptTasksCount(GameOptionsMenu gameOptionsMenu)
    {
        var commonTasksOption = gameOptionsMenu.Children.FirstOrDefault(x => x.name == "NumCommonTasks")?.TryCast<NumberOption>();
        if (commonTasksOption != null) commonTasksOption.ValidRange = new FloatRange(0f, 4f);

        var shortTasksOption = gameOptionsMenu.Children.FirstOrDefault(x => x.name == "NumShortTasks")?.TryCast<NumberOption>();
        if (shortTasksOption != null) shortTasksOption.ValidRange = new FloatRange(0f, 23f);

        var longTasksOption = gameOptionsMenu.Children.FirstOrDefault(x => x.name == "NumLongTasks")?.TryCast<NumberOption>();
        if (longTasksOption != null) longTasksOption.ValidRange = new FloatRange(0f, 15f);
    }
    
    [HarmonyPatch(typeof(ToggleOption), nameof(ToggleOption.OnEnable))]
    public static class ToggleOptionOnEnablePatch
    {
        public static bool Prefix(ToggleOption __instance)
        {
            return CustomOptionEnable(__instance);
        }
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.OnEnable))]
    public static class StringOptionOnEnablePatch
    {
        public static bool Prefix(StringOption __instance)
        {
            return CustomOptionEnable(__instance);
        }
    }
    
    [HarmonyPatch(typeof(ToggleOption), nameof(ToggleOption.Toggle))]
    public static class ToggleOptionTogglePatch
    {
        public static bool Prefix(ToggleOption __instance)
        {
            return CustomOptionToggle(__instance);
        }
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Increase))]
    public static class StringOptionIncreasePatch
    {
        public static bool Prefix(StringOption __instance)
        {
            return CustomOptionIncrease(__instance);
        }
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Decrease))]
    public static class StringOptionDecreasePatch
    {
        public static bool Prefix(StringOption __instance)
        {
            return CustomOptionDecrease(__instance);
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
    public static class RpcSyncSettingsPatch
    {
        public static void Postfix(PlayerControl __instance)
        {
            DeferrableAction.Defer(ShareCustomOptions, () => CachedPlayer.LocalPlayer != null && __instance != null);
        }
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.CoSpawnPlayer))]
    public static class AmongUsClientOnPlayerJoinedPatch
    {
        public static void Postfix(PlayerPhysics __instance)
        {
            DeferrableAction.Defer(ShareCustomOptions, () => CachedPlayer.LocalPlayer != null && __instance.myPlayer != null);
        }
    }

    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
    public class GameOptionsMenuUpdatePatch
    {
        public static void Postfix(GameOptionsMenu __instance)
        {
            CustomOptionMenuUpdate(__instance);
        }
    }
}