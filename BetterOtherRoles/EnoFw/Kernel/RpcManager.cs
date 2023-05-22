using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BetterOtherRoles.Players;
using HarmonyLib;
using Hazel;

namespace BetterOtherRoles.EnoFw.Kernel;

public class RpcManager
{
    public static readonly RpcManager Instance = new();

    private const byte ReservedRpcId = 254;
    
    public enum LocalExecution
    {
        None,
        Before,
        After
    }

    private readonly Dictionary<uint, List<MethodInfo>> Methods = new();

    public void Send(uint id, bool immediately = true, LocalExecution localExecution = LocalExecution.After)
    {
        Send(id, new NoData(), immediately, localExecution);
    }

    public void Send<T>(uint id, T data, bool immediately = true, LocalExecution localExecution = LocalExecution.After)
    {
        var rawData = string.Empty;
        if (typeof(T) != typeof(NoData))
        {
            rawData = Rpc.Serialize(data);
        }
        if (localExecution == LocalExecution.Before)
        {
            Invoke(id, rawData);
        }
        
        if (immediately)
        {
            var writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
                ReservedRpcId, SendOption.Reliable);
            writer.Write(id);
            writer.Write(rawData);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        else
        {
            var writer = AmongUsClient.Instance.StartRpc(CachedPlayer.LocalPlayer.PlayerControl.NetId, ReservedRpcId);
            writer.Write(id);
            writer.Write(rawData);
            writer.EndMessage();
        }

        if (localExecution == LocalExecution.After)
        {
            Invoke(id, rawData);
        }
    }

    public void Load()
    {
        var methods = Utils.Attributes.GetMethodsByAttribute<BindRpcAttribute>();
        foreach (var method in methods)
        {
            BetterOtherRolesPlugin.Logger.LogDebug($"RpcManager register {method.Method.Name} for id {method.Attribute.Id}");
            if (method.Method.ReturnType != typeof(void))
            {
                BetterOtherRolesPlugin.Logger.LogWarning($"BindRpc method {method.Method.Name} must return void");
                continue;
            }
            if (!Methods.ContainsKey(method.Attribute.Id))
            {
                Methods[method.Attribute.Id] = new List<MethodInfo>();
            }
            Methods[method.Attribute.Id].Add(method.Method);
        }
    }

    private void Invoke(uint id, string rawData)
    {
        if (!Methods.ContainsKey(id))
        {
            BetterOtherRolesPlugin.Logger.LogError($"Unhandled rpc [ID]: ${id}, [RAW CONTENT]: {rawData}");
            return;
        }

        var methods = Methods[id];
        foreach (var method in methods)
        {
            var parameter = method.GetParameters().FirstOrDefault();
            if (parameter == null)
            {
                method.Invoke(null, Array.Empty<object>());
                continue;
            }

            var deserializerMethod = typeof(Rpc).GetMethod(nameof(Rpc.Deserialize));
            if (deserializerMethod == null)
            {
                BetterOtherRolesPlugin.Logger.LogWarning("Unable to find deserializer method");
                continue;
            }
            var data = deserializerMethod.MakeGenericMethod(parameter.ParameterType)
                .Invoke(null, new object[] { rawData });
            method.Invoke(null, new[] { data });
        }
    }
    
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    internal static class PlayerControlHandleRpcPatch
    {
        private static void Postfix([HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
        {
            if (callId != ReservedRpcId) return;
            var rpcId = reader.ReadUInt32();
            var rawData = reader.ReadString();
            Instance.Invoke(rpcId, rawData);
        }
    }
    
    private class NoData
    {
        
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class BindRpcAttribute : Attribute
{
    public readonly uint Id;

    public BindRpcAttribute(uint id)
    {
        Id = id;
    }
}