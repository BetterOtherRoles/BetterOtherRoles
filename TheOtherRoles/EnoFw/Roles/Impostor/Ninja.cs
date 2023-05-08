using System;
using AmongUs.Data;
using Reactor.Networking.Attributes;
using TheOtherRoles.EnoFw.Kernel;
using TheOtherRoles.Objects;
using TheOtherRoles.Players;
using UnityEngine;
using Option = TheOtherRoles.EnoFw.Kernel.CustomOption;

namespace TheOtherRoles.EnoFw.Roles.Impostor;

public class Ninja : AbstractRole
{
    public static readonly Ninja Instance = new();

    // Fields
    public PlayerControl MarkedTarget;
    public Arrow Arrow;
    public bool IsInvisible;
    public float InvisibilityTimer;
    
    // Options
    public readonly Option NinjaCooldown;
    public readonly Option KnowsTargetLocation;
    public readonly Option TraceDuration;
    public readonly Option TraceColorDuration;
    public readonly Option InvisibilityDuration;

    public static Sprite MarkButtonSprite => GetSprite("TheOtherRoles.Resources.NinjaMarkButton.png", 115f);
    public static Sprite AssassinateButtonSprite => GetSprite("TheOtherRoles.Resources.NinjaAssassinateButton.png", 115f);

    private Ninja() : base(nameof(Ninja), "Ninja")
    {
        Team = Teams.Impostor;
        Color = Palette.ImpostorRed;
        CanTarget = true;
        
        SpawnRate = GetDefaultSpawnRateOption();
        
        NinjaCooldown = Tab.CreateFloatList(
            $"{Key}{nameof(NinjaCooldown)}",
            Cs($"{Name} cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        KnowsTargetLocation = Tab.CreateBool(
            $"{Key}{nameof(KnowsTargetLocation)}",
            Cs("Knows location of target"),
            true,
            SpawnRate);
        TraceDuration = Tab.CreateFloatList(
            $"{Key}{nameof(TraceDuration)}",
            Cs("Trace duration"),
            1f,
            20f,
            5f,
            0.5f,
            SpawnRate,
            string.Empty,
            "s");
        TraceColorDuration = Tab.CreateFloatList(
            $"{Key}{nameof(TraceColorDuration)}",
            Cs("Time till trace color has faded"),
            0f,
            20f,
            2f,
            0.5f,
            SpawnRate,
            string.Empty,
            "s");
        InvisibilityDuration = Tab.CreateFloatList(
            $"{Key}{nameof(InvisibilityDuration)}",
            Cs("Invisibility duration"),
            0f,
            20f,
            3f,
            1f,
            SpawnRate,
            string.Empty,
            "s");
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        MarkedTarget = null;
        InvisibilityTimer = 0f;
        IsInvisible = false;
        if (Arrow != null && Arrow.arrow != null) UnityEngine.Object.Destroy(Arrow.arrow);
        Arrow = new Arrow(Color.black);
        if (Arrow.arrow != null) Arrow.arrow.SetActive(false);
    }

    public static void SetInvisible(byte playerId, bool visible)
    {
        var data = new Tuple<byte, bool>(playerId, visible);
        Rpc_SetInvisible(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.SetInvisible)]
    private static void Rpc_SetInvisible(PlayerControl sender, string rawData)
    {
        var (playerId, visible) = Rpc.Deserialize<Tuple<byte, bool>>(rawData);

        var target = Helpers.playerById(playerId);
        if (target == null) return;
        if (visible)
        {
            target.cosmetics.currentBodySprite.BodySprite.color = Color.white;
            target.cosmetics.colorBlindText.gameObject.SetActive(DataManager.Settings.Accessibility.ColorBlindMode);
            target.cosmetics.colorBlindText.color = target.cosmetics.colorBlindText.color.SetAlpha(1f);
            if (Camouflager.Instance.CamouflageTimer <= 0) target.setDefaultLook();
            Instance.IsInvisible = false;
            return;
        }

        target.setLook("", 6, "", "", "", "");
        var color = Color.clear;
        if (CachedPlayer.LocalPlayer.Data.Role.IsImpostor || CachedPlayer.LocalPlayer.Data.IsDead) color.a = 0.1f;
        target.cosmetics.currentBodySprite.BodySprite.color = color;
        target.cosmetics.colorBlindText.color = target.cosmetics.colorBlindText.color.SetAlpha(color.a);
        target.cosmetics.colorBlindText.gameObject.SetActive(false);
        Instance.InvisibilityTimer = Instance.InvisibilityDuration;
        Instance.IsInvisible = true;
    }

    public static void PlaceNinjaTrace(float x, float y)
    {
        var data = new Tuple<float, float>(x, y);
        Rpc_PlaceNinjaTrace(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.PlaceNinjaTrace)]
    private static void Rpc_PlaceNinjaTrace(PlayerControl sender, string rawData)
    {
        var (x, y) = Rpc.Deserialize<Tuple<float, float>>(rawData);
        var position = Vector3.zero;
        position.x = x;
        position.y = y;
        var _ = new NinjaTrace(position, Instance.TraceDuration);
        if (CachedPlayer.LocalPlayer.PlayerControl == Instance.Player) return;
        Instance.MarkedTarget = null;
    }
}