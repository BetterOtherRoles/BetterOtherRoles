using System;

namespace BetterOtherRoles.EnoFw.Kernel;

public class EnoFwException : Exception
{
    public EnoFwException(string message)
    {
        BetterOtherRolesPlugin.Logger.LogError(message);
    }
}