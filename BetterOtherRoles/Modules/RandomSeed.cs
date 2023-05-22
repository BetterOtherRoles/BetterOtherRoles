using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils;
using BetterOtherRoles.EnoFw;
using BetterOtherRoles.Players;
using HarmonyLib;
using BetterOtherRoles.EnoFw.Kernel;
using UnityEngine;
using Random = System.Random;

namespace BetterOtherRoles.Modules;

public static class RandomSeed
{
    private static Random _random = new();

    public static void GenerateSeed()
    {
        if (CachedPlayer.LocalPlayer == null || !AmongUsClient.Instance.AmHost) return;
        var seed = new Random().Next();
        ShareRandomSeed(seed);
    }

    private static void ShareRandomSeed(int seed)
    {
        RpcManager.Instance.Send((uint) Rpc.Module.ShareRandomSeed, seed);
    }

    [BindRpc((uint) Rpc.Module.ShareRandomSeed)]
    public static void Rpc_ShareRandomSeed(int seed)
    {
        _random = new Random(seed);
    }

    public static void RandomizePlayersList(MeetingHud meetingHud)
    {
        if (!CustomOptions.RandomizeMeetingOrder) return;
        var alivePlayers = meetingHud.playerStates
            .Where(area => !area.AmDead).ToList();
        alivePlayers.Sort(SortListByNames);
        var playerPositions = alivePlayers.Select(area => area.transform.localPosition).ToList();
        var playersList = alivePlayers
            .OrderBy(_ => _random.Next())
            .ToList();

        for (var i = 0; i < playersList.Count; i++)
        {
            playersList[i].transform.localPosition = playerPositions[i];
        }
    }

    public static void RandomizeUploadLocation(List<GameObject> downloads, GameObject upload)
    {
        if (RolesManager.Rnd.Next(downloads.Count + 1) == 1) return;
        // var download = downloads.Find(d => d.GetComponent<Console>().Room == SystemTypes.Specimens);
        var download = downloads[RolesManager.Rnd.Next(downloads.Count)];
        ExchangeTaskPositions(upload, download);
    }

    private static void ExchangeTaskPositions(GameObject task1, GameObject task2)
    {
        var position1 = task1.transform.position;
        var position2 = task2.transform.position;

        var sprite1 = task1.GetComponent<SpriteRenderer>().sprite;
        var sprite2 = task2.GetComponent<SpriteRenderer>().sprite;

        var size1 = task1.GetComponent<BoxCollider2D>().size;
        var size2 = task2.GetComponent<BoxCollider2D>().size;

        var console1 = task1.GetComponent<Console>();
        var console2 = task2.GetComponent<Console>();

        var usableDistance1 = console1.UsableDistance;
        var usableDistance2 = console2.UsableDistance;

        task1.transform.position = position2;
        task2.transform.position = position1;

        task1.GetComponent<SpriteRenderer>().sprite = sprite2;
        task2.GetComponent<SpriteRenderer>().sprite = sprite1;

        task1.GetComponent<BoxCollider2D>().size = size2;
        task2.GetComponent<BoxCollider2D>().size = size1;

        if (console2.onlySameRoom)
        {
            console1.checkWalls = true;
        }

        console1.onlySameRoom = false;
        console2.onlySameRoom = false;

        console1.usableDistance = usableDistance2;
        console2.usableDistance = usableDistance1;
        
        BetterOtherRolesPlugin.Logger.LogDebug($"Upload task moved to {console2.Room}");
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
            .OrderBy(_ => RolesManager.Rnd.Next())
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
            .OrderBy(o => RolesManager.Rnd.Next())
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

    [HarmonyPatch(typeof(MedScanMinigame._WalkToPad_d__16), nameof(MedScanMinigame._WalkToPad_d__16.MoveNext))]
    public static class MedScanMinigameWalkToPad_Patch
    {
        public static bool Prefix(MedScanMinigame._WalkToPad_d__16 __instance)
        {
            if (!CustomOptions.RandomizeMeetingOrder) return true;
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