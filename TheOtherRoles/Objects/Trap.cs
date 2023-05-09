using System;
using System.Collections.Generic;
using System.Linq;
using TheOtherRoles.EnoFw.Roles.Crewmate;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;
using UnityEngine;

namespace TheOtherRoles.Objects {
    public class Trap {
        public static List<Trap> traps = new List<Trap>();
        public static Dictionary<byte, Trap> trapPlayerIdMap = new Dictionary<byte, Trap>();

        private static int instanceCounter = 0;
        public int instanceId = 0;
        public GameObject trap;
        public bool revealed = false;
        public bool triggerable = false;
        private int usedCount = 0;
        private int neededCount;
        public List<PlayerControl> trappedPlayer = new List<PlayerControl>();
        private Arrow arrow = new Arrow(Color.blue);

        private static Sprite trapSprite;
        public static Sprite getTrapSprite() {
            if (trapSprite) return trapSprite;
            trapSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Trapper_Trap_Ingame.png", 300f);
            return trapSprite;
        }

        public Trap(Vector3 p) {
            trap = new GameObject("Trap") { layer = 11 };
            trap.AddSubmergedComponent(SubmergedCompatibility.Classes.ElevatorMover);
            Vector3 position = new Vector3(p.x, p.y, p.z + 0.005f); // just behind player
            trap.transform.position = position;
            neededCount = Trapper.Instance.TrapNeededTriggerToReveal;

            var trapRenderer = trap.AddComponent<SpriteRenderer>();
            trapRenderer.sprite = getTrapSprite();
            trap.SetActive(false);
            if (CachedPlayer.LocalPlayer.PlayerId == Trapper.Instance.Player.PlayerId) trap.SetActive(true);
            this.instanceId = ++instanceCounter;
            traps.Add(this);
            arrow.Update(position);
            arrow.arrow.SetActive(false);
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(5, new Action<float>((x) => {
                if (x == 1f) {
                    this.triggerable = true;
                }
            })));
        }

        public static void clearTraps() {
            foreach (Trap t in traps) {
                UnityEngine.Object.Destroy(t.arrow.arrow);
                UnityEngine.Object.Destroy(t.trap); 
            }
            traps = new List<Trap>();
            trapPlayerIdMap = new Dictionary<byte, Trap>();
            instanceCounter = 0;
        }

        public static void clearRevealedTraps() {
            var trapsToClear = traps.FindAll(x => x.revealed);

            foreach (Trap t in trapsToClear) {
                traps.Remove(t);
                UnityEngine.Object.Destroy(t.trap);
            }
        }

        public static void triggerTrap(byte playerId, byte trapId) {            
            Trap t = traps.FirstOrDefault(x => x.instanceId == (int)trapId);
            PlayerControl player = Helpers.playerById(playerId);
            if (Trapper.Instance.Player == null || t == null || player == null) return;
            bool localIsTrapper = CachedPlayer.LocalPlayer.PlayerId == Trapper.Instance.Player.PlayerId;
            if (!trapPlayerIdMap.ContainsKey(playerId)) trapPlayerIdMap.Add(playerId, t);
            t.usedCount ++;
            t.triggerable = false;
            if (playerId == CachedPlayer.LocalPlayer.PlayerId || playerId == Trapper.Instance.Player.PlayerId) {
                t.trap.SetActive(true);
                SoundEffectsManager.play("trapperTrap");
            }
            player.moveable = false;
            player.NetTransform.Halt();
            Trapper.Instance.PlayersOnMap.Add(player); 
            if (localIsTrapper) t.arrow.arrow.SetActive(true);

            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Trapper.Instance.TrapDuration, new Action<float>((p) => { 
                if (p == 1f) {
                    player.moveable = true;
                    Trapper.Instance.PlayersOnMap.RemoveAll(x => x == player);
                    if (trapPlayerIdMap.ContainsKey(playerId)) trapPlayerIdMap.Remove(playerId);
                    t.arrow.arrow.SetActive(false);
                }
            })));

            if (t.usedCount == t.neededCount) {
                t.revealed = true;
            }

            t.trappedPlayer.Add(player);
            t.triggerable = true;

        }

        public static void Update() {
            if (Trapper.Instance.Player == null) return;
            CachedPlayer player = CachedPlayer.LocalPlayer;
            Vent vent = MapUtilities.CachedShipStatus.AllVents[0];
            float closestDistance = float.MaxValue;

            if (vent == null || player == null) return;
            float ud = vent.UsableDistance / 2;
            Trap target = null;
            foreach (Trap trap in traps) {
                if (trap.arrow.arrow.active) trap.arrow.Update();
                if (trap.revealed || !trap.triggerable || trap.trappedPlayer.Contains(player.PlayerControl)) continue;
                if (player.PlayerControl.inVent || !player.PlayerControl.CanMove) continue;
                float distance = Vector2.Distance(trap.trap.transform.position, player.PlayerControl.GetTruePosition());
                if (distance <= ud && distance < closestDistance) {
                    closestDistance = distance;
                    target = trap;
                }
            }
            if (target != null && player.PlayerId != Trapper.Instance.Player.PlayerId && !player.Data.IsDead) {
                Trapper.TriggerTrap(player.PlayerId,(byte)target.instanceId);
            }


            if (!player.Data.IsDead || player.PlayerId == Trapper.Instance.Player.PlayerId) return;
            foreach (Trap trap in traps) {
                if (!trap.trap.active) trap.trap.SetActive(true);
            }
        }
    }
}