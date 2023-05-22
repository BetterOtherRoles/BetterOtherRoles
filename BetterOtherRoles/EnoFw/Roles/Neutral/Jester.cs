using BetterOtherRoles.EnoFw.Kernel;
using UnityEngine;
using Option = BetterOtherRoles.EnoFw.Kernel.CustomOption;

namespace BetterOtherRoles.EnoFw.Roles.Neutral;

public class Jester : AbstractRole
{
    public static readonly Jester Instance = new();
    
    // Fields
    public bool TriggerJesterWin;
    
    // Options
    public readonly CustomOption CanCallEmergency;
    public readonly CustomOption HasImpostorVision;

    private Jester() : base(nameof(Jester), "Jester")
    {
        Team = Teams.Neutral;
        Color = new Color32(236, 98, 165, byte.MaxValue);
        
        SpawnRate = GetDefaultSpawnRateOption();

        CanCallEmergency = Tab.CreateBool(
            $"{Key}{nameof(CanCallEmergency)}",
            Cs("Can call emergency meeting"),
            true,
            SpawnRate);
        HasImpostorVision = Tab.CreateBool(
            $"{Key}{nameof(HasImpostorVision)}",
            Cs("Has impostor vision"),
            true,
            SpawnRate);
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        TriggerJesterWin = false;
    }
}