using BetterOtherRoles.EnoFw.Libs.Reactor.Utilities.Attributes;
using Il2CppInterop.Runtime.Attributes;
using UnityEngine;

namespace BetterOtherRoles.EnoFw.Modules.BorApi;

[RegisterInIl2Cpp]
public class BorComponent : MonoBehaviour
{
    private Rect _windowRect = new(0, 0, 500, Screen.height);
    string stringToEdit = "Hello World\nI've got 2 lines...";
    private bool value = true;
    
    public void OnGUI()
    {
        GUILayout.Window(10, _windowRect, (GUI.WindowFunction)WindowFunction, "");
    }
    
    [HideFromIl2Cpp]
    private void WindowFunction(int windowID)
    {
        stringToEdit = GUILayout.TextField(stringToEdit, GUILayout.Width(50));
    }
}