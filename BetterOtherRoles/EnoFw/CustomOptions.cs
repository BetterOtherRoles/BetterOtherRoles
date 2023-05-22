using System.Collections.Generic;
using BetterOtherRoles.EnoFw.Utils;
using UnityEngine;

namespace BetterOtherRoles.EnoFw;

using CustomOption = Kernel.CustomOption;
using Tab = Kernel.CustomOption.Tab;

public static class CustomOptions
{
    public static readonly Tab MainSettings;
    public static readonly Tab ImpostorsSettings;
    public static readonly Tab NeutralSettings;
    public static readonly Tab CrewmateSettings;
    public static readonly Tab ModifierSettings;

    public static readonly CustomOption Preset;
    public static readonly CustomOption EnableRoles;

    public static readonly CustomOption MinCrewmateRoles;
    public static readonly CustomOption MaxCrewmateRoles;
    public static readonly CustomOption FillCrewmateRoles;
    public static readonly CustomOption MinNeutralRoles;
    public static readonly CustomOption MaxNeutralRoles;
    public static readonly CustomOption MinImpostorRoles;
    public static readonly CustomOption MaxImpostorRoles;
    public static readonly CustomOption MinModifiers;
    public static readonly CustomOption MaxModifiers;

    public static readonly CustomOption RandomizeWirePositions;
    public static readonly CustomOption RandomizeUploadPositions;

    public static readonly CustomOption EnableBetterPolus;
    public static readonly CustomOption BetterPolusReactorDuration;

    public static readonly CustomOption EnableBetterSkeld;

    public static readonly CustomOption MaxEmergencyMeetings;
    public static readonly CustomOption BlockSkippingInEmergencyMeetings;
    public static readonly CustomOption NoVoteIsSelfVote;

    public static readonly CustomOption HidePlayerNames;

    public static readonly CustomOption AllowParallelMedBayScans;
    public static readonly CustomOption RandomizePositionDuringScan;

    public static readonly CustomOption FinishTasksBeforeHauntingOrZoomingOut;

    public static readonly CustomOption CamerasNightVision;
    public static readonly CustomOption CamerasNightVisionIfImpostor;

    public static readonly CustomOption ShieldFirstKilledPlayer;
    public static readonly CustomOption RemoveFirstKillShield;
    public static readonly CustomOption RemoveShieldTimer;
    public static readonly CustomOption ShowShieldIndicator;

    public static readonly CustomOption RandomizeMeetingOrder;

    public static readonly CustomOption DynamicMap;
    public static readonly CustomOption DynamicMapEnableSkeld;
    public static readonly CustomOption DynamicMapEnableDleks;
    public static readonly CustomOption DynamicMapEnableMira;
    public static readonly CustomOption DynamicMapEnablePolus;
    public static readonly CustomOption DynamicMapEnableAirShip;
    public static readonly CustomOption DynamicMapEnableSubmerged;
    public static readonly CustomOption DynamicMapSeparateSettings;

    // Guesser Game mode
    public static readonly CustomOption GuesserGameModeCrewNumber;
    public static readonly CustomOption GuesserGameModeNeutralNumber;
    public static readonly CustomOption GuesserGameModeImpostorNumber;
    public static readonly CustomOption GuesserForceJackalGuesser;
    public static readonly CustomOption GuesserGameModeHaveModifier;
    public static readonly CustomOption GuesserGameModeNumberOfShots;
    public static readonly CustomOption GuesserGameModeHasMultipleShotsPerMeeting;
    public static readonly CustomOption GuesserGameModeKillsThroughShield;
    public static readonly CustomOption GuesserGameModeEvilCanKillSpy;
    public static readonly CustomOption GuesserGameModeCantGuessSnitchIfTasksDone;

