using System;
using Reactor.Networking.Attributes;
using UnityEngine;

namespace TheOtherRoles.EnoFw.Roles.Impostor;

public static class Morphling
{
    public static PlayerControl morphling;
    public static Color color = Palette.ImpostorRed;
    private static Sprite sampleSprite;
    private static Sprite morphSprite;

    public static float cooldown = 30f;
    public static float duration = 10f;

    public static PlayerControl currentTarget;
    public static PlayerControl sampledTarget;
    public static PlayerControl morphTarget;
    public static float morphTimer = 0f;

    public static void resetMorph()
    {
        morphTarget = null;
        morphTimer = 0f;
        if (morphling == null) return;
        morphling.setDefaultLook();
    }

    public static void clearAndReload()
    {
        resetMorph();
        morphling = null;
        currentTarget = null;
        sampledTarget = null;
        morphTarget = null;
        morphTimer = 0f;
        cooldown = CustomOptionHolder.morphlingCooldown.getFloat();
        duration = CustomOptionHolder.morphlingDuration.getFloat();
    }

    public static Sprite getSampleSprite()
    {
        if (sampleSprite) return sampleSprite;
        sampleSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.SampleButton.png", 115f);
        return sampleSprite;
    }

    public static Sprite getMorphSprite()
    {
        if (morphSprite) return morphSprite;
        morphSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.MorphButton.png", 115f);
        return morphSprite;
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
        if (morphling == null || target == null) return;
        morphTimer = duration;
        morphTarget = target;
        if (Camouflager.camouflageTimer <= 0f)
        {
            morphling.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId,
                target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId,
                target.Data.DefaultOutfit.PetId);
        }
    }
}