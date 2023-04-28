using System.Linq;
using Reactor.Networking.Attributes;
using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.EnoFramework.Utils;
using TheOtherRoles.Objects;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;
using UnityEngine;
using Resources = TheOtherRoles.EnoFramework.Utils.Resources;

namespace TheOtherRoles.Customs.Roles.Crewmate;

[EnoSingleton]
public class Engineer : CustomRole
{
    private Sprite? _repairSprite;

    private CustomButton? _repairButton;

    public readonly EnoFramework.CustomOption NumberOfFixes;
    public readonly EnoFramework.CustomOption HighlightVentsForImpostors;
    public readonly EnoFramework.CustomOption HighlightVentsForNeutrals;

    public int UsedSabotagesFixes;

    public Engineer() : base(nameof(Engineer))
    {
        Team = Teams.Crewmate;
        Color = new Color32(0, 40, 245, byte.MaxValue);

        NumberOfFixes = OptionsTab.CreateFloatList(
            $"{Name}{nameof(NumberOfFixes)}",
            Colors.Cs(Color, "Number of sabotage fixes"),
            1f,
            3f,
            1f,
            1f,
            SpawnRate);
        HighlightVentsForImpostors = OptionsTab.CreateBool(
            $"{Name}{nameof(HighlightVentsForImpostors)}",
            Colors.Cs(Color, "Impostors see vents highlighted"),
            true,
            SpawnRate);
        HighlightVentsForNeutrals = OptionsTab.CreateBool(
            $"{Name}{nameof(HighlightVentsForNeutrals)}",
            Colors.Cs(Color, "Neutrals see vents highlighted"),
            false,
            SpawnRate);
    }

    public Sprite GetRepairSprite()
    {
        if (_repairSprite == null)
        {
            _repairSprite = Resources.LoadSpriteFromResources("TheOtherRoles.Resources.RepairButton.png", 115f);
        }

        return _repairSprite;
    }

    public override void CreateCustomButtons(HudManager hudManager)
    {
        base.CreateCustomButtons(hudManager);
        _repairButton = new CustomButton(
            OnRepairButtonClick,
            HasRepairButton,
            CouldUseRepairButton,
            () => { },
            GetRepairSprite(),
            CustomButton.ButtonPositions.upperRowRight,
            hudManager,
            "ActionQuaternary"
        );
    }

    private bool CouldUseRepairButton()
    {
        var sabotageActive = CachedPlayer.LocalPlayer.PlayerControl.myTasks.GetFastEnumerator().Any(task =>
            task.TaskType == TaskTypes.FixLights || task.TaskType == TaskTypes.RestoreOxy ||
            task.TaskType == TaskTypes.ResetReactor || task.TaskType == TaskTypes.ResetSeismic ||
            task.TaskType == TaskTypes.FixComms || task.TaskType == TaskTypes.StopCharles ||
            (SubmergedCompatibility.IsSubmerged && task.TaskType == SubmergedCompatibility.RetrieveOxygenMask));

        return sabotageActive && UsedSabotagesFixes < NumberOfFixes && CachedPlayer.LocalPlayer.PlayerControl.CanMove;
    }

    private bool HasRepairButton()
    {
        return Player != null && Is(CachedPlayer.LocalPlayer) && !CachedPlayer.LocalPlayer.Data.IsDead &&
               UsedSabotagesFixes < NumberOfFixes;
    }

    private void OnRepairButtonClick()
    {
        if (_repairButton == null) return;
        _repairButton.Timer = 0f;
        UseRepair(CachedPlayer.LocalPlayer);
        SoundEffectsManager.play("engineerRepair");
        foreach (var task in CachedPlayer.LocalPlayer.PlayerControl.myTasks.GetFastEnumerator())
        {
            if (task.TaskType == TaskTypes.FixLights)
            {
                FixLights(CachedPlayer.LocalPlayer);
            }
            else if (task.TaskType == TaskTypes.RestoreOxy)
            {
                MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.LifeSupp, 0 | 64);
                MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.LifeSupp, 1 | 64);
            }
            else if (task.TaskType == TaskTypes.ResetReactor)
            {
                MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Reactor, 16);
            }
            else if (task.TaskType == TaskTypes.ResetSeismic)
            {
                MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Laboratory, 16);
            }
            else if (task.TaskType == TaskTypes.FixComms)
            {
                MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Comms, 16 | 0);
                MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Comms, 16 | 1);
            }
            else if (task.TaskType == TaskTypes.StopCharles)
            {
                MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Reactor, 0 | 16);
                MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Reactor, 1 | 16);
            }
            else if (SubmergedCompatibility.IsSubmerged &&
                     task.TaskType == SubmergedCompatibility.RetrieveOxygenMask)
            {
                FixSubmergedOxygen(CachedPlayer.LocalPlayer);
            }
        }
    }

    [MethodRpc((uint)Rpc.Id.EngineerUseRepair)]
    private static void UseRepair(PlayerControl sender)
    {
        Singleton<Engineer>.Instance.UsedSabotagesFixes++;
        if (Helpers.shouldShowGhostInfo())
        {
            Helpers.showFlash(Singleton<Engineer>.Instance.Color, 0.5f, "Engineer Fix");
        }
    }

    [MethodRpc((uint)Rpc.Id.EngineerFixLights)]
    private static void FixLights(PlayerControl sender)
    {
        var switchSystem = MapUtilities.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
        switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
    }

    [MethodRpc((uint)Rpc.Id.EngineerFixSubmergedOxygen)]
    private static void FixSubmergedOxygen(PlayerControl sender)
    {
        SubmergedCompatibility.RepairOxygen();
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        UsedSabotagesFixes = 0;
    }
}