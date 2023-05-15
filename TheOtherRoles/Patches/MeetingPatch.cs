using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using static TheOtherRoles.TheOtherRoles;
using static TheOtherRoles.TORMapOptions;
using TheOtherRoles.Objects;
using System;
using TheOtherRoles.EnoFw;
using TheOtherRoles.EnoFw.Modules;
using TheOtherRoles.EnoFw.Roles.Crewmate;
using TheOtherRoles.EnoFw.Roles.Impostor;
using TheOtherRoles.EnoFw.Roles.Modifiers;
using TheOtherRoles.EnoFw.Roles.Neutral;
using TheOtherRoles.Modules;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;
using UnityEngine;

namespace TheOtherRoles.Patches
{
    [HarmonyPatch]
    class MeetingHudPatch
    {
        static bool[] selections;
        static SpriteRenderer[] renderers;
        private static GameData.PlayerInfo target = null;
        private const float scale = 0.65f;
        private static TMPro.TextMeshPro meetingExtraButtonText;
        private static PassiveButton[] swapperButtonList;
        private static TMPro.TextMeshPro meetingExtraButtonLabel;
        private static PlayerVoteArea swapped1 = null;
        private static PlayerVoteArea swapped2 = null;

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
        class MeetingCalculateVotesPatch
        {
            private static Dictionary<byte, int> CalculateVotes(MeetingHud __instance)
            {
                Dictionary<byte, int> dictionary = new Dictionary<byte, int>();
                for (int i = 0; i < __instance.playerStates.Length; i++)
                {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                    if (playerVoteArea.VotedFor != 252 && playerVoteArea.VotedFor != 255 &&
                        playerVoteArea.VotedFor != 254)
                    {
                        PlayerControl player = Helpers.playerById((byte)playerVoteArea.TargetPlayerId);
                        if (player == null || player.Data == null || player.Data.IsDead || player.Data.Disconnected)
                            continue;

                        int currentVotes;
                        int additionalVotes = Mayor.Instance.HasPlayer &&
                                              Mayor.Instance.Player.PlayerId == playerVoteArea.TargetPlayerId &&
                                              Mayor.Instance.VoteTwice
                            ? 2
                            : 1; // Mayor vote
                        if (dictionary.TryGetValue(playerVoteArea.VotedFor, out currentVotes))
                            dictionary[playerVoteArea.VotedFor] = currentVotes + additionalVotes;
                        else
                            dictionary[playerVoteArea.VotedFor] = additionalVotes;
                    }
                }

                // Swapper swap votes
                if (Swapper.Instance.Player != null && !Swapper.Instance.Player.Data.IsDead)
                {
                    swapped1 = null;
                    swapped2 = null;
                    foreach (PlayerVoteArea playerVoteArea in __instance.playerStates)
                    {
                        if (playerVoteArea.TargetPlayerId == Swapper.Instance.PlayerId1) swapped1 = playerVoteArea;
                        if (playerVoteArea.TargetPlayerId == Swapper.Instance.PlayerId2) swapped2 = playerVoteArea;
                    }

                    if (swapped1 != null && swapped2 != null)
                    {
                        if (!dictionary.ContainsKey(swapped1.TargetPlayerId)) dictionary[swapped1.TargetPlayerId] = 0;
                        if (!dictionary.ContainsKey(swapped2.TargetPlayerId)) dictionary[swapped2.TargetPlayerId] = 0;
                        int tmp = dictionary[swapped1.TargetPlayerId];
                        dictionary[swapped1.TargetPlayerId] = dictionary[swapped2.TargetPlayerId];
                        dictionary[swapped2.TargetPlayerId] = tmp;
                    }
                }


                return dictionary;
            }


