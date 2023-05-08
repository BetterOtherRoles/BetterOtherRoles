using System;
using System.Collections.Generic;
using Reactor.Networking.Attributes;
using TheOtherRoles.EnoFw.Kernel;
using TheOtherRoles.EnoFw.Modules;
using TheOtherRoles.Players;
using UnityEngine;
using Option = TheOtherRoles.EnoFw.Kernel.CustomOption;

namespace TheOtherRoles.EnoFw.Roles.Crewmate;

public class Deputy : AbstractRole
{
    public static readonly Deputy Instance = new();

    // Fields
    public readonly List<byte> HandcuffedPlayers = new();
    public readonly Dictionary<byte, float> HandcuffedKnows = new();
    public int UsedHandcuff;
    public int RemainingHandcuffs => NumberOfHandcuffs - UsedHandcuff;
    public Promotions Promotion => (Promotions)(int)PromotedWhen;
    public string IntroTextForSheriff => Cs($"Your Deputy is {(Player != null ? Player.Data.PlayerName : "unknown")}");

    // Options
    public readonly Option PromotedWhen;
    public readonly Option KeepsHandcuffsOnPromotion;
    public readonly Option HandcuffDuration;
    public readonly Option NumberOfHandcuffs;
    public readonly Option HandcuffCooldown;
    public readonly Option KnowsSheriff;

    public static Sprite HandcuffButtonSprite => GetSprite("TheOtherRoles.Resources.DeputyHandcuffButton.png", 115f);
    public static Sprite HandcuffedButtonSprite => GetSprite("TheOtherRoles.Resources.DeputyHandcuffed.png", 115f);

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
            PromotedWhen);
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
        Rpc_DeputyPromotes(PlayerControl.LocalPlayer);
    }

    [MethodRpc((uint)Rpc.Role.DeputyPromotes)]
    private static void Rpc_DeputyPromotes(PlayerControl sender)
    {
        if (Instance.Player == null) return;
        Sheriff.Instance.ReplaceCurrentSheriff(Instance.Player);
        Sheriff.Instance.FormerDeputy = Instance.Player;
        Instance.Player = null;
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
        Instance.UsedHandcuff++;
        Instance.HandcuffedPlayers.Add(targetId);
    }
}