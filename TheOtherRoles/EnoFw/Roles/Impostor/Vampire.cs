using System;
using Reactor.Networking.Attributes;
using TheOtherRoles.EnoFw.Kernel;
using TheOtherRoles.Objects;
using UnityEngine;
using Option = TheOtherRoles.EnoFw.Kernel.CustomOption;

namespace TheOtherRoles.EnoFw.Roles.Impostor;

public class Vampire : AbstractRole
{
    public static readonly Vampire Instance = new();

    // Fields
    public PlayerControl Bitten;
    public bool TargetNearGarlic;
    public bool LocalPlacedGarlic;
    public bool GarlicsActive => SpawnRate > 0;

    // Options
    public readonly Option BiteDelay;
    public readonly Option BiteCooldown;
    public readonly Option CanKillNearGarlics;

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

    public static Sprite BiteButtonSprite => GetSprite("TheOtherRoles.Resources.VampireButton.png", 115f);
    public static Sprite GarlicButtonSprite => GetSprite("TheOtherRoles.Resources.GarlicButton.png", 115f);

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
        Rpc_VampireSetBitten(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.VampireSetBitten)]
    private static void Rpc_VampireSetBitten(PlayerControl sender, string rawData)
    {
        var (targetId, performReset) = Rpc.Deserialize<Tuple<byte, bool>>(rawData);
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
        Rpc_PlaceGarlic(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.PlaceGarlic)]
    private static void Rpc_PlaceGarlic(PlayerControl sender, string rawData)
    {
        var (x, y, z) = Rpc.Deserialize<Tuple<float, float, float>>(rawData);
        var position = new Vector3(x, y, z);
        position.x = x;
        position.y = y;
        var _ = new Garlic(position);
    }
}