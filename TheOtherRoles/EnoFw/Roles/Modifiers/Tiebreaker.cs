using Reactor.Networking.Attributes;

namespace TheOtherRoles.EnoFw.Roles.Modifiers;

public static class Tiebreaker
{
    public static PlayerControl tiebreaker;

    public static bool isTiebreak = false;

    public static void clearAndReload()
    {
        tiebreaker = null;
        isTiebreak = false;
    }

    public static void SetTiebreak()
    {
        Rpc_SetTiebreak(PlayerControl.LocalPlayer);
    }

    [MethodRpc((uint)Rpc.Role.SetTiebreak)]
    private static void Rpc_SetTiebreak(PlayerControl sender)
    {
        isTiebreak = true;
    }
}