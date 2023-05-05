using System;
using Reactor.Networking.Attributes;
using UnityEngine;

namespace TheOtherRoles.EnoFw.Roles.Crewmate;

public static class Swapper
{
    public static PlayerControl swapper;
    public static Color color = new Color32(134, 55, 86, byte.MaxValue);
    private static Sprite spriteCheck;
    public static bool canCallEmergency = false;
    public static bool canOnlySwapOthers = false;
    public static int charges;
    public static float rechargeTasksNumber;
    public static float rechargedTasks;

    public static byte playerId1 = byte.MaxValue;
    public static byte playerId2 = byte.MaxValue;

    public static Sprite getCheckSprite()
    {
        if (spriteCheck) return spriteCheck;
        spriteCheck = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.SwapperCheck.png", 150f);
        return spriteCheck;
    }

    public static void clearAndReload()
    {
        swapper = null;
        playerId1 = byte.MaxValue;
        playerId2 = byte.MaxValue;
        canCallEmergency = CustomOptionHolder.swapperCanCallEmergency.getBool();
        canOnlySwapOthers = CustomOptionHolder.swapperCanOnlySwapOthers.getBool();
        charges = Mathf.RoundToInt(CustomOptionHolder.swapperSwapsNumber.getFloat());
        rechargeTasksNumber = Mathf.RoundToInt(CustomOptionHolder.swapperRechargeTasksNumber.getFloat());
        rechargedTasks = Mathf.RoundToInt(CustomOptionHolder.swapperRechargeTasksNumber.getFloat());
    }

    public static void SwapperSwap(byte pId1, byte pId2)
    {
        var data = new Tuple<byte, byte>(pId1, pId2);
        Rpc_SwapperSwap(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.SwapperSwap)]
    private static void Rpc_SwapperSwap(PlayerControl sender, string rawData)
    {
        var (pId1, pId2) = Rpc.Deserialize<Tuple<byte, byte>>(rawData);
        if (!MeetingHud.Instance) return;
        playerId1 = pId1;
        playerId2 = pId2;
    }
}