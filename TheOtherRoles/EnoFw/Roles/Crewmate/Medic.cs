using System;
using Reactor.Networking.Attributes;
using TheOtherRoles.Players;
using UnityEngine;

namespace TheOtherRoles.EnoFw.Roles.Crewmate;

public static class Medic
{
    public static PlayerControl medic;
    public static PlayerControl shielded;
    public static PlayerControl futureShielded;

    public static Color color = new Color32(126, 251, 194, byte.MaxValue);
    public static bool usedShield;

    public static int showShielded;
    public static bool showAttemptToShielded;
    public static bool showAttemptToMedic;
    public static bool setShieldAfterMeeting;
    public static bool showShieldAfterMeeting;
    public static bool meetingAfterShielding;

    public static Color shieldedColor = new Color32(0, 221, 255, byte.MaxValue);
    public static PlayerControl currentTarget;

    private static Sprite buttonSprite;

    public static Sprite getButtonSprite()
    {
        if (buttonSprite) return buttonSprite;
        buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.ShieldButton.png", 115f);
        return buttonSprite;
    }

    public static void clearAndReload()
    {
        medic = null;
        shielded = null;
        futureShielded = null;
        currentTarget = null;
        usedShield = false;
        showShielded = CustomOptionHolder.medicShowShielded.getSelection();
        showAttemptToShielded = CustomOptionHolder.medicShowAttemptToShielded.getBool();
        showAttemptToMedic = CustomOptionHolder.medicShowAttemptToMedic.getBool();
        setShieldAfterMeeting = CustomOptionHolder.medicSetOrShowShieldAfterMeeting.getSelection() == 2;
        showShieldAfterMeeting = CustomOptionHolder.medicSetOrShowShieldAfterMeeting.getSelection() == 1;
        meetingAfterShielding = false;
    }

    public static void MedicSetShielded(byte targetId)
    {
        var data = new Tuple<byte>(targetId);
        Rpc_MedicSetShielded(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.MedicSetShielded)]
    private static void Rpc_MedicSetShielded(PlayerControl sender, string rawData)
    {
        var targetId = Rpc.Deserialize<Tuple<byte>>(rawData).Item1;
        var target = Helpers.playerById(targetId);
        if (target == null) return;
        usedShield = true;
        shielded = target;
        futureShielded = null;
    }

    public static void SetFutureShielded(byte targetId)
    {
        var data = new Tuple<byte>(targetId);
        Rpc_SetFutureShielded(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.SetFutureShielded)]
    private static void Rpc_SetFutureShielded(PlayerControl sender, string rawData)
    {
        var targetId = Rpc.Deserialize<Tuple<byte>>(rawData).Item1;
        var target = Helpers.playerById(targetId);
        if (target == null) return;
        usedShield = true;
        futureShielded = target;
    }

    public static void ShieldedMurderAttempt()
    {
        Rpc_ShieldedMurderAttempt(PlayerControl.LocalPlayer);
    }

    [MethodRpc((uint)Rpc.Role.ShieldedMurderAttempt)]
    private static void Rpc_ShieldedMurderAttempt(PlayerControl sender)
    {
        if (shielded == null || medic == null) return;

        var isShieldedAndShow =
            shielded == CachedPlayer.LocalPlayer.PlayerControl && showAttemptToShielded;
        isShieldedAndShow =
            isShieldedAndShow &&
            (meetingAfterShielding ||
             !showShieldAfterMeeting); // Dont show attempt, if shield is not shown yet
        var isMedicAndShow = medic == CachedPlayer.LocalPlayer.PlayerControl && showAttemptToMedic;

        if (isShieldedAndShow || isMedicAndShow || Helpers.shouldShowGhostInfo())
            Helpers.showFlash(Palette.ImpostorRed, duration: 0.5f, "Failed Murder Attempt on Shielded Player");
    }
}