    // Hide N Seek Game mode
    public static readonly CustomOption HideNSeekHunterCount;
    public static readonly CustomOption HideNSeekKillCooldown;
    public static readonly CustomOption HideNSeekHunterVision;
    public static readonly CustomOption HideNSeekHuntedVision;
    public static readonly CustomOption HideNSeekTimer;
    public static readonly CustomOption HideNSeekCommonTasks;
    public static readonly CustomOption HideNSeekShortTasks;
    public static readonly CustomOption HideNSeekLongTasks;
    public static readonly CustomOption HideNSeekTaskWin;
    public static readonly CustomOption HideNSeekTaskPunish;
    public static readonly CustomOption HideNSeekCanSabotage;
    public static readonly CustomOption HideNSeekMap;
    public static readonly CustomOption HideNSeekHunterWaiting;

    public static readonly CustomOption HunterLightCooldown;
    public static readonly CustomOption HunterLightDuration;
    public static readonly CustomOption HunterLightVision;
    public static readonly CustomOption HunterLightPunish;
    public static readonly CustomOption HunterAdminCooldown;
    public static readonly CustomOption HunterAdminDuration;
    public static readonly CustomOption HunterAdminPunish;
    public static readonly CustomOption HunterArrowCooldown;
    public static readonly CustomOption HunterArrowDuration;
    public static readonly CustomOption HunterArrowPunish;

    public static readonly CustomOption HuntedShieldCooldown;
    public static readonly CustomOption HuntedShieldDuration;
    public static readonly CustomOption HuntedShieldRewindTime;
    public static readonly CustomOption HuntedShieldNumber;

    public static readonly CustomOption MafiaSpawnRate;
    
    public static readonly CustomOption HideModifiers;

