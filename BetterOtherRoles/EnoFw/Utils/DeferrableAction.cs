using System;
using System.Timers;

namespace BetterOtherRoles.EnoFw.Utils;

public class DeferrableAction
{
    private readonly Action _action;
    private readonly Func<bool> _condition;
    private readonly uint _interval;

    private Timer _retryTimer;

    public static void Defer(Action action, Func<bool> condition, uint interval = 1000)
    {
        new DeferrableAction(action, condition, interval).Start();
    }

    public DeferrableAction(Action action, Func<bool> condition, uint interval = 1000)
    {
        _action = action;
        _condition = condition;
        _interval = interval;
    }

    public void Start()
    {
        InternalStart();
    }

    private void InternalStart(bool deferred = false)
    {
        if (!deferred && !_condition())
        {
            BetterOtherRolesPlugin.Logger.LogInfo("Defer action");
            DeferStart();
            return;
        }

        _action();
    }

    private void DeferStart()
    {
        _retryTimer = new Timer(_interval);
        _retryTimer.Elapsed += DeferredHandshake;
        _retryTimer.Start();
    }

    private void DeferredHandshake(object source, ElapsedEventArgs e)
    {
        if (!_condition()) return;
        _retryTimer.Stop();
        _retryTimer = null;
        InternalStart(true);
    }
}