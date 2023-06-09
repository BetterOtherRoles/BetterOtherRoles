﻿using BetterOtherRoles.EnoFw.Kernel;
using BetterOtherRoles.EnoFw.Roles.Crewmate;
using BetterOtherRoles.EnoFw.Roles.Neutral;
using BetterOtherRoles.Objects;
using BetterOtherRoles.Players;
using UnityEngine;

namespace BetterOtherRoles.EnoFw.Roles.Modifiers;

public class Shifter : AbstractSimpleModifier
{
    public static readonly Shifter Instance = new();

    public PlayerControl FutureShift;
    public PlayerControl CurrentTarget;

    public static Sprite ShiftButtonSprite =>
        Helpers.loadSpriteFromResources("BetterOtherRoles.Resources.ShiftButton.png", 115f);

    private Shifter() : base(nameof(Shifter), "Shifter", Color.yellow)
    {
        AllowedTeams.Add(AbstractRole.Teams.Crewmate);
    }

    private static void ShiftRole(PlayerControl player1, PlayerControl player2, bool repeat = true)
    {
        if (Mayor.Instance.HasPlayer && Mayor.Instance.Player == player2)
        {
            if (repeat) ShiftRole(player2, player1, false);
            Mayor.Instance.Player = player1;
        }
        else if (Portalmaker.Instance.Player != null && Portalmaker.Instance.Player == player2)
        {
            if (repeat) ShiftRole(player2, player1, false);
            Portalmaker.Instance.Player = player1;
        }
        else if (Engineer.Instance.HasPlayer && Engineer.Instance.Player == player2)
        {
            if (repeat) ShiftRole(player2, player1, false);
            Engineer.Instance.Player = player1;
        }
        else if (Sheriff.Instance.Player != null && Sheriff.Instance.Player == player2)
        {
            if (repeat) ShiftRole(player2, player1, false);
            if (Sheriff.Instance.FormerDeputy != null && Sheriff.Instance.FormerDeputy == Sheriff.Instance.Player)
                Sheriff.Instance.FormerDeputy = player1; // Shifter also shifts info on promoted deputy (to get handcuffs)
            Sheriff.Instance.Player = player1;
        }
        else if (Deputy.Instance.Player != null && Deputy.Instance.Player == player2)
        {
            if (repeat) ShiftRole(player2, player1, false);
            Deputy.Instance.Player = player1;
        }
        else if (Lighter.Instance.HasPlayer && Lighter.Instance.Player == player2)
        {
            if (repeat) ShiftRole(player2, player1, false);
            Lighter.Instance.Player = player1;
        }
        else if (Detective.Instance.HasPlayer && Detective.Instance.Player == player2)
        {
            if (repeat) ShiftRole(player2, player1, false);
            Detective.Instance.Player = player1;
        }
        else if (TimeMaster.Instance.Player != null && TimeMaster.Instance.Player == player2)
        {
            if (repeat) ShiftRole(player2, player1, false);
            TimeMaster.Instance.Player = player1;
        }
        else if (Medic.Instance.Player != null && Medic.Instance.Player == player2)
        {
            if (repeat) ShiftRole(player2, player1, false);
            Medic.Instance.Player = player1;
        }
        else if (Swapper.Instance.Player != null && Swapper.Instance.Player == player2)
        {
            if (repeat) ShiftRole(player2, player1, false);
            Swapper.Instance.Player = player1;
        }
        else if (Seer.Instance.Player != null && Seer.Instance.Player == player2)
        {
            if (repeat) ShiftRole(player2, player1, false);
            Seer.Instance.Player = player1;
        }
        else if (Hacker.Instance.HasPlayer && Hacker.Instance.Player == player2)
        {
            if (repeat) ShiftRole(player2, player1, false);
            Hacker.Instance.Player = player1;
        }
        else if (Tracker.Instance.Player != null && Tracker.Instance.Player == player2)
        {
            if (repeat) ShiftRole(player2, player1, false);
            Tracker.Instance.Player = player1;
        }
        else if (Snitch.Instance.Player != null && Snitch.Instance.Player == player2)
        {
            if (repeat) ShiftRole(player2, player1, false);
            Snitch.Instance.Player = player1;
        }
        else if (Spy.Instance.Player != null && Spy.Instance.Player == player2)
        {
            if (repeat) ShiftRole(player2, player1, false);
            Spy.Instance.Player = player1;
        }
        else if (SecurityGuard.Instance.HasPlayer && SecurityGuard.Instance.Player == player2)
        {
            if (repeat) ShiftRole(player2, player1, false);
            SecurityGuard.Instance.Player = player1;
        }
        else if (NiceGuesser.Instance.HasPlayer && NiceGuesser.Instance.Player == player2)
        {
            if (repeat) ShiftRole(player2, player1, false);
            NiceGuesser.Instance.Player = player1;
        }
        else if (Medium.Instance.HasPlayer && Medium.Instance.Player == player2)
        {
            if (repeat) ShiftRole(player2, player1, false);
            Medium.Instance.Player = player1;
        }
        else if (Pursuer.Instance.Player != null && Pursuer.Instance.Player == player2)
        {
            if (repeat) ShiftRole(player2, player1, false);
            Pursuer.Instance.Player = player1;
        }
        else if (Trapper.Instance.Player != null && Trapper.Instance.Player == player2)
        {
            if (repeat) ShiftRole(player2, player1, false);
            Trapper.Instance.Player = player1;
        }
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        CurrentTarget = null;
        FutureShift = null;
    }

    public static void SetFutureShifted(byte playerId)
    {
        RpcManager.Instance.Send((uint)Rpc.Role.SetFutureShifted, playerId);
    }

    [BindRpc((uint)Rpc.Role.SetFutureShifted)]
    public static void Rpc_SetFutureShifted(byte playerId)
    {
        Instance.FutureShift = Helpers.playerById(playerId);
    }

    public static void ShifterShift(byte targetId)
    {
        RpcManager.Instance.Send((uint)Rpc.Role.ShifterShift, targetId);
    }

    [BindRpc((uint)Rpc.Role.ShifterShift)]
    public static void Rpc_ShifterShift(byte targetId)
    {
        var oldShifter = Instance.Player;
        var player = Helpers.playerById(targetId);
        if (player == null || oldShifter == null) return;

        Instance.FutureShift = null;
        Instance.ClearAndReload();

        // Suicide (exile) when impostor or impostor variants
        if (player.Data.Role.IsImpostor || Helpers.isNeutral(player)) {
            oldShifter.Exiled();
            if (oldShifter != Lawyer.Instance.Target || !AmongUsClient.Instance.AmHost || Lawyer.Instance.Player == null) return;
            Lawyer.LawyerPromotesToPursuer();
            return;
        }
            
        ShiftRole(oldShifter, player);

        // Set cooldowns to max for both players
        if (CachedPlayer.LocalPlayer.PlayerControl == oldShifter || CachedPlayer.LocalPlayer.PlayerControl == player)
            CustomButton.ResetAllCooldowns();
    }
}