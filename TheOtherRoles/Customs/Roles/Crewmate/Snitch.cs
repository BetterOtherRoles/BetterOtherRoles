using System.Collections.Generic;
using TheOtherRoles.Objects;
using UnityEngine;

namespace TheOtherRoles.Customs.Roles.Crewmate;

public static class Snitch
{
    public static PlayerControl snitch;
    public static Color color = new Color32(184, 251, 79, byte.MaxValue);
    public static List<Arrow> localArrows = new List<Arrow>();

    public enum InfoMode
    {
        None = 0,
        Chat = 1,
        Map = 2,
        ChatAndMap = 3,
    }

    public enum ArrowTargets
    {
        None = 0,
        Evil = 1,
        Snitch = 2,
        EvilAndSnitch = 3
    }

    public enum Targets
    {
        EvilPlayers = 0,
        Killers = 1
    }

    public static InfoMode infoMode = InfoMode.Chat;
    public static ArrowTargets arrowTargets = ArrowTargets.None;
    public static Targets targets = Targets.EvilPlayers;
    public static int taskCountForReveal = 1;

    public static bool isRevealed = false;
    public static Dictionary<byte, byte> playerRoomMap = new Dictionary<byte, byte>();
    public static TMPro.TextMeshPro text = null;
    public static bool needsUpdate = true;

    public static void clearAndReload()
    {
        if (localArrows != null)
        {
            foreach (Arrow arrow in localArrows)
                if (arrow?.arrow != null)
                    UnityEngine.Object.Destroy(arrow.arrow);
        }

        localArrows = new List<Arrow>();

        taskCountForReveal = Mathf.RoundToInt(CustomOptionHolder.snitchLeftTasksForReveal.getFloat());
        snitch = null;
        isRevealed = false;
        playerRoomMap = new Dictionary<byte, byte>();
        if (text != null) UnityEngine.Object.Destroy(text);
        text = null;
        needsUpdate = true;
        infoMode = (InfoMode)CustomOptionHolder.snitchInfoMode.getSelection();
        targets = (Targets)CustomOptionHolder.snitchTargets.getSelection();
        arrowTargets = (ArrowTargets)CustomOptionHolder.snitchArrowTargets.getSelection();
    }
}