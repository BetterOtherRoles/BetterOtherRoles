using AmongUs.Data;
using AmongUs.GameOptions;
using HarmonyLib;
using InnerNet;

namespace BetterOtherRoles.Modules {
    [HarmonyPatch]
    public static class DynamicLobbies {
        public static int LobbyLimit = 15;
        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HostGame))]
        public static class InnerNetClientHostPatch {
            public static void Prefix(InnerNet.InnerNetClient __instance, [HarmonyArgument(0)] GameOptionsData settings) {
                DataManager.Settings.Multiplayer.ChatMode = InnerNet.QuickChatModes.FreeChatOrQuickChat;
            }
            public static void Postfix(InnerNet.InnerNetClient __instance, [HarmonyArgument(0)] GameOptionsData settings) {
                settings.MaxPlayers = DynamicLobbies.LobbyLimit;
            }
        }
        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.JoinGame))]
        public static class InnerNetClientJoinPatch {
            public static void Prefix(InnerNet.InnerNetClient __instance) {
                DataManager.Settings.Multiplayer.ChatMode = InnerNet.QuickChatModes.FreeChatOrQuickChat;
            }
        }
    }
}
