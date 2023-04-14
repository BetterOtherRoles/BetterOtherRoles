using Rewired;

namespace TheOtherRoles.Modules;

public class CustomKeyBind
{
    public static readonly CustomKeyBind[] KeyBinds =
    {
        new("HunterAdmin", "Admin table button. (Hunter).", KeyboardKeyCode.G),
        new("HunterArrow", "Arrow button. (Hunter).", KeyboardKeyCode.T),
        new("UsePortal", "Use a portal.", KeyboardKeyCode.H),
        new("PortalMakerTp", "Teleport to a portal. (PortalMaker).", KeyboardKeyCode.J),
        new("DefuseBomb", "Defuse button for the bomb (if there is a Bomber).", KeyboardKeyCode.C),
        new("ZoomOut", "Zoom Out", KeyboardKeyCode.KeypadPlus),
    };

    public readonly string Name;
    public readonly string Description;
    public readonly KeyboardKeyCode DefaultKey;

    public CustomKeyBind(string name, string description, KeyboardKeyCode defaultKey)
    {
        Name = name;
        Description = description;
        DefaultKey = defaultKey;
    }
}