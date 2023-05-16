using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TheOtherRoles.EnoFw.Kernel;
using TheOtherRoles.Players;

namespace TheOtherRoles.EnoFw.Modules;

public static class ExternalResources
{
    private const string BaseEndpoint = "https://eno.re/BetterOtherRoles/api";

    private static HttpClient HttpClient
    {
        get
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "BetterOtherRoles Plugin");
            client.DefaultRequestHeaders.Add("X-Plugin-Version", TheOtherRolesPlugin.VersionString);
            if (AmongUsClient.Instance != null && CachedPlayer.LocalPlayer != null)
            {
                client.DefaultRequestHeaders.Add("X-Friend-Code", AmongUsClient.Instance.GetClient(CachedPlayer.LocalPlayer.PlayerControl.OwnerId).FriendCode);
                client.DefaultRequestHeaders.Add("X-Product-User-Id", AmongUsClient.Instance.GetClient(CachedPlayer.LocalPlayer.PlayerControl.OwnerId).ProductUserId);
            }
            return client;
        }
    }

    public static async Task<TReturn> Get<TReturn>(string uri)
    {
        var response = await HttpClient.GetAsync($"{BaseEndpoint}/{uri}", HttpCompletionOption.ResponseContentRead);
        if (!response.IsSuccessStatusCode)
        {
            throw new EnoFwException($"[GET]{uri} request reply with non-success status code");
        }

        var content = await response.Content.ReadAsStringAsync();
        return Rpc.Deserialize<TReturn>(content);
    }

    public static async Task<TReturn> Post<TReturn, TBody>(string uri, TBody body)
    {
        var response = await HttpClient.PostAsync($"{BaseEndpoint}/{uri}",
            new StringContent(Rpc.Serialize(body), Encoding.UTF8, "application/json"));
        if (!response.IsSuccessStatusCode)
        {
            throw new EnoFwException($"[POST]{uri} request reply with non-success status code");
        }
        var content = await response.Content.ReadAsStringAsync();
        return Rpc.Deserialize<TReturn>(content);
    }

    public static async Task<bool> Post<TBody>(string uri, TBody body)
    {
        var response = await HttpClient.PostAsync($"{BaseEndpoint}/{uri}",
            new StringContent(Rpc.Serialize(body), Encoding.UTF8, "application/json"));
        return response.IsSuccessStatusCode;
    }
}