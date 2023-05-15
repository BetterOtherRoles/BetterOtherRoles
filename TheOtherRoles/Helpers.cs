using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using System.Linq;
using TheOtherRoles.Modules;
using HarmonyLib;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;
using TheOtherRoles.CustomGameModes;
using Reactor.Utilities.Extensions;
using TheOtherRoles.EnoFw.Kernel;
using TheOtherRoles.EnoFw.Modules;
using TheOtherRoles.EnoFw.Roles.Crewmate;
using TheOtherRoles.EnoFw.Roles.Impostor;
using TheOtherRoles.EnoFw.Roles.Modifiers;
using TheOtherRoles.EnoFw.Roles.Neutral;

namespace TheOtherRoles
{
    public enum MurderAttemptResult
    {
        PerformKill,
        SuppressKill,
        BlankKill,
    }

    public enum CustomGamemodes
    {
        Classic,
        Guesser,
        HideNSeek
    }

    public static class Direction
    {
        public static Vector2 up = Vector2.up;
        public static Vector2 down = Vector2.down;
        public static Vector2 left = Vector2.left;
        public static Vector2 right = Vector2.right;
        public static Vector2 upleft = new Vector2(-0.70710677f, 0.70710677f);
        public static Vector2 upright = new Vector2(0.70710677f, 0.70710677f);
        public static Vector2 downleft = new Vector2(-0.70710677f, -0.70710677f);
        public static Vector2 downright = new Vector2(0.70710677f, -0.70710677f);
    }

    public static class Helpers
    {
        public static readonly Dictionary<string, Sprite> CachedSprites = new();


        public static PlayerControl setTarget(bool onlyCrewmates = false, bool targetPlayersInVents = false,
            List<PlayerControl> untargetablePlayers = null, PlayerControl targetingPlayer = null)
        {
            PlayerControl result = null;
            float num = AmongUs.GameOptions.GameOptionsData.KillDistances[
                Mathf.Clamp(GameOptionsManager.Instance.currentNormalGameOptions.KillDistance, 0, 2)];
            if (!MapUtilities.CachedShipStatus) return result;
            if (targetingPlayer == null) targetingPlayer = CachedPlayer.LocalPlayer.PlayerControl;
            if (targetingPlayer.Data.IsDead) return result;

            Vector2 truePosition = targetingPlayer.GetTruePosition();
            foreach (var playerInfo in GameData.Instance.AllPlayers.GetFastEnumerator())
            {
                if (!playerInfo.Disconnected && playerInfo.PlayerId != targetingPlayer.PlayerId && !playerInfo.IsDead &&
                    (!onlyCrewmates || !playerInfo.Role.IsImpostor))
                {
                    PlayerControl playerObject = playerInfo.Object;
                    if (untargetablePlayers != null && untargetablePlayers.Any(x => x == playerObject))
                    {
                        // if that player is not targetable: skip check
                        continue;
                    }

                    if (playerObject && (!playerObject.inVent || targetPlayersInVents))
                    {
                        Vector2 vector = playerObject.GetTruePosition() - truePosition;
                        float magnitude = vector.magnitude;
                        if (magnitude <= num && !PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized,
                                magnitude, Constants.ShipAndObjectsMask))
                        {
                            result = playerObject;
                            num = magnitude;
                        }
                    }
                }
            }

            return result;
        }


