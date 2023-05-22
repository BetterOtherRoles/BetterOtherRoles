namespace BetterOtherRoles.EnoFw.Modules;

public static class DevMode
{
#if DEBUG
    public const bool EnableDevMode = true;
    public const bool DisableEndGame = true;
    public const bool ShowRoleDescription = false;
#else
    public const bool EnableDevMode = false;
    public const bool DisableEndGame = false;
    public const bool ShowRoleDescription = false;
#endif
}