using TheOtherRoles.Players;
using TheOtherRoles.Utilities;

using TheOtherRoles.EnoFw.Roles.Crewmate;
using TheOtherRoles.EnoFw.Roles.Impostor;
using TheOtherRoles.EnoFw.Roles.Modifiers;
using TheOtherRoles.EnoFw.Roles.Neutral;
using UnityEngine;

namespace TheOtherRoles;

class RoleDescription
{
    public RoleInfo BaseInfo;
    public RoleId RoleId;
    
    public string Description;
    public string[] ButtonDescription;
    
    public bool IsNeutral;
    public bool IsGuesser => HandleGuesser.isGuesserGm && HandleGuesser.isGuesser(CachedPlayer.LocalPlayer.PlayerId);

    public RoleDescription(RoleInfo baseInfo, string description, string[] buttonDescription)
    {
        BaseInfo = baseInfo;
        RoleId = BaseInfo.RoleId;
        Description = description;
        ButtonDescription = buttonDescription;
        IsNeutral = BaseInfo.isNeutral;
    }

    public RoleDescription JesterDescription = new RoleDescription(RoleInfo.jester, "Your Role is simple.\nYou simply have to get voted and exiled from a meeting to win your game", new string[] { });
    public RoleDescription MayorDescription = new RoleDescription(RoleInfo.mayor, $"The mayor is the crewmate's boss.\n - His vote count as double.\n - He possess a special button that can trigger a meeting from anywhere if enabled !\n - If { Helpers.cs((Mayor.canSeeVoteColors ? Color.green : Color.red), "enabled")}, he can see the color of the vote in meeting after a certain amount of task done {(Mayor.canSeeVoteColors ? Mayor.tasksNeededToSeeVoteColors : "")}", new string[] { $"Meeting button :\n    - Description: Call a meeting from anywhere\nMax number of usage: { Mayor.remoteMeetingsLeft }" });

}