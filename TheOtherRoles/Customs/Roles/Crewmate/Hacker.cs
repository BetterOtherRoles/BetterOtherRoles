using System.Linq;
using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.EnoFramework.Utils;
using TheOtherRoles.Objects;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;
using TMPro;
using UnityEngine;
using Resources = TheOtherRoles.EnoFramework.Utils.Resources;

namespace TheOtherRoles.Customs.Roles.Crewmate;

[EnoSingleton]
public class Hacker : CustomRole
{
    private Sprite? _hackSprite;
    private Sprite? _vitalsSprite;
    private Sprite? _doorLogSprite;
    private Sprite? _adminSprite;
    private byte? _lastMapId;

    private CustomButton? _hackButton;
    private CustomButton? _adminButton;
    private TMP_Text? _adminButtonText;
    private CustomButton? _vitalsButton;
    private TMP_Text? _vitalsButtonText;

    public readonly EnoFramework.CustomOption HackCooldown;
    public readonly EnoFramework.CustomOption HackingDuration;
    public readonly EnoFramework.CustomOption OnlyColorType;
    public readonly EnoFramework.CustomOption MaxGadgetCharges;
    public readonly EnoFramework.CustomOption RechargeTasksNumber;
    public readonly EnoFramework.CustomOption CanMoveDuringGadget;

    public Minigame? Vitals;
    public Minigame? DoorLog;
    public float HackingTimer;
    public int UsedVitalsCharges;
    public int UsedAdminCharges;

