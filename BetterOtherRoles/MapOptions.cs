using System;
using System.Collections.Generic;
using BetterOtherRoles.EnoFw;
using BetterOtherRoles.Objects;
using UnityEngine;

namespace BetterOtherRoles;

static class TORMapOptions
{
    private static Sprite _shieldSprite;
    public static Color ShieldColor = new Color32(7, 55, 133, byte.MaxValue);
        
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

    public static CustomButton ShieldExpireButton;

    public static Sprite GetShieldSprite()
    {
        if (_shieldSprite == null)
        {
            _shieldSprite = Helpers.loadSpriteFromResources("BetterOtherRoles.Resources.Shield.png", 115f);
        }

        return _shieldSprite;
    }

    public static void UpdateShield()
    {
        if (firstKillPlayer != null && !removeShieldOnFirstMeeting && ShieldExpiresAt == null)
        {
            ShieldExpiresAt = DateTime.UtcNow.AddSeconds(shieldDuration);
        }

        if (firstKillPlayer != null && !removeShieldOnFirstMeeting && ShieldExpiresAt != null &&
            DateTime.UtcNow >= ShieldExpiresAt)
        {
            firstKillPlayer = null;
            ShieldExpiresAt = null;
        }

        if (firstKillPlayer != null && !removeShieldOnFirstMeeting && ShieldExpiresAt != null &&
            ShieldExpireButton != null)
        {
            var remainingTime = new DateTimeOffset(ShieldExpiresAt.Value).ToUnixTimeSeconds() -
                                                                       new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            if (remainingTime <= 5f)
            {
                ShieldExpireButton.Timer = remainingTime;
                ShieldExpireButton.actionButton.cooldownTimerText.color = Palette.ImpostorRed;
            }
            else
            {
                ShieldExpireButton.Timer = 0f;
            }
        }
    }

    public static void clearAndReloadMapOptions()
    {
        meetingsCount = 0;
        camerasToAdd = new List<SurvCamera>();
        ventsToSeal = new List<Vent>();
        playerIcons = new Dictionary<byte, PoolablePlayer>();
        ;

        maxNumberOfMeetings = Mathf.RoundToInt(CustomOptions.MaxEmergencyMeetings);
        blockSkippingInEmergencyMeetings = CustomOptions.BlockSkippingInEmergencyMeetings;
        noVoteIsSelfVote = CustomOptions.NoVoteIsSelfVote;
        hidePlayerNames = CustomOptions.HidePlayerNames;
        allowParallelMedBayScans = CustomOptions.AllowParallelMedBayScans;
        shieldFirstKill = CustomOptions.ShieldFirstKilledPlayer;
        removeShieldOnFirstMeeting = CustomOptions.RemoveFirstKillShield == "First meeting ended";
        shieldDuration = CustomOptions.RemoveShieldTimer;
        firstKillPlayer = null;
        ShieldExpiresAt = null;
    }

    public static void reloadPluginOptions()
    {
        ghostsSeeRoles = BetterOtherRolesPlugin.GhostsSeeRoles.Value;
        ghostsSeeModifier = BetterOtherRolesPlugin.GhostsSeeModifier.Value;
        ghostsSeeInformation = BetterOtherRolesPlugin.GhostsSeeInformation.Value;
        ghostsSeeVotes = BetterOtherRolesPlugin.GhostsSeeVotes.Value;
        showRoleSummary = BetterOtherRolesPlugin.ShowRoleSummary.Value;
        showLighterDarker = BetterOtherRolesPlugin.ShowLighterDarker.Value;
        enableSoundEffects = BetterOtherRolesPlugin.EnableSoundEffects.Value;
        enableHorseMode = BetterOtherRolesPlugin.EnableHorseMode.Value;
        //Patches.ShouldAlwaysHorseAround.isHorseMode = TheOtherRolesPlugin.EnableHorseMode.Value;
    }
}