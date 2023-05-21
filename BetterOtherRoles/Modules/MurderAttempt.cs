using System;
using BetterOtherRoles.EnoFw;
using BetterOtherRoles.Players;
using BetterOtherRoles.EnoFw.Libs.Reactor.Networking.Attributes;

namespace BetterOtherRoles.Modules;

public static class MurderAttempt
{
    public static void ShowFailedMurderAttempt(byte murderId, byte targetId)
    {
        var data = new Tuple<byte, byte>(murderId, targetId);
        Rpc_ShowFailedMurderAttempt(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }
    
    [MethodRpc((uint)Rpc.Module.ShowFailedMurderAttempt)]
    private static void Rpc_ShowFailedMurderAttempt(PlayerControl sender, string rawData)
    {
        if (CachedPlayer.LocalPlayer == null) return;
        var (murderId, targetId) = Rpc.Deserialize<Tuple<byte, byte>>(rawData);
        if (CachedPlayer.LocalPlayer.PlayerId != murderId) return;
        Helpers.playerById(targetId)?.ShowFailedMurder();
    }
}