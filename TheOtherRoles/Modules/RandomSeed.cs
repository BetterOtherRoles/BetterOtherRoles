using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace TheOtherRoles.Modules;

public class RandomSeed
{
    private static int _seed = 1;

    public static Random Rnd = new Random(1);

    public static void UpdateSeed()
    {
        _seed++;
        Rnd = new Random(_seed);
    }
    
    public class PlayerMeetingData
    {
        public static List<PlayerMeetingData> AllPlayerMeetingData = new ();

        public static void SetPlayerMeetingData(MeetingHud meetingHud)
        {
            /*
            AllPlayerMeetingData.Clear();
            for (var i = 0; i < meetingHud.playerStates.Length; i++)
            {
                AllPlayerMeetingData.Add(new PlayerMeetingData(meetingHud.playerStates[i], meetingHud.PlayerColoredParts[i], RandomSeed.Rnd.Next()));
            }
            */
        }
            
        public static void RandomizePlayersList(MeetingHud meetingHud)
        {
            /*
            meetingHud.playerStates = PlayerMeetingData.AllPlayerMeetingData.OrderBy(p => p.random)
                .Select(p => p.voteArea)
                .ToArray();
            meetingHud.PlayerColoredParts = PlayerMeetingData.AllPlayerMeetingData.OrderBy(p => p.random)
                .Select(p => p.sprite)
                .ToArray();
            */
        }
            
        public readonly PlayerVoteArea voteArea;
        public readonly SpriteRenderer sprite;
        public readonly int random;

        public PlayerMeetingData(PlayerVoteArea va, SpriteRenderer sr, int r)
        {
            voteArea = va;
            sprite = sr;
            random = r;
        }
    }
}