            static bool Prefix(MeetingHud __instance)
            {
                if (__instance.playerStates.All((PlayerVoteArea ps) => ps.AmDead || ps.DidVote))
                {
                    // If skipping is disabled, replace skipps/no-votes with self vote
                    if (target == null && blockSkippingInEmergencyMeetings && noVoteIsSelfVote)
                    {
                        foreach (PlayerVoteArea playerVoteArea in __instance.playerStates)
                        {
                            if (playerVoteArea.VotedFor == byte.MaxValue - 1)
                                playerVoteArea.VotedFor = playerVoteArea.TargetPlayerId; // TargetPlayerId
                        }
                    }

                    Dictionary<byte, int> self = CalculateVotes(__instance);
                    bool tie;
                    KeyValuePair<byte, int> max = self.MaxPair(out tie);
                    GameData.PlayerInfo exiled = GameData.Instance.AllPlayers.ToArray()
                        .FirstOrDefault(v => !tie && v.PlayerId == max.Key && !v.IsDead);

                    // TieBreaker 
                    List<GameData.PlayerInfo> potentialExiled = new List<GameData.PlayerInfo>();
                    bool skipIsTie = false;
                    if (self.Count > 0)
                    {
                        Tiebreaker.Instance.IsTiebreak = false;
                        int maxVoteValue = self.Values.Max();
                        PlayerVoteArea tb = null;
                        if (Tiebreaker.Instance.Player != null)
                            tb = __instance.playerStates.ToArray().FirstOrDefault(x =>
                                x.TargetPlayerId == Tiebreaker.Instance.Player.PlayerId);
                        bool isTiebreakerSkip = tb == null || tb.VotedFor == 253;
                        if (tb != null && tb.AmDead) isTiebreakerSkip = true;

                        foreach (KeyValuePair<byte, int> pair in self)
                        {
                            if (pair.Value != maxVoteValue || isTiebreakerSkip) continue;
                            if (pair.Key != 253)
                                potentialExiled.Add(GameData.Instance.AllPlayers.ToArray()
                                    .FirstOrDefault(x => x.PlayerId == pair.Key));
                            else
                                skipIsTie = true;
                        }
                    }

                    MeetingHud.VoterState[] array = new MeetingHud.VoterState[__instance.playerStates.Length];
                    for (int i = 0; i < __instance.playerStates.Length; i++)
                    {
                        PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                        array[i] = new MeetingHud.VoterState
                        {
                            VoterId = playerVoteArea.TargetPlayerId,
                            VotedForId = playerVoteArea.VotedFor
                        };

                        if (Tiebreaker.Instance.Player == null ||
                            playerVoteArea.TargetPlayerId != Tiebreaker.Instance.Player.PlayerId) continue;

                        byte tiebreakerVote = playerVoteArea.VotedFor;
                        if (swapped1 != null && swapped2 != null)
                        {
                            if (tiebreakerVote == swapped1.TargetPlayerId) tiebreakerVote = swapped2.TargetPlayerId;
                            else if (tiebreakerVote == swapped2.TargetPlayerId)
                                tiebreakerVote = swapped1.TargetPlayerId;
                        }

                        if (potentialExiled.FindAll(x => x != null && x.PlayerId == tiebreakerVote).Count > 0 &&
                            (potentialExiled.Count > 1 || skipIsTie))
                        {
                            exiled = potentialExiled.ToArray().FirstOrDefault(v => v.PlayerId == tiebreakerVote);
                            tie = false;
                            Tiebreaker.SetTiebreak();
                        }
                    }

                    // RPCVotingComplete
                    __instance.RpcVotingComplete(array, exiled, tie);
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.BloopAVoteIcon))]
        class MeetingHudBloopAVoteIconPatch
        {
            public static bool Prefix(MeetingHud __instance, [HarmonyArgument(0)] GameData.PlayerInfo voterPlayer,
                [HarmonyArgument(1)] int index, [HarmonyArgument(2)] Transform parent)
            {
                SpriteRenderer spriteRenderer =
                    UnityEngine.Object.Instantiate<SpriteRenderer>(__instance.PlayerVotePrefab);
                int cId = voterPlayer.DefaultOutfit.ColorId;
                if (!(!GameOptionsManager.Instance.currentNormalGameOptions.AnonymousVotes ||
                      (CachedPlayer.LocalPlayer.Data.IsDead && TORMapOptions.ghostsSeeVotes) || CustomGuid.IsDevMode ||
                      Mayor.Instance.IsLocalPlayer && Mayor.Instance.CanSeeVoteColors &&
                      TasksHandler.taskInfo(CachedPlayer.LocalPlayer.Data).Item1 >=
                      Mayor.Instance.TasksNeededToSeeVoteColors))
                    voterPlayer.Object.SetColor(6);
                voterPlayer.Object.SetPlayerMaterialColors(spriteRenderer);
                spriteRenderer.transform.SetParent(parent);
                spriteRenderer.transform.localScale = Vector3.zero;
                __instance.StartCoroutine(Effects.Bloop((float)index * 0.3f, spriteRenderer.transform, 1f, 0.5f));
                parent.GetComponent<VoteSpreader>().AddVote(spriteRenderer);
                voterPlayer.Object.SetColor(cId);
                return false;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.PopulateResults))]
        class MeetingHudPopulateVotesPatch
        {
            static bool Prefix(MeetingHud __instance, Il2CppStructArray<MeetingHud.VoterState> states)
            {
                // Swapper swap

                PlayerVoteArea swapped1 = null;
                PlayerVoteArea swapped2 = null;
                foreach (PlayerVoteArea playerVoteArea in __instance.playerStates)
                {
                    if (playerVoteArea.TargetPlayerId == Swapper.Instance.PlayerId1) swapped1 = playerVoteArea;
                    if (playerVoteArea.TargetPlayerId == Swapper.Instance.PlayerId2) swapped2 = playerVoteArea;
                }

                bool doSwap = swapped1 != null && swapped2 != null && Swapper.Instance.Player != null &&
                              !Swapper.Instance.Player.Data.IsDead;
                if (doSwap)
                {
                    __instance.StartCoroutine(Effects.Slide3D(swapped1.transform, swapped1.transform.localPosition,
                        swapped2.transform.localPosition, 1.5f));
                    __instance.StartCoroutine(Effects.Slide3D(swapped2.transform, swapped2.transform.localPosition,
                        swapped1.transform.localPosition, 1.5f));
                }


                __instance.TitleText.text =
                    FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MeetingVotingResults,
                        new Il2CppReferenceArray<Il2CppSystem.Object>(0));
                int num = 0;
                for (int i = 0; i < __instance.playerStates.Length; i++)
                {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                    byte targetPlayerId = playerVoteArea.TargetPlayerId;
                    // Swapper change playerVoteArea that gets the votes
                    if (doSwap && playerVoteArea.TargetPlayerId == swapped1.TargetPlayerId) playerVoteArea = swapped2;
                    else if (doSwap && playerVoteArea.TargetPlayerId == swapped2.TargetPlayerId)
                        playerVoteArea = swapped1;

                    playerVoteArea.ClearForResults();
                    int num2 = 0;
                    bool mayorFirstVoteDisplayed = false;
                    for (int j = 0; j < states.Length; j++)
                    {
                        MeetingHud.VoterState voterState = states[j];
                        GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(voterState.VoterId);
                        if (playerById == null)
                        {
                            Debug.LogError(
                                string.Format("Couldn't find player info for voter: {0}", voterState.VoterId));
                        }
                        else if (i == 0 && voterState.SkippedVote && !playerById.IsDead)
                        {
                            __instance.BloopAVoteIcon(playerById, num, __instance.SkippedVoting.transform);
                            num++;
                        }
                        else if (voterState.VotedForId == targetPlayerId && !playerById.IsDead)
                        {
                            __instance.BloopAVoteIcon(playerById, num2, playerVoteArea.transform);
                            num2++;
                        }

                        // Major vote, redo this iteration to place a second vote
                        if (Mayor.Instance.HasPlayer && voterState.VoterId == (sbyte)Mayor.Instance.Player.PlayerId &&
                            !mayorFirstVoteDisplayed && Mayor.Instance.VoteTwice)
                        {
                            mayorFirstVoteDisplayed = true;
                            j--;
                        }
                    }
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
        class MeetingHudVotingCompletedPatch
        {
            static void Postfix(MeetingHud __instance, [HarmonyArgument(0)] byte[] states,
                [HarmonyArgument(1)] GameData.PlayerInfo exiled, [HarmonyArgument(2)] bool tie)
            {
                // Regenerate RandomSeeds
                RandomSeed.GenerateSeed();

                // Reset swapper values
                Swapper.Instance.PlayerId1 = Byte.MaxValue;
                Swapper.Instance.PlayerId2 = Byte.MaxValue;

                // Lovers, Lawyer & Pursuer save next to be exiled, because RPC of ending game comes before RPC of exiled
                Lovers.Instance.NotAckedExiledIsLover = false;
                Pursuer.Instance.NotAckedExiled = false;
                if (exiled != null)
                {
                    Lovers.Instance.NotAckedExiledIsLover =
                        ((Lovers.Instance.Lover1 != null && Lovers.Instance.Lover1.PlayerId == exiled.PlayerId) ||
                         (Lovers.Instance.Lover2 != null && Lovers.Instance.Lover2.PlayerId == exiled.PlayerId));
                    Pursuer.Instance.NotAckedExiled =
                        (Pursuer.Instance.Player != null && Pursuer.Instance.Player.PlayerId == exiled.PlayerId) ||
                        (Lawyer.Instance.Player != null && Lawyer.Instance.Target != null &&
                         Lawyer.Instance.Target.PlayerId == exiled.PlayerId &&
                         Lawyer.Instance.Target != Jester.Instance.Player && !Lawyer.Instance.IsProsecutor);
                }

                // Mini
                if (!Mini.Instance.IsGrowingUpInMeeting)
                    Mini.Instance.TimeOfGrowthStart = Mini.Instance.TimeOfGrowthStart
                        .Add(DateTime.UtcNow.Subtract(Mini.Instance.TimeOfMeetingStart)).AddSeconds(10);

                // Snitch
                if (Snitch.Instance.Player != null && !Snitch.Instance.NeedsUpdate &&
                    Snitch.Instance.Player.Data.IsDead && Snitch.Instance.Text != null)
                {
                    UnityEngine.Object.Destroy(Snitch.Instance.Text);
                }
            }
        }


        static void swapperOnClick(int i, MeetingHud __instance)
        {
            if (__instance.state == MeetingHud.VoteStates.Results || Swapper.Instance.Charges <= 0) return;
            if (__instance.playerStates[i].AmDead) return;

            int selectedCount = selections.Where(b => b).Count();
            SpriteRenderer renderer = renderers[i];

            if (selectedCount == 0)
            {
                renderer.color = Color.yellow;
                selections[i] = true;
            }
            else if (selectedCount == 1)
            {
                if (selections[i])
                {
                    renderer.color = Color.red;
                    selections[i] = false;
                }
                else
                {
                    selections[i] = true;
                    renderer.color = Color.yellow;
                    meetingExtraButtonLabel.text = Helpers.cs(Color.yellow, "Confirm Swap");
                }
            }
            else if (selectedCount == 2)
            {
                if (selections[i])
                {
                    renderer.color = Color.red;
                    selections[i] = false;
                    meetingExtraButtonLabel.text = Helpers.cs(Color.red, "Confirm Swap");
                }
            }
        }

        static void swapperConfirm(MeetingHud __instance)
        {
            __instance.playerStates[0]
                .Cancel(); // This will stop the underlying buttons of the template from showing up
            if (__instance.state == MeetingHud.VoteStates.Results) return;
            if (selections.Where(b => b).Count() != 2) return;
            if (Swapper.Instance.Charges <= 0 || Swapper.Instance.PlayerId1 != Byte.MaxValue) return;

            PlayerVoteArea firstPlayer = null;
            PlayerVoteArea secondPlayer = null;
            for (int A = 0; A < selections.Length; A++)
            {
                if (selections[A])
                {
                    if (firstPlayer == null)
                    {
                        firstPlayer = __instance.playerStates[A];
                    }
                    else
                    {
                        secondPlayer = __instance.playerStates[A];
                    }

                    renderers[A].color = Color.green;
                }
                else if (renderers[A] != null)
                {
                    renderers[A].color = Color.gray;
                }

                if (swapperButtonList[A] != null)
                    swapperButtonList[A].OnClick
                        .RemoveAllListeners(); // Swap buttons can't be clicked / changed anymore
            }

            if (firstPlayer != null && secondPlayer != null)
            {
                Swapper.SwapperSwap(firstPlayer.TargetPlayerId, secondPlayer.TargetPlayerId);
                meetingExtraButtonLabel.text = Helpers.cs(Color.green, "Swapping!");
                Swapper.Instance.UsedSwaps++;
                meetingExtraButtonText.text = $"Swaps: {Swapper.Instance.Charges}";
            }
        }

        static void mayorToggleVoteTwice(MeetingHud __instance)
        {
            __instance.playerStates[0]
                .Cancel(); // This will stop the underlying buttons of the template from showing up
            if (__instance.state == MeetingHud.VoteStates.Results || Mayor.Instance.Player.Data.IsDead) return;
            if (Mayor.Instance.CanChooseSingleVote)
            {
                // Only accept changes until the mayor voted
                var mayorPVA =
                    __instance.playerStates.FirstOrDefault(x => x.TargetPlayerId == Mayor.Instance.Player.PlayerId);
                if (mayorPVA != null && mayorPVA.DidVote)
                {
                    SoundEffectsManager.play("fail");
                    return;
                }
            }

            Mayor.MayorSetVoteTwice(!Mayor.Instance.VoteTwice);

            meetingExtraButtonLabel.text = Helpers.cs(Mayor.Instance.Color,
                "Double Vote: " + (Mayor.Instance.VoteTwice
                    ? Helpers.cs(Color.green, "On ")
                    : Helpers.cs(Color.red, "Off")));
        }

        public static GameObject guesserUI;
        public static PassiveButton guesserUIExitButton;
        public static byte guesserCurrentTarget;

        static void guesserOnClick(int buttonTarget, MeetingHud __instance)
        {
            if (guesserUI != null || !(__instance.state == MeetingHud.VoteStates.Voted ||
                                       __instance.state == MeetingHud.VoteStates.NotVoted)) return;
            __instance.playerStates.ToList().ForEach(x => x.gameObject.SetActive(false));

            Transform PhoneUI = UnityEngine.Object.FindObjectsOfType<Transform>()
                .FirstOrDefault(x => x.name == "PhoneUI");
            Transform container = UnityEngine.Object.Instantiate(PhoneUI, __instance.transform);
            container.transform.localPosition = new Vector3(0, 0, -5f);
            guesserUI = container.gameObject;

            int i = 0;
            var buttonTemplate = __instance.playerStates[0].transform.FindChild("votePlayerBase");
            var maskTemplate = __instance.playerStates[0].transform.FindChild("MaskArea");
            var smallButtonTemplate = __instance.playerStates[0].Buttons.transform.Find("CancelButton");
            var textTemplate = __instance.playerStates[0].NameText;

            guesserCurrentTarget = __instance.playerStates[buttonTarget].TargetPlayerId;

            Transform exitButtonParent = (new GameObject()).transform;
            exitButtonParent.SetParent(container);
            Transform exitButton = UnityEngine.Object.Instantiate(buttonTemplate.transform, exitButtonParent);
            Transform exitButtonMask = UnityEngine.Object.Instantiate(maskTemplate, exitButtonParent);
            exitButton.gameObject.GetComponent<SpriteRenderer>().sprite =
                smallButtonTemplate.GetComponent<SpriteRenderer>().sprite;
            exitButtonParent.transform.localPosition = new Vector3(2.725f, 2.1f, -5);
            exitButtonParent.transform.localScale = new Vector3(0.217f, 0.9f, 1);
            guesserUIExitButton = exitButton.GetComponent<PassiveButton>();
            guesserUIExitButton.OnClick.RemoveAllListeners();
            guesserUIExitButton.OnClick.AddListener((System.Action)(() =>
            {
                __instance.playerStates.ToList().ForEach(x =>
                {
                    x.gameObject.SetActive(true);
                    if (CachedPlayer.LocalPlayer.Data.IsDead && x.transform.FindChild("ShootButton") != null)
                        UnityEngine.Object.Destroy(x.transform.FindChild("ShootButton").gameObject);
                });
                UnityEngine.Object.Destroy(container.gameObject);
            }));

            List<Transform> buttons = new List<Transform>();
            Transform selectedButton = null;

            foreach (var roleInfo in RoleInfo.allRoleInfos)
            {
                var guesserRoleId = NiceGuesser.Instance.IsLocalPlayer ? RoleId.NiceGuesser : RoleId.EvilGuesser;
                if (roleInfo.isModifier ||
                    roleInfo.roleId == guesserRoleId ||
                    (!HandleGuesser.evilGuesserCanGuessSpy && guesserRoleId == RoleId.EvilGuesser && roleInfo.roleId == RoleId.Spy && !HandleGuesser.isGuesserGm))
                    continue; // Not guessable roles & modifier
                if (HandleGuesser.isGuesserGm &&
                    (roleInfo.roleId == RoleId.NiceGuesser || roleInfo.roleId == RoleId.EvilGuesser))
                    continue; // remove Guesser for guesser game mode
                if (HandleGuesser.isGuesserGm && CachedPlayer.LocalPlayer.PlayerControl.Data.Role.IsImpostor &&
                    !HandleGuesser.evilGuesserCanGuessSpy && roleInfo.roleId == RoleId.Spy) continue;
                // remove all roles that cannot spawn due to the settings from the ui.
                var roleData = RoleManagerSelectRolesPatch.getRoleAssignmentData();
                TheOtherRolesPlugin.Instance.Log.LogDebug($"Role: {roleInfo.name} {(byte)roleInfo.roleId}");
                TheOtherRolesPlugin.Instance.Log.LogDebug(Rpc.Serialize(roleData.neutralSettings));
                TheOtherRolesPlugin.Instance.Log.LogDebug(Rpc.Serialize(roleData.impSettings));
                TheOtherRolesPlugin.Instance.Log.LogDebug(Rpc.Serialize(roleData.crewSettings));
                if (roleData.neutralSettings.ContainsKey((byte)roleInfo.roleId) && roleData.neutralSettings[(byte)roleInfo.roleId] == 0) continue;
                if (roleData.impSettings.ContainsKey((byte)roleInfo.roleId) && roleData.impSettings[(byte)roleInfo.roleId] == 0) continue;
                if (roleData.crewSettings.ContainsKey((byte)roleInfo.roleId) && roleData.crewSettings[(byte)roleInfo.roleId] == 0) continue;
                if (new List<RoleId> { RoleId.Janitor, RoleId.Godfather, RoleId.Mafioso }.Contains(roleInfo.roleId) && CustomOptions.MafiaSpawnRate.SelectionIndex == 0) continue;
                if (roleInfo.roleId == RoleId.Sidekick && (!Jackal.Instance.JackalCanCreateSidekick || Jackal.Instance.SpawnRate == 0)) continue;
                if (roleInfo.roleId == RoleId.Deputy && (Sheriff.Instance.DeputySpawnRate == 0 || Sheriff.Instance.SpawnRate == 0)) continue;
                if (roleInfo.roleId == RoleId.Pursuer && Lawyer.Instance.SpawnRate == 0) continue;
                if (roleInfo.roleId == RoleId.Spy && roleData.impostors.Count <= 1) continue;
                if (roleInfo.roleId == RoleId.Prosecutor && (Lawyer.Instance.IsProsecutorChance == 0 || Lawyer.Instance.SpawnRate == 0)) continue;
                if (roleInfo.roleId == RoleId.Lawyer && (Lawyer.Instance.IsProsecutorChance == 100 || Lawyer.Instance.SpawnRate == 0)) continue;
                if (roleInfo.roleId == RoleId.Fallen) continue;
                if (Snitch.Instance.HasPlayer && HandleGuesser.guesserCantGuessSnitch)
                {
                    var (playerCompleted, playerTotal) = TasksHandler.taskInfo(Snitch.Instance.Player.Data);
                    int numberOfLeftTasks = playerTotal - playerCompleted;
                    if (numberOfLeftTasks <= 0 && roleInfo.roleId == RoleId.Snitch) continue;
                }

                Transform buttonParent = new GameObject().transform;
                buttonParent.SetParent(container);
                Transform button = UnityEngine.Object.Instantiate(buttonTemplate, buttonParent);
                Transform buttonMask = UnityEngine.Object.Instantiate(maskTemplate, buttonParent);
                TMPro.TextMeshPro label = UnityEngine.Object.Instantiate(textTemplate, button);
                button.GetComponent<SpriteRenderer>().sprite = FastDestroyableSingleton<HatManager>.Instance
                    .GetNamePlateById("nameplate_NoPlate")?.viewData?.viewData?.Image;
                buttons.Add(button);
                int row = i / 5, col = i % 5;
                buttonParent.localPosition = new Vector3(-3.47f + 1.75f * col, 1.5f - 0.45f * row, -5);
                buttonParent.localScale = new Vector3(0.55f, 0.55f, 1f);
                label.text = Helpers.cs(roleInfo.color, roleInfo.name);
                label.alignment = TMPro.TextAlignmentOptions.Center;
                label.transform.localPosition = new Vector3(0, 0, label.transform.localPosition.z);
                label.transform.localScale *= 1.7f;
                int copiedIndex = i;

                button.GetComponent<PassiveButton>().OnClick.RemoveAllListeners();
                if (!CachedPlayer.LocalPlayer.Data.IsDead && !Helpers
                        .playerById((byte)__instance.playerStates[buttonTarget].TargetPlayerId).Data.IsDead)
                    button.GetComponent<PassiveButton>().OnClick.AddListener((System.Action)(() =>
                    {
                        if (selectedButton != button)
                        {
                            selectedButton = button;
                            buttons.ForEach(x =>
                                x.GetComponent<SpriteRenderer>().color = x == selectedButton ? Color.red : Color.white);
                        }
                        else
                        {
                            PlayerControl focusedTarget =
                                Helpers.playerById((byte)__instance.playerStates[buttonTarget].TargetPlayerId);
                            if (!(__instance.state == MeetingHud.VoteStates.Voted ||
                                  __instance.state == MeetingHud.VoteStates.NotVoted) || focusedTarget == null ||
                                HandleGuesser.remainingShots(CachedPlayer.LocalPlayer.PlayerId) <= 0) return;

                            if (!HandleGuesser.killsThroughShield && focusedTarget == Medic.Instance.Shielded)
                            {
                                // Depending on the options, shooting the shielded player will not allow the guess, notifiy everyone about the kill attempt and close the window
                                __instance.playerStates.ToList().ForEach(x => x.gameObject.SetActive(true));
                                UnityEngine.Object.Destroy(container.gameObject);

                                Medic.ShieldedMurderAttempt();
                                SoundEffectsManager.play("fail");
                                return;
                            }

                            var mainRoleInfo = RoleInfo.getRoleInfoForPlayer(focusedTarget, false).FirstOrDefault();
                            if (mainRoleInfo == null) return;

                            PlayerControl dyingTarget = (mainRoleInfo == roleInfo)
                                ? focusedTarget
                                : CachedPlayer.LocalPlayer.PlayerControl;

                            // Reset the GUI
                            __instance.playerStates.ToList().ForEach(x => x.gameObject.SetActive(true));
                            UnityEngine.Object.Destroy(container.gameObject);
                            if (HandleGuesser.hasMultipleShotsPerMeeting &&
                                HandleGuesser.remainingShots(CachedPlayer.LocalPlayer.PlayerId) > 1 &&
                                dyingTarget != CachedPlayer.LocalPlayer.PlayerControl)
                                __instance.playerStates.ToList().ForEach(x =>
                                {
                                    if (x.TargetPlayerId == dyingTarget.PlayerId &&
                                        x.transform.FindChild("ShootButton") != null)
                                        UnityEngine.Object.Destroy(x.transform.FindChild("ShootButton").gameObject);
                                });
                            else
                                __instance.playerStates.ToList().ForEach(x =>
                                {
                                    if (x.transform.FindChild("ShootButton") != null)
                                        UnityEngine.Object.Destroy(x.transform.FindChild("ShootButton").gameObject);
                                });

                            // Shoot player and send chat info if activated
                            HandleGuesser.GuesserShoot(CachedPlayer.LocalPlayer.PlayerId, dyingTarget.PlayerId,
                                focusedTarget.PlayerId, (byte)roleInfo.roleId);
                        }
                    }));

                i++;
            }

            container.transform.localScale *= 0.75f;
        }

        [HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.Select))]
        class PlayerVoteAreaSelectPatch
        {
            static bool Prefix(MeetingHud __instance)
            {
                return !(CachedPlayer.LocalPlayer != null &&
                         HandleGuesser.isGuesser(CachedPlayer.LocalPlayer.PlayerId) && guesserUI != null);
            }
        }

