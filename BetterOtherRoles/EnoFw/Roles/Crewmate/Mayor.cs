using System;
using BetterOtherRoles.EnoFw.Kernel;
using BetterOtherRoles.EnoFw.Libs.Reactor.Networking.Attributes;
using UnityEngine;
using Option = BetterOtherRoles.EnoFw.Kernel.CustomOption;

namespace BetterOtherRoles.EnoFw.Roles.Crewmate;

public class Mayor : AbstractRole
{
    public static readonly Mayor Instance = new();
    
    // Fields
    public int UsedRemoteMeetings;
    public bool VoteTwice = true;
    public int RemoteMeetingsLeft => MaxRemoteMeetings - UsedRemoteMeetings;

    // Options
    public readonly CustomOption CanChooseSingleVote;
    public readonly CustomOption CanSeeVoteColors;
    public readonly CustomOption TasksNeededToSeeVoteColors;
    public readonly CustomOption HasRemoteMeetingButton;
    public readonly CustomOption MaxRemoteMeetings;

    public static Sprite MeetingButtonSprite => GetSprite("BetterOtherRoles.Resources.EmergencyButton.png", 550f);

    private Mayor() : base(nameof(Mayor), "Mayor")
    {
        Team = Teams.Crewmate;
        Color = new Color32(32, 77, 66, byte.MaxValue);
        
        SpawnRate = GetDefaultSpawnRateOption();
        
        CanChooseSingleVote = Tab.CreateBool(
            $"{Key}{nameof(CanChooseSingleVote)}",
            Cs("Can choose single vote"),
            false,
            SpawnRate);
        CanSeeVoteColors = Tab.CreateBool(
            $"{Key}{nameof(CanSeeVoteColors)}",
            Cs("Can see vote colors"),
            false,
            SpawnRate);
        TasksNeededToSeeVoteColors = Tab.CreateFloatList(
            $"{Key}{nameof(TasksNeededToSeeVoteColors)}",
            Cs("Completed tasks needed to see vote colors"),
            0f,
            20f,
            5f,
            1f,
            CanSeeVoteColors);
        HasRemoteMeetingButton = Tab.CreateBool(
            $"{Key}{nameof(HasRemoteMeetingButton)}",
            Cs("Has mobile emergency button"),
            false,
            SpawnRate);
        MaxRemoteMeetings = Tab.CreateFloatList(
            $"{Key}{nameof(MaxRemoteMeetings)}",
            Cs("Number of remote meetings"),
            1f,
            5f,
            1f,
            1f,
            HasRemoteMeetingButton);
    }
    
    public override void ClearAndReload()
    {
        base.ClearAndReload();
        UsedRemoteMeetings = 0;
        VoteTwice = true;
    }

    public static void MayorSetVoteTwice(bool value)
    {
        var data = new Tuple<bool>(value);
        Rpc_MayorSetVoteTwice(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.MayorSetVoteTwice)]
    private static void Rpc_MayorSetVoteTwice(PlayerControl sender, string rawData)
    {
        Instance.VoteTwice = Rpc.Deserialize<Tuple<bool>>(rawData).Item1;
    }
}