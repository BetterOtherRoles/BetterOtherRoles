using System.Linq;
using System;
using System.Collections.Generic;
using TheOtherRoles.EnoFw;
using TheOtherRoles.Players;
using UnityEngine;
using TheOtherRoles.Utilities;
using TheOtherRoles.EnoFw.Roles.Crewmate;
using TheOtherRoles.EnoFw.Roles.Impostor;
using TheOtherRoles.EnoFw.Roles.Modifiers;
using TheOtherRoles.EnoFw.Roles.Neutral;

namespace TheOtherRoles
{
    class RoleInfo
    {
        public Color color;
        public string name;
        public string introDescription;
        public string shortDescription;
        public RoleId roleId;
        public bool isNeutral;
        public bool isModifier;

        RoleInfo(string name, Color color, string introDescription, string shortDescription, RoleId roleId,
            bool isNeutral = false, bool isModifier = false)
        {
            this.color = color;
            this.name = name;
            this.introDescription = introDescription;
            this.shortDescription = shortDescription;
            this.roleId = roleId;
            this.isNeutral = isNeutral;
            this.isModifier = isModifier;
        }

        public static RoleInfo jester = new RoleInfo("Jester", Jester.Instance.Color, "Get voted out", "Get voted out",
            RoleId.Jester, true);

        public static RoleInfo mayor = new RoleInfo("Mayor", Mayor.Instance.Color, "Your vote counts twice",
            "Your vote counts twice", RoleId.Mayor);

        public static RoleInfo portalmaker = new RoleInfo("Portalmaker", Portalmaker.Instance.Color,
            "You can create portals", "You can create portals", RoleId.Portalmaker);

        public static RoleInfo engineer = new RoleInfo("Engineer", Engineer.Instance.Color,
            "Maintain important systems on the ship", "Repair the ship", RoleId.Engineer);

        public static RoleInfo sheriff = new RoleInfo("Sheriff", Sheriff.Instance.Color,
            "Shoot the <color=#FF1919FF>Impostors</color>", "Shoot the Impostors", RoleId.Sheriff);

        public static RoleInfo deputy = new RoleInfo("Deputy", Deputy.Instance.Color,
            "Handcuff the <color=#FF1919FF>Impostors</color>", "Handcuff the Impostors", RoleId.Deputy);

        public static RoleInfo lighter = new RoleInfo("Lighter", Lighter.Instance.Color, "Your light never goes out",
            "Your light never goes out", RoleId.Lighter);

        public static RoleInfo godfather = new RoleInfo("Godfather", Godfather.Instance.Color, "Kill all Crewmates",
            "Kill all Crewmates", RoleId.Godfather);

        public static RoleInfo mafioso = new RoleInfo("Mafioso", Mafioso.Instance.Color,
            "Work with the <color=#FF1919FF>Mafia</color> to kill the Crewmates", "Kill all Crewmates", RoleId.Mafioso);

        public static RoleInfo janitor = new RoleInfo("Janitor", Janitor.Instance.Color,
            "Work with the <color=#FF1919FF>Mafia</color> by making dead bodies disappear", "Vanish dead bodies",
            RoleId.Janitor);

        public static RoleInfo morphling = new RoleInfo("Morphling", Morphling.Instance.Color,
            "Change your look to not get caught", "Change your look", RoleId.Morphling);

        public static RoleInfo camouflager = new RoleInfo("Camouflager", Camouflager.Instance.Color,
            "Camouflage and kill the Crewmates", "Hide among others", RoleId.Camouflager);

        public static RoleInfo vampire = new RoleInfo("Vampire", Vampire.Instance.Color,
            "Kill the Crewmates with your bites", "Bite your enemies", RoleId.Vampire);

        public static RoleInfo whisperer = new RoleInfo("Whisperer", Whisperer.Instance.Color,
            "Kill the Crewmates by whispering to him", "Order someone to die or kill someone.", RoleId.Whisperer);

        public static RoleInfo undertaker = new RoleInfo("Undertaker", Undertaker.Instance.Color,
            "Hide Dead Bodies by Dragging them to a secret location", "Drag dead bodies away.", RoleId.Whisperer);

