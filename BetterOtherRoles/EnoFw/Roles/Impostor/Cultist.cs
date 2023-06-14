using AmongUs.GameOptions;
using BetterOtherRoles.EnoFw.Kernel;
using UnityEngine;
using Option = BetterOtherRoles.EnoFw.Kernel.CustomOption;

namespace BetterOtherRoles.EnoFw.Roles.Impostor;

public class Cultist : AbstractRole
{
    public static readonly Cultist Instance = new();
    
    // Fields
    public bool HasChosenAlly { get; private set; }
    public PlayerControl CultMember { get; private set; }
    
    // Options
    public readonly CustomOption EnableChat;

    public static Sprite ChooseAllySprite => GetSprite("BetterOtherRoles.Resources.SampleButton.png", 115f);
    

    private Cultist() : base(nameof(Cultist), "Cultist")
    {
        Team = Teams.Impostor;
        Color = Palette.ImpostorRed;
        CanTarget = true;
        
        
        SpawnRate = GetDefaultSpawnRateOption();

        EnableChat = Tab.CreateBool(
            $"{Key}{nameof(EnableChat)}",
            Cs($"Has a chat between him and his mate"),
            true,
            SpawnRate);
    }
    
    public static void SetAlly(byte playerId)
    {
        RpcManager.Instance.Send((uint)Rpc.Role.CultistSetAlly, playerId);
    }

    [BindRpc((uint)Rpc.Role.CultistSetAlly)]
    public static void Rpc_SetAlly(byte playerId)
    {
        var player = Helpers.playerById(playerId);
        
        player.clearAllTasks();
        player.SetRole(RoleTypes.Impostor);
        Instance.CultMember = player;
        
        Instance.HasChosenAlly = true;
    }
}