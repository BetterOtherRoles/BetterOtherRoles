using System.Collections.Generic;
using System.Linq;
using BetterOtherRoles.Utilities.Attributes;
using UnityEngine;

namespace BetterOtherRoles.Modules;

[Autoload]
public static class BetterPolus
{
    public static CustomOption Enabled => CustomOptionHolder.EnableBetterPolus;
    public static CustomOption ReactorCountdown = CustomOptionHolder.PolusReactorCountdown;

    private static readonly Dictionary<EditableObjects, GameObject> Objects = new();
    private static readonly Dictionary<EditableObjects, Console> Consoles = new();
    private static readonly Dictionary<EditableObjects, SystemConsole> SystemConsoles = new();
    private static readonly Dictionary<EditableObjects, Vent> Vents = new();
    private static readonly Dictionary<EditableObjects, Vent> DefaultVentLefts = new();
    private static readonly Dictionary<EditableObjects, Vent> DefaultVentCenters = new();
    private static readonly Dictionary<EditableObjects, Vector3> DefaultPositions = new();
    private static readonly Dictionary<EditableObjects, Vector3> UpdatedPositions = new();
    private static readonly Dictionary<EditableObjects, float> DefaultScales = new();
    private static readonly Dictionary<EditableObjects, float> UpdatedScales = new();
    private static readonly Dictionary<EditableObjects, Transform> DefaultParents = new();

    private static bool isVentFetched;
    private static bool isRoomsFetched;
    private static bool isObjectsFetched;

    private static void Clear()
    {
        IsAdjustmentsDone = false;
        Objects.Clear();
        Consoles.Clear();
        SystemConsoles.Clear();
        Vents.Clear();
        DefaultVentLefts.Clear();
        DefaultVentCenters.Clear();
        DefaultPositions.Clear();
        DefaultScales.Clear();
        DefaultParents.Clear();

        isVentFetched = false;
        isRoomsFetched = false;
        isObjectsFetched = false;
    }

    private static bool IsAdjustmentsDone { get; set; }

    static BetterPolus()
    {
        UpdatedPositions[EditableObjects.DvdScreen] = new Vector3(26.635f, -15.92f, 1f);
        UpdatedPositions[EditableObjects.Vitals] = new Vector3(31.275f, -6.45f, 1f);
        UpdatedPositions[EditableObjects.Wifi] = new Vector3(15.975f, 0.084f, -0.015f);
        UpdatedPositions[EditableObjects.NavTask] = new Vector3(11.07f, -15.298f, -0.015f);
        UpdatedPositions[EditableObjects.TempCold] = new Vector3(7.772f, -17.103f, -0.017f);

        UpdatedScales[EditableObjects.DvdScreen] = 0.75f;
        
        GameEvents.OnGameEnded += Clear;
        GameEvents.OnGameStarted += Start;
    }

    private static void Start()
    {
        Clear();
        if (!ShipStatus.Instance || ShipStatus.Instance.Type != ShipStatus.MapType.Pb) return;
        
        FindPolusObjects();
        
        if (Enabled.getBool())
        {
            if (IsAdjustmentsDone) return;
            ApplyChanges();
        }
        else
        {
            if (!IsAdjustmentsDone) return;
            RevertChanges();
        }
    }
    
