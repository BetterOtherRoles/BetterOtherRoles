﻿using System.Collections.Generic;
using System.Linq;
using Reactor.Networking.Attributes;
using TheOtherRoles.EnoFw.Kernel;
using TheOtherRoles.Players;
using UnityEngine;
using Option = TheOtherRoles.EnoFw.Kernel.CustomOption;

namespace TheOtherRoles.EnoFw.Roles.Neutral;

public class Arsonist : AbstractRole
{
    public static readonly Arsonist Instance = new();
    
    // Fields
    public PlayerControl DouseTarget;
    public readonly List<PlayerControl> DousedPlayers = new();
    public bool TriggerArsonistWin;

    // Options
    public readonly Option DouseCooldown;
    public readonly Option DouseDuration;

    public static Sprite DouseButtonSprite => GetSprite("TheOtherRoles.Resources.DouseButton.png", 115f);
    public static Sprite IgniteButtonSprite => GetSprite("TheOtherRoles.Resources.IgniteButton.png", 115f);

    private Arsonist() : base(nameof(Arsonist), "Arsonist")
    {
        Team = Teams.Neutral;
        Color = new Color32(238, 112, 46, byte.MaxValue);
        CanTarget = true;
        
        SpawnRate = GetDefaultSpawnRateOption();
        
        DouseCooldown = Tab.CreateFloatList(
            $"{Key}{nameof(DouseCooldown)}",
            Cs("Douse cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        DouseDuration = Tab.CreateFloatList(
            $"{Key}{nameof(DouseDuration)}",
            Cs("Douse duration"),
            0f,
            10f,
            1f,
            1f,
            SpawnRate,
            string.Empty,
            "s");
    }

    public bool DousedEveryoneAlive()
    {
        return CachedPlayer.AllPlayers.All(x =>
        {
            return x.PlayerControl == Player || x.Data.IsDead || x.Data.Disconnected ||
                   DousedPlayers.Any(y => y.PlayerId == x.PlayerId);
        });
    }
    
    public override void ClearAndReload()
    {
        base.ClearAndReload();
        DouseTarget = null;
        DousedPlayers.Clear();
        TriggerArsonistWin = false;
        foreach (var p in TORMapOptions.playerIcons.Values)
        {
            if (p != null && p.gameObject != null) p.gameObject.SetActive(false);
        }
    }
    
    public static void ArsonistWin()
    {
        Rpc_ArsonistWin(PlayerControl.LocalPlayer);
    }

    [MethodRpc((uint)Rpc.Role.ArsonistWin)]
    private static void Rpc_ArsonistWin(PlayerControl sender)
    {
        Instance.TriggerArsonistWin = true;
        foreach (var player in CachedPlayer.AllPlayers.Select(p => p.PlayerControl))
        {
            if (player != Instance.Player)
            {
                player.Exiled();
            }
        }
    }
}