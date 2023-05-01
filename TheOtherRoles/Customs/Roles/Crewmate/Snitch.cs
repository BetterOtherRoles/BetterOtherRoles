using System.Collections.Generic;
using System.Linq;
using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.EnoFramework.Utils;
using TheOtherRoles.Objects;
using TMPro;
using UnityEngine;

namespace TheOtherRoles.Customs.Roles.Crewmate;

[EnoSingleton]
public class Snitch : CustomRole
{
    public readonly EnoFramework.CustomOption LeftTasksForReveal;
    public readonly EnoFramework.CustomOption InfoMode;
    public readonly EnoFramework.CustomOption InfoTargets;
    public readonly EnoFramework.CustomOption ArrowTargets;

    public readonly List<Arrow> Arrows = new();
    public readonly Dictionary<byte, byte> PlayerRoomMap = new();
    public bool IsRevealed;
    public bool NeedsUpdate;
    public TextMeshPro? Text;

    public Snitch() : base(nameof(Snitch))
    {
        Team = Teams.Crewmate;
        Color = new Color32(184, 251, 79, byte.MaxValue);

        IntroDescription = $"Finish your tasks to find the {Colors.Cs(Palette.ImpostorRed, "impostors")}";
        ShortDescription = "Finish your tasks";

        LeftTasksForReveal = OptionsTab.CreateFloatList(
            $"{Name}{nameof(LeftTasksForReveal)}",
            Cs("Task count where the snitch will be revealed"),
            0f,
            25f,
            5f,
            1f,
            SpawnRate);
        InfoMode = OptionsTab.CreateStringList(
            $"{Name}{nameof(InfoMode)}",
            Cs("Information mode"),
            new List<string> { "none", "chat", "map", "chat & map" },
            "none",
            SpawnRate);
        InfoTargets = OptionsTab.CreateStringList(
            $"{Name}{nameof(InfoTargets)}",
            Cs("Targets"),
            new List<string> { "all evil players", "killing players" },
            "all evil players",
            InfoMode);
        ArrowTargets = OptionsTab.CreateStringList(
            $"{Name}{nameof(ArrowTargets)}",
            Cs("Arrow targets"),
            new List<string> { "none", "only for snitch", "only for evil", "both" },
            "none",
            SpawnRate);
    }

    public bool InfoModeChat => (string)InfoMode is "chat" or "chat & map";
    public bool InfoModeMap => (string)InfoMode is "map" or "chat & map";
    public bool InfoTargetKillers => InfoMode != "none" && (string)InfoTargets is "all evil players" or "killing players";
    public bool InfoTargetEvils => InfoMode != "none" && InfoTargets == "all evil players";
    public bool ArrowTargetSnitch => (string)ArrowTargets is "only for snitch" or "both";
    public bool ArrowTargetEvil => (string)ArrowTargets is "only for evil" or "both";

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        foreach (var arrow in Arrows.Where(arrow => arrow.arrow != null))
        {
            UnityEngine.Object.Destroy(arrow.arrow);
        }
        Arrows.Clear();
        IsRevealed = false;
        PlayerRoomMap.Clear();
        if (Text != null) UnityEngine.Object.Destroy(Text);
        NeedsUpdate = false;
    }
}