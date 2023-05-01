using System.Collections.Generic;
using UnityEngine;
using BepInEx.Configuration;
using System;
using System.Linq;
using HarmonyLib;
using Hazel;
using System.Text;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;
using Reactor.Utilities.Extensions;
using AmongUs.GameOptions;
using TheOtherRoles.EnoFramework;
using TheOtherRoles.EnoFramework.Kernel;
using Object = UnityEngine.Object;

namespace TheOtherRoles {
    public class CustomOption {
        public enum CustomOptionType {
            General,
            Impostor,
            Neutral,
            Crewmate,
            Modifier,
            Guesser,
            HideNSeekMain,
            HideNSeekRoles
        }

        public static List<CustomOption> options = new List<CustomOption>();
        public static int preset = 0;
        public static ConfigEntry<string> vanillaSettings;

        public int id;
        public string name;
        public System.Object[] selections;

        public int defaultSelection;
        public ConfigEntry<int> entry;
        public int selection;
        public OptionBehaviour optionBehaviour;
        public CustomOption parent;
        public bool isHeader;
        public CustomOptionType type;

        // Option creation

        public CustomOption(int id, CustomOptionType type, string name,  System.Object[] selections, System.Object defaultValue, CustomOption parent, bool isHeader) {
            this.id = id;
            this.name = parent == null ? name : "- " + name;
            this.selections = selections;
            int index = Array.IndexOf(selections, defaultValue);
            this.defaultSelection = index >= 0 ? index : 0;
            this.parent = parent;
            this.isHeader = isHeader;
            this.type = type;
            selection = 0;
            if (id != 0) {
                entry = TheOtherRolesPlugin.Instance.Config.Bind($"Preset{preset}", id.ToString(), defaultSelection);
                selection = Mathf.Clamp(entry.Value, 0, selections.Length - 1);
            }
            options.Add(this);
        }

        public static CustomOption Create(int id, CustomOptionType type, string name, string[] selections, CustomOption parent = null, bool isHeader = false) {
            return new CustomOption(id, type, name, selections, "", parent, isHeader);
        }

        public static CustomOption Create(int id, CustomOptionType type, string name, float defaultValue, float min, float max, float step, CustomOption parent = null, bool isHeader = false) {
            List<object> selections = new();
            for (float s = min; s <= max; s += step)
                selections.Add(s);
            return new CustomOption(id, type, name, selections.ToArray(), defaultValue, parent, isHeader);
        }

        public static CustomOption Create(int id, CustomOptionType type, string name, bool defaultValue, CustomOption parent = null, bool isHeader = false) {
            return new CustomOption(id, type, name, new string[]{"Off", "On"}, defaultValue ? "On" : "Off", parent, isHeader);
        }

        // Static behaviour

        public static void switchPreset(int newPreset) {
            saveVanillaOptions();
            CustomOption.preset = newPreset;
            vanillaSettings = TheOtherRolesPlugin.Instance.Config.Bind($"Preset{preset}", "GameOptions", "");
            loadVanillaOptions();
            foreach (CustomOption option in CustomOption.options) {
                if (option.id == 0) continue;

                option.entry = TheOtherRolesPlugin.Instance.Config.Bind($"Preset{preset}", option.id.ToString(), option.defaultSelection);
                option.selection = Mathf.Clamp(option.entry.Value, 0, option.selections.Length - 1);
                if (option.optionBehaviour != null && option.optionBehaviour is StringOption stringOption) {
                    stringOption.oldValue = stringOption.Value = option.selection;
                    stringOption.ValueText.text = option.selections[option.selection].ToString();
                }
            }
        }

        public static void saveVanillaOptions() {
            vanillaSettings.Value = Convert.ToBase64String(GameOptionsManager.Instance.gameOptionsFactory.ToBytes(GameManager.Instance.LogicOptions.currentGameOptions));
        }

