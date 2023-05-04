using UnityEngine;

namespace TheOtherRoles.EnoFw.Roles.Impostor;

public static class Whisperer
{
    public static PlayerControl whisperer;
    public static Color color = Palette.ImpostorRed;


    public static float cooldown = 30f;
    public static float delay = 5f;
    private static Sprite buttonSprite;

    public static PlayerControl currentTarget; // Current target from Whisper ?
    public static PlayerControl whisperVictim; // Cursed player.
    public static PlayerControl whisperVictimTarget; // for ghost.
    public static PlayerControl whisperVictimToKill;

    public static Sprite getButtonSprite()
    {
        if (buttonSprite) return buttonSprite;
        buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.WhisperButton.png", 115f);
        return buttonSprite;
    }

    public static void resetWhisper()
    {
        HudManagerStartPatch.whispererKillButton.Timer = HudManagerStartPatch.whispererKillButton.MaxTimer;
        HudManagerStartPatch.whispererKillButton.Sprite = getButtonSprite();
        HudManagerStartPatch.whispererKillButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
        currentTarget = null;
        whisperVictim = null;
        whisperVictimTarget = null;
        whisperVictimToKill = null;
    }

    public static void clearAndReload()
    {
        whisperer = null;
        currentTarget = null;
        whisperVictim = null;
        whisperVictimTarget = null;
        whisperVictimToKill = null;
        cooldown = CustomOptionHolder.whispererCooldown.getFloat();
        delay = CustomOptionHolder.whispererDelay.getFloat();
    }
}