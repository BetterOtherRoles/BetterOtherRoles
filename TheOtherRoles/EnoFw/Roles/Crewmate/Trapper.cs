using System;
using System.Collections.Generic;
using Reactor.Networking.Attributes;
using TheOtherRoles.Objects;
using UnityEngine;

namespace TheOtherRoles.EnoFw.Roles.Crewmate;

public static class Trapper {
    public static PlayerControl trapper;
    public static Color color = new Color32(110, 57, 105, byte.MaxValue);

    public static float cooldown = 30f;
    public static int maxCharges = 5;
    public static int rechargeTasksNumber = 3;
    public static int rechargedTasks = 3;
    public static int charges = 1;
    public static int trapCountToReveal = 2;
    public static List<PlayerControl> playersOnMap = new List<PlayerControl>();
    public static bool anonymousMap = false;
    public static int infoType = 0; // 0 = Role, 1 = Good/Evil, 2 = Name
    public static float trapDuration = 5f; 

    private static Sprite trapButtonSprite;

    public static Sprite getButtonSprite() {
        if (trapButtonSprite) return trapButtonSprite;
        trapButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Trapper_Place_Button.png", 115f);
        return trapButtonSprite;
    }

    public static void clearAndReload() {
        trapper = null;
        cooldown = CustomOptionHolder.trapperCooldown.getFloat();
        maxCharges = Mathf.RoundToInt(CustomOptionHolder.trapperMaxCharges.getFloat());
        rechargeTasksNumber = Mathf.RoundToInt(CustomOptionHolder.trapperRechargeTasksNumber.getFloat());
        rechargedTasks = Mathf.RoundToInt(CustomOptionHolder.trapperRechargeTasksNumber.getFloat());
        charges = Mathf.RoundToInt(CustomOptionHolder.trapperMaxCharges.getFloat()) / 2;
        trapCountToReveal = Mathf.RoundToInt(CustomOptionHolder.trapperTrapNeededTriggerToReveal.getFloat());
        playersOnMap = new List<PlayerControl>();
        anonymousMap = CustomOptionHolder.trapperAnonymousMap.getBool();
        infoType = CustomOptionHolder.trapperInfoType.getSelection();
        trapDuration = CustomOptionHolder.trapperTrapDuration.getFloat();
    }

    public static void TriggerTrap(byte playerId, byte trapId)
    {
        var data = new Tuple<byte, byte>(playerId, trapId);
        Rpc_TriggerTrap(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.TriggerTrap)]
    private static void Rpc_TriggerTrap(PlayerControl sender, string rawData)
    {
        var (playerId, trapId) = Rpc.Deserialize<Tuple<byte, byte>>(rawData);
        Trap.triggerTrap(playerId, trapId);
    }

    public static void SetTrap(float x, float y)
    {
        var data = new Tuple<float, float>(x, y);
        Rpc_SetTrap(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.SetTrap)]
    private static void Rpc_SetTrap(PlayerControl sender, string rawData)
    {
        if (trapper == null) return;
        var (x, y) = Rpc.Deserialize<Tuple<float, float>>(rawData);
        charges--;
        var position = Vector3.zero;
        position.x = x;
        position.y = y;
        var _ = new Trap(position);
    }
}