        public static DeadBody setDeadTarget(float maxDistance = 0f, PlayerControl targetingPlayer = null)
        {
            DeadBody result = null;
            float closestDistance = float.MaxValue;

            if (!MapUtilities.CachedShipStatus) return null;

            if (targetingPlayer == null) targetingPlayer = CachedPlayer.LocalPlayer.PlayerControl;
            if (targetingPlayer.Data.IsDead) return null;

            maxDistance = maxDistance == 0f ? 1f : maxDistance + 0.1f;

            Vector2 truePosition = targetingPlayer.GetTruePosition() - new Vector2(-0.2f, -0.22f);

            bool flag = GameOptionsManager.Instance.currentNormalGameOptions.GhostsDoTasks
                        && (!AmongUsClient.Instance || !AmongUsClient.Instance.IsGameOver);

            Collider2D[] allocs = Physics2D.OverlapCircleAll(truePosition, maxDistance,
                LayerMask.GetMask("Players", "Ghost"));


            foreach (Collider2D collider2D in allocs)
            {
                if (!flag || collider2D.tag != "DeadBody") continue;
                DeadBody component = collider2D.GetComponent<DeadBody>();

                if (!(Vector2.Distance(truePosition, component.TruePosition) <=
                      maxDistance)) continue;

                float distance = Vector2.Distance(truePosition, component.TruePosition);

                if (!(distance < closestDistance)) continue;

                result = component;
                closestDistance = distance;
            }

            if (result && Undertaker.Instance.Player == targetingPlayer)
                Helpers.setDeadPlayerOutline(result, Undertaker.Instance.Color);

            return result;
        }

        public static void setPlayerOutline(PlayerControl target, Color color)
        {
            if (target == null || target.cosmetics?.currentBodySprite?.BodySprite == null) return;

            color = color.SetAlpha(Chameleon.Visibility(target.PlayerId));

            target.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", 1f);
            target.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", color);
        }

        public static void setDeadPlayerOutline(DeadBody deadTarget, Color? color)
        {
            if (deadTarget == null || deadTarget.bodyRenderers[0] == null) return;
            deadTarget.bodyRenderers[0].material.SetFloat("_Outline", color == null ? 0f : 1f);
            if (color != null) deadTarget.bodyRenderers[0].material.SetColor("_OutlineColor", color.Value);
        }

