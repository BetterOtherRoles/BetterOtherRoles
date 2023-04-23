using System.Text.Json;
using AmongUs.Data;
using Reactor.Networking.Attributes;
using TheOtherRoles.Customs.Roles.Impostor;
using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.Patches;
using TheOtherRoles.Players;
using UnityEngine;

namespace TheOtherRoles.Customs;

public static class Rpc
{
    public enum Id : uint
    {
        // Kernel
        ShareCustomOptionsChanges,
        RoleSetPlayer,
        ClearAndReloadRoles,
        SetInvisibility,
        UncheckedMurderPlayer,
        
        // Morphling
        MorphlingMorph,
        // Camouflager
        CamouflagerCamouflage,
        // Eraser
        EraserSetFutureErased,
        // Trickster
        TricksterPlaceBox,
        TricksterLightsOut,
        // Ninja
        NinjaPlaceTrace,
        // Bomber
        BomberPlantBomb,
        BomberDefuseBomb,
        // Arsonist
        ArsonistTriggerWin,
    }

    public static void SetInvisibility(PlayerControl player, bool visible)
    {
        RpcSetInvisibility(CachedPlayer.LocalPlayer, $"{player.PlayerId}|{(visible ? 1 : 0)}");
    }

    public static void UncheckedMurderPlayer(PlayerControl source, PlayerControl target, bool showAnimation)
    {
        RpcUncheckedMurderPlayer(CachedPlayer.LocalPlayer, $"{source.PlayerId}|{target.PlayerId}|{(showAnimation ? 1 : 0)}");
    }

    [MethodRpc((uint) Id.UncheckedMurderPlayer)]
    private static void RpcUncheckedMurderPlayer(PlayerControl sender, string rawData)
    {
        var data = rawData.Split("|");
        var sourceId = byte.Parse(data[0]);
        var targetId = byte.Parse(data[1]);
        var showAnimation = int.Parse(data[2]) == 1;
        var source = Helpers.playerById(sourceId);
        var target = Helpers.playerById(targetId);
        if (source == null || target == null) return;
        if (!showAnimation) KillAnimationCoPerformKillPatch.hideNextAnimation = true;
        source.MurderPlayer(target);
    }

    [MethodRpc((uint) Id.SetInvisibility)]
    private static void RpcSetInvisibility(PlayerControl sender, string rawData)
    {
        var data = rawData.Split("|");
        var playerId = byte.Parse(data[0]);
        var visible = int.Parse(data[1]) == 1;
        var target = Helpers.playerById(playerId);
        if (target == null) return;
        if (visible)
        {
            target.cosmetics.currentBodySprite.BodySprite.color = Color.white;
            target.cosmetics.colorBlindText.gameObject.SetActive(DataManager.Settings.Accessibility.ColorBlindMode);
            target.cosmetics.colorBlindText.color = target.cosmetics.colorBlindText.color.SetAlpha(1f);
            if (Singleton<Camouflager>.Instance.CamouflagerTimer <= 0) target.setDefaultLook();
            Singleton<Ninja>.Instance.IsInvisible = false;
            return;
        }
        target.setLook("", 6, "", "", "", "");
        var color = Color.clear;           
        if (CachedPlayer.LocalPlayer.Data.Role.IsImpostor || CachedPlayer.LocalPlayer.Data.IsDead) color.a = 0.1f;
        target.cosmetics.currentBodySprite.BodySprite.color = color;
        target.cosmetics.colorBlindText.color = target.cosmetics.colorBlindText.color.SetAlpha(color.a);
        target.cosmetics.colorBlindText.gameObject.SetActive(false);
        Singleton<Ninja>.Instance.InvisibilityTimer = Singleton<Ninja>.Instance.InvisibilityDuration;
        Singleton<Ninja>.Instance.IsInvisible = true;
    }
    
    public static string Serialize<T>(T data)
    {
        return JsonSerializer.Serialize(data);
    }

    public static T Deserialize<T>(string data)
    {
        return JsonSerializer.Deserialize<T>(data) ?? throw new KernelException("Unable to deserialize string");
    }
}