        public static RoleInfo eraser = new RoleInfo("Eraser", Eraser.Instance.Color,
            "Kill the Crewmates and erase their roles", "Erase the roles of your enemies", RoleId.Eraser);

        public static RoleInfo trickster = new RoleInfo("Trickster", Trickster.Instance.Color,
            "Use your jack-in-the-boxes to surprise others", "Surprise your enemies", RoleId.Trickster);

        public static RoleInfo cleaner = new RoleInfo("Cleaner", Cleaner.Instance.Color,
            "Kill everyone and leave no traces", "Clean up dead bodies", RoleId.Cleaner);

        public static RoleInfo warlock = new RoleInfo("Warlock", Warlock.Instance.Color,
            "Curse other players and kill everyone", "Curse and kill everyone", RoleId.Warlock);

        public static RoleInfo bountyHunter = new RoleInfo("Bounty Hunter", BountyHunter.Instance.Color,
            "Hunt your bounty down", "Hunt your bounty down", RoleId.BountyHunter);

        public static RoleInfo detective = new RoleInfo("Detective", Detective.Instance.Color,
            "Find the <color=#FF1919FF>Impostors</color> by examining footprints", "Examine footprints",
            RoleId.Detective);

        public static RoleInfo timeMaster = new RoleInfo("Time Master", TimeMaster.Instance.Color,
            "Save yourself with your time shield", "Use your time shield", RoleId.TimeMaster);

        public static RoleInfo medic = new RoleInfo("Medic", Medic.Instance.Color, "Protect someone with your shield",
            "Protect other players", RoleId.Medic);

        public static RoleInfo swapper = new RoleInfo("Swapper", Swapper.Instance.Color,
            "Swap votes to exile the <color=#FF1919FF>Impostors</color>", "Swap votes", RoleId.Swapper);

        public static RoleInfo seer = new RoleInfo("Seer", Seer.Instance.Color, "You will see players die",
            "You will see players die", RoleId.Seer);

        public static RoleInfo hacker = new RoleInfo("Hacker", Hacker.Instance.Color,
            "Hack systems to find the <color=#FF1919FF>Impostors</color>", "Hack to find the Impostors", RoleId.Hacker);

        public static RoleInfo tracker = new RoleInfo("Tracker", Tracker.Instance.Color,
            "Track the <color=#FF1919FF>Impostors</color> down", "Track the Impostors down", RoleId.Tracker);

        public static RoleInfo snitch = new RoleInfo("Snitch", Snitch.Instance.Color,
            "Finish your tasks to find the <color=#FF1919FF>Impostors</color>", "Finish your tasks", RoleId.Snitch);

        public static RoleInfo jackal = new RoleInfo("Jackal", Jackal.Instance.Color,
            "Kill all Crewmates and <color=#FF1919FF>Impostors</color> to win", "Kill everyone", RoleId.Jackal, true);

        public static RoleInfo sidekick = new RoleInfo("Sidekick", Sidekick.Instance.Color, "Help your Jackal to kill everyone",
            "Help your Jackal to kill everyone", RoleId.Sidekick, true);

        public static RoleInfo spy = new RoleInfo("Spy", Spy.Instance.Color,
            "Confuse the <color=#FF1919FF>Impostors</color>", "Confuse the Impostors", RoleId.Spy);

        public static RoleInfo securityGuard = new RoleInfo("Security Guard", SecurityGuard.Instance.Color,
            "Seal vents and place cameras", "Seal vents and place cameras", RoleId.SecurityGuard);

        public static RoleInfo arsonist = new RoleInfo("Arsonist", Arsonist.Instance.Color, "Let them burn",
            "Let them burn", RoleId.Arsonist, true);

        public static RoleInfo goodGuesser = new RoleInfo("Nice Guesser", Guesser.Instance.Color, "Guess and shoot",
            "Guess and shoot", RoleId.NiceGuesser);

        public static RoleInfo badGuesser = new RoleInfo("Evil Guesser", Palette.ImpostorRed, "Guess and shoot",
            "Guess and shoot", RoleId.EvilGuesser);