        public static void loadVanillaOptions() {
            string optionsString = vanillaSettings.Value;
            if (optionsString == "") return;
            GameOptionsManager.Instance.GameHostOptions = GameOptionsManager.Instance.gameOptionsFactory.FromBytes(Convert.FromBase64String(optionsString));
            GameOptionsManager.Instance.CurrentGameOptions = GameOptionsManager.Instance.GameHostOptions;
            GameManager.Instance.LogicOptions.SetGameOptions(GameOptionsManager.Instance.CurrentGameOptions);
            GameManager.Instance.LogicOptions.SyncOptions();
        }

        public static void ShareOptionChange(uint optionId) {
            var option = options.FirstOrDefault(x => x.id == optionId);
            if (option == null) return;
            var writer = AmongUsClient.Instance!.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.ShareOptions, SendOption.Reliable, -1);
            writer.Write((byte)1);
            writer.WritePacked((uint)option.id);
            writer.WritePacked(Convert.ToUInt32(option.selection));
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void ShareOptionSelections() {
            if (CachedPlayer.AllPlayers.Count <= 1 || AmongUsClient.Instance!.AmHost == false && CachedPlayer.LocalPlayer.PlayerControl == null) return;
            var optionsList = new List<CustomOption>(CustomOption.options);
            while (optionsList.Any())
            {
                byte amount = (byte) Math.Min(optionsList.Count, 200); // takes less than 3 bytes per option on average
                var writer = AmongUsClient.Instance!.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.ShareOptions, SendOption.Reliable, -1);
                writer.Write(amount);
                for (int i = 0; i < amount; i++)
                {
                    var option = optionsList[0];
                    optionsList.RemoveAt(0);
                    writer.WritePacked((uint) option.id);
                    writer.WritePacked(Convert.ToUInt32(option.selection));
                }
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
        }

        // Getter

        public int getSelection() {
            return selection;
        }

        public bool getBool() {
            return selection > 0;
        }

        public float getFloat() {
            return (float)selections[selection];
        }

        public int getQuantity() {
            return selection + 1;
        }

        // Option changes

        public void updateSelection(int newSelection) {
            selection = Mathf.Clamp((newSelection + selections.Length) % selections.Length, 0, selections.Length - 1);
            if (optionBehaviour != null && optionBehaviour is StringOption stringOption) {
                stringOption.oldValue = stringOption.Value = selection;
                stringOption.ValueText.text = selections[selection].ToString();

                if (AmongUsClient.Instance?.AmHost == true && CachedPlayer.LocalPlayer.PlayerControl) {
                    if (id == 0 && selection != preset) {
                        switchPreset(selection); // Switch presets
                        ShareOptionSelections();
                    } else if (entry != null) {
                        entry.Value = selection; // Save selection to config
                        ShareOptionChange((uint)id);// Share single selection
                    }
                }
            } else if (id == 0 && AmongUsClient.Instance?.AmHost == true && PlayerControl.LocalPlayer) {  // Share the preset switch for random maps, even if the menu isnt open!
                switchPreset(selection);
                ShareOptionSelections();// Share all selections
            }
        }
    }

    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
    class GameOptionsMenuStartPatch {
        public static void Postfix(GameOptionsMenu __instance) {

            switch (TORMapOptions.gameMode) {
                case CustomGamemodes.HideNSeek:
                    // createHideNSeekTabs(__instance);
                    adaptTaskCount(__instance);
                    break;
                case CustomGamemodes.Classic:
                case CustomGamemodes.Guesser:
                default:
                    adaptTaskCount(__instance);
                    break;
            }
        }

        private static void adaptTaskCount(GameOptionsMenu __instance) {
            // Adapt task count for main options
            var commonTasksOption = __instance.Children.FirstOrDefault(x => x.name == "NumCommonTasks").TryCast<NumberOption>();
            if (commonTasksOption != null) commonTasksOption.ValidRange = new FloatRange(0f, 4f);

            var shortTasksOption = __instance.Children.FirstOrDefault(x => x.name == "NumShortTasks").TryCast<NumberOption>();
            if (shortTasksOption != null) shortTasksOption.ValidRange = new FloatRange(0f, 23f);

            var longTasksOption = __instance.Children.FirstOrDefault(x => x.name == "NumLongTasks").TryCast<NumberOption>();
            if (longTasksOption != null) longTasksOption.ValidRange = new FloatRange(0f, 15f);
        }
    }


