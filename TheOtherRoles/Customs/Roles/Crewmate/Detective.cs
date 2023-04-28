using System.Linq;
using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.EnoFramework.Utils;
using TheOtherRoles.Objects;
using TheOtherRoles.Players;
using UnityEngine;

namespace TheOtherRoles.Customs.Roles.Crewmate;

[EnoSingleton]
public class Detective : CustomRole
{
    public readonly EnoFramework.CustomOption AnonymousFootprint;
    public readonly EnoFramework.CustomOption FootprintInterval;
    public readonly EnoFramework.CustomOption FootprintDuration;
    public readonly EnoFramework.CustomOption ReportNameDuration;
    public readonly EnoFramework.CustomOption ReportColorDuration;

    public float Timer = 6.2f;

    public Detective() : base(nameof(Detective))
    {
        Team = Teams.Crewmate;
        Color = new Color32(45, 106, 165, byte.MaxValue);

        AnonymousFootprint = OptionsTab.CreateBool(
            $"{Name}{nameof(AnonymousFootprint)}",
            Colors.Cs(Color, "Anonymous footprints"),
            false,
            SpawnRate);
        FootprintInterval = OptionsTab.CreateFloatList(
            $"{Name}{nameof(FootprintInterval)}",
            Colors.Cs(Color, "Footprint interval"),
            0.25f,
            10f,
            0.5f,
            0.25f,
            SpawnRate,
            string.Empty,
            "s");
        FootprintDuration = OptionsTab.CreateFloatList(
            $"{Name}{nameof(FootprintDuration)}",
            Colors.Cs(Color, "Footprint duration"),
            0.25f,
            10f,
            5f,
            0.25f,
            SpawnRate,
            string.Empty,
            "s");
        ReportNameDuration = OptionsTab.CreateFloatList(
            $"{Name}{nameof(ReportNameDuration)}",
            Colors.Cs(Color, "Time where detective reports will have name"),
            0f,
            60f,
            0f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        ReportColorDuration = OptionsTab.CreateFloatList(
            $"{Name}{nameof(ReportColorDuration)}",
            Colors.Cs(Color, "Time where detective reports will have color type"),
            0f,
            120f,
            20f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
    }

    public override void OnPlayerUpdate(PlayerControl currentPlayer)
    {
        base.OnPlayerUpdate(currentPlayer);
        if (Player == null || !Is(CachedPlayer.LocalPlayer)) return;
        Timer -= Time.fixedDeltaTime;
        if (Timer > 0f) return;
        Timer = FootprintInterval;
        foreach (var player in CachedPlayer.AllPlayers.Select(p => p.PlayerControl).Where(p =>
                     p != null && p != CachedPlayer.LocalPlayer.PlayerControl && !p.Data.IsDead && !p.inVent))
        {
            FootprintHolder.Instance.MakeFootprint(player);
        }
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        Timer = 6.2f;
    }
}