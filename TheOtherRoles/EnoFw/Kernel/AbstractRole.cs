using System.Collections.Generic;
using TheOtherRoles.EnoFw.Utils;
using TheOtherRoles.Players;
using UnityEngine;

namespace TheOtherRoles.EnoFw.Kernel;

public abstract class AbstractRole
{
    public static readonly Dictionary<string, AbstractRole> AllRoles = new();

    protected static Sprite GetSprite(string path, float pixelsPerUnit)
    {
        return Helpers.loadSpriteFromResources(path, pixelsPerUnit);
    }

    public readonly string Key;
    public string Name { get; protected set; }

    public Color Color { get; protected set; } = Palette.CrewmateBlue;
    public Teams Team { get; protected set; } = Teams.Crewmate;
    public PlayerControl Player { get; set; }
    public PlayerControl CurrentTarget { get; set; }
    public bool IsAssignable { get; protected set; }
    public bool CanTarget { get; protected set; }
    public readonly List<AbstractRole> IncompatibleRoles = new();
    public readonly List<AbstractRole> RequiredRoles = new();

    public CustomOption SpawnRate { get; protected set; }

    public bool HasPlayer => Player != null;
    public bool IsLocalPlayer => HasPlayer && Player.PlayerId == CachedPlayer.LocalPlayer.PlayerId;

    public bool IsAliveLocalPlayer => IsLocalPlayer && !Player.Data.IsDead;

    public bool ShouldShowRoleInfos => HasPlayer && (IsAliveLocalPlayer || Helpers.shouldShowGhostInfo());
    
    public bool IsDeadOrDisconnected => !HasPlayer || Player.Data.IsDead || Player.Data.Disconnected;

    protected CustomOption.Tab Tab => Team switch
    {
        Teams.Crewmate => CustomOptions.CrewmateSettings,
        Teams.Impostor => CustomOptions.ImpostorsSettings,
        _ => CustomOptions.NeutralSettings
    };

    public enum Teams
    {
        Crewmate,
        Impostor,
        Neutral
    }

    public virtual void SetTarget(PlayerControl player)
    {
        if (!CanTarget) return;
        CurrentTarget = player;
        if (CurrentTarget == null) return;
        Helpers.setPlayerOutline(CurrentTarget, Color);
    }

    public void SetTarget(byte playerId)
    {
        if (!CanTarget) return;
        var player = Helpers.playerById(playerId);
        SetTarget(player);
    }

    protected CustomOption GetDefaultSpawnRateOption()
    {
        return Tab.CreateFloatList(
            $"{Key}{nameof(SpawnRate)}",
            Cs(Name),
            0f,
            100f,
            50f,
            10f,
            null,
            string.Empty,
            "%");
    }

    protected AbstractRole(string key, string name, bool isAssignable = true)
    {
        IsAssignable = isAssignable;
        Key = key;
        Name = name;
        AllRoles.Add(Key, this);
    }

    public string Cs(string text)
    {
        return Colors.Cs(Color, text);
    }

    public virtual void ClearAndReload()
    {
        Player = null;
        CurrentTarget = null;
    }
}