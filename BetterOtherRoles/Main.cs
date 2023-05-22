using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Il2CppSystem.Security.Cryptography;
using Il2CppSystem.Text;
using AmongUs.Data;
using BetterOtherRoles.EnoFw;
using BetterOtherRoles.EnoFw.Kernel;
using BetterOtherRoles.EnoFw.Libs.Reactor.Localization;
using BetterOtherRoles.EnoFw.Libs.Reactor.Localization.Providers;
using BetterOtherRoles.EnoFw.Libs.Reactor.Networking;
using BetterOtherRoles.EnoFw.Libs.Reactor.Networking.Attributes;
using BetterOtherRoles.EnoFw.Libs.Reactor.Networking.Rpc;
using BetterOtherRoles.EnoFw.Libs.Reactor.Patches.Miscellaneous;
using BetterOtherRoles.EnoFw.Libs.Reactor.Utilities;
using BetterOtherRoles.EnoFw.Libs.Reactor.Utilities.Attributes;
using BetterOtherRoles.EnoFw.Modules;
using BetterOtherRoles.EnoFw.Modules.BorApi;
using BetterOtherRoles.Modules;
using BetterOtherRoles.Patches;
using BetterOtherRoles.Players;
using BetterOtherRoles.Utilities;
using Il2CppInterop.Runtime.Attributes;

namespace BetterOtherRoles;

[BepInPlugin(Id, "Better Other Roles", VersionString)]
[BepInProcess("Among Us.exe")]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
public class BetterOtherRolesPlugin : BasePlugin
{
    public const string Id = "betterotherroles.eno.re";
    public const string VersionString = "1.3.0";

    public static Version Version = Version.Parse(VersionString);
    internal static BepInEx.Logging.ManualLogSource Logger;

    public Harmony Harmony { get; } = new(Id);
    public static BetterOtherRolesPlugin Instance;

    public static ConfigEntry<int> Preset { get; private set; }

    public static ConfigEntry<string> DebugMode { get; private set; }
    public static ConfigEntry<bool> GhostsSeeInformation { get; set; }
    public static ConfigEntry<bool> GhostsSeeRoles { get; set; }
    public static ConfigEntry<bool> GhostsSeeModifier { get; set; }
    public static ConfigEntry<bool> GhostsSeeVotes { get; set; }
    public static ConfigEntry<bool> ShowRoleSummary { get; set; }
    public static ConfigEntry<bool> ShowLighterDarker { get; set; }
    public static ConfigEntry<bool> EnableSoundEffects { get; set; }
    public static ConfigEntry<bool> EnableHorseMode { get; set; }
    public static ConfigEntry<string> Ip { get; set; }
    public static ConfigEntry<ushort> Port { get; set; }
    public static ConfigEntry<string> ShowPopUpVersion { get; set; }
    public static ConfigEntry<string> DevGuid { get; set; }

    public static ConfigEntry<string> FeaturesCodes { get; private set; }
    
    public CustomRpcManager CustomRpcManager { get; } = new();

    public BetterOtherRolesPlugin()
    {
        PluginSingleton<BetterOtherRolesPlugin>.Instance = this;
        PluginSingleton<BasePlugin>.Initialize();
        
        RegisterInIl2CppAttribute.Initialize();
        ModList.Initialize();
        
        RegisterCustomRpcAttribute.Initialize();
        MessageConverterAttribute.Initialize();
        MethodRpcAttribute.Initialize();
        
        LocalizationManager.Register(new HardCodedLocalizationProvider());
    }


    // This is part of the Mini.RegionInstaller, Licensed under GPLv3
    // file="RegionInstallPlugin.cs" company="miniduikboot">
    public static void UpdateRegions()
    {
        var serverManager = FastDestroyableSingleton<ServerManager>.Instance;

        var currentRegion = serverManager.CurrentRegion;
        Logger.LogInfo($"Adding {CustomRegions.Regions.Length} regions");
        foreach (var region in CustomRegions.Regions)
        {
            if (region == null)
                Logger.LogError("Could not add region");
            else
            {
                if (currentRegion != null && region.Name.Equals(currentRegion.Name, StringComparison.OrdinalIgnoreCase))
                    currentRegion = region;
                serverManager.AddOrUpdateRegion(region);
            }
        }

        // AU remembers the previous region that was set, so we need to restore it
        if (currentRegion != null)
        {
            Logger.LogDebug("Resetting previous region");
            serverManager.SetRegion(currentRegion);
        }
    }