        public static RoleInfo vulture = new RoleInfo("Vulture", Vulture.Instance.Color, "Eat corpses to win", "Eat dead bodies",
            RoleId.Vulture, true);

        public static RoleInfo medium = new RoleInfo("Medium", Medium.Instance.Color,
            "Question the souls of the dead to gain information", "Question the souls", RoleId.Medium);

        public static RoleInfo trapper = new RoleInfo("Trapper", Trapper.Instance.Color,
            "Place traps to find the Impostors", "Place traps", RoleId.Trapper);

        public static RoleInfo lawyer = new RoleInfo("Lawyer", Lawyer.Instance.Color, "Defend your client", "Defend your client",
            RoleId.Lawyer, true);

        public static RoleInfo prosecutor = new RoleInfo("Prosecutor", Lawyer.Instance.Color, "Vote out your target",
            "Vote out your target", RoleId.Prosecutor, true);

        public static RoleInfo pursuer = new RoleInfo("Pursuer", Pursuer.Instance.Color, "Blank the Impostors",
            "Blank the Impostors", RoleId.Pursuer);

        public static RoleInfo impostor = new RoleInfo("Impostor", Palette.ImpostorRed,
            Helpers.cs(Palette.ImpostorRed, "Sabotage and kill everyone"), "Sabotage and kill everyone",
            RoleId.Impostor);

        public static RoleInfo crewmate = new RoleInfo("Crewmate", Color.white, "Find the Impostors",
            "Find the Impostors", RoleId.Crewmate);

        public static RoleInfo witch = new RoleInfo("Witch", Witch.Instance.Color, "Cast a spell upon your foes",
            "Cast a spell upon your foes", RoleId.Witch);

        public static RoleInfo ninja = new RoleInfo("Ninja", Ninja.Instance.Color, "Surprise and assassinate your foes",
            "Surprise and assassinate your foes", RoleId.Ninja);

        public static RoleInfo thief = new RoleInfo("Thief", Thief.Instance.Color, "Steal a killers role by killing them",
            "Steal a killers role", RoleId.Thief, true);

        public static RoleInfo fallen = new RoleInfo("Fallen", Thief.Instance.Color, "A Fallen Angel that lost his wings",
            "U did get u'r role stealed !", RoleId.Fallen, true);

        public static RoleInfo bomber = new RoleInfo("Bomber", Bomber.Instance.Color, "Bomb all Crewmates",
            "Bomb all Crewmates", RoleId.Bomber);

        public static RoleInfo hunter = new RoleInfo("Hunter", Palette.ImpostorRed,
            Helpers.cs(Palette.ImpostorRed, "Seek and kill everyone"), "Seek and kill everyone", RoleId.Impostor);

        public static RoleInfo hunted = new RoleInfo("Hunted", Color.white, "Hide", "Hide", RoleId.Crewmate);


        // Modifier
        public static RoleInfo bloody = new RoleInfo("Bloody", Color.yellow, "Your killer leaves a bloody trail",
            "Your killer leaves a bloody trail", RoleId.Bloody, false, true);

        public static RoleInfo antiTeleport = new RoleInfo("Anti tp", Color.yellow, "You will not get teleported",
            "You will not get teleported", RoleId.AntiTeleport, false, true);

        public static RoleInfo tiebreaker = new RoleInfo("Tiebreaker", Color.yellow, "Your vote breaks the tie",
            "Break the tie", RoleId.Tiebreaker, false, true);

        public static RoleInfo bait = new RoleInfo("Bait", Color.yellow, "Bait your enemies", "Bait your enemies",
            RoleId.Bait, false, true);

        public static RoleInfo sunglasses = new RoleInfo("Sunglasses", Color.yellow, "You got the sunglasses",
            "Your vision is reduced", RoleId.Sunglasses, false, true);

        public static RoleInfo lover = new RoleInfo("Lover", Lovers.Instance.Color, $"You are in love", $"You are in love",
            RoleId.Lover, false, true);

