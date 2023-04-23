using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using Hazel;
using System;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;
using System.Linq;
using TheOtherRoles.Modules;

namespace TheOtherRoles.Patches {
    public class GameStartManagerPatch  {
        public static readonly Dictionary<int, PlayerVersion> playerVersions = new();
        public static float timer = 600f;
        private static float kickingTimer;
        private static bool versionSent;
        private static string lobbyCodeText = string.Empty;

        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
        public class AmongUsClientOnPlayerJoinedPatch {
            public static void Postfix(AmongUsClient __instance) {
                if (CachedPlayer.LocalPlayer != null) {
                    Helpers.shareGameVersion();
                }
            }
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
        public class GameStartManagerStartPatch {
            public static void Postfix(GameStartManager __instance) {
                // Trigger version refresh
                versionSent = false;
                // Reset lobby countdown timer
                timer = 600f; 
                // Reset kicking timer
                kickingTimer = 0f;
                // Copy lobby code
                var code = InnerNet.GameCode.IntToGameName(AmongUsClient.Instance.GameId);
                GUIUtility.systemCopyBuffer = code;
                lobbyCodeText = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.RoomCode, new Il2CppReferenceArray<Il2CppSystem.Object>(0)) + "\r\n" + code;
            }
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
        public class GameStartManagerUpdatePatch {
            public static float StartingTimer = 0;
            private static bool update;
            private static string currentText = string.Empty;
        
            public static void Prefix(GameStartManager __instance) {
                if (!GameData.Instance ) return; // No instance
                update = GameData.Instance.PlayerCount != __instance.LastPlayerCount;
            }

            public static void Postfix(GameStartManager __instance) {
                // Send version as soon as CachedPlayer.LocalPlayer.PlayerControl exists
                if (PlayerControl.LocalPlayer != null && !versionSent) {
                    versionSent = true;
                    Helpers.shareGameVersion();
                }

                // Check version handshake infos

                var versionMismatch = false;
                var message = "";
                foreach (var client in AmongUsClient.Instance.allClients.ToArray()) {
                    if (client.Character == null) continue;
                    var dummyComponent = client.Character.GetComponent<DummyBehaviour>();
                    if (dummyComponent != null && dummyComponent.enabled)
                        continue;
                    else if (!playerVersions.ContainsKey(client.Id))  {
                        versionMismatch = true;
                        message += $"<color=#FF0000FF>{client.Character.Data.PlayerName} has a different or no version of The Other Roles\n</color>";
                    } else {
                        var pv = playerVersions[client.Id];
                        var diff = TheOtherRolesPlugin.Version.CompareTo(pv.Version);
                        switch (diff)
                        {
                            case > 0:
                                message += $"<color=#FF0000FF>{client.Character.Data.PlayerName} has an older version of The Other Roles (v{playerVersions[client.Id].Version.ToString()})\n</color>";
                                versionMismatch = true;
                                break;
                            case < 0:
                                message += $"<color=#FF0000FF>{client.Character.Data.PlayerName} has a newer version of The Other Roles (v{playerVersions[client.Id].Version.ToString()})\n</color>";
                                versionMismatch = true;
                                break;
                            default:
                            {
                                if (!pv.GuidMatches()) { // version presumably matches, check if Guid matches
                                    message += $"<color=#FF0000FF>{client.Character.Data.PlayerName} has a modified version of TOR v{playerVersions[client.Id].Version.ToString()} <size=30%>({pv.Guid.ToString()})</size>\n</color>";
                                    versionMismatch = true;
                                }

                                break;
                            }
                        }
                    }
                }

                // Display message to the host
                if (AmongUsClient.Instance.AmHost) {
                    if (versionMismatch) {
                        __instance.StartButton.color = __instance.startLabelText.color = Palette.DisabledClear;
                        __instance.GameStartText.text = message;
                        __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition + Vector3.up * 2;
                    } else {
                        __instance.StartButton.color = __instance.startLabelText.color = ((__instance.LastPlayerCount >= __instance.MinPlayers) ? Palette.EnabledColor : Palette.DisabledClear);
                        __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition;
                    }

                    // Make starting info available to clients:
                    if (StartingTimer <= 0 && __instance.startState == GameStartManager.StartingStates.Countdown) {
                        var writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.SetGameStarting, Hazel.SendOption.Reliable, -1);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.setGameStarting();
                    }
                }

                // Client update with handshake infos
                else {
                    if (!playerVersions.ContainsKey(AmongUsClient.Instance.HostId) || TheOtherRolesPlugin.Version.CompareTo(playerVersions[AmongUsClient.Instance.HostId].Version) != 0) {
                        kickingTimer += Time.deltaTime;
                        if (kickingTimer > 10) {
                            kickingTimer = 0;
			                AmongUsClient.Instance.ExitGame(DisconnectReasons.ExitGame);
                            SceneChanger.ChangeScene("MainMenu");
                        }

                        __instance.GameStartText.text = $"<color=#FF0000FF>The host has no or a different version of The Other Roles\nYou will be kicked in {Math.Round(10 - kickingTimer)}s</color>";
                        __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition + Vector3.up * 2;
                    } else if (versionMismatch) {
                        __instance.GameStartText.text = $"<color=#FF0000FF>Players With Different Versions:\n</color>" + message;
                        __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition + Vector3.up * 2;
                    } else {
                        __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition;
                        if (__instance.startState != GameStartManager.StartingStates.Countdown && StartingTimer <= 0) {
                            __instance.GameStartText.text = string.Empty;
                        }
                        else {
                            __instance.GameStartText.text = $"Starting in {(int)StartingTimer + 1}";
                            if (StartingTimer <= 0) {
                                __instance.GameStartText.text = string.Empty;
                            }
                        }
                    }
                }

