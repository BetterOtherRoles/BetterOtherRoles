using System;
using System.Collections.Generic;
using Reactor.Networking.Attributes;

namespace TheOtherRoles.EnoFw.Roles.Modifiers;

public static class Bloody
{
    public static List<PlayerControl> bloody = new();
    public static Dictionary<byte, float> active = new();
    public static Dictionary<byte, byte> bloodyKillerMap = new();

    public static float duration = 5f;

    public static void clearAndReload()
    {
        bloody.Clear();
        active.Clear();
        bloodyKillerMap.Clear();
        duration = CustomOptionHolder.modifierBloodyDuration.getFloat();
    }

    public static void SetBloody(byte killerPlayerId, byte bloodyPlayerId)
    {
        var data = new Tuple<byte, byte>(killerPlayerId, bloodyPlayerId);
        Rpc_SetBloody(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.SetBloody)]
    private static void Rpc_SetBloody(PlayerControl sender, string rawData)
    {
        var (killerPlayerId, bloodyPlayerId) = Rpc.Deserialize<Tuple<byte, byte>>(rawData);
        if (active.ContainsKey(killerPlayerId)) return;
        active.Add(killerPlayerId, duration);
        bloodyKillerMap.Add(killerPlayerId, bloodyPlayerId);
    }
}