using System;
using BetterOtherRoles.EnoFw;
using BetterOtherRoles.EnoFw.Kernel;
using BetterOtherRoles.Players;

namespace BetterOtherRoles.Modules;

public static class MurderAttempt
{
    public static void ShowFailedMurderAttempt(byte murderId, byte targetId)
    {
        var data = new Tuple<byte, byte>(murderId, targetId);
        RpcManager.Instance.Send((uint)Rpc.Module.ShowFailedMurderAttempt, data);
    }
    
    [BindRpc((uint)Rpc.Module.ShowFailedMurderAttempt)]
    public static void Rpc_ShowFailedMurderAttempt(Tuple<byte, byte> data)
    {
        if (CachedPlayer.LocalPlayer == null) return;
        var (murderId, targetId) = data;
        if (CachedPlayer.LocalPlayer.PlayerId != murderId) return;
        Helpers.playerById(targetId)?.ShowFailedMurder();
    }
}