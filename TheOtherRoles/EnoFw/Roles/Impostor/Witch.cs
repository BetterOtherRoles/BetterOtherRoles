using System;
using System.Collections.Generic;
using Reactor.Networking.Attributes;
using TheOtherRoles.EnoFw.Kernel;
using UnityEngine;
using Option = TheOtherRoles.EnoFw.Kernel.CustomOption;

namespace TheOtherRoles.EnoFw.Roles.Impostor;

public class Witch : AbstractRole
{
    public static readonly Witch Instance = new();
    
    // Fields
    public readonly List<PlayerControl> FutureSpelled = new();
    public PlayerControl SpellCastingTarget;
    public float CurrentCooldownAddition;
    
    // Options
    public readonly Option SpellCooldown;
    public readonly Option SpellCastingDuration;
    public readonly Option AdditionalCooldown;
    public readonly Option CanSpellAnyone;
    public readonly Option TriggerBothCooldown;
    public readonly Option WitchVoteSaveTargets;

    public static Sprite SpellButtonSprite => GetSprite("TheOtherRoles.Resources.SpellButton.png", 115f);
    public static Sprite SpelledOverlaySprite => GetSprite("TheOtherRoles.Resources.SpellButtonMeeting.png", 225f);

    private Witch() : base(nameof(Witch), "Witch")
    {
        Team = Teams.Impostor;
        Color = Palette.ImpostorRed;
        CanTarget = true;
        
        SpawnRate = GetDefaultSpawnRateOption();
        
        SpellCooldown = Tab.CreateFloatList(
            $"{Key}{nameof(SpellCooldown)}",
            Cs("Spell cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        SpellCastingDuration = Tab.CreateFloatList(
            $"{Key}{nameof(SpellCastingDuration)}",
            Cs("Spell casting duration"),
            0f,
            10f,
            1f,
            1f,
            SpawnRate,
            string.Empty,
            "s");
        AdditionalCooldown = Tab.CreateFloatList(
            $"{Key}{nameof(AdditionalCooldown)}",
            Cs("Spell additional cooldown"),
            0f,
            60f,
            10f,
            5f,
            SpawnRate,
            string.Empty,
            "s");
        CanSpellAnyone = Tab.CreateBool(
            $"{Key}{nameof(CanSpellAnyone)}",
            Cs("Can spell anyone"),
            false,
            SpawnRate);
        TriggerBothCooldown = Tab.CreateBool(
            $"{Key}{nameof(TriggerBothCooldown)}",
            Cs("Trigger both cooldown"),
            false,
            SpawnRate);
        WitchVoteSaveTargets = Tab.CreateBool(
            $"{Key}{nameof(WitchVoteSaveTargets)}",
            Cs("Voting the witch saves all targets"),
            false,
            SpawnRate);
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        FutureSpelled.Clear();
        SpellCastingTarget = null;
        CurrentCooldownAddition = 0f;
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
        
        var player = Helpers.playerById(playerId);
        if (player != null) {
            Instance.FutureSpelled.Add(player);
        }
    }
}