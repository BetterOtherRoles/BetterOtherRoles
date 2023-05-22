using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using AmongUs.Data;
using Assets.InnerNet;
using BepInEx;
using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Attributes;
using Newtonsoft.Json.Linq;
using TMPro;
using Twitch;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Version = SemanticVersioning.Version;

namespace BetterOtherRoles.Modules;

public class ModUpdateBehaviour : MonoBehaviour
{
    private static bool showPopUp = true;
    private static bool updateInProgress;

    private static ModUpdateBehaviour Instance { get; set; }
    public ModUpdateBehaviour(IntPtr ptr) : base(ptr) { }

    private class UpdateData
    {
        public readonly string Content;
        public readonly string Tag;
        private readonly DateTime _publicationDate;
        public string TimeString => _publicationDate.ToString(CultureInfo.InvariantCulture);
        public readonly JObject Request;
        public UpdateData(JObject data)
        {
            Tag = data["tag_name"]?.ToString().TrimStart('v');
            Content = data["body"]?.ToString();
            _publicationDate = DateTime.FromBinary(((Il2CppSystem.DateTime)data["published_at"]).ToBinaryRaw());
            Request = data;
        }

        public bool IsNewer(Version version)
        {
            if (!Version.TryParse(Tag, out var myVersion)) return false;
            return myVersion.BaseVersion() > version.BaseVersion();
        }
    }

    [HideFromIl2Cpp]
    private UpdateData RequiredUpdateData { get; set; }

