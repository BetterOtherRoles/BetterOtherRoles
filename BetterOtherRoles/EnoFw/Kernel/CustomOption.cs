using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BepInEx.Configuration;
using BetterOtherRoles.EnoFw.Modules.BorApi;
using BetterOtherRoles.EnoFw.Utils;
using BetterOtherRoles.Players;
using UnityEngine;

namespace BetterOtherRoles.EnoFw.Kernel;

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
            BetterOtherRolesPlugin.Logger.LogDebug($"[STRING]{option.Key} => {option.SelectionIndex} ### {e}");
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
                    return option.SelectionIndex;
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
            BetterOtherRolesPlugin.Logger.LogDebug($"[FLOAT]{option.Key} => {option.SelectionIndex} ### {e}");
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
            BetterOtherRolesPlugin.Logger.LogDebug($"[INT]{option.Key} => {option.SelectionIndex} ### {e}");
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
            BetterOtherRolesPlugin.Logger.LogDebug($"[BOOL]{option.Key} => {option.SelectionIndex} ### {e}");
            return false;
        }
    }

    public readonly string Key;
    public readonly string Name;
    public readonly Color Color;
    public readonly int DefaultSelection;
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
    public ICustomOptionEntry CloudEntry;

    private CustomOption(
        OptionType type,
        string key,
        string name,
        Color color,
        List<string> stringSelections,
        List<float> floatSelections,
        int defaultIndex = 0,
        bool isHeader = false,
        CustomOption parent = null)
    {
        Key = key;
        Name = name;
        Color = color;
        StringSelections = stringSelections;
        FloatSelections = floatSelections;
        SelectionIndex = defaultIndex;
        DefaultSelection = defaultIndex;
        Parent = parent;
        IsHeader = isHeader;
        Type = type;
        if (Key == nameof(CustomOptions.Preset))
        {
            Entry = BetterOtherRolesPlugin.Preset;
            CloudEntry = new PresetEntry(nameof(CustomOptions.Preset), 0);
        }
        else
        {
            Entry = BetterOtherRolesPlugin.Instance.Config.Bind("Game Settings", Key, SelectionIndex);
            CloudEntry = new CustomOptionEntry(Key, DefaultSelection);
        }
        SelectionIndex = Mathf.Clamp(Entry.Value, 0, StringSelections.Count - 1);
        CloudEntry.InternalSetValue(SelectionIndex);
    }

    public string DisplayNameWithIndentation => !IsHeader ? $"→ {DisplayName}" : DisplayName;

    public string DisplayName => Color == Color.clear ? Name : Colors.Cs(Color, Name);

    public string DisplayValue => Type == OptionType.Boolean ? (bool)this ? Colors.Cs(Color.green, "✔") : Colors.Cs(Color.red, "✖") : this;

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

    public void UpdateSelection(int selection, bool internalOnly = false)
    {
        SelectionIndex = Mathf.Clamp(
            (selection + StringSelections.Count) % StringSelections.Count,
            0,
            StringSelections.Count - 1);
        if (internalOnly)
        {
            CloudEntry.InternalSetValue(SelectionIndex);
            return;
        }
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

                CloudEntry?.SetValue(SelectionIndex);
            }
            else if (OptionBehaviour is StringOption stringOption)
            {
                stringOption.oldValue = stringOption.Value = SelectionIndex;
                stringOption.ValueText.text = StringSelections[SelectionIndex];

                if (!AmongUsClient.Instance.AmHost || !CachedPlayer.LocalPlayer.PlayerControl) return;
                if (Key == nameof(CustomOptions.Preset) && SelectionIndex != Tab.Preset)
                {
                    Tab.SwitchPreset(SelectionIndex);
                }
                else
                {
                    if (Entry != null)
                    {
                        Entry.Value = SelectionIndex;
                        ShareOptionChange();
                    }

                    CloudEntry?.SetValue(SelectionIndex);
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
        public static readonly List<Tab> Tabs = new();

        public static int Preset
        {
            get => BetterOtherRolesPlugin.Preset.Value;
            private set => BetterOtherRolesPlugin.Preset.Value = value;
        }

        public static void SetPreset(int preset)
        {
            CustomOptions.Preset.UpdateSelection(0);
        }

        public CustomOption CreateBool(
            string key,
            NameAndColor name,
            bool defaultValue,
            CustomOption parent = null)
        {
            var customOption = new CustomOption(
                OptionType.Boolean,
                key,
                name.Name,
                name.Color,
                new List<string> { "off", "on" },
                null,
                defaultValue ? 1 : 0,
                parent == null,
                parent);
            return Add(customOption);
        }

        public CustomOption CreateFloatList(
            string key,
            NameAndColor name,
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
                OptionType.FloatList,
                key,
                name.Name,
                name.Color,
                selections,
                floatSelections,
                floatSelections.Contains(defaultValue) ? floatSelections.IndexOf(defaultValue) : 0,
                parent == null,
                parent);
            return Add(customOption);
        }

        public CustomOption CreateStringList(
            string key,
            NameAndColor name,
            List<string> selections,
            string defaultValue = null,
            CustomOption parent = null)
        {
            var selection = defaultValue == null ? 0 : selections.IndexOf(defaultValue);
            if (selection < 0) selection = 0;
            var customOption = new CustomOption(
                OptionType.StringList,
                key,
                name.Name,
                name.Color,
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
            VanillaSettings = BetterOtherRolesPlugin.Instance.Config.Bind("Vanilla", "GameOptions", string.Empty);
            LoadVanillaOptions();
            foreach (var setting in Options)
            {
                if (setting.Key == nameof(CustomOptions.Preset)) continue;
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
    
    public class NameAndColor
    {
        public readonly string Name;
        public readonly Color Color;

        public NameAndColor(string name, Color color)
        {
            Name = name;
            Color = color;
        }
    }
}