    private static void FindPolusObjects()
    {
        var ventsList = Object.FindObjectsOfType<Vent>().ToList();
        var gameObjectsList = Object.FindObjectsOfType<GameObject>().ToList();
        var consolesList = Object.FindObjectsOfType<Console>().ToList();
        var systemConsolesList = Object.FindObjectsOfType<SystemConsole>().ToList();
        
        if (!isVentFetched)
        {
            Vents[EditableObjects.ElectricBuildingVent] =
                ventsList.Find(vent => vent.gameObject.name == "ElectricBuildingVent")!;
            Vents[EditableObjects.ElectricalVent] = ventsList.Find(vent => vent.gameObject.name == "ElectricalVent")!;
            Vents[EditableObjects.ScienceBuildingVent] =
                ventsList.Find(vent => vent.gameObject.name == "ScienceBuildingVent")!;
            Vents[EditableObjects.StorageVent] = ventsList.Find(vent => vent.gameObject.name == "StorageVent")!;

            isVentFetched = IsVentFetched();
        }
        
        if (!isRoomsFetched)
        {
            Objects[EditableObjects.CommunicationsRoom] = gameObjectsList.Find(o => o.name == "Comms")!;
            Objects[EditableObjects.WeaponsRoom] = gameObjectsList.Find(o => o.name == "Weapons");
            Objects[EditableObjects.WeaponsGun]= gameObjectsList.Find(o => o.name == "gun0001");
            Objects[EditableObjects.DropshipRoom] = gameObjectsList.Find(o => o.name == "Dropship")!;
            Objects[EditableObjects.DropshipRamp] = gameObjectsList.Find(o => o.name == "ramp");
            Objects[EditableObjects.OutsideRoom] = gameObjectsList.Find(o => o.name == "Outside")!;
            Objects[EditableObjects.ScienceRoom] = gameObjectsList.Find(o => o.name == "Science")!;

            isRoomsFetched = IsRoomsFetched();
        }
        
        

        if (!isObjectsFetched)
        {
            var dvdScreenAdmin = gameObjectsList.Find(o => o.name == "dvdscreen");
            
            Consoles[EditableObjects.Wifi] = consolesList.Find(console => console.name == "panel_wifi")!;
            Consoles[EditableObjects.NavTask] = consolesList.Find(console => console.name == "panel_nav")!;
            
            Consoles[EditableObjects.TempCold] = consolesList.Find(console => console.name == "panel_tempcold")!;
            
            SystemConsoles[EditableObjects.Vitals] = systemConsolesList.Find(console => console.name == "panel_vitals")!;
            Objects[EditableObjects.DvdScreen] = Object.Instantiate(dvdScreenAdmin)!;
            
            isObjectsFetched = IsObjectsFetched();
        }
    }

    private static void AdjustRoomsZ()
    {
        AdjustZPosition(Objects[EditableObjects.CommunicationsRoom], 2f);
        AdjustZPosition(Objects[EditableObjects.WeaponsRoom], 2f);
        AdjustZPosition(Objects[EditableObjects.DropshipRoom], 2f);
        AdjustZPosition(Objects[EditableObjects.DropshipRamp], 1.9f);
        AdjustZPosition(Objects[EditableObjects.WeaponsGun], -2f);
    }

    private static void ApplyChanges()
    {
        AdjustRoomsZ();
        AdjustPolus();
        IsAdjustmentsDone = true;
    }

    private static void RevertChanges()
    {
        RevertPolusAdjustments();
        IsAdjustmentsDone = false;
    }

    private static void AdjustPolus()
    {
        if (isObjectsFetched && isRoomsFetched)
        {
            MoveVitals();
            SwitchNavAndWifi();
            MoveTempCold();
        }
        
        if (isVentFetched)
            AdjustVents();
    }

    private static void RevertPolusAdjustments()
    {
        if (isObjectsFetched && isRoomsFetched)
        {
            MoveVitalsRevert();
            SwitchNavAndWifiRevert();
            MoveTempColdRevert();
        }
        
        if (isVentFetched)
            AdjustVentsRevert();
    }

    private static void AdjustZPosition(GameObject obj, float z)
    {
        if (!obj) return;
        var pos = obj.transform.position;
        pos.z = z;
        obj.transform.position = pos;
    }

    private static void AdjustVents()
    {
        DefaultVentLefts[EditableObjects.ElectricBuildingVent] = Vents[EditableObjects.ElectricBuildingVent].Left;
        Vents[EditableObjects.ElectricBuildingVent].Left = Vents[EditableObjects.ElectricalVent];

        DefaultVentCenters[EditableObjects.ElectricalVent] = Vents[EditableObjects.ElectricalVent];
        Vents[EditableObjects.ElectricalVent].Center = Vents[EditableObjects.ElectricBuildingVent];

        DefaultVentLefts[EditableObjects.ScienceBuildingVent] = Vents[EditableObjects.ScienceBuildingVent].Left;
        Vents[EditableObjects.ScienceBuildingVent].Left = Vents[EditableObjects.StorageVent];

        DefaultVentCenters[EditableObjects.StorageVent] = Vents[EditableObjects.StorageVent].Left;
        Vents[EditableObjects.StorageVent].Left = Vents[EditableObjects.ScienceBuildingVent];
    }

