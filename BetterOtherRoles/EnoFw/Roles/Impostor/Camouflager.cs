using System.Linq;
using BetterOtherRoles.EnoFw.Kernel;
using BetterOtherRoles.Players;
using UnityEngine;
using Option = BetterOtherRoles.EnoFw.Kernel.CustomOption;

namespace BetterOtherRoles.EnoFw.Roles.Impostor;

public class Camouflager : AbstractRole
{
    public static readonly Camouflager Instance = new();
    
    // Fields
    public float CamouflageTimer;
    
    // Options
    public readonly CustomOption CamoCooldown;
    public readonly CustomOption CamoDuration;

    public static Sprite CamouflageButtonSprite => GetSprite("BetterOtherRoles.Resources.CamoButton.png", 115f);

    private Camouflager() : base(nameof(Camouflager), "Camouflager")
    {
        Team = Teams.Impostor;
        Color = Palette.ImpostorRed;
        CanTarget = true;
        
        SpawnRate = GetDefaultSpawnRateOption();
        
        CamoCooldown = Tab.CreateFloatList(
            $"{Key}{nameof(Key)}",
            Cs($"{Name} cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        CamoDuration = Tab.CreateFloatList(
            $"{Key}{nameof(CamoDuration)}",
            Cs($"{Key} duration"),
            5f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
    }

    public void ResetCamouflage()
    {
        CamouflageTimer = 0f;
        foreach (var p in CachedPlayer.AllPlayers.Select(p => p.PlayerControl))
        {
            if (p == Ninja.Instance.Player && Ninja.Instance.IsInvisible) continue;
            p.setDefaultLook();
        }
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        ResetCamouflage();
        CamouflageTimer = 0f;
    }

    public static void CamouflagerCamouflage()
    {
        RpcManager.Instance.Send((uint)Rpc.Role.CamouflagerCamouflage);
    }

    [BindRpc((uint)Rpc.Role.CamouflagerCamouflage)]
    public static void Rpc_CamouflagerCamouflage()
    {
        if (Instance.Player == null) return;
        Instance.CamouflageTimer = Instance.CamoDuration;
        foreach (var player in CachedPlayer.AllPlayers.Select(p => p.PlayerControl))
        {
            player.setLook("", 6, "", "", "", "");
        }
    }
}