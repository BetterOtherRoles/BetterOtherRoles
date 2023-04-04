using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Reactor.Networking.Attributes;
using TheOtherRoles.Players;
using Random = System.Random;

namespace TheOtherRoles.Modules;

public class RandomSeed
{
    public enum CustomRpc
    {
        ShareRandomSeeds,
    }
    
    private static List<int> _seeds = new ();

    public static void GenerateSeeds()
    {
        if (CachedPlayer.LocalPlayer == null || !AmongUsClient.Instance.AmHost) return;
        var seeds = new List<int>();
        var rnd = new Random();
        for (var i = 0; i < 15; i++)
        {
            seeds.Add(rnd.Next());
        }
        ShareRandomizer(PlayerControl.LocalPlayer, string.Join("|", seeds));
    }

    [MethodRpc((uint) CustomRpc.ShareRandomSeeds)]
    public static void ShareRandomizer(PlayerControl sender, string rawData)
    {
        _seeds = rawData.Split("|").Select(int.Parse).ToList();
    }

    public static void RandomizePlayersList(MeetingHud meetingHud)
    {
        if (!CustomOptionHolder.randomizePlayersInMeeting.getBool()) return;
        var alivePlayers = meetingHud.playerStates.Where(area => !area.AmDead).ToArray();
        var playerPositions = alivePlayers.Select(area => area.transform.localPosition).ToArray();
        var playersList = alivePlayers
            .Select(ToRandomList)
            .OrderBy(ReorderList)
            .Select(GetPlayerVoteArea)
            .ToArray();

        for (var i = 0; i < playersList.Length; i++)
        {
            playersList[i].transform.localPosition = playerPositions[i];
        }
    }

    private static RandomPlayerVoteArea ToRandomList(PlayerVoteArea pva, int index)
    {
        return new RandomPlayerVoteArea(pva, _seeds[index]);
    }

    private static int ReorderList(RandomPlayerVoteArea pva)
    {
        return pva.RandomValue;
    }

    private static PlayerVoteArea GetPlayerVoteArea(RandomPlayerVoteArea pva)
    {
        return pva.Player;
    }
    
    public static async Task<HttpStatusCode> FetchSeed()
    {
        var httpClient = new HttpClient();
        var response = await httpClient.GetAsync("https://eno.re/BetterOtherRoles/api/seed.txt", HttpCompletionOption.ResponseContentRead);
        if (response.StatusCode != HttpStatusCode.OK) return response.StatusCode;
        System.Console.WriteLine(response.Content.ToString());
        
        return response.StatusCode;
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