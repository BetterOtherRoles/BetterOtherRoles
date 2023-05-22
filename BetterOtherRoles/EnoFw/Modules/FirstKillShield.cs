using BetterOtherRoles.EnoFw.Kernel;

namespace BetterOtherRoles.EnoFw.Modules;

public static class FirstKillShield
{
    public static void SetFirstKill(byte playerId)
    {
        RpcManager.Instance.Send((uint)Rpc.Module.SetFirstKill, playerId);
    }
    
    [BindRpc((uint)Rpc.Module.SetFirstKill)]
    public static void Rpc_SetFirstKill(byte playerId)
    {
        var target = Helpers.playerById(playerId);
        if (target == null) return;
        TORMapOptions.firstKillPlayer = target;
    }
}