        static void populateButtonsPostfix(MeetingHud __instance)
        {
            // Add Swapper Buttons
            bool addSwapperButtons = Swapper.Instance.Player != null &&
                                     CachedPlayer.LocalPlayer.PlayerControl == Swapper.Instance.Player &&
                                     !Swapper.Instance.Player.Data.IsDead;
            bool addMayorButton = Mayor.Instance.IsLocalPlayer && !Mayor.Instance.Player.Data.IsDead &&
                                  Mayor.Instance.CanChooseSingleVote;
            if (addSwapperButtons)
            {
                selections = new bool[__instance.playerStates.Length];
                renderers = new SpriteRenderer[__instance.playerStates.Length];
                swapperButtonList = new PassiveButton[__instance.playerStates.Length];

                for (int i = 0; i < __instance.playerStates.Length; i++)
                {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                    if (playerVoteArea.AmDead || (playerVoteArea.TargetPlayerId == Swapper.Instance.Player.PlayerId &&
                                                  Swapper.Instance.CanOnlySwapOthers)) continue;

                    GameObject template = playerVoteArea.Buttons.transform.Find("CancelButton").gameObject;
                    GameObject checkbox = UnityEngine.Object.Instantiate(template);
                    checkbox.transform.SetParent(playerVoteArea.transform);
                    checkbox.transform.position = template.transform.position;
                    checkbox.transform.localPosition = new Vector3(-0.95f, 0.03f, -1.3f);
                    if (HandleGuesser.isGuesserGm && HandleGuesser.isGuesser(CachedPlayer.LocalPlayer.PlayerId))
                        checkbox.transform.localPosition = new Vector3(-0.5f, 0.03f, -1.3f);
                    SpriteRenderer renderer = checkbox.GetComponent<SpriteRenderer>();
                    renderer.sprite = Swapper.CheckSprite;
                    renderer.color = Color.red;

                    if (Swapper.Instance.Charges <= 0) renderer.color = Color.gray;

                    PassiveButton button = checkbox.GetComponent<PassiveButton>();
                    swapperButtonList[i] = button;
                    button.OnClick.RemoveAllListeners();
                    int copiedIndex = i;
                    button.OnClick.AddListener((System.Action)(() => swapperOnClick(copiedIndex, __instance)));

                    selections[i] = false;
                    renderers[i] = renderer;
                }
            }

            // Add meeting extra button, i.e. Swapper Confirm Button or Mayor Toggle Double Vote Button. Swapper Button uses ExtraButtonText on the Left of the Button. (Future meeting buttons can easily be added here)
            if (addSwapperButtons || addMayorButton)
            {
                Transform meetingUI = UnityEngine.Object.FindObjectsOfType<Transform>()
                    .FirstOrDefault(x => x.name == "PhoneUI");

                var buttonTemplate = __instance.playerStates[0].transform.FindChild("votePlayerBase");
                var maskTemplate = __instance.playerStates[0].transform.FindChild("MaskArea");
                var textTemplate = __instance.playerStates[0].NameText;
                Transform meetingExtraButtonParent = (new GameObject()).transform;
                meetingExtraButtonParent.SetParent(meetingUI);
                Transform meetingExtraButton = UnityEngine.Object.Instantiate(buttonTemplate, meetingExtraButtonParent);

                Transform infoTransform = __instance.playerStates[0].NameText.transform.parent.FindChild("Info");
                TMPro.TextMeshPro meetingInfo =
                    infoTransform != null ? infoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                meetingExtraButtonText =
                    UnityEngine.Object.Instantiate(__instance.playerStates[0].NameText, meetingExtraButtonParent);
                meetingExtraButtonText.text = addSwapperButtons ? $"Swaps: {Swapper.Instance.Charges}" : "";
                meetingExtraButtonText.enableWordWrapping = false;
                meetingExtraButtonText.transform.localScale = Vector3.one * 1.7f;
                meetingExtraButtonText.transform.localPosition = new Vector3(-2.5f, 0f, 0f);

                Transform meetingExtraButtonMask =
                    UnityEngine.Object.Instantiate(maskTemplate, meetingExtraButtonParent);
                meetingExtraButtonLabel = UnityEngine.Object.Instantiate(textTemplate, meetingExtraButton);
                meetingExtraButton.GetComponent<SpriteRenderer>().sprite = FastDestroyableSingleton<HatManager>.Instance
                    .GetNamePlateById("nameplate_NoPlate")?.viewData?.viewData?.Image;
                meetingExtraButtonParent.localPosition = new Vector3(0, -2.225f, -5);
                meetingExtraButtonParent.localScale = new Vector3(0.55f, 0.55f, 1f);
                meetingExtraButtonLabel.alignment = TMPro.TextAlignmentOptions.Center;
                meetingExtraButtonLabel.transform.localPosition =
                    new Vector3(0, 0, meetingExtraButtonLabel.transform.localPosition.z);
                if (addSwapperButtons)
                {
                    meetingExtraButtonLabel.transform.localScale *= 1.7f;
                    meetingExtraButtonLabel.text = Helpers.cs(Color.red, "Confirm Swap");
                }
                else if (addMayorButton)
                {
                    meetingExtraButtonLabel.transform.localScale = new Vector3(
                        meetingExtraButtonLabel.transform.localScale.x * 1.5f,
                        meetingExtraButtonLabel.transform.localScale.x * 1.7f,
                        meetingExtraButtonLabel.transform.localScale.x * 1.7f);
                    meetingExtraButtonLabel.text = Helpers.cs(Mayor.Instance.Color,
                        "Double Vote: " + (Mayor.Instance.VoteTwice
                            ? Helpers.cs(Color.green, "On ")
                            : Helpers.cs(Color.red, "Off")));
                }

                PassiveButton passiveButton = meetingExtraButton.GetComponent<PassiveButton>();
                passiveButton.OnClick.RemoveAllListeners();
                if (!CachedPlayer.LocalPlayer.Data.IsDead)
                {
                    if (addSwapperButtons)
                        passiveButton.OnClick.AddListener((Action)(() => swapperConfirm(__instance)));
                    else if (addMayorButton)
                        passiveButton.OnClick.AddListener((Action)(() => mayorToggleVoteTwice(__instance)));
                }

                meetingExtraButton.parent.gameObject.SetActive(false);
                __instance.StartCoroutine(Effects.Lerp(7.27f, new Action<float>((p) =>
                {
                    // Button appears delayed, so that its visible in the voting screen only!
                    if (p == 1f)
                    {
                        meetingExtraButton.parent.gameObject.SetActive(true);
                    }
                })));
            }

            //Fix visor in Meetings 
            /**
            foreach (PlayerVoteArea pva in __instance.playerStates) {
                if(pva.PlayerIcon != null && pva.PlayerIcon.VisorSlot != null){
                    pva.PlayerIcon.VisorSlot.transform.position += new Vector3(0, 0, -1f);
                }
            } */

            // Add overlay for spelled players
            if (Witch.Instance.Player != null && Witch.Instance.FutureSpelled != null)
            {
                foreach (PlayerVoteArea pva in __instance.playerStates)
                {
                    if (Witch.Instance.FutureSpelled.Any(x => x.PlayerId == pva.TargetPlayerId))
                    {
                        SpriteRenderer rend = (new GameObject()).AddComponent<SpriteRenderer>();
                        rend.transform.SetParent(pva.transform);
                        rend.gameObject.layer = pva.Megaphone.gameObject.layer;
                        rend.transform.localPosition = new Vector3(-0.5f, -0.03f, -1f);
                        rend.sprite = Witch.SpelledOverlaySprite;
                    }
                }
            }

            // Add Guesser Buttons
            bool isGuesser = HandleGuesser.isGuesser(CachedPlayer.LocalPlayer.PlayerId);
            int remainingShots = HandleGuesser.remainingShots(CachedPlayer.LocalPlayer.PlayerId);

            if (isGuesser && !CachedPlayer.LocalPlayer.Data.IsDead && remainingShots > 0)
            {
                for (int i = 0; i < __instance.playerStates.Length; i++)
                {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                    if (playerVoteArea.AmDead ||
                        playerVoteArea.TargetPlayerId == CachedPlayer.LocalPlayer.PlayerId) continue;
                    if (CachedPlayer.LocalPlayer != null &&
                        CachedPlayer.LocalPlayer.PlayerControl == Eraser.Instance.Player &&
                        Eraser.Instance.AlreadyErased.Contains(playerVoteArea.TargetPlayerId)) continue;

                    GameObject template = playerVoteArea.Buttons.transform.Find("CancelButton").gameObject;
                    GameObject targetBox = UnityEngine.Object.Instantiate(template, playerVoteArea.transform);
                    targetBox.name = "ShootButton";
                    targetBox.transform.localPosition = new Vector3(-0.95f, 0.03f, -1.3f);
                    SpriteRenderer renderer = targetBox.GetComponent<SpriteRenderer>();
                    renderer.sprite = HandleGuesser.getTargetSprite();
                    PassiveButton button = targetBox.GetComponent<PassiveButton>();
                    button.OnClick.RemoveAllListeners();
                    int copiedIndex = i;
                    button.OnClick.AddListener((System.Action)(() => guesserOnClick(copiedIndex, __instance)));
                }
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.ServerStart))]
        class MeetingServerStartPatch
        {
            static void Postfix(MeetingHud __instance)
            {
                TheOtherRolesPlugin.Logger.LogDebug("MeetingServerStartPatch postfix");
                populateButtonsPostfix(__instance);
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
        public static class MeetingHudStartPatch
        {
            public static void Postfix(MeetingHud __instance)
            {
                RandomSeed.RandomizePlayersList(__instance);
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Deserialize))]
        class MeetingDeserializePatch
        {
            static void Postfix(MeetingHud __instance, [HarmonyArgument(0)] MessageReader reader,
                [HarmonyArgument(1)] bool initialState)
            {
                // Add swapper buttons
                if (initialState)
                {
                    populateButtonsPostfix(__instance);
                }
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
        class MeetingHudUpdatePatch
        {
            static void Postfix(MeetingHud __instance)
            {
                // Deactivate skip Button if skipping on emergency meetings is disabled
                if (target == null && blockSkippingInEmergencyMeetings)
                    __instance.SkipVoteButton.gameObject.SetActive(false);

                if (__instance.state >= MeetingHud.VoteStates.Discussion && TORMapOptions.removeShieldOnFirstMeeting)
                {
                    // Remove first kill shield
                    TORMapOptions.firstKillPlayer = null;
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.StartMeeting))]
        private static class PlayerControlStartMeetingPatch
        {

            public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] GameData.PlayerInfo meetingTarget)
            {
                EventUtility.meetingStartsUpdate();
                var hudManagerInstance = FastDestroyableSingleton<HudManager>.Instance;
                if (hudManagerInstance != null)
                {
                    var roomTracker = hudManagerInstance.roomTracker;
                    var roomId = byte.MinValue;
                    if (roomTracker != null && roomTracker.LastRoom != null)
                    {
                        roomId = (byte)roomTracker.LastRoom.RoomId;
                    }

                    if (Snitch.Instance.HasPlayer && roomTracker != null)
                    {
                        CommonRpc.ShareRoom(CachedPlayer.LocalPlayer.PlayerId, roomId);
                    }
                }


                // Reset Bait list
                Bait.Instance.Active.Clear();
                // Save AntiTeleport position, if the player is able to move (i.e. not on a ladder or a gap thingy)
                if (CachedPlayer.LocalPlayer.PlayerPhysics.enabled && CachedPlayer.LocalPlayer.PlayerControl.moveable ||
                    CachedPlayer.LocalPlayer.PlayerControl.inVent
                    || HudManagerStartPatch.hackerVitalsButton.isEffectActive ||
                    HudManagerStartPatch.hackerAdminTableButton.isEffectActive ||
                    HudManagerStartPatch.securityGuardCamButton.isEffectActive
                    || Portal.isTeleporting &&
                    Portal.teleportedPlayers.Last().playerId == CachedPlayer.LocalPlayer.PlayerId)
                {
                    AntiTeleport.Instance.Position = CachedPlayer.LocalPlayer.transform.position;
                }

                // Medium meeting start time
                Medium.Instance.MeetingStartTime = DateTime.UtcNow;
                // Mini
                Mini.Instance.TimeOfMeetingStart = DateTime.UtcNow;
                Mini.Instance.AgeOnMeetingStart = Mathf.FloorToInt(Mini.Instance.GrowingProgress() * 18);
                // Reset vampire bitten
                Vampire.Instance.Bitten = null;
                // Count meetings
                if (meetingTarget == null) meetingsCount++;
                // Save the meeting target
                target = meetingTarget;


                // Add Portal info into Portalmaker Chat:
                if (Portalmaker.Instance.ShouldShowRoleInfos)
                {
                    if (Portal.teleportedPlayers.Count > 0)
                    {
                        string msg = "Portal Log:\n";
                        foreach (var entry in Portal.teleportedPlayers)
                        {
                            float timeBeforeMeeting = ((float)(DateTime.UtcNow - entry.time).TotalMilliseconds) / 1000;
                            msg += Portalmaker.Instance.LogHasTime ? $"{(int)timeBeforeMeeting}s ago: " : "";
                            msg = msg + $"{entry.name} used the teleporter\n";
                        }

                        FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(Portalmaker.Instance.Player,
                            $"{msg}");
                    }
                }

                // Add trapped Info into Trapper chat
                if (Trapper.Instance.ShouldShowRoleInfos)
                {
                    if (Trap.traps.Any(x => x.revealed))
                        FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(Trapper.Instance.Player,
                            "Trap Logs:");
                    foreach (Trap trap in Trap.traps)
                    {
                        if (!trap.revealed) continue;
                        string message = $"Trap {trap.instanceId}: \n";
                        trap.trappedPlayer = trap.trappedPlayer.OrderBy(x => Rnd.Next()).ToList();
                        foreach (PlayerControl p in trap.trappedPlayer)
                        {
                            if (Trapper.Instance.InfoTypeRole)
                                message += RoleInfo.GetRolesString(p, false, false, true) + "\n";
                            else if (Trapper.Instance.InfoTypeGoodOrEvil)
                            {
                                if (Helpers.isNeutral(p) || p.Data.Role.IsImpostor) message += "Evil Role \n";
                                else message += "Good Role \n";
                            }
                            else message += p.Data.PlayerName + "\n";
                        }

                        FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(Trapper.Instance.Player,
                            $"{message}");
                    }
                }

                // Add Snitch info
                string output = "";

                if (Snitch.Instance.ShouldShowRoleInfos)
                {
                    var (playerCompleted, playerTotal) = TasksHandler.taskInfo(Snitch.Instance.Player.Data);
                    int numberOfTasks = playerTotal - playerCompleted;
                    if (numberOfTasks == 0)
                    {
                        output = $"Bad alive roles in game: \n \n";
                        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(0.4f,
                            new Action<float>((x) =>
                            {
                                if (x == 1f)
                                {
                                    foreach (PlayerControl p in CachedPlayer.AllPlayers)
                                    {
                                        if (Snitch.Instance.InfoTargetKillingPlayers && !Helpers.isKiller(p)) continue;
                                        else if (Snitch.Instance.InfoTargetEvilPlayers && !Helpers.isEvil(p)) continue;
                                        if (!Snitch.Instance.PlayerRoomMap.ContainsKey(p.PlayerId)) continue;
                                        if (p.Data.IsDead) continue;
                                        var room = Snitch.Instance.PlayerRoomMap[p.PlayerId];
                                        var roomName = "open fields";
                                        if (room != byte.MinValue)
                                        {
                                            roomName =
                                                DestroyableSingleton<TranslationController>.Instance.GetString(
                                                    (SystemTypes)room);
                                        }

                                        output += "- " + RoleInfo.GetRolesString(p, false, false, true) +
                                                  ", was last seen " + roomName + "\n";
                                    }

                                    FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(Snitch.Instance.Player,
                                        $"{output}");
                                }
                            })));
                    }
                }

                if (CachedPlayer.LocalPlayer.Data.IsDead && output != "")
                    FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(CachedPlayer.LocalPlayer, $"{output}");

                Trapper.Instance.PlayersOnMap.Clear();
                Snitch.Instance.PlayerRoomMap.Clear();

                // Remove revealed traps
                Trap.clearRevealedTraps();

                Bomber.Instance.ClearBomb(false);

                // Reset zoomed out ghosts
                Helpers.toggleZoom(reset: true);

                // Stop all playing sounds
                SoundEffectsManager.stopAll();

                // Close In-Game Settings Display if open
                // HudManagerUpdate.CloseSettings();
            }
        }
    }
}