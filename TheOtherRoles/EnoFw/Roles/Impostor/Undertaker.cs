using System;
using System.Linq;
using Reactor.Networking.Attributes;
using TheOtherRoles.Modules;
using UnityEngine;

namespace TheOtherRoles.EnoFw.Roles.Impostor;

public static class Undertaker
{
    public enum DragDistance
    {
        Short,
        Medium,
        Long
    }

    public static PlayerControl undertaker;
    public static Color color = Palette.ImpostorRed;

    public static float dragSpeedModifier = 9f;
    public static float cooldown = 20f;
    public static DragDistance dragDistance = DragDistance.Short;
    public static bool disableKillButton = true;
    public static bool disableReportButton = true;
    public static bool disableVentButton = true;

    public static float[] distancesList = new float[] { (2f / 3f), (4f / 3f), 2f };


    public static DateTime LastDragged { get; set; }
    public static DeadBody currentDeadTarget;
    public static DeadBody draggedBody;

    private static Sprite dragSprite;
    private static Sprite dropSprite;

    public static Sprite getDragButtonSprite()
    {
        if (dragSprite) return dragSprite;
        dragSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.DragButton.png", 115f);
        return dragSprite;
    }

    public static Sprite getDropButtonSprite()
    {
        if (dropSprite) return dropSprite;
        dropSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.DropButton.png", 115f);
        return dropSprite;
    }

    public static void clearAndReload()
    {
        undertaker = null;
        currentDeadTarget = null;
        draggedBody = null;
        dragSpeedModifier = (float)CustomOptionHolder.undertakerDragSpeedModifier.getSelection();
        cooldown = CustomOptionHolder.undertakerAbilityCooldown.getFloat();
        dragDistance = (DragDistance)CustomOptionHolder.undertakerDragDistance.getSelection();
        disableKillButton = CustomOptionHolder.undertakerDisableKillButtonWhileDragging.getBool();
        disableReportButton = CustomOptionHolder.undertakerDisableReportButtonWhileDragging.getBool();
        disableVentButton = CustomOptionHolder.undertakerDisableVentButtonWhileDragging.getBool();
    }

    public static void DropBody(float x, float y)
    {
        var data = new Tuple<float, float>(x, y);
        Rpc_DropBody(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.UndertakerDropBody)]
    private static void Rpc_DropBody(PlayerControl sender, string rawData)
    {
        if (undertaker == null || draggedBody == null) return;
        var (x, y) = Rpc.Deserialize<Tuple<float, float>>(rawData);
        var transform = draggedBody.transform;
        var position = new Vector3(x, y, transform.position.z);
        transform.position = position;
        draggedBody = null;
        LastDragged = DateTime.UtcNow;
    }

    public static void DragBody(byte playerId)
    {
        var data = new Tuple<byte>(playerId);
        Rpc_DragBody(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.UndertakerDragBody)]
    private static void Rpc_DragBody(PlayerControl sender, string rawData)
    {
        if (undertaker == null) return;
        var playerId = Rpc.Deserialize<Tuple<byte>>(rawData).Item1;
        var body = UnityEngine.Object.FindObjectsOfType<DeadBody>().FirstOrDefault(b => b.ParentId == playerId);
        if (body == null) return;
        draggedBody = body;
    }
}