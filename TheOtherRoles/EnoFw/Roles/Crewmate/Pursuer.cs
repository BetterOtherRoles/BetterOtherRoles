using System;
using System.Collections.Generic;
using Reactor.Networking.Attributes;
using TheOtherRoles.EnoFw.Roles.Neutral;
using UnityEngine;

namespace TheOtherRoles.EnoFw.Roles.Crewmate;

public static class Pursuer
{
    public static PlayerControl pursuer;
    public static PlayerControl target;
    public static Color color = Lawyer.color;
    public static List<PlayerControl> blankedList = new List<PlayerControl>();
    public static int blanks = 0;
    public static Sprite blank;
    public static bool notAckedExiled = false;

    public static float cooldown = 30f;
    public static int blanksNumber = 5;

    public static Sprite getTargetSprite()
    {
        if (blank) return blank;
        blank = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.PursuerButton.png", 115f);
        return blank;
    }

    public static void clearAndReload()
    {
        pursuer = null;
        target = null;
        blankedList = new List<PlayerControl>();
        blanks = 0;
        notAckedExiled = false;

        cooldown = CustomOptionHolder.pursuerCooldown.getFloat();
        blanksNumber = Mathf.RoundToInt(CustomOptionHolder.pursuerBlanksNumber.getFloat());
    }

    public static void SetBlanked(byte playerId, bool add)
    {
        var data = new Tuple<byte, bool>(playerId, add);
        Rpc_SetBlanked(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.SetBlanked)]
    private static void Rpc_SetBlanked(PlayerControl sender, string rawData)
    {
        var (playerId, add) = Rpc.Deserialize<Tuple<byte, bool>>(rawData);
        
        var blankTarget = Helpers.playerById(playerId);
        if (target == null) return;
        blankedList.RemoveAll(x => x.PlayerId == playerId);
        if (add) blankedList.Add(blankTarget);     
    }
}