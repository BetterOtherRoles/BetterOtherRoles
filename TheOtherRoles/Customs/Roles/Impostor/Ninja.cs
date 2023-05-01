using Reactor.Networking.Attributes;
using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.EnoFramework.Utils;
using TheOtherRoles.Objects;
using TheOtherRoles.Players;
using UnityEngine;
using Resources = TheOtherRoles.EnoFramework.Utils.Resources;

namespace TheOtherRoles.Customs.Roles.Impostor;

[EnoSingleton]
public class Ninja : CustomRole
{
    private Sprite? _markSprite;
    private Sprite? _killSprite;

    public Arrow? Arrow = new(Color.black);

    private CustomButton? _ninjaButton;

    public readonly EnoFramework.CustomOption NinjaCooldown;
    public readonly EnoFramework.CustomOption KnowsTargetLocation;
    public readonly EnoFramework.CustomOption TraceDuration;
    public readonly EnoFramework.CustomOption TraceColorDuration;
    public readonly EnoFramework.CustomOption InvisibilityDuration;

    public PlayerControl? MarkedTarget;
    public bool IsInvisible;
    public float InvisibilityTimer;

    public Ninja() : base(nameof(Ninja))
    {
        Team = Teams.Impostor;
        Color = Palette.ImpostorRed;
        CanTarget = true;

        IntroDescription = "Surprise and assassinate your foes";
        ShortDescription = "Surprise and assassinate your foes";

        NinjaCooldown = OptionsTab.CreateFloatList(
            $"{Name}{nameof(NinjaCooldown)}",
            Cs($"{Name} cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        KnowsTargetLocation = OptionsTab.CreateBool(
            $"{Name}{nameof(KnowsTargetLocation)}",
            Cs("Knows location of target"),
            true,
            SpawnRate);
        TraceDuration = OptionsTab.CreateFloatList(
            $"{Name}{nameof(TraceDuration)}",
            Cs("Trace duration"),
            1f,
            20f,
            5f,
            0.5f,
            SpawnRate,
            string.Empty,
            "s");
        TraceColorDuration = OptionsTab.CreateFloatList(
            $"{Name}{nameof(TraceColorDuration)}",
            Cs("Time till trace color has faded"),
            0f,
            20f,
            2f,
            0.5f,
            SpawnRate,
            string.Empty,
            "s");
        InvisibilityDuration = OptionsTab.CreateFloatList(
            $"{Name}{nameof(InvisibilityDuration)}",
            Cs("Invisibility duration"),
            0f,
            20f,
            3f,
            1f,
            SpawnRate,
            string.Empty,
            "s");
    }

    public Sprite GetMarkSprite()
    {
        if (_markSprite == null)
        {
            _markSprite = Resources.LoadSpriteFromResources("TheOtherRoles.Resources.NinjaMarkButton.png", 115f);
        }

        return _markSprite;
    }

    public Sprite GetKillSprite()
    {
        if (_killSprite == null)
        {
            _killSprite = Resources.LoadSpriteFromResources("TheOtherRoles.Resources.NinjaAssassinateButton.png", 115f);
        }

        return _killSprite;
    }

    public override void CreateCustomButtons(HudManager hudManager)
    {
        base.CreateCustomButtons(hudManager);
        _ninjaButton = new CustomButton(
            OnNinjaButtonClick,
            HasNinjaButton,
            CouldUseNinjaButton,
            ResetNinjaButton,
            GetMarkSprite(),
            CustomButton.ButtonPositions.upperRowLeft,
            hudManager,
            "ActionQuaternary"
        );
    }

    private void ResetNinjaButton()
    {
        if (_ninjaButton == null) return;
        _ninjaButton.Timer = _ninjaButton.MaxTimer;
        MarkedTarget = null;
    }

    private bool CouldUseNinjaButton()
    {
        if (_ninjaButton == null) return false;
        _ninjaButton.Sprite = MarkedTarget != null
            ? GetKillSprite()
            : GetMarkSprite();
        return (CurrentTarget != null || MarkedTarget != null) &&
               CachedPlayer.LocalPlayer.PlayerControl.CanMove;
    }

    private bool HasNinjaButton()
    {
        return Player != null && Player == CachedPlayer.LocalPlayer.PlayerControl &&
               !CachedPlayer.LocalPlayer.Data.IsDead;
    }

    private void OnNinjaButtonClick()
    {
        if (Player == null || _ninjaButton == null) return;
        if (MarkedTarget != null)
        {
            // Murder attempt with teleport
            var attempt = Helpers.checkMurderAttempt(Player, MarkedTarget);
            if (attempt == MurderAttemptResult.PerformKill)
            {
                // Create first trace before killing
                var pos = CachedPlayer.LocalPlayer.transform.position;
                PlaceTrace(CachedPlayer.LocalPlayer, $"{pos.x}|{pos.y}");
                Rpc.SetInvisibility(Player, false);
                // Perform Kill
                if (SubmergedCompatibility.IsSubmerged)
                {
                    SubmergedCompatibility.ChangeFloor(MarkedTarget.transform.localPosition.y > -7);
                }
                Rpc.UncheckedMurderPlayer(CachedPlayer.LocalPlayer, MarkedTarget, true);
                // Create Second trace after killing
                pos = MarkedTarget.transform.position;
                PlaceTrace(CachedPlayer.LocalPlayer, $"{pos.x}|{pos.y}");
            }

            switch (attempt)
            {
                case MurderAttemptResult.BlankKill or MurderAttemptResult.PerformKill:
                    _ninjaButton.Timer = _ninjaButton.MaxTimer;
                    Player.killTimer = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
                    break;
                case MurderAttemptResult.SuppressKill:
                    _ninjaButton.Timer = 0f;
                    break;
            }

            MarkedTarget = null;
            return;
        }

        if (CurrentTarget == null) return;
        MarkedTarget = CurrentTarget;
        _ninjaButton.Timer = 5f;
        SoundEffectsManager.play("warlockCurse");

        // Ghost Info
        var writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
            (byte)CustomRPC.ShareGhostInfo, Hazel.SendOption.Reliable, -1);
        writer.Write(CachedPlayer.LocalPlayer.PlayerId);
        writer.Write((byte)RPCProcedure.GhostInfoTypes.NinjaMarked);
        writer.Write(MarkedTarget.PlayerId);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }
    
    [MethodRpc((uint)Rpc.Id.NinjaPlaceTrace)]
    private static void PlaceTrace(PlayerControl sender, string rawData)
    {
        var rawPos = rawData.Split("|");
        var position = Vector3.zero;
        position.x = float.Parse(rawPos[0]);
        position.y = float.Parse(rawPos[1]);
        var _ = new NinjaTrace(position, Singleton<Ninja>.Instance.TraceDuration);
        if (!Singleton<Ninja>.Instance.Is(CachedPlayer.LocalPlayer.PlayerControl))
        {
            Singleton<Ninja>.Instance.MarkedTarget = null;
        }
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        MarkedTarget = null;
        IsInvisible = false;
        InvisibilityTimer = 0f;
        if (Arrow?.arrow != null) UnityEngine.Object.Destroy(Arrow.arrow);
        Arrow = new Arrow(Color.black);
        if (Arrow.arrow != null) Arrow.arrow.SetActive(true);
    }
}