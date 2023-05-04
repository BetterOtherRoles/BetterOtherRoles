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

    [MethodRpc((uint)CustomRpc.UndertakerDropBody)]
    public static void DropBody(PlayerControl sender, string rawData)
    {
        if (undertaker == null || draggedBody == null) return;
        draggedBody.transform.position = Vector3Extensions.Deserialize(rawData);
        draggedBody = null;
        LastDragged = DateTime.UtcNow;
    }

    [MethodRpc((uint)CustomRpc.UndertakerDragBody)]
    public static void DragBody(PlayerControl sender, string rawData)
    {
        if (undertaker == null) return;
        var playerId = byte.Parse(rawData);
        var body = UnityEngine.Object.FindObjectsOfType<DeadBody>().FirstOrDefault(b => b.ParentId == playerId);
        if (body == null) return;
        draggedBody = body;
    }
}