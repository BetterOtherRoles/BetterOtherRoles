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
using TheOtherRoles.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheOtherRoles.EnoFw;

public class RoleDescriptionComponent : MonoBehaviour
{
    public static RoleDescriptionComponent Instance { get; private set; }
    
    public Rect windowRect;

    private bool _opened = false;
    private static bool RoleDescriptionIsOpen =>
        roleDescriptionDisplay != null && roleDescriptionDisplay.gameObject != null;
    
    private static Minigame roleDescriptionDisplay;
    
    private readonly Dictionary<int, Vector2> TabScrolls = new();
    private readonly Dictionary<int, Vector2> TabEditorScrolls = new();

    private int _tabSelection;
    private Page _currentPage = Page.ShowRoleDescription;

    private string _lastSceneName = "";

    private Texture2D _darkTexture;
    private Texture2D _primaryTexture;
    private Texture2D _secondaryTexture;
    private Texture2D _cleanTexture;

    private enum Page
    {
        ShowRoleDescription
    }

    public RoleDescriptionComponent()
    {
        // Initialize ? Or should I do a Begin ?
        Instance = this;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        if (!_opened)
        {
            _opened = CustomOption.Tab.Tabs.Count > 0 && RoleDescriptionIsOpen;
        }
        else
        {
            if (Minigame.Instance != null)
            {
                roleDescriptionDisplay.Close();
            }
            
            _opened = false;
        }
    }
    
    private void NextPage()
    {
        // TODO : Adapt.
        //
        // if (_tabSelection == CustomOption.Tab.Tabs.Count - 1)
        // {
        //     _tabSelection = 0;
        // }
        // else
        // {
        //     _tabSelection++;
        // }
    }

    private void PreviousPage()
    {
        // TODO : Adapt.
        //
        // if (_tabSelection == 0)
        // {
        //     _tabSelection = CustomOption.Tab.Tabs.Count - 1;
        // }
        // else
        // {
        //     _tabSelection--;
        // }
    }
    
    public void OnGUI()
    {
        if (!_opened) return;
        
        if (FastDestroyableSingleton<HudManager>.Instance.FullScreen == null && AmongUsClient.Instance.GameState is InnerNetClient.GameStates.Started && GameOptionsManager.Instance.currentGameOptions.GameMode != GameModes.HideNSeek);
        if (Camera.main == null || Minigame.Instance != null) return;
        
        if (roleDescriptionDisplay == null) InitializeMainDisplay();
        
        var roleTransform = roleDescriptionDisplay.transform;

        roleTransform.SetParent(Camera.main.transform, false);
        roleTransform.localPosition = new Vector3(0.0f, (-0.66f * 2), -100);
        roleDescriptionDisplay.Begin(null);

        var bounds = roleDescriptionDisplay.gameObject.GetComponent<MeshRenderer>().bounds;
        
        windowRect = new(bounds.min.x, bounds.min.y, bounds.max.x, bounds.max.y);
        
        InitTextures();
        GUILayout.Window(0, windowRect, (GUI.WindowFunction)WindowFunction, "");
    }
    
    private static void InitializeMainDisplay()
    {
        TheOtherRolesPlugin.Logger.LogMessage($"Initialize roledescription UI");
        // var vitalsObj = GameObject.FindObjectsOfType<SystemConsole>().ToList().Find(console => console.name == "panel_vitals");
        var vitalsObj = UnityEngine.Object.FindObjectsOfType<SystemConsole>()
            .FirstOrDefault(x => x.gameObject.name.Contains("panel_vitals"));
        var allRoleInfo = RoleInfo.getRoleInfoForPlayer(CachedPlayer.LocalPlayer);
        var isGuesser = HandleGuesser.isGuesserGm && HandleGuesser.isGuesser(CachedPlayer.LocalPlayer.PlayerId);

        if (Camera.main == null || vitalsObj == null) return;
        
        roleDescriptionDisplay = Instantiate((Object)vitalsObj.MinigamePrefab, Camera.main.transform, false)
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
    
    public void WindowFunction(int windowID)
    {
        GUI.Box(new Rect(0f, 0f, 500f, 40f), _primaryTexture);
        GUI.Label(new Rect(10f, 5f, 460f, 30f),
            $"{CredentialsPatch.FullCredentialsText} {CredentialsPatch.FullVersionText}",
            ImGUI.Styles.Instance.TitleLabel);
        
        if (_currentPage is Page.ShowRoleDescription)
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
            case Page.ShowRoleDescription:
                RenderShowRoleDescriptionShower();
                break;
        }
        
    }
    
    private void RenderShowRoleDescriptionShower()
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
}