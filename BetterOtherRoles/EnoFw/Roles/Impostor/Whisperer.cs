using BetterOtherRoles.EnoFw.Kernel;
using UnityEngine;
using Option = BetterOtherRoles.EnoFw.Kernel.CustomOption;

namespace BetterOtherRoles.EnoFw.Roles.Impostor;

public class Whisperer : AbstractRole
{
    public static readonly Whisperer Instance = new();
    
    // Fields
    public PlayerControl WhisperVictim;
    public PlayerControl WhisperVictimTarget;
    public PlayerControl WhisperVictimToKill;
    
    // Options
    public readonly CustomOption WhisperCooldown;
    public readonly CustomOption WhisperDelay;

    public static Sprite WhisperButtonSprite => GetSprite("BetterOtherRoles.Resources.WhisperButton.png", 115f);

    private Whisperer() : base(nameof(Whisperer), "Whisperer")
    {
        Team = Teams.Impostor;
        Color = Palette.ImpostorRed;
        CanTarget = true;
        
        SpawnRate = GetDefaultSpawnRateOption();
        
        WhisperCooldown = Tab.CreateFloatList(
            $"{Key}{nameof(WhisperCooldown)}",
            Cs("Whisper cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        WhisperDelay = Tab.CreateFloatList(
            $"{Key}{nameof(WhisperDelay)}",
            Cs("Whisper kill delay"),
            1f,
            15f,
            5f,
            1f,
            SpawnRate,
            string.Empty,
            "s");
    }

    public void ResetWhisper()
    {
        HudManagerStartPatch.whispererKillButton.Timer = HudManagerStartPatch.whispererKillButton.MaxTimer;
        HudManagerStartPatch.whispererKillButton.Sprite = WhisperButtonSprite;
        HudManagerStartPatch.whispererKillButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
        CurrentTarget = null;
        WhisperVictim = null;
        WhisperVictimTarget = null;
        WhisperVictimToKill = null;
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        WhisperVictim = null;
        WhisperVictimTarget = null;
        WhisperVictimToKill = null;
    }
}