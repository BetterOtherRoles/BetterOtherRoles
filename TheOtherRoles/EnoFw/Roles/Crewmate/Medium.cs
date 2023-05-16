using System;
using System.Collections.Generic;
using System.Linq;
using TheOtherRoles.EnoFw.Kernel;
using TheOtherRoles.EnoFw.Roles.Impostor;
using TheOtherRoles.EnoFw.Roles.Modifiers;
using TheOtherRoles.EnoFw.Roles.Neutral;
using UnityEngine;
using Option = TheOtherRoles.EnoFw.Kernel.CustomOption;

namespace TheOtherRoles.EnoFw.Roles.Crewmate;

public class Medium : AbstractRole
{
    public static readonly Medium Instance = new();
    
    // Fields
    public DeadPlayer Target;
    public DeadPlayer SoulTarget;
    public readonly List<Tuple<DeadPlayer, Vector3>> DeadBodies = new();
    public readonly List<Tuple<DeadPlayer, Vector3>> FeatureDeadBodies = new();
    public readonly List<SpriteRenderer> Souls = new();
    public DateTime MeetingStartTime = DateTime.UtcNow;
    
    // Options
    public readonly Option QuestionCooldown;
    public readonly Option QuestionDuration;
    public readonly Option OneQuestionPerSoul;
    public readonly Option AdditionalInfoChance;

    public static Sprite SoulSprite => GetSprite("TheOtherRoles.Resources.Soul.png", 500f);
    public static Sprite QuestionButtonSprite => GetSprite("TheOtherRoles.Resources.MediumButton.png", 115f);