        public static RoleInfo mini = new RoleInfo("Mini", Color.yellow, "No one will harm you until you grow up",
            "No one will harm you", RoleId.Mini, false, true);

        public static RoleInfo vip = new RoleInfo("VIP", Color.yellow, "You are the VIP",
            "Everyone is notified when you die", RoleId.Vip, false, true);

        public static RoleInfo invert = new RoleInfo("Invert", Color.yellow, "Your movement is inverted",
            "Your movement is inverted", RoleId.Invert, false, true);

        public static RoleInfo chameleon = new RoleInfo("Chameleon", Color.yellow, "You're hard to see when not moving",
            "You're hard to see when not moving", RoleId.Chameleon, false, true);

        public static RoleInfo shifter = new RoleInfo("Shifter", Color.yellow, "Shift your role", "Shift your role",
            RoleId.Shifter, false, true);


        public static List<RoleInfo> allRoleInfos = new List<RoleInfo>()
        {
            impostor,
            godfather,
            mafioso,
            janitor,
            morphling,
            camouflager,
            vampire,
            whisperer,
            undertaker,
            eraser,
            trickster,
            cleaner,
            warlock,
            bountyHunter,
            witch,
            ninja,
            bomber,
            goodGuesser,
            badGuesser,
            lover,
            jester,
            arsonist,
            jackal,
            sidekick,
            vulture,
            pursuer,
            lawyer,
            thief,
            fallen,
            prosecutor,
            crewmate,
            mayor,
            portalmaker,
            engineer,
            sheriff,
            deputy,
            lighter,
            detective,
            timeMaster,
            medic,
            swapper,
            seer,
            hacker,
            tracker,
            snitch,
            spy,
            securityGuard,
            bait,
            medium,
            trapper,
            bloody,
            antiTeleport,
            tiebreaker,
            sunglasses,
            mini,
            vip,
            invert,
            chameleon,
            shifter
        };

