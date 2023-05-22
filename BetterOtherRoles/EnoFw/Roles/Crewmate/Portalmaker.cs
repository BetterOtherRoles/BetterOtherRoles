using System;
using BetterOtherRoles.EnoFw.Kernel;
using BetterOtherRoles.Objects;
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
        RpcManager.Instance.Send((uint)Rpc.Role.UsePortal, new Tuple<byte, byte>(playerId, exit));
    }

    [BindRpc((uint)Rpc.Role.UsePortal)]
    public static void Rpc_UsePortal(Tuple<byte, byte> data)
    {
        var (playerId, exit) = data;
        Local_UsePortal(playerId, exit);
    }
    
    public static void Local_UsePortal(byte playerId, byte exit)
    {
        Portal.startTeleport(playerId, exit);
    }

    public static void PlacePortal(float x, float y, float z)
    {
        RpcManager.Instance.Send((uint)Rpc.Role.PlacePortal, new Tuple<float, float, float>(x, y, z), false);
    }

    [BindRpc((uint)Rpc.Role.PlacePortal)]
    public static void Rpc_PlacePortal(Tuple<float, float, float> xyz)
    {
        var (x, y, z) = xyz;
        var position = new Vector3(x, y, z);
        var _ = new Portal(position);
    }
}