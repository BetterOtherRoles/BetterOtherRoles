using TheOtherRoles.EnoFramework.Kernel;
using UnityEngine;

namespace TheOtherRoles.Customs.Roles.Impostor;

[EnoSingleton]
public class EvilGuesser : CustomRole
{
    public readonly EnoFramework.CustomOption NumberOfShots;
    public readonly EnoFramework.CustomOption MultipleShotsPerMeeting;
    public readonly EnoFramework.CustomOption GuessBypassShields;
    public readonly EnoFramework.CustomOption CanGuessSpy;
    public readonly EnoFramework.CustomOption CanGuessSnitchIfTasksDone;

    public int UsedGuess;

    public EvilGuesser() : base(nameof(EvilGuesser))
    {
        Team = Teams.Crewmate;
        Color = Palette.ImpostorRed;
        DisplayName = "Evil guesser";

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
        CanGuessSpy = OptionsTab.CreateBool(
            $"{Name}{nameof(CanGuessSpy)}",
            Cs("Can guess spy"),
            false,
            SpawnRate);
        CanGuessSnitchIfTasksDone = OptionsTab.CreateBool(
            $"{Name}{nameof(CanGuessSnitchIfTasksDone)}",
            Cs("Can guess snitch when tasks completed"),
            true,
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