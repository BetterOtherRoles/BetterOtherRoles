using System.Collections.Generic;
using TheOtherRoles.EnoFramework;
using TheOtherRoles.EnoFramework.Kernel;
using UnityEngine;
using TheOtherRoles.EnoFramework.Utils;

namespace TheOtherRoles.Customs.Roles;

public abstract class CustomRole
{
    public static readonly List<CustomRole> AllRoles = new();

    public static CustomRole? GetRoleByName(string name)
    {
        return AllRoles.Find(role => role.Name == name);
    }
    
    public readonly string Name;
    public Color Color = Palette.CrewmateBlue;
    public Teams Team = Teams.Crewmate;
    public bool CanTarget = false;
    public bool TriggerWin = false;
    public PlayerControl? Player;
    public PlayerControl? CurrentTarget;
    public EnoFramework.CustomOption SpawnRate { get; private set; }

    protected EnoFramework.CustomOption.Tab OptionsTab => Team switch
    {
        Teams.Crewmate => Singleton<CustomOptionsHolder>.Instance.CrewmateSettings,
        Teams.Impostor => Singleton<CustomOptionsHolder>.Instance.ImpostorsSettings,
        _ => Singleton<CustomOptionsHolder>.Instance.NeutralSettings
    };

    public bool IsCrewmate => Team == Teams.Crewmate;
    public bool IsImpostor => Team == Teams.Impostor;
    public bool IsNeutral => Team == Teams.Neutral;

    public CustomRole(string name)
    {
        Name = name;

        SpawnRate = OptionsTab.CreateFloatList(
            $"{Name}SpawnRate",
            Colors.Cs(Color, "Spawn rate"),
            0f,
            100f,
            50f,
            10f,
            null,
            string.Empty,
            "%");
        AllRoles.Add(this);
    }

    public bool Is(PlayerControl player)
    {
        return Player == player;
    }

    public void SetPlayer(PlayerControl player)
    {
        Player = player;
    }

    public virtual void CreateCustomButtons(HudManager hudManager)
    {
        
    }

    public virtual void ClearAndReload()
    {
        Player = null;
        CurrentTarget = null;
    }

    public enum Teams
    {
        Crewmate,
        Impostor,
        Neutral
    }
}