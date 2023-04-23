using Hazel;
using System;
using System.Collections.Generic;
using System.Linq;
using TheOtherRoles.Customs.Roles.Impostor;
using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;
using UnityEngine;

namespace TheOtherRoles.Objects {
    public class Bomb {
        public GameObject bomb;
        public GameObject background;

        private static Sprite bombSprite;
        private static Sprite backgroundSprite;
        private static Sprite defuseSprite;
        public static bool canDefuse = false;

        public static Sprite getBombSprite() {
            if (bombSprite) return bombSprite;
            bombSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Bomb.png", 300f);
            return bombSprite;
        }
        public static Sprite getBackgroundSprite() {
            if (backgroundSprite) return backgroundSprite;
            backgroundSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.BombBackground.png", 110f / Singleton<Bomber>.Instance.BombHearRange);
            return backgroundSprite;
        }

        public static Sprite getDefuseSprite() {
            if (defuseSprite) return defuseSprite;
            defuseSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Bomb_Button_Defuse.png", 115f);
            return defuseSprite;
        }

        public Bomb(Vector2 p) {
            bomb = new GameObject("Bomb") { layer = 11 };
            bomb.AddSubmergedComponent(SubmergedCompatibility.Classes.ElevatorMover);
            var position = new Vector3(p.x, p.y, p.y / 1000 + 0.001f); // just behind player
            bomb.transform.position = position;

            background = new GameObject("Background") { layer = 11 };
            background.transform.SetParent(bomb.transform);
            background.transform.localPosition = new Vector3(0, 0, -1f); // before player
            background.transform.position = position;

            var bombRenderer = bomb.AddComponent<SpriteRenderer>();
            bombRenderer.sprite = getBombSprite();
            var backgroundRenderer = background.AddComponent<SpriteRenderer>();
            backgroundRenderer.sprite = getBackgroundSprite();

            bomb.SetActive(false);
            background.SetActive(false);
            if (CachedPlayer.LocalPlayer.PlayerControl == Singleton<Bomber>.Instance.Player) {
                bomb.SetActive(true);
            }
            Singleton<Bomber>.Instance.Bomb = this;
            var c = Color.white;
            var g = Color.red;
            backgroundRenderer.color = Color.white;
            Singleton<Bomber>.Instance.IsBombActive = false;

            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Singleton<Bomber>.Instance.BombActivationTime, new Action<float>((x) => {
                if (x == 1f && this != null) {
                    bomb.SetActive(true);
                    background.SetActive(true);
                    SoundEffectsManager.playAtPosition("bombFuseBurning", p, Singleton<Bomber>.Instance.BombDestructionTime, Singleton<Bomber>.Instance.BombHearRange, true);
                    Singleton<Bomber>.Instance.IsBombActive = true;

                    FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Singleton<Bomber>.Instance.BombDestructionTime, new Action<float>((x) => { // can you feel the pain?
                        Color combinedColor = Mathf.Clamp01(x) * g + Mathf.Clamp01(1 - x) * c;
                        if (backgroundRenderer) backgroundRenderer.color = combinedColor;
                        if (x == 1f && this != null) {
                            explode(this);
                        }
                    })));
                }
            })));

        }
        public static void explode(Bomb b) {
            if (b == null) return;
            if (Singleton<Bomber>.Instance.Player != null) {
                var position = b.bomb.transform.position;
                var distance = Vector2.Distance(position, CachedPlayer.LocalPlayer.transform.position);  // every player only checks that for their own client (desynct with positions sucks)
                if (distance < Singleton<Bomber>.Instance.BombDestructionRange && !CachedPlayer.LocalPlayer.Data.IsDead) {
                    Helpers.checkMurderAttemptAndKill(Singleton<Bomber>.Instance.Player, CachedPlayer.LocalPlayer.PlayerControl, false, false, true, true);
                }
                SoundEffectsManager.playAtPosition("bombExplosion", position, range: Singleton<Bomber>.Instance.BombHearRange) ;
            }
            Singleton<Bomber>.Instance.ClearBomb();
            canDefuse = false;
            Singleton<Bomber>.Instance.IsBombActive = false;
        }

        public static void update() {
            if (Singleton<Bomber>.Instance.Bomb == null || !Singleton<Bomber>.Instance.IsBombActive) {
                canDefuse = false;
                return;
            }
            Singleton<Bomber>.Instance.Bomb.background.transform.Rotate(Vector3.forward * 50 * Time.fixedDeltaTime);

            if (Vector2.Distance(CachedPlayer.LocalPlayer.PlayerControl.GetTruePosition(), Singleton<Bomber>.Instance.Bomb.bomb.transform.position) > 1f) canDefuse = false;
            else canDefuse = true;
        }

        public static void clearBackgroundSprite() {
            backgroundSprite = null;
        }
    }
}