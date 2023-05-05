using System;
using System.Collections.Generic;
using Hazel;
using Reactor.Networking.Attributes;
using TheOtherRoles.EnoFw.Modules;
using TheOtherRoles.Players;
using UnityEngine;

namespace TheOtherRoles.EnoFw.Roles.Crewmate;

public static class Deputy
{
    public static PlayerControl Player;
    public static Color Color = Sheriff.color;

    public static PlayerControl currentTarget;
    public static List<byte> handcuffedPlayers = new List<byte>();
    public static int promotesToSheriff; // No: 0, Immediately: 1, After Meeting: 2
    public static bool keepsHandcuffsOnPromotion;
    public static float handcuffDuration;
    public static float remainingHandcuffs;
    public static float handcuffCooldown;
    public static bool knowsSheriff;
    public static Dictionary<byte, float> handcuffedKnows = new Dictionary<byte, float>();

    private static Sprite buttonSprite;
    private static Sprite handcuffedSprite;

    public static Sprite getButtonSprite()
    {
        if (buttonSprite) return buttonSprite;
        buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.DeputyHandcuffButton.png", 115f);
        return buttonSprite;
    }

    public static Sprite getHandcuffedButtonSprite()
    {
        if (handcuffedSprite) return handcuffedSprite;
        handcuffedSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.DeputyHandcuffed.png", 115f);
        return handcuffedSprite;
    }

    // Can be used to enable / disable the handcuff effect on the target's buttons
    public static void setHandcuffedKnows(bool active = true, byte playerId = byte.MaxValue)
    {
        if (playerId == byte.MaxValue)
            playerId = CachedPlayer.LocalPlayer.PlayerId;

        if (active && playerId == CachedPlayer.LocalPlayer.PlayerId)
        {
            GhostInfos.ShareGhostInfo(GhostInfos.Types.HandcuffNoticed,
                Rpc.Serialize(new Tuple<byte>(CachedPlayer.LocalPlayer.PlayerId)));
        }

        if (active)
        {
            handcuffedKnows.Add(playerId, handcuffDuration);
            handcuffedPlayers.RemoveAll(x => x == playerId);
        }

        if (playerId == CachedPlayer.LocalPlayer.PlayerId)
        {
            HudManagerStartPatch.setAllButtonsHandcuffedStatus(active);
            SoundEffectsManager.play("deputyHandcuff");
        }
    }

    public static void ClearAndReload()
    {
        Player = null;
        currentTarget = null;
        handcuffedPlayers = new List<byte>();
        handcuffedKnows = new Dictionary<byte, float>();
        HudManagerStartPatch.setAllButtonsHandcuffedStatus(false, true);
        promotesToSheriff = CustomOptionHolder.deputyGetsPromoted.getSelection();
        remainingHandcuffs = CustomOptionHolder.deputyNumberOfHandcuffs.getFloat();
        handcuffCooldown = CustomOptionHolder.deputyHandcuffCooldown.getFloat();
        keepsHandcuffsOnPromotion = CustomOptionHolder.deputyKeepsHandcuffs.getBool();
        handcuffDuration = CustomOptionHolder.deputyHandcuffDuration.getFloat();
        knowsSheriff = CustomOptionHolder.deputyKnowsSheriff.getBool();
    }

    public static void DeputyPromotes()
    {
        Rpc_DeputyPromotes(PlayerControl.LocalPlayer);
    }

    [MethodRpc((uint)Rpc.Role.DeputyPromotes)]
    private static void Rpc_DeputyPromotes(PlayerControl sender)
    {
        if (Player == null) return;
        Sheriff.replaceCurrentSheriff(Player);
        Sheriff.formerDeputy = Player;
        Player = null;
    }

    public static void DeputyUsedHandcuffs(byte targetId)
    {
        var data = new Tuple<byte>(targetId);
        Rpc_DeputyUsedHandcuffs(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.DeputyUsedHandcuffs)]
    private static void Rpc_DeputyUsedHandcuffs(PlayerControl sender, string rawData)
    {
        var targetId = Rpc.Deserialize<Tuple<byte>>(rawData).Item1;
        remainingHandcuffs--;
        handcuffedPlayers.Add(targetId);
    }
}