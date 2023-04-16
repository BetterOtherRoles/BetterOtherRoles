using System.Collections.Generic;
using System.Linq;
using Reactor.Networking.Attributes;
using TheOtherRoles.Players;
using Random = System.Random;

namespace TheOtherRoles.Modules;

public static class RandomSeed
{
    
    private static List<int> seeds = new ();

    public static void GenerateSeeds()
    {
        if (CachedPlayer.LocalPlayer == null || !AmongUsClient.Instance.AmHost) return;
        var seedsList = new List<int>();
        var rnd = new Random();
        for (var i = 0; i < 15; i++)
        {
            seedsList.Add(rnd.Next());
        }
        ShareRandomizer(PlayerControl.LocalPlayer, string.Join("|", seedsList));
    }

    [MethodRpc((uint) CustomRpc.ShareRandomSeeds)]
    private static void ShareRandomizer(PlayerControl sender, string rawData)
    {
        seeds = rawData.Split("|").Select(int.Parse).ToList();
    }

    public static void RandomizePlayersList(MeetingHud meetingHud)
    {
        if (!CustomOptionHolder.randomizePlayersInMeeting.getBool()) return;
        var alivePlayers = meetingHud.playerStates
            .Where(area => !area.AmDead).ToList();
        alivePlayers.Sort(SortListByNames);
        var playerPositions = alivePlayers.Select(area => area.transform.localPosition).ToList();
        var playersList = alivePlayers
            .Select(ToRandomList)
            .OrderBy(ReorderList)
            .Select(GetPlayerVoteArea)
            .ToList();

        for (var i = 0; i < playersList.Count; i++)
        {
            playersList[i].transform.localPosition = playerPositions[i];
        }
    }

    private static int SortListByNames(PlayerVoteArea a, PlayerVoteArea b)
    {
        return string.CompareOrdinal(a.NameText.text, b.NameText.text);
    }

    private static RandomPlayerVoteArea ToRandomList(PlayerVoteArea pva, int index)
    {
        return new RandomPlayerVoteArea(pva, seeds[index]);
    }

    private static int ReorderList(RandomPlayerVoteArea pva)
    {
        return pva.RandomValue;
    }

    private static PlayerVoteArea GetPlayerVoteArea(RandomPlayerVoteArea pva)
    {
        return pva.Player;
    }

    private class RandomPlayerVoteArea
    {
        public readonly PlayerVoteArea Player;
        public readonly int RandomValue;

        public RandomPlayerVoteArea(PlayerVoteArea player, int randomValue)
        {
            Player = player;
            RandomValue = randomValue;
        }
    }
}