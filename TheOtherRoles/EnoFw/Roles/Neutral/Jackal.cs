using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Reactor.Networking.Attributes;
using TheOtherRoles.EnoFw.Modules;
using TheOtherRoles.EnoFw.Roles.Crewmate;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;
using TMPro;
using UnityEngine;

namespace TheOtherRoles.EnoFw.Roles.Neutral;

public static class Jackal
{
    public static PlayerControl jackal;
    public static Color color = new Color32(0, 180, 235, byte.MaxValue);
    public static PlayerControl fakeSidekick;
    public static PlayerControl currentTarget;
    public static List<PlayerControl> formerJackals = new List<PlayerControl>();

    public static float cooldown = 30f;
    public static float createSidekickCooldown = 30f;
    public static bool canUseVents = true;
    public static bool canCreateSidekick = true;
    public static Sprite buttonSprite;
    public static bool jackalPromotedFromSidekickCanCreateSidekick = true;
    public static bool canCreateSidekickFromImpostor = true;
    public static bool hasImpostorVision;
    public static bool wasTeamRed;
    public static bool wasImpostor;
    public static bool wasSpy;

    public static Sprite getSidekickButtonSprite()
    {
        if (buttonSprite) return buttonSprite;
        buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.SidekickButton.png", 115f);
        return buttonSprite;
    }

    public static void removeCurrentJackal()
    {
        if (formerJackals.All(x => x.PlayerId != jackal.PlayerId)) formerJackals.Add(jackal);
        jackal = null;
        currentTarget = null;
        fakeSidekick = null;
        cooldown = CustomOptionHolder.jackalKillCooldown.getFloat();
        createSidekickCooldown = CustomOptionHolder.jackalCreateSidekickCooldown.getFloat();
    }

    public static void clearAndReload()
    {
        jackal = null;
        currentTarget = null;
        fakeSidekick = null;
        cooldown = CustomOptionHolder.jackalKillCooldown.getFloat();
        createSidekickCooldown = CustomOptionHolder.jackalCreateSidekickCooldown.getFloat();
        canUseVents = CustomOptionHolder.jackalCanUseVents.getBool();
        canCreateSidekick = CustomOptionHolder.jackalCanCreateSidekick.getBool();
        jackalPromotedFromSidekickCanCreateSidekick =
            CustomOptionHolder.jackalPromotedFromSidekickCanCreateSidekick.getBool();
        canCreateSidekickFromImpostor = CustomOptionHolder.jackalCanCreateSidekickFromImpostor.getBool();
        formerJackals.Clear();
        hasImpostorVision = CustomOptionHolder.jackalAndSidekickHaveImpostorVision.getBool();
        wasTeamRed = wasImpostor = wasSpy = false;
    }

    public static void JackalCreatesSidekick(byte targetId)
    {
        var data = new Tuple<byte>(targetId);
        Rpc_JackalCreatesSidekick(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.JackalCreatesSidekick)]
    private static void Rpc_JackalCreatesSidekick(PlayerControl sender, string rawData)
    {
        var targetId = Rpc.Deserialize<Tuple<byte>>(rawData).Item1;
        var player = Helpers.playerById(targetId);
        if (player == null) return;
        if (Lawyer.target == player && Lawyer.isProsecutor && Lawyer.lawyer != null && !Lawyer.lawyer.Data.IsDead) Lawyer.isProsecutor = false;

        if (!canCreateSidekickFromImpostor && player.Data.Role.IsImpostor) {
            fakeSidekick = player;
        } else {
            var localWasSpy = Spy.spy != null && player == Spy.spy;
            var localWasImpostor = player.Data.Role.IsImpostor;  // This can only be reached if impostors can be sidekicked.
            FastDestroyableSingleton<RoleManager>.Instance.SetRole(player, RoleTypes.Crewmate);
            if (player == Lawyer.lawyer && Lawyer.target != null)
            {
                var playerInfoTransform = Lawyer.target.cosmetics.nameText.transform.parent.FindChild("Info");
                var playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TextMeshPro>() : null;
                if (playerInfo != null) playerInfo.text = "";
            }
            CommonRpc.Local_ErasePlayerRoles(player.PlayerId);
            Sidekick.sidekick = player;
            if (player.PlayerId == CachedPlayer.LocalPlayer.PlayerId) CachedPlayer.LocalPlayer.PlayerControl.moveable = true;
            if (localWasSpy || localWasImpostor) Sidekick.wasTeamRed = true;
            Sidekick.wasSpy = localWasSpy;
            Sidekick.wasImpostor = localWasImpostor;
            if (player == CachedPlayer.LocalPlayer.PlayerControl) SoundEffectsManager.play("jackalSidekick");
        }
        canCreateSidekick = false;
    }
}