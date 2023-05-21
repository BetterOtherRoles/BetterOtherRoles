using System;
using BetterOtherRoles.EnoFw.Kernel;
using BetterOtherRoles.Objects;
using Reactor.Networking.Attributes;
using UnityEngine;
using Option = BetterOtherRoles.EnoFw.Kernel.CustomOption;

namespace BetterOtherRoles.EnoFw.Roles.Impostor;

public class Bomber : AbstractRole
{
    public static readonly Bomber Instance = new();
    
    // Fields
    public Bomb Bomb;
    public bool IsPlanted;
    public bool IsActive;
    
    // Options
    public readonly CustomOption BombCooldown;
    public readonly CustomOption BombDestructionTime;
    public readonly CustomOption BombActivationTime;
    public readonly CustomOption BombDestructionRange;
    public readonly CustomOption BombHearRange;
    public readonly CustomOption BombDefuseDuration;

    public static Sprite PlantBombButtonSprite => GetSprite("BetterOtherRoles.Resources.Bomb_Button_Plant.png", 115f);

    private Bomber() : base(nameof(Bomber), "Bomber")
    {
        Team = Teams.Impostor;
        Color = Palette.ImpostorRed;
        CanTarget = true;
        
        SpawnRate = GetDefaultSpawnRateOption();
        
        BombCooldown = Tab.CreateFloatList(
            $"{Key}{nameof(BombCooldown)}",
            Cs("Plant bomb cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        BombDestructionTime = Tab.CreateFloatList(
            $"{Key}{nameof(BombDestructionTime)}",
            Cs("Bomb explosion time"),
            2f,
            60f,
            20f,
            1f,
            SpawnRate,
            string.Empty,
            "s");
        BombActivationTime = Tab.CreateFloatList(
            $"{Key}{nameof(BombActivationTime)}",
            Cs("Bomb activation time"),
            2f,
            30f,
            3f,
            0.5f,
            SpawnRate,
            string.Empty,
            "s");
        BombDestructionRange = Tab.CreateFloatList(
            $"{Key}{nameof(BombDestructionRange)}",
            Cs("Bomb explosion range"),
            5f,
            150f,
            50f,
            5f,
            SpawnRate);
        BombHearRange = Tab.CreateFloatList(
            $"{Key}{nameof(BombHearRange)}",
            Cs("Bomb hear range"),
            5f,
            150f,
            50f,
            5f,
            SpawnRate);
        BombDefuseDuration = Tab.CreateFloatList(
            $"{Key}{nameof(BombDefuseDuration)}",
            Cs("Bomb defuse duration"),
            0.5f,
            15f,
            3f,
            0.5f,
            SpawnRate,
            string.Empty,
            "s");
    }

    public void ClearBomb(bool stopSound = true)
    {
        if (Bomb != null)
        {
            UnityEngine.Object.Destroy(Bomb.bomb);
            UnityEngine.Object.Destroy(Bomb.background);
            Bomb = null;
        }

        IsPlanted = false;
        IsActive = false;
        if (stopSound)
        {
            SoundEffectsManager.stop("bombFuseBurning");
        }
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        if (Bomb != null) ClearBomb(false);
        Bomb.clearBackgroundSprite();
    }

    public static void DefuseBomb()
    {
        Rpc_DefuseBomb(PlayerControl.LocalPlayer);
    }

    [MethodRpc((uint)Rpc.Role.DefuseBomb)]
    private static void Rpc_DefuseBomb(PlayerControl sender)
    {
        SoundEffectsManager.playAtPosition("bombDefused", Instance.Bomb.bomb.transform.position, range: Instance.BombHearRange);
        Instance.ClearBomb();
        HudManagerStartPatch.bomberButton.Timer = HudManagerStartPatch.bomberButton.MaxTimer;
        HudManagerStartPatch.bomberButton.isEffectActive = false;
        HudManagerStartPatch.bomberButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
    }

    public static void PlaceBomb(float x, float y, float z)
    {
        var data = new Tuple<float, float, float>(x, y, z);
        Rpc_PlaceBomb(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.PlaceBomb)]
    private static void Rpc_PlaceBomb(PlayerControl sender, string rawData)
    {
        var (x, y, z) = Rpc.Deserialize<Tuple<float, float, float>>(rawData);
        if (Instance.Player == null) return;
        var position = new Vector3(x, y, z);
        var _ = new Bomb(position);
    }
}