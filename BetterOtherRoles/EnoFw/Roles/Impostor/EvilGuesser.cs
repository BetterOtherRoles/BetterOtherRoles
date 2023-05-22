using BetterOtherRoles.EnoFw.Kernel;
using UnityEngine;
using Option = BetterOtherRoles.EnoFw.Kernel.CustomOption;

namespace BetterOtherRoles.EnoFw.Roles.Impostor;

public class EvilGuesser : AbstractRole
{
    public static readonly EvilGuesser Instance = new();
    
    // Fields
    public int RemainingGuess = 2;
    
    // Options
    public readonly CustomOption NumberOfShots;
    public readonly CustomOption MultipleShotsPerMeeting;
    public readonly CustomOption KillTroughShield;
    public readonly CustomOption CanKillSpy;
    public readonly CustomOption CantGuessSnitchIfTasksDone;

    private EvilGuesser() : base(nameof(EvilGuesser), "Evil Guesser")
    {
        Team = Teams.Impostor;
        Color = Palette.ImpostorRed;

        SpawnRate = GetDefaultSpawnRateOption();
        SpawnRate.OnlyForGameModes(CustomOption.GameMode.Classic);
        
        NumberOfShots = Tab.CreateFloatList(
            $"{Key}{nameof(NumberOfShots)}",
            Cs("Number of shots"),
            1f,
            15f,
            1f,
            1f,
            SpawnRate);
        MultipleShotsPerMeeting = Tab.CreateBool(
            $"{Key}{nameof(MultipleShotsPerMeeting)}",
            Cs("Can guess multiple time per meeting"),
            false,
            SpawnRate);
        KillTroughShield = Tab.CreateBool(
            $"{Key}{nameof(KillTroughShield)}",
            Cs("Guess ignore medic shield"),
            false,
            SpawnRate);
        CanKillSpy = Tab.CreateBool(
            $"{Name}{nameof(CanKillSpy)}",
            Cs("Evil guesser can guess Spy"),
            false,
            SpawnRate);
        CantGuessSnitchIfTasksDone = Tab.CreateBool(
            $"{Key}{nameof(CantGuessSnitchIfTasksDone)}",
            Cs("Can guess Snitch when tasks completed"),
            true,
            SpawnRate);
    }
    
    public int RemainingShots(byte playerId, bool shoot = false)
    {
        if (Player == null || Player.PlayerId != playerId) return RemainingGuess;
        if (shoot)
        {
            RemainingGuess = Mathf.Max(0, RemainingGuess - 1);
        }

        return RemainingGuess;
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        RemainingGuess = NumberOfShots;
    }
    
    public void Clear(byte playerId)
    {
        if (IsPlayer(playerId))
        {
            Player = null;
        }
    }
}