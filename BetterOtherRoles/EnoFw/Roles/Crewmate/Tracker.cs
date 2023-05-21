using System;
using System.Collections.Generic;
using System.Linq;
using BetterOtherRoles.EnoFw.Kernel;
using BetterOtherRoles.Objects;
using BetterOtherRoles.EnoFw.Libs.Reactor.Networking.Attributes;
using UnityEngine;
using Option = BetterOtherRoles.EnoFw.Kernel.CustomOption;

namespace BetterOtherRoles.EnoFw.Roles.Crewmate;

public class Tracker : AbstractRole
{
    public static readonly Tracker Instance = new();
    
    // Fields
    public PlayerControl Tracked;
    public bool UsedTracker;
    public float TimeUntilUpdate;
    public float CorpsesTrackingTimer;
    public Arrow Arrow;
    public readonly List<Vector3> DeadBodyPositions = new();
    public readonly List<Arrow> Arrows = new();
    
    // Options
    public readonly CustomOption UpdateArrowInterval;
    public readonly CustomOption ResetTargetAfterMeeting;
    public readonly CustomOption CanTrackCorpses;
    public readonly CustomOption CorpsesTrackingCooldown;
    public readonly CustomOption CorpsesTrackingDuration;

    public static Sprite TrackCorpsesButtonSprite => GetSprite("BetterOtherRoles.Resources.PathfindButton.png", 115f);
    public static Sprite TrackButtonSprite => GetSprite("BetterOtherRoles.Resources.TrackerButton.png", 115f);

    private Tracker() : base(nameof(Tracker), "Tracker")
    {
        Team = Teams.Crewmate;
        Color = new Color32(100, 58, 220, byte.MaxValue);
        CanTarget = true;
        
        SpawnRate = GetDefaultSpawnRateOption();
        
        UpdateArrowInterval = Tab.CreateFloatList(
            $"{Key}{nameof(UpdateArrowInterval)}",
            Cs("Update interval"),
            1f,
            30f,
            5f,
            1f,
            SpawnRate,
            string.Empty,
            "s");
        ResetTargetAfterMeeting = Tab.CreateBool(
            $"{Key}{nameof(ResetTargetAfterMeeting)}",
            Cs("Reset target after meeting"),
            true,
            SpawnRate);
        CanTrackCorpses = Tab.CreateBool(
            $"{Key}{nameof(CanTrackCorpses)}",
            Cs("Can track dead bodies"),
            false,
            SpawnRate);
        CorpsesTrackingCooldown = Tab.CreateFloatList(
            $"{Key}{nameof(CorpsesTrackingCooldown)}",
            Cs("Corpses tracking cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            CanTrackCorpses,
            string.Empty,
            "s");
        CorpsesTrackingDuration = Tab.CreateFloatList(
            $"{Key}{nameof(CorpsesTrackingDuration)}",
            Cs("Corpses tracking duration"),
            2.5f,
            30f,
            5f,
            2.5f,
            CanTrackCorpses,
            string.Empty,
            "s");
    }

    

    public void ResetTracked()
    {
        CurrentTarget = Tracked = null;
        UsedTracker = false;
        if (Arrow?.arrow != null) UnityEngine.Object.Destroy(Arrow.arrow);
        Arrow = new Arrow(Color.blue);
        if (Arrow.arrow != null) Arrow.arrow.SetActive(false);
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        ResetTracked();
        TimeUntilUpdate = 0f;
        foreach (var arrow in Arrows.Where(arrow => arrow.arrow != null))
        {
            UnityEngine.Object.Destroy(arrow.arrow);
        }
        Arrows.Clear();
        DeadBodyPositions.Clear();
        CorpsesTrackingTimer = 0f;
    }

    public static void TrackerUsedTracker(byte playerId)
    {
        var data = new Tuple<byte>(playerId);
        Rpc_TrackerUsedTracker(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.TrackerUsedTracker)]
    private static void Rpc_TrackerUsedTracker(PlayerControl sender, string rawData)
    {
        var targetId = Rpc.Deserialize<Tuple<byte>>(rawData).Item1;
        var target = Helpers.playerById(targetId);
        if (target == null) return;
        Instance.UsedTracker = true;
        Instance.Tracked = target;
    }
}