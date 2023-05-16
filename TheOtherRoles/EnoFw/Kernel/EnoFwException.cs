using System;

namespace TheOtherRoles.EnoFw.Kernel;

public class EnoFwException : Exception
{
    public EnoFwException(string message)
    {
        TheOtherRolesPlugin.Logger.LogError(message);
    }
}