    public override void Load()
    {
        Logger = Log;
        Instance = this;
        
        AddComponent<BorComponent>().Plugin = this;
        AddComponent<Coroutines.Component>();
        AddComponent<Dispatcher>();
        
        FreeNamePatch.Initialize();

        DebugMode = Config.Bind("Custom", "Enable Debug Mode", "false");
        GhostsSeeInformation = Config.Bind("Custom", "Ghosts See Remaining Tasks", true);
        GhostsSeeRoles = Config.Bind("Custom", "Ghosts See Roles", true);
        GhostsSeeModifier = Config.Bind("Custom", "Ghosts See Modifier", true);
        GhostsSeeVotes = Config.Bind("Custom", "Ghosts See Votes", true);
        ShowRoleSummary = Config.Bind("Custom", "Show Role Summary", true);
        ShowLighterDarker = Config.Bind("Custom", "Show Lighter / Darker", true);
        EnableSoundEffects = Config.Bind("Custom", "Enable Sound Effects", true);
        EnableHorseMode = Config.Bind("Custom", "Enable Horse Mode", false);
        ShowPopUpVersion = Config.Bind("Custom", "Show PopUp", "0");

        Ip = Config.Bind("Custom", "Custom Server IP", "127.0.0.1");
        Port = Config.Bind("Custom", "Custom Server Port", (ushort)22023);

        UpdateRegions();

        DevGuid = Config.Bind("Custom", "Dev Guid", "");
        FeaturesCodes = Config.Bind("Custom", "Feature codes", "");
        Preset = Config.Bind("MainConfig", "Preset", 0);
        Harmony.PatchAll();

        RpcManager.Instance.Load();
        CustomColors.Load();
        CustomOptions.Load();
        BorClient.Load();

        if (BepInExUpdater.UpdateRequired)
        {
            AddComponent<BepInExUpdater>();
            return;
        }

        EventUtility.Load();

        SubmergedCompatibility.Initialize();
        AddComponent<ModUpdateBehaviour>();
        AddComponent<AdminComponent>();
        MainMenuPatch.addSceneChangeCallbacks();

        Logger.LogInfo($"Current GUID: {CustomGuid.CurrentGuid.ToString()}");
        RolesManager.LoadRoles();
    }
    
    public override bool Unload()
    {
        Harmony.UnpatchSelf();

        return base.Unload();
    }
    
    [RegisterInIl2Cpp]
    private class BorComponent : MonoBehaviour
    {
        [HideFromIl2Cpp]
        public BetterOtherRolesPlugin Plugin { get; internal set; }

        public BorComponent(IntPtr ptr) : base(ptr)
        {
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.F5)) return;

            Plugin!.Log.LogInfo("Reloading all configs");

            foreach (var pluginInfo in IL2CPPChainloader.Instance.Plugins.Values)
            {
                var config = ((BasePlugin) pluginInfo.Instance).Config;
                if (!config.Any())
                {
                    continue;
                }

                try
                {
                    config.Reload();
                }
                catch (Exception e)
                {
                    Plugin.Log.LogWarning($"Exception occured during reload of {pluginInfo.Metadata.Name}: {e}");
                }
            }
        }
    }
}

// Deactivate bans, since I always leave my local testing game and ban myself
[HarmonyPatch(typeof(StatsManager), nameof(StatsManager.AmBanned), MethodType.Getter)]
public static class AmBannedPatch
{
    public static void Postfix(out bool __result)
    {
        __result = false;
    }
}

[HarmonyPatch(typeof(ChatController), nameof(ChatController.Awake))]
public static class ChatControllerAwakePatch
{
    private static void Prefix()
    {
        if (!EOSManager.Instance.isKWSMinor)
        {
            DataManager.Settings.Multiplayer.ChatMode = InnerNet.QuickChatModes.FreeChatOrQuickChat;
        }
    }
}

// Debugging tools
[HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
public static class DebugManager
{
    private static readonly string passwordHash = "d1f51dfdfd8d38027fd2ca9dfeb299399b5bdee58e6c0b3b5e9a45cd4e502848";
    private static readonly System.Random random = new System.Random((int)DateTime.Now.Ticks);
    private static List<PlayerControl> bots = new List<PlayerControl>();

    public static void Postfix(KeyboardJoystick __instance)
    {
        // Check if debug mode is active.
        StringBuilder builder = new StringBuilder();
        SHA256 sha = SHA256Managed.Create();
        Byte[] hashed = sha.ComputeHash(Encoding.UTF8.GetBytes(BetterOtherRolesPlugin.DebugMode.Value));
        foreach (var b in hashed)
        {
            builder.Append(b.ToString("x2"));
        }

        string enteredHash = builder.ToString();
        if (enteredHash != passwordHash) return;


        // Spawn dummys
        if (Input.GetKeyDown(KeyCode.F))
        {
            var playerControl = UnityEngine.Object.Instantiate(AmongUsClient.Instance.PlayerPrefab);
            var i = playerControl.PlayerId = (byte)GameData.Instance.GetAvailableId();

            bots.Add(playerControl);
            GameData.Instance.AddPlayer(playerControl);
            AmongUsClient.Instance.Spawn(playerControl, -2, InnerNet.SpawnFlags.None);

            playerControl.transform.position = CachedPlayer.LocalPlayer.transform.position;
            playerControl.GetComponent<DummyBehaviour>().enabled = true;
            playerControl.NetTransform.enabled = false;
            playerControl.SetName(RandomString(10));
            playerControl.SetColor((byte)random.Next(Palette.PlayerColors.Length));
            GameData.Instance.RpcSetTasks(playerControl.PlayerId, new byte[0]);
        }

        // Terminate round
        if (Input.GetKeyDown(KeyCode.L))
        {
            KernelRpc.ForceEnd();
        }
    }

    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}