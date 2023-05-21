using System;
using BetterOtherRoles.EnoFw.Kernel;
using Reactor.Networking.Attributes;
using UnityEngine;
using Option = BetterOtherRoles.EnoFw.Kernel.CustomOption;

namespace BetterOtherRoles.EnoFw.Roles.Impostor;

public class Morphling : AbstractRole
{
    public static readonly Morphling Instance = new();
    
    // Fields
    public PlayerControl SampledTarget;
    public PlayerControl MorphTarget;
    public float MorphTimer;
    
    // Options
    public readonly CustomOption MorphCooldown;
    public readonly CustomOption MorphDuration;

    public static Sprite SampleButtonSprite => GetSprite("BetterOtherRoles.Resources.SampleButton.png", 115f);
    public static Sprite MorphButtonSprite => GetSprite("BetterOtherRoles.Resources.MorphButton.png", 115f);

    private Morphling() : base(nameof(Morphling), "Morphling")
    {
        Team = Teams.Impostor;
        Color = Palette.ImpostorRed;
        CanTarget = true;
        
        SpawnRate = GetDefaultSpawnRateOption();
        
        MorphCooldown = Tab.CreateFloatList(
            $"{Key}{nameof(MorphCooldown)}",
            Cs("Morph cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        MorphDuration = Tab.CreateFloatList(
            $"{Key}{nameof(MorphDuration)}",
            Cs("Morph duration"),
            5f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
    }

    public void ResetMorph()
    {
        MorphTarget = null;
        MorphTimer = 0f;
        if (Player == null) return;
        Player.setDefaultLook();
    }

    public override void ClearAndReload()
    {
        ResetMorph();
        base.ClearAndReload();
        SampledTarget = null;
    }

    public static void MorphlingMorph(byte targetId)
    {
        var data = new Tuple<byte>(targetId);
        Rpc_MorphlingMorph(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.MorphlingMorph)]
    private static void Rpc_MorphlingMorph(PlayerControl sender, string rawData)
    {
        var targetId = Rpc.Deserialize<Tuple<byte>>(rawData).Item1;
        var target = Helpers.playerById(targetId);
        if (Instance.Player == null || target == null) return;
        Instance.MorphTimer = Instance.MorphDuration;
        Instance.MorphTarget = target;
        if (Camouflager.Instance.CamouflageTimer <= 0f)
        {
            Instance.Player.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId,
                target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId,
                target.Data.DefaultOutfit.PetId);
        }
    }
}