    private Medium() : base(nameof(Medium), "Medium")
    {
        Team = Teams.Crewmate;
        Color = new Color32(98, 120, 115, byte.MaxValue);
        
        SpawnRate = GetDefaultSpawnRateOption();
        
        QuestionCooldown = Tab.CreateFloatList(
            $"{Key}{nameof(QuestionCooldown)}",
            Cs("Questioning cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        QuestionDuration = Tab.CreateFloatList(
            $"{Key}{nameof(QuestionDuration)}",
            Cs("Questioning duration"),
            0f,
            15f,
            3f,
            1f,
            SpawnRate,
            string.Empty,
            "s");
        OneQuestionPerSoul = Tab.CreateBool(
            $"{Key}{nameof(OneQuestionPerSoul)}",
            Cs("Each soul can only be questionned once"),
            false,
            SpawnRate);
        AdditionalInfoChance = Tab.CreateFloatList(
            $"{Key}{nameof(AdditionalInfoChance)}",
            Cs("Chance to have additional information"),
            0f,
            100f,
            0f,
            10f,
            SpawnRate,
            string.Empty,
            "%");
    }

    enum SpecialMediumInfo
    {
        SheriffSuicide,
        ThiefSuicide,
        ActiveLoverDies,
        PassiveLoverSuicide,
        LawyerKilledByClient,
        JackalKillsSidekick,
        ImpostorTeamkill,
        SubmergedO2,
        WarlockSuicide,
        BodyCleaned,
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        Target = null;
        SoulTarget = null;
        DeadBodies.Clear();
        FeatureDeadBodies.Clear();
        Souls.Clear();
        MeetingStartTime = DateTime.UtcNow;
    }

    public static string getInfo(PlayerControl target, PlayerControl killer)
    {
        string msg = "";

        List<SpecialMediumInfo> infos = new List<SpecialMediumInfo>();
        // collect fitting death info types.
        // suicides:
        if (killer == target)
        {
            if (target == Sheriff.Instance.Player || target == Sheriff.Instance.FormerSheriff)
                infos.Add(SpecialMediumInfo.SheriffSuicide);
            if (Lovers.Instance.Is(target)) infos.Add(SpecialMediumInfo.PassiveLoverSuicide);
            if (target == Thief.Instance.Player) infos.Add(SpecialMediumInfo.ThiefSuicide);
            if (target == Warlock.Instance.Player) infos.Add(SpecialMediumInfo.WarlockSuicide);
        }
        else
        {
            if (Lovers.Instance.Is(target)) infos.Add(SpecialMediumInfo.ActiveLoverDies);
            if (target.Data.Role.IsImpostor && killer.Data.Role.IsImpostor && Thief.Instance.FormerThief != killer)
                infos.Add(SpecialMediumInfo.ImpostorTeamkill);
        }

        if (target == Sidekick.Instance.Player &&
            (killer == Jackal.Instance.Player || Jackal.Instance.FormerJackals.Any(x => x.PlayerId == killer.PlayerId)))
            infos.Add(SpecialMediumInfo.JackalKillsSidekick);
        if (target == Lawyer.Instance.Player && killer == Lawyer.Instance.Target) infos.Add(SpecialMediumInfo.LawyerKilledByClient);
        if (Instance.Target.wasCleaned) infos.Add(SpecialMediumInfo.BodyCleaned);

        if (infos.Count > 0)
        {
            var selectedInfo = infos[TheOtherRoles.Rnd.Next(infos.Count)];
            switch (selectedInfo)
            {
                case SpecialMediumInfo.SheriffSuicide:
                    msg = "Yikes, that Sheriff shot backfired.";
                    break;
                case SpecialMediumInfo.WarlockSuicide:
                    msg = "MAYBE I cursed the person next to me and killed myself. Oops.";
                    break;
                case SpecialMediumInfo.ThiefSuicide:
                    msg = "I tried to steal the gun from their pocket, but they were just happy to see me.";
                    break;
                case SpecialMediumInfo.ActiveLoverDies:
                    msg = "I wanted to get out of this toxic relationship anyways.";
                    break;
                case SpecialMediumInfo.PassiveLoverSuicide:
                    msg = "The love of my life died, thus with a kiss I die.";
                    break;
                case SpecialMediumInfo.LawyerKilledByClient:
                    msg = "My client killed me. Do I still get paid?";
                    break;
                case SpecialMediumInfo.JackalKillsSidekick:
                    msg = "First they sidekicked me, then they killed me. At least I don't need to do tasks anymore.";
                    break;
                case SpecialMediumInfo.ImpostorTeamkill:
                    msg = "I guess they confused me for the Spy, is there even one?";
                    break;
                case SpecialMediumInfo.BodyCleaned:
                    msg = "Is my dead body some kind of art now or... aaand it's gone.";
                    break;
            }
        }
        else
        {
            int randomNumber = TheOtherRoles.Rnd.Next(4);
            string typeOfColor = Helpers.isLighterColor(Instance.Target.killerIfExisting.Data.DefaultOutfit.ColorId)
                ? "lighter"
                : "darker";
            float timeSinceDeath = ((float)(Instance.MeetingStartTime - Instance.Target.timeOfDeath).TotalMilliseconds);

            if (randomNumber == 0)
                msg = "If my role hasn't been saved, there's no " +
                      RoleInfo.GetRolesString(Instance.Target.player, false) + " in the game anymore.";
            else if (randomNumber == 1) msg = "I'm not sure, but I guess a " + typeOfColor + " color killed me.";
            else if (randomNumber == 2)
                msg = "If I counted correctly, I died " + Math.Round(timeSinceDeath / 1000) +
                      "s before the next meeting started.";
            else
                msg = "It seems like my killer was the " +
                      RoleInfo.GetRolesString(Medium.Instance.Target.killerIfExisting, false, false, true) + ".";
        }

        if (TheOtherRoles.Rnd.Next(0, 100) <= Medium.Instance.AdditionalInfoChance)
        {
            int count = 0;
            string condition = "";
            var alivePlayersList = PlayerControl.AllPlayerControls.ToArray().Where(pc => !pc.Data.IsDead);
            switch (TheOtherRoles.Rnd.Next(3))
            {
                case 0:
                    count = alivePlayersList.Count(pc => pc.Data.Role.IsImpostor ||
                                                         new List<RoleInfo> { RoleInfo.jackal, RoleInfo.sidekick, RoleInfo.sheriff, RoleInfo.thief }
                                                             .Contains(RoleInfo.getRoleInfoForPlayer(pc, false).FirstOrDefault()));
                    condition = "killer" + (count == 1 ? "" : "s");
                    break;
                case 1:
                    count = alivePlayersList.Count(Helpers.roleCanUseVents);
                    condition = "player" + (count == 1 ? "" : "s") + " who can use vents";
                    break;
                case 2:
                    count = alivePlayersList
                        .Count(pc => Helpers.isNeutral(pc) && pc != Jackal.Instance.Player && pc != Sidekick.Instance.Player &&
                                     pc != Thief.Instance.Player);
                    condition = "player" + (count == 1 ? "" : "s") + " who " + (count == 1 ? "is" : "are") +
                                " neutral but cannot kill";
                    break;
                case 3:
                    //count = alivePlayersList.Where(pc =>
                    break;
            }

            msg += $"\nWhen you asked, {count} " + condition + (count == 1 ? " was" : " were") + " still alive";
        }

        return Instance.Target.player.Data.PlayerName + "'s Soul:\n" + msg;
    }
}