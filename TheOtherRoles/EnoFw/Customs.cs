using TheOtherRoles.EnoFw.Roles.Crewmate;
using TheOtherRoles.EnoFw.Roles.Impostor;
using TheOtherRoles.EnoFw.Roles.Modifiers;
using TheOtherRoles.EnoFw.Roles.Neutral;

namespace TheOtherRoles.EnoFw;

public static class Customs
{
    public static class Roles
    {
        // Crewmate roles
        public static Deputy Deputy;
        public static Detective Detective;
        public static Engineer Engineer;
        public static Hacker Hacker;
        public static Lighter Lighter;
        public static Mayor Mayor;
        public static Medic Medic;
        public static Medium Medium;
        public static Portalmaker Portalmaker;
        public static Pursuer Pursuer;
        public static SecurityGuard SecurityGuard;
        public static Seer Seer;
        public static Sheriff Sheriff;
        public static Snitch Snitch;
        public static Spy Spy;
        public static Swapper Swapper;
        public static TimeMaster TimeMaster;
        public static Tracker Tracker;
        public static Trapper Trapper;
        
        // Impostor roles
        public static Bomber Bomber;
        public static BountyHunter BountyHunter;
        public static Camouflager Camouflager;
        public static Cleaner Cleaner;
        public static Eraser Eraser;
        public static Godfather Godfather;
        public static Janitor Janitor;
        public static Mafioso Mafioso;
        public static Morphling Morphling;
        public static Ninja Ninja;
        public static Trickster Trickster;
        public static Undertaker Undertaker;
        public static Vampire Vampire;
        public static Warlock Warlock;
        public static Whisperer Whisperer;
        public static Witch Witch;
        
        // Neutral roles
        public static Arsonist Arsonist;
        public static Fallen Fallen;
        public static Guesser Guesser;
        public static Jackal Jackal;
        public static Jester Jester;
        public static Lawyer Lawyer;
        public static Sidekick Sidekick;
        public static Thief Thief;
        public static Vulture Vulture;
    }
    
    public static class Modifiers
    {
        public static AntiTeleport AntiTeleport;
        public static Bait Bait;
        public static Bloody Bloody;
        public static Chameleon Chameleon;
        public static Invert Invert;
        public static Lovers Lovers;
        public static Mini Mini;
        public static Shifter Shifter;
        public static Sunglasses Sunglasses;
        public static Tiebreaker Tiebreaker;
        public static Vip Vip;
    }

    public static void Load()
    {
        Roles.Deputy = Deputy.Instance;
        Roles.Detective = Detective.Instance;
        Roles.Engineer = Engineer.Instance;
        Roles.Hacker = Hacker.Instance;
        Roles.Lighter = Lighter.Instance;
        Roles.Mayor = Mayor.Instance;
        Roles.Medic = Medic.Instance;
        Roles.Medium = Medium.Instance;
        Roles.Portalmaker = Portalmaker.Instance;
        Roles.Pursuer = Pursuer.Instance;
        Roles.SecurityGuard = SecurityGuard.Instance;
        Roles.Seer = Seer.Instance;
        Roles.Sheriff = Sheriff.Instance;
        Roles.Snitch = Snitch.Instance;
        Roles.Spy = Spy.Instance;
        Roles.Swapper = Swapper.Instance;
        Roles.TimeMaster = TimeMaster.Instance;
        Roles.Tracker = Tracker.Instance;
        Roles.Trapper = Trapper.Instance;
        Roles.Bomber = Bomber.Instance;
        Roles.BountyHunter = BountyHunter.Instance;
        Roles.Camouflager = Camouflager.Instance;
        Roles.Cleaner = Cleaner.Instance;
        Roles.Eraser = Eraser.Instance;
        Roles.Godfather = Godfather.Instance;
        Roles.Janitor = Janitor.Instance;
        Roles.Mafioso = Mafioso.Instance;
        Roles.Morphling = Morphling.Instance;
        Roles.Ninja = Ninja.Instance;
        Roles.Trickster = Trickster.Instance;
        Roles.Undertaker = Undertaker.Instance;
        Roles.Vampire = Vampire.Instance;
        Roles.Warlock = Warlock.Instance;
        Roles.Whisperer = Whisperer.Instance;
        Roles.Witch = Witch.Instance;
        Roles.Arsonist = Arsonist.Instance;
        Roles.Fallen = Fallen.Instance;
        Roles.Guesser = Guesser.Instance;
        Roles.Jackal = Jackal.Instance;
        Roles.Jester = Jester.Instance;
        Roles.Lawyer = Lawyer.Instance;
        Roles.Sidekick = Sidekick.Instance;
        Roles.Thief = Thief.Instance;
        Roles.Vulture = Vulture.Instance;
        Modifiers.AntiTeleport = AntiTeleport.Instance;
        Modifiers.Bait = Bait.Instance;
        Modifiers.Bloody = Bloody.Instance;
        Modifiers.Chameleon = Chameleon.Instance;
        Modifiers.Invert = Invert.Instance;
        Modifiers.Lovers = Lovers.Instance;
        Modifiers.Mini = Mini.Instance;
        Modifiers.Shifter = Shifter.Instance;
        Modifiers.Sunglasses = Sunglasses.Instance;
        Modifiers.Tiebreaker = Tiebreaker.Instance;
        Modifiers.Vip = Vip.Instance;
    }
}