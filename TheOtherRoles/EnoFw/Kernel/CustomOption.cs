using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using BepInEx.Configuration;
using TheOtherRoles.Players;
using UnityEngine;

namespace TheOtherRoles.EnoFw.Kernel;

public class CustomOption
{
    public enum OptionType
    {
        Boolean,
        StringList,
        FloatList,
    }

    public enum Maps
    {
        Skeld,
        MiraHq,
        Polus,
        Airship,
        Submerged
    }

    public enum GameMode
    {
        Classic,
        HideNSeek,
        Guesser
    }

    public static GameMode CurrentGameMode
    {
        get
        {
            switch (TORMapOptions.gameMode)
            {
                case CustomGamemodes.Guesser:
                    return GameMode.Guesser;
                case CustomGamemodes.HideNSeek:
                    return GameMode.HideNSeek;
                case CustomGamemodes.Classic:
                default:
                    return GameMode.Classic;
            }
        }
    }

    private static readonly Regex StringToFloatRegex = new("[^0-9 -]");

    public static ConfigEntry<string> VanillaSettings;

    public static implicit operator string(CustomOption option)
    {
        try
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
        catch (Exception e)
        {
            System.Console.WriteLine($"[STRING]{option.Key} => {option.SelectionIndex} ### {e}");
            return "";
            ;
        }
    }

