using System.Timers;
using TheOtherRoles.EnoFw.Kernel;
using TheOtherRoles.Modules;

namespace TheOtherRoles.EnoFw.Modules;

public static class VersionHandshake
{
    private static Timer _retryTimer;

    public static void DeferHandshake()
    {
        _retryTimer = new Timer(1000);

        _retryTimer.Elapsed += Handshake;

        _retryTimer.AutoReset = true;
        _retryTimer.Enabled = true;
    }

    private static void Handshake(object source, ElapsedEventArgs e)
    {
        if (PlayerControl.LocalPlayer == null || AmongUsClient.Instance == null) return;
        _retryTimer.AutoReset = false;
        _retryTimer.Enabled = false;
        KernelRpc.VersionHandshake(
            AmongUsClient.Instance.ClientId,
            TheOtherRolesPlugin.Version,
            CustomGuid.Guid,
            AmongUsClient.Instance.AmHost ? Patches.GameStartManagerPatch.timer : -1,
            true);
    }
}