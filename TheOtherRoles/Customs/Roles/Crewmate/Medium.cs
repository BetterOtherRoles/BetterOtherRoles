using System;
using System.Collections.Generic;
using System.Linq;
using TheOtherRoles.Customs.Modifiers;
using TheOtherRoles.Customs.Roles.Impostor;
using TheOtherRoles.Customs.Roles.Neutral;
using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.Objects;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;
using UnityEngine;
using Resources = TheOtherRoles.EnoFramework.Utils.Resources;

namespace TheOtherRoles.Customs.Roles.Crewmate;

[EnoSingleton]
public class Medium : CustomRole
{
    private Sprite? _soulSprite;
    private Sprite? _questionSprite;

    private CustomButton? _questionButton;

    public readonly EnoFramework.CustomOption QuestionCooldown;
    public readonly EnoFramework.CustomOption QuestionDuration;
    public readonly EnoFramework.CustomOption OneQuestionPerSoul;
    public readonly EnoFramework.CustomOption AdditionalInfoChance;

    public DeadPlayer? DeadTarget;
    public DeadPlayer? SoulTarget;
    public readonly List<Tuple<DeadPlayer, Vector3>> DeadBodies = new();
    public readonly List<Tuple<DeadPlayer, Vector3>> FeatureDeadBodies = new();
    public readonly List<SpriteRenderer> Souls = new();
    public DateTime MeetingStartTime = DateTime.UtcNow;

    public Medium() : base(nameof(Medium))
    {
        Team = Teams.Crewmate;
        Color = new Color32(98, 120, 115, byte.MaxValue);

        IntroDescription = "Question the souls of the dead to gain information";
        ShortDescription = "Question the souls";

        QuestionCooldown = OptionsTab.CreateFloatList(
            $"{Name}{nameof(QuestionCooldown)}",
            Cs("Questioning cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        QuestionDuration = OptionsTab.CreateFloatList(
            $"{Name}{nameof(QuestionDuration)}",
            Cs("Questioning duration"),
            0f,
            15f,
            3f,
            1f,
            SpawnRate,
            string.Empty,
            "s");
        OneQuestionPerSoul = OptionsTab.CreateBool(
            $"{Name}{nameof(OneQuestionPerSoul)}",
            Cs("Each soul can only be questionned once"),
            false,
            SpawnRate);
        AdditionalInfoChance = OptionsTab.CreateFloatList(
            $"{Name}{nameof(AdditionalInfoChance)}",
            Cs("Chance to have additional information"),
            0f,
            100f,
            0f,
            10f,
            null,
            string.Empty,
            "%");
    }

    private enum SpecialMediumInfo
    {
        SheriffSuicide,
        ThiefSuicide,
        ActiveLoverDies,
        PassiveLoverSuicide,
        LawyerKilledByClient,
        JackalKillsSidekick,
        ImpostorTeamKill,
        WarlockSuicide,
        BodyCleaned,
    }

    public Sprite GetSoulSprite()
    {
        if (_soulSprite == null)
        {
            _soulSprite = Resources.LoadSpriteFromResources("TheOtherRoles.Resources.Soul.png", 500f);
        }

        return _soulSprite;
    }

    public Sprite GetQuestionSprite()
    {
        if (_questionSprite == null)
        {
            _questionSprite = Resources.LoadSpriteFromResources("TheOtherRoles.Resources.MediumButton.png", 115f);
        }

        return _questionSprite;
    }

    public override void CreateCustomButtons(HudManager hudManager)
    {
        base.CreateCustomButtons(hudManager);
        _questionButton = new CustomButton(
            OnQuestionButtonClick,
            IsLocalPlayerAndAlive,
            CouldUseQuestionButton,
            ResetQuestionButton,
            GetQuestionSprite(),
            CustomButton.ButtonPositions.lowerRowRight,
            hudManager,
            "ActionQuaternary",
            true,
            QuestionDuration,
            OnQuestionButtonEffectEnd
        );
    }

    private void OnQuestionButtonEffectEnd()
    {
        if (_questionButton == null) return;
        _questionButton.Timer = _questionButton.MaxTimer;
        if (DeadTarget == null || DeadTarget.player == null) return;
        var msg = GetInfo(DeadTarget.player, DeadTarget.killerIfExisting);
        FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(CachedPlayer.LocalPlayer.PlayerControl, msg);

        // Ghost Info
        var writer = AmongUsClient.Instance.StartRpcImmediately(
            CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.ShareGhostInfo,
            Hazel.SendOption.Reliable, -1);
        writer.Write(DeadTarget.player.PlayerId);
        writer.Write((byte)RPCProcedure.GhostInfoTypes.MediumInfo);
        writer.Write(msg);
        AmongUsClient.Instance.FinishRpcImmediately(writer);

        // Remove soul
        if (OneQuestionPerSoul)
        {
            var closestDistance = float.MaxValue;
            SpriteRenderer? target = null;

            foreach ((DeadPlayer db, Vector3 ps) in DeadBodies)
            {
                if (db != DeadTarget) continue;
                var deadBody = Tuple.Create(db, ps);
                DeadBodies.Remove(deadBody);
                break;
            }

            foreach (var rend in Souls)
            {
                var distance = Vector2.Distance(rend.transform.position,
                    CachedPlayer.LocalPlayer.PlayerControl.GetTruePosition());
                if (!(distance < closestDistance)) continue;
                closestDistance = distance;
                target = rend;
            }

            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(5f,
                new Action<float>((p) =>
                {
                    if (target != null)
                    {
                        var tmp = target.color;
                        tmp.a = Mathf.Clamp01(1 - p);
                        target.color = tmp;
                    }

                    if (p == 1f && target != null && target.gameObject != null)
                        UnityEngine.Object.Destroy(target.gameObject);
                })));
            if (target != null) Souls.Remove(target);
        }

        SoundEffectsManager.stop("mediumAsk");
    }

