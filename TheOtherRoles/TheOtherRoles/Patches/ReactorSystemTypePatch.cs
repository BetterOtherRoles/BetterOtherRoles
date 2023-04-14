using HarmonyLib;


namespace TheOtherRoles.Patches
{
    [HarmonyPatch(typeof(ReactorSystemType), nameof(ReactorSystemType.RepairDamage))]
    public static class ReactorSystemType_RepairDamagePatch
    {
        public static bool Prefix(ReactorSystemType __instance, PlayerControl player, byte opCode)
        {
            if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 2 && opCode == 128 && !__instance.IsActive && CustomOptionHolder.enableBetterPolus.getBool())
            {
                __instance.Countdown = CustomOptionHolder.betterPolusReactorDuration.getFloat();
                __instance.UserConsolePairs.Clear();
                __instance.IsDirty = true;

                return false;
            }

            return true;
        }
    }
}