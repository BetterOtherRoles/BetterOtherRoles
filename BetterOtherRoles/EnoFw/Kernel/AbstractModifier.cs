using System.Collections.Generic;
using BetterOtherRoles.EnoFw.Utils;
using UnityEngine;

namespace BetterOtherRoles.EnoFw.Kernel;

public abstract class AbstractModifier
{
    public static readonly Dictionary<string, AbstractModifier> AllModifiers = new();
    
    public readonly string Key;
    public readonly string Name;
    public Color Color;
    public readonly List<AbstractRole.Teams> AllowedTeams = new();
    public readonly CustomOption SpawnRate;

    protected AbstractModifier(string key, string name, Color color)
    {
        Key = key;
        Name = name;
        Color = color;
        AllModifiers.Add(Key, this);

        SpawnRate = CustomOptions.ModifierSettings.CreateFloatList(
            $"{Key}{nameof(SpawnRate)}",
            CustomOptions.Cs(Color, Name),
            0f,
            100f,
            0f,
            10f,
            null,
            string.Empty,
            "%");
    }

    public abstract bool Is(PlayerControl player);
    public abstract bool Is(byte playerId);
    public abstract void ClearAndReload();
}