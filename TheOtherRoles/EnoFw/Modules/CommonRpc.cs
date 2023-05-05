using System;
using System.Linq;
using Reactor.Networking.Attributes;
using TheOtherRoles.CustomGameModes;
using TheOtherRoles.EnoFw.Roles.Crewmate;
using TheOtherRoles.EnoFw.Roles.Impostor;
using TheOtherRoles.EnoFw.Roles.Modifiers;
using TheOtherRoles.EnoFw.Roles.Neutral;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;
using UnityEngine;

namespace TheOtherRoles.EnoFw.Modules;

public static class CommonRpc
{
    public static void ShareRoom(byte playerId, byte roomId)
    {
        var data = new Tuple<byte, byte>(playerId, roomId);
        Rpc_ShareRoom(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }
    
    [MethodRpc((uint)Rpc.Module.ShareRoom)]
    private static void Rpc_ShareRoom(PlayerControl sender, string rawData)
    {
        if (sender.PlayerId == CachedPlayer.LocalPlayer.PlayerId) return;
        var (playerId, roomId) = Rpc.Deserialize<Tuple<byte, byte>>(rawData);
        Snitch.playerRoomMap[playerId] = roomId;
    }
    
    public static void HuntedRewindTime(byte playerId)
    {
        var data = new Tuple<byte>(playerId);
        Rpc_HuntedRewindTime(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }
    
    [MethodRpc((uint)Rpc.Module.HuntedRewindTime)]
    private static void Rpc_HuntedRewindTime(PlayerControl sender, string rawData)
    {
        var playerId = Rpc.Deserialize<Tuple<byte>>(rawData).Item1;
        Hunted.timeshieldActive.Remove(playerId); // Shield is no longer active when rewinding
        SoundEffectsManager.stop("timemasterShield");  // Shield sound stopped when rewinding
        if (playerId == CachedPlayer.LocalPlayer.PlayerControl.PlayerId) {
            HudManagerStartPatch.resetHuntedRewindButton();
        }
        FastDestroyableSingleton<HudManager>.Instance.FullScreen.color = new Color(0f, 0.5f, 0.8f, 0.3f);
        FastDestroyableSingleton<HudManager>.Instance.FullScreen.enabled = true;
        FastDestroyableSingleton<HudManager>.Instance.FullScreen.gameObject.SetActive(true);
        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Hunted.shieldRewindTime, new Action<float>((p) => {
            if (p == 1f) FastDestroyableSingleton<HudManager>.Instance.FullScreen.enabled = false;
        })));

        if (!CachedPlayer.LocalPlayer.Data.Role.IsImpostor) return; // only rewind hunter

        TimeMaster.isRewinding = true;

        if (MapBehaviour.Instance)
            MapBehaviour.Instance.Close();
        if (Minigame.Instance)
            Minigame.Instance.ForceClose();
        CachedPlayer.LocalPlayer.PlayerControl.moveable = false;
    }
    
