using BetterOtherRoles.EnoFw.Kernel;
using BetterOtherRoles.Utilities;
using UnityEngine;
using Option = BetterOtherRoles.EnoFw.Kernel.CustomOption;

namespace BetterOtherRoles.EnoFw.Roles.Crewmate;

public class Hacker : AbstractRole
{
    public static readonly Hacker Instance = new();

    // Fields
    public Minigame Vitals;
    public Minigame DoorLog;
    public float Timer;
    public int RechargedTasks;
    public int VitalsCharges;
    public int AdminCharges;

    // Options
    public readonly CustomOption HackCooldown;
    public readonly CustomOption HackingDuration;
    public readonly CustomOption OnlyColorType;
    public readonly CustomOption MaxGadgetCharges;
    public readonly CustomOption RechargeTasksNumber;
    public readonly CustomOption CanMoveDuringGadget;

    private static Sprite _vitalsSprite;
    private static Sprite _doorLogSprite;
    private static Sprite _adminSprite;

    public static Sprite HackButtonSprite => GetSprite("BetterOtherRoles.Resources.HackerButton.png", 115f);

    public static Sprite VitalsButtonSprite
    {
        get
        {
            if (_vitalsSprite != null) return _vitalsSprite;
            _vitalsSprite = FastDestroyableSingleton<HudManager>.Instance.UseButton
                .fastUseSettings[ImageNames.VitalsButton].Image;
            return _vitalsSprite;
        }
    }

    public static Sprite DoorLogButtonSprite
    {
        get
        {
            if (_doorLogSprite != null) return _doorLogSprite;
            _doorLogSprite = FastDestroyableSingleton<HudManager>.Instance.UseButton
                .fastUseSettings[ImageNames.DoorLogsButton].Image;
            return _doorLogSprite;
        }
    }

    public static Sprite AdminButtonSprite
    {
        get
        {
            if (_adminSprite != null) return _adminSprite;
            var mapId = GameOptionsManager.Instance.currentNormalGameOptions.MapId;
            var button = mapId switch
            {
                0 or 3 => FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[
                    ImageNames.AdminMapButton] // Skeld & Dleks
                ,
                1 => FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings
                    [ImageNames.MIRAAdminButton] // Mira HQ
                ,
                4 => FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[
                    ImageNames.AirshipAdminButton] // Airship
                ,
                _ => FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[
                    ImageNames.PolusAdminButton]
            };

            _adminSprite = button.Image;
            return _adminSprite;
        }
    }

    private Hacker() : base(nameof(Hacker), "Hacker")
    {
        Team = Teams.Crewmate;
        Color = new Color32(117, 250, 76, byte.MaxValue);
        
        SpawnRate = GetDefaultSpawnRateOption();
        
        HackCooldown = Tab.CreateFloatList(
            $"{Key}{nameof(HackCooldown)}",
            Cs("Hack cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        HackingDuration = Tab.CreateFloatList(
            $"{Key}{nameof(HackingDuration)}",
            Cs("Hacking duration"),
            2.5f,
            60f,
            10f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        OnlyColorType = Tab.CreateBool(
            $"{Key}{nameof(OnlyColorType)}",
            Cs("Only display color type"),
            false,
            SpawnRate);
        MaxGadgetCharges = Tab.CreateFloatList(
            $"{Key}{nameof(MaxGadgetCharges)}",
            Cs("Max mobile gadget charges"),
            1f,
            30f,
            5f,
            1f,
            SpawnRate);
        RechargeTasksNumber = Tab.CreateFloatList(
            $"{Key}{nameof(RechargeTasksNumber)}",
            Cs("Number of tasks needed for recharging"),
            1f,
            5f,
            2f,
            1f,
            SpawnRate);
        CanMoveDuringGadget = Tab.CreateBool(
            $"{Key}{nameof(CanMoveDuringGadget)}",
            Cs("Can move during mobile gadget duration"),
            false,
            SpawnRate);
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        RechargedTasks = RechargeTasksNumber;
        VitalsCharges = MaxGadgetCharges;
        AdminCharges = MaxGadgetCharges;
        Vitals = null;
        DoorLog = null;
        Timer = 0f;
        _adminSprite = null;
    }
}