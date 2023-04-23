﻿using System.Collections.Generic;
using AmongUs.Data;
using TheOtherRoles.Customs.Roles.Impostor;
using UnityEngine;

namespace TheOtherRoles.Customs.Modifiers;

public static class Chameleon
{
    public static List<PlayerControl> chameleon = new List<PlayerControl>();
    public static float minVisibility = 0.2f;
    public static float holdDuration = 1f;
    public static float fadeDuration = 0.5f;
    public static Dictionary<byte, float> lastMoved;

    public static void clearAndReload()
    {
        chameleon = new List<PlayerControl>();
        lastMoved = new Dictionary<byte, float>();
        holdDuration = CustomOptionHolder.modifierChameleonHoldDuration.getFloat();
        fadeDuration = CustomOptionHolder.modifierChameleonFadeDuration.getFloat();
        minVisibility = CustomOptionHolder.modifierChameleonMinVisibility.getSelection() / 10f;
    }

    public static float visibility(byte playerId)
    {
        float visibility = 1f;
        if (lastMoved != null && lastMoved.ContainsKey(playerId))
        {
            var tStill = Time.time - lastMoved[playerId];
            if (tStill > holdDuration)
            {
                if (tStill - holdDuration > fadeDuration) visibility = minVisibility;
                else visibility = (1 - (tStill - holdDuration) / fadeDuration) * (1 - minVisibility) + minVisibility;
            }
        }

        if (PlayerControl.LocalPlayer.Data.IsDead && visibility < 0.1f)
        {
            // Ghosts can always see!
            visibility = 0.1f;
        }

        return visibility;
    }

    public static void update()
    {
        foreach (var chameleonPlayer in chameleon)
        {
            if (chameleonPlayer == Ninja.ninja && Ninja.isInvisble) continue; // Dont make Ninja visible...
            // check movement by animation
            PlayerPhysics playerPhysics = chameleonPlayer.MyPhysics;
            var currentPhysicsAnim = playerPhysics.Animations.Animator.GetCurrentAnimation();
            if (currentPhysicsAnim != playerPhysics.Animations.group.IdleAnim)
            {
                lastMoved[chameleonPlayer.PlayerId] = Time.time;
            }

            // calculate and set visibility
            float visibility = Chameleon.visibility(chameleonPlayer.PlayerId);
            float petVisibility = visibility;
            if (chameleonPlayer.Data.IsDead)
            {
                visibility = 0.5f;
                petVisibility = 1f;
            }

            try
            {
                // Sometimes renderers are missing for weird reasons. Try catch to avoid exceptions
                chameleonPlayer.cosmetics.currentBodySprite.BodySprite.color =
                    chameleonPlayer.cosmetics.currentBodySprite.BodySprite.color.SetAlpha(visibility);
                if (DataManager.Settings.Accessibility.ColorBlindMode)
                    chameleonPlayer.cosmetics.colorBlindText.color =
                        chameleonPlayer.cosmetics.colorBlindText.color.SetAlpha(visibility);
                chameleonPlayer.SetHatAndVisorAlpha(visibility);
                chameleonPlayer.cosmetics.skin.layer.color =
                    chameleonPlayer.cosmetics.skin.layer.color.SetAlpha(visibility);
                chameleonPlayer.cosmetics.colorBlindText.color =
                    chameleonPlayer.cosmetics.skin.layer.color.SetAlpha(visibility);
                chameleonPlayer.cosmetics.nameText.color =
                    chameleonPlayer.cosmetics.nameText.color.SetAlpha(visibility);
                chameleonPlayer.cosmetics.currentPet.rend.color =
                    chameleonPlayer.cosmetics.currentPet.rend.color.SetAlpha(petVisibility);
                chameleonPlayer.cosmetics.currentPet.shadowRend.color =
                    chameleonPlayer.cosmetics.currentPet.shadowRend.color.SetAlpha(petVisibility);
            }
            catch
            {
            }
        }
    }
}