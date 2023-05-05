using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Reactor.Networking.Attributes;
using TheOtherRoles.EnoFw.Roles.Crewmate;
using TheOtherRoles.EnoFw.Roles.Impostor;
using TheOtherRoles.EnoFw.Roles.Modifiers;
using TheOtherRoles.EnoFw.Roles.Neutral;
using TheOtherRoles.Objects;
using TheOtherRoles.Patches;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;

namespace TheOtherRoles.EnoFw.Kernel;

public static class KernelRpc
{
    public static void ShareGameMode(byte gameMode)
    {
        var data = new Tuple<byte>(gameMode);
        Rpc_ShareGameMode(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }
    
    [MethodRpc((uint)Rpc.Kernel.ShareGameMode)]
    private static void Rpc_ShareGameMode(PlayerControl sender, string rawData)
    {
        var gameMode = Rpc.Deserialize<Tuple<byte>>(rawData).Item1;
        TORMapOptions.gameMode = (CustomGamemodes)gameMode;
    }
    
    public static void SetGameStarting()
    {
        Rpc_SetGameStarting(PlayerControl.LocalPlayer);
    }
    
    [MethodRpc((uint)Rpc.Kernel.SetGameStarting)]
    private static void Rpc_SetGameStarting(PlayerControl sender)
    {
        GameStartManagerPatch.GameStartManagerUpdatePatch.startingTimer = 5f;
    }
    
    public static void DynamicMapOption(byte mapId)
    {
        var data = new Tuple<byte>(mapId);
        Rpc_DynamicMapOption(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Kernel.DynamicMapOption)]
    private static void Rpc_DynamicMapOption(PlayerControl sender, string rawData)
    {
        var mapId = Rpc.Deserialize<Tuple<byte>>(rawData).Item1;
        GameOptionsManager.Instance.currentNormalGameOptions.MapId = mapId;
    }
    
    public static void UncheckedCmdReportDeadBody(byte sourceId, byte? targetId)
    {
        var data = new Tuple<byte, byte?>(sourceId, targetId);
        Rpc_UncheckedCmdReportDeadBody(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }
    
    [MethodRpc((uint)Rpc.Kernel.UncheckedCmdReportDeadBody)]
    private static void Rpc_UncheckedCmdReportDeadBody(PlayerControl sender, string rawData)
    {
        var (sourceId, targetId) = Rpc.Deserialize<Tuple<byte, byte?>>(rawData);
        var source = Helpers.playerById(sourceId);
        var target = targetId == null ? null : Helpers.playerById(targetId.Value);
        if (source == null || (targetId != null && target == null)) return;
        source.ReportDeadBody(target == null ? null : target.Data);
    }
    
    public static void UncheckedExilePlayer(byte targetId)
    {
        var data = new Tuple<byte>(targetId);
        Rpc_UncheckedExilePlayer(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }
    
    [MethodRpc((uint)Rpc.Kernel.UncheckedExilePlayer)]
    private static void Rpc_UncheckedExilePlayer(PlayerControl sender, string rawData)
    {
        var targetId = Rpc.Deserialize<Tuple<byte>>(rawData).Item1;
        var target = Helpers.playerById(targetId);
        if (target == null) return;
        target.Exiled();
    }
    
    public static void UncheckedMurderPlayer(byte sourceId, byte targetId, bool showAnimation)
    {
        var data = new Tuple<byte, byte, bool>(sourceId, targetId, showAnimation);
        Rpc_UncheckedMurderPlayer(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }
    
    [MethodRpc((uint)Rpc.Kernel.UncheckedMurderPlayer)]
    private static void Rpc_UncheckedMurderPlayer(PlayerControl sender, string rawData)
    {
        if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;
        var (sourceId, targetId, showAnimation) = Rpc.Deserialize<Tuple<byte, byte, bool>>(rawData);
        var source = Helpers.playerById(sourceId);
        var target = Helpers.playerById(targetId);
        if (source == null || target == null) return;
        if (!showAnimation) KillAnimationCoPerformKillPatch.hideNextAnimation = true;
        source.MurderPlayer(target);
    }
    
    public static void UseUncheckedVent(int ventId, byte playerId, bool isEnter)
    {
        var data = new Tuple<int, byte, bool>(ventId, playerId, isEnter);
        Rpc_UseUncheckedVent(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }
    
    [MethodRpc((uint)Rpc.Kernel.UseUncheckedVent)]
    private static void Rpc_UseUncheckedVent(PlayerControl sender, string rawData)
    {
        var (ventId, playerId, isEnter) = Rpc.Deserialize<Tuple<int, byte, bool>>(rawData);
        var player = Helpers.playerById(playerId);
        if (player == null) return;
        JackInTheBox.startAnimation(ventId);
        player.MyPhysics.StopAllCoroutines();
        player.MyPhysics.StartCoroutine(isEnter ? player.MyPhysics.CoEnterVent(ventId) : player.MyPhysics.CoExitVent(ventId));
    }
    
    public static void VersionHandshake(int clientId, Version version, Guid guid, float timer)
    {
        var data = new Tuple<int, string, string, float>(clientId, version.ToString(), guid.ToString(), timer);
        Rpc_VersionHandshake(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }
    
    [MethodRpc((uint)Rpc.Kernel.VersionHandshake)]
    private static void Rpc_VersionHandshake(PlayerControl sender, string rawData)
    {
        var (clientId, versionString, guidString, timer) = Rpc.Deserialize<Tuple<int, string, string, float>>(rawData);
        var version = Version.Parse(versionString);
        var guid = Guid.Parse(guidString);
        GameStartManagerPatch.playerVersions[clientId] = new GameStartManagerPatch.PlayerVersion(version, guid);
        if (!AmongUsClient.Instance.AmHost && timer >= 0f) GameStartManagerPatch.timer = timer;
    }
    
    public static void SetModifier(byte modifierId, byte playerId, byte flag)
    {
        var data = new Tuple<byte, byte, byte>(modifierId, playerId, flag);
        Rpc_SetModifier(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }
    
    [MethodRpc((uint)Rpc.Kernel.SetModifier)]
    private static void Rpc_SetModifier(PlayerControl sender, string rawData)
    {
        var data = Rpc.Deserialize<Tuple<byte, byte, byte>>(rawData);
        Local_SetModifier((RoleId)data.Item1, data.Item2, data.Item3);
    }

    private static void Local_SetModifier(RoleId modifierId, byte playerId, byte flag)
    {
        var player = Helpers.playerById(playerId);
        if (player == null) return;
        switch (modifierId)
        {
            case RoleId.Bait:
                Bait.bait.Add(player);
                break;
            case RoleId.Lover:
                if (flag == 0) Lovers.lover1 = player;
                else Lovers.lover2 = player;
                break;
            case RoleId.Bloody:
                Bloody.bloody.Add(player);
                break;
            case RoleId.AntiTeleport:
                AntiTeleport.antiTeleport.Add(player);
                break;
            case RoleId.Tiebreaker:
                Tiebreaker.tiebreaker = player;
                break;
            case RoleId.Sunglasses:
                Sunglasses.sunglasses.Add(player);
                break;
            case RoleId.Mini:
                Mini.mini = player;
                break;
            case RoleId.Vip:
                Vip.vip.Add(player);
                break;
            case RoleId.Invert:
                Invert.invert.Add(player);
                break;
            case RoleId.Chameleon:
                Chameleon.chameleon.Add(player);
                break;
            case RoleId.Shifter:
                Shifter.shifter = player;
                break;
        }
    }

    public static void WorkaroundSetRoles(Dictionary<byte, byte> roles)
    {
        Rpc_WorkaroundSetRoles(PlayerControl.LocalPlayer, Rpc.Serialize(roles));
    }

    [MethodRpc((uint)Rpc.Kernel.WorkaroundSetRoles)]
    private static void Rpc_WorkaroundSetRoles(PlayerControl sender, string rawData)
    {
        var roles = Rpc.Deserialize<Dictionary<byte, byte>>(rawData);
        foreach (var role in roles)
        {
            Local_SetRole((RoleId)role.Key, role.Value);
        }
    }

    public static void SetRole(byte roleId, byte playerId)
    {
        Rpc_SetRole(PlayerControl.LocalPlayer, Rpc.Serialize(new Tuple<byte, byte>(roleId, playerId)));
    }

    [MethodRpc((uint)Rpc.Kernel.SetRole)]
    private static void Rpc_SetRole(PlayerControl sender, string rawData)
    {
        var data = Rpc.Deserialize<Tuple<byte, byte>>(rawData);
        Local_SetRole((RoleId)data.Item1, data.Item2);
    }

    private static void Local_SetRole(RoleId roleId, byte playerId)
    {
        var player = Helpers.playerById(playerId);
        if (player == null) return;
        switch (roleId)
        {
            case RoleId.Jester:
                Jester.jester = player;
                break;
            case RoleId.Mayor:
                Mayor.mayor = player;
                break;
            case RoleId.Portalmaker:
                Portalmaker.portalmaker = player;
                break;
            case RoleId.Engineer:
                Engineer.engineer = player;
                break;
            case RoleId.Sheriff:
                Sheriff.sheriff = player;
                break;
            case RoleId.Deputy:
                Deputy.Player = player;
                break;
            case RoleId.Lighter:
                Lighter.lighter = player;
                break;
            case RoleId.Godfather:
                Godfather.godfather = player;
                break;
            case RoleId.Mafioso:
                Mafioso.mafioso = player;
                break;
            case RoleId.Janitor:
                Janitor.janitor = player;
                break;
            case RoleId.Detective:
                Detective.detective = player;
                break;
            case RoleId.TimeMaster:
                TimeMaster.timeMaster = player;
                break;
            case RoleId.Medic:
                Medic.medic = player;
                break;
            case RoleId.Shifter:
                Shifter.shifter = player;
                break;
            case RoleId.Swapper:
                Swapper.swapper = player;
                break;
            case RoleId.Seer:
                Seer.seer = player;
                break;
            case RoleId.Morphling:
                Morphling.morphling = player;
                break;
            case RoleId.Camouflager:
                Camouflager.camouflager = player;
                break;
            case RoleId.Hacker:
                Hacker.hacker = player;
                break;
            case RoleId.Tracker:
                Tracker.tracker = player;
                break;
            case RoleId.Vampire:
                Vampire.vampire = player;
                break;
            case RoleId.Snitch:
                Snitch.snitch = player;
                break;
            case RoleId.Jackal:
                Jackal.jackal = player;
                break;
            case RoleId.Sidekick:
                Sidekick.sidekick = player;
                break;
            case RoleId.Eraser:
                Eraser.eraser = player;
                break;
            case RoleId.Spy:
                Spy.spy = player;
                break;
            case RoleId.Trickster:
                Trickster.trickster = player;
                break;
            case RoleId.Cleaner:
                Cleaner.cleaner = player;
                break;
            case RoleId.Warlock:
                Warlock.warlock = player;
                break;
            case RoleId.SecurityGuard:
                SecurityGuard.securityGuard = player;
                break;
            case RoleId.Arsonist:
                Arsonist.arsonist = player;
                break;
            case RoleId.EvilGuesser:
                Guesser.evilGuesser = player;
                break;
            case RoleId.NiceGuesser:
                Guesser.niceGuesser = player;
                break;
            case RoleId.BountyHunter:
                BountyHunter.bountyHunter = player;
                break;
            case RoleId.Vulture:
                Vulture.vulture = player;
                break;
            case RoleId.Medium:
                Medium.medium = player;
                break;
            case RoleId.Trapper:
                Trapper.trapper = player;
                break;
            case RoleId.Lawyer:
                Lawyer.lawyer = player;
                break;
            case RoleId.Prosecutor:
                Lawyer.lawyer = player;
                Lawyer.isProsecutor = true;
                break;
            case RoleId.Pursuer:
                Pursuer.pursuer = player;
                break;
            case RoleId.Witch:
                Witch.witch = player;
                break;
            case RoleId.Ninja:
                Ninja.ninja = player;
                break;
            case RoleId.Thief:
                Thief.thief = player;
                break;
            case RoleId.Bomber:
                Bomber.bomber = player;
                break;
            case RoleId.Whisperer:
                Whisperer.whisperer = player;
                break;
            case RoleId.Undertaker:
                Undertaker.undertaker = player;
                break;
        }
    }

    public static void ForceEnd()
    {
        Rpc_ForceEnd(PlayerControl.LocalPlayer);
    }

    [MethodRpc((uint)Rpc.Kernel.ForceEnd)]
    private static void Rpc_ForceEnd(PlayerControl sender)
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

    public static void ShareOptions(Dictionary<int, int> options)
    {
        Rpc_ShareOptions(PlayerControl.LocalPlayer, Rpc.Serialize(options));
    }

    [MethodRpc((uint)Rpc.Kernel.ShareOptions)]
    private static void Rpc_ShareOptions(PlayerControl sender, string rawData)
    {
        if (sender.AmOwner) return;
        var options = Rpc.Deserialize<Dictionary<int, int>>(rawData);
        foreach (var o in options)
        {
            var option = CustomOption.options.Find(option => option.id == o.Key);
            option?.updateSelection(o.Value);
        }
    }

    public static void ResetVariables()
    {
        Rpc_ResetVariables(PlayerControl.LocalPlayer);
    }

    [MethodRpc((uint)Rpc.Kernel.ResetVariables)]
    private static void Rpc_ResetVariables(PlayerControl sender)
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
        TheOtherRoles.clearAndReloadRoles();
        GameHistory.clearGameHistory();
        HudManagerStartPatch.setCustomButtonCooldowns();
        TORMapOptions.reloadPluginOptions();
        Helpers.toggleZoom(reset: true);
        GameStartManagerPatch.GameStartManagerUpdatePatch.startingTimer = 0;
        SurveillanceMinigamePatch.nightVisionOverlays = null;
        EventUtility.clearAndReload();
    }
}