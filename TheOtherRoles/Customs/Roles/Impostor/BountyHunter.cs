using System.Linq;
using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.EnoFramework.Utils;
using TheOtherRoles.Objects;

namespace TheOtherRoles.Customs.Roles.Impostor;

[EnoSingleton]
public class BountyHunter : CustomRole
{
    public Arrow? Arrow;

    public readonly EnoFramework.CustomOption BountyDuration;
    public readonly EnoFramework.CustomOption BountyKillCooldown;
    public readonly EnoFramework.CustomOption PunishmentTime;
    public readonly EnoFramework.CustomOption ShowArrow;
    public readonly EnoFramework.CustomOption ArrowUpdateInterval;

    public float BountyUpdateTimer;
    public float ArrowUpdateTimer;

    public TMPro.TextMeshPro? CooldownText;
    public PlayerControl? Bounty;

    public BountyHunter() : base(nameof(BountyHunter))
    {
        Team = Teams.Impostor;
        Color = Palette.ImpostorRed;
        CanTarget = true;

        BountyDuration = OptionsTab.CreateFloatList(
            $"{Name}{nameof(BountyDuration)}",
            Colors.Cs(Color, "Bounty duration"),
            10f,
            120f,
            30f,
            5f,
            SpawnRate,
            string.Empty,
            "s");
        BountyKillCooldown = OptionsTab.CreateFloatList(
            $"{Name}{nameof(BountyKillCooldown)}",
            Colors.Cs(Color, "Cooldown after killing bounty"),
            2.5f,
            60f,
            10f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        PunishmentTime = OptionsTab.CreateFloatList(
            $"{Name}{nameof(PunishmentTime)}",
            Colors.Cs(Color, "Additional cooldown after killing others"),
            2.5f,
            60f,
            10f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        ShowArrow = OptionsTab.CreateBool(
            $"{Name}{nameof(ShowArrow)}",
            Colors.Cs(Color, "Show arrow pointing towards the bounty"),
            false,
            SpawnRate);
        ArrowUpdateInterval = OptionsTab.CreateFloatList(
            $"{Name}{nameof(ArrowUpdateInterval)}",
            Colors.Cs(Color, "Arrow update interval"),
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
        Arrow = new Arrow(Color);
        Bounty = null;
        ArrowUpdateTimer = 0f;
        BountyUpdateTimer = 0f;
        if (Arrow?.arrow != null) UnityEngine.Object.Destroy(Arrow.arrow);
        Arrow = null;
        if (CooldownText != null && CooldownText.gameObject != null)
            UnityEngine.Object.Destroy(CooldownText.gameObject);
        CooldownText = null;
        foreach (var p in TORMapOptions.playerIcons.Values.Where(p => p != null && p.gameObject != null))
        {
            p.gameObject.SetActive(false);
        }
    }
}