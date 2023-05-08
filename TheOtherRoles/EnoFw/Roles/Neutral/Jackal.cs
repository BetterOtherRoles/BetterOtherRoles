using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Reactor.Networking.Attributes;
using TheOtherRoles.EnoFw.Kernel;
using TheOtherRoles.EnoFw.Modules;
using TheOtherRoles.EnoFw.Roles.Crewmate;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;
using TMPro;
using UnityEngine;
using Option = TheOtherRoles.EnoFw.Kernel.CustomOption;

namespace TheOtherRoles.EnoFw.Roles.Neutral;

public class Jackal : AbstractRole
{
    public static readonly Jackal Instance = new();
    
    // Fields
    public PlayerControl FakeSidekick;
    public readonly List<PlayerControl> FormerJackals = new();
    public bool WasTeamRed;
    public bool WasImpostor;
    public bool WasSpy;
    public bool CanCreateSidekick;
    
    // Options
    public readonly Option KillCooldown;
    public readonly Option HasImpostorVision;
    public readonly Option CanUseVents;
    public readonly Option JackalCanCreateSidekick;
    public readonly Option CreateSidekickCooldown;
    public readonly Option SidekickPromoteToJackal;
    public readonly Option PromotedFromSidekickCanCreateSidekick;
    public readonly Option CanCreateSidekickFromImpostor;

    public static Sprite SidekickButtonSprite => GetSprite("TheOtherRoles.Resources.SidekickButton.png", 115f);

    private Jackal() : base(nameof(Jackal), "Jackal")
    {
        Team = Teams.Neutral;
        Color = new Color32(0, 180, 235, byte.MaxValue);
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
            true,
            SpawnRate);
        CanUseVents = Tab.CreateBool(
            $"{Key}{nameof(CanUseVents)}",
            Cs("Can use vents"),
            true,
            SpawnRate);
        JackalCanCreateSidekick = Tab.CreateBool(
            $"{Key}{nameof(JackalCanCreateSidekick)}",
            Cs("Can create a sidekick"),
            false,
            SpawnRate);
        CreateSidekickCooldown = Tab.CreateFloatList(
            $"{Key}{nameof(CreateSidekickCooldown)}",
            Cs("Create a sidekick cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            JackalCanCreateSidekick,
            string.Empty,
            "s");
        CanCreateSidekickFromImpostor = Tab.CreateBool(
            $"{Key}{nameof(CanCreateSidekickFromImpostor)}",
            Cs("Can convert an impostor to Sidekick"),
            false,
            JackalCanCreateSidekick);
        SidekickPromoteToJackal = Tab.CreateBool(
            $"{Key}{nameof(SidekickPromoteToJackal)}",
            Cs("Sidekick gets promoted to jackal on jackal death"),
            false,
            JackalCanCreateSidekick);
        PromotedFromSidekickCanCreateSidekick = Tab.CreateBool(
            $"{Key}{nameof(PromotedFromSidekickCanCreateSidekick)}",
            Cs("Jackal promoted from sidekick can create a sidekick"),
            false,
            JackalCanCreateSidekick);
    }

    public void RemoveCurrentJackal()
    {
        if (FormerJackals.All(x => x.PlayerId != Player.PlayerId)) FormerJackals.Add(Player);
        Player = null;
        CurrentTarget = null;
        FakeSidekick = null;
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        FakeSidekick = null;
        FormerJackals.Clear();
        WasTeamRed = false;
        WasImpostor = false;
        WasSpy = false;
        CanCreateSidekick = JackalCanCreateSidekick;
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
        if (Lawyer.Instance.Target == player && Lawyer.Instance.IsProsecutor && Lawyer.Instance.Player != null && !Lawyer.Instance.Player.Data.IsDead) Lawyer.Instance.IsProsecutor = false;

        if (!Instance.CanCreateSidekickFromImpostor && player.Data.Role.IsImpostor) {
            Instance.FakeSidekick = player;
        } else {
            var localWasSpy = Spy.Instance.Player != null && player == Spy.Instance.Player;
            var localWasImpostor = player.Data.Role.IsImpostor;  // This can only be reached if impostors can be sidekicked.
            FastDestroyableSingleton<RoleManager>.Instance.SetRole(player, RoleTypes.Crewmate);
            if (player == Lawyer.Instance.Player && Lawyer.Instance.Target != null)
            {
                var playerInfoTransform = Lawyer.Instance.Target.cosmetics.nameText.transform.parent.FindChild("Info");
                var playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TextMeshPro>() : null;
                if (playerInfo != null) playerInfo.text = "";
            }
            CommonRpc.Local_ErasePlayerRoles(player.PlayerId);
            Sidekick.Instance.Player = player;
            if (player.PlayerId == CachedPlayer.LocalPlayer.PlayerId) CachedPlayer.LocalPlayer.PlayerControl.moveable = true;
            if (localWasSpy || localWasImpostor) Sidekick.Instance.WasTeamRed = true;
            Sidekick.Instance.WasSpy = localWasSpy;
            Sidekick.Instance.WasImpostor = localWasImpostor;
            if (player == CachedPlayer.LocalPlayer.PlayerControl) SoundEffectsManager.play("jackalSidekick");
        }
        Instance.CanCreateSidekick = false;
    }
}