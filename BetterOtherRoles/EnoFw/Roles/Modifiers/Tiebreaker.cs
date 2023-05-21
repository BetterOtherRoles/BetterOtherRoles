using BetterOtherRoles.EnoFw.Kernel;
using Reactor.Networking.Attributes;
using UnityEngine;

namespace BetterOtherRoles.EnoFw.Roles.Modifiers;

public class Tiebreaker : AbstractSimpleModifier
{
    public static readonly Tiebreaker Instance = new();

    public bool IsTiebreak;

    private Tiebreaker() : base(nameof(Tiebreaker), "Tiebreaker", Color.yellow)
    {
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        IsTiebreak = false;
    }

    public static void SetTiebreak()
    {
        Rpc_SetTiebreak(PlayerControl.LocalPlayer);
    }

    [MethodRpc((uint)Rpc.Role.SetTiebreak)]
    private static void Rpc_SetTiebreak(PlayerControl sender)
    {
        Instance.IsTiebreak = true;
    }
}