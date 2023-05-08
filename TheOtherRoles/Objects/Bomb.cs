using System;
using TheOtherRoles.EnoFw.Roles.Impostor;
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
            backgroundSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.BombBackground.png", 110f / Bomber.Instance.BombHearRange);
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
            Vector3 position = new Vector3(p.x, p.y, p.y / 1000 + 0.001f); // just behind player
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
            if (CachedPlayer.LocalPlayer.PlayerControl == Bomber.Instance.Player) {
                bomb.SetActive(true);
            }
            Bomber.Instance.Bomb = this;
            Color c = Color.white;
            Color g = Color.red;
            backgroundRenderer.color = Color.white;
            Bomber.Instance.IsActive = false;

            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Bomber.Instance.BombActivationTime, new Action<float>((x) => {
                if (x == 1f && this != null) {
                    bomb.SetActive(true);
                    background.SetActive(true);
                    SoundEffectsManager.playAtPosition("bombFuseBurning", p, Bomber.Instance.BombDestructionTime, Bomber.Instance.BombHearRange, true);
                    Bomber.Instance.IsActive = true;

                    FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Bomber.Instance.BombDestructionTime, new Action<float>((x) => { // can you feel the pain?
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
            if (Bomber.Instance.Player != null) {
                var position = b.bomb.transform.position;
                var distance = Vector2.Distance(position, CachedPlayer.LocalPlayer.transform.position);  // every player only checks that for their own client (desynct with positions sucks)
                if (distance < Bomber.Instance.BombDestructionRange && !CachedPlayer.LocalPlayer.Data.IsDead) {
                    Helpers.checkMurderAttemptAndKill(Bomber.Instance.Player, CachedPlayer.LocalPlayer.PlayerControl, false, false, true, true);
                }
                SoundEffectsManager.playAtPosition("bombExplosion", position, range: Bomber.Instance.BombHearRange) ;
            }
            Bomber.Instance.ClearBomb();
            canDefuse = false;
            Bomber.Instance.IsActive = false;
        }

        public static void update() {
            if (Bomber.Instance.Bomb == null || !Bomber.Instance.IsActive) {
                canDefuse = false;
                return;
            }
            Bomber.Instance.Bomb.background.transform.Rotate(Vector3.forward * 50 * Time.fixedDeltaTime);

            if (Vector2.Distance(CachedPlayer.LocalPlayer.PlayerControl.GetTruePosition(), Bomber.Instance.Bomb.bomb.transform.position) > 1f) canDefuse = false;
            else canDefuse = true;
        }

        public static void clearBackgroundSprite() {
            backgroundSprite = null;
        }
    }
}