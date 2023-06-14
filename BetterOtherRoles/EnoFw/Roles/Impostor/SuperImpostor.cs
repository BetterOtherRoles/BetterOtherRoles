using System.Collections.Generic;
using BetterOtherRoles.EnoFw.Kernel;
using UnityEngine;

namespace BetterOtherRoles.EnoFw.Roles.Impostor;

public class SuperImpostor : AbstractRole
{
    public static readonly SuperImpostor Instance = new();

    // Fields
    public bool ChooseAllRole => (string)ChooseMethod is "all role";
    public bool ChooseNonTakenRole => (string)ChooseMethod is "non taken role";

    // Options
    public readonly CustomOption ChooseMethod;
    
    public static Sprite ChooseAllySprite => GetSprite("BetterOtherRoles.Resources.SampleButton.png", 115f);
    
    private SuperImpostor() : base(nameof(SuperImpostor), "SuperImpostor")
    {
        Team = Teams.Impostor;
        Color = Palette.ImpostorRed;
        CanTarget = false;
        
        
        SpawnRate = GetDefaultSpawnRateOption();

        ChooseMethod = Tab.CreateStringList(
            $"{Key}{nameof(ChooseMethod)}",
            Cs($"Choose Role Method"),
            new List<string> { "all role", "non taken role" },
            "all role",
            SpawnRate);
    }
}