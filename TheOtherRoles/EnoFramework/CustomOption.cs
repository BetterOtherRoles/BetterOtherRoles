using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using BepInEx.Configuration;
using Reactor.Networking.Attributes;
using TheOtherRoles.Customs;
using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.Players;
using UnityEngine;

namespace TheOtherRoles.EnoFramework;

public class CustomOption
{
    public enum OptionType
    {
        Boolean,
        StringList,
        FloatList,
    }

    private static readonly Regex StringToFloatRegex = new("[^0-9 -]");

    public static ConfigEntry<string>? VanillaSettings;

    public static implicit operator string(CustomOption option)
    {
        switch (option.Type)
        {
            case OptionType.Boolean:
                return option ? "yes" : "no";
            case OptionType.StringList:
            case OptionType.FloatList:
            default:
                return option.StringSelections[option.SelectionIndex];
        }
    }

    public static implicit operator float(CustomOption option)
    {
        switch (option.Type)
        {
            case OptionType.Boolean:
                return option ? 1f : 0f;
            case OptionType.StringList:
                return (float)Convert.ToDouble(
                    StringToFloatRegex.Replace(option, string.Empty),
                    CultureInfo.InvariantCulture.NumberFormat);
            case OptionType.FloatList:
                if (option.FloatSelections == null)
                    throw new KernelException($"Error: FloatSelections is null in customSetting {option.Key}");
                return option.FloatSelections[option.SelectionIndex];
            default:
                throw new KernelException("Error: CustomSetting type out of enum SettingType range");
        }
    }

    public static implicit operator int(CustomOption option)
    {
        return Mathf.RoundToInt((float)option);
    }

    public static implicit operator bool(CustomOption option)
    {
        switch (option.Type)
        {
            case OptionType.Boolean:
                return option.SelectionIndex == 1;
            case OptionType.StringList:
            case OptionType.FloatList:
            default:
                return option.SelectionIndex > 0;
        }
    }

    public readonly string Key;
    public readonly string Name;
    public readonly List<string> StringSelections;
    public readonly List<float>? FloatSelections;
    public int SelectionIndex;
    public OptionBehaviour? OptionBehaviour;
    public readonly CustomOption? Parent;
    public readonly bool IsHeader;
    public readonly OptionType Type;

    public ConfigEntry<int>? Entry;

    private CustomOption(
        OptionType type,
        string key,
        string name,
        List<string> stringSelections,
        List<float>? floatSelections,
        int defaultIndex = 0,
        bool isHeader = false,
        CustomOption? parent = null)
    {
        Key = key;
        Name = parent == null ? name : $"- {name}";
        StringSelections = stringSelections;
        FloatSelections = floatSelections;
        SelectionIndex = defaultIndex;
        Parent = parent;
        IsHeader = isHeader;
        Type = type;
        Entry = TheOtherRolesPlugin.Instance.Config.Bind(
            Key == nameof(Singleton<CustomOptionsHolder>.Instance.Preset) ? $"MainConfig" : $"Preset{Tab.Preset}", Key,
            SelectionIndex);
        SelectionIndex = Mathf.Clamp(Entry.Value, 0, StringSelections.Count - 1);
    }

    public void UpdateSelection(int selection)
    {
        SelectionIndex = Mathf.Clamp(
            (selection + StringSelections.Count) % StringSelections.Count,
            0,
            StringSelections.Count - 1);
        if (StringSelections.Count > 0 && OptionBehaviour != null && OptionBehaviour is StringOption stringOption)
        {
            stringOption.oldValue = stringOption.Value = SelectionIndex;
            stringOption.ValueText.text = StringSelections[SelectionIndex].ToString();

            if (!AmongUsClient.Instance.AmHost || !CachedPlayer.LocalPlayer?.PlayerControl) return;
            if (Key == nameof(Singleton<CustomOptionsHolder>.Instance.Preset) && SelectionIndex != Tab.Preset)
            {
                Tab.SwitchPreset(SelectionIndex);
                ShareOptionChange();
            }
            else if (Entry != null)
            {
                Entry.Value = SelectionIndex;
                ShareOptionChange();
            }
        }
        else if (Key == nameof(Singleton<CustomOptionsHolder>.Instance.Preset) && AmongUsClient.Instance.AmHost &&
                 PlayerControl.LocalPlayer)
        {
            // Share the preset switch for random maps, even if the menu isnt open!
            Tab.SwitchPreset(SelectionIndex);
            Tab.ShareCustomOptions(); // Share all selections
        }
    }

