using System.Collections.Generic;
using System.Linq;
using Reactor.Networking.Attributes;
using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.EnoFramework.Utils;
using TheOtherRoles.Objects;
using TheOtherRoles.Players;
using UnityEngine;
using Resources = TheOtherRoles.EnoFramework.Utils.Resources;

namespace TheOtherRoles.Customs.Roles.Neutral;

[EnoSingleton]
public class Arsonist : CustomRole
{
    private Sprite? _douseSprite;
    private Sprite? _igniteSprite;

    private CustomButton? _douseButton;

    public readonly EnoFramework.CustomOption DouseCooldown;
    public readonly EnoFramework.CustomOption DouseDuration;

    public PlayerControl? DouseTarget;
    public List<PlayerControl> DousedPlayers = new();

    public Arsonist() : base(nameof(Arsonist))
    {
        Team = Teams.Neutral;
        Color = new Color32(238, 112, 46, byte.MaxValue);
        CanTarget = true;

        DouseCooldown = OptionsTab.CreateFloatList(
            $"{Name}{nameof(DouseCooldown)}",
            Colors.Cs(Color, "Douse cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        DouseDuration = OptionsTab.CreateFloatList(
            $"{Name}{nameof(DouseDuration)}",
            Colors.Cs(Color, "Douse duration"),
            0f,
            10f,
            1f,
            1f,
            SpawnRate,
            string.Empty,
            "s");
    }

    public Sprite GetDouseSprite()
    {
        if (_douseSprite == null)
        {
            _douseSprite = Resources.LoadSpriteFromResources("TheOtherRoles.Resources.DouseButton.png", 115f);
        }

        return _douseSprite;
    }

    public Sprite GetIgniteSprite()
    {
        if (_igniteSprite == null)
        {
            _igniteSprite = Resources.LoadSpriteFromResources("TheOtherRoles.Resources.IgniteButton.png", 115f);
        }

        return _igniteSprite;
    }

    public override void CreateCustomButtons(HudManager hudManager)
    {
        base.CreateCustomButtons(hudManager);
        _douseButton = new CustomButton(
            OnDouseButtonClick,
            HasDouseButton,
            CouldUseDouseButton,
            ResetDouseButton,
            GetDouseSprite(),
            CustomButton.ButtonPositions.lowerRowRight,
            hudManager,
            "ActionQuaternary",
            true,
            DouseDuration,
            OnDouseButtonEffectEnd
        );
    }

    private void OnDouseButtonEffectEnd()
    {
        if (_douseButton == null) return;
        if (DouseTarget != null) DousedPlayers.Add(DouseTarget);

        _douseButton.Timer = DousedEveryoneAlive() ? 0 : _douseButton.MaxTimer;

        foreach (var p in DousedPlayers)
        {
            if (!TORMapOptions.playerIcons.TryGetValue(p.PlayerId, out var icon)) continue;
            if (icon == null) continue;
            icon.setSemiTransparent(false);
        }

        // Ghost Info
        if (DouseTarget == null) return;
        var writer = AmongUsClient.Instance.StartRpcImmediately(
            CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.ShareGhostInfo,
            Hazel.SendOption.Reliable, -1);
        writer.Write(CachedPlayer.LocalPlayer.PlayerId);
        writer.Write((byte)RPCProcedure.GhostInfoTypes.ArsonistDouse);
        writer.Write(DouseTarget.PlayerId);
        AmongUsClient.Instance.FinishRpcImmediately(writer);

        DouseTarget = null;
    }

    private void ResetDouseButton()
    {
        if (_douseButton == null) return;
        _douseButton.Timer = _douseButton.MaxTimer;
        _douseButton.isEffectActive = false;
        DouseTarget = null;
    }

    private bool CouldUseDouseButton()
    {
        if (_douseButton == null) return false;
        var dousedEveryoneAlive = DousedEveryoneAlive();
        if (dousedEveryoneAlive) _douseButton.actionButton.graphic.sprite = GetIgniteSprite();

        if (_douseButton.isEffectActive && DouseTarget != CurrentTarget)
        {
            DouseTarget = null;
            _douseButton.Timer = 0f;
            _douseButton.isEffectActive = false;
        }

        return CachedPlayer.LocalPlayer.PlayerControl.CanMove &&
               (dousedEveryoneAlive || CurrentTarget != null);
    }

    private bool HasDouseButton()
    {
        return Player != null && Is(CachedPlayer.LocalPlayer.PlayerControl) && !CachedPlayer.LocalPlayer.Data.IsDead;
    }

    private void OnDouseButtonClick()
    {
        if (_douseButton == null) return;
        var dousedEveryoneAlive = DousedEveryoneAlive();
        if (dousedEveryoneAlive)
        {
            TriggerArsonistWin(CachedPlayer.LocalPlayer);
            _douseButton.HasEffect = false;
        }
        else if (CurrentTarget != null)
        {
            DouseTarget = CurrentTarget;
            _douseButton.HasEffect = true;
            SoundEffectsManager.play("arsonistDouse");
        }
    }

    public bool DousedEveryoneAlive()
    {
        return CachedPlayer.AllPlayers.All(p =>
            p.PlayerControl == Player || p.Data.IsDead || p.Data.Disconnected ||
            DousedPlayers.Any(dp => dp.PlayerId == p.PlayerId));
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        DouseTarget = null;
        TriggerWin = false;
        foreach (var p in TORMapOptions.playerIcons.Values.Where(p => p != null && p.gameObject != null))
        {
            p.gameObject.SetActive(false);
        }
    }

    [MethodRpc((uint)Rpc.Id.ArsonistTriggerWin)]
    private static void TriggerArsonistWin(PlayerControl sender)
    {
        Singleton<Arsonist>.Instance.TriggerWin = true;
        var nonArsonistPlayers =
            CachedPlayer.AllPlayers.Where(p => p.PlayerControl != Singleton<Arsonist>.Instance.Player);
        foreach (PlayerControl p in nonArsonistPlayers)
        {
            p.Exiled();
        }
    }
}