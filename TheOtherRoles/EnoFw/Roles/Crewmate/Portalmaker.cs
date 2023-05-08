using System;
using Reactor.Networking.Attributes;
using TheOtherRoles.EnoFw.Kernel;
using TheOtherRoles.Objects;
using UnityEngine;
using Option = TheOtherRoles.EnoFw.Kernel.CustomOption;

namespace TheOtherRoles.EnoFw.Roles.Crewmate;

public class Portalmaker : AbstractRole
{
    public static readonly Portalmaker Instance = new();
    
    // Fields
    
    // Options
    public readonly Option PortalCooldown;
    public readonly Option UsePortalCooldown;
    public readonly Option LogOnlyColorType;
    public readonly Option LogHasTime;
    public readonly Option CanPortalFromAnywhere;

    public static Sprite PlacePortalButtonSprite => GetSprite("TheOtherRoles.Resources.PlacePortalButton.png", 115f);
    public static Sprite UsePortalButtonSprite => GetSprite("TheOtherRoles.Resources.UsePortalButton.png", 115f);

    private Portalmaker() : base(nameof(Portalmaker), "Portal maker")
    {
        Team = Teams.Crewmate;
        Color = new Color32(69, 69, 169, byte.MaxValue);
        
        SpawnRate = GetDefaultSpawnRateOption();
        
        PortalCooldown = Tab.CreateFloatList(
            $"{Key}{nameof(PortalCooldown)}",
            Cs("Portal cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        UsePortalCooldown = Tab.CreateFloatList(
            $"{Key}{nameof(UsePortalCooldown)}",
            Cs("Use portal cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        LogOnlyColorType = Tab.CreateBool(
            $"{Key}{nameof(LogOnlyColorType)}",
            Cs("Log only display color type"),
            true,
            SpawnRate);
        LogHasTime = Tab.CreateBool(
            $"{Key}{nameof(LogHasTime)}",
            Cs("Log display time"),
            true,
            SpawnRate);
        CanPortalFromAnywhere = Tab.CreateBool(
            $"{Key}{nameof(CanPortalFromAnywhere)}",
            Cs("Can use portal from everywhere"),
            true,
            SpawnRate);
    }

    public static void UsePortal(byte playerId, byte exit)
    {
        var data = new Tuple<byte, byte>(playerId, exit);
        Rpc_UsePortal(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.UsePortal)]
    private static void Rpc_UsePortal(PlayerControl sender, string rawData)
    {
        var (playerId, exit) = Rpc.Deserialize<Tuple<byte, byte>>(rawData);
        Local_UsePortal(playerId, exit);
    }
    
    public static void Local_UsePortal(byte playerId, byte exit)
    {
        Portal.startTeleport(playerId, exit);
    }

    public static void PlacePortal(float x, float y)
    {
        var data = new Tuple<float, float>(x, y);
        Rpc_PlacePortal(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.PlacePortal)]
    private static void Rpc_PlacePortal(PlayerControl sender, string rawData)
    {
        var (x, y) = Rpc.Deserialize<Tuple<float, float>>(rawData);
        var position = Vector3.zero;
        position.x = x;
        position.y = y;
        var _ = new Portal(position);
    }
}