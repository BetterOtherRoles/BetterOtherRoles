using System;
using BetterOtherRoles.EnoFw.Kernel;
using BetterOtherRoles.Objects;
using UnityEngine;
using Option = BetterOtherRoles.EnoFw.Kernel.CustomOption;

namespace BetterOtherRoles.EnoFw.Roles.Impostor;

public class Vampire : AbstractRole
{
    public static readonly Vampire Instance = new();

    // Fields
    public PlayerControl Bitten;
    public bool TargetNearGarlic;
    public bool LocalPlacedGarlic;
    public bool GarlicsActive => SpawnRate > 0;

    // Options
    public readonly CustomOption BiteDelay;
    public readonly CustomOption BiteCooldown;
    public readonly CustomOption CanKillNearGarlics;

    private Vampire() : base(nameof(Vampire), "Vampire")
    {
        Team = Teams.Impostor;
        Color = Palette.ImpostorRed;
        CanTarget = true;
        
        SpawnRate = GetDefaultSpawnRateOption();

        BiteDelay = Tab.CreateFloatList(
            $"{Key}{nameof(BiteDelay)}",
            Cs("Vampire kill delay"),
            1f,
            20f,
            10f,
            1f,
            SpawnRate,
            string.Empty,
            "s");
        BiteCooldown = Tab.CreateFloatList(
            $"{Key}{nameof(BiteCooldown)}",
            Cs($"{Name} cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        CanKillNearGarlics = Tab.CreateBool(
            $"{Name}{nameof(CanKillNearGarlics)},",
            Cs("Can kill near garlics"),
            true,
            SpawnRate);
    }

    public static Sprite BiteButtonSprite => GetSprite("BetterOtherRoles.Resources.VampireButton.png", 115f);
    public static Sprite GarlicButtonSprite => GetSprite("BetterOtherRoles.Resources.GarlicButton.png", 115f);

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        Bitten = null;
        TargetNearGarlic = false;
        LocalPlacedGarlic = false;
    }

    public static void VampireSetBitten(byte targetId, bool performReset)
    {
        var data = new Tuple<byte, bool>(targetId, performReset);
        RpcManager.Instance.Send((uint)Rpc.Role.VampireSetBitten, data);
    }

    [BindRpc((uint)Rpc.Role.VampireSetBitten)]
    public static void Rpc_VampireSetBitten(Tuple<byte, bool> data)
    {
        var (targetId, performReset) = data;
        if (performReset)
        {
            Instance.Bitten = null;
            return;
        }

        if (Instance.Player == null) return;
        var player = Helpers.playerById(targetId);
        if (player.Data.IsDead) return;
        Instance.Bitten = player;
    }

    public static void PlaceGarlic(float x, float y, float z)
    {
        var data = new Tuple<float, float, float>(x, y, z);
        RpcManager.Instance.Send((uint)Rpc.Role.PlaceGarlic, data, false);
    }

    [BindRpc((uint)Rpc.Role.PlaceGarlic)]
    public static void Rpc_PlaceGarlic(Tuple<float, float, float> xyz)
    {
        var (x, y, z) = xyz;
        var position = new Vector3(x, y, z);
        var _ = new Garlic(position);
    }
}