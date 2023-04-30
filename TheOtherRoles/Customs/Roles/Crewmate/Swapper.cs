using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.EnoFramework.Utils;
using UnityEngine;
using Resources = TheOtherRoles.EnoFramework.Utils.Resources;

namespace TheOtherRoles.Customs.Roles.Crewmate;

[EnoSingleton]
public class Swapper : CustomRole
{
    private Sprite? _swapSprite;

    public readonly EnoFramework.CustomOption CanCallEmergencyMeeting;
    public readonly EnoFramework.CustomOption CanOnlySwapOthers;
    public readonly EnoFramework.CustomOption NumberOfSwaps;
    public readonly EnoFramework.CustomOption RechargeTasksNumber;

    public int UsedSwaps;
    public bool EnableSwap;
    public PlayerControl? FirstPlayer;
    public PlayerControl? SecondPlayer;

    public Swapper() : base(nameof(Swapper))
    {
        Team = Teams.Crewmate;
        Color = new Color32(134, 55, 86, byte.MaxValue);

        CanCallEmergencyMeeting = OptionsTab.CreateBool(
            $"{Name}{nameof(CanCallEmergencyMeeting)}",
            Colors.Cs(Color, "Can call emergency meeting"),
            true,
            SpawnRate);
        CanOnlySwapOthers = OptionsTab.CreateBool(
            $"{Name}{nameof(CanOnlySwapOthers)}",
            Colors.Cs(Color, "Can only swap others"),
            false,
            SpawnRate);
        NumberOfSwaps = OptionsTab.CreateFloatList(
            $"{Name}{nameof(NumberOfSwaps)}",
            Colors.Cs(Color, "Initial swap charges"),
            0f,
            5f,
            1f,
            1f,
            SpawnRate);
        RechargeTasksNumber = OptionsTab.CreateFloatList(
            $"{Name}{nameof(RechargeTasksNumber)}",
            Colors.Cs(Color, "Number of tasks needed for recharging"),
            1f,
            10f,
            2f,
            1f,
            SpawnRate);
    }

    public Sprite GetSwapSprite()
    {
        if (_swapSprite == null)
        {
            _swapSprite = Resources.LoadSpriteFromResources("TheOtherRoles.Resources.SwapperCheck.png", 150f);
        }

        return _swapSprite;
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        UsedSwaps = 0;
        EnableSwap = false;
        FirstPlayer = null;
        SecondPlayer = null;
    }
}