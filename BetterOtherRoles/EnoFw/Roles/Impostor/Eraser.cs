using System;
using System.Collections.Generic;
using BetterOtherRoles.EnoFw.Kernel;
using UnityEngine;
using Option = BetterOtherRoles.EnoFw.Kernel.CustomOption;

namespace BetterOtherRoles.EnoFw.Roles.Impostor;

public class Eraser : AbstractRole
{
    public static readonly Eraser Instance = new();
    
    // Fields
    public readonly List<byte> AlreadyErased = new();
    public readonly List<PlayerControl> FutureErased = new();
    
    // Options
    public readonly CustomOption EraseCooldown;
    public readonly CustomOption CanEraseAnyone;

    public static Sprite EraseButtonSprite => GetSprite("BetterOtherRoles.Resources.EraserButton.png", 115f);

    private Eraser() : base(nameof(Eraser), "Eraser")
    {
        Team = Teams.Impostor;
        Color = Palette.ImpostorRed;
        CanTarget = true;
        
        SpawnRate = GetDefaultSpawnRateOption();
        
        EraseCooldown = Tab.CreateFloatList(
            nameof(EraseCooldown),
            Cs($"{Key} cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        CanEraseAnyone = Tab.CreateBool(
            $"{Key}{nameof(CanEraseAnyone)}",
            Cs("Can erase anyone"),
            false,
            SpawnRate);
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        FutureErased.Clear();
        AlreadyErased.Clear();
    }

    public static void SetFutureErased(byte playerId)
    {
        RpcManager.Instance.Send((uint)Rpc.Role.SetFutureErased, playerId);
    }

    [BindRpc((uint)Rpc.Role.SetFutureErased)]
    public static void Rpc_SetFutureErased(byte playerId)
    {
        var player = Helpers.playerById(playerId);
        if (player == null) return;
        Instance.FutureErased.Add(player);
    }
}