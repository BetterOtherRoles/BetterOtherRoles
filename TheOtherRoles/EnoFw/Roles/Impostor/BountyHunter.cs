using System.Linq;
using TheOtherRoles.EnoFw.Kernel;
using TheOtherRoles.Objects;
using TMPro;
using Option = TheOtherRoles.EnoFw.Kernel.CustomOption;

namespace TheOtherRoles.EnoFw.Roles.Impostor;

public class BountyHunter : AbstractRole
{
    public static readonly BountyHunter Instance = new();
    
    // Fields
    public Arrow Arrow;
    public PlayerControl Bounty;
    public float ArrowUpdateTimer;
    public float BountyUpdateTimer;
    public TextMeshPro CooldownText;
    
    // Options
    public readonly Option BountyDuration;
    public readonly Option BountyKillCooldown;
    public readonly Option PunishmentTime;
    public readonly Option ShowArrow;
    public readonly Option ArrowUpdateInterval;

    private BountyHunter() : base(nameof(BountyHunter), "Bounty hunter")
    {
        Team = Teams.Impostor;
        Color = Palette.ImpostorRed;
        CanTarget = true;
        
        SpawnRate = GetDefaultSpawnRateOption();
        
        BountyDuration = Tab.CreateFloatList(
            $"{Key}{nameof(BountyDuration)}",
            Cs("Bounty duration"),
            10f,
            120f,
            30f,
            5f,
            SpawnRate,
            string.Empty,
            "s");
        BountyKillCooldown = Tab.CreateFloatList(
            $"{Key}{nameof(BountyKillCooldown)}",
            Cs("Cooldown after killing bounty"),
            2.5f,
            60f,
            10f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        PunishmentTime = Tab.CreateFloatList(
            $"{Key}{nameof(PunishmentTime)}",
            Cs("Additional cooldown after killing others"),
            2.5f,
            60f,
            10f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        ShowArrow = Tab.CreateBool(
            $"{Key}{nameof(ShowArrow)}",
            Cs("Show arrow pointing towards the bounty"),
            false,
            SpawnRate);
        ArrowUpdateInterval = Tab.CreateFloatList(
            $"{Key}{nameof(ArrowUpdateInterval)}",
            Cs("Arrow update interval"),
            2.5f,
            60f,
            10f,
            2.5f,
            ShowArrow,
            string.Empty,
            "s");
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        if (Arrow != null && Arrow.arrow != null) UnityEngine.Object.Destroy(Arrow.arrow);
        Arrow = null;
        if (CooldownText != null && CooldownText.gameObject != null) UnityEngine.Object.Destroy(CooldownText.gameObject);
        CooldownText = null;
        Bounty = null;
        ArrowUpdateTimer = 0f;
        BountyUpdateTimer = 0f;
        foreach (var p in TORMapOptions.playerIcons.Values.Where(p => p != null && p.gameObject != null))
        {
            p.gameObject.SetActive(false);
        }
    }
}