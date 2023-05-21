using System;

namespace BetterOtherRoles.EnoFw.Libs.SocketIOClient.Extensions;

internal static class DisposableExtensions
{
    public static void TryDispose(this IDisposable disposable)
    {
        disposable?.Dispose();
    }
}