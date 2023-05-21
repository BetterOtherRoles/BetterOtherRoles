using BetterOtherRoles.EnoFw;
using HarmonyLib;


namespace BetterOtherRoles.Patches
{
    [HarmonyPatch(typeof(ReactorSystemType), nameof(ReactorSystemType.RepairDamage))]
    public static class ReactorSystemType_RepairDamagePatch
    {
        public static bool Prefix(ReactorSystemType __instance, PlayerControl player, byte opCode)
        {
            if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 2 && opCode == 128 && !__instance.IsActive && CustomOptions.EnableBetterPolus)
            {
                __instance.Countdown = CustomOptions.BetterPolusReactorDuration;
                __instance.UserConsolePairs.Clear();
                __instance.IsDirty = true;

                return false;
            }

            return true;
        }
    }
}