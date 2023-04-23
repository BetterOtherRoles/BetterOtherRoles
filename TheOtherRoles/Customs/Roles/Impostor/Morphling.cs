using Reactor.Networking.Attributes;
using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.EnoFramework.Utils;
using TheOtherRoles.Objects;
using TheOtherRoles.Players;
using UnityEngine;
using Resources = TheOtherRoles.EnoFramework.Utils.Resources;

namespace TheOtherRoles.Customs.Roles.Impostor;

[EnoSingleton]
public class Morphling : CustomRole {
    private Sprite? _sampleSprite;
    private Sprite? _morphSprite;
    
    private CustomButton? _morphlingButton;
    
    private PoolablePlayer? _targetDisplay;

    public readonly EnoFramework.CustomOption MorphCooldown;
    public readonly EnoFramework.CustomOption MorphDuration;
    
    public byte? SampledTargetId;
    public byte? MorphTargetId;
    public float MorphTimer;

    public Morphling(): base(nameof(Morphling))
    {
        Team = Teams.Impostor;
        Color = Palette.ImpostorRed;
        CanTarget = true;
        
        MorphCooldown = OptionsTab.CreateFloatList(
            $"{Name}{nameof(MorphCooldown)}",
            Colors.Cs(Color, $"{Name} cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        MorphDuration = OptionsTab.CreateFloatList(
            $"{Name}{nameof(MorphDuration)}",
            Colors.Cs(Color, $"{Name} duration"),
            5f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
    }

    private Sprite GetSampleSprite()
    {
        if (_sampleSprite == null)
        {
            _sampleSprite = Resources.LoadSpriteFromResources("TheOtherRoles.Resources.SampleButton.png", 115f);
        }
        return _sampleSprite;
    }

    private Sprite GetMorphSprite()
    {
        if (_morphSprite == null)
        {
            _morphSprite = Resources.LoadSpriteFromResources("TheOtherRoles.Resources.MorphButton.png", 115f);
        }

        return _morphSprite;
    }
    
    public override void CreateCustomButtons(HudManager hudManager)
    {
        base.CreateCustomButtons(hudManager);
        _morphlingButton = new CustomButton(
            OnMorphlingButtonClick,
            HasMorphlingButton,
            CouldUseMorphlingButton,
            ResetMorphlingButton,
            GetSampleSprite(),
            CustomButton.ButtonPositions.upperRowLeft,
            hudManager,
            "ActionQuaternary",
            true,
            MorphDuration,
            OnMorphlingButtonEffectEnd
        );
    }

    private void OnMorphlingButtonEffectEnd()
    {
        if (_morphlingButton == null || !HasMorphlingButton()) return;
        if (SampledTargetId != null) return;
        _morphlingButton.Timer = _morphlingButton.MaxTimer;
        _morphlingButton.Sprite = GetSampleSprite();
        SoundEffectsManager.play("morphlingMorph");
        SetButtonTargetDisplay(null);
    }

    private void ResetMorphlingButton()
    {
        if (_morphlingButton == null) return;
        _morphlingButton.Timer = _morphlingButton.MaxTimer;
        _morphlingButton.Sprite = GetSampleSprite();
        _morphlingButton.isEffectActive = false;
        _morphlingButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
        if (!HasMorphlingButton()) return;
        SampledTargetId = null;
        SetButtonTargetDisplay(null);
    }

    private bool CouldUseMorphlingButton()
    {
        return HasMorphlingButton() && (CurrentTarget != null || SampledTargetId != null);
    }

    private bool HasMorphlingButton()
    {
        return _morphlingButton != null && !CachedPlayer.LocalPlayer.Data.IsDead && Is(CachedPlayer.LocalPlayer);
    }

    private void OnMorphlingButtonClick()
    {
        if (_morphlingButton == null) return;
        if (!Is(CachedPlayer.LocalPlayer)) return;
        if (SampledTargetId != null)
        {
            MorphlingMorph(CachedPlayer.LocalPlayer, $"{SampledTargetId}");
            _morphlingButton.EffectDuration = MorphDuration;
            SoundEffectsManager.play("morphlingMorph");
        }
        else if (CurrentTarget != null)
        {
            SampledTargetId = CurrentTarget.PlayerId;
            _morphlingButton.Sprite = GetMorphSprite();
            _morphlingButton.EffectDuration = 1f;
            SoundEffectsManager.play("morphlingSample");
            
            // Add poolable player to the button so that the target outfit is shown
            var sampledTargetId = SampledTargetId;
            if (sampledTargetId != null)
                SetButtonTargetDisplay(Helpers.playerById(sampledTargetId.Value), _morphlingButton);
        }
    }
    
    public void ResetMorph()
    {
        if (MorphTargetId != null)
        {
            var player = Helpers.playerById(MorphTargetId.Value);
            if (player != null)
            {
                player.setDefaultLook();
            }
        }
        MorphTargetId = null;
        MorphTimer = 0f;
    }

    public override void ClearAndReload()
    {
        ResetMorph();
        base.ClearAndReload();
    }
    
    [MethodRpc((uint) Rpc.Id.MorphlingMorph)]
    private static void MorphlingMorph(PlayerControl sender, string rawData)
    {
        if (Singleton<Morphling>.Instance.Player == null) return;
        var targetId = byte.Parse(rawData);
        var target = Helpers.playerById(targetId);
        if (Singleton<Camouflager>.Instance.CamouflagerTimer <= 0f)
        {
            Singleton<Morphling>.Instance.Player.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId);
        }
    }
    
    private void SetButtonTargetDisplay(PlayerControl? target, CustomButton? button = null, Vector3? offset = null) {
        if (target == null || button == null) {
            if (_targetDisplay == null) return; // Reset the poolable player
            GameObject gameObject;
            (gameObject = _targetDisplay.gameObject).SetActive(false);
            Object.Destroy(gameObject);
            _targetDisplay = null;
            return;
        }
        // Add poolable player to the button so that the target outfit is shown
        button.actionButton.cooldownTimerText.transform.localPosition = new Vector3(0, 0, -1f);  // Before the poolable player
        _targetDisplay = Object.Instantiate(Patches.IntroCutsceneOnDestroyPatch.playerPrefab, button.actionButton.transform);
        var data = target.Data;
        target.SetPlayerMaterialColors(_targetDisplay.cosmetics.currentBodySprite.BodySprite);
        _targetDisplay.SetSkin(data.DefaultOutfit.SkinId, data.DefaultOutfit.ColorId);
        _targetDisplay.SetHat(data.DefaultOutfit.HatId, data.DefaultOutfit.ColorId);
        _targetDisplay.cosmetics.nameText.text = "";  // Hide the name!
        _targetDisplay.transform.localPosition = new Vector3(0f, 0.22f, -0.01f);
        if (offset != null) _targetDisplay.transform.localPosition += (Vector3)offset;
        _targetDisplay.transform.localScale = Vector3.one * 0.33f;
        _targetDisplay.setSemiTransparent(false);
        _targetDisplay.gameObject.SetActive(true);
    }
}