    private void ShareOptionChange()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        ShareCustomOptionsChanges(PlayerControl.LocalPlayer, Rpc.Serialize(new List<CustomOptionInfo>
        {
            new()
            {
                Key = Key,
                Selection = SelectionIndex,
            },
        }));
    }

    [MethodRpc((uint)Rpc.Id.ShareCustomOptionsChanges)]
    private static void ShareCustomOptionsChanges(PlayerControl _, string rawData)
    {
        if (AmongUsClient.Instance.AmHost) return;
        var customOptionInfos = Rpc.Deserialize<CustomOptionInfo[]>(rawData);
        foreach (var customOptionInfo in customOptionInfos)
        {
            var option = Tab.Options.Find(co => co.Key == customOptionInfo.Key);
            if (option == null) return;
            option.SelectionIndex = customOptionInfo.Selection;
        }
    }

    private class CustomOptionInfo
    {
        public string Key { get; set; }
        public int Selection { get; set; }
    }

    public class Tab
    {
        public static List<Tab> Tabs = new();

        public static int Preset { get; private set; } = 1;

        public CustomOption CreateBool(
            string key,
            string name,
            bool defaultValue,
            CustomOption? parent = null)
        {
            var customOption = new CustomOption(
                OptionType.Boolean,
                key,
                name,
                new List<string> { "off", "on" },
                null,
                defaultValue ? 1 : 0,
                parent == null,
                parent);
            return Add(customOption);
        }

        public CustomOption CreateFloatList(
            string key,
            string name,
            float minValue,
            float maxValue,
            float defaultValue,
            float step,
            CustomOption? parent = null,
            string prefix = "",
            string suffix = "")
        {
            var selections = new List<string>();
            var floatSelections = new List<float>();
            for (var i = minValue; i <= maxValue; i += step)
            {
                floatSelections.Add(i);
                selections.Add($"{prefix}{i}{suffix}");
            }

            var customOption = new CustomOption(
                CustomOption.OptionType.FloatList,
                key,
                name,
                selections,
                floatSelections,
                floatSelections.Contains(defaultValue) ? floatSelections.IndexOf(defaultValue) : 0,
                parent == null,
                parent);
            return Add(customOption);
        }

        public CustomOption CreateStringList(
            string key,
            string name,
            List<string> selections,
            string? defaultValue = null,
            CustomOption? parent = null)
        {
            var selection = defaultValue == null ? 0 : selections.IndexOf(defaultValue);
            if (selection < 0) selection = 0;
            var customOption = new CustomOption(
                CustomOption.OptionType.StringList,
                key,
                name,
                selections,
                null,
                selection,
                parent == null,
                parent
            );
            return Add(customOption);
        }

        public static List<CustomOption> Options => Tabs.SelectMany(tab => tab.Settings).ToList();

        public static void ShareCustomOptions()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            var options = Tabs.SelectMany(settingsTab => settingsTab.Settings).Select(setting =>
                new CustomOptionInfo { Key = setting.Key, Selection = setting.SelectionIndex }).ToList();
            ShareCustomOptionsChanges(
                PlayerControl.LocalPlayer,
                Rpc.Serialize(options)
            );
        }

        public static void SwitchPreset(int newPreset)
        {
            SaveVanillaOptions();
            Preset = newPreset + 1;
            VanillaSettings = TheOtherRolesPlugin.Instance.Config.Bind($"Preset{Preset}", "GameOptions", string.Empty);
            LoadVanillaOptions();
            foreach (var setting in Tabs.SelectMany(settingsTab => settingsTab.Settings))
            {
                if (setting.Key == nameof(Singleton<CustomOptionsHolder>.Instance.Preset)) continue;
                setting.Entry =
                    TheOtherRolesPlugin.Instance.Config.Bind($"Preset{Preset}", $"{setting.Key}",
                        setting.SelectionIndex);
                setting.SelectionIndex = Mathf.Clamp(setting.Entry.Value, 0, setting.StringSelections.Count - 1);
                if (setting.OptionBehaviour == null ||
                    setting.OptionBehaviour is not StringOption stringOption) continue;
                stringOption.oldValue = stringOption.Value = setting.SelectionIndex;
                stringOption.ValueText.text = setting.StringSelections[setting.SelectionIndex];
            }
        }

        private static void LoadVanillaOptions()
        {
            if (VanillaSettings != null)
            {
                var optionsString = VanillaSettings.Value;
                if (optionsString == string.Empty) return;
                GameOptionsManager.Instance.GameHostOptions =
                    GameOptionsManager.Instance.gameOptionsFactory.FromBytes(Convert.FromBase64String(optionsString));
            }

            GameOptionsManager.Instance.CurrentGameOptions = GameOptionsManager.Instance.GameHostOptions;
            GameManager.Instance.LogicOptions.SetGameOptions(GameOptionsManager.Instance.CurrentGameOptions);
            GameManager.Instance.LogicOptions.SyncOptions();
        }

        private static void SaveVanillaOptions()
        {
            if (VanillaSettings != null)
            {
                VanillaSettings.Value =
                    Convert.ToBase64String(
                        GameOptionsManager.Instance.gameOptionsFactory.ToBytes(GameManager.Instance.LogicOptions
                            .currentGameOptions));
            }
        }

        public readonly string Key;
        public readonly string Title;
        public readonly string IconPath;
        public readonly List<CustomOption> Settings = new();

        public Tab(string key, string title, string iconPath)
        {
            Key = key;
            Title = title;
            IconPath = iconPath;
        }

        public CustomOption Add(CustomOption option)
        {
            Settings.Add(option);
            return option;
        }
    }
}