                // Start Timer
                if (StartingTimer > 0) {
                    StartingTimer -= Time.deltaTime;
                }
                // Lobby timer
                if (!GameData.Instance) return; // No instance

                if (update) currentText = __instance.PlayerCounter.text;

                timer = Mathf.Max(0f, timer -= Time.deltaTime);
                var minutes = (int)timer / 60;
                var seconds = (int)timer % 60;
                var suffix = $" ({minutes:00}:{seconds:00})";

                __instance.PlayerCounter.text = currentText + suffix;
                __instance.PlayerCounter.autoSizeTextContainer = true;

                if (AmongUsClient.Instance.AmHost) {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.ShareGamemode, Hazel.SendOption.Reliable, -1);
                    writer.Write((byte) TORMapOptions.gameMode);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.shareGamemode((byte) TORMapOptions.gameMode);
                }
            }
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
        public class GameStartManagerBeginGame {
            public static bool Prefix(GameStartManager __instance) {
                // Block game start if not everyone has the same mod version
                var continueStart = true;

                if (!AmongUsClient.Instance.AmHost) return continueStart;
                foreach (var client in AmongUsClient.Instance.allClients.GetFastEnumerator()) {
                    if (client.Character == null) continue;
                    var dummyComponent = client.Character.GetComponent<DummyBehaviour>();
                    if (dummyComponent != null && dummyComponent.enabled)
                        continue;
                        
                    if (!playerVersions.ContainsKey(client.Id)) {
                        continueStart = false;
                        break;
                    }
                        
                    var pv = playerVersions[client.Id];
                    var diff = TheOtherRolesPlugin.Version.CompareTo(pv.Version);
                    if (diff == 0 && pv.GuidMatches()) continue;
                    continueStart = false;
                    break;
                }
                if (continueStart && TORMapOptions.gameMode == CustomGamemodes.HideNSeek) {
                    var mapId = (byte) CustomOptionHolder.hideNSeekMap.getSelection();
                    if (mapId >= 3) mapId++;
                    var writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.DynamicMapOption, Hazel.SendOption.Reliable, -1);
                    writer.Write(mapId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.dynamicMapOption(mapId);
                }            
                else if (CustomOptionHolder.dynamicMap.getBool() && continueStart) {
                    // 0 = Skeld
                    // 1 = Mira HQ
                    // 2 = Polus
                    // 3 = Dleks - deactivated
                    // 4 = Airship
                    // 5 = Submerged
                    byte chosenMapId = 0;
                    var probabilities = new List<float>();
                    probabilities.Add(CustomOptionHolder.dynamicMapEnableSkeld.getSelection() / 10f);
                    probabilities.Add(CustomOptionHolder.dynamicMapEnableMira.getSelection() / 10f);
                    probabilities.Add(CustomOptionHolder.dynamicMapEnablePolus.getSelection() / 10f);
                    probabilities.Add(CustomOptionHolder.dynamicMapEnableAirShip.getSelection() / 10f);
                    probabilities.Add(CustomOptionHolder.dynamicMapEnableSubmerged.getSelection() / 10f);

                    // if any map is at 100%, remove all maps that are not!
                    if (probabilities.Contains(1.0f)) {
                        for (var i=0; i < probabilities.Count; i++) {
                            if (probabilities[i] != 1.0) probabilities[i] = 0;
                        }
                    }

                    var sum = probabilities.Sum();
                    if (sum == 0) return continueStart;  // All maps set to 0, why are you doing this???
                    for (var i = 0; i < probabilities.Count; i++) {  // Normalize to [0,1]
                        probabilities[i] /= sum;
                    }
                    var selection = (float)TheOtherRoles.rnd.NextDouble();
                    var cumSum = 0f;
                    for (byte i = 0; i < probabilities.Count; i++) {
                        cumSum += probabilities[i];
                        if (!(cumSum > selection)) continue;
                        chosenMapId = i;
                        break;
                    }
                    if (chosenMapId >= 3) chosenMapId++;  // Skip dlekS
                                                              
                    var writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.DynamicMapOption, Hazel.SendOption.Reliable, -1);
                    writer.Write(chosenMapId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.dynamicMapOption(chosenMapId);
                }
                return continueStart;
            }
        }

        public class PlayerVersion {
            public readonly Version Version;
            public readonly Guid Guid;

            public PlayerVersion(Version version, Guid guid) {
                Version = version;
                Guid = guid;
            }

            public bool GuidMatches() {
                return CustomGuid.Guid.Equals(Guid);
            }
        }
    }
}
