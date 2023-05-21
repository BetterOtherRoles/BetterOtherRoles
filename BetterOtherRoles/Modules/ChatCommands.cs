using System;
using HarmonyLib;
using System.Linq;
using AmongUs.Data;
using BetterOtherRoles.EnoFw.Roles.Crewmate;
using BetterOtherRoles.EnoFw.Roles.Modifiers;
using BetterOtherRoles.EnoFw.Roles.Neutral;
using BetterOtherRoles.Players;
using BetterOtherRoles.Utilities;
using BetterOtherRoles.EnoFw.Modules;

namespace BetterOtherRoles.Modules
{
    [HarmonyPatch]
    public static class ChatCommands
    {
        public static bool isLover(this PlayerControl player) =>
            !(player == null) && (Lovers.Instance.Is(player));

        [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
        private static class SendChatPatch
        {
            static bool Prefix(ChatController __instance)
            {
                string text = __instance.TextArea.text;
                bool handled = false;
                if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started)
                {
                    if (text.ToLower().StartsWith("/kick "))
                    {
                        string playerName = text.Substring(6);
                        PlayerControl target =
                            CachedPlayer.AllPlayers.FirstOrDefault(x => x.Data.PlayerName.Equals(playerName));
                        if (target != null && AmongUsClient.Instance != null && AmongUsClient.Instance.CanBan())
                        {
                            var client = AmongUsClient.Instance.GetClient(target.OwnerId);
                            if (client != null)
                            {
                                AmongUsClient.Instance.KickPlayer(client.Id, false);
                                handled = true;
                            }
                        }
                    }
                    else if (text.ToLower().StartsWith("/ban "))
                    {
                        string playerName = text.Substring(6);
                        PlayerControl target =
                            CachedPlayer.AllPlayers.FirstOrDefault(x => x.Data.PlayerName.Equals(playerName));
                        if (target != null && AmongUsClient.Instance != null && AmongUsClient.Instance.CanBan())
                        {
                            var client = AmongUsClient.Instance.GetClient(target.OwnerId);
                            if (client != null)
                            {
                                AmongUsClient.Instance.KickPlayer(client.Id, true);
                                handled = true;
                            }
                        }
                    }
                    else if (text.ToLower().StartsWith("/players"))
                    {
                        foreach (var client in AmongUsClient.Instance.allClients.ToArray())
                        {
                            BetterOtherRolesPlugin.Logger.LogDebug(
                                $"&&&&& {client.Character.Data.PlayerName}: {client.Id} {client.FriendCode}");
                        }

                        handled = true;
                    }
                    else if (text.ToLower().StartsWith("/shield "))
                    {
                        var playerName = text.Replace("/shield ", "");
                        TORMapOptions.firstKillName = playerName;
                        handled = true;
                    }
                }

                if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay)
                {
                    if (text.ToLower().Equals("/murder"))
                    {
                        CachedPlayer.LocalPlayer.PlayerControl.Exiled();
                        FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(
                            CachedPlayer.LocalPlayer.Data, CachedPlayer.LocalPlayer.Data);
                        handled = true;
                    }
                    else if (text.ToLower().StartsWith("/color "))
                    {
                        handled = true;
                        int col;
                        if (!Int32.TryParse(text.Substring(7), out col))
                        {
                            __instance.AddChat(CachedPlayer.LocalPlayer.PlayerControl,
                                "Unable to parse color id\nUsage: /color {id}");
                        }

                        col = Math.Clamp(col, 0, Palette.PlayerColors.Length - 1);
                        CachedPlayer.LocalPlayer.PlayerControl.SetColor(col);
                        __instance.AddChat(CachedPlayer.LocalPlayer.PlayerControl, "Changed color succesfully");
                        ;
                    }
                }

                if (text.ToLower().StartsWith("/tp ") && CachedPlayer.LocalPlayer.Data.IsDead)
                {
                    string playerName = text.Substring(4).ToLower();
                    PlayerControl target =
                        CachedPlayer.AllPlayers.FirstOrDefault(x => x.Data.PlayerName.ToLower().Equals(playerName));
                    if (target != null)
                    {
                        CachedPlayer.LocalPlayer.transform.position = target.transform.position;
                        handled = true;
                    }
                }

                if (handled)
                {
                    __instance.TextArea.Clear();
                    __instance.quickChatMenu.ResetGlyphs();
                }

                return !handled;
            }
        }

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        public static class EnableChat
        {
            public static void Postfix(HudManager __instance)
            {
                if (!__instance.Chat.isActiveAndEnabled &&
                    (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay ||
                     (CachedPlayer.LocalPlayer.PlayerControl.isLover() && Lovers.Instance.EnableChat)))
                    __instance.Chat.SetVisible(true);
            }
        }

        [HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetName))]
        public static class SetBubbleName
        {
            public static void Postfix(ChatBubble __instance, [HarmonyArgument(0)] string playerName)
            {
                var sourcePlayer = PlayerControl.AllPlayerControls.ToArray().ToList()
                    .FirstOrDefault(x => x.Data != null && x.Data.PlayerName.Equals(playerName));
                if (CachedPlayer.LocalPlayer != null && CachedPlayer.LocalPlayer.Data.Role.IsImpostor &&
                    (Spy.Instance.Player != null && sourcePlayer.PlayerId == Spy.Instance.Player.PlayerId ||
                     Sidekick.Instance.Player != null && Sidekick.Instance.WasTeamRed &&
                     sourcePlayer.PlayerId == Sidekick.Instance.Player.PlayerId ||
                     Jackal.Instance.Player != null && Jackal.Instance.WasTeamRed &&
                     sourcePlayer.PlayerId == Jackal.Instance.Player.PlayerId) &&
                    __instance != null) __instance.NameText.color = Palette.ImpostorRed;
            }
        }

        [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
        public static class AddChat
        {
            public static bool Prefix(ChatController __instance, [HarmonyArgument(0)] PlayerControl sourcePlayer)
            {
                if (__instance != FastDestroyableSingleton<HudManager>.Instance.Chat)
                    return true;
                PlayerControl localPlayer = CachedPlayer.LocalPlayer.PlayerControl;
                return localPlayer == null || MeetingHud.Instance != null || LobbyBehaviour.Instance != null ||
                       localPlayer.Data.IsDead ||
                       localPlayer.isLover() && Lovers.Instance.EnableChat ||
                       sourcePlayer.PlayerId == CachedPlayer.LocalPlayer.PlayerId;
            }
        }
    }
}