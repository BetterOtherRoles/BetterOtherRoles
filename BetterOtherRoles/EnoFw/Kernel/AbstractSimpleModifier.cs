using UnityEngine;

namespace BetterOtherRoles.EnoFw.Kernel;

public  abstract class AbstractSimpleModifier : AbstractModifier
{
    public PlayerControl Player;
    
    protected AbstractSimpleModifier(string key, string name, Color color) : base(key, name, color)
    {
        
    }

    public override bool Is(byte playerId)
    {
        return Player != null && Player.PlayerId == playerId;
    }

    public override bool Is(PlayerControl player)
    {
        return player != null && Is(player.PlayerId);
    }

    public override void ClearAndReload()
    {
        Player = null;
    }
}