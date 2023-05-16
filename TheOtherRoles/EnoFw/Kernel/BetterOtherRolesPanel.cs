using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using HarmonyLib;
using UnityEngine;

namespace TheOtherRoles.EnoFw.Kernel;

public static class BetterOtherRolesPanel
{
    private static int _settingsPage = -1;

    private const bool LobbyTextScroller = true;

    private const float LobbyTextRowHeight = 0.101f;

    [HarmonyPatch(typeof(IGameOptionsExtensions), nameof(IGameOptionsExtensions.ToHudString))]
    internal static class GameOptionsDataPatch
    {
        private static void Postfix(ref string __result)
        {
            if (GameOptionsManager.Instance.currentGameMode == GameModes.HideNSeek) return;

            CustomOption.Tab tab = null;
            var builder = new StringBuilder();
            builder.AppendLine("Press Tab To Change Page\n");
            builder.AppendLine($"Currently Viewing Page ({(_settingsPage + 2)}/{CustomOption.Tab.Tabs.Count + 1})\n");
            if (_settingsPage >= 0)
            {
                tab = CustomOption.Tab.Tabs[_settingsPage];
                builder.AppendLine(tab.Title);
            }

            if (tab == null)
            {
                builder.Append(new StringBuilder(__result));
            }
            else
            {
                builder.Append(RenderTab(tab));
            }

            __result = $"<size=1.25>{builder}</size>";
        }
    }

    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
    internal static class Update
    {
        private static void Postfix(ref GameOptionsMenu __instance)
        {
            __instance.GetComponentInParent<Scroller>().ContentYBounds.max = (__instance.Children.Length - 6.5f) / 2;
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    internal static class LobbyPatch
    {
        public static void Postfix(HudManager __instance)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (_settingsPage > 3)
                    _settingsPage = -1;
                else
                    _settingsPage++;
            }
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    internal class HudManagerUpdate
    {
        private const float
            MinX = -5.233334F /*-5.3F*/,
            OriginalY = 2.9F,
            MinY = 3F; // Differs to cause excess options to appear cut off to encourage scrolling

        private static Scroller Scroller;
        private static Vector3 LastPosition = new Vector3(MinX, MinY);

        public static void Prefix(HudManager __instance)
        {
            if (__instance.GameSettings?.transform == null) return;


            // Scroller disabled
            if (!LobbyTextScroller)
            {
                // Remove scroller if disabled late
                if (Scroller != null)
                {
                    __instance.GameSettings.transform.SetParent(Scroller.transform.parent);
                    __instance.GameSettings.transform.localPosition = new Vector3(MinX, OriginalY);

                    Object.Destroy(Scroller);
                }

                return;
            }

            CreateScroller(__instance);

            Scroller.gameObject.SetActive(__instance.GameSettings.gameObject.activeSelf);

            if (!Scroller.gameObject.active) return;

            var rows = __instance.GameSettings.text.Count(c => c == '\n');
            var maxY = Mathf.Max(MinY, rows * LobbyTextRowHeight + (rows - 38) * LobbyTextRowHeight);

            Scroller.ContentYBounds = new FloatRange(MinY, maxY);

            // Prevent scrolling when the player is interacting with a menu
            if (PlayerControl.LocalPlayer?.CanMove != true)
            {
                __instance.GameSettings.transform.localPosition = LastPosition;

                return;
            }

            if (__instance.GameSettings.transform.localPosition.x != MinX ||
                __instance.GameSettings.transform.localPosition.y < MinY) return;

            LastPosition = __instance.GameSettings.transform.localPosition;
        }

        private static void CreateScroller(HudManager __instance)
        {
            if (Scroller != null) return;

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

            Scroller.Inner = __instance.GameSettings.transform;
            __instance.GameSettings.transform.SetParent(Scroller.transform);
        }
    }


    private static StringBuilder RenderTab(CustomOption.Tab tab)
    {
        var builder = new StringBuilder();
        foreach (var option in tab.Settings.Where(o =>
                     o.Parent == null && o.AllowedGameModes.Contains(CustomOption.CurrentGameMode)))
        {
            builder.Append(RenderOption(option));
        }

        return builder;
    }

    private static StringBuilder RenderOption(CustomOption option, int deep = 0)
    {
        var prefix = option.IsHeader ? "\n" : string.Empty;
        if (deep > 0)
        {
            for (var i = 0; i < deep; i++)
            {
                prefix += "  ";
            }
        }

        var result = new StringBuilder();
        result.AppendLine($"{prefix}{option.Name}: {(string)option}");
        if (option.SelectionIndex == 0 || !option.HasChildren) return result;
        foreach (var subOption in option.Children.Where(o => o.AllowedGameModes.Contains(CustomOption.CurrentGameMode)))
        {
            result.Append(RenderOption(subOption, deep + 1));
        }

        return result;
    }
}

/*
[RegisterInIl2Cpp]
public class BetterOtherRolesPanelComponent : MonoBehaviour
{
    public static BetterOtherRolesPanelComponent Instance { get; private set; }
    
    [HideFromIl2Cpp]
    public bool ShiftKey { get; private set; }

    public BetterOtherRolesPanelComponent()
    {
        Instance = this;
    }

    public void Update()
    {
        if (PositionTester.Enabled)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            {
                ShiftKey = true;
            }
            if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
            {
                ShiftKey = false;
            }
            if (Input.GetKeyDown(KeyCode.UpArrow) && ShiftKey)
            {
                var position = PositionTester.CurrentPosition;
                position.z += PositionTester.Offset;
                PositionTester.UpdatePosition(position);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) && ShiftKey)
            {
                var position = PositionTester.CurrentPosition;
                position.z -= PositionTester.Offset;
                PositionTester.UpdatePosition(position);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                var position = PositionTester.CurrentPosition;
                position.x += PositionTester.Offset;
                PositionTester.UpdatePosition(position);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                var position = PositionTester.CurrentPosition;
                position.x -= PositionTester.Offset;
                PositionTester.UpdatePosition(position);
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                var position = PositionTester.CurrentPosition;
                position.y += PositionTester.Offset;
                PositionTester.UpdatePosition(position);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                var position = PositionTester.CurrentPosition;
                position.y -= PositionTester.Offset;
                PositionTester.UpdatePosition(position);
            }
        }
    }
}

public static class PositionTester
{
    public const bool Enabled = true;
    public const float Offset = 0.1f;

    public static Transform Transform;

    public static Vector3 CurrentPosition => Transform == null ? Vector3.zero : Transform.position;

    public static void UpdatePosition(Vector3 pos)
    {
        if (Transform == null) return;
        Transform.localPosition = pos;
        TheOtherRolesPlugin.Logger.LogDebug($"{pos.x} {pos.y} {pos.z}");
    }
}
*/