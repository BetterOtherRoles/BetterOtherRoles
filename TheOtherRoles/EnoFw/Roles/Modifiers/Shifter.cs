﻿using System;
using Hazel;
using Reactor.Networking.Attributes;
using TheOtherRoles.EnoFw.Roles.Crewmate;
using TheOtherRoles.EnoFw.Roles.Neutral;
using TheOtherRoles.Objects;
using TheOtherRoles.Players;
using UnityEngine;

namespace TheOtherRoles.EnoFw.Roles.Modifiers;

public static class Shifter
{
    public static PlayerControl shifter;

    public static PlayerControl futureShift;
    public static PlayerControl currentTarget;

    private static Sprite buttonSprite;

    public static Sprite getButtonSprite()
    {
        if (buttonSprite) return buttonSprite;
        buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.ShiftButton.png", 115f);
        return buttonSprite;
    }

    public static void shiftRole(PlayerControl player1, PlayerControl player2, bool repeat = true)
    {
        if (Mayor.mayor != null && Mayor.mayor == player2)
        {
            if (repeat) shiftRole(player2, player1, false);
            Mayor.mayor = player1;
        }
        else if (Portalmaker.portalmaker != null && Portalmaker.portalmaker == player2)
        {
            if (repeat) shiftRole(player2, player1, false);
            Portalmaker.portalmaker = player1;
        }
        else if (Engineer.engineer != null && Engineer.engineer == player2)
        {
            if (repeat) shiftRole(player2, player1, false);
            Engineer.engineer = player1;
        }
        else if (Sheriff.sheriff != null && Sheriff.sheriff == player2)
        {
            if (repeat) shiftRole(player2, player1, false);
            if (Sheriff.formerDeputy != null && Sheriff.formerDeputy == Sheriff.sheriff)
                Sheriff.formerDeputy = player1; // Shifter also shifts info on promoted deputy (to get handcuffs)
            Sheriff.sheriff = player1;
        }
        else if (Deputy.Player != null && Deputy.Player == player2)
        {
            if (repeat) shiftRole(player2, player1, false);
            Deputy.Player = player1;
        }
        else if (Lighter.lighter != null && Lighter.lighter == player2)
        {
            if (repeat) shiftRole(player2, player1, false);
            Lighter.lighter = player1;
        }
        else if (Detective.detective != null && Detective.detective == player2)
        {
            if (repeat) shiftRole(player2, player1, false);
            Detective.detective = player1;
        }
        else if (TimeMaster.timeMaster != null && TimeMaster.timeMaster == player2)
        {
            if (repeat) shiftRole(player2, player1, false);
            TimeMaster.timeMaster = player1;
        }
        else if (Medic.medic != null && Medic.medic == player2)
        {
            if (repeat) shiftRole(player2, player1, false);
            Medic.medic = player1;
        }
        else if (Swapper.swapper != null && Swapper.swapper == player2)
        {
            if (repeat) shiftRole(player2, player1, false);
            Swapper.swapper = player1;
        }
        else if (Seer.seer != null && Seer.seer == player2)
        {
            if (repeat) shiftRole(player2, player1, false);
            Seer.seer = player1;
        }
        else if (Hacker.hacker != null && Hacker.hacker == player2)
        {
            if (repeat) shiftRole(player2, player1, false);
            Hacker.hacker = player1;
        }
        else if (Tracker.tracker != null && Tracker.tracker == player2)
        {
            if (repeat) shiftRole(player2, player1, false);
            Tracker.tracker = player1;
        }
        else if (Snitch.snitch != null && Snitch.snitch == player2)
        {
            if (repeat) shiftRole(player2, player1, false);
            Snitch.snitch = player1;
        }
        else if (Spy.spy != null && Spy.spy == player2)
        {
            if (repeat) shiftRole(player2, player1, false);
            Spy.spy = player1;
        }
        else if (SecurityGuard.securityGuard != null && SecurityGuard.securityGuard == player2)
        {
            if (repeat) shiftRole(player2, player1, false);
            SecurityGuard.securityGuard = player1;
        }
        else if (Guesser.niceGuesser != null && Guesser.niceGuesser == player2)
        {
            if (repeat) shiftRole(player2, player1, false);
            Guesser.niceGuesser = player1;
        }
        else if (Medium.medium != null && Medium.medium == player2)
        {
            if (repeat) shiftRole(player2, player1, false);
            Medium.medium = player1;
        }
        else if (Pursuer.pursuer != null && Pursuer.pursuer == player2)
        {
            if (repeat) shiftRole(player2, player1, false);
            Pursuer.pursuer = player1;
        }
        else if (Trapper.trapper != null && Trapper.trapper == player2)
        {
            if (repeat) shiftRole(player2, player1, false);
            Trapper.trapper = player1;
        }
    }

    public static void clearAndReload()
    {
        shifter = null;
        currentTarget = null;
        futureShift = null;
    }

    public static void SetFutureShifted(byte playerId)
    {
        var data = new Tuple<byte>(playerId);
        Rpc_SetFutureShifted(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.SetFutureShifted)]
    private static void Rpc_SetFutureShifted(PlayerControl sender, string rawData)
    {
        var playerId = Rpc.Deserialize<Tuple<byte>>(rawData).Item1;
        futureShift = Helpers.playerById(playerId);
    }

    public static void ShifterShift(byte targetId)
    {
        var data = new Tuple<byte>(targetId);
        Rpc_ShifterShift(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.ShifterShift)]
    private static void Rpc_ShifterShift(PlayerControl sender, string rawData)
    {
        var targetId = Rpc.Deserialize<Tuple<byte>>(rawData).Item1;
        var oldShifter = shifter;
        var player = Helpers.playerById(targetId);
        if (player == null || oldShifter == null) return;

        futureShift = null;
        clearAndReload();

        // Suicide (exile) when impostor or impostor variants
        if (player.Data.Role.IsImpostor || Helpers.isNeutral(player)) {
            oldShifter.Exiled();
            if (oldShifter != Lawyer.target || !AmongUsClient.Instance.AmHost || Lawyer.lawyer == null) return;
            Lawyer.LawyerPromotesToPursuer();
            return;
        }
            
        shiftRole(oldShifter, player);

        // Set cooldowns to max for both players
        if (CachedPlayer.LocalPlayer.PlayerControl == oldShifter || CachedPlayer.LocalPlayer.PlayerControl == player)
            CustomButton.ResetAllCooldowns();
    }
}