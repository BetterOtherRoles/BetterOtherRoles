using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using BetterOtherRoles.EnoFw.Roles.Crewmate;
using BetterOtherRoles.EnoFw.Roles.Impostor;
using BetterOtherRoles.EnoFw.Roles.Modifiers;
using BetterOtherRoles.EnoFw.Roles.Neutral;
using BetterOtherRoles.Objects;
using BetterOtherRoles.Patches;
using BetterOtherRoles.Players;
using BetterOtherRoles.Utilities;

namespace BetterOtherRoles.EnoFw.Kernel;

public static class KernelRpc
{
    public static void ShareGameMode(byte gameMode)
    {
        RpcManager.Instance.Send((uint)Rpc.Kernel.ShareGameMode, gameMode);
    }
    
    [BindRpc((uint)Rpc.Kernel.ShareGameMode)]
    public static void Rpc_ShareGameMode(byte gameMode)
    {
        TORMapOptions.gameMode = (CustomGamemodes)gameMode;
    }
    
    public static void SetGameStarting()
    {
        RpcManager.Instance.Send((uint)Rpc.Kernel.SetGameStarting);
    }
    
    [BindRpc((uint)Rpc.Kernel.SetGameStarting)]
    public static void Rpc_SetGameStarting()
    {
        GameStartManagerPatch.GameStartManagerUpdatePatch.startingTimer = 5f;
    }
    
    public static void DynamicMapOption(byte mapId)
    {
        RpcManager.Instance.Send((uint)Rpc.Kernel.DynamicMapOption, mapId);
    }

    [BindRpc((uint)Rpc.Kernel.DynamicMapOption)]
    public static void Rpc_DynamicMapOption(byte mapId)
    {
        GameOptionsManager.Instance.currentNormalGameOptions.MapId = mapId;
    }
    
    public static void UncheckedCmdReportDeadBody(byte sourceId, byte? targetId)
    {
        var data = new Tuple<byte, byte?>(sourceId, targetId);
        RpcManager.Instance.Send((uint)Rpc.Kernel.UncheckedCmdReportDeadBody, data);
    }
    
    [BindRpc((uint)Rpc.Kernel.UncheckedCmdReportDeadBody)]
    public static void Rpc_UncheckedCmdReportDeadBody(Tuple<byte, byte?> data)
    {
        var (sourceId, targetId) = data;
        var source = Helpers.playerById(sourceId);
        var target = targetId == null ? null : Helpers.playerById(targetId.Value)?.Data;
        if (source == null) return;
        source.ReportDeadBody(target);
    }
    
    public static void UncheckedExilePlayer(byte targetId)
    {
        RpcManager.Instance.Send((uint)Rpc.Kernel.UncheckedExilePlayer, targetId);
    }
    
    [BindRpc((uint)Rpc.Kernel.UncheckedExilePlayer)]
    public static void Rpc_UncheckedExilePlayer(byte targetId)
    {
        var target = Helpers.playerById(targetId);
        if (target == null) return;
        target.Exiled();
    }
    
    public static void UncheckedMurderPlayer(byte sourceId, byte targetId, bool showAnimation)
    {
        var data = new Tuple<byte, byte, bool>(sourceId, targetId, showAnimation);
        RpcManager.Instance.Send((uint)Rpc.Kernel.UncheckedMurderPlayer, data);
    }
    
    [BindRpc((uint)Rpc.Kernel.UncheckedMurderPlayer)]
    public static void Rpc_UncheckedMurderPlayer(Tuple<byte, byte, bool> data)
    {
        if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;
        var (sourceId, targetId, showAnimation) = data;
        var source = Helpers.playerById(sourceId);
        var target = Helpers.playerById(targetId);
        if (source == null || target == null) return;
        if (!showAnimation) KillAnimationCoPerformKillPatch.hideNextAnimation = true;
        source.MurderPlayer(target);
    }
    
    public static void UseUncheckedVent(int ventId, byte playerId, bool isEnter)
    {
        var data = new Tuple<int, byte, bool>(ventId, playerId, isEnter);
        RpcManager.Instance.Send((uint)Rpc.Kernel.UseUncheckedVent, data, false);
    }
    
    [BindRpc((uint)Rpc.Kernel.UseUncheckedVent)]
    public static void Rpc_UseUncheckedVent(Tuple<int, byte, bool> data)
    {
        var (ventId, playerId, isEnter) = data;
        var player = Helpers.playerById(playerId);
        if (player == null) return;
        JackInTheBox.startAnimation(ventId);
        player.MyPhysics.StopAllCoroutines();
        player.MyPhysics.StartCoroutine(isEnter ? player.MyPhysics.CoEnterVent(ventId) : player.MyPhysics.CoExitVent(ventId));
    }
    
