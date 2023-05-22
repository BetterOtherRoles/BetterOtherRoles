using BetterOtherRoles.EnoFw.Kernel;
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
        RpcManager.Instance.Send((uint)Rpc.Role.SetTiebreak);
    }

    [BindRpc((uint)Rpc.Role.SetTiebreak)]
    public static void Rpc_SetTiebreak()
    {
        Instance.IsTiebreak = true;
    }
}