    public static implicit operator float(CustomOption option)
    {
        try
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
                        throw new Exception($"Error: FloatSelections is null in customSetting {option.Key}");
                    return option.FloatSelections[option.SelectionIndex];
                default:
                    throw new Exception("Error: CustomSetting type out of enum SettingType range");
            }
        }
        catch (Exception e)
        {
            System.Console.WriteLine($"[FLOAT]{option.Key} => {option.SelectionIndex} ### {e}");
            return 0f;
        }
    }

    public static implicit operator int(CustomOption option)
    {
        try
        {
            return Mathf.RoundToInt((float)option);
        }
        catch (Exception e)
        {
            System.Console.WriteLine($"[INT]{option.Key} => {option.SelectionIndex} ### {e}");
            return 0;
        }
    }

    public static implicit operator bool(CustomOption option)
    {
        try
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
        catch (Exception e)
        {
            System.Console.WriteLine($"[BOOL]{option.Key} => {option.SelectionIndex} ### {e}");
            return false;
        }
    }

    public readonly string Key;
    public readonly string Name;
    public readonly List<string> StringSelections;
    public readonly List<float> FloatSelections;
    public int SelectionIndex;
    public OptionBehaviour OptionBehaviour;
    public readonly CustomOption Parent;
    public readonly bool IsHeader;
    public readonly OptionType Type;
    public bool HasChildren => Tab.Options.Any(o => o.Parent != null && o.Parent.Key == Key);
    public List<CustomOption> Children => Tab.Options.Where(o => o.Parent != null && o.Parent.Key == Key).ToList();

    public readonly List<Maps> AllowedMaps = new()
        { Maps.Skeld, Maps.MiraHq, Maps.Polus, Maps.Airship, Maps.Submerged };

    public readonly List<GameMode> AllowedGameModes = new() { GameMode.Classic, GameMode.Guesser };

    public ConfigEntry<int> Entry;

    private CustomOption(
        OptionType type,
        string key,
        string name,
        List<string> stringSelections,
        List<float> floatSelections,
        int defaultIndex = 0,
        bool isHeader = false,
        CustomOption parent = null)
    {
        Key = key;
        Name = parent == null ? name : $"→ {name}";
        StringSelections = stringSelections;
        FloatSelections = floatSelections;
        SelectionIndex = defaultIndex;
        Parent = parent;
        IsHeader = isHeader;
        Type = type;
        Entry = TheOtherRolesPlugin.Instance.Config.Bind(
            Key == nameof(CustomOptions.Preset) ? "MainConfig" : $"Preset{Tab.Preset}", Key,
            SelectionIndex);
        SelectionIndex = Mathf.Clamp(Entry.Value, 0, StringSelections.Count - 1);
    }

    public CustomOption OnlyForMaps(params Maps[] maps)
    {
        AllowedMaps.Clear();
        AllowedMaps.AddRange(maps);

        return this;
    }

    public CustomOption OnlyForGameModes(params GameMode[] gameModes)
    {
        AllowedGameModes.Clear();
        AllowedGameModes.AddRange(gameModes);

        return this;
    }

    public void UpdateSelection(int selection)
    {
        SelectionIndex = Mathf.Clamp(
            (selection + StringSelections.Count) % StringSelections.Count,
            0,
            StringSelections.Count - 1);
        if (StringSelections.Count > 0 && OptionBehaviour != null)
        {
            if (Type == OptionType.Boolean && OptionBehaviour is ToggleOption boolOption)
            {
                boolOption.oldValue = boolOption.CheckMark.enabled = this;
                if (Entry != null)
                {
                    Entry.Value = SelectionIndex;
                    ShareOptionChange();
                }
            }
            else if (OptionBehaviour is StringOption stringOption)
            {
                stringOption.oldValue = stringOption.Value = SelectionIndex;
                stringOption.ValueText.text = StringSelections[SelectionIndex];

                if (!AmongUsClient.Instance.AmHost || !CachedPlayer.LocalPlayer.PlayerControl) return;
                if (Key == nameof(CustomOptions.Preset) && SelectionIndex != Tab.Preset)
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
        }
        else if (Key == nameof(CustomOptions.Preset) && AmongUsClient.Instance.AmHost && PlayerControl.LocalPlayer != null)
        {
            // Share the preset switch for random maps, even if the menu isnt open!
            Tab.SwitchPreset(SelectionIndex);
            Tab.ShareCustomOptions(); // Share all selections
        }
    }

    private void ShareOptionChange()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        var data = new Dictionary<string, int>
        {
            {
                Key, SelectionIndex
            }
        };
        KernelRpc.ShareOptions(data);
    }

    private class CustomOptionInfo
    {
        public string Key { get; set; }
        public int Selection { get; set; }
    }

    public class Tab
    {
        public readonly static List<Tab> Tabs = new();

        public static int Preset { get; private set; } = 1;

        public CustomOption CreateBool(
            string key,
            string name,
            bool defaultValue,
            CustomOption parent = null)
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
            CustomOption parent = null,
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
            string defaultValue = null,
            CustomOption parent = null)
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

        public static List<CustomOption> Options => Tabs.SelectMany(tab => tab.Settings)
            .Where(o => o.AllowedGameModes.Contains(CurrentGameMode)).ToList();

        public static void ShareCustomOptions()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            var data = Options.ToDictionary(option => option.Key, option => option.SelectionIndex);
            KernelRpc.ShareOptions(data);
        }

        public static void SwitchPreset(int newPreset)
        {
            SaveVanillaOptions();
            Preset = newPreset;
            VanillaSettings = TheOtherRolesPlugin.Instance.Config.Bind($"Preset{Preset}", "GameOptions", string.Empty);
            LoadVanillaOptions();
            foreach (var setting in Options)
            {
                if (setting.Key == nameof(CustomOptions.Preset)) continue;
                setting.Entry =
                    TheOtherRolesPlugin.Instance.Config.Bind($"Preset{Preset}", $"{setting.Key}",
                        setting.SelectionIndex);
                setting.SelectionIndex = Mathf.Clamp(setting.Entry.Value, 0, setting.StringSelections.Count - 1);
                if (setting.OptionBehaviour == null) continue;
                if (setting.OptionBehaviour is StringOption stringOption)
                {
                    stringOption.oldValue = stringOption.Value = setting.SelectionIndex;
                    stringOption.ValueText.text = setting.StringSelections[setting.SelectionIndex];
                } else if (setting.OptionBehaviour is ToggleOption boolOption)
                {
                    boolOption.oldValue = boolOption.CheckMark.enabled = setting;
                }
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

            Tabs.Add(this);
        }

        private CustomOption Add(CustomOption option)
        {
            Settings.Add(option);
            return option;
        }
    }
}