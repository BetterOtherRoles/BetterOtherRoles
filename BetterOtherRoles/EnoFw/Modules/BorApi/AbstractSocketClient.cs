using System;
using System.Collections.Generic;
using System.Globalization;
using BetterOtherRoles.EnoFw.Libs;
using BetterOtherRoles.EnoFw.Libs.SocketIOClient;
using BetterOtherRoles.EnoFw.Libs.SocketIOClient.Transport;

namespace BetterOtherRoles.EnoFw.Modules.BorApi;

public abstract class AbstractSocketClient<TEvent> where TEvent : struct, IConvertible
{
    protected readonly SocketIOUnity Io;
    private readonly Dictionary<TEvent, Action<SocketIOResponse>> _registeredHandlers = new();
    protected bool Debug;
    
    public bool Connected => Io.Connected;

    protected AbstractSocketClient(string endpoint, bool debug = false)
    {
        if (!typeof(TEvent).IsEnum) throw new ArgumentException($"{nameof(TEvent)} must be an enumerated type");
        Debug = debug;
        var uri = new Uri(endpoint);
        Io = new SocketIOUnity(uri, new SocketIOOptions
        {
            Query = new Dictionary<string, string>(),
            EIO = EngineIO.V4,
            Transport = TransportProtocol.WebSocket
        });

        // Reserved socket.io events
        Io.OnConnected += OnConnected;
        Io.OnPing += OnPing;
        Io.OnPong += OnPong;
        Io.OnDisconnected += OnDisconnected;
        Io.OnReconnectAttempt += OnReconnectAttempt;

        Io.OnAnyInUnityThread(OnAny);
    }

    protected void On<T>(TEvent eventName, Action<T> handler)
    {
        if (_registeredHandlers.ContainsKey(eventName))
        {
            _registeredHandlers.Remove(eventName);
        }
        var h = (SocketIOResponse rawData) =>
        {
            var data = Rpc.Deserialize<T>(rawData.GetValue<string>());
            handler(data);
        };
        _registeredHandlers.Add(eventName, h);
        Io.OnUnityThread(Event(eventName), h);
    }

    protected void On(TEvent eventName, Action handler)
    {
        if (_registeredHandlers.ContainsKey(eventName))
        {
            _registeredHandlers.Remove(eventName);
        }
        var h = (SocketIOResponse rawData) =>
        {
            handler();
        };
        _registeredHandlers.Add(eventName, h);
        Io.OnUnityThread(Event(eventName), h);
    }

    protected void Emit<T>(TEvent eventName, T argument)
    {
        var data = Rpc.Serialize(argument);
        Io.Emit(Event(eventName), data);
    }
    
    protected void Emit(TEvent eventName)
    {
        Io.Emit(Event(eventName));
    }

    protected void Off(TEvent eventName)
    {
        if (!_registeredHandlers.ContainsKey(eventName)) return;
        _registeredHandlers.Remove(eventName);
        Io.Off(Event(eventName));
    }

    protected void Off()
    {
        foreach (var registeredHandler in _registeredHandlers)
        {
            Io.Off(Event(registeredHandler.Key));
        }
        _registeredHandlers.Clear();
    }

    private static string Event(TEvent eventName)
    {
        return eventName.ToUInt32(CultureInfo.CurrentCulture).ToString();
    }

    protected virtual void OnAny(string eventName, SocketIOResponse response)
    {
        if (Debug) BetterOtherRolesPlugin.Logger.LogInfo($"Events incoming: {eventName}");
    }

    protected virtual void OnConnected(object sender, EventArgs e)
    {
        if (Debug) BetterOtherRolesPlugin.Logger.LogInfo("socket.OnConnected");
    }

    protected virtual void OnPing(object sender, EventArgs e)
    {
        if (Debug) BetterOtherRolesPlugin.Logger.LogInfo("Ping");
    }

    protected virtual void OnPong(object sender, TimeSpan e)
    {
        if (Debug) BetterOtherRolesPlugin.Logger.LogInfo("Pong: " + e.TotalMilliseconds);
    }

    protected virtual void OnDisconnected(object sender, string e)
    {
        if (Debug) BetterOtherRolesPlugin.Logger.LogInfo("disconnect: " + e);
    }

    protected virtual void OnReconnectAttempt(object sender, int e)
    {
        if (Debug) BetterOtherRolesPlugin.Logger.LogInfo($"{DateTime.Now} Reconnecting: attempt = {e}");
    }
}