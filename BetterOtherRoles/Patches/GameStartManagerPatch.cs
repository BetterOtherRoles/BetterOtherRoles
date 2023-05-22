using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using BetterOtherRoles.EnoFw;
using BetterOtherRoles.EnoFw.Kernel;
using BetterOtherRoles.EnoFw.Libs.Reactor.Networking.Extensions;
using BetterOtherRoles.EnoFw.Modules;
using BetterOtherRoles.Players;
using BetterOtherRoles.Utilities;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using BetterOtherRoles.Modules;

namespace BetterOtherRoles.Patches
{
    public static class GameStartManagerPatch
    {
        public static readonly Dictionary<int, VersionHandshake.PlayerVersion> PlayerVersions = new();
        public static float timer = 600f;
        private static float kickingTimer = 0f;
        private static bool versionSent = false;
        private static string lobbyCodeText = "";

        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
        public class AmongUsClientOnPlayerJoinedPatch
        {
            public static void Postfix(AmongUsClient __instance)
            {
                VersionHandshake.Share();
                if (AmongUsClient.Instance.AmHost)
                {
                    KernelRpc.ShareGameMode((byte)TORMapOptions.gameMode);
                }
            }
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
        public class GameStartManagerStartPatch
        {
            public static void Postfix(GameStartManager __instance)
            {
                // Trigger version refresh
                versionSent = false;
                // Reset lobby countdown timer
                timer = 600f;
                // Reset kicking timer
                kickingTimer = 0f;
                // Copy lobby code
                string code = InnerNet.GameCode.IntToGameName(AmongUsClient.Instance.GameId);
                GUIUtility.systemCopyBuffer = code;
                lobbyCodeText =
                    FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.RoomCode,
                        new Il2CppReferenceArray<Il2CppSystem.Object>(0)) + "\r\n" + code;
            }
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
        public class GameStartManagerUpdatePatch
        {
            public static float startingTimer = 0;
            private static bool update = false;
            private static string currentText = "";

            public static void Prefix(GameStartManager __instance)
            {
                if (!GameData.Instance) return; // No instance
                update = GameData.Instance.PlayerCount != __instance.LastPlayerCount;
            }

