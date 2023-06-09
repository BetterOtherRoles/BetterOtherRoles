﻿using System.Collections.Generic;
using AmongUs.GameOptions;
using BetterOtherRoles.EnoFw.Kernel;
using BetterOtherRoles.EnoFw.Roles.Crewmate;
using BetterOtherRoles.EnoFw.Roles.Impostor;
using BetterOtherRoles.Objects;
using BetterOtherRoles.Utilities;
using UnityEngine;
using Option = BetterOtherRoles.EnoFw.Kernel.CustomOption;

namespace BetterOtherRoles.EnoFw.Roles.Neutral;

public class Thief : AbstractRole
{
    public static readonly Thief Instance = new();
    
    // Fields
    public PlayerControl FormerThief;
    public PlayerControl PlayerStolen;
    public bool SuicideFlag;
    public bool StealRole => (string)StealMode is "steal role";
    public bool BecomePartner => (string)StealMode is "become partner";

    // Options
    public readonly CustomOption KillCooldown;
    public readonly CustomOption HasImpostorVision;
    public readonly CustomOption CanUseVents;
    public readonly CustomOption CanKillSheriff;
    public readonly CustomOption StealMode;

    private Thief() : base(nameof(Thief), "Thief")
    {
        Team = Teams.Neutral;
        Color = new Color32(71, 99, 45, byte.MaxValue);
        CanTarget = true;
        
        SpawnRate = GetDefaultSpawnRateOption();
        
        KillCooldown = Tab.CreateFloatList(
            $"{Key}{nameof(KillCooldown)}",
            Cs("Kill cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        HasImpostorVision = Tab.CreateBool(
            $"{Key}{nameof(HasImpostorVision)}",
            Cs("Has impostor vision"),
            false,
            SpawnRate);
        CanUseVents = Tab.CreateBool(
            $"{Key}{nameof(CanUseVents)}",
            Cs("Can use vents"),
            true,
            SpawnRate);
        CanKillSheriff = Tab.CreateBool(
            $"{Key}{nameof(CanKillSheriff)}",
            Cs("Can kill sheriff"),
            true,
            SpawnRate);
        StealMode = Tab.CreateStringList(
            $"{Key}{nameof(StealMode)}",
            Cs("Steal mode"),
            new List<string> { "steal role", "become partner" },
            "steal role",
            SpawnRate);
    }
    
    public override void ClearAndReload()
    {
        base.ClearAndReload();
        FormerThief = null;
        PlayerStolen = null;
        SuicideFlag = false;
    }

    public static void ThiefStealsRole(byte targetId)
    {
        RpcManager.Instance.Send((uint)Rpc.Role.ThiefStealsRole, targetId);
    }

    [BindRpc((uint)Rpc.Role.ThiefStealsRole)]
    public static void Rpc_ThiefStealsRole(byte targetId)
    {
        var target = Helpers.playerById(targetId);
        var thiefPlayer = Instance.Player;
        if (target == null) return;
        if (target == Sheriff.Instance.Player) Sheriff.Instance.Player = thiefPlayer;
        if (target == Jackal.Instance.Player)
        {
            Jackal.Instance.Player = thiefPlayer;
            Jackal.Instance.FormerJackals.Add(target);
        }

        if (target == Sidekick.Instance.Player)
        {
            Sidekick.Instance.Player = thiefPlayer;
            Jackal.Instance.FormerJackals.Add(target);
        }

        if (target == EvilGuesser.Instance.Player) EvilGuesser.Instance.Player = thiefPlayer;
        if (target == Godfather.Instance.Player) Godfather.Instance.Player = thiefPlayer;
        if (target == Mafioso.Instance.Player) Mafioso.Instance.Player = thiefPlayer;
        if (target == Janitor.Instance.Player) Janitor.Instance.Player = thiefPlayer;
        if (target == Morphling.Instance.Player) Morphling.Instance.Player = thiefPlayer;
        if (target == Camouflager.Instance.Player) Camouflager.Instance.Player = thiefPlayer;
        if (target == Vampire.Instance.Player) Vampire.Instance.Player = thiefPlayer;
        if (target == Whisperer.Instance.Player) Whisperer.Instance.Player = thiefPlayer;
        if (target == Undertaker.Instance.Player) Undertaker.Instance.Player = thiefPlayer;
        if (target == Eraser.Instance.Player) Eraser.Instance.Player = thiefPlayer;
        if (target == Trickster.Instance.Player) Trickster.Instance.Player = thiefPlayer;
        if (target == Cleaner.Instance.Player) Cleaner.Instance.Player = thiefPlayer;
        if (target == Warlock.Instance.Player) Warlock.Instance.Player = thiefPlayer;
        if (target == BountyHunter.Instance.Player) BountyHunter.Instance.Player = thiefPlayer;
        if (target == Witch.Instance.Player) Witch.Instance.Player = thiefPlayer;
        if (target == Ninja.Instance.Player) Ninja.Instance.Player = thiefPlayer;
        if (target == Bomber.Instance.Player) Bomber.Instance.Player = thiefPlayer;
        if (target.Data.Role.IsImpostor)
        {
            RoleManager.Instance.SetRole(Instance.Player, RoleTypes.Impostor);
            FastDestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(Instance.Player.killTimer,
                GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown);
        }

        if (target == Lawyer.Instance.Target)
        {
            Lawyer.Instance.Target = thiefPlayer;
            Lawyer.Instance.FormerLawyer = target;
        }

        if (Instance.StealRole)
        {
            Fallen.Instance.ClearAndReload();
            Fallen.Instance.Player = target; // Change target to Fallen ???
            RoleManager.Instance.SetRole(target, RoleTypes.Crewmate);
            Fallen.Instance.Player.clearAllTasks();
        }

        if (Instance.Player == PlayerControl.LocalPlayer) CustomButton.ResetAllCooldowns();
        Instance.ClearAndReload();
        Instance.FormerThief = Instance.Player; // After clearAndReload, else it would get reset...
        Instance.PlayerStolen = target;
        
        thiefPlayer.MurderPlayer(target);
    }
}