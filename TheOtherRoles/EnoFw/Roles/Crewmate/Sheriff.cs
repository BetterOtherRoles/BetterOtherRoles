using TheOtherRoles.EnoFw.Kernel;
using UnityEngine;
using Option = TheOtherRoles.EnoFw.Kernel.CustomOption;

namespace TheOtherRoles.EnoFw.Roles.Crewmate;

public class Sheriff : AbstractRole
{
    public static readonly Sheriff Instance = new();
    
    // Fields
    public PlayerControl FormerDeputy; // Needed for keeping handcuffs + shifting
    public PlayerControl FormerSheriff; // When deputy gets promoted...
    public string IntroTextForDeputy => Cs($"Your Sheriff is {(Player != null ? Player.Data.PlayerName : "unknown")}");
    
    // Options
    public readonly Option Cooldown;
    public readonly Option CanKillNeutrals;
    public readonly Option SpyCanDieToSheriff;
    public readonly Option DeputySpawnRate;

    private Sheriff() : base(nameof(Sheriff), "Sheriff")
    {
        Team = Teams.Crewmate;
        Color = new Color32(248, 205, 70, byte.MaxValue);
        CanTarget = true;
        
        SpawnRate = GetDefaultSpawnRateOption();
        
        Cooldown = Tab.CreateFloatList(
            $"{Key}{nameof(Cooldown)}",
            Cs("Kill cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        CanKillNeutrals = Tab.CreateBool(
            $"{Key}{nameof(CanKillNeutrals)}",
            Cs("Can kill neutrals"),
            true,
            SpawnRate);
        SpyCanDieToSheriff = Tab.CreateBool(
            $"{Key}{nameof(SpyCanDieToSheriff)}",
            Cs("Can kill Spy"),
            true,
            SpawnRate);
        DeputySpawnRate = Tab.CreateFloatList(
            $"{Key}{nameof(DeputySpawnRate)}",
            Cs("Has a Deputy"),
            0f,
            100f,
            50f,
            10f,
            SpawnRate,
            string.Empty,
            "%");
    }
    
    public void ReplaceCurrentSheriff(PlayerControl deputy)
    {
        if (FormerSheriff == null) FormerSheriff = Player;
        Player = deputy;
        CurrentTarget = null;
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        FormerSheriff = null;
        FormerDeputy = null;
    }
}