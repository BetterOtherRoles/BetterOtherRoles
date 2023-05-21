using System;
using System.Collections.Generic;
using BetterOtherRoles.EnoFw.Kernel;
using BetterOtherRoles.EnoFw.Roles.Neutral;
using BetterOtherRoles.EnoFw.Libs.Reactor.Networking.Attributes;
using UnityEngine;
using Option = BetterOtherRoles.EnoFw.Kernel.CustomOption;

namespace BetterOtherRoles.EnoFw.Roles.Crewmate;

public class Pursuer : AbstractRole
{
    public static readonly Pursuer Instance = new();

    // Fields
    public int UsedBlanks;
    public bool NotAckedExiled;
    public readonly List<PlayerControl> BlankedList = new();

    // Options
    public readonly CustomOption BlankCooldown;
    public readonly CustomOption BlankNumber;

    public static Sprite BlankButtonSprite => GetSprite("BetterOtherRoles.Resources.PursuerButton.png", 115f);

    private Pursuer() : base(nameof(Pursuer), "Pursuer", false)
    {
        Team = Teams.Crewmate;
        Color = new Color32(134, 153, 25, byte.MaxValue);
        CanTarget = true;

        BlankCooldown = CustomOptions.NeutralSettings.CreateFloatList(
            $"{Name}{nameof(BlankCooldown)}",
            Cs("Pursuer blank cooldown"),
            10f,
            60f,
            30f,
            2.5f,
            Lawyer.Instance.SpawnRate,
            string.Empty,
            "s");
        BlankNumber = CustomOptions.NeutralSettings.CreateFloatList(
            $"{Name}{nameof(BlankNumber)}",
            Cs("Pursuer blank amount"),
            0f,
            10f,
            5f,
            1f,
            Lawyer.Instance.SpawnRate);
    }

    public override void ClearAndReload()
    {
        base.ClearAndReload();
        UsedBlanks = 0;
        NotAckedExiled = false;
        BlankedList.Clear();
    }

    public static void SetBlanked(byte playerId, bool add)
    {
        var data = new Tuple<byte, bool>(playerId, add);
        Rpc_SetBlanked(PlayerControl.LocalPlayer, Rpc.Serialize(data));
    }

    [MethodRpc((uint)Rpc.Role.SetBlanked)]
    private static void Rpc_SetBlanked(PlayerControl sender, string rawData)
    {
        var (playerId, add) = Rpc.Deserialize<Tuple<byte, bool>>(rawData);

        var blankTarget = Helpers.playerById(playerId);
        if (blankTarget == null) return;
        Instance.BlankedList.RemoveAll(x => x.PlayerId == playerId);
        if (add) Instance.BlankedList.Add(blankTarget);
    }
}