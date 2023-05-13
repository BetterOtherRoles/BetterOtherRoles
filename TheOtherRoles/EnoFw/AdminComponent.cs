using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using InnerNet;
using TheOtherRoles.EnoFw.Kernel;
using TheOtherRoles.EnoFw.Utils;
using TheOtherRoles.Modules;
using TheOtherRoles.Patches;
using TheOtherRoles.Players;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheOtherRoles.EnoFw;

public class AdminComponent : MonoBehaviour
{
    public static AdminComponent Instance { get; private set; }

    public Rect windowRect = new(0, 0, 500, Screen.height);

    private bool _opened = false;

    private readonly Dictionary<int, Vector2> TabScrolls = new();
    private readonly Dictionary<int, Vector2> TabEditorScrolls = new();

    private int _tabSelection;
    private Page _currentPage = Page.ShowCustomOptions;

    private string _lastSceneName = "";

    private Texture2D _darkTexture;
    private Texture2D _primaryTexture;
    private Texture2D _secondaryTexture;
    private Texture2D _cleanTexture;

    private enum Page
    {
        ShowCustomOptions,
        EditCustomOptions,
        ManageLobbyColors
    }

    public AdminComponent()
    {
        Instance = this;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.F4))
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        if (!_opened)
        {
            _opened = CustomOption.Tab.Tabs.Count > 0;
        }
        else
        {
            _opened = false;
        }
    }

    private void NextPage()
    {
        if (_tabSelection == CustomOption.Tab.Tabs.Count - 1)
        {
            _tabSelection = 0;
        }
        else
        {
            _tabSelection++;
        }
    }

    private void PreviousPage()
    {
        if (_tabSelection == 0)
        {
            _tabSelection = CustomOption.Tab.Tabs.Count - 1;
        }
        else
        {
            _tabSelection--;
        }
    }

    public void OnGUI()
    {
        if (!_opened) return;
        InitTextures();
        GUILayout.Window(0, windowRect, (GUI.WindowFunction)WindowFunction, "");
    }

    private void InitTextures()
    {
        if (SceneManager.GetActiveScene().name != _lastSceneName)
        {
            _lastSceneName = SceneManager.GetActiveScene().name;

            _darkTexture = new Texture2D(1, 1);
            _darkTexture.SetPixel(0, 0, ImGUI.DarkColor);
            _darkTexture.Apply(true, false);
            _primaryTexture = new Texture2D(1, 1);
            _primaryTexture.SetPixel(0, 0, ImGUI.PrimaryColor);
            _primaryTexture.Apply(true, false);
            _secondaryTexture = new Texture2D(1, 1);
            _secondaryTexture.SetPixel(0, 0, ImGUI.SecondaryColor);
            _secondaryTexture.Apply(true, false);
            _cleanTexture = new Texture2D(1, 1);
            _cleanTexture.SetPixel(0, 0, Color.clear);
            _cleanTexture.Apply(true, false);
        }
        //GUI.skin.verticalScrollbar = ImGUI.Styles.Instance.VerticalScrollbar;
        //GUI.skin.verticalScrollbarThumb = ImGUI.Styles.Instance.VerticalScrollbarThumb;

        //GUI.skin.scrollView.padding.left = 15;
        GUI.skin.window.normal.background = _darkTexture;
        GUI.skin.window.active.background = _darkTexture;
        GUI.skin.window.hover.background = _darkTexture;
        GUI.skin.window.focused.background = _darkTexture;
        GUI.skin.window.onActive.background = _darkTexture;
        GUI.skin.window.onFocused.background = _darkTexture;
        GUI.skin.window.onHover.background = _darkTexture;
        GUI.skin.window.onNormal.background = _darkTexture;

        GUI.skin.verticalScrollbar.normal.background = _primaryTexture;
        GUI.skin.verticalScrollbar.active.background = _primaryTexture;
        GUI.skin.verticalScrollbar.hover.background = _primaryTexture;
        GUI.skin.verticalScrollbar.focused.background = _primaryTexture;

        GUI.skin.verticalScrollbarThumb.normal.background = _secondaryTexture;
        GUI.skin.verticalScrollbarThumb.active.background = _secondaryTexture;
        GUI.skin.verticalScrollbarThumb.hover.background = _secondaryTexture;
        GUI.skin.verticalScrollbarThumb.focused.background = _secondaryTexture;
    }

    private void InitMenuTextures()
    {
        GUI.skin.button.normal.background = _primaryTexture;
        GUI.skin.button.active.background = _primaryTexture;
        GUI.skin.button.hover.background = _primaryTexture;
        GUI.skin.button.focused.background = _primaryTexture;
        GUI.skin.button.onNormal.background = _primaryTexture;
        GUI.skin.button.onActive.background = _primaryTexture;
        GUI.skin.button.onHover.background = _primaryTexture;
        GUI.skin.button.onFocused.background = _primaryTexture;
        GUI.skin.button.padding.top = 5;
        GUI.skin.button.padding.bottom = 5;
        GUI.skin.button.fontSize = 35;
        GUI.skin.button.fontStyle = FontStyle.Bold;
    }

    private void InitCloseButtonTexture()
    {
        GUI.skin.button.normal.background = _cleanTexture;
        GUI.skin.button.active.background = _cleanTexture;
        GUI.skin.button.hover.background = _cleanTexture;
        GUI.skin.button.focused.background = _cleanTexture;
        GUI.skin.button.onNormal.background = _cleanTexture;
        GUI.skin.button.onActive.background = _cleanTexture;
        GUI.skin.button.onHover.background = _cleanTexture;
        GUI.skin.button.onFocused.background = _cleanTexture;
        GUI.skin.button.fontSize = 18;
    }

    public void InitMenu()
    {
        if (CachedPlayer.LocalPlayer == null || !CustomGuid.IsAdmin(CachedPlayer.LocalPlayer)) return;
        GUI.skin.button.fontSize = 11;
        if (GUI.Button(new Rect(5f, 50f, 150f, 30f), "Show custom options"))
        {
            _currentPage = Page.ShowCustomOptions;
        }

        if (GUI.Button(new Rect(160f, 50f, 150f, 30f), "Edit custom options"))
        {
            _currentPage = Page.EditCustomOptions;
        }

        if (GUI.Button(new Rect(315f, 50f, 150f, 30f), "Manage lobby colors"))
        {
            _currentPage = Page.ManageLobbyColors;
        }
    }

    public void WindowFunction(int windowID)
    {
        GUI.Box(new Rect(0f, 0f, 500f, 40f), _primaryTexture);
        GUI.Label(new Rect(10f, 5f, 460f, 30f),
            $"{CredentialsPatch.FullCredentialsText} {CredentialsPatch.FullVersionText}",
            ImGUI.Styles.Instance.TitleLabel);
        InitMenuTextures();
        InitMenu();
        if (_currentPage is Page.ShowCustomOptions or Page.EditCustomOptions)
        {
            GUI.skin.button.fontSize = 18;
            if (GUI.Button(new Rect(400f, 85f, 30f, 30f), "\uffe9")) PreviousPage();
            if (GUI.Button(new Rect(450f, 85f, 30f, 30f), "\uffeb")) NextPage();
        }

        InitCloseButtonTexture();
        if (GUI.Button(new Rect(460f, 0f, 40f, 40f), "╳"))
        {
            _opened = false;
        }

        GUILayout.Space(65f);
        switch (_currentPage)
        {
            default:
            case Page.ShowCustomOptions:
                RenderCustomOptionsShower();
                break;
            case Page.EditCustomOptions:
                RenderCustomOptionsEditor();
                break;
            case Page.ManageLobbyColors:
                RenderLobbyColorsManager();
                break;
        }
    }

    private void RenderLobbyColorsManager()
    {
        GUILayout.Label("Not yet implemented", ImGUI.Styles.Instance.HeadingLabel);
    }

    private void RenderCustomOptionsEditor()
    {
        GUILayout.Label("Not yet implemented", ImGUI.Styles.Instance.HeadingLabel);
        /*
        GUILayout.Label(CustomOption.Tab.Tabs[_tabSelection].Title, ImGUI.Styles.Instance.HeadingLabel);
        GUILayout.Space(20f);
        TabEditorScrolls[_tabSelection] =
            GUILayout.BeginScrollView(!TabEditorScrolls.ContainsKey(_tabSelection)
                ? Vector2.zero
                : TabEditorScrolls[_tabSelection]);
        RenderTabEditor(CustomOption.Tab.Tabs[_tabSelection]);
        GUILayout.EndScrollView();
        */
    }

    private void RenderCustomOptionsShower()
    {
        GUILayout.Label(CustomOption.Tab.Tabs[_tabSelection].Title, ImGUI.Styles.Instance.HeadingLabel);
        GUILayout.Space(20f);
        TabScrolls[_tabSelection] =
            GUILayout.BeginScrollView(!TabScrolls.ContainsKey(_tabSelection)
                ? Vector2.zero
                : TabScrolls[_tabSelection]);
        RenderTab(CustomOption.Tab.Tabs[_tabSelection]);
        GUILayout.EndScrollView();
    }
    
    private static void RenderTabEditor(CustomOption.Tab tab)
    {
        foreach (var option in tab.Settings.Where(o =>
                     o.Parent == null && o.AllowedGameModes.Contains(CustomOption.CurrentGameMode)))
        {
            RenderOptionEditor(option);
        }
    }
    
    private static void RenderOptionEditor(CustomOption option, int deep = 0)
    {
        var prefix = option.IsHeader ? "\n" : string.Empty;
        if (deep > 0)
        {
            for (var i = 0; i < deep; i++)
            {
                prefix += "  ";
            }
        }

        if (option.Type == CustomOption.OptionType.Boolean)
        {
            var value = GUILayout.Toggle((bool)option, option.Name);
            if (value != (bool)option)
            {
                option.UpdateSelection(option.SelectionIndex + 1);
            }
        }
        else
        {
            var value = GUILayout.SelectionGrid(option.SelectionIndex, (Il2CppStringArray)option.StringSelections.ToArray(), 4);
            if (value != option.SelectionIndex)
            {
                option.UpdateSelection(value);
            }
        }
        
        if (option.SelectionIndex == 0 || !option.HasChildren) return;
        foreach (var subOption in option.Children.Where(o => o.AllowedGameModes.Contains(CustomOption.CurrentGameMode)))
        {
            RenderOptionEditor(subOption, deep + 1);
        }
    }

    private static void RenderTab(CustomOption.Tab tab)
    {
        foreach (var option in tab.Settings.Where(o =>
                     o.Parent == null && o.AllowedGameModes.Contains(CustomOption.CurrentGameMode)))
        {
            RenderOption(option);
        }
    }

    private static void RenderOption(CustomOption option, int deep = 0)
    {
        var prefix = option.IsHeader ? "\n" : string.Empty;
        if (deep > 0)
        {
            for (var i = 0; i < deep; i++)
            {
                prefix += "  ";
            }
        }

        GUILayout.Label($"{prefix}{option.Name}: {option.DisplayValue}", ImGUI.Styles.Instance.OptionLabel);
        if (option.SelectionIndex == 0 || !option.HasChildren) return;
        foreach (var subOption in option.Children.Where(o => o.AllowedGameModes.Contains(CustomOption.CurrentGameMode)))
        {
            RenderOption(subOption, deep + 1);
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    internal static class HudManagerUpdatePatch
    {
        private static PassiveButton _toggleSettingsButton;
        private static GameObject _toggleSettingsButtonObject;
        
        private static void Postfix(HudManager __instance)
        {
            if (__instance == null || __instance.MapButton == null || AmongUsClient.Instance == null || GameOptionsManager.Instance == null || CachedPlayer.LocalPlayer == null) return;
            if (!_toggleSettingsButton || !_toggleSettingsButtonObject) {
                _toggleSettingsButtonObject = Instantiate(__instance.MapButton.gameObject, __instance.MapButton.transform.parent);
                _toggleSettingsButtonObject.transform.localPosition = __instance.MapButton.transform.localPosition + new Vector3(0, -0.66f, -500f);
                var renderer = _toggleSettingsButtonObject.GetComponent<SpriteRenderer>();
                renderer.sprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.CurrentSettingsButton.png", 180f);
                _toggleSettingsButton = _toggleSettingsButtonObject.GetComponent<PassiveButton>();
                _toggleSettingsButton.OnClick.RemoveAllListeners();
                _toggleSettingsButton.OnClick.AddListener((System.Action)Instance.Toggle);
            }

            var active =
                AmongUsClient.Instance.GameState is InnerNetClient.GameStates.Joined
                    or InnerNetClient.GameStates.Started && GameOptionsManager.Instance.currentGameOptions.GameMode != GameModes.HideNSeek;
            _toggleSettingsButtonObject.SetActive(active);
            if (__instance.MapButton.gameObject.active)
            {
                _toggleSettingsButtonObject.transform.localPosition = __instance.MapButton.transform.localPosition + new Vector3(0, -0.66f, -500f);
            }
            else
            {
                _toggleSettingsButtonObject.transform.localPosition = __instance.MapButton.transform.localPosition;
            }
        }
    }
}