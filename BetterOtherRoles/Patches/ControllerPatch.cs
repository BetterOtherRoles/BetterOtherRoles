using BetterOtherRoles.Modules;
using HarmonyLib;
using Rewired;
using Rewired.Data;

namespace BetterOtherRoles.Patches;

[HarmonyPatch(typeof(InputManager_Base), nameof(InputManager_Base.Awake))]
public static class ControllerPatch
{
    private static void Prefix(InputManager_Base __instance)
    {
        foreach (var keyBind in CustomKeyBind.KeyBinds)
        {
            __instance.userData.RegisterBind(keyBind.Name, keyBind.Description, keyBind.DefaultKey);
        }
    }
    
    private static int RegisterBind(this UserData self, string name, string description, KeyboardKeyCode keycode, int elementIdentifierId = -1, int category = 0, InputActionType type = InputActionType.Button)
    {
        self.AddAction(category);
        var action = self.GetAction(self.actions.Count - 1)!;

        action.name = name;
        action.descriptiveName = description;
        action.categoryId = category;
        action.type = type;
        action.userAssignable = true;

        var a = new ActionElementMap
        {
            _elementIdentifierId = elementIdentifierId,
            _actionId = action.id,
            _elementType = ControllerElementType.Button,
            _axisContribution = Pole.Positive,
            _keyboardKeyCode = keycode,
            _modifierKey1 = ModifierKey.None,
            _modifierKey2 = ModifierKey.None,
            _modifierKey3 = ModifierKey.None
        };
        self.keyboardMaps._items[0].actionElementMaps.Add(a);
        self.joystickMaps._items[0].actionElementMaps.Add(a);
            
        return action.id;
    }
}