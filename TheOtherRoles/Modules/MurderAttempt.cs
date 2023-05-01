using System.Linq;
using Reactor.Networking.Attributes;
using TheOtherRoles.Customs;
using TheOtherRoles.Players;

namespace TheOtherRoles.Modules;

public static class MurderAttempt
{
    [MethodRpc((uint)Rpc.Id.ShowFailedMurderAttempt)]
    public static void ShowFailedMurderAttempt(PlayerControl sender, string rawData)
    {
        if (CachedPlayer.LocalPlayer == null) return;
        var data = rawData.Split("|").Select(byte.Parse).ToArray();
        var murderId = data[0];
        var targetId = data[1];
        if (CachedPlayer.LocalPlayer.PlayerId != murderId) return;
        Helpers.playerById(targetId)?.ShowFailedMurder();
    }
}