            public static void Postfix(GameStartManager __instance)
            {
                // Send version as soon as CachedPlayer.LocalPlayer.PlayerControl exists
                if (DevMode.EnableDevMode) __instance.MinPlayers = 1;
                if (PlayerControl.LocalPlayer != null && !versionSent)
                {
                    versionSent = true;
                    VersionHandshake.Share();
                }

                // Check version handshake infos

                var versionMismatch = false;
                var message = "";
                foreach (var client in AmongUsClient.Instance.allClients.ToArray())
                {
                    if (client.Character == null) continue;
                    var dummyComponent = client.Character.GetComponent<DummyBehaviour>();
                    if (dummyComponent != null && dummyComponent.enabled)
                        continue;
                    else if (!PlayerVersions.ContainsKey(client.Id))
                    {
                        versionMismatch = true;
                        message +=
                            $"<color=#FF0000FF>{client.Character.Data.PlayerName} has a different or no version of Better Other Roles\n</color>";
                    }
                    else
                    {
                        var pv = PlayerVersions[client.Id];
                        int diff = BetterOtherRolesPlugin.Version.CompareTo(pv.Version);
                        if (diff > 0)
                        {
                            message +=
                                $"<color=#FF0000FF>{client.Character.Data.PlayerName} has an older version of Better Other Roles (v{PlayerVersions[client.Id].Version.ToString()})\n</color>";
                            versionMismatch = true;
                        }
                        else if (diff < 0)
                        {
                            message +=
                                $"<color=#FF0000FF>{client.Character.Data.PlayerName} has a newer version of Better Other Roles (v{PlayerVersions[client.Id].Version.ToString()})\n</color>";
                            versionMismatch = true;
                        }
                        else if (!pv.GuidMatches())
                        {
                            // version presumably matches, check if Guid matches
                            message +=
                                $"<color=#FF0000FF>{client.Character.Data.PlayerName} has a modified version of BOR v{PlayerVersions[client.Id].Version.ToString()} <size=30%>({pv.Guid.ToString()})</size>\n</color>";
                            versionMismatch = true;
                        }
                    }
                }

                // Display message to the host
                if (AmongUsClient.Instance.AmHost)
                {
                    if (versionMismatch)
                    {
                        __instance.StartButton.color = __instance.startLabelText.color = Palette.DisabledClear;
                        __instance.GameStartText.text = message;
                        __instance.GameStartText.transform.localPosition =
                            __instance.StartButton.transform.localPosition + Vector3.up * 2;
                    }
                    else
                    {
                        __instance.StartButton.color = __instance.startLabelText.color =
                            ((__instance.LastPlayerCount >= __instance.MinPlayers)
                                ? Palette.EnabledColor
                                : Palette.DisabledClear);
                        __instance.GameStartText.transform.localPosition =
                            __instance.StartButton.transform.localPosition;
                    }

                    // Make starting info available to clients:
                    if (startingTimer <= 0 && __instance.startState == GameStartManager.StartingStates.Countdown)
                    {
                        KernelRpc.SetGameStarting();
                    }
                }

                // Client update with handshake infos
                else
                {
                    if (!PlayerVersions.ContainsKey(AmongUsClient.Instance.HostId) ||
                        BetterOtherRolesPlugin.Version.CompareTo(PlayerVersions[AmongUsClient.Instance.HostId].Version) !=
                        0)
                    {
                        kickingTimer += Time.deltaTime;
                        if (kickingTimer > 20)
                        {
                            kickingTimer = 0;
                            AmongUsClient.Instance.ExitGame(DisconnectReasons.ExitGame);
                            SceneChanger.ChangeScene("MainMenu");
                        }

                        __instance.GameStartText.text =
                            $"<color=#FF0000FF>The host has no or a different version of Better Other Roles\nYou will be kicked in {Math.Round(20 - kickingTimer)}s</color>";
                        __instance.GameStartText.transform.localPosition =
                            __instance.StartButton.transform.localPosition + Vector3.up * 2;
                    }
                    else if (versionMismatch)
                    {
                        __instance.GameStartText.text =
                            $"<color=#FF0000FF>Players With Different Versions:\n</color>" + message;
                        __instance.GameStartText.transform.localPosition =
                            __instance.StartButton.transform.localPosition + Vector3.up * 2;
                    }
                    else
                    {
                        __instance.GameStartText.transform.localPosition =
                            __instance.StartButton.transform.localPosition;
                        if (__instance.startState != GameStartManager.StartingStates.Countdown && startingTimer <= 0)
                        {
                            __instance.GameStartText.text = string.Empty;
                        }
                        else
                        {
                            __instance.GameStartText.text = $"Starting in {(int)startingTimer + 1}";
                            if (startingTimer <= 0)
                            {
                                __instance.GameStartText.text = string.Empty;
                            }
                        }
                    }
                }

                // Start Timer
                if (startingTimer > 0)
                {
                    startingTimer -= Time.deltaTime;
                }

                // Lobby timer
                if (!GameData.Instance) return; // No instance

                if (update) currentText = __instance.PlayerCounter.text;

                timer = Mathf.Max(0f, timer -= Time.deltaTime);
                int minutes = (int)timer / 60;
                int seconds = (int)timer % 60;
                string suffix = $" ({minutes:00}:{seconds:00})";

                __instance.PlayerCounter.text = currentText + suffix;
                __instance.PlayerCounter.autoSizeTextContainer = true;
            }
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
        public class GameStartManagerBeginGame
        {
            public static bool Prefix(GameStartManager __instance)
            {
                // Block game start if not everyone has the same mod version
                bool continueStart = true;

                if (AmongUsClient.Instance.AmHost)
                {
                    foreach (InnerNet.ClientData client in AmongUsClient.Instance.allClients.GetFastEnumerator())
                    {
                        if (client.Character == null) continue;
                        var dummyComponent = client.Character.GetComponent<DummyBehaviour>();
                        if (dummyComponent != null && dummyComponent.enabled)
                            continue;

                        if (!PlayerVersions.ContainsKey(client.Id))
                        {
                            continueStart = false;
                            break;
                        }

                        var pv = PlayerVersions[client.Id];
                        int diff = BetterOtherRolesPlugin.Version.CompareTo(pv.Version);
                        if (diff != 0 || !pv.GuidMatches())
                        {
                            continueStart = false;
                            break;
                        }
                    }

                    if (continueStart && TORMapOptions.gameMode == CustomGamemodes.HideNSeek)
                    {
                        byte mapId = (byte)CustomOptions.HideNSeekMap.SelectionIndex;
                        if (mapId >= 3) mapId++;
                        KernelRpc.DynamicMapOption(mapId);
                    }
                    else if (CustomOptions.DynamicMap && continueStart)
                    {
                        // 0 = Skeld
                        // 1 = Mira HQ
                        // 2 = Polus
                        // 3 = Dleks - deactivated
                        // 4 = Airship
                        // 5 = Submerged
                        byte chosenMapId = 0;
                        var probabilities = new List<float>
                        {
                            CustomOptions.DynamicMapEnableSkeld / 10f,
                            CustomOptions.DynamicMapEnableMira / 10f,
                            CustomOptions.DynamicMapEnablePolus / 10f,
                            CustomOptions.DynamicMapEnableAirShip / 10f,
                            CustomOptions.DynamicMapEnableSubmerged / 10f
                        };

                        // if any map is at 100%, remove all maps that are not!
                        if (probabilities.Contains(1.0f))
                        {
                            for (int i = 0; i < probabilities.Count; i++)
                            {
                                if (probabilities[i] != 1.0) probabilities[i] = 0;
                            }
                        }

                        float sum = probabilities.Sum();
                        if (sum == 0) return continueStart; // All maps set to 0, why are you doing this???
                        for (int i = 0; i < probabilities.Count; i++)
                        {
                            // Normalize to [0,1]
                            probabilities[i] /= sum;
                        }

                        float selection = (float)RolesManager.Rnd.NextDouble();
                        float cumsum = 0;
                        for (byte i = 0; i < probabilities.Count; i++)
                        {
                            cumsum += probabilities[i];
                            if (cumsum > selection)
                            {
                                chosenMapId = i;
                                break;
                            }
                        }

                        // Translate chosen map to presets page and use that maps random map preset page
                        if (CustomOptions.DynamicMapSeparateSettings)
                        {
                            CustomOptions.Preset.UpdateSelection(chosenMapId + 5);
                        }

                        if (chosenMapId >= 3) chosenMapId++; // Skip dlekS
                        KernelRpc.DynamicMapOption(chosenMapId);
                    }
                }

                return continueStart;
            }
        }
    }
}