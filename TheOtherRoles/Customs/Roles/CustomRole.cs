using System;
using System.Collections.Generic;
using System.Linq;
using Reactor.Networking.Attributes;
using TheOtherRoles.EnoFramework;
using TheOtherRoles.EnoFramework.Kernel;
using UnityEngine;
using TheOtherRoles.EnoFramework.Utils;
using TheOtherRoles.Players;

namespace TheOtherRoles.Customs.Roles;

public abstract class CustomRole
{
    public static readonly List<CustomRole> AllRoles = new();

    public static List<CustomRole> CrewmateRoles => AllRoles.Where(role => role.IsCrewmate).ToList();
    public static List<CustomRole> ImpostorRoles => AllRoles.Where(role => role.IsImpostor).ToList();
    public static List<CustomRole> NeutralRoles => AllRoles.Where(role => role.IsNeutral).ToList();

    public static CustomRole? GetRoleByName(string name)
    {
        return AllRoles.Find(role => role.Name == name);
    }

    public static CustomRole? GetRoleByPlayer(PlayerControl player)
    {
        return AllRoles.Find(role => role.Is(player));
    }

    public readonly string Name;
    public string? DisplayName;
    public Color Color = Palette.CrewmateBlue;
    public Teams Team = Teams.Crewmate;
    public bool CanTarget = false;
    public bool TriggerWin = false;
    public PlayerControl? Player;
    public PlayerControl? CurrentTarget;
    public EnoFramework.CustomOption? SpawnRate { get; private set; }
    public readonly List<Type> IncompatibleRoles = new();
    public string IntroDescription = "";
    public string ShortDescription = "";

    public string NameText => DisplayName ?? Name;

    protected EnoFramework.CustomOption.Tab OptionsTab => Team switch
    {
        Teams.Crewmate => Singleton<CustomOptionsHolder>.Instance.CrewmateSettings,
        Teams.Impostor => Singleton<CustomOptionsHolder>.Instance.ImpostorsSettings,
        _ => Singleton<CustomOptionsHolder>.Instance.NeutralSettings
    };

    public bool IsCrewmate => Team == Teams.Crewmate;
    public bool IsImpostor => Team == Teams.Impostor;
    public bool IsNeutral => Team == Teams.Neutral;
    public readonly EnoFramework.RoleInfo Info;

    protected CustomRole(string name, bool hasSpawnRate = true)
    {
        Name = name;
        if (!hasSpawnRate) return;
        SpawnRate = OptionsTab.CreateFloatList(
            $"{Name}{nameof(SpawnRate)}",
            Cs("Spawn rate"),
            0f,
            100f,
            50f,
            10f,
            null,
            string.Empty,
            "%");
        Info = new EnoFramework.RoleInfo(this);
        AllRoles.Add(this);
    }

    public bool Is(PlayerControl player)
    {
        return Player == player;
    }

    public bool Is(byte playerId)
    {
        return Player != null && Player.PlayerId == playerId;
    }

    public bool IsLocalPlayer()
    {
        return Player != null && Is(CachedPlayer.LocalPlayer);
    }

    public bool IsLocalPlayerAndAlive()
    {
        return IsLocalPlayer() && !CachedPlayer.LocalPlayer.Data.IsDead;
    }

    public string Cs(string text)
    {
        return Cs(text);
    }

    public void SetPlayer(PlayerControl player)
    {
        // Player can only have one role
        foreach (var role in AllRoles.Where(role => role.Player == player))
        {
            role.Player = null;
        }

        Player = player;
    }

    public virtual void CreateCustomButtons(HudManager hudManager)
    {
    }

    public virtual void OnPlayerUpdate(PlayerControl player)
    {
    }

    public virtual void OnMeetingCheckForEndVoting(MeetingHud meetingHud)
    {
    }

    public virtual void OnMeetingBloopAVoteIcon(MeetingHud meetingHud, GameData.PlayerInfo voter, int index,
        Transform parent)
    {
    }

    public virtual void OnMeetingPopulateResults(MeetingHud meetingHud, Il2CppStructArray<MeetingHud.VoterState> states)
    {
    }

    public virtual void OnMeetingVotingComplete(MeetingHud meetingHud, byte[] states, GameData.PlayerInfo exiled,
        bool tie)
    {
    }

    public virtual void OnMeetingSelectPlayer(MeetingHud meetingHud)
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

    public static void SetRole(CustomRole role, PlayerControl player)
    {
        RpcSetRole(CachedPlayer.LocalPlayer, $"{player.PlayerId}|{role.Name}");
    }

    public static void ClearAndReloadRoles()
    {
        RpcClearAndReloadRoles(CachedPlayer.LocalPlayer);
    }

    [MethodRpc((uint)Rpc.Id.RoleSetPlayer)]
    private static void RpcSetRole(PlayerControl sender, string rawData)
    {
        var data = rawData.Split("|");
        var playerId = byte.Parse(data[0]);
        var roleName = data[1];
        var role = GetRoleByName(roleName);
        if (role == null) return;
        var player = Helpers.playerById(playerId);
        if (player == null) return;
        role.SetPlayer(player);
    }

    [MethodRpc((uint)Rpc.Id.ClearAndReloadRoles)]
    private static void RpcClearAndReloadRoles(PlayerControl sender)
    {
        foreach (var role in AllRoles)
        {
            role.ClearAndReload();
        }
    }
}