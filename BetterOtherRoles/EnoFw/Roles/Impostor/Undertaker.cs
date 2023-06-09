﻿using System;
using System.Collections.Generic;
using System.Linq;
using BetterOtherRoles.EnoFw.Kernel;
using BetterOtherRoles.Players;
using UnityEngine;
using Option = BetterOtherRoles.EnoFw.Kernel.CustomOption;

namespace BetterOtherRoles.EnoFw.Roles.Impostor;

public class Undertaker : AbstractRole
{
    public static readonly Undertaker Instance = new();

    // Fields
    public DeadBody CurrentDeadTarget;
    public DeadBody DraggedBody;
    public bool CanDropBody;
    public DateTime LastDraggedAt;

    public float RealDragDistance
    {
        get
        {
            return (string)DragDistance switch
            {
                "short" => 2f / 3f,
                "medium" => 4f / 3f,
                _ => 2f
            };
        }
    }

    // Options
    public readonly CustomOption DragCooldown;
    public readonly CustomOption SpeedModifierWhenDragging;
    public readonly CustomOption DragDistance;
    public readonly CustomOption DisableKillWhileDragging;
    public readonly CustomOption DisableReportWhileDragging;
    public readonly CustomOption DisableVentWhileDragging;

    public static Sprite DragButtonSprite => GetSprite("BetterOtherRoles.Resources.DragButton.png", 115f);
    public static Sprite DropButtonSprite => GetSprite("BetterOtherRoles.Resources.DropButton.png", 115f);

    private Undertaker() : base(nameof(Undertaker), "Undertaker")
    {
        Team = Teams.Impostor;
        Color = Palette.ImpostorRed;
        CanTarget = false;
        
        SpawnRate = GetDefaultSpawnRateOption();
        
        DragCooldown = Tab.CreateFloatList(
            $"{Key}{nameof(DragCooldown)}",
            Cs("Drag cooldown"),
            2.5f,
            60f,
            20f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        SpeedModifierWhenDragging = Tab.CreateFloatList(
            $"{Key}{nameof(SpeedModifierWhenDragging)}",
            Cs("Speed modifier while dragging"),
            -90f,
            100f,
            -30f,
            5f,
            SpawnRate,
            string.Empty,
            "%");
        DragDistance = Tab.CreateStringList(
            $"{Key}{nameof(DragDistance)}",
            Cs("Drag distance"),
            new List<string> { "short", "medium", "long" },
            "short",
            SpawnRate);
        DisableKillWhileDragging = Tab.CreateBool(
            $"{Key}{nameof(DisableKillWhileDragging)}",
            Cs("Cannot kill while dragging"),
            true,
            SpawnRate);
        DisableReportWhileDragging = Tab.CreateBool(
            $"{Key}{nameof(DisableReportWhileDragging)}",
            Cs("Cannot report while dragging"),
            true,
            SpawnRate);
        DisableVentWhileDragging = Tab.CreateBool(
            $"{Key}{nameof(DisableVentWhileDragging)}",
            Cs("Cannot vent while dragging"),
            true,
            SpawnRate);
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        CurrentDeadTarget = null;
        DraggedBody = null;
        CanDropBody = false;
        LastDraggedAt = DateTime.UtcNow;
    }

    public static void DropBody()
    {
        if (!Instance.IsLocalPlayer) return;
        var pos = Instance.Player.transform.position;
        DropBody(pos.x, pos.y, pos.z);
    }

    public static void DropBody(float x, float y, float z)
    {
        var data = new Tuple<float, float, float>(x, y, z);
        RpcManager.Instance.Send((uint)Rpc.Role.UndertakerDropBody, data);
    }

    [BindRpc((uint)Rpc.Role.UndertakerDropBody)]
    public static void Rpc_DropBody(Tuple<float, float, float> xyz)
    {
        if (Instance.Player == null || Instance.DraggedBody == null) return;

        var (x, y, z) = xyz;
        var transform = Instance.DraggedBody.transform;
        var position = new Vector3(x, y, z);
        transform.position = position;
        Instance.DraggedBody = null;
        Instance.CurrentDeadTarget = null;
        Instance.LastDraggedAt = DateTime.UtcNow;
    }

    public static void DragBody(byte playerId)
    {
        RpcManager.Instance.Send((uint)Rpc.Role.UndertakerDragBody, playerId);
    }

    [BindRpc((uint)Rpc.Role.UndertakerDragBody)]
    public static void Rpc_DragBody(byte playerId)
    {
        if (Instance.Player == null) return;
        var body = UnityEngine.Object.FindObjectsOfType<DeadBody>().FirstOrDefault(b => b.ParentId == playerId);
        if (body == null) return;
        Instance.DraggedBody = body;
    }
}