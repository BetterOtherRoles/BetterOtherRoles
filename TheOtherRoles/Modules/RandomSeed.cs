using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace TheOtherRoles.Modules;

public class RandomSeed
{
    private static int _seed = 1;

    private static Random _rnd = new(1);

    public static void ResetSeed()
    {
        _seed = 1;
        UpdateSeed();
    }

    public static void UpdateSeed()
    {
        _seed++;
        _rnd = new Random(_seed);
    }

    public static void RandomizePlayersList(MeetingHud meetingHud)
    {
        if (!CustomOptionHolder.randomizePlayersInMeeting.getBool()) return;
        var alivePlayers = meetingHud.playerStates.Where(area => !area.AmDead).ToArray();
        var playerPositions = alivePlayers.Select(area => area.transform.localPosition).ToArray();
        var playersList = alivePlayers.ToList();
        
        playersList.Sort(SortByName);
        ShuffleList(playersList);

        for (var i = 0; i < playersList.Count; i++)
        {
            playersList[i].transform.localPosition = playerPositions[i];
        }
    }

    private static void ShuffleList<T>(IList<T> values)
    {
        for (var i = values.Count - 1; i > 0; i--)
        {
            var k = _rnd.Next(i + 1);
            (values[k], values[i]) = (values[i], values[k]);
        }
    }

    private static int SortByName(PlayerVoteArea a, PlayerVoteArea b)
    {
        return string.CompareOrdinal(a.NameText.text, b.NameText.text);
    }
}