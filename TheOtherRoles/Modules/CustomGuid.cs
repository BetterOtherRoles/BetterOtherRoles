using System;
using System.Reflection;

namespace TheOtherRoles.Modules;

public static class CustomGuid
{
    public static Guid Guid => TheOtherRolesPlugin.DevGuid.Value != ""
        ? Guid.Parse(TheOtherRolesPlugin.DevGuid.Value)
        : CurrentGuid;

    private static Guid CurrentGuid => Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId;

    public static bool IsDevMode => TheOtherRolesPlugin.DevGuid.Value != "" &&
                                    Guid.Parse(TheOtherRolesPlugin.DevGuid.Value).Equals(CurrentGuid);
}