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
        var alivePlayers = meetingHud.playerStates.Where(area => !area.AmDead).ToArray();
        var playerPositions = alivePlayers.Select(area => area.transform.localPosition).ToArray();
        var playerVoteAreas = alivePlayers
            .OrderBy(p => p.AmDead ? int.MaxValue : _rnd.Next())
            .ToArray();
            
        for (var i = 0; i < playerVoteAreas.Length; i++)
        {
            System.Console.WriteLine($"{i} - {playerVoteAreas[i].NameText.text}");
            playerVoteAreas[i].transform.localPosition = playerPositions[i];
        }
    }
}