using System.Linq;
using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.EnoFramework.Utils;
using TheOtherRoles.Objects;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;
using UnityEngine;
using Resources = TheOtherRoles.EnoFramework.Utils.Resources;

namespace TheOtherRoles.Customs.Roles.Crewmate;

[EnoSingleton]
public class Mayor : CustomRole
{
    private Sprite? _buzzerSprite;

    private CustomButton? _buzzerButton;

    public readonly EnoFramework.CustomOption CanChooseSingleVote;
    public readonly EnoFramework.CustomOption CanSeeVoteColors;
    public readonly EnoFramework.CustomOption TasksNeededToSeeVoteColors;
    public readonly EnoFramework.CustomOption HasRemoteMeetingButton;
    public readonly EnoFramework.CustomOption MaxRemoteMeetings;

    public bool VoteTwice;
    public int UsedRemoteMeetings;

    public Mayor() : base(nameof(Mayor))
    {
        Team = Teams.Crewmate;
        Color = new Color32(32, 77, 66, byte.MaxValue);

        CanChooseSingleVote = OptionsTab.CreateBool(
            $"{Name}{nameof(CanChooseSingleVote)}",
            Colors.Cs(Color, "Can choose single vote"),
            false,
            SpawnRate);
        CanSeeVoteColors = OptionsTab.CreateBool(
            $"{Name}{nameof(CanSeeVoteColors)}",
            Colors.Cs(Color, "Can see vote colors"),
            false,
            SpawnRate);
        TasksNeededToSeeVoteColors = OptionsTab.CreateFloatList(
            $"{Name}{nameof(TasksNeededToSeeVoteColors)}",
            Colors.Cs(Color, "Completed tasks needed to see vote colors"),
            0f,
            20f,
            5f,
            1f,
            CanSeeVoteColors);
        HasRemoteMeetingButton = OptionsTab.CreateBool(
            $"{Name}{nameof(HasRemoteMeetingButton)}",
            Colors.Cs(Color, "Has mobile emergency button"),
            false,
            SpawnRate);
        MaxRemoteMeetings = OptionsTab.CreateFloatList(
            $"{Name}{nameof(MaxRemoteMeetings)}",
            Colors.Cs(Color, "Number of remote meetings"),
            1f,
            5f,
            1f,
            1f,
            CanSeeVoteColors);
    }

    public Sprite GetBuzzerSprite()
    {
        if (_buzzerSprite == null)
        {
            _buzzerSprite = Resources.LoadSpriteFromResources("TheOtherRoles.Resources.EmergencyButton.png", 550f);
        }

        return _buzzerSprite;
    }

    public override void CreateCustomButtons(HudManager hudManager)
    {
        base.CreateCustomButtons(hudManager);
        _buzzerButton = new CustomButton(
            OnBuzzerButtonClick,
            HasBuzzerButton,
            CouldUseBuzzerButton,
            ResetBuzzerButton,
            GetBuzzerSprite(),
            CustomButton.ButtonPositions.lowerRowRight,
            hudManager,
            "ActionQuaternary",
            true,
            0f,
            () => { },
            false,
            "Meeting"
        );
    }

    private void ResetBuzzerButton()
    {
        if (_buzzerButton == null) return;
        _buzzerButton.Timer = _buzzerButton.MaxTimer;
    }

    private bool CouldUseBuzzerButton()
    {
        if (_buzzerButton == null) return false;
        _buzzerButton.actionButton.OverrideText("Emergency (" + (MaxRemoteMeetings - UsedRemoteMeetings) + ")");
        var sabotageActive = CachedPlayer.LocalPlayer.PlayerControl.myTasks.GetFastEnumerator().Any(task =>
            task.TaskType == TaskTypes.FixLights || task.TaskType == TaskTypes.RestoreOxy ||
            task.TaskType == TaskTypes.ResetReactor || task.TaskType == TaskTypes.ResetSeismic ||
            task.TaskType == TaskTypes.FixComms || task.TaskType == TaskTypes.StopCharles ||
            (SubmergedCompatibility.IsSubmerged && task.TaskType == SubmergedCompatibility.RetrieveOxygenMask));

        return !sabotageActive && CachedPlayer.LocalPlayer.PlayerControl.CanMove &&
               UsedRemoteMeetings < MaxRemoteMeetings;
    }

    private bool HasBuzzerButton()
    {
        return Player != null && Is(CachedPlayer.LocalPlayer) && !CachedPlayer.LocalPlayer.Data.IsDead &&
               HasRemoteMeetingButton;
    }

    private void OnBuzzerButtonClick()
    {
        if (Player == null || _buzzerButton == null) return;
        CachedPlayer.LocalPlayer.NetTransform.Halt(); // Stop current movement 
        UsedRemoteMeetings++;
        Helpers.handleVampireBiteOnBodyReport(); // Manually call Vampire handling, since the CmdReportDeadBody Prefix won't be called
        Rpc.UncheckedReportDeadBody(Player, null);
        _buzzerButton.Timer = 1f;
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        VoteTwice = false;
        UsedRemoteMeetings = 0;
    }
}