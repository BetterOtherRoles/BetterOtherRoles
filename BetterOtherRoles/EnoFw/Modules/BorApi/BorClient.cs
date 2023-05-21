using System.Collections.Generic;
using System.Linq;
using BetterOtherRoles.EnoFw.Utils;
using BetterOtherRoles.Players;
using HarmonyLib;
using InnerNet;
using UnityEngine;

namespace BetterOtherRoles.EnoFw.Modules.BorApi;

public class BorClient : AbstractSocketClient<BorClient.Events>
{
#if DEBUG
    private const string Endpoint = "http://127.0.0.1:3000";
#endif
#if RELEASE
    private const string Endpoint = "https://neptune.goeno.cloud:3443";
#endif

    public static readonly BorClient Instance = new();

    public readonly Dictionary<string, PublicAccountInfo> PublicAccountInfos = new();

    public MyAccount MyAccount { get; private set; }

    public static void Load()
    {
        if (!Instance.Connected)
        {
            new DeferrableAction(
                () => { Instance.Connect(EOSManager.Instance.FriendCode, EOSManager.Instance.ProductUserId); },
                () => EOSManager.InstanceExists && EOSManager.Instance.FriendCode is not (null or "") &&
                      EOSManager.Instance.ProductUserId is not
                          (null or "")).Start();
        }
    }

    private BorClient() : base(Endpoint)
    {
        On<string>(Events.Error, OnError);
        On<List<PublicAccountInfo>>(Events.PublicAccountInfos, OnPublicAccountInfos);
        On<MyAccount>(Events.MyAccount, OnMyAccount);
    }

    private void OnMyAccount(MyAccount myAccount)
    {
        MyAccount = myAccount;
    }

    private void OnPublicAccountInfos(List<PublicAccountInfo> infos)
    {
        BetterOtherRolesPlugin.Logger.LogInfo("OnPublicAccountInfos");
        foreach (var info in infos)
        {
            PublicAccountInfos[info.FriendCode] = info;
        }
    }

    private void OnError(string error)
    {
        BetterOtherRolesPlugin.Logger.LogError(error);
    }

    public void SendKillDone(PlayerControl target)
    {
        var friendCode = CustomLobby.GetFriendCode(target);
        if (friendCode == null) return;
        Emit(Events.MurderPlayer, friendCode);
    }

    public void SubscribeToAccounts(List<string> friendCodes)
    {
        if (!Connected) return;
        Emit(Events.SubscribeToAccounts, friendCodes);
    }

    public void SendTaskCompleted(PlayerTask task)
    {
        if (!Connected) return;
        Emit(Events.TaskDone, task.name);
    }

    public async void Connect(string friendCode, string clientId)
    {
        var token = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{friendCode}:{clientId}"));
        var query = Io.Options.Query.ToDictionary(v => v.Key, v => v.Value);
        BetterOtherRolesPlugin.Logger.LogInfo($"{friendCode}:{clientId}");
        query.Add("token", token);
        Io.Options.Query = query;
        await Io.ConnectAsync();
    }

    [HarmonyPatch(typeof(NormalPlayerTask), nameof(NormalPlayerTask.Complete))]
    internal static class PlayerTaskCompletePatch
    {
        private static void Postfix(NormalPlayerTask __instance)
        {
#if RELEASE
            if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started) return;
            if (CachedPlayer.LocalPlayer == null || __instance.Owner.PlayerId != CachedPlayer.LocalPlayer.PlayerId) return;
#endif
            BetterOtherRolesPlugin.Logger.LogInfo($"Task completed: {__instance.name}");
            Instance.SendTaskCompleted(__instance);
        }
    }

    public enum Events : uint
    {
        Error = 0,
        SubscribeToAccounts,
        PublicAccountInfos,
        TaskDone,
        MurderPlayer,
        WinGame,
        MyAccount
    }
}