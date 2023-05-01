using TheOtherRoles.CustomGameModes;
using TheOtherRoles.Customs.Roles.Crewmate;
using TheOtherRoles.Customs.Roles.Impostor;
using TheOtherRoles.EnoFramework.Kernel;
using UnityEngine;

namespace TheOtherRoles.Utilities;

public static class HandleGuesser
{
    private static Sprite? targetSprite;
    public static bool isGuesserGm = false;

    public static Sprite getTargetSprite()
    {
        if (targetSprite != null) return targetSprite;
        targetSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.TargetIcon.png", 150f);
        return targetSprite;
    }

    public static bool isGuesser(byte playerId)
    {
        if (isGuesserGm) return GuesserGM.isGuesser(playerId);
        return Singleton<NiceGuesser>.Instance.Is(playerId) || Singleton<EvilGuesser>.Instance.Is(playerId);
    }

    public static void clear(byte playerId)
    {
        if (isGuesserGm)
        {
            GuesserGM.clear(playerId);
        }
        else
        {
            Singleton<NiceGuesser>.Instance.Clear(playerId);
            Singleton<EvilGuesser>.Instance.Clear(playerId);
        }
    }

    public static int remainingShots(byte playerId, bool shoot = false)
    {
        if (isGuesserGm) return GuesserGM.remainingShots(playerId, shoot);
        if (Singleton<NiceGuesser>.Instance.Is(playerId))
        {
            return Singleton<NiceGuesser>.Instance.NumberOfShots -
                   (Singleton<NiceGuesser>.Instance.UsedGuess + (shoot ? 1 : 0));
        }
        if (Singleton<EvilGuesser>.Instance.Is(playerId))
        {
            return Singleton<EvilGuesser>.Instance.NumberOfShots -
                   (Singleton<EvilGuesser>.Instance.UsedGuess + (shoot ? 1 : 0));
        }

        return 0;
    }

    public static void clearAndReload()
    {
        Singleton<NiceGuesser>.Instance.ClearAndReload();
        Singleton<EvilGuesser>.Instance.ClearAndReload();
        GuesserGM.clearAndReload();
        isGuesserGm = TORMapOptions.gameMode == CustomGamemodes.Guesser;
    }
}