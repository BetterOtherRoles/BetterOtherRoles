using TheOtherRoles.Customs.Roles;
using UnityEngine;

namespace TheOtherRoles.EnoFramework;

public class RoleInfo
{
    public readonly CustomRole Role;
    public Color Color => Role.Color;
    public string Name => Role.NameText;
    public string IntroDescription => Role.IntroDescription;
    public string ShortDescription => Role.ShortDescription;

    public RoleInfo(CustomRole role)
    {
        Role = role;
    }
}