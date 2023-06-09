﻿using System;
using System.Collections.Generic;
using BetterOtherRoles.EnoFw.Kernel;
using BetterOtherRoles.EnoFw.Modules;
using BetterOtherRoles.EnoFw.Utils;
using BetterOtherRoles.Players;
using UnityEngine;
using Option = BetterOtherRoles.EnoFw.Kernel.CustomOption;

namespace BetterOtherRoles.EnoFw.Roles.Crewmate;

public class Deputy : AbstractRole
{
    public static readonly Deputy Instance = new();

    // Fields
    public readonly List<byte> HandcuffedPlayers = new();
    public readonly Dictionary<byte, float> HandcuffedKnows = new();
    public int UsedHandcuff;
    public int RemainingHandcuffs => NumberOfHandcuffs - UsedHandcuff;
    public Promotions Promotion => (Promotions)(int)PromotedWhen;
    public string IntroTextForSheriff => Colors.Cs(Color, $"Your Deputy is {(Player != null ? Player.Data.PlayerName : "unknown")}");

    // Options
    public readonly CustomOption PromotedWhen;
    public readonly CustomOption KeepsHandcuffsOnPromotion;
    public readonly CustomOption HandcuffDuration;
    public readonly CustomOption NumberOfHandcuffs;
    public readonly CustomOption HandcuffCooldown;
    public readonly CustomOption KnowsSheriff;

    public static Sprite HandcuffButtonSprite => GetSprite("BetterOtherRoles.Resources.DeputyHandcuffButton.png", 115f);
    public static Sprite HandcuffedButtonSprite => GetSprite("BetterOtherRoles.Resources.DeputyHandcuffed.png", 115f);

    private Deputy() : base(nameof(Deputy), "Deputy")
    {
        Team = Teams.Crewmate;
        Color = new Color32(248, 205, 70, byte.MaxValue);
        CanTarget = true;

        SpawnRate = Sheriff.Instance.DeputySpawnRate;
        
        PromotedWhen = Tab.CreateStringList(
            $"{Key}{nameof(PromotedWhen)}",
            Cs("Gets promoted to Sheriff"),
            new List<string> { "no", "yes (immediately)", "yes (after meeting)" },
            "no",
            SpawnRate);
        KeepsHandcuffsOnPromotion = Tab.CreateBool(
            $"{Key}{nameof(KeepsHandcuffsOnPromotion)}",
            Cs("Keeps handcuffs on promotion"),
            true,
            PromotedWhen);
        NumberOfHandcuffs = Tab.CreateFloatList(
            $"{Key}{nameof(NumberOfHandcuffs)}",
            Cs("Number of handcuffs"),
            0f,
            15f,
            3f,
            1f,
            SpawnRate);
        HandcuffCooldown = Tab.CreateFloatList(
            $"{Key}{nameof(HandcuffCooldown)}",
            Cs("Handcuff cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            NumberOfHandcuffs,
            string.Empty,
            "s");
        HandcuffDuration = Tab.CreateFloatList(
            $"{Key}{nameof(HandcuffDuration)}",
            Cs("Handcuff duration"),
            2f,
            60f,
            10f,
            1f,
            NumberOfHandcuffs,
            string.Empty,
            "s");
        KnowsSheriff = Tab.CreateBool(
            $"{Key}{nameof(KnowsSheriff)}",
            Cs("Sheriff and Deputy know each other"),
            true,
            SpawnRate);
    }

    public enum Promotions
    {
        No = 0,
        Immediately,
        AfterMeeting
    }

    // Can be used to enable / disable the handcuff effect on the target's buttons
    public void SetHandcuffedKnows(bool active = true, byte playerId = byte.MaxValue)
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
            HandcuffedKnows.Add(playerId, HandcuffDuration);
            HandcuffedPlayers.RemoveAll(x => x == playerId);
        }

        if (playerId == CachedPlayer.LocalPlayer.PlayerId)
        {
            HudManagerStartPatch.setAllButtonsHandcuffedStatus(active);
            SoundEffectsManager.play("deputyHandcuff");
        }
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        HandcuffedPlayers.Clear();
        HandcuffedKnows.Clear();
        UsedHandcuff = 0;
        HudManagerStartPatch.setAllButtonsHandcuffedStatus(false, true);
    }

    public static void DeputyPromotes()
    {
        RpcManager.Instance.Send((uint)Rpc.Role.DeputyPromotes);
    }

    [BindRpc((uint)Rpc.Role.DeputyPromotes)]
    public static void Rpc_DeputyPromotes()
    {
        if (Instance.Player == null) return;
        Sheriff.Instance.ReplaceCurrentSheriff(Instance.Player);
        Sheriff.Instance.FormerDeputy = Instance.Player;
        Instance.Player = null;
    }

    public static void DeputyUsedHandcuffs(byte targetId)
    {
        RpcManager.Instance.Send((uint)Rpc.Role.DeputyUsedHandcuffs, targetId);
    }

    [BindRpc((uint)Rpc.Role.DeputyUsedHandcuffs)]
    public static void Rpc_DeputyUsedHandcuffs(byte targetId)
    {
        Instance.UsedHandcuff++;
        Instance.HandcuffedPlayers.Add(targetId);
    }
}