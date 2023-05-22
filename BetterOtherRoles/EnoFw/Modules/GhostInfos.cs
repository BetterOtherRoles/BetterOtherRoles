using System;
using BetterOtherRoles.EnoFw.Kernel;
using BetterOtherRoles.EnoFw.Roles.Crewmate;
using BetterOtherRoles.EnoFw.Roles.Impostor;
using BetterOtherRoles.EnoFw.Roles.Neutral;
using BetterOtherRoles.Utilities;

namespace BetterOtherRoles.EnoFw.Modules;

public static class GhostInfos
{
    public enum Types : byte
    {
        HandcuffNoticed,
        HandcuffOver,
        ArsonistDouse,
        BountyTarget,
        NinjaMarked,
        WarlockTarget,
        MediumInfo,
        BlankUsed,
        DetectiveOrMedicInfo,
        VampireTimer,
        WhispererTimerAndTarget
    }

    public static void ShareGhostInfo(Types type, string rawData)
    {
        RpcManager.Instance.Send((uint)Rpc.Module.ShareGhostInfo, new Tuple<byte, string>((byte)type, rawData), true, RpcManager.LocalExecution.None);
    }

    [BindRpc((uint)Rpc.Module.ShareGhostInfo)]
    public static void Rpc_ShareGhostInfo(Tuple<byte, string> rawData)
    {
        var (id, data) = rawData;
        switch ((Types)id)
        {
            case Types.HandcuffNoticed:
                HandcuffNoticed(data);
                return;
            case Types.HandcuffOver:
                HandcuffOver(data);
                return;
            case Types.ArsonistDouse:
                ArsonistDouse(data);
                return;
            case Types.BountyTarget:
                BountyTarget(data);
                return;
            case Types.NinjaMarked:
                NinjaMarked(data);
                return;
            case Types.WarlockTarget:
                WarlockTarget(data);
                return;
            case Types.MediumInfo:
                MediumInfo(data);
                return;
            case Types.DetectiveOrMedicInfo:
                DetectiveOrMedicInfo(data);
                return;
            case Types.BlankUsed:
                BlankUsed(data);
                return;
            case Types.VampireTimer:
                VampireTimer(data);
                return;
            case Types.WhispererTimerAndTarget:
                WhispererTimerAndTarget(data);
                return;
        }
    }

    private static void WhispererTimerAndTarget(string rawData)
    {
        var (victimId, victimToKillId, timer) = Rpc.Deserialize<Tuple<byte, byte, float>>(rawData);
        Whisperer.Instance.WhisperVictim = Helpers.playerById(victimId);
        Whisperer.Instance.WhisperVictimTarget = Helpers.playerById(victimToKillId);
        HudManagerStartPatch.whispererKillButton.Timer = timer;
    }

    private static void VampireTimer(string rawData)
    {
        var timer = Rpc.Deserialize<Tuple<float>>(rawData).Item1;
        HudManagerStartPatch.vampireKillButton.Timer = timer;
    }

    private static void BlankUsed(string rawData)
    {
        var playerId = Rpc.Deserialize<Tuple<byte>>(rawData).Item1;
        var player = Helpers.playerById(playerId);
        if (player == null) return;
        Pursuer.Instance.BlankedList.Remove(player);
    }

    private static void DetectiveOrMedicInfo(string rawData)
    {
        if (!Helpers.shouldShowGhostInfo()) return;
        var (playerId, info) = Rpc.Deserialize<Tuple<byte, string>>(rawData);
        var player = Helpers.playerById(playerId);
        if (player == null) return;
        FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(player, info);
    }

    private static void MediumInfo(string rawData)
    {
        if (!Helpers.shouldShowGhostInfo()) return;
        var (playerId, info) = Rpc.Deserialize<Tuple<byte, string>>(rawData);
        var player = Helpers.playerById(playerId);
        if (player == null) return;
        FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(player, info);
    }

    private static void WarlockTarget(string rawData)
    {
        var playerId = Rpc.Deserialize<Tuple<byte>>(rawData).Item1;
        if (playerId == byte.MaxValue)
        {
            Warlock.Instance.CurseVictim = null;
        }
        Warlock.Instance.CurseVictim = Helpers.playerById(playerId);
    }

    private static void NinjaMarked(string rawData)
    {
        var playerId = Rpc.Deserialize<Tuple<byte>>(rawData).Item1;
        Ninja.Instance.MarkedTarget = Helpers.playerById(playerId);
    }

    private static void BountyTarget(string rawData)
    {
        var playerId = Rpc.Deserialize<Tuple<byte>>(rawData).Item1;
        BountyHunter.Instance.Bounty = Helpers.playerById(playerId);
    }

    private static void HandcuffNoticed(string rawData)
    {
        var playerId = Rpc.Deserialize<Tuple<byte>>(rawData).Item1;
        Deputy.Instance.SetHandcuffedKnows(true, playerId);
    }

    private static void HandcuffOver(string rawData)
    {
        var playerId = Rpc.Deserialize<Tuple<byte>>(rawData).Item1;
        Deputy.Instance.HandcuffedKnows.Remove(playerId);
    }

    private static void ArsonistDouse(string rawData)
    {
        var playerId = Rpc.Deserialize<Tuple<byte>>(rawData).Item1;
        var player = Helpers.playerById(playerId);
        if (player == null) return;
        Arsonist.Instance.DousedPlayers.Add(player);
    }
}