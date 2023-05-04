using System;
using Reactor.Networking.Attributes;
using TheOtherRoles.EnoFw.Roles.Crewmate;
using TheOtherRoles.EnoFw.Roles.Neutral;

namespace TheOtherRoles.EnoFw.Modules;

public static class CommonRpc
{
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
}