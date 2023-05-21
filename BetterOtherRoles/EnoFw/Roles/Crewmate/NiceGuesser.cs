using BetterOtherRoles.EnoFw.Kernel;
using UnityEngine;
using Option = BetterOtherRoles.EnoFw.Kernel.CustomOption;

namespace BetterOtherRoles.EnoFw.Roles.Crewmate;

public class NiceGuesser : AbstractRole
{
    public static readonly NiceGuesser Instance = new();
    
    // Fields
    public int RemainingGuess = 2;
    
    // Options
    public readonly CustomOption NumberOfShots;
    public readonly CustomOption MultipleShotsPerMeeting;
    public readonly CustomOption KillTroughShield;

    private NiceGuesser() : base(nameof(NiceGuesser), "Nice Guesser")
    {
        Team = Teams.Crewmate;
        Color = new Color32(255, 255, 0, byte.MaxValue);

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