        public static List<RoleInfo> getRoleInfoForPlayer(PlayerControl p, bool showModifier = true)
        {
            List<RoleInfo> infos = new List<RoleInfo>();
            if (p == null) return infos;

            // Modifier
            if (showModifier)
            {
                // after dead modifier
                if (!CustomOptions.HideModifiers || PlayerControl.LocalPlayer.Data.IsDead ||
                    AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Ended)
                {
                    if (Bait.Instance.Is(p)) infos.Add(bait);
                    if (Bloody.Instance.Is(p)) infos.Add(bloody);
                    if (Vip.Instance.Is(p)) infos.Add(vip);
                }

                if (p == Lovers.Instance.Lover1 || p == Lovers.Instance.Lover2) infos.Add(lover);
                if (p == Tiebreaker.Instance.Player) infos.Add(tiebreaker);
                if (AntiTeleport.Instance.Is(p)) infos.Add(antiTeleport);
                if (Sunglasses.Instance.Is(p)) infos.Add(sunglasses);
                if (p == Mini.Instance.Player) infos.Add(mini);
                if (Invert.Instance.Is(p)) infos.Add(invert);
                if (Chameleon.Instance.Is(p)) infos.Add(chameleon);
                if (p == Shifter.Instance.Player) infos.Add(shifter);
            }

            int count = infos.Count; // Save count after modifiers are added so that the role count can be checked

            // Special roles
            if (p == Jester.Instance.Player) infos.Add(jester);
            if (p == Mayor.Instance.Player) infos.Add(mayor);
            if (p == Portalmaker.Instance.Player) infos.Add(portalmaker);
            if (p == Engineer.Instance.Player) infos.Add(engineer);
            if (p == Sheriff.Instance.Player || p == Sheriff.Instance.FormerSheriff) infos.Add(sheriff);
            if (p == Deputy.Instance.Player) infos.Add(deputy);
            if (p == Lighter.Instance.Player) infos.Add(lighter);
            if (p == Godfather.Instance.Player) infos.Add(godfather);
            if (p == Mafioso.Instance.Player) infos.Add(mafioso);
            if (p == Janitor.Instance.Player) infos.Add(janitor);
            if (p == Morphling.Instance.Player) infos.Add(morphling);
            if (p == Camouflager.Instance.Player) infos.Add(camouflager);
            if (p == Vampire.Instance.Player) infos.Add(vampire);
            if (p == Whisperer.Instance.Player) infos.Add(whisperer);
            if (p == Undertaker.Instance.Player) infos.Add(undertaker);
            if (p == Eraser.Instance.Player) infos.Add(eraser);
            if (p == Trickster.Instance.Player) infos.Add(trickster);
            if (p == Cleaner.Instance.Player) infos.Add(cleaner);
            if (p == Warlock.Instance.Player) infos.Add(warlock);
            if (p == Witch.Instance.Player) infos.Add(witch);
            if (p == Ninja.Instance.Player) infos.Add(ninja);
            if (p == Bomber.Instance.Player) infos.Add(bomber);
            if (p == Detective.Instance.Player) infos.Add(detective);
            if (p == TimeMaster.Instance.Player) infos.Add(timeMaster);
            if (p == Medic.Instance.Player) infos.Add(medic);
            if (p == Swapper.Instance.Player) infos.Add(swapper);
            if (p == Seer.Instance.Player) infos.Add(seer);
            if (p == Hacker.Instance.Player) infos.Add(hacker);
            if (p == Tracker.Instance.Player) infos.Add(tracker);
            if (p == Snitch.Instance.Player) infos.Add(snitch);
            if (p == Jackal.Instance.Player ||
                (Jackal.Instance.FormerJackals != null && Jackal.Instance.FormerJackals.Any(x => x.PlayerId == p.PlayerId)))
                infos.Add(jackal);
            if (p == Sidekick.Instance.Player) infos.Add(sidekick);
            if (p == Spy.Instance.Player) infos.Add(spy);
            if (p == SecurityGuard.Instance.Player) infos.Add(securityGuard);
            if (p == Arsonist.Instance.Player) infos.Add(arsonist);
            if (p == Guesser.Instance.NiceGuesser) infos.Add(goodGuesser);
            if (p == Guesser.Instance.EvilGuesser) infos.Add(badGuesser);
            if (p == BountyHunter.Instance.Player) infos.Add(bountyHunter);
            if (p == Vulture.Instance.Player) infos.Add(vulture);
            if (p == Medium.Instance.Player) infos.Add(medium);
            if (p == Lawyer.Instance.Player && !Lawyer.Instance.IsProsecutor) infos.Add(lawyer);
            if (p == Lawyer.Instance.Player && Lawyer.Instance.IsProsecutor) infos.Add(prosecutor);
            if (p == Trapper.Instance.Player) infos.Add(trapper);
            if (p == Pursuer.Instance.Player) infos.Add(pursuer);
            if (p == Thief.Instance.Player) infos.Add(thief);
            if (p == Fallen.Instance.Player) infos.Add(fallen);

            // Default roles (just impostor, just crewmate, or hunter / hunted for hide n seek
            if (infos.Count == count)
            {
                if (p.Data.Role.IsImpostor)
                    infos.Add(TORMapOptions.gameMode == CustomGamemodes.HideNSeek
                        ? RoleInfo.hunter
                        : RoleInfo.impostor);
                else
                    infos.Add(TORMapOptions.gameMode == CustomGamemodes.HideNSeek
                        ? RoleInfo.hunted
                        : RoleInfo.crewmate);
            }

            return infos;
        }

