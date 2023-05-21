using System.Collections.Generic;
using System.Linq;
using BetterOtherRoles.EnoFw.Utils;
using UnityEngine;

namespace BetterOtherRoles.EnoFw.Kernel;

public  abstract class AbstractMultipleModifier : AbstractModifier
{
    public readonly List<PlayerControl> Players = new();
    public readonly CustomOption Quantity;
    
    protected AbstractMultipleModifier(string key, string name, Color color) : base(key, name, color)
    {
        Quantity = CustomOptions.ModifierSettings.CreateFloatList(
            $"{Key}{nameof(Quantity)}",
            Colors.Cs(Color, "Quantity"),
            1f,
            15f,
            1f,
            1f,
            SpawnRate);
    }

    public override bool Is(byte playerId)
    {
        return Players.Any(p => p.PlayerId == playerId);
    }

    public override bool Is(PlayerControl player)
    {
        return player != null && Is(player.PlayerId);
    }

    public override void ClearAndReload()
    {
        Players.Clear();
    }
}