    [HarmonyPatch] 
    class GameOptionsDataPatch
    {
        /*private static IEnumerable<MethodBase> TargetMethods() {
            return typeof(IGameOptionsExtensions.).GetMethods().Where(x => x.ReturnType == typeof(string) && x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType == typeof(int));
        }*/

        private static string BuildOptionsOfTab(EnoFramework.CustomOption.Tab tab, bool headersOnly)
        {
            var result = new StringBuilder("\n");

            if (headersOnly)
            {
                foreach (var option in tab.Settings.Where(option => option.Parent == null))
                {
                    result.AppendLine($"{option.Name}: {(string)option}");
                }

                return result.ToString();
            }

            foreach (var option in tab.Settings)
            {
                if (option.Parent != null)
                {
                    if (option.Parent == false ||
                        (option.Parent.Parent != null && option.Parent.Parent == false)) continue;
                    result.AppendLine($"{option.Name}: {(string)option}");
                    continue;
                }
                result.AppendLine($"{option.Name}: {(string)option}");
            }
            

            return result.ToString();
        }

        public static string buildAllOptions(string vanillaSettings = "", bool hideExtras = false) {
            if (vanillaSettings == "")
                vanillaSettings = GameOptionsManager.Instance.CurrentGameOptions.ToHudString(PlayerControl.AllPlayerControls.Count);
            var counter = TheOtherRolesPlugin.optionsPage;
            var hudString = counter != 0 && !hideExtras ? Helpers.cs(DateTime.Now.Second % 2 == 0 ? Color.white : Color.red, "(Use scroll wheel if necessary)\n\n") : "";
            const int maxPage = 6;
            switch (counter) {
                case 0:
                    hudString += (!hideExtras ? "" :"Page 1: Vanilla Settings \n\n") + vanillaSettings;
                    break;
                case 1:
                    hudString += "Page 2: Better Other Roles Settings \n" + BuildOptionsOfTab(Singleton<CustomOptionsHolder>.Instance.MainSettings, false);
                    break;
                case 2:
                    hudString += "Page 3: Impostor Role Settings \n" + BuildOptionsOfTab(Singleton<CustomOptionsHolder>.Instance.ImpostorsSettings, false);
                    break;
                case 3:
                    hudString += "Page 4: Neutral Role Settings \n" + BuildOptionsOfTab(Singleton<CustomOptionsHolder>.Instance.NeutralSettings, false);
                    break;
                case 4:
                    hudString += "Page 5: Crewmate Role Settings \n" + BuildOptionsOfTab(Singleton<CustomOptionsHolder>.Instance.CrewmateSettings, false);
                    break;
                case 5:
                    hudString += "Page 6: Modifier Settings \n" + BuildOptionsOfTab(Singleton<CustomOptionsHolder>.Instance.ModifierSettings, false);
                    break;
            }

            if (!hideExtras || counter != 0) hudString += $"\n Press TAB or Page Number for more... ({counter + 1}/{maxPage})";
            return hudString;
        }


        [HarmonyPatch(typeof(IGameOptionsExtensions), nameof(IGameOptionsExtensions.ToHudString))]
        private static void Postfix(ref string __result)
        {
            if (GameOptionsManager.Instance.currentGameOptions.GameMode == AmongUs.GameOptions.GameModes.HideNSeek) return; // Allow Vanilla Hide N Seek
            __result = buildAllOptions(vanillaSettings:__result);
        }
    }