    private void ResetQuestionButton()
    {
        if (_questionButton == null) return;
        _questionButton.Timer = _questionButton.MaxTimer;
        _questionButton.isEffectActive = false;
        SoulTarget = null;
    }

    private bool CouldUseQuestionButton()
    {
        if (_questionButton == null) return false;
        if (_questionButton.isEffectActive && DeadTarget != SoulTarget)
        {
            SoulTarget = null;
            _questionButton.Timer = 0f;
            _questionButton.isEffectActive = false;
        }

        return DeadTarget != null && CachedPlayer.LocalPlayer.PlayerControl.CanMove;
    }

    private void OnQuestionButtonClick()
    {
        if (DeadTarget == null || _questionButton == null) return;
        SoulTarget = DeadTarget;
        _questionButton.HasEffect = true;
        SoundEffectsManager.play("mediumAsk");
    }

    public string GetInfo(PlayerControl target, PlayerControl killer)
    {
        var msg = "";
        if (DeadTarget == null) return msg;

        var infos = new List<SpecialMediumInfo>();
        // collect fitting death info types.
        // suicides:
        if (killer == target)
        {
            if (Singleton<Sheriff>.Instance.Is(target) || Singleton<Deputy>.Instance.Is(target))
            {
                infos.Add(SpecialMediumInfo.SheriffSuicide);
            }

            if (target == Lovers.lover1 || target == Lovers.lover2)
            {
                infos.Add(SpecialMediumInfo.PassiveLoverSuicide);
            }

            if (Singleton<Thief>.Instance.Is(target))
            {
                infos.Add(SpecialMediumInfo.ThiefSuicide);
            }

            if (Singleton<Warlock>.Instance.Is(target))
            {
                infos.Add(SpecialMediumInfo.WarlockSuicide);
            }
        }
        else
        {
            if (target == Lovers.lover1 || target == Lovers.lover2)
            {
                infos.Add(SpecialMediumInfo.ActiveLoverDies);
            }

            if (target.Data.Role.IsImpostor && killer.Data.Role.IsImpostor && !Singleton<Thief>.Instance.Is(target))
            {
                infos.Add(SpecialMediumInfo.ImpostorTeamKill);
            }
        }

        if ((Singleton<Sidekick>.Instance.Is(target) || Singleton<Jackal>.Instance.FutureSidekick == target) &&
            Singleton<Jackal>.Instance.Is(killer))
        {
            infos.Add(SpecialMediumInfo.JackalKillsSidekick);
        }

        if (Singleton<Lawyer>.Instance.Is(target) && Singleton<Lawyer>.Instance.Target == killer)
        {
            infos.Add(SpecialMediumInfo.LawyerKilledByClient);
        }

        if (DeadTarget.wasCleaned)
        {
            infos.Add(SpecialMediumInfo.BodyCleaned);
        }

        if (infos.Count > 0)
        {
            var selectedInfo = infos[TheOtherRoles.rnd.Next(infos.Count)];
            msg = selectedInfo switch
            {
                SpecialMediumInfo.SheriffSuicide => "Yikes, that Sheriff/Deputy shot backfired.",
                SpecialMediumInfo.WarlockSuicide => "MAYBE I cursed the person next to me and killed myself. Oops.",
                SpecialMediumInfo.ThiefSuicide =>
                    "I tried to steal the gun from their pocket, but they were just happy to see me.",
                SpecialMediumInfo.ActiveLoverDies => "I wanted to get out of this toxic relationship anyways.",
                SpecialMediumInfo.PassiveLoverSuicide => "The love of my life died, thus with a kiss I die.",
                SpecialMediumInfo.LawyerKilledByClient => "My client killed me. Do I still get paid?",
                SpecialMediumInfo.JackalKillsSidekick =>
                    "First they sidekicked me, then they killed me. At least I don't need to do tasks anymore.",
                SpecialMediumInfo.ImpostorTeamKill => "I guess they confused me for the Spy, is there even one?",
                SpecialMediumInfo.BodyCleaned => "Is my dead body some kind of art now or... aaand it's gone.",
                _ => msg
            };
        }
        else
        {
            var randomNumber = TheOtherRoles.rnd.Next(4);
            var typeOfColor = Helpers.isLighterColor(DeadTarget.killerIfExisting.Data.DefaultOutfit.ColorId)
                ? "lighter"
                : "darker";
            var timeSinceDeath = (float)(MeetingStartTime - DeadTarget.timeOfDeath).TotalMilliseconds;
            var targetRole = GetRoleByPlayer(DeadTarget.player);
            var targetRoleName = targetRole != null
                ? targetRole.NameText
                : DeadTarget.player.Data.Role.IsImpostor
                    ? "impostor"
                    : "crewmate";
            var killerRole = DeadTarget.killerIfExisting != null ? GetRoleByPlayer(DeadTarget.killerIfExisting) : null;
            var killerRoleName = DeadTarget.killerIfExisting == null
                ? "unknown"
                : killerRole != null
                    ? killerRole.NameText
                    : DeadTarget.killerIfExisting.Data.Role.IsImpostor
                        ? "impostor"
                        : "crewmate";

            if (randomNumber == 0)
            {
                msg = $"If my role hasn't been saved, there's no {targetRoleName} in the game anymore.";
            }
            else if (randomNumber == 1)
            {
                msg = $"I'm not sure, but I guess a {typeOfColor} color killed me.";
            }
            else if (randomNumber == 2)
            {
                msg =
                    $"If I counted correctly, I died {Math.Round(timeSinceDeath / 1000)}s before the next meeting started.";
            }
            else
            {
                msg = $"It seems like my killer was the {killerRoleName}.";
            }
        }

        if (TheOtherRoles.rnd.Next(100) <= AdditionalInfoChance)
        {
            int count;
            string condition;
            var alivePlayersList = PlayerControl.AllPlayerControls.ToArray().Where(pc => !pc.Data.IsDead);
            switch (TheOtherRoles.rnd.Next(3))
            {
                case 0:
                    count = alivePlayersList.Count(pc =>
                        pc.Data.Role.IsImpostor || Singleton<Jackal>.Instance.Is(pc) ||
                        Singleton<Sheriff>.Instance.Is(pc) || (Singleton<Deputy>.Instance.Is(pc) &&
                                                               Singleton<Deputy>.Instance.KillButtonEnabled) ||
                        Singleton<Thief>.Instance.Is(pc));
                    condition = $"killer{(count is 0 or > 1 ? "s" : "")}";
                    break;
                case 1:
                    count = alivePlayersList.Count(pc =>
                        pc.Data.Role.IsImpostor || Singleton<Engineer>.Instance.Is(pc) ||
                        (Singleton<Jackal>.Instance.Is(pc) && Singleton<Jackal>.Instance.CanUseVents) ||
                        (Singleton<Thief>.Instance.Is(pc) && Singleton<Thief>.Instance.CanUseVents) ||
                        (Singleton<Vulture>.Instance.Is(pc) && Singleton<Vulture>.Instance.CanUseVents) ||
                        (Singleton<Spy>.Instance.Is(pc) && Singleton<Spy>.Instance.CanEnterVents));
                    condition = $"player{(count is 0 or > 1 ? "s" : "")} who can use vents";
                    break;
                default:
                    count = alivePlayersList.Count(pc =>
                        GetRoleByPlayer(pc)?.IsNeutral == true && !Singleton<Jackal>.Instance.Is(pc) &&
                        Singleton<Thief>.Instance.Is(pc));
                    condition =
                        $"player{(count is 0 or > 1 ? "s" : "")} who {(count is 0 or > 1 ? "are" : "is")} neutral but cannot kill";
                    break;
            }

            msg += $"\nWhen you asked, {count} " + condition + (count == 1 ? " was" : " were") + " still alive";
        }

        return DeadTarget.player.Data.PlayerName + "'s Soul:\n" + msg;
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        DeadTarget = null;
        SoulTarget = null;
        DeadBodies.Clear();
        FeatureDeadBodies.Clear();
        Souls.Clear();
        MeetingStartTime = DateTime.UtcNow;
    }
}