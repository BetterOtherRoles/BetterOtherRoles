using TheOtherRoles.EnoFw.Kernel;
using TheOtherRoles.EnoFw.Roles.Neutral;
using TheOtherRoles.EnoFw.Utils;
using UnityEngine;
using Option = TheOtherRoles.EnoFw.Kernel.CustomOption;

namespace TheOtherRoles.EnoFw.Roles.Modifiers;

public class Lovers : AbstractSimpleModifier
{
    public static readonly Lovers Instance = new();

    public PlayerControl Lover1;
    public PlayerControl Lover2;

    // Lovers save if next to be exiled is a lover, because RPC of ending game comes before RPC of exiled
    public bool NotAckedExiledIsLover;

    public readonly Option ImpostorRate;
    public readonly Option BothDie;
    public readonly Option EnableChat;

    private Lovers() : base(nameof(Lovers), "Lovers", new Color32(232, 57, 185, byte.MaxValue))
    {
        ImpostorRate = CustomOptions.ModifierSettings.CreateFloatList(
            $"{Key}{nameof(ImpostorRate)}",
            Colors.Cs(Color, "Chance that one Lover is impostor"),
            0f,
            100f,
            0f,
            10f,
            SpawnRate,
            string.Empty,
            "%");
        BothDie = CustomOptions.ModifierSettings.CreateBool(
            $"{Key}{nameof(BothDie)}",
            Colors.Cs(Color, "Both lovers die"),
            false,
            SpawnRate);
        EnableChat = CustomOptions.ModifierSettings.CreateBool(
            $"{Key}{nameof(EnableChat)}",
            Colors.Cs(Color, "Enable Lover chat"),
            false,
            SpawnRate);
    }

    public bool Existing => Lover1 != null && Lover2 != null && !Lover1.Data.Disconnected && !Lover2.Data.Disconnected;
    public bool ExistingAndAlive => Existing && !Lover1.Data.IsDead && !Lover2.Data.IsDead && !NotAckedExiledIsLover;

    public PlayerControl OtherLover(PlayerControl oneLover)
    {
        if (!ExistingAndAlive) return null;
        if (oneLover == Lover1) return Lover2;
        return oneLover == Lover2 ? Lover1 : null;
    }

    public bool ExistingWithKiller => Existing && (Lover1 == Jackal.Instance.Player ||
                                                   Lover2 == Jackal.Instance.Player ||
                                                   Lover1 == Sidekick.Instance.Player ||
                                                   Lover2 == Sidekick.Instance.Player || Lover1.Data.Role.IsImpostor ||
                                                   Lover2.Data.Role.IsImpostor);

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        Lover1 = null;
        Lover2 = null;
        NotAckedExiledIsLover = false;
    }

    public override bool Is(PlayerControl player)
    {
        return Existing && player != null && Is(player.PlayerId);
    }

    public override bool Is(byte playerId)
    {
        return Existing && (playerId == Lover1.PlayerId || playerId == Lover2.PlayerId);
    }
}

public static class LoverPlayerControlExtension
{
    public static PlayerControl GetPartner(this PlayerControl player)
    {
        if (player == null)
            return null;
        if (Lovers.Instance.Lover1 == player)
            return Lovers.Instance.Lover2;
        return Lovers.Instance.Lover2 == player ? Lovers.Instance.Lover1 : null;
    }
    
    public static bool HasAliveKillingLover(this PlayerControl player)
    {
        if (!Lovers.Instance.ExistingAndAlive || !Lovers.Instance.ExistingWithKiller) return false;
        return player != null && (player == Lovers.Instance.Lover1 || player == Lovers.Instance.Lover2);
    }
}