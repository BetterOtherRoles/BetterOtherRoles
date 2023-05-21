using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using BetterOtherRoles.EnoFw.Roles.Crewmate;
using BetterOtherRoles.Objects;
using BetterOtherRoles.Players;
using BetterOtherRoles.Utilities;
using UnityEngine;


namespace BetterOtherRoles.Patches;

[HarmonyPatch(typeof(MapBehaviour))]
class MapBehaviourPatch {
	public static Dictionary<PlayerControl, SpriteRenderer> herePoints = new Dictionary<PlayerControl, SpriteRenderer>();

	[HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.FixedUpdate))]
	static void Postfix(MapBehaviour __instance) {
		if (Trapper.Instance.Player != null && CachedPlayer.LocalPlayer.PlayerId == Trapper.Instance.Player.PlayerId) {
			foreach (PlayerControl player in Trapper.Instance.PlayersOnMap) {
				if (herePoints.ContainsKey(player)) continue;
				Vector3 v = Trap.trapPlayerIdMap[player.PlayerId].trap.transform.position;
				v /= MapUtilities.CachedShipStatus.MapScale;
				v.x *= Mathf.Sign(MapUtilities.CachedShipStatus.transform.localScale.x);
				v.z = -1f;
				var herePoint = UnityEngine.Object.Instantiate(__instance.HerePoint, __instance.HerePoint.transform.parent, true);
				herePoint.transform.localPosition = v;
				herePoint.enabled = true;
				int colorId = player.CurrentOutfit.ColorId;
				if (Trapper.Instance.AnonymousMap) player.CurrentOutfit.ColorId = 6;
				player.SetPlayerMaterialColors(herePoint);
				player.CurrentOutfit.ColorId = colorId;
				herePoints.Add(player, herePoint);
			}
			foreach (var s in herePoints.Where(x => !Trapper.Instance.PlayersOnMap.Contains(x.Key)).ToList()) {
				UnityEngine.Object.Destroy(s.Value);
				herePoints.Remove(s.Key);
			}
		} else if (Snitch.Instance.Player != null && CachedPlayer.LocalPlayer.PlayerId == Snitch.Instance.Player.PlayerId && !Snitch.Instance.Player.Data.IsDead && Snitch.Instance.InfoModeChat) {
			var (playerCompleted, playerTotal) = TasksHandler.taskInfo(Snitch.Instance.Player.Data);
			int numberOfTasks = playerTotal - playerCompleted;

			if (numberOfTasks == 0) {
				if (MeetingHud.Instance == null) {
					foreach (PlayerControl player in CachedPlayer.AllPlayers) {
						if (Snitch.Instance.InfoTargetEvilPlayers && !Helpers.isEvil(player)) continue;
						else if (Snitch.Instance.InfoTargetKillingPlayers && !Helpers.isKiller(player)) continue;
						if (player.Data.IsDead) continue;
						Vector3 v = player.transform.position;
						v /= MapUtilities.CachedShipStatus.MapScale;
						v.x *= Mathf.Sign(MapUtilities.CachedShipStatus.transform.localScale.x);
						v.z = -1f;
						if (herePoints.ContainsKey(player)) {
							herePoints[player].transform.localPosition = v;
							continue;
						}
						var herePoint = UnityEngine.Object.Instantiate(__instance.HerePoint, __instance.HerePoint.transform.parent, true);
						herePoint.transform.localPosition = v;
						herePoint.enabled = true;
						int colorId = player.CurrentOutfit.ColorId;
						player.CurrentOutfit.ColorId = 6;
						player.SetPlayerMaterialColors(herePoint);
						player.CurrentOutfit.ColorId = colorId;
						herePoints.Add(player, herePoint);
					}
				} else {
					foreach (var s in herePoints) {
						UnityEngine.Object.Destroy(s.Value);
						herePoints.Remove(s.Key);
					}
				}
			}
		}
		// HudManagerUpdate.CloseSettings();
	}
}