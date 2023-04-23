using TheOtherRoles.Customs.Roles;
using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.EnoFramework.Utils;

namespace TheOtherRoles.EnoFramework;

[EnoSingleton(100)]
public class CustomOptionsHolder
{
    public readonly CustomOption.Tab MainSettings;
    public readonly CustomOption.Tab ImpostorsSettings;
    public readonly CustomOption.Tab NeutralSettings;
    public readonly CustomOption.Tab CrewmateSettings;
    public readonly CustomOption.Tab ModifierSettings;

    public readonly CustomOption Preset;
    public readonly CustomOption EnableRoles;

    public readonly CustomOption MinCrewmateRoles;
    public readonly CustomOption MaxCrewmateRoles;
    public readonly CustomOption MinNeutralRoles;
    public readonly CustomOption MaxNeutralRoles;
    public readonly CustomOption MinImpostorRoles;
    public readonly CustomOption MaxImpostorRoles;
    public readonly CustomOption MinModifiers;
    public readonly CustomOption MaxModifiers;

    public readonly CustomOption EnableBetterPolus;
    public readonly CustomOption BetterPolusReactorDuration;

    public readonly CustomOption EnableBetterSkeld;

    public readonly CustomOption MaxEmergencyMeetings;
    public readonly CustomOption BlockSkippingInEmergencyMeetings;
    public readonly CustomOption NoVoteIsSelfVote;
    
    public readonly CustomOption HidePlayerNames;
    public readonly CustomOption AllowParallelMedBayScans;
    public readonly CustomOption FinishTasksBeforeHauntingOrZoomingOut;
    
    public readonly CustomOption CamerasNightVision;
    public readonly CustomOption CamerasNightVisionIfImpostor;
    
    public readonly CustomOption ShieldFirstKilledPlayer;
    
    public readonly CustomOption RandomizeMeetingOrder;

    public CustomOptionsHolder()
    {
        // TABS
        MainSettings = new CustomOption.Tab(nameof(MainSettings), "The Other Roles Settings",
            "TheOtherRoles.Resources.TabIcon.png");
        ImpostorsSettings = new CustomOption.Tab(nameof(ImpostorsSettings), "Impostor Roles Settings",
            "TheOtherRoles.Resources.TabIconImpostor.png");
        NeutralSettings = new CustomOption.Tab(nameof(NeutralSettings), "Neutral Roles Settings",
            "TheOtherRoles.Resources.TabIconNeutral.png");
        CrewmateSettings = new CustomOption.Tab(nameof(CrewmateSettings), "Crewmate Roles Settings",
            "TheOtherRoles.Resources.TabIconCrewmate.png");
        ModifierSettings = new CustomOption.Tab(nameof(ModifierSettings), "Modifier Settings",
            "TheOtherRoles.Resources.TabIconModifier.png");

        // OPTIONS
        Preset = MainSettings.CreateFloatList(
            nameof(Preset),
            Colors.Cs("#504885", "Current preset"),
            1f,
            5f,
            1f,
            1f,
            null,
            "Preset ");
        EnableRoles = MainSettings.CreateBool(
            nameof(EnableRoles),
            Colors.Cs("#ff1010", "Enable roles"),
            true);
        
        MinCrewmateRoles = MainSettings.CreateFloatList(
            nameof(MinCrewmateRoles),
            Colors.Cs(Colors.Crewmate, "Minimum Crewmate Roles"),
            0f,
            15f,
            15f,
            1f);
        MaxCrewmateRoles = MainSettings.CreateFloatList(
            nameof(MaxCrewmateRoles),
            Colors.Cs(Colors.Crewmate, "Maximum Crewmate Roles"),
            0f,
            15f,
            15f,
            1f);
        MinNeutralRoles = MainSettings.CreateFloatList(
            nameof(MinNeutralRoles),
            Colors.Cs(Colors.Neutral, "Minimum Neutral Roles"),
            0f,
            15f,
            15f,
            1f);
        MaxNeutralRoles = MainSettings.CreateFloatList(
            nameof(MaxNeutralRoles),
            Colors.Cs(Colors.Neutral, "Maximum Neutral Roles"),
            0f,
            15f,
            15f,
            1f);
        MinImpostorRoles = MainSettings.CreateFloatList(
            nameof(MinImpostorRoles),
            Colors.Cs(Colors.Impostor, "Minimum Impostor Roles"),
            0f,
            15f,
            15f,
            1f);
        MaxImpostorRoles = MainSettings.CreateFloatList(
            nameof(MaxImpostorRoles),
            Colors.Cs(Colors.Impostor, "Maximum Impostor Roles"),
            0f,
            15f,
            15f,
            1f);
        MinModifiers = MainSettings.CreateFloatList(
            nameof(MinModifiers),
            Colors.Cs(Colors.Modifier, "Minimum Modifiers"),
            0f,
            15f,
            15f,
            1f);
        MaxModifiers = MainSettings.CreateFloatList(
            nameof(MaxModifiers),
            Colors.Cs(Colors.Modifier, "Maximum Modifiers"),
            0f,
            15f,
            15f,
            1f);

        EnableBetterPolus = MainSettings.CreateBool(
            nameof(EnableBetterPolus),
            Colors.Cs("#04701e", "Enable Better Polus"),
            false);
        BetterPolusReactorDuration = MainSettings.CreateFloatList(
            nameof(BetterPolusReactorDuration),
            Colors.Cs("#04701e", "Reactor sabotage duration"),
            15f,
            60f,
            40f,
            2.5f,
            EnableBetterPolus,
            string.Empty,
            "s");
        
        EnableBetterSkeld = MainSettings.CreateBool(
            nameof(EnableBetterSkeld),
            "Enable Better Skeld",
            false);

        MaxEmergencyMeetings = MainSettings.CreateFloatList(
            nameof(MaxEmergencyMeetings),
            "Number of emergency meetings",
            0f,
            15f,
            10f,
            1f);
        BlockSkippingInEmergencyMeetings = MainSettings.CreateBool(
            nameof(BlockSkippingInEmergencyMeetings),
            "Disable skip button in emergency meetings",
            false,
            MaxEmergencyMeetings);
        NoVoteIsSelfVote = MainSettings.CreateBool(
            nameof(NoVoteIsSelfVote),
            "No vote is self vote",
            false,
            MaxEmergencyMeetings);

        HidePlayerNames = MainSettings.CreateBool(
            nameof(HidePlayerNames),
            "Hide player names",
            false);
        AllowParallelMedBayScans = MainSettings.CreateBool(
            nameof(AllowParallelMedBayScans),
            "Allow parallel MedBay scans",
            false);
        FinishTasksBeforeHauntingOrZoomingOut = MainSettings.CreateBool(
            nameof(FinishTasksBeforeHauntingOrZoomingOut),
            "Finish tasks before haunting or zooming out",
            false);
        
        CamerasNightVision = MainSettings.CreateBool(
            nameof(CamerasNightVision),
            "Cams switch to night vision if lights are off",
            false);
        CamerasNightVisionIfImpostor = MainSettings.CreateBool(
            nameof(CamerasNightVisionIfImpostor),
            "Impostor ignore night vision cams",
            false,
            CamerasNightVision);
        
        ShieldFirstKilledPlayer = MainSettings.CreateBool(
            nameof(ShieldFirstKilledPlayer),
            "Shield first killed player",
            false);
        
        RandomizeMeetingOrder = MainSettings.CreateBool(
            nameof(RandomizeMeetingOrder),
            "Randomize players in meeting",
            false);
    }
}