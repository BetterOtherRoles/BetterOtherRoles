using System.Collections.Generic;
using System.Linq;
using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.EnoFramework.Utils;
using TheOtherRoles.Objects;
using TheOtherRoles.Players;
using UnityEngine;
using Resources = TheOtherRoles.EnoFramework.Utils.Resources;

namespace TheOtherRoles.Customs.Roles.Neutral;

[EnoSingleton]
public class Vulture : CustomRole
{
    private Sprite? _eatSprite;

    private CustomButton? _eatButton;

    public readonly EnoFramework.CustomOption EatCooldown;
    public readonly EnoFramework.CustomOption EatNumberToWin;
    public readonly EnoFramework.CustomOption CanUseVents;
    public readonly EnoFramework.CustomOption ShowArrows;

    public readonly List<Arrow> Arrows = new();
    public int EatenBodies;

    public Vulture() : base(nameof(Vulture))
    {
        Team = Teams.Neutral;
        Color = new Color32(139, 69, 19, byte.MaxValue);
        CanTarget = false;

        EatCooldown = OptionsTab.CreateFloatList(
            $"{Name}{nameof(EatCooldown)}",
            Colors.Cs(Color, "Eat cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        EatNumberToWin = OptionsTab.CreateFloatList(
            $"{Name}{nameof(EatNumberToWin)}",
            Colors.Cs(Color, "Number of corpses needed to be eaten"),
            1f,
            10f,
            4f,
            1f,
            SpawnRate,
            string.Empty,
            "s");
        CanUseVents = OptionsTab.CreateBool(
            $"{Name}{nameof(CanUseVents)}",
            Colors.Cs(Color, "Can use vents"),
            true,
            SpawnRate);
        ShowArrows = OptionsTab.CreateBool(
            $"{Name}{nameof(ShowArrows)}",
            Colors.Cs(Color, "Show arrows pointing towards the corpses"),
            true,
            SpawnRate);
    }

    public Sprite GetEatSprite()
    {
        if (_eatSprite == null)
        {
            _eatSprite = Resources.LoadSpriteFromResources("TheOtherRoles.Resources.VultureButton.png", 115f);
        }

        return _eatSprite;
    }

    public override void CreateCustomButtons(HudManager hudManager)
    {
        base.CreateCustomButtons(hudManager);
        _eatButton = new CustomButton(
            OnEatButtonClick,
            HasEatButton,
            CouldUseEatButton,
            ResetEatButton,
            GetEatSprite(),
            CustomButton.ButtonPositions.lowerRowCenter,
            hudManager,
            "ActionQuaternary"
        );
    }

    private void ResetEatButton()
    {
        if (_eatButton == null) return;
        _eatButton.Timer = _eatButton.MaxTimer;
    }

    private bool CouldUseEatButton()
    {
        if (_eatButton == null) return false;
        return _eatButton.hudManager.ReportButton.graphic.color == Palette.EnabledColor &&
               CachedPlayer.LocalPlayer.PlayerControl.CanMove;
    }

    private bool HasEatButton()
    {
        return Player != null && Player == CachedPlayer.LocalPlayer.PlayerControl &&
               !CachedPlayer.LocalPlayer.Data.IsDead;
    }

    private void OnEatButtonClick()
    {
        if (Player == null || _eatButton == null) return;
        foreach (var collider2D in Physics2D.OverlapCircleAll(CachedPlayer.LocalPlayer.PlayerControl.GetTruePosition(),
                     CachedPlayer.LocalPlayer.PlayerControl.MaxReportDistance, Constants.PlayersOnlyMask))
        {
            if (collider2D.tag != "DeadBody") continue;
            var component = collider2D.GetComponent<DeadBody>();
            if (!component || component.Reported) continue;
            var truePosition = CachedPlayer.LocalPlayer.PlayerControl.GetTruePosition();
            var truePosition2 = component.TruePosition;
            if (!(Vector2.Distance(truePosition2, truePosition) <=
                  CachedPlayer.LocalPlayer.PlayerControl.MaxReportDistance) ||
                !CachedPlayer.LocalPlayer.PlayerControl.CanMove || PhysicsHelpers.AnythingBetween(truePosition,
                    truePosition2, Constants.ShipAndObjectsMask, false)) continue;
            var playerInfo = GameData.Instance.GetPlayerById(component.ParentId);
            Rpc.CleanDeadBody(Player.PlayerId, playerInfo.PlayerId);
            _eatButton.Timer = _eatButton.MaxTimer;
            SoundEffectsManager.play("vultureEat");
            break;
        }
    }

    public override void OnPlayerUpdate(PlayerControl player)
    {
        base.OnPlayerUpdate(player);
        if (Player == null || !Is(CachedPlayer.LocalPlayer) || !ShowArrows) return;
        if (Player.Data.IsDead)
        {
            ClearArrows();
            return;
        }
        DeadBody[] deadBodies = UnityEngine.Object.FindObjectsOfType<DeadBody>();
        var arrowNeedUpdate = Arrows.Count != deadBodies.Length;
        var index = 0;
        if (arrowNeedUpdate)
        {
            ClearArrows();
        }

        foreach (var body in deadBodies)
        {
            if (arrowNeedUpdate)
            {
                Arrows.Add(new Arrow(Color.blue));
                Arrows[index].arrow.SetActive(true);
            }
            Arrows[index].Update(body.transform.position);
            index++;
        }
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        EatenBodies = 0;
        TriggerWin = false;
        ClearArrows();
    }

    private void ClearArrows()
    {
        if (Arrows.Count == 0) return;
        foreach (var arrow in Arrows.Where(arrow => arrow?.arrow != null))
        {
            UnityEngine.Object.Destroy(arrow.arrow);
        }
        Arrows.Clear();
    }
}