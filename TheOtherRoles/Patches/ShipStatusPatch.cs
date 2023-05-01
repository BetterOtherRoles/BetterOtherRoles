using System.Linq;
using HarmonyLib;
using TheOtherRoles.Utilities;
using static TheOtherRoles.TheOtherRoles;
using TheOtherRoles.Modules;
using UnityEngine;
using TheOtherRoles.CustomGameModes;
using AmongUs.GameOptions;
using TheOtherRoles.Customs.Modifiers;
using TheOtherRoles.Customs.Roles.Crewmate;
using TheOtherRoles.Customs.Roles.Impostor;
using TheOtherRoles.Customs.Roles.Neutral;

namespace TheOtherRoles.Patches 
{

    [HarmonyPatch(typeof(ShipStatus))]
    public static class ShipStatusPatch 
    {
        
        private static int originalNumCommonTasksOption = 0;
        private static int originalNumShortTasksOption = 0;
        private static int originalNumLongTasksOption = 0;
        public static float originalNumCrewVisionOption = 0;
        public static float originalNumImpVisionOption = 0;
        public static float originalNumKillCooldownOption = 0;

        // Better TOR modifications start here :
        // Better Polus.

        // Positions
        public static readonly Vector3 DvdScreenNewPos = new Vector3(26.635f, -15.92f, 1f);
        public static readonly Vector3 VitalsNewPos = new Vector3(31.275f, -6.45f, 1f);
        
        public static readonly Vector3 WifiNewPos = new Vector3(15.975f, 0.084f, 1f);
        public static readonly Vector3 NavNewPos = new Vector3(11.07f, -15.298f, -0.015f);
        
        public static readonly Vector3 TempColdNewPos = new Vector3(7.772f, -17.103f, -0.017f);
        
        // Scales
        public const float DvdScreenNewScale = 0.75f;

        // Checks
        public static bool IsAdjustmentsDone;
        public static bool IsObjectsFetched;
        public static bool IsRoomsFetched;
        public static bool IsVentsFetched;

        // Tasks Tweak
        public static Console WifiConsole;
        public static Console NavConsole;

        // Vitals Tweak
        public static SystemConsole Vitals;
        public static GameObject DvdScreenOffice;

        // Vents Tweak
        public static Vent ElectricBuildingVent;
        public static Vent ElectricalVent;
        public static Vent ScienceBuildingVent;
        public static Vent StorageVent;
        
        // TempCold Tweak
        public static Console TempCold;
        
        // Rooms
        public static GameObject Comms;
        public static GameObject DropShip;
        public static GameObject Outside;
        public static GameObject Science;

        // Better TOR modifications end here.
        
