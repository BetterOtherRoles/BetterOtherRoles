using System;
using AmongUs.GameOptions;
using Reactor.Networking.Attributes;
using TheOtherRoles.EnoFw.Roles.Crewmate;
using TheOtherRoles.EnoFw.Roles.Impostor;
using TheOtherRoles.Objects;
using TheOtherRoles.Utilities;
using UnityEngine;

namespace TheOtherRoles.EnoFw.Roles.Neutral;

public static class Thief
{
    public static PlayerControl thief;
    public static Color color = new Color32(71, 99, 45, byte.MaxValue);
    public static PlayerControl currentTarget;
    public static PlayerControl formerThief;
    public static PlayerControl playerStealed;

    public enum StealMethod
    {
        StealRole,
        BecomePartner
    }

    public static StealMethod stealMethod = StealMethod.StealRole;

    public static float cooldown = 30f;

    public static bool suicideFlag = false; // Used as a flag for suicide

    public static bool hasImpostorVision;
    public static bool canUseVents;
    public static bool canKillSheriff;


    public static void clearAndReload()
    {
        thief = null;
        suicideFlag = false;
        currentTarget = null;
        formerThief = null;
        playerStealed = null;
        stealMethod = (StealMethod)CustomOptionHolder.thiefStealMethod.getSelection();
        hasImpostorVision = CustomOptionHolder.thiefHasImpVision.getBool();
        cooldown = CustomOptionHolder.thiefCooldown.getFloat();
        canUseVents = CustomOptionHolder.thiefCanUseVents.getBool();
        canKillSheriff = CustomOptionHolder.thiefCanKillSheriff.getBool();
    }

    public static void ThiefStealsRole(byte targetId)
    {
        var data = new Tuple<byte>(targetId);
        Rpc_ThiefStealsRole(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.ThiefStealsRole)]
    private static void Rpc_ThiefStealsRole(PlayerControl sender, string rawData)
    {
        var targetId = Rpc.Deserialize<Tuple<byte>>(rawData).Item1;
        var target = Helpers.playerById(targetId);
        var thiefPlayer = thief;
        if (target == null) return;
        if (target == Sheriff.sheriff) Sheriff.sheriff = thiefPlayer;
        if (target == Jackal.jackal)
        {
            Jackal.jackal = thiefPlayer;
            Jackal.formerJackals.Add(target);
        }

        if (target == Sidekick.sidekick)
        {
            Sidekick.sidekick = thiefPlayer;
            Jackal.formerJackals.Add(target);
        }

        if (target == Guesser.evilGuesser) Guesser.evilGuesser = thiefPlayer;
        if (target == Godfather.godfather) Godfather.godfather = thiefPlayer;
        if (target == Mafioso.mafioso) Mafioso.mafioso = thiefPlayer;
        if (target == Janitor.janitor) Janitor.janitor = thiefPlayer;
        if (target == Morphling.morphling) Morphling.morphling = thiefPlayer;
        if (target == Camouflager.camouflager) Camouflager.camouflager = thiefPlayer;
        if (target == Vampire.vampire) Vampire.vampire = thiefPlayer;
        if (target == Whisperer.whisperer) Whisperer.whisperer = thiefPlayer;
        if (target == Undertaker.undertaker) Undertaker.undertaker = thiefPlayer;
        if (target == Eraser.eraser) Eraser.eraser = thiefPlayer;
        if (target == Trickster.trickster) Trickster.trickster = thiefPlayer;
        if (target == Cleaner.cleaner) Cleaner.cleaner = thiefPlayer;
        if (target == Warlock.warlock) Warlock.warlock = thiefPlayer;
        if (target == BountyHunter.bountyHunter) BountyHunter.bountyHunter = thiefPlayer;
        if (target == Witch.witch) Witch.witch = thiefPlayer;
        if (target == Ninja.ninja) Ninja.ninja = thiefPlayer;
        if (target == Bomber.bomber) Bomber.bomber = thiefPlayer;
        if (target.Data.Role.IsImpostor)
        {
            RoleManager.Instance.SetRole(thief, RoleTypes.Impostor);
            FastDestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(thief.killTimer,
                GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown);
        }

        if (target == Lawyer.target)
        {
            Lawyer.target = thiefPlayer;
            Lawyer.formerLawyer = target;
        }

        if (stealMethod != StealMethod.BecomePartner)
        {
            Fallen.clearAndReload();
            Fallen.fallen = target; // Change target to Fallen ???
            RoleManager.Instance.SetRole(target, RoleTypes.Crewmate);

            foreach (var task in target.myTasks.GetFastEnumerator())
            {
                // if (task.TaskType == TaskTypes.FixLights) continue;
                // if (task.TaskType == TaskTypes.RestoreOxy) continue;
                // if (task.TaskType == TaskTypes.ResetReactor) continue;
                // if (task.TaskType == TaskTypes.ResetSeismic) continue;
                // if (task.TaskType == TaskTypes.FixComms) continue;
                // if (task.TaskType == TaskTypes.StopCharles) continue;
                // if (SubmergedCompatibility.IsSubmerged && task.TaskType == SubmergedCompatibility.RetrieveOxygenMask) continue;

                // task.Complete();
                // task.IsComplete = true;

                task.OnRemove();
                target.myTasks.Remove(task);
                UnityEngine.Object.Destroy(task.gameObject);
            }
        }

        if (thief == PlayerControl.LocalPlayer) CustomButton.ResetAllCooldowns();
        clearAndReload();
        formerThief = thief; // After clearAndReload, else it would get reset...
        playerStealed = target;
    }
}