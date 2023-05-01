using TheOtherRoles.EnoFramework.Kernel;
using UnityEngine;

namespace TheOtherRoles.Customs.Roles.Crewmate;

[EnoSingleton]
public class NiceGuesser : CustomRole
{
    public readonly EnoFramework.CustomOption NumberOfShots;
    public readonly EnoFramework.CustomOption MultipleShotsPerMeeting;
    public readonly EnoFramework.CustomOption GuessBypassShields;

    public int UsedGuess;

    public NiceGuesser() : base(nameof(NiceGuesser))
    {
        Team = Teams.Crewmate;
        Color = new Color32(255, 255, 0, byte.MaxValue);
        DisplayName = "Nice guesser";

        IntroDescription = "Guess and shoot";
        ShortDescription = "Guess and shoot";

        NumberOfShots = OptionsTab.CreateFloatList(
            $"{Name}{nameof(NumberOfShots)}",
            Cs("Number of shots"),
            1f,
            15f,
            1f,
            1f,
            SpawnRate);
        MultipleShotsPerMeeting = OptionsTab.CreateBool(
            $"{Name}{nameof(MultipleShotsPerMeeting)}",
            Cs("Can guess multiple time per meeting"),
            false,
            SpawnRate);
        GuessBypassShields = OptionsTab.CreateBool(
            $"{Name}{nameof(GuessBypassShields)}",
            Cs("Guess ignore medic shield"),
            false,
            SpawnRate);
    }

    public void Clear(byte playerId)
    {
        if (Is(playerId))
        {
            Player = null;
        }
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        UsedGuess = 0;
    }
}