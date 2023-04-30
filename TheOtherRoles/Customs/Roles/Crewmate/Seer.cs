using System.Collections.Generic;
using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.EnoFramework.Utils;
using UnityEngine;
using Resources = TheOtherRoles.EnoFramework.Utils.Resources;

namespace TheOtherRoles.Customs.Roles.Crewmate;

[EnoSingleton]
public class Seer : CustomRole
{
    private Sprite? _soulSprite;

    public readonly EnoFramework.CustomOption ShowDeathFlash;
    public readonly EnoFramework.CustomOption ShowDeathSouls;
    public readonly EnoFramework.CustomOption LimitSoulsDuration;
    public readonly EnoFramework.CustomOption SoulsDuration;

    public readonly List<Vector3> DeadBodyPositions = new();

    public Seer() : base(nameof(Seer))
    {
        Team = Teams.Crewmate;
        Color = new Color32(97, 178, 108, byte.MaxValue);

        ShowDeathFlash = OptionsTab.CreateBool(
            $"{Name}{nameof(ShowDeathFlash)}",
            Colors.Cs(Color, "Show flash on player death"),
            true,
            SpawnRate);
        ShowDeathSouls = OptionsTab.CreateBool(
            $"{Name}{nameof(ShowDeathSouls)}",
            Colors.Cs(Color, "Show souls of death players"),
            true,
            SpawnRate);
        LimitSoulsDuration = OptionsTab.CreateBool(
            $"{Name}{nameof(LimitSoulsDuration)}",
            Colors.Cs(Color, "Limit souls duration"),
            false,
            ShowDeathSouls);
        SoulsDuration = OptionsTab.CreateFloatList(
            $"{Name}{nameof(SoulsDuration)}",
            Colors.Cs(Color, "Souls duration"),
            0f,
            120f,
            15f,
            5f,
            LimitSoulsDuration,
            string.Empty,
            "s");
    }

    public Sprite GetSoulSprite()
    {
        if (_soulSprite == null)
        {
            _soulSprite = Resources.LoadSpriteFromResources("TheOtherRoles.Resources.Soul.png", 500f);
        }

        return _soulSprite;
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        DeadBodyPositions.Clear();
    }
}