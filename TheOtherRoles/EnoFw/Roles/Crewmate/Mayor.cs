using System;
using Reactor.Networking.Attributes;
using TheOtherRoles.Patches;
using TheOtherRoles.Players;
using UnityEngine;

namespace TheOtherRoles.EnoFw.Roles.Crewmate;

public static class Mayor
{
    public static PlayerControl mayor;
    public static Color color = new Color32(32, 77, 66, byte.MaxValue);
    public static Minigame emergency = null;
    public static Sprite emergencySprite = null;
    public static int remoteMeetingsLeft = 1;

    public static bool canSeeVoteColors = false;
    public static int tasksNeededToSeeVoteColors;
    public static bool meetingButton = true;
    public static int mayorChooseSingleVote;

    public static bool voteTwice = true;

    public static Sprite getMeetingSprite()
    {
        if (emergencySprite) return emergencySprite;
        emergencySprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.EmergencyButton.png", 550f);
        return emergencySprite;
    }

    public static void clearAndReload()
    {
        mayor = null;
        emergency = null;
        emergencySprite = null;
        remoteMeetingsLeft = Mathf.RoundToInt(CustomOptionHolder.mayorMaxRemoteMeetings.getFloat());
        canSeeVoteColors = CustomOptionHolder.mayorCanSeeVoteColors.getBool();
        tasksNeededToSeeVoteColors = (int)CustomOptionHolder.mayorTasksNeededToSeeVoteColors.getFloat();
        meetingButton = CustomOptionHolder.mayorMeetingButton.getBool();
        mayorChooseSingleVote = CustomOptionHolder.mayorChooseSingleVote.getSelection();
        voteTwice = true;
    }

    public static void MayorSetVoteTwice(bool value)
    {
        var data = new Tuple<bool>(value);
        Rpc_MayorSetVoteTwice(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.MayorSetVoteTwice)]
    private static void Rpc_MayorSetVoteTwice(PlayerControl sender, string rawData)
    {
        voteTwice = Rpc.Deserialize<Tuple<bool>>(rawData).Item1;
    }
}