    public static void HuntedShield(byte playerId)
    {
        var data = new Tuple<byte>(playerId);
        Rpc_HuntedShield(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }
    
    [MethodRpc((uint)Rpc.Module.HuntedShield)]
    private static void Rpc_HuntedShield(PlayerControl sender, string rawData)
    {
        var playerId = Rpc.Deserialize<Tuple<byte>>(rawData).Item1;
        if (!Hunted.timeshieldActive.Contains(playerId)) Hunted.timeshieldActive.Add(playerId);
        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Hunted.shieldDuration, new Action<float>((p) => {
            if (p == 1f) Hunted.timeshieldActive.Remove(playerId);
        })));
    }
    
    public static void ShareTimer(float punish)
    {
        var data = new Tuple<float>(punish);
        Rpc_ShareTimer(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }
    
    [MethodRpc((uint)Rpc.Module.ShareTimer)]
    private static void Rpc_ShareTimer(PlayerControl sender, string rawData)
    {
        var punish = Rpc.Deserialize<Tuple<float>>(rawData).Item1;
        HideNSeek.timer -= punish;
    }
    
    public static void SetGuesserGm(byte playerId)
    {
        var data = new Tuple<byte>(playerId);
        Rpc_SetGuesserGm(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }
    
    [MethodRpc((uint)Rpc.Module.SetGuesserGm)]
    private static void Rpc_SetGuesserGm(PlayerControl sender, string rawData)
    {
        var playerId = Rpc.Deserialize<Tuple<byte>>(rawData).Item1;
        var target = Helpers.playerById(playerId);
        if (target == null) return;
        var _ = new GuesserGM(target);
    }
    
    public static void CleanBody(byte playerId, byte cleaningPlayerId)
    {
        var data = new Tuple<byte, byte>(playerId, cleaningPlayerId);
        Rpc_CleanBody(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.CleanBody)]
    private static void Rpc_CleanBody(PlayerControl sender, string rawData)
    {
        var (playerId, cleaningPlayerId) = Rpc.Deserialize<Tuple<byte, byte>>(rawData);
        if (Medium.featureDeadBodies != null)
        {
            var deadBody = Medium.featureDeadBodies.Find(x => x.Item1.player.PlayerId == playerId).Item1;
            if (deadBody != null) deadBody.wasCleaned = true;
        }

        DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
        foreach (var body in array)
        {
            if (GameData.Instance.GetPlayerById(body.ParentId).PlayerId == playerId)
            {
                UnityEngine.Object.Destroy(body.gameObject);
            }
        }

        if (Vulture.vulture == null || cleaningPlayerId != Vulture.vulture.PlayerId) return;
        Vulture.eatenBodies++;
        if (Vulture.eatenBodies == Vulture.vultureNumberToWin)
        {
            Vulture.triggerVultureWin = true;
        }
    }

    public static void ErasePlayerRoles(byte playerId, bool ignoreModifier = true)
    {
        var data = new Tuple<byte, bool>(playerId, ignoreModifier);
        Rpc_ErasePlayerRoles(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.ErasePlayerRoles)]
    private static void Rpc_ErasePlayerRoles(PlayerControl sender, string rawData)
    {
        var (playerId, ignoreModifier) = Rpc.Deserialize<Tuple<byte, bool>>(rawData);
        Local_ErasePlayerRoles(playerId, ignoreModifier);
        Eraser.alreadyErased.Add(playerId);
    }

    public static void Local_ErasePlayerRoles(byte playerId, bool ignoreModifier = true)
    {
        var player = Helpers.playerById(playerId);
        if (player == null) return;
        // Crewmate roles
        if (player == Mayor.mayor) Mayor.clearAndReload();
        if (player == Portalmaker.portalmaker) Portalmaker.clearAndReload();
        if (player == Engineer.engineer) Engineer.clearAndReload();
        if (player == Sheriff.sheriff) Sheriff.clearAndReload();
        if (player == Deputy.Player) Deputy.ClearAndReload();
        if (player == Lighter.lighter) Lighter.clearAndReload();
        if (player == Detective.detective) Detective.clearAndReload();
        if (player == TimeMaster.timeMaster) TimeMaster.clearAndReload();
        if (player == Medic.medic) Medic.clearAndReload();
        if (player == Shifter.shifter) Shifter.clearAndReload();
        if (player == Seer.seer) Seer.clearAndReload();
        if (player == Hacker.hacker) Hacker.clearAndReload();
        if (player == Tracker.tracker) Tracker.clearAndReload();
        if (player == Snitch.snitch) Snitch.clearAndReload();
        if (player == Swapper.swapper) Swapper.clearAndReload();
        if (player == Spy.spy) Spy.clearAndReload();
        if (player == SecurityGuard.securityGuard) SecurityGuard.clearAndReload();
        if (player == Medium.medium) Medium.clearAndReload();
        if (player == Trapper.trapper) Trapper.clearAndReload();

        // Impostor roles
        if (player == Morphling.morphling) Morphling.clearAndReload();
        if (player == Camouflager.camouflager) Camouflager.clearAndReload();
        if (player == Godfather.godfather) Godfather.clearAndReload();
        if (player == Mafioso.mafioso) Mafioso.clearAndReload();
        if (player == Janitor.janitor) Janitor.clearAndReload();
        if (player == Vampire.vampire) Vampire.clearAndReload();
        if (player == Whisperer.whisperer) Whisperer.clearAndReload();
        if (player == Undertaker.undertaker) Undertaker.clearAndReload();
        if (player == Eraser.eraser) Eraser.clearAndReload();
        if (player == Trickster.trickster) Trickster.clearAndReload();
        if (player == Cleaner.cleaner) Cleaner.clearAndReload();
        if (player == Warlock.warlock) Warlock.clearAndReload();
        if (player == Witch.witch) Witch.clearAndReload();
        if (player == Ninja.ninja) Ninja.clearAndReload();
        if (player == Bomber.bomber) Bomber.clearAndReload();

        // Other roles
        if (player == Jester.jester) Jester.clearAndReload();
        if (player == Arsonist.arsonist) Arsonist.clearAndReload();
        if (Guesser.isGuesser(player.PlayerId)) Guesser.clear(player.PlayerId);
        if (player == Jackal.jackal)
        {
            // Promote Sidekick and hence override the the Jackal or erase Jackal
            if (Sidekick.promotesToJackal && Sidekick.sidekick != null && !Sidekick.sidekick.Data.IsDead)
            {
                Sidekick.Local_SidekickPromotes();
            }
            else
            {
                Jackal.clearAndReload();
            }
        }

        if (player == Sidekick.sidekick) Sidekick.clearAndReload();
        if (player == BountyHunter.bountyHunter) BountyHunter.clearAndReload();
        if (player == Vulture.vulture) Vulture.clearAndReload();
        if (player == Lawyer.lawyer) Lawyer.clearAndReload();
        if (player == Pursuer.pursuer) Pursuer.clearAndReload();
        if (player == Thief.thief) Thief.clearAndReload();
        if (player == Fallen.fallen) Fallen.clearAndReload();

        // Modifier
        if (ignoreModifier) return;
        if (player == Lovers.lover1 || player == Lovers.lover2)
            Lovers.clearAndReload(); // The whole Lover couple is being erased
        if (Bait.bait.Any(x => x.PlayerId == player.PlayerId))
            Bait.bait.RemoveAll(x => x.PlayerId == player.PlayerId);
        if (Bloody.bloody.Any(x => x.PlayerId == player.PlayerId))
            Bloody.bloody.RemoveAll(x => x.PlayerId == player.PlayerId);
        if (AntiTeleport.antiTeleport.Any(x => x.PlayerId == player.PlayerId))
            AntiTeleport.antiTeleport.RemoveAll(x => x.PlayerId == player.PlayerId);
        if (Sunglasses.sunglasses.Any(x => x.PlayerId == player.PlayerId))
            Sunglasses.sunglasses.RemoveAll(x => x.PlayerId == player.PlayerId);
        if (player == Tiebreaker.tiebreaker) Tiebreaker.clearAndReload();
        if (player == Mini.mini) Mini.clearAndReload();
        if (Vip.vip.Any(x => x.PlayerId == player.PlayerId)) Vip.vip.RemoveAll(x => x.PlayerId == player.PlayerId);
        if (Invert.invert.Any(x => x.PlayerId == player.PlayerId))
            Invert.invert.RemoveAll(x => x.PlayerId == player.PlayerId);
        if (Chameleon.chameleon.Any(x => x.PlayerId == player.PlayerId))
            Chameleon.chameleon.RemoveAll(x => x.PlayerId == player.PlayerId);
    }
}