    public static void SetModifier(byte modifierId, byte playerId, byte flag)
    {
        var data = new Tuple<byte, byte, byte>(modifierId, playerId, flag);
        RpcManager.Instance.Send((uint)Rpc.Kernel.SetModifier, data);
    }
    
    [BindRpc((uint)Rpc.Kernel.SetModifier)]
    public static void Rpc_SetModifier(Tuple<byte, byte, byte> data)
    {
        Local_SetModifier((RoleId)data.Item1, data.Item2, data.Item3);
    }

    private static void Local_SetModifier(RoleId modifierId, byte playerId, byte flag)
    {
        var player = Helpers.playerById(playerId);
        if (player == null) return;
        switch (modifierId)
        {
            case RoleId.Bait:
                Bait.Instance.Players.Add(player);
                break;
            case RoleId.Lover:
                if (flag == 0) Lovers.Instance.Lover1 = player;
                else Lovers.Instance.Lover2 = player;
                break;
            case RoleId.Bloody:
                Bloody.Instance.Players.Add(player);
                break;
            case RoleId.AntiTeleport:
                AntiTeleport.Instance.Players.Add(player);
                break;
            case RoleId.Tiebreaker:
                Tiebreaker.Instance.Player = player;
                break;
            case RoleId.Sunglasses:
                Sunglasses.Instance.Players.Add(player);
                break;
            case RoleId.Mini:
                Mini.Instance.Player = player;
                break;
            case RoleId.Vip:
                Vip.Instance.Players.Add(player);
                break;
            case RoleId.Invert:
                Invert.Instance.Players.Add(player);
                break;
            case RoleId.Chameleon:
                Chameleon.Instance.Players.Add(player);
                break;
            case RoleId.Shifter:
                Shifter.Instance.Player = player;
                break;
        }
    }

    public static void WorkaroundSetRoles(Dictionary<byte, byte> roles)
    {
        RpcManager.Instance.Send((uint)Rpc.Kernel.WorkaroundSetRoles, roles);
    }

    [BindRpc((uint)Rpc.Kernel.WorkaroundSetRoles)]
    public static void Rpc_WorkaroundSetRoles(Dictionary<byte, byte> roles)
    {
        foreach (var role in roles)
        {
            Local_SetRole((RoleId)role.Key, role.Value);
        }
    }

    public static void SetRole(byte roleId, byte playerId)
    {
        RpcManager.Instance.Send((uint)Rpc.Kernel.SetRole, new Tuple<byte, byte>(roleId, playerId));
    }

    [BindRpc((uint)Rpc.Kernel.SetRole)]
    public static void Rpc_SetRole(Tuple<byte, byte> data)
    {
        Local_SetRole((RoleId)data.Item1, data.Item2);
    }

