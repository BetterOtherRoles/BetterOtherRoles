using System;
using System.Linq;
using BetterOtherRoles.CustomGameModes;
using BetterOtherRoles.EnoFw.Kernel;
using BetterOtherRoles.EnoFw.Roles.Crewmate;
using BetterOtherRoles.EnoFw.Roles.Impostor;
using BetterOtherRoles.EnoFw.Roles.Modifiers;
using BetterOtherRoles.EnoFw.Roles.Neutral;
using BetterOtherRoles.Players;
using BetterOtherRoles.Utilities;
using UnityEngine;

namespace BetterOtherRoles.EnoFw.Modules;

public static class CommonRpc
{
    public static void ShareRoom(byte playerId, byte roomId)
    {
        RpcManager.Instance.Send((uint)Rpc.Module.ShareRoom, new Tuple<byte, byte>(playerId, roomId), true, RpcManager.LocalExecution.None);
    }
    
    [BindRpc((uint)Rpc.Module.ShareRoom)]
    public static void Rpc_ShareRoom(Tuple<byte, byte> data)
    {
        var (playerId, roomId) = data;
        Snitch.Instance.PlayerRoomMap[playerId] = roomId;
    }
    
    public static void HuntedRewindTime(byte playerId)
    {
        RpcManager.Instance.Send((uint)Rpc.Module.HuntedRewindTime, playerId);
    }
    
    [BindRpc((uint)Rpc.Module.HuntedRewindTime)]
    public static void Rpc_HuntedRewindTime(byte playerId)
    {
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

        TimeMaster.Instance.IsRewinding = true;

        if (MapBehaviour.Instance)
            MapBehaviour.Instance.Close();
        if (Minigame.Instance)
            Minigame.Instance.ForceClose();
        CachedPlayer.LocalPlayer.PlayerControl.moveable = false;
    }
    
    public static void HuntedShield(byte playerId)
    {
        RpcManager.Instance.Send((uint)Rpc.Module.HuntedShield, playerId);
    }
    
    [BindRpc((uint)Rpc.Module.HuntedShield)]
    public static void Rpc_HuntedShield(byte playerId)
    {
        if (!Hunted.timeshieldActive.Contains(playerId)) Hunted.timeshieldActive.Add(playerId);
        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Hunted.shieldDuration, new Action<float>((p) => {
            if (p == 1f) Hunted.timeshieldActive.Remove(playerId);
        })));
    }
    
    public static void ShareTimer(float punish)
    {
        RpcManager.Instance.Send((uint)Rpc.Module.ShareTimer, punish);
    }
    
    [BindRpc((uint)Rpc.Module.ShareTimer)]
    public static void Rpc_ShareTimer(float punish)
    {
        HideNSeek.timer -= punish;
    }
    
    public static void SetGuesserGm(byte playerId)
    {
        RpcManager.Instance.Send((uint)Rpc.Module.SetGuesserGm, playerId);
    }
    
    [BindRpc((uint)Rpc.Module.SetGuesserGm)]
    public static void Rpc_SetGuesserGm(byte playerId)
    {
        var target = Helpers.playerById(playerId);
        if (target == null) return;
        var _ = new GuesserGM(target);
    }
    
    public static void CleanBody(byte playerId, byte cleaningPlayerId)
    {
        RpcManager.Instance.Send((uint)Rpc.Role.CleanBody, new Tuple<byte, byte>(playerId, cleaningPlayerId));
    }

    [BindRpc((uint)Rpc.Role.CleanBody)]
    public static void Rpc_CleanBody(Tuple<byte, byte> data)
    {
        var (playerId, cleaningPlayerId) = data;
        if (Medium.Instance.FeatureDeadBodies != null)
        {
            var deadBody = Medium.Instance.FeatureDeadBodies.Find(x => x.Item1.player.PlayerId == playerId).Item1;
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

        if (Vulture.Instance.Player == null || cleaningPlayerId != Vulture.Instance.Player.PlayerId) return;
        Vulture.Instance.EatenBodies++;
        if (Vulture.Instance.EatenBodies == Vulture.Instance.EatNumberToWin)
        {
            Vulture.Instance.TriggerVultureWin = true;
        }
    }

    public static void ErasePlayerRoles(byte playerId, bool ignoreModifier = true)
    {
        RpcManager.Instance.Send((uint)Rpc.Role.ErasePlayerRoles, new Tuple<byte, bool>(playerId, ignoreModifier));
    }

    [BindRpc((uint)Rpc.Role.ErasePlayerRoles)]
    public static void Rpc_ErasePlayerRoles(Tuple<byte, bool> data)
    {
        var (playerId, ignoreModifier) = data;
        Local_ErasePlayerRoles(playerId, ignoreModifier);
        Eraser.Instance.AlreadyErased.Add(playerId);
    }

    public static void Local_ErasePlayerRoles(byte playerId, bool ignoreModifier = true)
    {
        var player = Helpers.playerById(playerId);
        if (player == null) return;
        // Crewmate roles
        if (player == Shifter.Instance.Player) Shifter.Instance.ClearAndReload();

        // Impostor roles

        // Other roles
        if (player == Jackal.Instance.Player)
        {
            // Promote Sidekick and hence override the the Jackal or erase Jackal
            if (Sidekick.Instance.PromotesToJackal && Sidekick.Instance.Player != null && !Sidekick.Instance.Player.Data.IsDead)
            {
                Sidekick.Local_SidekickPromotes();
            }
            else
            {
                Jackal.Instance.ClearAndReload();
            }
        }
        foreach (var role in AbstractRole.AllRoles.Where(role => role.Value.HasPlayer && player == role.Value.Player))
        {
            role.Value.ClearAndReload();
        }

        // Modifier
        if (ignoreModifier) return;
        if (Lovers.Instance.Is(player))
            Lovers.Instance.ClearAndReload(); // The whole Lover couple is being erased
        if (Bait.Instance.Is(player))
            Bait.Instance.Players.RemoveAll(x => x.PlayerId == player.PlayerId);
        if (Bloody.Instance.Is(player))
            Bloody.Instance.Players.RemoveAll(x => x.PlayerId == player.PlayerId);
        if (AntiTeleport.Instance.Is(player))
            AntiTeleport.Instance.Players.RemoveAll(x => x.PlayerId == player.PlayerId);
        if (Sunglasses.Instance.Is(player))
            Sunglasses.Instance.Players.RemoveAll(x => x.PlayerId == player.PlayerId);
        if (player == Tiebreaker.Instance.Player) Tiebreaker.Instance.ClearAndReload();
        if (player == Mini.Instance.Player) Mini.Instance.ClearAndReload();
        if (Vip.Instance.Is(player)) Vip.Instance.Players.RemoveAll(x => x.PlayerId == player.PlayerId);
        if (Invert.Instance.Is(player))
            Invert.Instance.Players.RemoveAll(x => x.PlayerId == player.PlayerId);
        if (Chameleon.Instance.Is(player))
            Chameleon.Instance.Players.RemoveAll(x => x.PlayerId == player.PlayerId);
    }
}