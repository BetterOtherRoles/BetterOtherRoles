using Hazel;
using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.EnoFramework.Utils;
using TheOtherRoles.Objects;
using TheOtherRoles.Players;
using UnityEngine;
using Resources = TheOtherRoles.EnoFramework.Utils.Resources;

namespace TheOtherRoles.Customs.Roles.Impostor;

[EnoSingleton]
public class Cleaner : CustomRole
{
    private Sprite? _cleanSprite;

    private CustomButton? _cleanButton;

    public readonly EnoFramework.CustomOption CleanCooldown;

    public Cleaner() : base(nameof(Cleaner))
    {
        Team = Teams.Impostor;
        Color = Palette.ImpostorRed;
        CanTarget = true;

        CleanCooldown = OptionsTab.CreateFloatList(
            $"{Name}{nameof(CleanCooldown)}",
            Colors.Cs(Color, $"Clean cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
    }

    public Sprite GetCleanSprite()
    {
        if (_cleanSprite == null)
        {
            _cleanSprite = Resources.LoadSpriteFromResources("TheOtherRoles.Resources.CleanButton.png", 115f);
        }

        return _cleanSprite;
    }

    public override void CreateCustomButtons(HudManager hudManager)
    {
        base.CreateCustomButtons(hudManager);
        _cleanButton = new CustomButton(
            OnCleanButtonClick,
            HasCleanButton,
            CouldUseCleanButton,
            ResetCleanButton,
            GetCleanSprite(),
            CustomButton.ButtonPositions.upperRowLeft,
            hudManager,
            "ActionQuaternary"
        );
    }

    private void ResetCleanButton()
    {
        if (_cleanButton == null) return;
        _cleanButton.Timer = _cleanButton.MaxTimer;
    }

    private bool CouldUseCleanButton()
    {
        if (_cleanButton == null) return false;
        return _cleanButton.hudManager.ReportButton.graphic.color == Palette.EnabledColor &&
               CachedPlayer.LocalPlayer.PlayerControl.CanMove;
    }

    private bool HasCleanButton()
    {
        return Player != null && Player == CachedPlayer.LocalPlayer.PlayerControl &&
               !CachedPlayer.LocalPlayer.Data.IsDead;
    }

    private void OnCleanButtonClick()
    {
        if (_cleanButton == null || Player == null) return;
        foreach (var collider2D in Physics2D.OverlapCircleAll(
                     CachedPlayer.LocalPlayer.PlayerControl.GetTruePosition(),
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

            var writer = AmongUsClient.Instance.StartRpcImmediately(
                CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.CleanBody,
                Hazel.SendOption.Reliable, -1);
            writer.Write(playerInfo.PlayerId);
            writer.Write(Player.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.cleanBody(playerInfo.PlayerId, Player.PlayerId);

            Player.killTimer = _cleanButton.Timer = _cleanButton.MaxTimer;
            SoundEffectsManager.play("cleanerClean");
            break;
        }
    }
}