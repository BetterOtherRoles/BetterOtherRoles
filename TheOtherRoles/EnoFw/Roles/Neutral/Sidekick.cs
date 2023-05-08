using Reactor.Networking.Attributes;
using TheOtherRoles.EnoFw.Kernel;
using UnityEngine;
using Option = TheOtherRoles.EnoFw.Kernel.CustomOption;

namespace TheOtherRoles.EnoFw.Roles.Neutral;

public class Sidekick : AbstractRole
{
    public static readonly Sidekick Instance = new();
    
    // Fields
    public bool WasTeamRed;
    public bool WasImpostor;
    public bool WasSpy;
    
    // Options
    public readonly Option CanKill;
    public readonly Option CanUseVents;
    public Option PromotesToJackal => Jackal.Instance.SidekickPromoteToJackal;
    public Option HasImpostorVision => Jackal.Instance.HasImpostorVision;

    private Sidekick() : base(nameof(Sidekick), "Sidekick", false)
    {
        Team = Teams.Neutral;
        Color = new Color32(0, 180, 235, byte.MaxValue);
        CanTarget = true;

        CanKill = Tab.CreateBool(
            $"{Key}{nameof(CanKill)}",
            Cs("Sidekick can kill"),
            false,
            Jackal.Instance.SpawnRate);
        CanUseVents = Tab.CreateBool(
            $"{Key}{nameof(CanUseVents)}",
            Cs("Sidekick can use vents"),
            false,
            Jackal.Instance.SpawnRate);
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        WasTeamRed = false;
        WasImpostor = false;
        WasSpy = false;
    }

    public static void SidekickPromotes()
    {
        Rpc_SidekickPromotes(PlayerControl.LocalPlayer);
    }

    [MethodRpc((uint)Rpc.Role.SidekickPromotes)]
    private static void Rpc_SidekickPromotes(PlayerControl sender)
    {
        Local_SidekickPromotes();
    }

    public static void Local_SidekickPromotes()
    {
        Jackal.Instance.RemoveCurrentJackal();
        Jackal.Instance.Player = Instance.Player;
        Jackal.Instance.CanCreateSidekick = Jackal.Instance.PromotedFromSidekickCanCreateSidekick;
        Jackal.Instance.WasTeamRed = Instance.WasTeamRed;
        Jackal.Instance.WasSpy = Instance.WasSpy;
        Jackal.Instance.WasImpostor = Instance.WasImpostor;
        Instance.ClearAndReload();
    }
}