    [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
    public static class GameOptionsNextPagePatch
    {
        public static void Postfix(KeyboardJoystick __instance)
        {
            int page = TheOtherRolesPlugin.optionsPage;
            if (Input.GetKeyDown(KeyCode.Tab)) {
                TheOtherRolesPlugin.optionsPage = (TheOtherRolesPlugin.optionsPage + 1) % 7;
            }
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) {
                TheOtherRolesPlugin.optionsPage = 0;
            }
            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) {
                TheOtherRolesPlugin.optionsPage = 1;
            }
            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) {
                TheOtherRolesPlugin.optionsPage = 2;
            }
            if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) {
                TheOtherRolesPlugin.optionsPage = 3;
            }
            if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5)) {
                TheOtherRolesPlugin.optionsPage = 4;
            }
            if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6)) {
                TheOtherRolesPlugin.optionsPage = 5;
            }
            if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7)) {
                TheOtherRolesPlugin.optionsPage = 6;
            }
            if (Input.GetKeyDown(KeyCode.F1))
                HudManagerUpdate.ToggleSettings(HudManager.Instance);

            if (page != TheOtherRolesPlugin.optionsPage) {
                Vector3 position = (Vector3)FastDestroyableSingleton<HudManager>.Instance?.GameSettings?.transform.localPosition;
                FastDestroyableSingleton<HudManager>.Instance.GameSettings.transform.localPosition = new Vector3(position.x, 2.9f, position.z);
            }
        }
    }

    
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class GameSettingsScalePatch {
        public static void Prefix(HudManager __instance) {
            if (__instance.GameSettings != null) __instance.GameSettings.fontSize = 1.2f; 
        }
    }


    // This class is taken and adapted from Town of Us Reactivated, https://github.com/eDonnes124/Town-Of-Us-R/blob/master/source/Patches/CustomOption/Patches.cs, Licensed under GPLv3
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class HudManagerUpdate {
        public static float
            MinX,/*-5.3F*/
            OriginalY = 2.9F,
            MinY = 2.9F;


        public static Scroller Scroller;
        private static Vector3 LastPosition;
        private static float lastAspect;
        private static bool setLastPosition = false;

        public static void Prefix(HudManager __instance) {
            if (__instance.GameSettings?.transform == null) return;

            // Sets the MinX position to the left edge of the screen + 0.1 units
            Rect safeArea = Screen.safeArea;
            float aspect = Mathf.Min((Camera.main).aspect, safeArea.width / safeArea.height);
            float safeOrthographicSize = CameraSafeArea.GetSafeOrthographicSize(Camera.main);
            MinX = 0.1f - safeOrthographicSize * aspect;

            if (!setLastPosition || aspect != lastAspect) {
                LastPosition = new Vector3(MinX, MinY);
                lastAspect = aspect;
                setLastPosition = true;
                if (Scroller != null) Scroller.ContentXBounds = new FloatRange(MinX, MinX);                
            }

            CreateScroller(__instance);

            Scroller.gameObject.SetActive(__instance.GameSettings.gameObject.activeSelf);

            if (!Scroller.gameObject.active) return;

            var rows = __instance.GameSettings.text.Count(c => c == '\n');
            float LobbyTextRowHeight = 0.06F;
            var maxY = Mathf.Max(MinY, rows * LobbyTextRowHeight + (rows - 38) * LobbyTextRowHeight);

            Scroller.ContentYBounds = new FloatRange(MinY, maxY);

            // Prevent scrolling when the player is interacting with a menu
            if (CachedPlayer.LocalPlayer?.PlayerControl.CanMove != true) {
                __instance.GameSettings.transform.localPosition = LastPosition;

                return;
            }

            if (__instance.GameSettings.transform.localPosition.x != MinX ||
                __instance.GameSettings.transform.localPosition.y < MinY) return;

            LastPosition = __instance.GameSettings.transform.localPosition;
        }

        private static void CreateScroller(HudManager __instance) {
            if (Scroller != null) return;

            Transform target = __instance.GameSettings.transform;

            Scroller = new GameObject("SettingsScroller").AddComponent<Scroller>();
            Scroller.transform.SetParent(__instance.GameSettings.transform.parent);
            Scroller.gameObject.layer = 5;

            Scroller.transform.localScale = Vector3.one;
            Scroller.allowX = false;
            Scroller.allowY = true;
            Scroller.active = true;
            Scroller.velocity = new Vector2(0, 0);
            Scroller.ScrollbarYBounds = new FloatRange(0, 0);
            Scroller.ContentXBounds = new FloatRange(MinX, MinX);
            Scroller.enabled = true;

            Scroller.Inner = target;
            target.SetParent(Scroller.transform);
        }

        [HarmonyPrefix]
        public static void Prefix2(HudManager __instance) {
            if (!settingsTMPs[0]) return;
            foreach (var tmp in settingsTMPs) tmp.text = "";
            var settingsString = GameOptionsDataPatch.buildAllOptions(hideExtras: true);
            var blocks = settingsString.Split("\n\n", StringSplitOptions.RemoveEmptyEntries); ;
            var curString = "";
            var j = 0;
            foreach (var curBlock in blocks)
            {
                if (Helpers.lineCount(curBlock) + Helpers.lineCount(curString) < 40) {
                    curString += curBlock + "\n\n";
                } else {
                    settingsTMPs[j].text = curString;
                    j++;
                    
                    curString = "\n" + curBlock + "\n\n";
                    if (curString.Substring(0, 2) != "\n\n") curString = "\n" + curString;
                }
            }
            if (j < 3) settingsTMPs[j].text = curString;        
        }

        private static TMPro.TextMeshPro[] settingsTMPs = new TMPro.TextMeshPro[3];
        private static GameObject settingsBackground;
        public static void OpenSettings(HudManager __instance) {
            if (__instance.FullScreen == null || MapBehaviour.Instance && MapBehaviour.Instance.IsOpen
                /*|| AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started*/
                || GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return;
            settingsBackground = Object.Instantiate(__instance.FullScreen.gameObject, __instance.transform);
            settingsBackground.SetActive(true);
            var renderer = settingsBackground.GetComponent<SpriteRenderer>();
            renderer.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            renderer.enabled = true;
            for (int i = 0; i < 3; i++) {
                settingsTMPs[i] = Object.Instantiate(__instance.KillButton.cooldownTimerText, __instance.transform);
                settingsTMPs[i].alignment = TMPro.TextAlignmentOptions.TopLeft;
                settingsTMPs[i].enableWordWrapping = false;
                settingsTMPs[i].transform.localScale = Vector3.one * 0.25f;
                settingsTMPs[i].transform.localPosition += new Vector3(-4f + 3f * i, 1.8f, -500f);
                settingsTMPs[i].gameObject.SetActive(true);
            }
        }

        public static void CloseSettings() {
            foreach (var tmp in settingsTMPs)
                if (tmp) tmp.gameObject.Destroy();

            if (settingsBackground) settingsBackground.Destroy();
        }

        public static void ToggleSettings(HudManager __instance) {
            if (settingsTMPs[0]) CloseSettings();
            else OpenSettings(__instance);
        }

        static PassiveButton toggleSettingsButton;
        static GameObject toggleSettingsButtonObject;
        [HarmonyPostfix]
        public static void Postfix(HudManager __instance) {
            if (!toggleSettingsButton || !toggleSettingsButtonObject) {
                // add a special button for settings viewing:
                toggleSettingsButtonObject = GameObject.Instantiate(__instance.MapButton.gameObject, __instance.MapButton.transform.parent);
                toggleSettingsButtonObject.transform.localPosition = __instance.MapButton.transform.localPosition + new Vector3(0, -0.66f, -500f);
                SpriteRenderer renderer = toggleSettingsButtonObject.GetComponent<SpriteRenderer>();
                renderer.sprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.CurrentSettingsButton.png", 180f);
                toggleSettingsButton = toggleSettingsButtonObject.GetComponent<PassiveButton>();
                toggleSettingsButton.OnClick.RemoveAllListeners();
                toggleSettingsButton.OnClick.AddListener((Action)(() => ToggleSettings(__instance)));
            }
            toggleSettingsButtonObject.SetActive(__instance.MapButton.gameObject.active && !(MapBehaviour.Instance && MapBehaviour.Instance.IsOpen) && GameOptionsManager.Instance.currentGameOptions.GameMode != GameModes.HideNSeek);
            toggleSettingsButtonObject.transform.localPosition = __instance.MapButton.transform.localPosition + new Vector3(0, -0.66f, -500f);
        }
    }
}
