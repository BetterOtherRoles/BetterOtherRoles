using System;
using BetterOtherRoles.EnoFw.Kernel;
using BetterOtherRoles.Objects;
using BetterOtherRoles.EnoFw.Libs.Reactor.Networking.Attributes;
using UnityEngine;
using Option = BetterOtherRoles.EnoFw.Kernel.CustomOption;

namespace BetterOtherRoles.EnoFw.Roles.Crewmate;

public class Portalmaker : AbstractRole
{
    public static readonly Portalmaker Instance = new();
    
    // Fields
    
    // Options
    public readonly CustomOption PortalCooldown;
    public readonly CustomOption UsePortalCooldown;
    public readonly CustomOption LogOnlyColorType;
    public readonly CustomOption LogHasTime;
    public readonly CustomOption CanPortalFromAnywhere;

    public static Sprite PlacePortalButtonSprite => GetSprite("BetterOtherRoles.Resources.PlacePortalButton.png", 115f);
    public static Sprite UsePortalButtonSprite => GetSprite("BetterOtherRoles.Resources.UsePortalButton.png", 115f);

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

    public static void PlacePortal(float x, float y, float z)
    {
        var data = new Tuple<float, float, float>(x, y, z);
        Rpc_PlacePortal(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.PlacePortal, false)]
    private static void Rpc_PlacePortal(PlayerControl sender, string rawData)
    {
        var (x, y, z) = Rpc.Deserialize<Tuple<float, float, float>>(rawData);
        var position = new Vector3(x, y, z);
        var _ = new Portal(position);
    }
}