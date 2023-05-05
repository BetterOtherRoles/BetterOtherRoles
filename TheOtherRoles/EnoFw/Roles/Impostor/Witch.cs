﻿using System;
using System.Collections.Generic;
using Reactor.Networking.Attributes;
using UnityEngine;

namespace TheOtherRoles.EnoFw.Roles.Impostor;

public static class Witch
{
    public static PlayerControl witch;
    public static Color color = Palette.ImpostorRed;

    public static List<PlayerControl> futureSpelled = new List<PlayerControl>();
    public static PlayerControl currentTarget;
    public static PlayerControl spellCastingTarget;
    public static float cooldown = 30f;
    public static float spellCastingDuration = 2f;
    public static float cooldownAddition = 10f;
    public static float currentCooldownAddition;
    public static bool canSpellAnyone;
    public static bool triggerBothCooldowns = true;
    public static bool witchVoteSavesTargets = true;

    private static Sprite buttonSprite;

    public static Sprite getButtonSprite()
    {
        if (buttonSprite) return buttonSprite;
        buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.SpellButton.png", 115f);
        return buttonSprite;
    }

    private static Sprite spelledOverlaySprite;

    public static Sprite getSpelledOverlaySprite()
    {
        if (spelledOverlaySprite) return spelledOverlaySprite;
        spelledOverlaySprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.SpellButtonMeeting.png", 225f);
        return spelledOverlaySprite;
    }


    public static void clearAndReload()
    {
        witch = null;
        futureSpelled = new List<PlayerControl>();
        currentTarget = spellCastingTarget = null;
        cooldown = CustomOptionHolder.witchCooldown.getFloat();
        cooldownAddition = CustomOptionHolder.witchAdditionalCooldown.getFloat();
        currentCooldownAddition = 0f;
        canSpellAnyone = CustomOptionHolder.witchCanSpellAnyone.getBool();
        spellCastingDuration = CustomOptionHolder.witchSpellCastingDuration.getFloat();
        triggerBothCooldowns = CustomOptionHolder.witchTriggerBothCooldowns.getBool();
        witchVoteSavesTargets = CustomOptionHolder.witchVoteSavesTargets.getBool();
    }

    public static void SetFutureSpelled(byte playerId)
    {
        var data = new Tuple<byte>(playerId);
        Rpc_SetFutureSpelled(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.SetFutureSpelled)]
    private static void Rpc_SetFutureSpelled(PlayerControl sender, string rawData)
    {
        var playerId = Rpc.Deserialize<Tuple<byte>>(rawData).Item1;
        
        PlayerControl player = Helpers.playerById(playerId);
        if (futureSpelled == null)
            futureSpelled = new List<PlayerControl>();
        if (player != null) {
            futureSpelled.Add(player);
        }
    }
}