        public static String GetRolesString(PlayerControl p, bool useColors, bool showModifier = true,
            bool suppressGhostInfo = false)
        {
            string roleName;
            roleName = String.Join(" ",
                getRoleInfoForPlayer(p, showModifier).Select(x => useColors ? Helpers.cs(x.color, x.name) : x.name)
                    .ToArray());
            if (Lawyer.Instance.Target != null && p.PlayerId == Lawyer.Instance.Target.PlayerId &&
                CachedPlayer.LocalPlayer.PlayerControl != Lawyer.Instance.Target)
                roleName += (useColors ? Helpers.cs(Pursuer.Instance.Color, " §") : " §");
            if (HandleGuesser.isGuesserGm && HandleGuesser.isGuesser(p.PlayerId)) roleName += " (Guesser)";

            if (suppressGhostInfo || p == null) return roleName;
            if (p == Shifter.Instance.Player &&
                (CachedPlayer.LocalPlayer.PlayerControl == Shifter.Instance.Player || Helpers.shouldShowGhostInfo()) &&
                Shifter.Instance.FutureShift != null)
                roleName += Helpers.cs(Color.yellow, " ← " + Shifter.Instance.FutureShift.Data.PlayerName);
            if (p == Vulture.Instance.Player && (CachedPlayer.LocalPlayer.PlayerControl == Vulture.Instance.Player ||
                                         Helpers.shouldShowGhostInfo()))
                roleName += Helpers.cs(Vulture.Instance.Color, $" ({Vulture.Instance.EatNumberToWin - Vulture.Instance.EatenBodies} left)");
            if (!Helpers.shouldShowGhostInfo()) return roleName;
            if (Eraser.Instance.FutureErased.Contains(p))
                roleName = Helpers.cs(Color.gray, "(erased) ") + roleName;
            if (Vampire.Instance.Player != null && !Vampire.Instance.Player.Data.IsDead &&
                Vampire.Instance.Bitten == p && !p.Data.IsDead)
                roleName = Helpers.cs(Vampire.Instance.Color,
                    $"(bitten : {(int)HudManagerStartPatch.vampireKillButton.Timer + 1}s) ") + roleName;
            if (p == Whisperer.Instance.WhisperVictim)
                roleName = Helpers.cs(Whisperer.Instance.Color, $"(whsp) ") + roleName;
            if (p == Whisperer.Instance.WhisperVictimToKill)
                roleName = Helpers.cs(Whisperer.Instance.Color,
                    $"(whsp kill : {(int)HudManagerStartPatch.whispererKillButton.Timer + 1}s) ") + roleName;
            if (Undertaker.Instance.DraggedBody != null && p == Undertaker.Instance.Player)
                roleName = Helpers.cs(Undertaker.Instance.Color, "(dragging) ") + roleName;
            if (Deputy.Instance.HandcuffedPlayers.Contains(p.PlayerId))
                roleName = Helpers.cs(Color.gray, "(cuffed) ") + roleName;
            if (Deputy.Instance.HandcuffedKnows.ContainsKey(p.PlayerId)) // Active cuff
                roleName = Helpers.cs(Deputy.Instance.Color, "(cuffed) ") + roleName;
            if (p == Warlock.Instance.CurseVictim)
                roleName = Helpers.cs(Warlock.Instance.Color, "(cursed) ") + roleName;
            if (p == Ninja.Instance.MarkedTarget)
                roleName = Helpers.cs(Ninja.Instance.Color, "(marked) ") + roleName;
            if (Pursuer.Instance.BlankedList.Contains(p) && !p.Data.IsDead)
                roleName = Helpers.cs(Pursuer.Instance.Color, "(blanked) ") + roleName;
            if (Witch.Instance.FutureSpelled.Contains(p) &&
                !MeetingHud.Instance) // This is already displayed in meetings!
                roleName = Helpers.cs(Witch.Instance.Color, "☆ ") + roleName;
            if (BountyHunter.Instance.Bounty == p)
                roleName = Helpers.cs(BountyHunter.Instance.Color, "(bounty) ") + roleName;
            if (Arsonist.Instance.DousedPlayers.Contains(p))
                roleName = Helpers.cs(Arsonist.Instance.Color, "♨ ") + roleName;
            if (p == Arsonist.Instance.Player)
                roleName += Helpers.cs(Arsonist.Instance.Color,
                    $" ({CachedPlayer.AllPlayers.Count(x => { return x.PlayerControl != Arsonist.Instance.Player && !x.Data.IsDead && !x.Data.Disconnected && Arsonist.Instance.DousedPlayers.All(y => y.PlayerId != x.PlayerId); })} left)");
            if (p == Jackal.Instance.FakeSidekick)
                roleName = Helpers.cs(Sidekick.Instance.Color, $" (fake SK)") + roleName;
            return roleName;
        }
    }
}