//
//  SocketIOUnity.cs
//  SocketIOUnity
//
//  Created by itisnajim on 10/30/2021.
//  Copyright (c) 2021 itisnajim. All rights reserved.
//

using System;
using System.Threading;
using BetterOtherRoles.EnoFw.Libs.SocketIOClient;

namespace BetterOtherRoles.EnoFw.Libs;

public class SocketIOUnity : SocketIO
{
    public enum UnityThreadScope
    {
        Update,
        LateUpdate,
        FixedUpdate
    }

    private UnityThreadScope _unityThreadScope = UnityThreadScope.Update;

    public SocketIOUnity(string uri, UnityThreadScope threadScope = UnityThreadScope.Update) : base(uri)
    {
        CommonInit(threadScope);
    }

    public SocketIOUnity(Uri uri, UnityThreadScope unityThreadScope = UnityThreadScope.Update) : base(uri)
    {
        CommonInit(unityThreadScope);
    }

    public SocketIOUnity(string uri, SocketIOOptions options, UnityThreadScope threadScope = UnityThreadScope.Update) : base(uri, options)
    {
        CommonInit(threadScope);
    }

    public SocketIOUnity(Uri uri, SocketIOOptions options, UnityThreadScope threadScope = UnityThreadScope.Update) : base(uri, options)
    {
        CommonInit(threadScope);
    }

    private void CommonInit(UnityThreadScope threadScope)
    {
        UnityThread.InitUnityThread();
        _unityThreadScope = threadScope;
    }

    /// <summary>
    /// Register a new handler for the given event.
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="callback"></param>
    public void OnUnityThread(string eventName, Action<SocketIOResponse> callback)
    {
        On(eventName, res =>
        {
            ExecuteInUnityThread(() => callback(res));
        });

    }

    public void OnAnyInUnityThread(OnAnyHandler handler)
    {
        OnAny((name, response) =>
        {
            ExecuteInUnityThread(() => handler(name, response));
        });
    }

    /// <summary>
    /// Emits an event to the socket
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="data">Any other parameters can be included. All serializable datastructures are supported, including byte[]</param>
    /// <returns></returns>
    public void Emit(string eventName, params object[] data)
    {
        EmitAsync(eventName, data).ContinueWith(_ => {});
    }

    public void Emit(string eventName, Action<SocketIOResponse> ack, params object[] data)
    {
        EmitAsync(eventName, CancellationToken.None, ack, data).ContinueWith(_ => {});
    }

    public void Connect()
    {
        ConnectAsync().ContinueWith(_ => {});
    }

    public void Disconnect()
    {
        DisconnectAsync().ContinueWith(_ => {});
    }

    private void ExecuteInUnityThread(Action action)
    {
        switch (_unityThreadScope)
        {
            case UnityThreadScope.LateUpdate :
                UnityThread.executeInLateUpdate(action);
                break;
            case UnityThreadScope.FixedUpdate :
                UnityThread.executeInFixedUpdate(action);
                break;
            case UnityThreadScope.Update:
            default :
                UnityThread.ExecuteInUpdate(action);
                break;
        }
    }

}