        [HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.IsGameOverDueToDeath))]
        public static class LogicGameFlowNormalIsGameOverDueToDeathPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch]
            public static void Postfix(ShipStatus __instance, ref bool __result)
            {
                __result = false;
            }
        }
        
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
        public static class ShipStatusCalculateLightRadiusPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch]
            public static bool Prefix(ref float __result, ShipStatus __instance, [HarmonyArgument(0)] GameData.PlayerInfo player) {
                if (!__instance.Systems.ContainsKey(SystemTypes.Electrical) || GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return true;

                
                if (!HideNSeek.isHideNSeekGM || (HideNSeek.isHideNSeekGM && !Hunter.lightActive.Contains(player.PlayerId))) {
                    // If player is a role which has Impostor vision
                    if (Helpers.hasImpVision(player)) {
                        //__result = __instance.MaxLightRadius * GameOptionsManager.Instance.currentNormalGameOptions.ImpostorLightMod;
                        __result = GetNeutralLightRadius(__instance, true);
                        return false;
                    }
                }

                // If player is Lighter with ability active
                if (Singleton<Lighter>.Instance.Player != null && Singleton<Lighter>.Instance.Player.PlayerId == player.PlayerId) {
                    float unlerped = Mathf.InverseLerp(__instance.MinLightRadius, __instance.MaxLightRadius, GetNeutralLightRadius(__instance, false));
                    __result = Mathf.Lerp(__instance.MaxLightRadius * Singleton<Lighter>.Instance.PlayerModeLightsOffVision, __instance.MaxLightRadius * Singleton<Lighter>.Instance.PlayerModeLightsOnVision, unlerped);
                }

                // If Game mode is Hide N Seek and hunter with ability active
                else if (HideNSeek.isHideNSeekGM && Hunter.isLightActive(player.PlayerId)) {
                    float unlerped = Mathf.InverseLerp(__instance.MinLightRadius, __instance.MaxLightRadius, GetNeutralLightRadius(__instance, false));
                    __result = Mathf.Lerp(__instance.MaxLightRadius * Hunter.lightVision, __instance.MaxLightRadius * Hunter.lightVision, unlerped);
                }

                // If there is a Trickster with their ability active
                else if (Singleton<Trickster>.Instance.Player != null && Trickster.lightsOutTimer > 0f) {
                    float lerpValue = 1f;
                    if (Trickster.lightsOutDuration - Trickster.lightsOutTimer < 0.5f) {
                        lerpValue = Mathf.Clamp01((Trickster.lightsOutDuration - Trickster.lightsOutTimer) * 2);
                    } else if (Trickster.lightsOutTimer < 0.5) {
                        lerpValue = Mathf.Clamp01(Trickster.lightsOutTimer * 2);
                    }

                    __result = Mathf.Lerp(__instance.MinLightRadius, __instance.MaxLightRadius, 1 - lerpValue) * GameOptionsManager.Instance.currentNormalGameOptions.CrewLightMod;
                }

                // If player is Lawyer, apply Lawyer vision modifier
                else if (Singleton<Lawyer>.Instance.Player != null && Singleton<Lawyer>.Instance.Player.PlayerId == player.PlayerId) {
                    float unlerped = Mathf.InverseLerp(__instance.MinLightRadius, __instance.MaxLightRadius, GetNeutralLightRadius(__instance, false));
                    __result = Mathf.Lerp(__instance.MinLightRadius, __instance.MaxLightRadius * Lawyer.vision, unlerped);
                    return false;
                }

                // Default light radius
                else {
                    __result = GetNeutralLightRadius(__instance, false);
                }
                if (Sunglasses.sunglasses.FindAll(x => x.PlayerId == player.PlayerId).Count > 0) // Sunglasses
                    __result *= 1f - Sunglasses.vision * 0.1f;

                return false;
            }

            public static float GetNeutralLightRadius(ShipStatus shipStatus, bool isImpostor) {
                if (SubmergedCompatibility.IsSubmerged) {
                    return SubmergedCompatibility.GetSubmergedNeutralLightRadius(isImpostor);
                }

                if (isImpostor) return shipStatus.MaxLightRadius * GameOptionsManager.Instance.currentNormalGameOptions.ImpostorLightMod;

                SwitchSystem switchSystem = MapUtilities.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
                float lerpValue = switchSystem.Value / 255f;

                return Mathf.Lerp(shipStatus.MinLightRadius, shipStatus.MaxLightRadius, lerpValue) * GameOptionsManager.Instance.currentNormalGameOptions.CrewLightMod;
            }
        }

        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
        public static class ShipStatusAwakePatch
        {
            [HarmonyPrefix]
            [HarmonyPatch]
            public static void Prefix(ShipStatus __instance)
            {
                IsAdjustmentsDone = false;
                IsObjectsFetched = false;
                IsRoomsFetched = false;
                IsVentsFetched = false;

                if(CustomOptionHolder.enableBetterPolus.getBool())
                    ApplyChanges(__instance);

                if(CustomOptionHolder.enableBetterSkeld.getBool())
                {
                    ClassInjector.RegisterTypeInIl2Cpp<SkeldPatcher>();
                    
                    var skeldpatcher = new GameObject("BetterSkeld Patcher");
                    if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 0)
                        skeldpatcher.AddComponent<SkeldPatcher>();

                    // var dlekspatcher = new GameObject("BetterKleds Patcher");
                    // if(GameOptionsManager.Instance.currentNormalGameOptions.MapId == 3)
                    //    dlekspatcher.AddComponent<DleksPatcher>();
                }
            }
        }


        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.FixedUpdate))]
        public static class ShipStatusFixedUpdatePatch
        {
            [HarmonyPrefix]
            [HarmonyPatch]
            public static void Prefix(ShipStatus __instance)
            {
                if ((!IsObjectsFetched || !IsAdjustmentsDone) && CustomOptionHolder.enableBetterPolus.getBool())
                {
                    ApplyChanges(__instance);
                }
            }
        }

        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Begin))]
        public static class ShipStatusBeginPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch]
            public static bool Prefix(ShipStatus __instance)
            {
                if(CustomOptionHolder.enableBetterPolus.getBool())
                    ApplyChanges(__instance);

                originalNumCommonTasksOption = GameOptionsManager.Instance.currentNormalGameOptions.NumCommonTasks;
                originalNumShortTasksOption = GameOptionsManager.Instance.currentNormalGameOptions.NumShortTasks;
                originalNumLongTasksOption = GameOptionsManager.Instance.currentNormalGameOptions.NumLongTasks;

                if (TORMapOptions.gameMode != CustomGamemodes.HideNSeek) {
                    var commonTaskCount = __instance.CommonTasks.Count;
                    var normalTaskCount = __instance.NormalTasks.Count;
                    var longTaskCount = __instance.LongTasks.Count;

                    if (GameOptionsManager.Instance.currentNormalGameOptions.NumCommonTasks > commonTaskCount) GameOptionsManager.Instance.currentNormalGameOptions.NumCommonTasks = commonTaskCount;
                    if (GameOptionsManager.Instance.currentNormalGameOptions.NumShortTasks > normalTaskCount) GameOptionsManager.Instance.currentNormalGameOptions.NumShortTasks = normalTaskCount;
                    if (GameOptionsManager.Instance.currentNormalGameOptions.NumLongTasks > longTaskCount) GameOptionsManager.Instance.currentNormalGameOptions.NumLongTasks = longTaskCount;
                } else {
                    GameOptionsManager.Instance.currentNormalGameOptions.NumCommonTasks = Mathf.RoundToInt(CustomOptionHolder.hideNSeekCommonTasks.getFloat());
                    GameOptionsManager.Instance.currentNormalGameOptions.NumShortTasks = Mathf.RoundToInt(CustomOptionHolder.hideNSeekShortTasks.getFloat());
                    GameOptionsManager.Instance.currentNormalGameOptions.NumLongTasks = Mathf.RoundToInt(CustomOptionHolder.hideNSeekLongTasks.getFloat());
                }

                return true;
            }

            [HarmonyPostfix]
            public static void Postfix(ShipStatus __instance)
            {
                // Restore original settings after the tasks have been selected
                GameOptionsManager.Instance.currentNormalGameOptions.NumCommonTasks = originalNumCommonTasksOption;
                GameOptionsManager.Instance.currentNormalGameOptions.NumShortTasks = originalNumShortTasksOption;
                GameOptionsManager.Instance.currentNormalGameOptions.NumLongTasks = originalNumLongTasksOption;
            }
        }

        public static void resetVanillaSettings() {
            GameOptionsManager.Instance.currentNormalGameOptions.ImpostorLightMod = originalNumImpVisionOption;
            GameOptionsManager.Instance.currentNormalGameOptions.CrewLightMod = originalNumCrewVisionOption;
            GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown = originalNumKillCooldownOption;
        }
        

        private static void ApplyChanges(ShipStatus instance)
        {
            if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 2)
            {
                FindPolusObjects();
                AdjustPolus();
            }
        }

        public static void FindPolusObjects()
        {
            FindVents();
            FindRooms();
            FindObjects();
        }

        public static void AdjustPolus()
        {
            if (IsObjectsFetched && IsRoomsFetched)
            {
                MoveVitals();
                SwitchNavWifi();
                MoveTempCold();
            }
            else
            {
                TheOtherRolesPlugin.Logger.LogError("Couldn't move elements as not all of them have been fetched.");
            }

            AdjustVents();

            IsAdjustmentsDone = true;
        }
        
        // --------------------
        // - Objects Fetching -
        // --------------------

        public static void FindVents()
        {
            
            if (IsVentsFetched)
                return;
            
            var ventsList = Object.FindObjectsOfType<Vent>().ToList();
            
            ElectricBuildingVent = ventsList.Find(vent => vent.gameObject.name == "ElectricBuildingVent");
            ElectricalVent = ventsList.Find(vent => vent.gameObject.name == "ElectricalVent");
            ScienceBuildingVent = ventsList.Find(vent => vent.gameObject.name == "ScienceBuildingVent");
            StorageVent = ventsList.Find(vent => vent.gameObject.name == "StorageVent");

            IsVentsFetched = ElectricBuildingVent != null && ElectricalVent != null && ScienceBuildingVent != null && StorageVent != null;
        }

        public static void FindRooms()
        {
            if (IsRoomsFetched)
                return;

            var roomList = Object.FindObjectsOfType<GameObject>().ToList();
            
            Comms = roomList.Find(o => o.name == "Comms");
            DropShip = roomList.Find(o => o.name == "Dropship");
            Outside = roomList.Find(o => o.name == "Outside");
            Science = roomList.Find(o => o.name == "Science");
            
            IsRoomsFetched = Comms != null && DropShip != null && Outside != null && Science != null;
        }

        public static void FindObjects()
        {
            if (IsObjectsFetched)
                return;

            var objectList = Object.FindObjectsOfType<Console>().ToList();

            WifiConsole = objectList.Find(console => console.name == "panel_wifi");
            NavConsole = objectList.Find(console => console.name == "panel_nav");
            TempCold = objectList.Find(console => console.name == "panel_tempcold");

            Vitals = Object.FindObjectsOfType<SystemConsole>().ToList()
                    .Find(console => console.name == "panel_vitals");
            
                
            GameObject DvdScreenAdmin = Object.FindObjectsOfType<GameObject>().ToList()
                    .Find(o => o.name == "dvdscreen");

            if (DvdScreenAdmin != null)
            {
                DvdScreenOffice = Object.Instantiate(DvdScreenAdmin);
            }
            
            IsObjectsFetched = WifiConsole != null && NavConsole != null && Vitals != null && DvdScreenOffice != null && TempCold != null;
        }

        // -------------------
        // - Map Adjustments -
        // -------------------
        
        public static void AdjustVents()
        {
            if (IsVentsFetched)
            {
                ElectricBuildingVent.Left = ElectricalVent;
                ElectricalVent.Center = ElectricBuildingVent;

                ScienceBuildingVent.Left = StorageVent;
                StorageVent.Center = ScienceBuildingVent;
            }
            else
            {
                TheOtherRolesPlugin.Logger.LogError("Couldn't adjust Vents as not all objects have been fetched.");
            }
        }

        public static void MoveTempCold()
        {
            if (TempCold.transform.position != TempColdNewPos)
            {
                Transform tempColdTransform = TempCold.transform;
                tempColdTransform.parent = Outside.transform;
                tempColdTransform.position = TempColdNewPos;

                // Fixes collider being too high
                BoxCollider2D collider = TempCold.GetComponent<BoxCollider2D>();
                collider.isTrigger = false;
                collider.size += new Vector2(0f, -0.3f);
            }
        }
        
        public static void SwitchNavWifi()
        {
            if (WifiConsole.transform.position != WifiNewPos)
            {
                Transform wifiTransform = WifiConsole.transform;
                wifiTransform.parent = DropShip.transform;
                wifiTransform.position = WifiNewPos;
            }

            if (NavConsole.transform.position != NavNewPos)
            {
                Transform navTransform = NavConsole.transform;
                navTransform.parent = Comms.transform;
                navTransform.position = NavNewPos;
                
                // Prevents crewmate being able to do the task from outside
                NavConsole.checkWalls = true;
            }
        }
        
        public static void MoveVitals()
        {
            if (Vitals.transform.position != VitalsNewPos)
            {
                // Vitals
                Transform vitalsTransform = Vitals.gameObject.transform;
                vitalsTransform.parent = Science.transform;
                vitalsTransform.position = VitalsNewPos;
            }

            if (DvdScreenOffice.transform.position != DvdScreenNewPos)
            {
                // DvdScreen
                Transform dvdScreenTransform = DvdScreenOffice.transform;
                dvdScreenTransform.position = DvdScreenNewPos;
                
                var localScale = dvdScreenTransform.localScale;
                localScale =
                    new Vector3(DvdScreenNewScale, localScale.y,
                        localScale.z);
                dvdScreenTransform.localScale = localScale;
            }
        }
    }
}
