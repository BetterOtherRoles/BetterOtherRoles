using TheOtherRoles.EnoFw.Kernel;
using UnityEngine;
using Option = TheOtherRoles.EnoFw.Kernel.CustomOption;

namespace TheOtherRoles.EnoFw.Roles.Neutral;

public class Jester : AbstractRole
{
    public static readonly Jester Instance = new();
    
    // Fields
    public bool TriggerJesterWin;
    
    // Options
    public readonly Option CanCallEmergency;
    public readonly Option HasImpostorVision;

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