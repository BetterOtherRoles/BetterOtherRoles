using Reactor.Networking.Attributes;
using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.EnoFramework.Utils;
using TheOtherRoles.Objects;
using TheOtherRoles.Players;
using UnityEngine;
using Resources = TheOtherRoles.EnoFramework.Utils.Resources;

namespace TheOtherRoles.Customs.Roles.Impostor;

[EnoSingleton]
public class Bomber : CustomRole
{
    private Sprite? _bombSprite;

    private CustomButton? _bombButton;
    private CustomButton? _defuseButton;

    public readonly EnoFramework.CustomOption BombCooldown;
    public readonly EnoFramework.CustomOption BombDestructionTime;
    public readonly EnoFramework.CustomOption BombActivationTime;
    public readonly EnoFramework.CustomOption BombDestructionRange;
    public readonly EnoFramework.CustomOption BombHearRange;
    public readonly EnoFramework.CustomOption BombDefuseDuration;

    public Bomb? Bomb;
    public bool IsBombPlanted;
    public bool IsBombActive;

    public Bomber() : base(nameof(Bomber))
    {
        Team = Teams.Impostor;
        Color = Palette.ImpostorRed;
        CanTarget = true;

        IntroDescription = "Bomb all Crewmates";
        ShortDescription = "Bomb all Crewmates";

        BombCooldown = OptionsTab.CreateFloatList(
            $"{Name}{nameof(BombCooldown)}",
            Cs("Plant bomb cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            SpawnRate,
            string.Empty,
            "s");
        BombDestructionTime = OptionsTab.CreateFloatList(
            $"{Name}{nameof(BombDestructionTime)}",
            Cs("Bomb explosion time"),
            2f,
            60f,
            20f,
            1f,
            SpawnRate,
            string.Empty,
            "s");
        BombActivationTime = OptionsTab.CreateFloatList(
            $"{Name}{nameof(BombActivationTime)}",
            Cs("Bomb activation time"),
            2f,
            30f,
            3f,
            0.5f,
            SpawnRate,
            string.Empty,
            "s");
        BombDestructionRange = OptionsTab.CreateFloatList(
            $"{Name}{nameof(BombDestructionRange)}",
            Cs("Bomb explosion range"),
            5f,
            150f,
            50f,
            5f,
            SpawnRate);
        BombHearRange = OptionsTab.CreateFloatList(
            $"{Name}{nameof(BombHearRange)}",
            Cs("Bomb hear range"),
            5f,
            150f,
            50f,
            5f,
            SpawnRate);
        BombDefuseDuration = OptionsTab.CreateFloatList(
            $"{Name}{nameof(BombDefuseDuration)}",
            Cs("Bomb defuse duration"),
            0.5f,
            15f,
            3f,
            0.5f,
            SpawnRate,
            string.Empty,
            "s");
    }

    public Sprite GetBombSprite()
    {
        if (_bombSprite == null)
        {
            _bombSprite = Resources.LoadSpriteFromResources("TheOtherRoles.Resources.Bomb_Button_Plant.png", 115f);
        }

        return _bombSprite;
    }

    public override void CreateCustomButtons(HudManager hudManager)
    {
        base.CreateCustomButtons(hudManager);
        _bombButton = new CustomButton(
            OnBombButtonClick,
            HasBombButton,
            CouldUseBombButton,
            ResetBombButton,
            GetBombSprite(),
            CustomButton.ButtonPositions.upperRowLeft,
            hudManager,
            "ActionQuaternary",
            true,
            BombDestructionTime,
            OnBombButtonEffectEnd
        );
        _defuseButton = new CustomButton(
            OnDefuseButtonClick,
            HasDefuseButton,
            CouldUseDefuseButton,
            ResetDefuseButton,
            Bomb.getDefuseSprite(),
            new Vector3(0f, 1f, 0),
            hudManager,
            "defuseBomb",
            true,
            BombDefuseDuration,
            OnDefuseButtonEffectEnd,
            true
        );
    }

    private void OnDefuseButtonEffectEnd()
    {
        if (_defuseButton == null) return;
        DefuseBomb(CachedPlayer.LocalPlayer);

        _defuseButton.Timer = 0f;
        Bomb.canDefuse = false;
    }

    private void ResetDefuseButton()
    {
        if (_defuseButton == null) return;
        _defuseButton.Timer = 0f;
        _defuseButton.isEffectActive = false;
    }

    private bool CouldUseDefuseButton()
    {
        if (_defuseButton == null) return false;
        if (_defuseButton.isEffectActive && !Bomb.canDefuse)
        {
            _defuseButton.Timer = 0f;
            _defuseButton.isEffectActive = false;
        }

        return CachedPlayer.LocalPlayer.PlayerControl.CanMove;
    }

    private bool HasDefuseButton()
    {
        if (_defuseButton == null) return false;
        _defuseButton.PositionOffset = HudManagerStartPatch.shifterShiftButton.HasButton()
            ? new Vector3(0f, 2f, 0f)
            : new Vector3(0f, 1f, 0f);
        return Player != null && Bomb.canDefuse && !CachedPlayer.LocalPlayer.Data.IsDead;
    }

    private void OnDefuseButtonClick()
    {
        if (_defuseButton == null) return;
        _defuseButton.HasEffect = true;
    }

    private void OnBombButtonEffectEnd()
    {
        if (_bombButton == null) return;
        _bombButton.Timer = _bombButton.MaxTimer;
        _bombButton.isEffectActive = false;
        _bombButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
    }

    private void ResetBombButton()
    {
        if (_bombButton == null) return;
        _bombButton.Timer = _bombButton.MaxTimer;
    }

    private bool CouldUseBombButton()
    {
        return CachedPlayer.LocalPlayer.PlayerControl.CanMove && !IsBombPlanted;
    }

    private bool HasBombButton()
    {
        return Player != null && Is(CachedPlayer.LocalPlayer.PlayerControl) && !CachedPlayer.LocalPlayer.Data.IsDead;
    }

    private void OnBombButtonClick()
    {
        if (Player == null || _bombButton == null) return;
        if (Helpers.checkMurderAttempt(Player, Player) != MurderAttemptResult.BlankKill)
        {
            var pos = CachedPlayer.LocalPlayer.transform.position;
            PlantBomb(CachedPlayer.LocalPlayer, $"{pos.x}|{pos.y}");
            SoundEffectsManager.play("trapperTrap");
        }

        _bombButton.Timer = _bombButton.MaxTimer;
        IsBombPlanted = true;
    }

    public void ClearBomb(bool stopSound = true)
    {
        if (Bomb != null)
        {
            UnityEngine.Object.Destroy(Bomb.bomb);
            UnityEngine.Object.Destroy(Bomb.background);
            Bomb = null;
        }

        IsBombPlanted = false;
        IsBombActive = false;
        if (stopSound) SoundEffectsManager.stop("bombFuseBurning");
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        ClearBomb(false);
        Bomb.clearBackgroundSprite();
    }

    [MethodRpc((uint)Rpc.Id.BomberPlantBomb)]
    private static void PlantBomb(PlayerControl sender, string rawData)
    {
        if (Singleton<Bomber>.Instance.Player == null) return;
        var rawPos = rawData.Split("|");
        var position = Vector3.zero;
        position.x = float.Parse(rawPos[0]);
        position.y = float.Parse(rawPos[1]);
        var _ = new Bomb(position);
    }

    [MethodRpc((uint)Rpc.Id.BomberDefuseBomb)]
    private static void DefuseBomb(PlayerControl sender)
    {
        if (Singleton<Bomber>.Instance.Bomb != null)
        {
            SoundEffectsManager.playAtPosition("bombDefused", Singleton<Bomber>.Instance.Bomb.bomb.transform.position,
                range: Singleton<Bomber>.Instance.BombHearRange);
        }

        Singleton<Bomber>.Instance.ClearBomb();
        Singleton<Bomber>.Instance.OnBombButtonEffectEnd();
    }
}