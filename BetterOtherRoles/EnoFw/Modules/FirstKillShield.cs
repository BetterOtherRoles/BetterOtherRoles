using System;
using Reactor.Networking.Attributes;

namespace BetterOtherRoles.EnoFw.Modules;

public static class FirstKillShield
{
    public static void SetFirstKill(byte playerId)
    {
        var data = new Tuple<byte>(playerId);
        Rpc_SetFirstKill(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }
    
    [MethodRpc((uint)Rpc.Module.SetFirstKill)]
    private static void Rpc_SetFirstKill(PlayerControl sender, string rawData)
    {
        var playerId = Rpc.Deserialize<Tuple<byte>>(rawData).Item1;
        var target = Helpers.playerById(playerId);
        if (target == null) return;
        TORMapOptions.firstKillPlayer = target;
    }
}