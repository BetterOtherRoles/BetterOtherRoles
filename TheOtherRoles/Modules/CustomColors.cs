using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using UnityEngine;
using HarmonyLib;
using AmongUs.Data.Legacy;
using TheOtherRoles.EnoFw;
using TheOtherRoles.Utilities;
using Color = System.Drawing.Color;

namespace TheOtherRoles.Modules;

public class CustomColors
{
    private static readonly Dictionary<int, string> ColorStrings = new Dictionary<int, string>();
    public static readonly List<int> LighterColors = new() { 3, 4, 5, 7, 10, 11, 13, 14, 17 };
    private static uint _pickableColors = (uint)Palette.ColorNames.Length;

    private static readonly List<int> ORDER = new List<int>
    {
        0, 1, 2, 3, 4,
        5, 6, 7, 8, 9,
        10, 11, 12, 13, 14,
        15, 16, 17, 18, 19,
        20, 21, 22, 23, 24,
        25, 26, 27, 28, 29,
        30, 31, 32, 33, 34
    };

    public static async void Load()
    {
        var namesList = Palette.ColorNames.ToList();
        var colorsList = Palette.PlayerColors.ToList();
        var shadowsList = Palette.ShadowColors.ToList();

        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "BetterOtherRoles Client");
        var response = await client.GetAsync(CustomGuid.ColorsUrl, HttpCompletionOption.ResponseContentRead);
        if (!response.IsSuccessStatusCode) return;
        var data = await response.Content.ReadAsStringAsync();
        var items = Rpc.Deserialize<List<ApiCustomColor>>(data);
        var colors = items.Select(i => new CustomColor
            {
                LongName = i.LongName,
                Color = new Color32((byte)i.Color[0], (byte)i.Color[1], (byte)i.Color[2], byte.MaxValue),
                Shadow = new Color32((byte)i.Shadow[0], (byte)i.Shadow[1], (byte)i.Shadow[2], byte.MaxValue),
                IsLighterColor = i.IsLighterColor
            }).OrderBy(cc => cc.RawColor.GetHue())
            .ThenBy(o => o.RawColor.R * 3 + o.RawColor.G * 2 + o.RawColor.B * 1).ToList();

        _pickableColors += (uint)colors.Count; // Colors to show in Tab

        var id = 50000;
        foreach (var cc in colors)
        {
            namesList.Add((StringNames)id);
            ColorStrings[id++] = cc.LongName;
            colorsList.Add(cc.Color);
            shadowsList.Add(cc.Shadow);
            if (cc.IsLighterColor)
            {
                LighterColors.Add(colorsList.Count - 1);
            }
        }

        Palette.ColorNames = namesList.ToArray();
        Palette.PlayerColors = colorsList.ToArray();
        Palette.ShadowColors = shadowsList.ToArray();
    }

    private struct CustomColor
    {
        public string LongName;
        public Color32 Color;
        public Color32 Shadow;
        public bool IsLighterColor;

        public Color RawColor => System.Drawing.Color.FromArgb(Color.r, Color.g, Color.b, Color.a);
    }

    private class ApiCustomColor
    {
        public string LongName { get; set; }
        public int[] Color { get; set; }
        public int[] Shadow { get; set; }
        public bool IsLighterColor { get; set; }
    }

    [HarmonyPatch]
    public static class CustomColorPatches
    {
        [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new[]
        {
            typeof(StringNames),
            typeof(Il2CppReferenceArray<Il2CppSystem.Object>)
        })]
        private class ColorStringPatch
        {
            [HarmonyPriority(Priority.Last)]
            public static bool Prefix(ref string __result, [HarmonyArgument(0)] StringNames name)
            {
                if ((int)name >= 50000)
                {
                    string text = CustomColors.ColorStrings[(int)name];
                    if (text != null)
                    {
                        __result = text;
                        return false;
                    }
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerTab), nameof(PlayerTab.OnEnable))]
        private static class PlayerTabEnablePatch
        {
            public static void Postfix(PlayerTab __instance)
            {
                // Replace instead
                Il2CppArrayBase<ColorChip> chips = __instance.ColorChips.ToArray();

                int cols = 5; // TODO: Design an algorithm to dynamically position chips to optimally fill space
                for (int i = 0; i < ORDER.Count; i++)
                {
                    int pos = ORDER[i];
                    if (pos < 0 || pos > chips.Length)
                        continue;
                    ColorChip chip = chips[pos];
                    int row = i / cols, col = i % cols; // Dynamically do the positioning
                    chip.transform.localPosition = new Vector3(-0.975f + (col * 0.485f), 1.475f - (row * 0.49f),
                        chip.transform.localPosition.z);
                    chip.transform.localScale *= 0.78f;
                }

                for (int j = ORDER.Count; j < chips.Length; j++)
                {
                    // If number isn't in order, hide it
                    ColorChip chip = chips[j];
                    chip.transform.localScale *= 0f;
                    chip.enabled = false;
                    chip.Button.enabled = false;
                    chip.Button.OnClick.RemoveAllListeners();
                }
            }
        }

        [HarmonyPatch(typeof(LegacySaveManager), nameof(LegacySaveManager.LoadPlayerPrefs))]
        private static class LoadPlayerPrefsPatch
        {
            // Fix Potential issues with broken colors
            private static bool needsPatch = false;

            public static void Prefix([HarmonyArgument(0)] bool overrideLoad)
            {
                if (!LegacySaveManager.loaded || overrideLoad)
                    needsPatch = true;
            }

            public static void Postfix()
            {
                if (!needsPatch) return;
                LegacySaveManager.colorConfig %= CustomColors._pickableColors;
                needsPatch = false;
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckColor))]
        private static class PlayerControlCheckColorPatch
        {
            private static bool isTaken(PlayerControl player, uint color)
            {
                foreach (GameData.PlayerInfo p in GameData.Instance.AllPlayers.GetFastEnumerator())
                    if (!p.Disconnected && p.PlayerId != player.PlayerId && p.DefaultOutfit.ColorId == color)
                        return true;
                return false;
            }

            public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte bodyColor)
            {
                // Fix incorrect color assignment
                uint color = (uint)bodyColor;
                if (isTaken(__instance, color) || color >= Palette.PlayerColors.Length)
                {
                    int num = 0;
                    while (num++ < 50 && (color >= CustomColors._pickableColors || isTaken(__instance, color)))
                    {
                        color = (color + 1) % CustomColors._pickableColors;
                    }
                }

                __instance.RpcSetColor((byte)color);
                return false;
            }
        }
    }
}