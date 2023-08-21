using System;
using System.Linq;
using BetterOtherRoles.Utilities.Attributes;
using UnityEngine;
using UniverseLib.Input;

namespace BetterOtherRoles.UI;

[RegisterInIl2Cpp]
public class BetterUIBehaviour : MonoBehaviour
{
    private void Update()
    {
        if (InputManager.GetKeyDown(KeyCode.F2))
        {
            UIManager.CustomOptionsPanel?.Toggle();
        }
#if DEBUG
        if (InputManager.GetKeyDown(KeyCode.F3))
        {
            UIManager.LocalOptionsPanel?.Toggle();
        }

        if (InputManager.GetKeyDown(KeyCode.F4))
        {
            UIManager.CreditsPanel?.Toggle();
        }

        if (InputManager.GetKeyDown(KeyCode.F5))
        {
            var gameObjects = FindObjectsOfType<GameObject>();
            foreach (var go in gameObjects)
            {
                if (go.name == "Comms" || go.name == "Weapons")
                {
                    var pos = go.transform.position;
                    pos.z = 2f;
                    go.transform.position = pos;
                }
                BetterOtherRolesPlugin.Logger.LogMessage($"{go.name}: {go.transform.position} < {go.transform.parent?.name}");
            }
        }
        
        if (InputManager.GetKeyDown(KeyCode.F6))
        {
            BetterOtherRolesPlugin.Logger.LogMessage($"[MapID : {GameOptionsManager.Instance.currentNormalGameOptions.MapId}");

            var gameObjects = FindObjectsOfType<GameObject>()
                .OrderBy(go => go.name);
 
            int index = 0;
            foreach (var go in gameObjects)
            {
                BetterOtherRolesPlugin.Logger.LogMessage($"[{index}] - {go.name}");
                index++;
            }
        }
#endif
    }
}