    private static void AdjustVentsRevert()
    {
        Vents[EditableObjects.ElectricBuildingVent].Left = DefaultVentLefts[EditableObjects.ElectricBuildingVent];
        Vents[EditableObjects.ElectricalVent].Center = DefaultVentCenters[EditableObjects.ElectricalVent];

        Vents[EditableObjects.ScienceBuildingVent].Left = DefaultVentLefts[EditableObjects.ScienceBuildingVent];
        Vents[EditableObjects.StorageVent].Center = DefaultVentCenters[EditableObjects.StorageVent];
    }

    private static void MoveTempCold()
    {
        var tempColdTransform = Consoles[EditableObjects.TempCold].transform;
        DefaultParents[EditableObjects.TempCold] = tempColdTransform.parent;
        tempColdTransform.parent = Objects[EditableObjects.OutsideRoom].transform;
        DefaultPositions[EditableObjects.TempCold] = tempColdTransform.position;
        tempColdTransform.position = UpdatedPositions[EditableObjects.TempCold];
        var collider = Consoles[EditableObjects.TempCold].GetComponent<BoxCollider2D>();
        collider.isTrigger = false;
        collider.size += new Vector2(0f, -0.3f);
    }

    private static void MoveTempColdRevert()
    {
        var tempColdTransform = Consoles[EditableObjects.TempCold].transform;
        tempColdTransform.parent = DefaultParents[EditableObjects.TempCold];
        tempColdTransform.position = DefaultPositions[EditableObjects.TempCold];
        var collider = Consoles[EditableObjects.TempCold].GetComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size -= new Vector2(0f, -0.3f);
    }

    private static void SwitchNavAndWifi()
    {
        var wifiTransform = Consoles[EditableObjects.Wifi].transform;
        DefaultParents[EditableObjects.Wifi] = wifiTransform.parent;
        wifiTransform.parent = Objects[EditableObjects.DropshipRoom].transform;
        DefaultPositions[EditableObjects.Wifi] = wifiTransform.position;
        wifiTransform.position = UpdatedPositions[EditableObjects.Wifi];

        var navTransform = Consoles[EditableObjects.NavTask].transform;
        DefaultParents[EditableObjects.NavTask] = navTransform.parent;
        navTransform.parent = Objects[EditableObjects.CommunicationsRoom].transform;
        DefaultPositions[EditableObjects.NavTask] = navTransform.position;
        navTransform.position = UpdatedPositions[EditableObjects.NavTask];
        Consoles[EditableObjects.NavTask].checkWalls = true;
    }

    private static void SwitchNavAndWifiRevert()
    {
        var wifiTransform = Consoles[EditableObjects.Wifi].transform;
        wifiTransform.parent = DefaultParents[EditableObjects.Wifi];
        wifiTransform.position = DefaultPositions[EditableObjects.Wifi];

        var navTransform = Consoles[EditableObjects.NavTask].transform;
        navTransform.parent = DefaultParents[EditableObjects.NavTask];
        navTransform.position = DefaultPositions[EditableObjects.NavTask];
        Consoles[EditableObjects.NavTask].checkWalls = false;
    }

