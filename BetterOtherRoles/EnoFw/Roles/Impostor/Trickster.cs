using System;
using BetterOtherRoles.EnoFw.Kernel;
using BetterOtherRoles.Objects;
using BetterOtherRoles.Players;
using Reactor.Networking.Attributes;
using UnityEngine;
using Option = BetterOtherRoles.EnoFw.Kernel.CustomOption;

namespace BetterOtherRoles.EnoFw.Roles.Impostor;

public class Trickster : AbstractRole
{
    public static readonly Trickster Instance = new();
    
    // Fields
    public float LightsOutTimer;
    
    // Options
    public readonly CustomOption PlaceBoxCooldown;
    public readonly CustomOption LightsOutCooldown;
    public readonly CustomOption LightsOutDuration;

    public static Sprite PlaceBoxButtonSprite => GetSprite("BetterOtherRoles.Resources.PlaceJackInTheBoxButton.png", 115f);
    public static Sprite LightsOutButtonSprite => GetSprite("BetterOtherRoles.Resources.LightsOutButton.png", 115f);
    public static Sprite TricksterVentButtonSprite => GetSprite("BetterOtherRoles.Resources.TricksterVentButton.png", 115f);

    private Trickster() : base(nameof(Trickster), "Trickster")
    {
        Team = Teams.Impostor;
        Color = Palette.ImpostorRed;
        
        SpawnRate = GetDefaultSpawnRateOption();
        
        PlaceBoxCooldown = Tab.CreateFloatList(
            $"{Key}{nameof(PlaceBoxCooldown)}",
            Cs("Place box cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        LightsOutCooldown = Tab.CreateFloatList(
            $"{Key}{nameof(LightsOutCooldown)}",
            Cs("Lights out cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        LightsOutDuration = Tab.CreateFloatList(
            $"{Key}{nameof(LightsOutDuration)}",
            Cs("Lights out duration"),
            5f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        LightsOutTimer = 0f;
    }

    public static void LightsOut()
    {
        Rpc_LightsOut(PlayerControl.LocalPlayer);
    }

    [MethodRpc((uint)Rpc.Role.LightsOut)]
    private static void Rpc_LightsOut(PlayerControl sender)
    {
        Instance.LightsOutTimer = Instance.LightsOutDuration;
        // If the local player is impostor indicate lights out
        if (!Helpers.hasImpVision(GameData.Instance.GetPlayerById(CachedPlayer.LocalPlayer.PlayerId))) return;
        var _ = new CustomMessage("Lights are out", Instance.LightsOutDuration);
    }

    public static void PlaceJackInTheBox(float x, float y)
    {
        var data = new Tuple<float, float>(x, y);
        Rpc_PlaceJackInTheBox(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.PlaceJackInTheBox)]
    private static void Rpc_PlaceJackInTheBox(PlayerControl sender, string rawData)
    {
        var (x, y) = Rpc.Deserialize<Tuple<float, float>>(rawData);
        var position = Vector3.zero;
        position.x = x;
        position.y = y;
        position.z = sender.transform.position.z;
        var _ = new JackInTheBox(position);
    }
}