using System.Collections.Generic;
using System.Linq;
using BetterOtherRoles.EnoFw.Kernel;
using BetterOtherRoles.EnoFw.Utils;
using HarmonyLib;

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
    public List<MyPreset> MyPresets { get; private set; } = new();

    public static void Load()
    {
        if (!Instance.Connected)
        {
            DeferrableAction.Defer(InternalLoad, IsReadyToLoad);
        }
    }

    private static void InternalLoad()
    {
        Instance.Connect(EOSManager.Instance.FriendCode, EOSManager.Instance.ProductUserId);
    }

    private static bool IsReadyToLoad()
    {
        if (!EOSManager.InstanceExists) return false;
        return EOSManager.Instance.FriendCode is not (null or "") &&
               EOSManager.Instance.ProductUserId is not (null or "");
    }

    private BorClient() : base(Endpoint)
    {
        On<string>(Events.Error, OnError);
        On<List<PublicAccountInfo>>(Events.PublicAccountInfos, OnPublicAccountInfos);
        On<MyAccount>(Events.MyAccount, OnMyAccount);
        On<List<MyPreset>>(Events.MyPresets, OnMyPresets);
        On<List<CustomOptionValue>>(Events.LoadPreset, OnLoadPreset);
    }

    private static void OnLoadPreset(List<CustomOptionValue> optionValues)
    {
        foreach (var option in CustomOption.Tab.Options)
        {
            var opt = optionValues.Find(o => o.Key == option.Key);
            if (opt == null) continue;
            option.UpdateSelection(opt.Value, true);
        }
    }

    private void OnMyPresets(List<MyPreset> myPresets)
    {
        MyPresets = myPresets;
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

    public void CustomOptionsReferential()
    {
        var referential = new List<ApiCustomOption>();
        foreach (var option in CustomOption.Tab.Options.Where(o => o.IsHeader))
        {
            RenderApiCustomOption(referential, option);
        }

        Emit(Events.CustomOptionsReferential, referential);
    }

    private void RenderApiCustomOption(List<ApiCustomOption> referential, CustomOption option)
    {
        var opt = new ApiCustomOption
        {
            Key = option.Key,
            Name = option.Name,
            Color = option.Color,
            DefaultValue = option.SelectionIndex,
            Children = new List<ApiCustomOption>(),
        };
        referential.Add(opt);
        if (!option.HasChildren) return;
        foreach (var subOption in option.Children)
        {
            RenderApiCustomOption(opt.Children, subOption);
        }
    }

    public void UpdateOptions(Dictionary<string, int> allOptions)
    {
        var data = allOptions.Select(opt => new CustomOptionValue { Key = opt.Key, Value = opt.Value }).ToList();
        Emit(Events.UpdateOptions, data);
    }

    public void UpdateOption(string key, int value)
    {
        var data = new List<CustomOptionValue>
        {
            new()
            {
                Key = key,
                Value = value
            }
        };
        Emit(Events.UpdateOptions, data);
    }

    public void ChangeCurrentPreset(int index)
    {
        if (index >= MyPresets.Count) return;
        var myPreset = MyPresets[index];
        Emit(Events.ChangeCurrentPreset, myPreset.Id);
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
        Error,
        SubscribeToAccounts,
        PublicAccountInfos,
        TaskDone,
        MurderPlayer,
        WinGame,
        MyAccount,
        ChangeCurrentPreset,
        UpdatePreset,
        LoadPreset,
        CreatePreset,
        UpdateOptions,
        CustomOptionsReferential,
        MyPresets
    }
}