using TheOtherRoles.CustomGameModes;
using TheOtherRoles.EnoFw;
using TheOtherRoles.EnoFw.Roles.Neutral;
using UnityEngine;

namespace TheOtherRoles.Utilities {
    public static class HandleGuesser {
        private static Sprite targetSprite;
        public static bool isGuesserGm = false;
        public static bool hasMultipleShotsPerMeeting = false;
        public static bool killsThroughShield = true;
        public static bool evilGuesserCanGuessSpy = true;
        public static bool guesserCantGuessSnitch = false;

        public static Sprite getTargetSprite() {
            if (targetSprite) return targetSprite;
            targetSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.TargetIcon.png", 150f);
            return targetSprite;
        }

        public static bool isGuesser(byte playerId) {
            if (isGuesserGm) return GuesserGM.isGuesser(playerId);
            return Guesser.Instance.IsGuesser(playerId);
        }

        public static void clear(byte playerId) {
            if (isGuesserGm) GuesserGM.clear(playerId);
            else Guesser.Instance.Clear(playerId);
        }

        public static int remainingShots(byte playerId, bool shoot = false) {
            if (isGuesserGm) return GuesserGM.remainingShots(playerId, shoot);
            return Guesser.Instance.RemainingShots(playerId, shoot);
        }

        public static void clearAndReload() {
            Guesser.Instance.ClearAndReload();
            GuesserGM.clearAndReload();
            isGuesserGm = TORMapOptions.gameMode == CustomGamemodes.Guesser;
            if (isGuesserGm)
            {
                guesserCantGuessSnitch = CustomOptions.GuesserGameModeCantGuessSnitchIfTasksDone;
                hasMultipleShotsPerMeeting = CustomOptions.GuesserGameModeHasMultipleShotsPerMeeting;
                killsThroughShield = CustomOptions.GuesserGameModeKillsThroughShield;
                evilGuesserCanGuessSpy = CustomOptions.GuesserGameModeEvilCanKillSpy;
            } else
            {
                guesserCantGuessSnitch = Guesser.Instance.CantGuessSnitchIfTasksDone;
                hasMultipleShotsPerMeeting = Guesser.Instance.MultipleShotsPerMeeting;
                killsThroughShield = Guesser.Instance.KillTroughShield;
                evilGuesserCanGuessSpy = Guesser.Instance.CanKillSpy;
            }

        }
    }
}