    public Hacker() : base(nameof(Hacker))
    {
        Team = Teams.Crewmate;
        Color = new Color32(117, 250, 76, byte.MaxValue);

        HackCooldown = OptionsTab.CreateFloatList(
            $"{Name}{nameof(HackCooldown)}",
            Colors.Cs(Color, "Hack cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        HackingDuration = OptionsTab.CreateFloatList(
            $"{Name}{nameof(HackingDuration)}",
            Colors.Cs(Color, "Hacking duration"),
            2.5f,
            60f,
            10f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        OnlyColorType = OptionsTab.CreateBool(
            $"{Name}{nameof(OnlyColorType)}",
            Colors.Cs(Color, "Only display color type"),
            false,
            SpawnRate);
        MaxGadgetCharges = OptionsTab.CreateFloatList(
            $"{Name}{nameof(MaxGadgetCharges)}",
            Colors.Cs(Color, "Max mobile gadget charges"),
            1f,
            30f,
            5f,
            1f,
            SpawnRate);
        RechargeTasksNumber = OptionsTab.CreateFloatList(
            $"{Name}{nameof(RechargeTasksNumber)}",
            Colors.Cs(Color, "Number of tasks needed for recharging"),
            1f,
            5f,
            2f,
            1f,
            SpawnRate);
        CanMoveDuringGadget = OptionsTab.CreateBool(
            $"{Name}{nameof(CanMoveDuringGadget)}",
            Colors.Cs(Color, "Can move during mobile gadget duration"),
            false,
            SpawnRate);
    }

    public Sprite GetHackSprite()
    {
        if (_hackSprite == null)
        {
            _hackSprite = Resources.LoadSpriteFromResources("TheOtherRoles.Resources.HackerButton.png", 115f);
        }

        return _hackSprite;
    }

    public Sprite GetVitalsSprite()
    {
        if (_vitalsSprite == null)
        {
            _vitalsSprite = FastDestroyableSingleton<HudManager>.Instance.UseButton
                .fastUseSettings[ImageNames.VitalsButton]
                .Image;
        }

        return _vitalsSprite;
    }

    public Sprite GetDoorLogSprite()
    {
        if (_doorLogSprite == null)
        {
            _doorLogSprite = FastDestroyableSingleton<HudManager>.Instance.UseButton
                .fastUseSettings[ImageNames.DoorLogsButton]
                .Image;
        }

        return _doorLogSprite;
    }

    public Sprite GetAdminSprite()
    {
        var mapId = GameOptionsManager.Instance.currentNormalGameOptions.MapId;
        if (_adminSprite != null && _lastMapId == mapId) return _adminSprite;
        var button = mapId switch
        {
            0 or 3 => FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[
                ImageNames.AdminMapButton],
            1 => FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings
                [ImageNames.MIRAAdminButton],
            4 => FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings
                [ImageNames.AirshipAdminButton],
            _ => FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings
                [ImageNames.PolusAdminButton]
        };

        _lastMapId = mapId;
        _adminSprite = button.Image;

        return _adminSprite;
    }

    public override void CreateCustomButtons(HudManager hudManager)
    {
        base.CreateCustomButtons(hudManager);
        _hackButton = new CustomButton(
            OnHackButtonClick,
            HasHackButton,
            () => true,
            ResetHackButton,
            GetHackSprite(),
            CustomButton.ButtonPositions.upperRowRight,
            hudManager,
            "ActionQuaternary",
            true,
            0f,
            OnHackButtonEffectEnd
        );
        _adminButton = new CustomButton(
            OnAdminButtonClick,
            HasAdminButton,
            CouldUseAdminButton,
            ResetAdminButton,
            GetAdminSprite(),
            CustomButton.ButtonPositions.lowerRowRight,
            hudManager,
            "ActionSecondary",
            true,
            0f,
            OnAdminButtonEffectEnd,
            GameOptionsManager.Instance.currentNormalGameOptions.MapId == 3,
            "ADMIN"
        );
        _adminButtonText = UnityEngine.Object.Instantiate(_adminButton.actionButton.cooldownTimerText,
            _adminButton.actionButton.cooldownTimerText.transform.parent);
        _adminButtonText.text = "";
        _adminButtonText.enableWordWrapping = false;
        _adminButtonText.transform.localScale = Vector3.one * 0.5f;
        _adminButtonText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
        
        _vitalsButton = new CustomButton(
            OnVitalsButtonClick,
            HasVitalsButton,
            CouldUseVitalsButton,
            ResetVitalsButton,
            GetVitalsSprite(),
            CustomButton.ButtonPositions.lowerRowCenter,
            hudManager,
            "ActionSecondary",
            true,
            0f,
            OnVitalsButtonEffectEnd,
            false,
            GameOptionsManager.Instance.currentNormalGameOptions.MapId == 1 ? "DOORLOG" : "VITALS"
        );
        _vitalsButtonText = UnityEngine.Object.Instantiate(_vitalsButton.actionButton.cooldownTimerText,
            _vitalsButton.actionButton.cooldownTimerText.transform.parent);
        _vitalsButtonText.text = "";
        _vitalsButtonText.enableWordWrapping = false;
        _vitalsButtonText.transform.localScale = Vector3.one * 0.5f;
        _vitalsButtonText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
    }

    private void OnVitalsButtonEffectEnd()
    {
        if (_vitalsButton == null) return;
        _vitalsButton.Timer = _vitalsButton.MaxTimer;
        if (_adminButton is { isEffectActive: false }) CachedPlayer.LocalPlayer.PlayerControl.moveable = true;
        if (!Minigame.Instance) return;
        if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 1 && DoorLog != null)
        {
            DoorLog.ForceClose();
        } else if (Vitals != null)
        {
            Vitals.ForceClose();
        }
    }

    private void ResetVitalsButton()
    {
        if (_vitalsButton == null) return;
        _vitalsButton.Timer = _vitalsButton.MaxTimer;
        _vitalsButton.isEffectActive = false;
        _vitalsButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
    }

    private bool CouldUseVitalsButton()
    {
        if (_vitalsButton == null) return false;
        if (_vitalsButtonText != null)
        {
            _vitalsButtonText.text = $"{MaxGadgetCharges - UsedVitalsCharges} / {MaxGadgetCharges}";
        }
            
        _vitalsButton.actionButton.graphic.sprite =
            GameOptionsManager.Instance.currentNormalGameOptions.MapId == 1
                ? GetDoorLogSprite()
                : GetVitalsSprite();
        _vitalsButton.actionButton.OverrideText(
            GameOptionsManager.Instance.currentNormalGameOptions.MapId == 1 ? "DOORLOG" : "VITALS");
        return UsedVitalsCharges < MaxGadgetCharges;
    }

    private bool HasVitalsButton()
    {
        return Player != null && Is(CachedPlayer.LocalPlayer) &&
               !CachedPlayer.LocalPlayer.Data.IsDead &&
               GameOptionsManager.Instance.currentNormalGameOptions.MapId != 0 &&
               GameOptionsManager.Instance.currentNormalGameOptions.MapId != 3;
    }

    private void OnVitalsButtonClick()
    {
        if (GameOptionsManager.Instance.currentNormalGameOptions.MapId != 1)
        {
            if (Vitals == null)
            {
                var e = UnityEngine.Object.FindObjectsOfType<SystemConsole>()
                    .FirstOrDefault(x => x.gameObject.name.Contains("panel_vitals"));
                if (e == null || Camera.main == null) return;
                Vitals = UnityEngine.Object.Instantiate(e.MinigamePrefab, Camera.main.transform, false);
            }
            if (Camera.main == null) return;
            var transform = Vitals.transform;
            transform.SetParent(Camera.main.transform, false);
            transform.localPosition = new Vector3(0.0f, 0.0f, -50f);
            Vitals.Begin(null);
        }
        else
        {
            if (DoorLog == null)
            {
                var e = UnityEngine.Object.FindObjectsOfType<SystemConsole>()
                    .FirstOrDefault(x => x.gameObject.name.Contains("SurvLogConsole"));
                if (e == null || Camera.main == null) return;
                DoorLog = UnityEngine.Object.Instantiate(e.MinigamePrefab, Camera.main.transform, false);
            }
            if (Camera.main == null) return;
            var transform = DoorLog.transform;
            transform.SetParent(Camera.main.transform, false);
            transform.localPosition = new Vector3(0.0f, 0.0f, -50f);
            DoorLog.Begin(null);
        }

        if (CanMoveDuringGadget == false) CachedPlayer.LocalPlayer.PlayerControl.moveable = false;
        CachedPlayer.LocalPlayer.NetTransform.Halt(); // Stop current movement 
        UsedVitalsCharges++;
    }

    private void OnAdminButtonEffectEnd()
    {
        if (_adminButton == null) return;
        _adminButton.Timer = _adminButton.MaxTimer;
        if (_vitalsButton is { isEffectActive: false }) CachedPlayer.LocalPlayer.PlayerControl.moveable = true;
        if (MapBehaviour.Instance && MapBehaviour.Instance.isActiveAndEnabled) MapBehaviour.Instance.Close();
    }

    private void ResetAdminButton()
    {
        if (_adminButton == null) return;
        _adminButton.Timer = _adminButton.MaxTimer;
        _adminButton.isEffectActive = false;
        _adminButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
    }

    private bool CouldUseAdminButton()
    {
        if (_adminButtonText != null)
        {
            _adminButtonText.text = $"{MaxGadgetCharges - UsedAdminCharges} / {MaxGadgetCharges}";
        }

        return UsedAdminCharges < MaxGadgetCharges;
    }

    private bool HasAdminButton()
    {
        return Player != null && Is(CachedPlayer.LocalPlayer) && !CachedPlayer.LocalPlayer.Data.IsDead;
    }

    private void OnAdminButtonClick()
    {
        if (!MapBehaviour.Instance || !MapBehaviour.Instance.isActiveAndEnabled)
        {
            var hudManager = FastDestroyableSingleton<HudManager>.Instance;
            hudManager.InitMap();
            MapBehaviour.Instance.ShowCountOverlay(allowedToMove: true, showLivePlayerPosition: true,
                includeDeadBodies: true);
        }

        if (CanMoveDuringGadget == false) CachedPlayer.LocalPlayer.PlayerControl.moveable = false;
        CachedPlayer.LocalPlayer.NetTransform.Halt(); // Stop current movement 
        UsedAdminCharges++;
    }

    private void OnHackButtonEffectEnd()
    {
        if (_hackButton == null) return;
        _hackButton.Timer = _hackButton.MaxTimer;
    }

    private void ResetHackButton()
    {
        if (_hackButton == null) return;
        _hackButton.Timer = _hackButton.MaxTimer;
        _hackButton.isEffectActive = false;
        _hackButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
    }

    private bool HasHackButton()
    {
        return Player != null && Is(CachedPlayer.LocalPlayer) && !CachedPlayer.LocalPlayer.Data.IsDead;
    }

    private void OnHackButtonClick()
    {
        HackingTimer = HackingDuration;
        SoundEffectsManager.play("hackerHack");
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        Vitals = null;
        DoorLog = null;
        HackingTimer = 0f;
        UsedVitalsCharges = 0;
        UsedAdminCharges = 0;
    }
}