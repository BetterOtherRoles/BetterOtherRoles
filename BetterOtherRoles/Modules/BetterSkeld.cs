using System.Collections.Generic;
using System.Linq;
using BetterOtherRoles.Utilities.Attributes;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace BetterOtherRoles.Modules;

[Autoload]
public static class BetterSkeld
{
    public static CustomOption Enabled => CustomOptionHolder.EnableBetterSkeld;
    // private static CustomOption ReactorCountdown = CustomOptionHolder.PolusReactorCountdown;

    private static readonly Dictionary<EditableObjects, GameObject> Objects = new();
    private static readonly Dictionary<EditableObjects, Console> Consoles = new();
    private static readonly Dictionary<EditableObjects, SystemConsole> SystemConsoles = new();
    private static readonly Dictionary<EditableObjects, Vent> Vents = new();
    private static readonly Dictionary<EditableObjects, Vent> DefaultVentLefts = new();
    private static readonly Dictionary<EditableObjects, Vent> DefaultVentCenters = new();
    private static readonly Dictionary<EditableObjects, Vent> DefaultVentRights = new();
    private static readonly Dictionary<EditableObjects, Vector3> DefaultPositions = new();
    private static readonly Dictionary<EditableObjects, Vector3> UpdatedPositions = new();
    private static readonly Dictionary<EditableObjects, Vector3> DefaultScales = new();
    private static readonly Dictionary<EditableObjects, Vector3> UpdatedScales = new();
    private static readonly Dictionary<EditableObjects, Quaternion> DefaultRotations = new();
    private static readonly Dictionary<EditableObjects, Quaternion> UpdatedRotations = new();
    private static readonly Dictionary<EditableObjects, Transform> DefaultParents = new();

    private static bool isVentsFetched;
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
        DefaultVentRights.Clear();
        DefaultPositions.Clear();
        DefaultScales.Clear();
        DefaultRotations.Clear();
        DefaultParents.Clear();

