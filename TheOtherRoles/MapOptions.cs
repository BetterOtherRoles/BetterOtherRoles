using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheOtherRoles{
    static class TORMapOptions {
        // Set values
        public static int maxNumberOfMeetings = 10;
        public static bool blockSkippingInEmergencyMeetings = false;
        public static bool noVoteIsSelfVote = false;
        public static bool hidePlayerNames = false;
        public static bool ghostsSeeRoles = true;
        public static bool ghostsSeeModifier = true;
        public static bool ghostsSeeInformation = true;
        public static bool ghostsSeeVotes = true;
        public static bool showRoleSummary = true;
        public static bool allowParallelMedBayScans = false;
        public static bool showLighterDarker = true;
        public static bool enableSoundEffects = true;
        public static bool enableHorseMode = false;
        public static bool shieldFirstKill = false;
        public static bool removeShieldOnFirstMeeting = false;
        public static float shieldDuration;
        public static CustomGamemodes gameMode = CustomGamemodes.Classic;

        // Updating values
        public static int meetingsCount = 0;
        public static List<SurvCamera> camerasToAdd = new List<SurvCamera>();
        public static List<Vent> ventsToSeal = new List<Vent>();
        public static Dictionary<byte, PoolablePlayer> playerIcons = new Dictionary<byte, PoolablePlayer>();
        public static string firstKillName;
        public static PlayerControl firstKillPlayer;
        
        public static DateTime? ShieldExpiresAt;

        public static void UpdateShield()
        {
            if (firstKillPlayer != null && !removeShieldOnFirstMeeting && ShieldExpiresAt == null)
            {
                ShieldExpiresAt = DateTime.UtcNow.AddSeconds(shieldDuration);
            }

            if (firstKillPlayer != null && !removeShieldOnFirstMeeting && ShieldExpiresAt != null && DateTime.UtcNow >= ShieldExpiresAt)
            {
                firstKillPlayer = null;
                ShieldExpiresAt = null;
            }
        }

        public static void clearAndReloadMapOptions() {
            meetingsCount = 0;
            camerasToAdd = new List<SurvCamera>();
            ventsToSeal = new List<Vent>();
            playerIcons = new Dictionary<byte, PoolablePlayer>(); ;

            maxNumberOfMeetings = Mathf.RoundToInt(CustomOptionHolder.maxNumberOfMeetings.getSelection());
            blockSkippingInEmergencyMeetings = CustomOptionHolder.blockSkippingInEmergencyMeetings.getBool();
            noVoteIsSelfVote = CustomOptionHolder.noVoteIsSelfVote.getBool();
            hidePlayerNames = CustomOptionHolder.hidePlayerNames.getBool();
            allowParallelMedBayScans = CustomOptionHolder.allowParallelMedBayScans.getBool();
            shieldFirstKill = CustomOptionHolder.shieldFirstKill.getBool();
            removeShieldOnFirstMeeting = CustomOptionHolder.removeFirstKillShield.getSelection() == 0;
            shieldDuration = CustomOptionHolder.removeShieldTimer.getFloat();
            firstKillPlayer = null;
            ShieldExpiresAt = null;
        }

        public static void reloadPluginOptions() {
            ghostsSeeRoles = TheOtherRolesPlugin.GhostsSeeRoles.Value;
            ghostsSeeModifier = TheOtherRolesPlugin.GhostsSeeModifier.Value;
            ghostsSeeInformation = TheOtherRolesPlugin.GhostsSeeInformation.Value;
            ghostsSeeVotes = TheOtherRolesPlugin.GhostsSeeVotes.Value;
            showRoleSummary = TheOtherRolesPlugin.ShowRoleSummary.Value;
            showLighterDarker = TheOtherRolesPlugin.ShowLighterDarker.Value;
            enableSoundEffects = TheOtherRolesPlugin.EnableSoundEffects.Value;
            enableHorseMode = TheOtherRolesPlugin.EnableHorseMode.Value;
            //Patches.ShouldAlwaysHorseAround.isHorseMode = TheOtherRolesPlugin.EnableHorseMode.Value;
        }
    }
}
