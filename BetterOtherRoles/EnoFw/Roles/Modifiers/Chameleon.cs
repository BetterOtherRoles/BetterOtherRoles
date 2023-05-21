using System.Collections.Generic;
using AmongUs.Data;
using BetterOtherRoles.EnoFw.Kernel;
using BetterOtherRoles.EnoFw.Roles.Impostor;
using BetterOtherRoles.EnoFw.Utils;
using UnityEngine;
using Option = BetterOtherRoles.EnoFw.Kernel.CustomOption;

namespace BetterOtherRoles.EnoFw.Roles.Modifiers;

public class Chameleon : AbstractMultipleModifier
{
    public static readonly Chameleon Instance = new();

    public readonly Dictionary<byte, float> LastMoved = new();

    public readonly CustomOption HoldDuration;
    public readonly CustomOption FadeDuration;
    public readonly CustomOption MinVisibilityOption;
    private float MinVisibility => MinVisibilityOption / 100f;

    private Chameleon() : base(nameof(Chameleon), "Chameleon", Color.yellow)
    {
        HoldDuration = CustomOptions.ModifierSettings.CreateFloatList(
            $"{Key}{nameof(HoldDuration)}",
            Colors.Cs(Color, "Time until fading starts"),
            1f,
            10f,
            3f,
            0.5f,
            SpawnRate,
            string.Empty,
            "s");
        FadeDuration = CustomOptions.ModifierSettings.CreateFloatList(
            $"{Key}{nameof(FadeDuration)}",
            Colors.Cs(Color, "Fade duration"),
            0.25f,
            10f,
            1f,
            0.25f,
            SpawnRate,
            string.Empty,
            "s");
        MinVisibilityOption = CustomOptions.ModifierSettings.CreateFloatList(
            $"{Key}{nameof(MinVisibilityOption)}",
            Colors.Cs(Color, "Minimum visibility"),
            0f,
            100f,
            0f,
            10f,
            SpawnRate,
            string.Empty,
            "%");
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        LastMoved.Clear();
    }

    public static float Visibility(byte playerId)
    {
        var visibility = 1f;
        if (Instance.LastMoved.TryGetValue(playerId, out var value))
        {
            var tStill = Time.time - value;
            if (tStill > Instance.HoldDuration)
            {
                if (tStill - Instance.HoldDuration > Instance.FadeDuration) visibility = Instance.MinVisibility;
                else
                    visibility =
                        (1 - (tStill - Instance.HoldDuration) / Instance.FadeDuration) * (1 - Instance.MinVisibility) +
                        Instance.MinVisibility;
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
        foreach (var chameleonPlayer in Instance.Players)
        {
            if (chameleonPlayer == Ninja.Instance.Player && Ninja.Instance.IsInvisible)
                continue; // Dont make Ninja visible...
            // check movement by animation
            PlayerPhysics playerPhysics = chameleonPlayer.MyPhysics;
            var currentPhysicsAnim = playerPhysics.Animations.Animator.GetCurrentAnimation();
            if (currentPhysicsAnim != playerPhysics.Animations.group.IdleAnim)
            {
                Instance.LastMoved[chameleonPlayer.PlayerId] = Time.time;
            }

            // calculate and set visibility
            var visibility = Chameleon.Visibility(chameleonPlayer.PlayerId);
            var petVisibility = visibility;
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