    public void Awake()
    {
        if (Instance) Destroy(this);
        Instance = this;
            
        SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>) OnSceneLoaded);
        this.StartCoroutine(CoCheckUpdates());
        foreach (var file in Directory.GetFiles(Paths.PluginPath, "*.old"))
        {
            File.Delete(file);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (updateInProgress || scene.name != "MainMenu") return;
        if (RequiredUpdateData is null) {
            showPopUp = false;
            return;
        }

        var template = GameObject.Find("ExitGameButton");
        if (!template) return;
            
        var button = Instantiate(template, null);
        var buttonTransform = button.transform;
        var pos = buttonTransform.localPosition;
        pos.y += 1.2f;
        buttonTransform.localPosition = pos;

        var passiveButton = button.GetComponent<PassiveButton>();
        var buttonSprite = button.GetComponent<SpriteRenderer>();
        passiveButton.OnClick = new Button.ButtonClickedEvent();
        passiveButton.OnClick.AddListener((Action) (() =>
        {
            this.StartCoroutine(CoUpdate());
            button.SetActive(false);
        }));

        var text = button.transform.GetChild(0).GetComponent<TMP_Text>();

        StartCoroutine(Effects.Lerp(0.1f, (Action<float>)(_ => text.SetText("Update"))));

        buttonSprite.color = text.color = Color.red;
        passiveButton.OnMouseOut.AddListener((Action)(() => buttonSprite.color = text.color = Color.red));
            
        var announcement = $"<size=150%>A new BetterOtherRoles update to {RequiredUpdateData.Tag} is available</size>\n{RequiredUpdateData.Content}";
        var mgr = FindObjectOfType<MainMenuManager>(true);

        try {
            var updateVersion = RequiredUpdateData.Content[^5..];
            if (Version.Parse(BetterOtherRolesPlugin.VersionString).BaseVersion() < Version.Parse(updateVersion).BaseVersion()) {
                passiveButton.OnClick.RemoveAllListeners();
                passiveButton.OnClick = new Button.ButtonClickedEvent();
                passiveButton.OnClick.AddListener((Action)(() => {
                    mgr.StartCoroutine(CoShowAnnouncement("<size=150%>A MANUAL UPDATE IS REQUIRED</size>"));
                }));
            }
        } catch {  
            BetterOtherRolesPlugin.Logger.LogError("parsing version for auto updater failed :(");
        }
        if (showPopUp) mgr.StartCoroutine(CoShowAnnouncement(announcement, shortTitle: "BOR Update", date: RequiredUpdateData.TimeString));
        showPopUp = false;
    }
        
    [HideFromIl2Cpp]
    private IEnumerator CoUpdate()
    {
        updateInProgress = true;
        var updateName = "BetterOtherRoles";
            
        var popup = Instantiate(TwitchManager.Instance.TwitchPopup);
        popup.TextAreaTMP.fontSize *= 0.7f;
        popup.TextAreaTMP.enableAutoSizing = false;
            
        popup.Show();

        var button = popup.transform.GetChild(2).gameObject;
        button.SetActive(false);

        popup.TextAreaTMP.text = $"Updating {updateName}\nPlease wait...";
            
        var download = Task.Run(DownloadUpdate);
        while (!download.IsCompleted) yield return null;
            
        if (download.Result)
        {
            var passiveButton = button.GetComponent<PassiveButton>();
            passiveButton.OnClick = new Button.ButtonClickedEvent();
            passiveButton.OnClick.AddListener((Action) Application.Quit);
        }

        button.SetActive(true);
        popup.TextAreaTMP.text = download.Result ? $"{updateName}\nupdated successfully\nRestart the game." : "Update wasn't successful\nTry again later,\nor update manually.";
            
    }

    private static int announcementNumber = 501;
    [HideFromIl2Cpp]
    private static IEnumerator CoShowAnnouncement(string announcement, bool show=true, string shortTitle="TOR Update", string date="")
    {
        var popUp = Instantiate(FindObjectOfType<AnnouncementPopUp>(true));
        popUp.gameObject.SetActive(show);
        yield return popUp.Init(show);
            
        var announcementS = new Announcement
        {
            Title = "BOR Announcement",
            Text = announcement, // Can add clickable urls like this: "[https://www.google.de/]Text[]"
            ShortTitle = shortTitle,
            Id = $"borUpdateAnnouncement_{announcementNumber}",
            Language = 0,
            Number = announcementNumber++,
            SubTitle = "",
            PinState = true,
            Date = date == "" ? DateTime.Today.ToString(CultureInfo.InvariantCulture) : date
        };

        DataManager.Player.Announcements.allAnnouncements.Insert(0, announcementS);
        popUp.CreateAnnouncementList();
    }

    [HideFromIl2Cpp]
    private static IEnumerator CoCheckUpdates()
    {
        var torUpdateCheck = Task.Run(() => Instance.GetGithubUpdate("BetterOtherRoles", "BetterOtherRoles"));
        while (!torUpdateCheck.IsCompleted) yield return null;
        if (torUpdateCheck.Result != null && torUpdateCheck.Result.IsNewer(Version.Parse(BetterOtherRolesPlugin.VersionString)))
        {
            Instance.RequiredUpdateData = torUpdateCheck.Result;
        }

        Instance.OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    [HideFromIl2Cpp]
    private async Task<UpdateData> GetGithubUpdate(string owner, string repo)
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "BetterOtherRoles Updater");

        try {
            var req = await client.GetAsync($"https://api.github.com/repos/{owner}/{repo}/releases/latest", HttpCompletionOption.ResponseContentRead);

            if (!req.IsSuccessStatusCode) return null;

            var dataString = await req.Content.ReadAsStringAsync();
            var data = JObject.Parse(dataString);
            return new UpdateData(data);
        } catch (HttpRequestException) {
            return null;
        }
    }


    [HideFromIl2Cpp]
    private async Task<bool> DownloadUpdate()
    {
        var data = RequiredUpdateData;
            
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "BetterOtherRoles Updater");
            
        var assets = data.Request["assets"];
        var downloadUri = "";
        for (var current = assets.First; current != null; current = current.Next) 
        {
            var browserDownloadURL = current["browser_download_url"]?.ToString();
            if (browserDownloadURL == null || current["content_type"] == null) continue;
            if (!current["content_type"].ToString().Equals("application/x-msdownload") || !browserDownloadURL.EndsWith(".dll")) continue;
            downloadUri = browserDownloadURL;
            break;
        }

        if (downloadUri.Length == 0) return false;

        var res = await client.GetAsync(downloadUri, HttpCompletionOption.ResponseContentRead);
        
        var filePath = Path.Combine(Paths.PluginPath, "BetterOtherRoles.dll");
            
        if (File.Exists($"{filePath}.old")) File.Delete($"{filePath}.old");
        if (File.Exists(filePath)) File.Move(filePath, $"{filePath}.old");

        await using var responseStream = await res.Content.ReadAsStreamAsync();
        await using var fileStream = File.Create(filePath);
        await responseStream.CopyToAsync(fileStream);

        return true;
    }
}