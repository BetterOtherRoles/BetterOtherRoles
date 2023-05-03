using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using Reactor.Networking.Attributes;
using TheOtherRoles.Players;
using UnityEngine;
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

    public static void RandomizeUploadLocation(List<GameObject> downloads, GameObject upload)
    {
        var randomizedDownloads = downloads
            .OrderBy(o => TheOtherRoles.rnd.Next())
            .ToList();
        if (TheOtherRoles.rnd.Next(randomizedDownloads.Count + 1) == 1) return;
        var randomDownload = randomizedDownloads[0];
        var randomUpload = upload;
        var downloadSprite = randomDownload.GetComponent<SpriteRenderer>().sprite;
        var uploadSprite = randomUpload.GetComponent<SpriteRenderer>().sprite;
        var downloadPosition = randomDownload.transform.position;
        var uploadPosition = randomUpload.transform.position;
        randomDownload.transform.position = uploadPosition;
        randomUpload.transform.position = downloadPosition;
        randomDownload.GetComponent<SpriteRenderer>().sprite = uploadSprite;
        randomUpload.GetComponent<SpriteRenderer>().sprite = downloadSprite;
    }

    public static void RandomizePositions(List<GameObject> gameObjects)
    {
        var positions = gameObjects
            .Select(o => o.transform.position)
            .ToList();
        var sprites = gameObjects
            .Select(o => o.GetComponent<SpriteRenderer>().sprite)
            .ToList();
        var randomizedList = gameObjects
            .OrderBy(_ => TheOtherRoles.rnd.Next())
            .ToList();
        for (var i = 0; i < randomizedList.Count; i++)
        {
            randomizedList[i].transform.position = positions[i];
            randomizedList[i].GetComponent<SpriteRenderer>().sprite = sprites[i];
        }
    }
    
    public static void RandomizeLocalPositions(List<GameObject> gameObjects)
    {
        var positions = gameObjects
            .Select(o => o.transform.localPosition)
            .ToList();
        var randomizedList = gameObjects
            .OrderBy(o => TheOtherRoles.rnd.Next())
            .ToList();
        for (var i = 0; i < randomizedList.Count; i++)
        {
            randomizedList[i].transform.localPosition = positions[i];
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
    
    [HarmonyPatch(typeof(MedScanMinigame._WalkToPad_d__16), nameof(MedScanMinigame._WalkToPad_d__16.MoveNext))]
    public static class MedScanMinigameWalkToPad_Patch
    {
        public static bool Prefix(MedScanMinigame._WalkToPad_d__16 __instance)
        {
            if (!CustomOptionHolder.randomizePositionDuringScan.getBool()) return true;
            var minigame = __instance.__4__this;
            minigame.StartCoroutine(FixMedbayScanPosition(minigame));
            return false;
        }
    }

    private static IEnumerator FixMedbayScanPosition(MedScanMinigame minigame)
    {
        var panel = UnityEngine.Object.FindObjectsOfType<GameObject>().ToList()
            .Find(o => o.name == "panel_medplatform");

        if (panel == null || Camera.main == null) yield break;
                
        var panelSize = panel.GetComponent<SpriteRenderer>().bounds.size * 0.3f;
            
        minigame.state = MedScanMinigame.PositionState.WalkingToPad;
        var myPhysics = PlayerControl.LocalPlayer.MyPhysics;
            
        Vector2 worldPos = ShipStatus.Instance.MedScanner.Position;
        worldPos += new Vector2(UnityEngine.Random.Range(-panelSize.x, panelSize.x), UnityEngine.Random.Range(-panelSize.y, panelSize.y));
        
        Camera.main.GetComponent<FollowerCamera>().Locked = false;
        yield return myPhysics.WalkPlayerTo(worldPos, 0.001f, 1f);
        yield return new WaitForSeconds(0.1f);
        Camera.main.GetComponent<FollowerCamera>().Locked = true;
        minigame.walking = null;
    }
}