        public static Sprite loadSpriteFromResources(string path, float pixelsPerUnit)
        {
            try
            {
                if (CachedSprites.TryGetValue(path + pixelsPerUnit, out var sprite)) return sprite;
                Texture2D texture = loadTextureFromResources(path);
                sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f),
                    pixelsPerUnit);
                sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
                return CachedSprites[path + pixelsPerUnit] = sprite;
            }
            catch
            {
                TheOtherRolesPlugin.Logger.LogDebug("Error loading sprite from path: " + path);
            }

            return null;
        }

        public static unsafe Texture2D loadTextureFromResources(string path)
        {
            try
            {
                Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream stream = assembly.GetManifestResourceStream(path);
                var length = stream.Length;
                var byteTexture = new Il2CppStructArray<byte>(length);
                stream.Read(new Span<byte>(IntPtr.Add(byteTexture.Pointer, IntPtr.Size * 4).ToPointer(), (int)length));
                if (path.Contains("HorseHats"))
                {
                    byteTexture = new Il2CppStructArray<byte>(byteTexture.Reverse().ToArray());
                }

                ImageConversion.LoadImage(texture, byteTexture, false);
                return texture;
            }
            catch
            {
                TheOtherRolesPlugin.Logger.LogDebug("Error loading texture from resources: " + path);
            }

            return null;
        }

        public static Texture2D loadTextureFromDisk(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                    var byteTexture = Il2CppSystem.IO.File.ReadAllBytes(path);
                    ImageConversion.LoadImage(texture, byteTexture, false);
                    return texture;
                }
            }
            catch
            {
                TheOtherRolesPlugin.Logger.LogError("Error loading texture from disk: " + path);
            }

            return null;
        }

        public static AudioClip loadAudioClipFromResources(string path, string clipName = "UNNAMED_TOR_AUDIO_CLIP")
        {
            // must be "raw (headerless) 2-channel signed 32 bit pcm (le)" (can e.g. use Audacity® to export)
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream stream = assembly.GetManifestResourceStream(path);
                var byteAudio = new byte[stream.Length];
                _ = stream.Read(byteAudio, 0, (int)stream.Length);
                float[] samples = new float[byteAudio.Length / 4]; // 4 bytes per sample
                int offset;
                for (int i = 0; i < samples.Length; i++)
                {
                    offset = i * 4;
                    samples[i] = (float)BitConverter.ToInt32(byteAudio, offset) / Int32.MaxValue;
                }

                int channels = 2;
                int sampleRate = 48000;
                AudioClip audioClip = AudioClip.Create(clipName, samples.Length / 2, channels, sampleRate, false);
                audioClip.SetData(samples, 0);
                return audioClip;
            }
            catch
            {
                TheOtherRolesPlugin.Logger.LogDebug("Error loading AudioClip from resources: " + path);
            }

            return null;

            /* Usage example:
            AudioClip exampleClip = Helpers.loadAudioClipFromResources("TheOtherRoles.Resources.exampleClip.raw");
            if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(exampleClip, false, 0.8f);
            */
        }

        public static PlayerControl playerById(byte id)
        {
            foreach (PlayerControl player in CachedPlayer.AllPlayers)
                if (player.PlayerId == id)
                    return player;
            return null;
        }

        public static Dictionary<byte, PlayerControl> allPlayersById()
        {
            Dictionary<byte, PlayerControl> res = new Dictionary<byte, PlayerControl>();
            foreach (PlayerControl player in CachedPlayer.AllPlayers)
                res.Add(player.PlayerId, player);
            return res;
        }

        public static void handleVampireBiteOnBodyReport()
        {
            // Murder the bitten player and reset bitten (regardless whether the kill was successful or not)
            Helpers.checkMurderAttemptAndKill(Vampire.Instance.Player, Vampire.Instance.Bitten, true, false);
            Vampire.VampireSetBitten(byte.MaxValue, true);
        }

        public static void handleUndertakerDropOnBodyReport()
        {
            if (Undertaker.Instance.Player == null) return;
            var position = Undertaker.Instance.DraggedBody != null
                ? Undertaker.Instance.DraggedBody.transform.position
                : Vector3.zero;
            Undertaker.DropBody(position.x, position.y, position.z);
        }

        public static void handleWhispererKillOnBodyReport()
        {
            if (Whisperer.Instance.WhisperVictimToKill != null &&
                Whisperer.Instance.WhisperVictimToKill != Medic.Instance.Shielded &&
                (!TORMapOptions.shieldFirstKill ||
                 Whisperer.Instance.WhisperVictimToKill != TORMapOptions.firstKillPlayer))
                Helpers.checkMurderAttemptAndKill(Whisperer.Instance.Player, Whisperer.Instance.WhisperVictimToKill,
                    true, false);
            else
                Helpers.checkMurderAttemptAndKill(Whisperer.Instance.Player, Whisperer.Instance.WhisperVictim, true,
                    false);

            // & reset anyway.

            Whisperer.Instance.CurrentTarget = null;
            Whisperer.Instance.WhisperVictim = null;
            Whisperer.Instance.WhisperVictimTarget = null;
            Whisperer.Instance.WhisperVictimToKill = null;

            HudManagerStartPatch.whispererKillButton.Timer = HudManagerStartPatch.whispererKillButton.MaxTimer;
        }

        public static void refreshRoleDescription(PlayerControl player)
        {
            List<RoleInfo> infos = RoleInfo.getRoleInfoForPlayer(player);
            List<string> taskTexts = new(infos.Count);

            foreach (var roleInfo in infos)
            {
                taskTexts.Add(getRoleString(roleInfo));
            }

            var toRemove = new List<PlayerTask>();
            foreach (PlayerTask t in player.myTasks.GetFastEnumerator())
            {
                var textTask = t.TryCast<ImportantTextTask>();
                if (textTask == null) continue;

                var currentText = textTask.Text;

                if (taskTexts.Contains(currentText))
                    taskTexts.Remove(
                        currentText); // TextTask for this RoleInfo does not have to be added, as it already exists
                else toRemove.Add(t); // TextTask does not have a corresponding RoleInfo and will hence be deleted
            }

            foreach (PlayerTask t in toRemove)
            {
                t.OnRemove();
                player.myTasks.Remove(t);
                UnityEngine.Object.Destroy(t.gameObject);
            }

            // Add TextTask for remaining RoleInfos
            foreach (string title in taskTexts)
            {
                var task = new GameObject("RoleTask").AddComponent<ImportantTextTask>();
                task.transform.SetParent(player.transform, false);
                task.Text = title;
                player.myTasks.Insert(0, task);
            }
        }

        internal static string getRoleString(RoleInfo roleInfo)
        {
            if (roleInfo.name == "Jackal")
            {
                var getSidekickText = Jackal.Instance.CanCreateSidekick ? " and recruit a Sidekick" : "";
                return cs(roleInfo.color, $"{roleInfo.name}: Kill everyone{getSidekickText}");
            }

            if (roleInfo.name == "Invert")
            {
                return cs(roleInfo.color, $"{roleInfo.name}: {roleInfo.shortDescription} ({Invert.Instance.Meetings})");
            }

            return cs(roleInfo.color, $"{roleInfo.name}: {roleInfo.shortDescription}");
        }

        public static bool isLighterColor(int colorId)
        {
            return CustomColors.LighterColors.Contains(colorId);
        }

        public static bool isCustomServer()
        {
            if (FastDestroyableSingleton<ServerManager>.Instance == null) return false;
            StringNames n = FastDestroyableSingleton<ServerManager>.Instance.CurrentRegion.TranslateName;
            return n != StringNames.ServerNA && n != StringNames.ServerEU && n != StringNames.ServerAS;
        }

        public static bool hasFakeTasks(this PlayerControl player)
        {
            return (player == Jester.Instance.Player || player == Jackal.Instance.Player ||
                    player == Sidekick.Instance.Player ||
                    player == Arsonist.Instance.Player || player == Vulture.Instance.Player ||
                    Jackal.Instance.FormerJackals.Any(x => x == player));
        }

        public static bool canBeErased(this PlayerControl player)
        {
            return (player != Jackal.Instance.Player && player != Sidekick.Instance.Player &&
                    !Jackal.Instance.FormerJackals.Any(x => x == player));
        }

        public static bool shouldShowGhostInfo()
        {
            return CustomGuid.IsDevMode || (CachedPlayer.LocalPlayer.PlayerControl != null &&
                                            CachedPlayer.LocalPlayer.PlayerControl.Data.IsDead &&
                                            TORMapOptions.ghostsSeeInformation);
        }

        public static void clearAllTasks(this PlayerControl player)
        {
            if (player == null) return;
            foreach (var playerTask in player.myTasks.GetFastEnumerator())
            {
                playerTask.OnRemove();
                UnityEngine.Object.Destroy(playerTask.gameObject);
            }

            player.myTasks.Clear();

            if (player.Data != null && player.Data.Tasks != null)
                player.Data.Tasks.Clear();
        }

        public static void setSemiTransparent(this PoolablePlayer player, bool value)
        {
            float alpha = value ? 0.25f : 1f;
            foreach (SpriteRenderer r in player.gameObject.GetComponentsInChildren<SpriteRenderer>())
                r.color = new Color(r.color.r, r.color.g, r.color.b, alpha);
            player.cosmetics.nameText.color = new Color(player.cosmetics.nameText.color.r,
                player.cosmetics.nameText.color.g, player.cosmetics.nameText.color.b, alpha);
        }

        public static string GetString(this TranslationController t, StringNames key,
            params Il2CppSystem.Object[] parts)
        {
            return t.GetString(key, parts);
        }

        public static string cs(Color c, string s)
        {
            return string.Format("<color=#{0:X2}{1:X2}{2:X2}{3:X2}>{4}</color>", ToByte(c.r), ToByte(c.g), ToByte(c.b),
                ToByte(c.a), s);
        }

        public static int lineCount(string text)
        {
            return text.Count(c => c == '\n');
        }

        private static byte ToByte(float f)
        {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }

        public static KeyValuePair<byte, int> MaxPair(this Dictionary<byte, int> self, out bool tie)
        {
            tie = true;
            KeyValuePair<byte, int> result = new KeyValuePair<byte, int>(byte.MaxValue, int.MinValue);
            foreach (KeyValuePair<byte, int> keyValuePair in self)
            {
                if (keyValuePair.Value > result.Value)
                {
                    result = keyValuePair;
                    tie = false;
                }
                else if (keyValuePair.Value == result.Value)
                {
                    tie = true;
                }
            }

            return result;
        }

        public static bool hidePlayerName(PlayerControl source, PlayerControl target)
        {
            if (Camouflager.Instance.CamouflageTimer > 0f) return true; // No names are visible
            if (Patches.SurveillanceMinigamePatch.nightVisionIsActive) return true;
            else if (Ninja.Instance.IsInvisible && Ninja.Instance.Player == target) return true;
            else if (!TORMapOptions.hidePlayerNames) return false; // All names are visible
            else if (source == null || target == null) return true;
            else if (source == target) return false; // Player sees his own name
            else if (source.Data.Role.IsImpostor && (target.Data.Role.IsImpostor || target == Spy.Instance.Player ||
                                                     target == Sidekick.Instance.Player &&
                                                     Sidekick.Instance.WasTeamRed ||
                                                     target == Jackal.Instance.Player && Jackal.Instance.WasTeamRed))
                return false; // Members of team Impostors see the names of Impostors/Spies
            else if ((source == Lovers.Instance.Lover1 || source == Lovers.Instance.Lover2) &&
                     (target == Lovers.Instance.Lover1 || target == Lovers.Instance.Lover2))
                return false; // Members of team Lovers see the names of each other
            else if ((source == Jackal.Instance.Player || source == Sidekick.Instance.Player) &&
                     (target == Jackal.Instance.Player ||
                      target == Sidekick.Instance.Player ||
                      target == Jackal.Instance.FakeSidekick))
                return false; // Members of team Jackal see the names of each other
            else if (Deputy.Instance.KnowsSheriff &&
                     (source == Sheriff.Instance.Player || source == Deputy.Instance.Player) &&
                     (target == Sheriff.Instance.Player || target == Deputy.Instance.Player))
                return false; // Sheriff & Deputy see the names of each other
            return true;
        }

        public static void setDefaultLook(this PlayerControl target, bool enforceNightVisionUpdate = true)
        {
            target.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId,
                target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId,
                enforceNightVisionUpdate);
        }

        public static void setLook(this PlayerControl target, String playerName, int colorId, string hatId,
            string visorId, string skinId, string petId, bool enforceNightVisionUpdate = true)
        {
            target.RawSetColor(colorId);
            target.RawSetVisor(visorId, colorId);
            target.RawSetHat(hatId, colorId);
            target.RawSetName(hidePlayerName(CachedPlayer.LocalPlayer.PlayerControl, target) ? "" : playerName);

            SkinViewData nextSkin = FastDestroyableSingleton<HatManager>.Instance.GetSkinById(skinId).viewData.viewData;
            PlayerPhysics playerPhysics = target.MyPhysics;
            AnimationClip clip = null;
            var spriteAnim = playerPhysics.myPlayer.cosmetics.skin.animator;
            var currentPhysicsAnim = playerPhysics.Animations.Animator.GetCurrentAnimation();


            if (currentPhysicsAnim == playerPhysics.Animations.group.RunAnim) clip = nextSkin.RunAnim;
            else if (currentPhysicsAnim == playerPhysics.Animations.group.SpawnAnim) clip = nextSkin.SpawnAnim;
            else if (currentPhysicsAnim == playerPhysics.Animations.group.EnterVentAnim) clip = nextSkin.EnterVentAnim;
            else if (currentPhysicsAnim == playerPhysics.Animations.group.ExitVentAnim) clip = nextSkin.ExitVentAnim;
            else if (currentPhysicsAnim == playerPhysics.Animations.group.IdleAnim) clip = nextSkin.IdleAnim;
            else clip = nextSkin.IdleAnim;
            float progress = playerPhysics.Animations.Animator.m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            playerPhysics.myPlayer.cosmetics.skin.skin = nextSkin;
            playerPhysics.myPlayer.cosmetics.skin.UpdateMaterial();

            spriteAnim.Play(clip, 1f);
            spriteAnim.m_animator.Play("a", 0, progress % 1);
            spriteAnim.m_animator.Update(0f);

            if (target.cosmetics.currentPet) UnityEngine.Object.Destroy(target.cosmetics.currentPet.gameObject);
            target.cosmetics.currentPet =
                UnityEngine.Object.Instantiate<PetBehaviour>(FastDestroyableSingleton<HatManager>.Instance
                    .GetPetById(petId).viewData.viewData);
            target.cosmetics.currentPet.transform.position = target.transform.position;
            target.cosmetics.currentPet.Source = target;
            target.cosmetics.currentPet.Visible = target.Visible;
            target.SetPlayerMaterialColors(target.cosmetics.currentPet.rend);

            if (enforceNightVisionUpdate) Patches.SurveillanceMinigamePatch.enforceNightVision(target);
            Chameleon.update(); // so that morphling and camo wont make the chameleons visible
        }

        public static void showFlash(Color color, float duration = 1f, string message = "")
        {
            if (FastDestroyableSingleton<HudManager>.Instance == null ||
                FastDestroyableSingleton<HudManager>.Instance.FullScreen == null) return;
            FastDestroyableSingleton<HudManager>.Instance.FullScreen.gameObject.SetActive(true);
            FastDestroyableSingleton<HudManager>.Instance.FullScreen.enabled = true;
            // Message Text
            TMPro.TextMeshPro messageText = GameObject.Instantiate(
                FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText,
                FastDestroyableSingleton<HudManager>.Instance.transform);
            messageText.text = message;
            messageText.enableWordWrapping = false;
            messageText.transform.localScale = Vector3.one * 0.5f;
            messageText.transform.localPosition += new Vector3(0f, 2f, -69f);
            messageText.gameObject.SetActive(true);
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(duration, new Action<float>((p) =>
            {
                var renderer = FastDestroyableSingleton<HudManager>.Instance.FullScreen;

                if (p < 0.5)
                {
                    if (renderer != null)
                        renderer.color = new Color(color.r, color.g, color.b, Mathf.Clamp01(p * 2 * 0.75f));
                }
                else
                {
                    if (renderer != null)
                        renderer.color = new Color(color.r, color.g, color.b, Mathf.Clamp01((1 - p) * 2 * 0.75f));
                }

                if (p == 1f && renderer != null) renderer.enabled = false;
                if (p == 1f) messageText.gameObject.Destroy();
            })));
        }

        public static bool roleCanUseVents(this PlayerControl player)
        {
            bool roleCouldUse = false;
            if (Engineer.Instance.HasPlayer && Engineer.Instance.Player == player)
                roleCouldUse = true;
            else if (Jackal.Instance.CanUseVents && Jackal.Instance.Player != null && Jackal.Instance.Player == player)
                roleCouldUse = true;
            else if (Sidekick.Instance.CanUseVents && Sidekick.Instance.Player != null &&
                     Sidekick.Instance.Player == player)
                roleCouldUse = true;
            else if (Spy.Instance.CanEnterVents && Spy.Instance.Player != null && Spy.Instance.Player == player)
                roleCouldUse = true;
            else if (Vulture.Instance.CanUseVents && Vulture.Instance.Player != null &&
                     Vulture.Instance.Player == player)
                roleCouldUse = true;
            else if (Thief.Instance.CanUseVents && Thief.Instance.Player != null && Thief.Instance.Player == player)
                roleCouldUse = true;
            else if (player.Data?.Role != null && player.Data.Role.CanVent)
            {
                if (Janitor.Instance.Player != null &&
                    Janitor.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl)
                    roleCouldUse = false;
                else if (Mafioso.Instance.Player != null &&
                         Mafioso.Instance.Player == CachedPlayer.LocalPlayer.PlayerControl &&
                         Godfather.Instance.Player != null && !Godfather.Instance.Player.Data.IsDead)
                    roleCouldUse = false;
                else
                    roleCouldUse = true;
            }

            return roleCouldUse;
        }

        public static MurderAttemptResult checkMurderAttempt(PlayerControl killer, PlayerControl target,
            bool blockRewind = false, bool ignoreBlank = false, bool ignoreIfKillerIsDead = false,
            bool showShieldAnimation = true)
        {
            var targetRole = RoleInfo.getRoleInfoForPlayer(target, false).FirstOrDefault();

            // Modified vanilla checks
            if (AmongUsClient.Instance.IsGameOver) return MurderAttemptResult.SuppressKill;
            if (killer == null || killer.Data == null || (killer.Data.IsDead && !ignoreIfKillerIsDead) ||
                killer.Data.Disconnected)
                return MurderAttemptResult.SuppressKill; // Allow non Impostor kills compared to vanilla code
            if (target == null || target.Data == null || target.Data.IsDead || target.Data.Disconnected)
                return MurderAttemptResult.SuppressKill; // Allow killing players in vents compared to vanilla code

            // Handle first kill attempt
            if (TORMapOptions.shieldFirstKill && TORMapOptions.firstKillPlayer == target)
            {
                if (showShieldAnimation)
                {
                    MurderAttempt.ShowFailedMurderAttempt(killer.PlayerId, target.PlayerId);
                }

                return MurderAttemptResult.SuppressKill;
            }

            // Handle blank shot
            if (!ignoreBlank && Pursuer.Instance.BlankedList.Any(x => x.PlayerId == killer.PlayerId))
            {
                Pursuer.SetBlanked(killer.PlayerId, false);
                return MurderAttemptResult.BlankKill;
            }

            // Block impostor shielded kill
            if (Medic.Instance.Shielded != null && Medic.Instance.Shielded == target)
            {
                if (showShieldAnimation)
                {
                    Medic.ShieldedMurderAttempt();
                    MurderAttempt.ShowFailedMurderAttempt(killer.PlayerId, target.PlayerId);
                    SoundEffectsManager.play("fail");
                }
                return MurderAttemptResult.SuppressKill;
            }

            // Block impostor not fully grown mini kill
            else if (Mini.Instance.Player != null && target == Mini.Instance.Player && !Mini.Instance.IsGrownUp)
            {
                MurderAttempt.ShowFailedMurderAttempt(killer.PlayerId, target.PlayerId);
                return MurderAttemptResult.SuppressKill;
            }

            // Block Time Master with time shield kill
            else if (TimeMaster.Instance.ShieldActive && TimeMaster.Instance.Player != null &&
                     TimeMaster.Instance.Player == target)
            {
                if (!blockRewind)
                {
                    // Only rewind the attempt was not called because a meeting started
                    TimeMaster.TimeMasterRewindTime();
                }

                return MurderAttemptResult.SuppressKill;
            }

            // Thief if hit crew only kill if setting says so, but also kill the thief.
            else if (killer == Thief.Instance.Player && !target.Data.Role.IsImpostor && !new List<RoleInfo>
                         {
                             RoleInfo.jackal, Thief.Instance.CanKillSheriff ? RoleInfo.sheriff : null, RoleInfo.sidekick
                         }
                         .Contains(targetRole))
            {
                Thief.Instance.SuicideFlag = true;
                return MurderAttemptResult.SuppressKill;
            }

            // Block hunted with time shield kill
            else if (Hunted.timeshieldActive.Contains(target.PlayerId))
            {
                CommonRpc.HuntedRewindTime(target.PlayerId);

                return MurderAttemptResult.SuppressKill;
            }

            return MurderAttemptResult.PerformKill;
        }

        public static MurderAttemptResult checkMurderAttemptAndKill(PlayerControl killer, PlayerControl target,
            bool isMeetingStart = false, bool showAnimation = true, bool ignoreBlank = false,
            bool ignoreIfKillerIsDead = false)
        {
            // The local player checks for the validity of the kill and performs it afterwards (different to vanilla, where the host performs all the checks)
            // The kill attempt will be shared using a custom RPC, hence combining modded and unmodded versions is impossible

            var murder = checkMurderAttempt(killer, target, isMeetingStart, ignoreBlank, ignoreIfKillerIsDead);
            if (murder == MurderAttemptResult.PerformKill)
            {
                KernelRpc.UncheckedMurderPlayer(killer.PlayerId, target.PlayerId, showAnimation);
            }

            return murder;
        }

        public static List<PlayerControl> getKillerTeamMembers(PlayerControl player)
        {
            List<PlayerControl> team = new List<PlayerControl>();
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (player.Data.Role.IsImpostor && p.Data.Role.IsImpostor && player.PlayerId != p.PlayerId &&
                    team.All(x => x.PlayerId != p.PlayerId)) team.Add(p);
                else if (player == Jackal.Instance.Player && p == Sidekick.Instance.Player) team.Add(p);
                else if (player == Sidekick.Instance.Player && p == Jackal.Instance.Player) team.Add(p);
            }

            return team;
        }

        public static bool isNeutral(PlayerControl player)
        {
            RoleInfo roleInfo = RoleInfo.getRoleInfoForPlayer(player, false).FirstOrDefault();
            if (roleInfo != null)
                return roleInfo.isNeutral;
            return false;
        }

        public static bool isKiller(PlayerControl player)
        {
            return player.Data.Role.IsImpostor ||
                   (isNeutral(player) &&
                    player != Jester.Instance.Player &&
                    player != Arsonist.Instance.Player &&
                    player != Vulture.Instance.Player &&
                    player != Lawyer.Instance.Player &&
                    player != Pursuer.Instance.Player);
        }

        public static bool isEvil(PlayerControl player)
        {
            return player.Data.Role.IsImpostor || isNeutral(player);
        }

        public static bool zoomOutStatus = false;

        public static void toggleZoom(bool reset = false)
        {
            float orthographicSize = reset || zoomOutStatus ? 3f : 12f;

            zoomOutStatus = !zoomOutStatus && !reset;
            Camera.main.orthographicSize = orthographicSize;
            foreach (var cam in Camera.allCameras)
            {
                if (cam != null && cam.gameObject.name == "UI Camera")
                    cam.orthographicSize =
                        orthographicSize; // The UI is scaled too, else we cant click the buttons. Downside: map is super small.
            }

            if (HudManagerStartPatch.zoomOutButton != null)
            {
                HudManagerStartPatch.zoomOutButton.Sprite = zoomOutStatus
                    ? Helpers.loadSpriteFromResources("TheOtherRoles.Resources.PlusButton.png", 75f)
                    : Helpers.loadSpriteFromResources("TheOtherRoles.Resources.MinusButton.png", 150f);
                HudManagerStartPatch.zoomOutButton.PositionOffset =
                    zoomOutStatus ? new Vector3(0f, 3f, 0) : new Vector3(0.4f, 2.8f, 0);
            }

            ResolutionManager.ResolutionChanged.Invoke((float)Screen.width /
                                                       Screen
                                                           .height); // This will move button positions to the correct position.
        }


        public static bool hasImpVision(GameData.PlayerInfo player)
        {
            return player.Role.IsImpostor
                   || ((Jackal.Instance.Player != null && Jackal.Instance.Player.PlayerId == player.PlayerId ||
                        Jackal.Instance.FormerJackals.Any(x => x.PlayerId == player.PlayerId)) &&
                       Jackal.Instance.HasImpostorVision)
                   || (Sidekick.Instance.Player != null && Sidekick.Instance.Player.PlayerId == player.PlayerId &&
                       Sidekick.Instance.HasImpostorVision)
                   || (Spy.Instance.Player != null && Spy.Instance.Player.PlayerId == player.PlayerId &&
                       Spy.Instance.HasImpostorVision)
                   || (Jester.Instance.HasPlayer && Jester.Instance.Player.PlayerId == player.PlayerId &&
                       Jester.Instance.HasImpostorVision)
                   || (Thief.Instance.Player != null && Thief.Instance.Player.PlayerId == player.PlayerId &&
                       Thief.Instance.HasImpostorVision);
        }

        public static object TryCast(this Il2CppObjectBase self, Type type)
        {
            return AccessTools.Method(self.GetType(), nameof(Il2CppObjectBase.TryCast)).MakeGenericMethod(type)
                .Invoke(self, Array.Empty<object>());
        }
    }
}