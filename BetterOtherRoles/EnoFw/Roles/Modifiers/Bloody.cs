using System;
using System.Collections.Generic;
using BetterOtherRoles.EnoFw.Kernel;
using UnityEngine;
using Option = BetterOtherRoles.EnoFw.Kernel.CustomOption;

namespace BetterOtherRoles.EnoFw.Roles.Modifiers;

public class Bloody : AbstractMultipleModifier
{
    public static readonly Bloody Instance = new();

    public readonly Dictionary<byte, float> Active = new();
    public readonly Dictionary<byte, byte> BloodyKillerMap = new();

    public readonly CustomOption Duration;

    private Bloody() : base(nameof(Bloody), "Bloody", Color.yellow)
    {
        Duration = CustomOptions.ModifierSettings.CreateFloatList(
            $"{Key}{nameof(Duration)}",
            CustomOptions.Cs(Color, "Trail duration"),
            3f,
            60f,
            10f,
            1f,
            SpawnRate,
            string.Empty,
            "s");
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        Active.Clear();
        BloodyKillerMap.Clear();
    }

    public static void SetBloody(byte killerPlayerId, byte bloodyPlayerId)
    {
        var data = new Tuple<byte, byte>(killerPlayerId, bloodyPlayerId);
        RpcManager.Instance.Send((uint)Rpc.Role.SetBloody, data);
    }

    [BindRpc((uint)Rpc.Role.SetBloody)]
    public static void Rpc_SetBloody(Tuple<byte, byte> data)
    {
        var (killerPlayerId, bloodyPlayerId) = data;
        if (Instance.Active.ContainsKey(killerPlayerId)) return;
        Instance.Active.Add(killerPlayerId, Instance.Duration);
        Instance.BloodyKillerMap.Add(killerPlayerId, bloodyPlayerId);
    }
}