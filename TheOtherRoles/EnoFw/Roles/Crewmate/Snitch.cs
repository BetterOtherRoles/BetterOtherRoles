using System.Collections.Generic;
using System.Linq;
using TheOtherRoles.EnoFw.Kernel;
using TheOtherRoles.Objects;
using TMPro;
using UnityEngine;
using Option = TheOtherRoles.EnoFw.Kernel.CustomOption;

namespace TheOtherRoles.EnoFw.Roles.Crewmate;

public class Snitch : AbstractRole
{
    public static readonly Snitch Instance = new();
    
    // Fields
    public readonly List<Arrow> Arrows = new();
    public readonly Dictionary<byte, byte> PlayerRoomMap = new();
    public bool IsRevealed;
    public bool NeedsUpdate;
    public TextMeshPro Text;
    
    public bool InfoModeNone => (string)InfoMode is "none";
    public bool InfoModeChat => (string)InfoMode is "chat" or "chat & map";
    public bool InfoModeMap => (string)InfoMode is "map" or "chat & map";
    public bool InfoTargetEvilPlayers => (string)InfoTargets is "all evil players";
    public bool InfoTargetKillingPlayers => (string)InfoTargets is "killing players";
    public bool ArrowTargetsNone => (string)ArrowTargets is "none";
    public bool ArrowTargetsSnitch => (string)ArrowTargets is "only for snitch" or "both";
    public bool ArrowTargetsEvil => (string)ArrowTargets is "only for evil" or "both";
    
    // Options
    public readonly Option LeftTasksForReveal;
    public readonly Option InfoMode;
    public readonly Option InfoTargets;
    public readonly Option ArrowTargets;

    private Snitch() : base(nameof(Snitch), "Snitch")
    {
        Team = Teams.Crewmate;
        Color = new Color32(184, 251, 79, byte.MaxValue);
        
        SpawnRate = GetDefaultSpawnRateOption();
        
        LeftTasksForReveal = Tab.CreateFloatList(
            $"{Key}{nameof(LeftTasksForReveal)}",
            Cs("Task count where the snitch will be revealed"),
            0f,
            25f,
            5f,
            1f,
            SpawnRate);
        InfoMode = Tab.CreateStringList(
            $"{Key}{nameof(InfoMode)}",
            Cs("Information mode"),
            new List<string> { "none", "chat", "map", "chat & map" },
            "none",
            SpawnRate);
        InfoTargets = Tab.CreateStringList(
            $"{Key}{nameof(InfoTargets)}",
            Cs("Targets"),
            new List<string> { "all evil players", "killing players" },
            "all evil players",
            InfoMode);
        ArrowTargets = Tab.CreateStringList(
            $"{Key}{nameof(ArrowTargets)}",
            Cs("Arrow targets"),
            new List<string> { "none", "only for snitch", "only for evil", "both" },
            "none",
            SpawnRate);
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        foreach (var arrow in Arrows.Where(arrow => arrow.arrow != null))
        {
            UnityEngine.Object.Destroy(arrow.arrow);
        }
        Arrows.Clear();
        IsRevealed = false;
        PlayerRoomMap.Clear();
        NeedsUpdate = false;
    }
}