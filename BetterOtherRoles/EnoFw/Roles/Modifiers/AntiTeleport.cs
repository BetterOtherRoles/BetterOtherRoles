using System.Collections.Generic;
using BetterOtherRoles.EnoFw.Kernel;
using BetterOtherRoles.Players;
using UnityEngine;

namespace BetterOtherRoles.EnoFw.Roles.Modifiers;

public class AntiTeleport : AbstractMultipleModifier
{
    public static readonly AntiTeleport Instance = new();

    public Vector3 Position = Vector3.zero;
    

    private AntiTeleport() : base(nameof(AntiTeleport), "Anti Teleport", Color.yellow)
    {
        
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        Position = Vector3.zero;
    }

    public void SetPosition()
    {
        // Check if this has been set, otherwise first spawn on submerged will fail
        if (Position == Vector3.zero) return;
        if (!Is(CachedPlayer.LocalPlayer.PlayerControl)) return;
        CachedPlayer.LocalPlayer.NetTransform.RpcSnapTo(Position);
        if (SubmergedCompatibility.IsSubmerged)
        {
            SubmergedCompatibility.ChangeFloor(Position.y > -7);
        }
    }
}