using BetterOtherRoles.EnoFw.Kernel;
using UnityEngine;
using Option = BetterOtherRoles.EnoFw.Kernel.CustomOption;

namespace BetterOtherRoles.EnoFw.Roles.Impostor;

public class Warlock : AbstractRole
{
    public static readonly Warlock Instance = new();
    
    // Fields
    public PlayerControl CurseVictim;
    public PlayerControl CurseVictimTarget;
    
    // Options
    public readonly CustomOption CurseCooldown;
    public readonly CustomOption RootDuration;

    public static Sprite CurseButtonSprite => GetSprite("BetterOtherRoles.Resources.CurseButton.png", 115f);
    public static Sprite CurseKillButtonSprite => GetSprite("BetterOtherRoles.Resources.CurseKillButton.png", 115f);

    private Warlock() : base(nameof(Warlock), "Warlock")
    {
        Team = Teams.Impostor;
        Color = Palette.ImpostorRed;
        CanTarget = true;
        
        SpawnRate = GetDefaultSpawnRateOption();
        
        CurseCooldown = Tab.CreateFloatList(
            $"{Key}{nameof(CurseCooldown)}",
            Cs("Curse cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        RootDuration = Tab.CreateFloatList(
            $"{Key}{nameof(RootDuration)}",
            Cs("Root duration"),
            0f,
            15f,
            5f,
            1f,
            SpawnRate,
            string.Empty,
            "s");
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        CurseVictim = null;
        CurseVictimTarget = null;
    }

    public void ResetCurse()
    {
        HudManagerStartPatch.warlockCurseButton.Timer = HudManagerStartPatch.warlockCurseButton.MaxTimer;
        HudManagerStartPatch.warlockCurseButton.Sprite = CurseButtonSprite;
        HudManagerStartPatch.warlockCurseButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
        CurrentTarget = null;
        CurseVictim = null;
        CurseVictimTarget = null;
    }
}