    private static void Local_SetRole(RoleId roleId, byte playerId)
    {
        var player = Helpers.playerById(playerId);
        if (player == null) return;
        switch (roleId)
        {
            case RoleId.Jester:
                Jester.Instance.Player = player;
                break;
            case RoleId.Mayor:
                Mayor.Instance.Player = player;
                break;
            case RoleId.Portalmaker:
                Portalmaker.Instance.Player = player;
                break;
            case RoleId.Engineer:
                Engineer.Instance.Player = player;
                break;
            case RoleId.Sheriff:
                Sheriff.Instance.Player = player;
                break;
            case RoleId.Deputy:
                Deputy.Instance.Player = player;
                break;
            case RoleId.Lighter:
                Lighter.Instance.Player = player;
                break;
            case RoleId.Godfather:
                Godfather.Instance.Player = player;
                break;
            case RoleId.Mafioso:
                Mafioso.Instance.Player = player;
                break;
            case RoleId.Janitor:
                Janitor.Instance.Player = player;
                break;
            case RoleId.Detective:
                Detective.Instance.Player = player;
                break;
            case RoleId.TimeMaster:
                TimeMaster.Instance.Player = player;
                break;
            case RoleId.Medic:
                Medic.Instance.Player = player;
                break;
            case RoleId.Shifter:
                Shifter.Instance.Player = player;
                break;
            case RoleId.Swapper:
                Swapper.Instance.Player = player;
                break;
            case RoleId.Seer:
                Seer.Instance.Player = player;
                break;
            case RoleId.Morphling:
                Morphling.Instance.Player = player;
                break;
            case RoleId.Camouflager:
                Camouflager.Instance.Player = player;
                break;
            case RoleId.Hacker:
                Hacker.Instance.Player = player;
                break;
            case RoleId.Tracker:
                Tracker.Instance.Player = player;
                break;
            case RoleId.Vampire:
                Vampire.Instance.Player = player;
                break;
            case RoleId.Snitch:
                Snitch.Instance.Player = player;
                break;
            case RoleId.Jackal:
                Jackal.Instance.Player = player;
                break;
            case RoleId.Sidekick:
                Sidekick.Instance.Player = player;
                break;
            case RoleId.Eraser:
                Eraser.Instance.Player = player;
                break;
            case RoleId.Spy:
                Spy.Instance.Player = player;
                break;
            case RoleId.Trickster:
                Trickster.Instance.Player = player;
                break;
            case RoleId.Cleaner:
                Cleaner.Instance.Player = player;
                break;
            case RoleId.Warlock:
                Warlock.Instance.Player = player;
                break;
            case RoleId.SecurityGuard:
                SecurityGuard.Instance.Player = player;
                break;
            case RoleId.Arsonist:
                Arsonist.Instance.Player = player;
                break;
            case RoleId.EvilGuesser:
                EvilGuesser.Instance.Player = player;
                break;
            case RoleId.NiceGuesser:
                NiceGuesser.Instance.Player = player;
                break;
            case RoleId.BountyHunter:
                BountyHunter.Instance.Player = player;
                break;
            case RoleId.Vulture:
                Vulture.Instance.Player = player;
                break;
            case RoleId.Medium:
                Medium.Instance.Player = player;
                break;
            case RoleId.Trapper:
                Trapper.Instance.Player = player;
                break;
            case RoleId.Lawyer:
                Lawyer.Instance.Player = player;
                break;
            case RoleId.Prosecutor:
                Lawyer.Instance.Player = player;
                Lawyer.Instance.IsProsecutor = true;
                break;
            case RoleId.Pursuer:
                Pursuer.Instance.Player = player;
                break;
            case RoleId.Witch:
                Witch.Instance.Player = player;
                break;
            case RoleId.Ninja:
                Ninja.Instance.Player = player;
                break;
            case RoleId.Thief:
                Thief.Instance.Player = player;
                break;
            case RoleId.Bomber:
                Bomber.Instance.Player = player;
                break;
            case RoleId.Whisperer:
                Whisperer.Instance.Player = player;
                break;
            case RoleId.Undertaker:
                Undertaker.Instance.Player = player;
                break;
        }
    }

    public static void ForceEnd()
    {
        RpcManager.Instance.Send((uint)Rpc.Kernel.ForceEnd);
    }

    [BindRpc((uint)Rpc.Kernel.ForceEnd)]
    public static void Rpc_ForceEnd()
    {
        if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;
        foreach (var player in CachedPlayer.AllPlayers.Select(p => p.PlayerControl))
        {
            if (player.Data.Role.IsImpostor) continue;
            GameData.Instance.GetPlayerById(player.PlayerId);
            // player.RemoveInfected(); (was removed in 2022.12.08, no idea if we ever need that part again, replaced by these 2 lines.) 
            player.SetRole(RoleTypes.Crewmate);
            player.MurderPlayer(player);
            player.Data.IsDead = true;
        }
    }

    public static void ShareOptions(Dictionary<string, int> options)
    {
        RpcManager.Instance.Send((uint)Rpc.Kernel.ShareOptions, options, true, RpcManager.LocalExecution.None);
    }

    [BindRpc((uint)Rpc.Kernel.ShareOptions)]
    public static void Rpc_ShareOptions(Dictionary<string, int> options)
    {
        CustomOption.Tab.SetPreset(0);
        foreach (var o in options)
        {
            if (o.Key == CustomOptions.Preset.Key) continue;
            var option = CustomOption.Tab.Options.Find(option => option.Key == o.Key);
            option?.UpdateSelection(o.Value);
        }
    }

    public static void ResetVariables()
    {
        RpcManager.Instance.Send((uint)Rpc.Kernel.ResetVariables);
    }

    [BindRpc((uint)Rpc.Kernel.ResetVariables)]
    public static void Rpc_ResetVariables()
    {
        Local_ResetVariables();
    }

    public static void Local_ResetVariables()
    {
        Garlic.clearGarlics();
        JackInTheBox.clearJackInTheBoxes();
        NinjaTrace.clearTraces();
        Portal.clearPortals();
        Bloodytrail.resetSprites();
        Trap.clearTraps();
        TORMapOptions.clearAndReloadMapOptions();
        RolesManager.ClearAndReloadRoles();
        GameHistory.clearGameHistory();
        HudManagerStartPatch.setCustomButtonCooldowns();
        TORMapOptions.reloadPluginOptions();
        Helpers.toggleZoom(reset: true);
        GameStartManagerPatch.GameStartManagerUpdatePatch.startingTimer = 0;
        SurveillanceMinigamePatch.nightVisionOverlays = null;
        EventUtility.clearAndReload();
    }
}