        isVentsFetched = false;
        isRoomsFetched = false;
        isObjectsFetched = false;
    }

    private static bool IsAdjustmentsDone { get; set; }

    static BetterSkeld()
    {
        UpdatedPositions[EditableObjects.Vitals] = new Vector3(1.9162f, -16.1985f, -2.4142f);
        
        UpdatedRotations[EditableObjects.Vitals] = Quaternion.Euler(0f, 0f, 90f);
        
        UpdatedScales[EditableObjects.Vitals] = new Vector3(0.6636f, 0.7418f, 1f);
        
        GameEvents.OnGameEnded += Clear;
        GameEvents.OnGameStarted += Start;
    }
    
    private static void Start()
    {
        Clear();
        BetterOtherRolesPlugin.Logger.LogDebug("Patching TheSkeld...");
        if (!ShipStatus.Instance || GameOptionsManager.Instance.currentNormalGameOptions.MapId != 0) return;
        BetterOtherRolesPlugin.Logger.LogDebug("Patching Objects...");
        FindSkeldObjects();
        
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

    private static void FindSkeldObjects()
    {
        var ventsList = Object.FindObjectsOfType<Vent>().ToList();
        var consolesList = Object.FindObjectsOfType<Console>().ToList();
        var gameObjects = Object.FindObjectsOfType<GameObject>().ToList();
        
        var res = Addressables.LoadAssetAsync<GameObject>(AmongUsClient.Instance.ShipPrefabs[(System.Index) (int) ShipStatus.MapType.Pb]).Result;
        if (!res)
            return;

        if (!isVentsFetched)
        {
            Vents[EditableObjects.AdminVent] = ventsList.Find(o => o.name == "AdminVent");
            Vents[EditableObjects.CafeteriaVent] = ventsList.Find(o => o.name == "CafeVent");
            Vents[EditableObjects.NavigationUpperVent] = ventsList.Find(o => o.name == "NavVentNorth");
            Vents[EditableObjects.NavigationLowerVent] = ventsList.Find(o => o.name == "NavVentSouth");
            Vents[EditableObjects.WeaponsVent] = ventsList.Find(o => o.name == "WeaponsVent");
            Vents[EditableObjects.ShieldVent] = ventsList.Find(o => o.name == "ShieldsVent");
            Vents[EditableObjects.CorridorVent] = ventsList.Find(o => o.name == "BigYVent");
            Vents[EditableObjects.ElectricalVent] = ventsList.Find(o => o.name == "ElecVent");
            Vents[EditableObjects.SecurityVent] = ventsList.Find(o => o.name == "SecurityVent");
            Vents[EditableObjects.MedicalBayVent] = ventsList.Find(o => o.name == "MedVent");
            Vents[EditableObjects.ReactorUpperVent] = ventsList.Find(o => o.name == "UpperReactorVent");
            Vents[EditableObjects.ReactorLowerVent] = ventsList.Find(o => o.name == "ReactorVent");
            Vents[EditableObjects.UpperEngineVent] = ventsList.Find(o => o.name == "LEngineVent");
            Vents[EditableObjects.LowerEngineVent] = ventsList.Find(o => o.name == "REngineVent");
            
            isVentsFetched = IsVentsFetched();
            
            BetterOtherRolesPlugin.Logger.LogDebug($"Are vents fetched ? : {isVentsFetched}");
        }

        if (!isObjectsFetched)
        {
            Consoles[EditableObjects.Admin] = consolesList.Find(o => o.name == "MapRoomConsole");
            Objects[EditableObjects.AdminAnimation] = gameObjects.Find(o => o.name == "MapAnimation");
            Objects[EditableObjects.Vitals] = !res ? null : GameObject.Instantiate(res.transform.Find("Office/panel_vitals").gameObject);
            
            isObjectsFetched = IsObjectsFetched();
            
            BetterOtherRolesPlugin.Logger.LogDebug($"Are objects fetched ? : {isObjectsFetched}");
        }

        if (!isRoomsFetched)
        {
            Objects[EditableObjects.CommunicationsRoom] = gameObjects.Find(o => o.name == "Comms");

            isRoomsFetched = IsRoomsFetched();
            
            BetterOtherRolesPlugin.Logger.LogDebug($"Are rooms fetched ? : {isRoomsFetched}");
        }
    }

    private static bool IsVentsFetched()
    {
        if (!Vents.TryGetValue(EditableObjects.AdminVent, out var adminVent) ||
            !adminVent) return false;
        if (!Vents.TryGetValue(EditableObjects.CafeteriaVent, out var cafeteriaVent) ||
            !cafeteriaVent) return false;
        if (!Vents.TryGetValue(EditableObjects.NavigationUpperVent, out var navigationUpperVent) ||
            !navigationUpperVent) return false;
        if (!Vents.TryGetValue(EditableObjects.NavigationLowerVent, out var navigationLowerVent) ||
            !navigationLowerVent) return false;
        if (!Vents.TryGetValue(EditableObjects.WeaponsVent, out var weaponsVent) ||
            !weaponsVent) return false;
        if (!Vents.TryGetValue(EditableObjects.ShieldVent, out var shieldVent) ||
            !shieldVent) return false;
        if (!Vents.TryGetValue(EditableObjects.CorridorVent, out var corridorVent) ||
            !corridorVent) return false;
        if (!Vents.TryGetValue(EditableObjects.ElectricalVent, out var electricalVent) ||
            !electricalVent) return false;
        if (!Vents.TryGetValue(EditableObjects.SecurityVent, out var securityVent) ||
            !securityVent) return false;
        if (!Vents.TryGetValue(EditableObjects.MedicalBayVent, out var medicalBayVent) ||
            !medicalBayVent) return false;
        if (!Vents.TryGetValue(EditableObjects.ReactorUpperVent, out var reactorUpperVent) ||
            !reactorUpperVent) return false;
        if (!Vents.TryGetValue(EditableObjects.ReactorLowerVent, out var reactorLowerVent) ||
            !reactorLowerVent) return false;
        if (!Vents.TryGetValue(EditableObjects.UpperEngineVent, out var upperEngineVent) ||
            !upperEngineVent) return false;
        if (!Vents.TryGetValue(EditableObjects.LowerEngineVent, out var lowerEngineVent) ||
            !lowerEngineVent) return false;
            
        
        return true;
    }

    private static bool IsObjectsFetched()
    {
        if (!Consoles.TryGetValue(EditableObjects.Admin, out var admin) || !admin) return false;
        if (!Objects.TryGetValue(EditableObjects.AdminAnimation, out var adminAnimation) || !adminAnimation) return false;
        if (!Objects.TryGetValue(EditableObjects.Vitals, out var vitals) || !vitals) return false;

        return true;
    }

    private static bool IsRoomsFetched()
    {
        if (!Objects.TryGetValue(EditableObjects.CommunicationsRoom, out var communicationRoom) ||
            !communicationRoom) return false;
        
        return true;
    }

    private static void ApplyChanges()
    {
        AdjustSkeld();
        IsAdjustmentsDone = true;
    }

    private static void AdjustSkeld()
    {
        if (isObjectsFetched && isRoomsFetched)
            DisableAdminAndCreateVitals();
        

        if (isVentsFetched)
            AdjustVents();
    }

    private static void AdjustVents()
    {
        BetterOtherRolesPlugin.Logger.LogDebug($"Adjusting Skeld Vents...");
        
        // Adjusting right side vents.
        
        DefaultVentCenters[EditableObjects.WeaponsVent] = Vents[EditableObjects.WeaponsVent].Center;
        Vents[EditableObjects.WeaponsVent].Center = Vents[EditableObjects.CafeteriaVent];
        
        DefaultVentCenters[EditableObjects.CafeteriaVent] = Vents[EditableObjects.CafeteriaVent].Center;
        Vents[EditableObjects.CafeteriaVent].Center = Vents[EditableObjects.WeaponsVent];
        
        DefaultVentRights[EditableObjects.NavigationUpperVent] = Vents[EditableObjects.NavigationUpperVent].Right;
        Vents[EditableObjects.NavigationUpperVent].Right = Vents[EditableObjects.NavigationLowerVent];
        
        DefaultVentRights[EditableObjects.NavigationLowerVent] = Vents[EditableObjects.NavigationLowerVent].Right;
        Vents[EditableObjects.NavigationLowerVent].Right = Vents[EditableObjects.NavigationUpperVent];
        
        DefaultVentCenters[EditableObjects.NavigationUpperVent] = Vents[EditableObjects.NavigationUpperVent].Center;
        Vents[EditableObjects.NavigationUpperVent].Center = Vents[EditableObjects.CorridorVent];
        
        DefaultVentCenters[EditableObjects.NavigationLowerVent] = Vents[EditableObjects.NavigationLowerVent].Center;
        Vents[EditableObjects.NavigationLowerVent].Center = Vents[EditableObjects.CorridorVent];
        
        DefaultVentCenters[EditableObjects.CorridorVent] = Vents[EditableObjects.CorridorVent].Center;
        Vents[EditableObjects.CorridorVent].Center = Vents[EditableObjects.NavigationUpperVent];
        
        DefaultVentLefts[EditableObjects.WeaponsVent] = Vents[EditableObjects.WeaponsVent].Left;
        Vents[EditableObjects.WeaponsVent].Left = Vents[EditableObjects.CorridorVent];
        
        DefaultVentCenters[EditableObjects.AdminVent] = Vents[EditableObjects.AdminVent].Center;
        Vents[EditableObjects.AdminVent].Center = Vents[EditableObjects.ShieldVent];
        
        DefaultVentCenters[EditableObjects.ShieldVent] = Vents[EditableObjects.ShieldVent].Center;
        Vents[EditableObjects.ShieldVent].Center = Vents[EditableObjects.AdminVent];
        
        // Adjusting left side vents.

        DefaultVentCenters[EditableObjects.UpperEngineVent] = Vents[EditableObjects.UpperEngineVent].Center;
        Vents[EditableObjects.UpperEngineVent].Center = Vents[EditableObjects.MedicalBayVent];
        
        DefaultVentCenters[EditableObjects.MedicalBayVent] = Vents[EditableObjects.MedicalBayVent].Center;
        Vents[EditableObjects.MedicalBayVent].Center = Vents[EditableObjects.UpperEngineVent];
        
        DefaultVentRights[EditableObjects.ReactorUpperVent] = Vents[EditableObjects.ReactorUpperVent].Right;
        Vents[EditableObjects.ReactorUpperVent].Right = Vents[EditableObjects.SecurityVent];
        
        DefaultVentRights[EditableObjects.SecurityVent] = Vents[EditableObjects.SecurityVent].Right;
        Vents[EditableObjects.SecurityVent].Right = Vents[EditableObjects.ReactorUpperVent];
        
        DefaultVentLefts[EditableObjects.ReactorUpperVent] = Vents[EditableObjects.ReactorUpperVent].Left;
        Vents[EditableObjects.ReactorUpperVent].Left = Vents[EditableObjects.ReactorLowerVent];
        
        DefaultVentLefts[EditableObjects.ReactorLowerVent] = Vents[EditableObjects.ReactorLowerVent].Left;
        Vents[EditableObjects.ReactorLowerVent].Left = Vents[EditableObjects.ReactorUpperVent];
        
        DefaultVentCenters[EditableObjects.ReactorLowerVent] = Vents[EditableObjects.ReactorLowerVent].Center;
        Vents[EditableObjects.ReactorLowerVent].Center = Vents[EditableObjects.SecurityVent];
        
        DefaultVentCenters[EditableObjects.ElectricalVent] = Vents[EditableObjects.ElectricalVent].Center;
        Vents[EditableObjects.ElectricalVent].Center = Vents[EditableObjects.LowerEngineVent];
        
        DefaultVentCenters[EditableObjects.LowerEngineVent] = Vents[EditableObjects.LowerEngineVent].Center;
        Vents[EditableObjects.LowerEngineVent].Center = Vents[EditableObjects.ElectricalVent];
        
    }

    private static void RevertChanges()
    {
        RevertSkeldAdjustment();
        IsAdjustmentsDone = false;
    }

    private static void RevertSkeldAdjustment()
    {
        if (isObjectsFetched && isRoomsFetched)
            DisableAdminAndCreateVitalsRevert();
        

        if (isVentsFetched)
            AdjustVentsRevert();
    }

    private static void AdjustVentsRevert()
    {
        // Reverting right side vents adjustments.
        
        Vents[EditableObjects.WeaponsVent].Center = DefaultVentCenters[EditableObjects.WeaponsVent];
        Vents[EditableObjects.CafeteriaVent].Center = DefaultVentCenters[EditableObjects.CafeteriaVent];
        
        Vents[EditableObjects.NavigationUpperVent].Right = DefaultVentRights[EditableObjects.NavigationUpperVent];
        Vents[EditableObjects.NavigationLowerVent].Right = DefaultVentRights[EditableObjects.NavigationLowerVent];
        Vents[EditableObjects.NavigationUpperVent].Center = DefaultVentCenters[EditableObjects.NavigationUpperVent];
        Vents[EditableObjects.NavigationLowerVent].Center = DefaultVentCenters[EditableObjects.NavigationLowerVent];
        
        Vents[EditableObjects.CorridorVent].Center = DefaultVentCenters[EditableObjects.CorridorVent];
        
        Vents[EditableObjects.WeaponsVent].Left = DefaultVentLefts[EditableObjects.WeaponsVent];
        
        Vents[EditableObjects.AdminVent].Center = DefaultVentCenters[EditableObjects.AdminVent];
        Vents[EditableObjects.ShieldVent].Center = DefaultVentCenters[EditableObjects.ShieldVent];
        
        // Reverting left side vents adjustments.
        
        Vents[EditableObjects.UpperEngineVent].Center = DefaultVentCenters[EditableObjects.UpperEngineVent];
        Vents[EditableObjects.MedicalBayVent].Center = DefaultVentCenters[EditableObjects.MedicalBayVent];
        
        Vents[EditableObjects.SecurityVent].Right = DefaultVentRights[EditableObjects.SecurityVent];
        
        Vents[EditableObjects.ReactorUpperVent].Left = DefaultVentLefts[EditableObjects.ReactorUpperVent];
        Vents[EditableObjects.ReactorUpperVent].Right = DefaultVentRights[EditableObjects.ReactorUpperVent];

        Vents[EditableObjects.ReactorLowerVent].Left = DefaultVentLefts[EditableObjects.ReactorLowerVent];
        Vents[EditableObjects.ReactorLowerVent].Center = DefaultVentCenters[EditableObjects.ReactorLowerVent];
        
        Vents[EditableObjects.ElectricalVent].Center = DefaultVentCenters[EditableObjects.ElectricalVent];
        Vents[EditableObjects.LowerEngineVent].Center = DefaultVentCenters[EditableObjects.LowerEngineVent];
    }

    private static void DisableAdminAndCreateVitalsRevert()
    {
        GameObject.Destroy(Objects[EditableObjects.Vitals]);
        Consoles[EditableObjects.Admin].GetComponent<CircleCollider2D>().enabled = true;
        Objects[EditableObjects.AdminAnimation].active = true;
    }

    private static void DisableAdminAndCreateVitals()
    {
        
        BetterOtherRolesPlugin.Logger.LogDebug($"Disabling admin and create vitals...");
        
        var vitalsTransform = Objects[EditableObjects.Vitals].transform;
        vitalsTransform.position = UpdatedPositions[EditableObjects.Vitals];
        vitalsTransform.rotation = UpdatedRotations[EditableObjects.Vitals];
        vitalsTransform.localScale = UpdatedScales[EditableObjects.Vitals];
        vitalsTransform.parent = Objects[EditableObjects.CommunicationsRoom].transform;
            

        var admin = Consoles[EditableObjects.Admin];
            admin.GetComponent<CircleCollider2D>().enabled = false;
            
        var animation = Objects[EditableObjects.AdminAnimation];
            animation.active = false;
    }


    private enum EditableObjects
    {
        Vitals,
        Admin,
        AdminAnimation,
        CommunicationsRoom,
        AdminVent,
        CafeteriaVent,
        NavigationUpperVent,
        NavigationLowerVent,
        WeaponsVent,
        ShieldVent,
        CorridorVent,
        ElectricalVent,
        SecurityVent,
        MedicalBayVent,
        ReactorUpperVent,
        ReactorLowerVent,
        UpperEngineVent,
        LowerEngineVent
    }
    
}