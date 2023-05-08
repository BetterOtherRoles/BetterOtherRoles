using System;
using System.Collections.Generic;
using Reactor.Networking.Attributes;
using TheOtherRoles.EnoFw.Kernel;
using UnityEngine;
using Option = TheOtherRoles.EnoFw.Kernel.CustomOption;

namespace TheOtherRoles.EnoFw.Roles.Impostor;

public class Eraser : AbstractRole
{
    public static readonly Eraser Instance = new();
    
    // Fields
    public readonly List<byte> AlreadyErased = new();
    public readonly List<PlayerControl> FutureErased = new();
    
    // Options
    public readonly Option EraseCooldown;
    public readonly Option CanEraseAnyone;

    public static Sprite EraseButtonSprite => GetSprite("TheOtherRoles.Resources.EraserButton.png", 115f);

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
        var data = new Tuple<byte>(playerId);
        Rpc_SetFutureErased(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.SetFutureErased)]
    private static void Rpc_SetFutureErased(PlayerControl sender, string rawData)
    {
        var playerId = Rpc.Deserialize<Tuple<byte>>(rawData).Item1;
        var player = Helpers.playerById(playerId);
        if (player == null) return;
        Instance.FutureErased.Add(player);
    }
}