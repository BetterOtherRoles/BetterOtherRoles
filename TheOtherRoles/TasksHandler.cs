using HarmonyLib;
using System;
using TheOtherRoles.EnoFw.Roles.Crewmate;
using TheOtherRoles.EnoFw.Roles.Modifiers;
using TheOtherRoles.EnoFw.Roles.Neutral;
using TheOtherRoles.Utilities;

namespace TheOtherRoles {
    [HarmonyPatch]
    public static class TasksHandler {

        public static Tuple<int, int> taskInfo(GameData.PlayerInfo playerInfo) {
            int TotalTasks = 0;
            int CompletedTasks = 0;
            if (!playerInfo.Disconnected && playerInfo.Tasks != null &&
                playerInfo.Object &&
                playerInfo.Role && playerInfo.Role.TasksCountTowardProgress &&
                !playerInfo.Object.hasFakeTasks() && !playerInfo.Role.IsImpostor
                ) {
                foreach (var playerInfoTask in playerInfo.Tasks.GetFastEnumerator())
                {
                    if (playerInfoTask.Complete) CompletedTasks++;
                    TotalTasks++;
                }
            }
            return Tuple.Create(CompletedTasks, TotalTasks);
        }

        [HarmonyPatch(typeof(GameData), nameof(GameData.RecomputeTaskCounts))]
        private static class GameDataRecomputeTaskCountsPatch {
            private static bool Prefix(GameData __instance) {
               

                var totalTasks = 0;
                var completedTasks = 0;
                
                foreach (var playerInfo in GameData.Instance.AllPlayers.GetFastEnumerator())
                {
                    if (playerInfo.Object
                        && playerInfo.Object.HasAliveKillingLover() // Tasks do not count if a Crewmate has an alive killing Lover
                        || Lawyer.Instance.Player != null && playerInfo.PlayerId == Lawyer.Instance.Player.PlayerId // Tasks of the Lawyer do not count
                        || (Pursuer.Instance.HasPlayer && playerInfo.PlayerId == Pursuer.Instance.Player.PlayerId && Pursuer.Instance.Player.Data.IsDead) // Tasks of the Pursuer only count, if he's alive
                        || (Thief.Instance.Player != null && playerInfo.PlayerId == Thief.Instance.Player.PlayerId) // Thief's tasks only count after joining crew team as sheriff (and then the thief is not the thief anymore)
                       )
                        continue;
                    var (playerCompleted, playerTotal) = taskInfo(playerInfo);
                    totalTasks += playerTotal;
                    completedTasks += playerCompleted;
                }
                
                __instance.TotalTasks = totalTasks;
                __instance.CompletedTasks = completedTasks;
                return false;
            }
        }
        
    }
}
