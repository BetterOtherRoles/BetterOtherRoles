using System.Linq;
using Assets.InnerNet;

namespace BetterOtherRoles.Modules;

public static class ModCredits
{
    public static readonly string[] TORGithubContributors =
    {
        "Alex2911",
        "amsyarasyiq",
        "MaximeGillot",
        "Psynomit",
        "probablyadnf",
        "JustASysAdmin",
    };
    
    public static readonly string[] BORGithubContributors =
    {
        "Thiladon",
        "EnoPM",
    };

    public static readonly string[] TORDiscordModerators = { "Streamblox", "Draco Cordraconis" };

    public static readonly string[] SpecialThanks =
    {
        "Thanks to all BetterOtherRoles Discord helpers!",
        "Thanks to miniduikboot & GD for hosting modded servers",
        "Special thanks to K3ndo & Smeggy",
    };

    public static readonly string[] TOROtherCredits =
    {
        "OxygenFilter - For the versions v2.3.0 to v2.6.1, we were using the OxygenFilter for automatic deobfuscation.",
        "Reactor - The framework used for all versions before v2.0.0, and again since 4.2.0.",
        "BepInEx - Used to hook game functions.",
        "Essentials - Custom game options by DorCoMaNdO:",
        " - Before v1.6: We used the default Essentials release.",
        " - v1.6-v1.8: We slightly changed the default Essentials.",
        " - v2.0.0 and later: As we were not using Reactor anymore, we are using our own implementation, inspired by the one from DorCoMaNdO.",
        "Jackal and Sidekick - Original idea for the Jackal and Sidekick came from Dhalucard.",
        "Among-Us-Love-Couple-Mod - Idea for the Lovers modifier comes from Woodi-dev.",
        "Jester - Idea for the Jester role came from Maartii.",
        "ExtraRolesAmongUs - Idea for the Engineer and Medic role came from NotHunter101. Also some code snippets from their implementation were used.",
        "Among-Us-Sheriff-Mod - Idea for the Sheriff role came from Woodi-dev.",
        "TooManyRolesMods - Idea for the Detective and Time Master roles comes from Hardel-DW. Also some code snippets from their implementation were used.",
        "TownOfUs - Idea for the Swapper, Shifter, Arsonist and a similar Mayor role came from Slushiegoose.",
        "Ottomated - Idea for the Morphling, Snitch and Camouflager role came from Ottomated.",
        "Slushiegoose - Idea for the Vulture role came from Slushiegoose.",
    };

    public static readonly string[] BOROtherCredits =
    {
        "Brybry - Better Polus map modifications.",
        "Dadoum - Better Skeld map modifications & keybindings fix.",
        "Thilladon - Better Skeld map modifications, keybindings fix, updating snitch informations methods.",
        "Eno - Fixing vents animations, pool order of players in meeting, code refactoring.",
    };

    public static Announcement GetBetterOtherRolesCredits()
    {
        var creditsAnnouncement = new Announcement
        {
            Id = "borCredits",
            Language = 0,
            Number = 500,
            Title = "Better Other Roles Credits & Resources",
            ShortTitle = "BOR Credits",
            SubTitle = "",
            PinState = false,
            Date = "09.04.2023"
        };
        var torGithubContributors = TORGithubContributors.Select(username => $"[https://github.com/{username}]{username}[]");
        var borGithubContributors = BORGithubContributors.Select(username => $"[https://github.com/{username}]{username}[]");
        var creditsString = @"<align=""center"">";
        creditsString += $"BetterOtherRoles Github Contributors:\n{string.Join(", ", torGithubContributors)}\n\n";
        creditsString += $"\nBetterOtherRoles Github Contributors:\n{string.Join(", ", borGithubContributors)}\n\n";
        creditsString += $"\nTheOtherRoles Discord Moderators:\n{string.Join(", ", TORDiscordModerators)}\n\n";
        creditsString += $"\n{string.Join("\n", SpecialThanks)}\n\n";
        creditsString += "</align>";
        creditsString += "<align=\"center\">BetterOtherRoles Credits & Resources:</align>\n";
        creditsString += "<size=70%>Modded by Eisbison, EndOfFile, Thunderstorm584, Mallöris & Gendelo.</size>\n";
        creditsString += "<size=70%>Design by Bavari.</size>\n";
        creditsString += $"<size=60%>{string.Join("\n", TOROtherCredits)}\n\n</size>";
        creditsString += "<align=\"center\">BetterOtherRoles Credits & Resources:</align>\n";
        creditsString += $"<size=60%>{string.Join("\n", BOROtherCredits)}</size>";
        creditsString += "";
        creditsAnnouncement.Text = creditsString;

        return creditsAnnouncement;
    }
}