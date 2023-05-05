using System;
using Reactor.Networking.Attributes;
using TheOtherRoles.EnoFw.Roles.Crewmate;
using TheOtherRoles.Players;
using UnityEngine;

namespace TheOtherRoles.EnoFw.Roles.Neutral;

public static class Lawyer
{
    public static PlayerControl lawyer;
    public static PlayerControl target;
    public static PlayerControl formerLawyer;
    public static Color color = new Color32(134, 153, 25, byte.MaxValue);
    public static Sprite targetSprite;
    public static bool triggerProsecutorWin = false;
    public static bool isProsecutor = false;
    public static bool canCallEmergency = true;

    public static float vision = 1f;
    public static bool lawyerKnowsRole = false;
    public static bool targetCanBeJester = false;
    public static bool targetWasGuessed = false;

    public static Sprite getTargetSprite()
    {
        if (targetSprite) return targetSprite;
        targetSprite = Helpers.loadSpriteFromResources("", 150f);
        return targetSprite;
    }

    public static void clearAndReload(bool clearTarget = true)
    {
        lawyer = null;
        formerLawyer = null;
        if (clearTarget)
        {
            target = null;
            targetWasGuessed = false;
        }

        isProsecutor = false;
        triggerProsecutorWin = false;
        vision = CustomOptionHolder.lawyerVision.getFloat();
        lawyerKnowsRole = CustomOptionHolder.lawyerKnowsRole.getBool();
        targetCanBeJester = CustomOptionHolder.lawyerTargetCanBeJester.getBool();
        canCallEmergency = CustomOptionHolder.jesterCanCallEmergency.getBool();
    }

    public static void LawyerPromotesToPursuer()
    {
        Rpc_LawyerPromotesToPursuer(PlayerControl.LocalPlayer);
    }

    [MethodRpc((uint)Rpc.Role.LawyerPromotesToPursuer)]
    private static void Rpc_LawyerPromotesToPursuer(PlayerControl sender)
    {
        var player = lawyer;
        var client = target;
        clearAndReload(false);

        Pursuer.pursuer = player;

        if (player.PlayerId != CachedPlayer.LocalPlayer.PlayerId || client == null) return;
        var playerInfoTransform = client.cosmetics.nameText.transform.parent.FindChild("Info");
        var playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
        if (playerInfo != null) playerInfo.text = "";
    }

    public static void LawyerSetTarget(byte playerId)
    {
        var data = new Tuple<byte>(playerId);
        Rpc_LawyerSetTarget(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.LawyerSetTarget)]
    private static void Rpc_LawyerSetTarget(PlayerControl sender, string rawData)
    {
        var playerId = Rpc.Deserialize<Tuple<byte>>(rawData).Item1;

        target = Helpers.playerById(playerId);
    }
}