    static CustomOptions()
    {
        MainSettings = new Tab(nameof(MainSettings), "Better Other Roles Settings",
            "BetterOtherRoles.Resources.TabIcon.png");
        ImpostorsSettings = new Tab(nameof(ImpostorsSettings), "Impostor Roles Settings",
            "BetterOtherRoles.Resources.TabIconImpostor.png");
        NeutralSettings = new Tab(nameof(NeutralSettings), "Neutral Roles Settings",
            "BetterOtherRoles.Resources.TabIconNeutral.png");
        CrewmateSettings = new Tab(nameof(CrewmateSettings), "Crewmate Roles Settings",
            "BetterOtherRoles.Resources.TabIconCrewmate.png");
        ModifierSettings = new Tab(nameof(ModifierSettings), "Modifier Settings",
            "BetterOtherRoles.Resources.TabIconModifier.png");

        Preset = MainSettings.CreateStringList(
            nameof(Preset),
            Cs("#504885", "Current preset"),
            new List<string> { "Online", "Preset 1", "Preset 2", "Preset 3", "Preset 4", "Preset 5", "Skeld", "Dleks", "Mira HQ", "Polus", "Airship", "Submerged" },
            "Online");
        EnableRoles = MainSettings.CreateBool(
            nameof(EnableRoles),
            Cs("#ff1010", "Enable roles"),
            true);

        MinCrewmateRoles = MainSettings.CreateFloatList(
            nameof(MinCrewmateRoles),
            Cs(Colors.Crewmate, "Minimum Crewmate Roles"),
            0f,
            15f,
            15f,
            1f);
        MaxCrewmateRoles = MainSettings.CreateFloatList(
            nameof(MaxCrewmateRoles),
            Cs(Colors.Crewmate, "Maximum Crewmate Roles"),
            0f,
            15f,
            15f,
            1f);
        FillCrewmateRoles = MainSettings.CreateBool(
            nameof(FillCrewmateRoles),
            Cs(Colors.Crewmate, "Fill crewmate roles"),
            false);
        MinNeutralRoles = MainSettings.CreateFloatList(
            nameof(MinNeutralRoles),
            Cs(Colors.Neutral, "Minimum Neutral Roles"),
            0f,
            15f,
            15f,
            1f);
        MaxNeutralRoles = MainSettings.CreateFloatList(
            nameof(MaxNeutralRoles),
            Cs(Colors.Neutral, "Maximum Neutral Roles"),
            0f,
            15f,
            15f,
            1f);
        MinImpostorRoles = MainSettings.CreateFloatList(
            nameof(MinImpostorRoles),
            Cs(Colors.Impostor, "Minimum Impostor Roles"),
            0f,
            15f,
            15f,
            1f);
        MaxImpostorRoles = MainSettings.CreateFloatList(
            nameof(MaxImpostorRoles),
            Cs(Colors.Impostor, "Maximum Impostor Roles"),
            0f,
            15f,
            15f,
            1f);
        MinModifiers = MainSettings.CreateFloatList(
            nameof(MinModifiers),
            Cs(Colors.Modifier, "Minimum Modifiers"),
            0f,
            15f,
            15f,
            1f);
        MaxModifiers = MainSettings.CreateFloatList(
            nameof(MaxModifiers),
            Cs(Colors.Modifier, "Maximum Modifiers"),
            0f,
            15f,
            15f,
            1f);

        RandomizeWirePositions = MainSettings.CreateBool(
            nameof(RandomizeWirePositions),
            Cs("Randomize wire tasks positions"),
            false);

        RandomizeUploadPositions = MainSettings.CreateBool(
            nameof(RandomizeUploadPositions),
            Cs("Randomize upload task position"),
            false);

        EnableBetterPolus = MainSettings.CreateBool(
            nameof(EnableBetterPolus),
            Cs("#04701e", "Enable Better Polus"),
            false).OnlyForMaps(CustomOption.Maps.Polus);
        BetterPolusReactorDuration = MainSettings.CreateFloatList(
            nameof(BetterPolusReactorDuration),
            Cs("#04701e", "Reactor sabotage duration"),
            15f,
            100f,
            40f,
            2.5f,
            EnableBetterPolus,
            string.Empty,
            "s").OnlyForMaps(CustomOption.Maps.Polus);

        EnableBetterSkeld = MainSettings.CreateBool(
            nameof(EnableBetterSkeld),
            Cs("Enable Better Skeld"),
            false).OnlyForMaps(CustomOption.Maps.Skeld);

        MaxEmergencyMeetings = MainSettings.CreateFloatList(
            nameof(MaxEmergencyMeetings),
            Cs("Number of emergency meetings"),
            0f,
            15f,
            10f,
            1f);
        BlockSkippingInEmergencyMeetings = MainSettings.CreateBool(
            nameof(BlockSkippingInEmergencyMeetings),
            Cs("Disable skip button in emergency meetings"),
            false,
            MaxEmergencyMeetings);
        NoVoteIsSelfVote = MainSettings.CreateBool(
            nameof(NoVoteIsSelfVote),
            Cs("No vote is self vote"),
            false,
            MaxEmergencyMeetings);

        HidePlayerNames = MainSettings.CreateBool(
            nameof(HidePlayerNames),
            Cs("Hide player names"),
            false);

        AllowParallelMedBayScans = MainSettings.CreateBool(
            nameof(AllowParallelMedBayScans),
            Cs("Allow parallel MedBay scans"),
            false);
        RandomizePositionDuringScan = MainSettings.CreateBool(
            nameof(RandomizePositionDuringScan),
            Cs("Randomize position during medbay scan"),
            false);

        FinishTasksBeforeHauntingOrZoomingOut = MainSettings.CreateBool(
            nameof(FinishTasksBeforeHauntingOrZoomingOut),
            Cs("Finish tasks before haunting or zooming out"),
            false);

        CamerasNightVision = MainSettings.CreateBool(
            nameof(CamerasNightVision),
            Cs("Cams switch to night vision if lights are off"),
            false);
        CamerasNightVisionIfImpostor = MainSettings.CreateBool(
            nameof(CamerasNightVisionIfImpostor),
            Cs("Impostor ignore night vision cams"),
            false,
            CamerasNightVision);

        ShieldFirstKilledPlayer = MainSettings.CreateBool(
            nameof(ShieldFirstKilledPlayer),
            Cs("Shield first killed player"),
            false);
        RemoveFirstKillShield = MainSettings.CreateStringList(
            nameof(RemoveFirstKillShield),
            Cs("Remove shield when"),
            new List<string> { "First meeting ended", "Timer expired" },
            "First meeting ended",
            ShieldFirstKilledPlayer);
        RemoveShieldTimer = MainSettings.CreateFloatList(
            nameof(RemoveShieldTimer),
            Cs("Shield duration"),
            10f,
            600f,
            60f,
            10f,
            RemoveFirstKillShield,
            string.Empty,
            "s");
        ShowShieldIndicator = MainSettings.CreateBool(
            nameof(ShowShieldIndicator),
            Cs("Show shield indicator"),
            false,
            RemoveFirstKillShield);

        RandomizeMeetingOrder = MainSettings.CreateBool(
            nameof(RandomizeMeetingOrder),
            Cs("Randomize players in meeting"),
            false);

        DynamicMap = MainSettings.CreateBool(
            nameof(DynamicMap),
            Cs("Play on a random map"),
            false);
        DynamicMapEnableSkeld = MainSettings.CreateFloatList(
            nameof(DynamicMapEnableSkeld),
            Cs("Skeld"),
            0f,
            100f,
            0f,
            1f,
            DynamicMap,
            string.Empty,
            "%");
        DynamicMapEnableDleks = MainSettings.CreateFloatList(
            nameof(DynamicMapEnableDleks),
            Cs("Dleks"),
            0f,
            100f,
            0f,
            1f,
            DynamicMap,
            string.Empty,
            "%");
        DynamicMapEnableMira = MainSettings.CreateFloatList(
            nameof(DynamicMapEnableMira),
            Cs("Mira HQ"),
            0f,
            100f,
            0f,
            1f,
            DynamicMap,
            string.Empty,
            "%");
        DynamicMapEnablePolus = MainSettings.CreateFloatList(
            nameof(DynamicMapEnablePolus),
            Cs("Polus"),
            0f,
            100f,
            0f,
            1f,
            DynamicMap,
            string.Empty,
            "%");
        DynamicMapEnableAirShip = MainSettings.CreateFloatList(
            nameof(DynamicMapEnableAirShip),
            Cs("Airship"),
            0f,
            100f,
            0f,
            1f,
            DynamicMap,
            string.Empty,
            "%");
        DynamicMapEnableSubmerged = MainSettings.CreateFloatList(
            nameof(DynamicMapEnableSubmerged),
            Cs("Submerged"),
            0f,
            100f,
            0f,
            1f,
            DynamicMap,
            string.Empty,
            "%");
        DynamicMapSeparateSettings = MainSettings.CreateBool(
            nameof(DynamicMapSeparateSettings),
            Cs("Use random map setting presets"),
            false,
            DynamicMap);

        GuesserGameModeCrewNumber = MainSettings.CreateFloatList(
                nameof(GuesserGameModeCrewNumber),
                Cs("Number of crewmate guessers"),
                1f,
                15f,
                15f,
                1f)
            .OnlyForGameModes(CustomOption.GameMode.Guesser);
        GuesserGameModeNeutralNumber = MainSettings.CreateFloatList(
                nameof(GuesserGameModeNeutralNumber),
                Cs("Number of neutral guessers"),
                1f,
                15f,
                15f,
                1f)
            .OnlyForGameModes(CustomOption.GameMode.Guesser);
        GuesserGameModeImpostorNumber = MainSettings.CreateFloatList(
                nameof(GuesserGameModeImpostorNumber),
                Cs("Number of impostor guessers"),
                1f,
                15f,
                15f,
                1f)
            .OnlyForGameModes(CustomOption.GameMode.Guesser);
        GuesserForceJackalGuesser = MainSettings.CreateBool(
                nameof(GuesserForceJackalGuesser),
                Cs("Force Jackal to be guesser"),
                false)
            .OnlyForGameModes(CustomOption.GameMode.Guesser);
        GuesserGameModeHaveModifier = MainSettings.CreateBool(
                nameof(GuesserGameModeHaveModifier),
                Cs("Guessers can have a modifier"),
                true)
            .OnlyForGameModes(CustomOption.GameMode.Guesser);
        GuesserGameModeNumberOfShots = MainSettings.CreateFloatList(
                nameof(GuesserGameModeNumberOfShots),
                Cs("Guesser number of shots"),
                1f,
                15f,
                3f,
                1f)
            .OnlyForGameModes(CustomOption.GameMode.Guesser);
        GuesserGameModeHasMultipleShotsPerMeeting = MainSettings.CreateBool(
                nameof(GuesserGameModeHasMultipleShotsPerMeeting),
                Cs("Guesser can shoot multiple times per meeting"),
                false)
            .OnlyForGameModes(CustomOption.GameMode.Guesser);
        GuesserGameModeKillsThroughShield = MainSettings.CreateBool(
                nameof(GuesserGameModeKillsThroughShield),
                Cs("Guesser ignore the Medic shield"),
                true)
            .OnlyForGameModes(CustomOption.GameMode.Guesser);
        GuesserGameModeEvilCanKillSpy = MainSettings.CreateBool(
                nameof(GuesserGameModeEvilCanKillSpy),
                Cs("Impostor Guesser can guess the Spy"),
                true)
            .OnlyForGameModes(CustomOption.GameMode.Guesser);
        GuesserGameModeCantGuessSnitchIfTasksDone = MainSettings.CreateBool(
                nameof(GuesserGameModeCantGuessSnitchIfTasksDone),
                Cs("Guesser can't guess Snitch when tasks completed"),
                true)
            .OnlyForGameModes(CustomOption.GameMode.Guesser);

        HideNSeekMap = MainSettings.CreateStringList(
                nameof(HideNSeekMap),
                Cs("Map"),
                new List<string> { "The Skeld", "Mira HQ", "Polus", "Airship", "Submerged" })
            .OnlyForGameModes(CustomOption.GameMode.HideNSeek);
        HideNSeekHunterCount = MainSettings.CreateFloatList(
                nameof(HideNSeekHunterCount),
                Cs("Number of Hunters"),
                1f,
                3f,
                1f,
                1f)
            .OnlyForGameModes(CustomOption.GameMode.HideNSeek);
        HideNSeekKillCooldown = MainSettings.CreateFloatList(
                nameof(HideNSeekKillCooldown),
                Cs("Kill cooldown"),
                2.5f,
                60f,
                10f,
                2.5f)
            .OnlyForGameModes(CustomOption.GameMode.HideNSeek);
        HideNSeekHunterVision = MainSettings.CreateFloatList(
                nameof(HideNSeekHunterVision),
                Cs("Hunter vision"),
                0.25f,
                2f,
                0.5f,
                0.25f)
            .OnlyForGameModes(CustomOption.GameMode.HideNSeek);
        HideNSeekHuntedVision = MainSettings.CreateFloatList(
                nameof(HideNSeekHuntedVision),
                Cs("Hunted vision"),
                0.25f,
                2f,
                0.5f,
                0.25f)
            .OnlyForGameModes(CustomOption.GameMode.HideNSeek);
        HideNSeekCommonTasks = HideNSeekHuntedVision = MainSettings.CreateFloatList(
                nameof(HideNSeekCommonTasks),
                Cs("Common tasks"),
                0f,
                4f,
                1f,
                1f)
            .OnlyForGameModes(CustomOption.GameMode.HideNSeek);
        HideNSeekShortTasks = MainSettings.CreateFloatList(
                nameof(HideNSeekShortTasks),
                Cs("Short tasks"),
                1f,
                23f,
                1f,
                1f)
            .OnlyForGameModes(CustomOption.GameMode.HideNSeek);
        HideNSeekLongTasks = MainSettings.CreateFloatList(
                nameof(HideNSeekLongTasks),
                Cs("Long tasks"),
                0f,
                15f,
                3f,
                1f)
            .OnlyForGameModes(CustomOption.GameMode.HideNSeek);
        HideNSeekTimer = MainSettings.CreateFloatList(
                nameof(HideNSeekTimer),
                Cs("Timer"),
                1f,
                30f,
                5f,
                1f,
                null,
                string.Empty,
                "min")
            .OnlyForGameModes(CustomOption.GameMode.HideNSeek);
        HideNSeekTaskWin = MainSettings.CreateBool(
                nameof(HideNSeekTaskWin),
                Cs("Task win is possible"),
                false)
            .OnlyForGameModes(CustomOption.GameMode.HideNSeek);
        HideNSeekTaskPunish = MainSettings.CreateFloatList(
            nameof(HideNSeekTaskPunish),
            Cs("Finish task timer reduction"),
            0f,
            30f,
            10f,
            1f,
            null,
            string.Empty,
            "s")
            .OnlyForGameModes(CustomOption.GameMode.HideNSeek);
        HideNSeekCanSabotage = MainSettings.CreateBool(
                nameof(HideNSeekCanSabotage),
                Cs("Enable sabotages"),
                false)
            .OnlyForGameModes(CustomOption.GameMode.HideNSeek);
        HideNSeekHunterWaiting = MainSettings.CreateFloatList(
                nameof(HideNSeekHunterWaiting),
                Cs("Time the Hunter needs to wait"),
                2.5f,
                60f,
                15f,
                2.5f,
                null,
                string.Empty,
                "s")
            .OnlyForGameModes(CustomOption.GameMode.HideNSeek);
        
        HunterLightCooldown = MainSettings.CreateFloatList(
                nameof(HunterLightCooldown),
                Cs(Color.red, "Hunter light cooldown"),
                5f,
                60f,
                30f,
                1f,
                null,
                string.Empty,
                "s")
            .OnlyForGameModes(CustomOption.GameMode.HideNSeek);
        HunterLightDuration = MainSettings.CreateFloatList(
                nameof(HunterLightDuration),
                Cs(Color.red, "Hunter light duration"),
                1f,
                60f,
                5f,
                1f,
                null,
                string.Empty,
                "s")
            .OnlyForGameModes(CustomOption.GameMode.HideNSeek);
        HunterLightVision = MainSettings.CreateFloatList(
                nameof(HunterLightVision),
                Cs(Color.red, "Hunter light vision"),
                1f,
                5f,
                3f,
                0.25f,
                null,
                string.Empty,
                "s")
            .OnlyForGameModes(CustomOption.GameMode.HideNSeek);
        HunterLightPunish = MainSettings.CreateFloatList(
                nameof(HunterLightPunish),
                Cs(Color.red, "Hunter light timer reduction"),
                0f,
                30f,
                5f,
                1f,
                null,
                string.Empty,
                "s")
            .OnlyForGameModes(CustomOption.GameMode.HideNSeek);
        HunterAdminCooldown = MainSettings.CreateFloatList(
                nameof(HunterAdminCooldown),
                Cs(Color.red, "Hunter admin cooldown"),
                5f,
                60f,
                30f,
                1f,
                null,
                string.Empty,
                "s")
            .OnlyForGameModes(CustomOption.GameMode.HideNSeek);
        HunterAdminDuration = MainSettings.CreateFloatList(
                nameof(HunterAdminDuration),
                Cs(Color.red, "Hunter admin duration"),
                1f,
                60f,
                5f,
                1f,
                null,
                string.Empty,
                "s")
            .OnlyForGameModes(CustomOption.GameMode.HideNSeek);
        HunterAdminPunish = MainSettings.CreateFloatList(
                nameof(HunterAdminPunish),
                Cs(Color.red, "Hunter admin time reduction"),
                0f,
                30f,
                5f,
                1f,
                null,
                string.Empty,
                "s")
            .OnlyForGameModes(CustomOption.GameMode.HideNSeek);
        HunterArrowCooldown = MainSettings.CreateFloatList(
                nameof(HunterArrowCooldown),
                Cs(Color.red, "Hunter arrow cooldown"),
                5f,
                60f,
                30f,
                1f,
                null,
                string.Empty,
                "s")
            .OnlyForGameModes(CustomOption.GameMode.HideNSeek);
        HunterArrowDuration = MainSettings.CreateFloatList(
                nameof(HunterArrowDuration),
                Cs(Color.red, "Hunter arrow duration"),
                1f,
                60f,
                5f,
                1f,
                null,
                string.Empty,
                "s")
            .OnlyForGameModes(CustomOption.GameMode.HideNSeek);
        HunterArrowPunish = MainSettings.CreateFloatList(
                nameof(HunterArrowPunish),
                Cs(Color.red, "Hunter arrow time reduction"),
                0f,
                30f,
                5f,
                1f,
                null,
                string.Empty,
                "s")
            .OnlyForGameModes(CustomOption.GameMode.HideNSeek);
        
        HuntedShieldCooldown = MainSettings.CreateFloatList(
                nameof(HuntedShieldCooldown),
                Cs(Color.gray, "Hunted shield cooldown"),
                5f,
                60f,
                30f,
                1f,
                null,
                string.Empty,
                "s")
            .OnlyForGameModes(CustomOption.GameMode.HideNSeek);
        HuntedShieldDuration = MainSettings.CreateFloatList(
                nameof(HuntedShieldDuration),
                Cs(Color.gray, "Hunted shield duration"),
                1f,
                60f,
                5f,
                1f,
                null,
                string.Empty,
                "s")
            .OnlyForGameModes(CustomOption.GameMode.HideNSeek);
        HuntedShieldRewindTime = MainSettings.CreateFloatList(
                nameof(HuntedShieldRewindTime),
                Cs(Color.gray, "Hunted shield rewind time"),
                1f,
                10f,
                3f,
                1f,
                null,
                string.Empty,
                "s")
            .OnlyForGameModes(CustomOption.GameMode.HideNSeek);
        HuntedShieldNumber = MainSettings.CreateFloatList(
                nameof(HuntedShieldNumber),
                Cs(Color.gray, "Hunted shield number"),
                1f,
                15f,
                3f,
                1f,
                null,
                string.Empty,
                "s")
            .OnlyForGameModes(CustomOption.GameMode.HideNSeek);

        MafiaSpawnRate = ImpostorsSettings.CreateFloatList(
            nameof(MafiaSpawnRate),
            Cs(Palette.ImpostorRed, "Mafia"),
            0f,
            100f,
            0f,
            10f,
            null,
            string.Empty,
            "%");

        HideModifiers = ModifierSettings.CreateBool(
            nameof(HideModifiers),
            Cs(Color.yellow, "Hide after death modifiers"),
            false);
    }
    
    public static CustomOption.NameAndColor Cs(string color, string name)
    {
        return new CustomOption.NameAndColor(name, Colors.FromHex(color));
    }

    public static CustomOption.NameAndColor Cs(Color color, string name)
    {
        return new CustomOption.NameAndColor(name, color);
    }

    public static CustomOption.NameAndColor Cs(string name)
    {
        return new CustomOption.NameAndColor(name, Color.clear);
    }

    public static void Load()
    {
        BetterOtherRolesPlugin.Logger.LogDebug("Custom options loaded");
    }
}