    private static void MoveVitals()
    {
        var vitalsTransform = SystemConsoles[EditableObjects.Vitals].gameObject.transform;
        DefaultParents[EditableObjects.Vitals] = vitalsTransform.parent;
        vitalsTransform.parent = Objects[EditableObjects.ScienceRoom].transform;
        DefaultPositions[EditableObjects.Vitals] = vitalsTransform.position;
        vitalsTransform.position = UpdatedPositions[EditableObjects.Vitals];

        var dvdScreenTransform = Objects[EditableObjects.DvdScreen].transform;
        DefaultPositions[EditableObjects.DvdScreen] = dvdScreenTransform.position;
        dvdScreenTransform.position = UpdatedPositions[EditableObjects.DvdScreen];
        var localScale = dvdScreenTransform.localScale;
        DefaultScales[EditableObjects.DvdScreen] = localScale.x;
        localScale = new Vector3(UpdatedScales[EditableObjects.DvdScreen], localScale.y, localScale.z);
        dvdScreenTransform.localScale = localScale;
    }

    private static void MoveVitalsRevert()
    {
        var vitalsTransform = SystemConsoles[EditableObjects.Vitals].gameObject.transform;
        vitalsTransform.parent = DefaultParents[EditableObjects.Vitals];
        vitalsTransform.position = DefaultPositions[EditableObjects.Vitals];

        var dvdScreenTransform = Objects[EditableObjects.DvdScreen].transform;
        dvdScreenTransform.position = DefaultPositions[EditableObjects.DvdScreen];
        var localScale = dvdScreenTransform.localScale;
        localScale = new Vector3(DefaultScales[EditableObjects.DvdScreen], localScale.y, localScale.z);
        dvdScreenTransform.localScale = localScale;
    }

    private static bool IsVentFetched()
    {
        if (!Vents.TryGetValue(EditableObjects.ElectricBuildingVent, out var electricBuildingVent) ||
            !electricBuildingVent) return false;
        if (!Vents.TryGetValue(EditableObjects.ElectricalVent, out var electricalVent) ||
            !electricalVent) return false;
        if (!Vents.TryGetValue(EditableObjects.ScienceBuildingVent, out var scienceBuildingVent) ||
            !scienceBuildingVent) return false;
        if (!Vents.TryGetValue(EditableObjects.StorageVent, out var storageVent) ||
            !storageVent) return false;

        return true;
    }

    private static bool IsRoomsFetched()
    {
        if (!Objects.TryGetValue(EditableObjects.CommunicationsRoom, out var communicationRoom) ||
            !communicationRoom) return false;
        if (!Objects.TryGetValue(EditableObjects.WeaponsRoom, out var weaponsRoom) || 
            !weaponsRoom) return false;
        if (!Objects.TryGetValue(EditableObjects.DropshipRoom, out var dropshipRoom) ||
            !dropshipRoom) return false;
        if (!Objects.TryGetValue(EditableObjects.OutsideRoom, out var outsideRoom) ||
            !outsideRoom) return false;
        if (!Objects.TryGetValue(EditableObjects.ScienceRoom, out var scienceRoom) ||
            !scienceRoom) return false;

        return true;
    }

    private static bool IsObjectsFetched()
    {
        if (!Objects.TryGetValue(EditableObjects.WeaponsGun, out var weaponsGun) || !weaponsGun) return false;
        if (!Objects.TryGetValue(EditableObjects.DropshipRamp, out var dropshipRamp) || !dropshipRamp) return false;
        if (!Consoles.TryGetValue(EditableObjects.NavTask, out var navTask) || !navTask) return false;
        if (!SystemConsoles.TryGetValue(EditableObjects.Vitals, out var vitals) || !vitals) return false;
        if (!Objects.TryGetValue(EditableObjects.DvdScreen, out var dvdScreen) || !dvdScreen) return false;
        if (!Consoles.TryGetValue(EditableObjects.TempCold, out var tempCold) || !tempCold) return false;

        return true;
    }

    private enum EditableObjects
    {
        DvdScreen,
        Vitals,
        Wifi,
        NavTask,
        TempCold,
        CommunicationsRoom,
        DropshipRoom,
        DropshipRamp,
        WeaponsRoom,
        WeaponsGun,
        OutsideRoom,
        ScienceRoom,
        ElectricBuildingVent,
        ElectricalVent,
        ScienceBuildingVent,
        StorageVent
    }
}