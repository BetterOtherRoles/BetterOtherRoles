using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Unity.IL2CPP;
using BetterOtherRoles.EnoFw.Libs.Reactor.Networking.Rpc;
using BetterOtherRoles.EnoFw.Libs.Reactor.Utilities;
using HarmonyLib;
using Hazel;

namespace BetterOtherRoles.EnoFw.Libs.Reactor.Networking.Attributes;

/// <summary>
/// Automatically registers a method rpc.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class MethodRpcAttribute : Attribute
{
    private static readonly HashSet<Assembly> _registeredAssemblies = new();

    /// <summary>
    /// Gets the id of the rpc.
    /// </summary>
    public uint Id { get; }

    /// <summary>
    /// Gets or sets the send option of the rpc.
    /// </summary>
    public SendOption SendOption { get; set; } = SendOption.Reliable;

    /// <summary>
    /// Gets or sets the local handling of the rpc.
    /// </summary>
    public RpcLocalHandling LocalHandling { get; set; } = RpcLocalHandling.Before;

    /// <summary>
    /// Gets or sets a value indicating whether the rpc should be sent immediately.
    /// </summary>
    public bool SendImmediately { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MethodRpcAttribute"/> class.
    /// </summary>
    /// <param name="id">The id of the rpc.</param>
    /// <param name="sendImmediately">If the rpc should be sent immediately.</param>
    /// <param name="localHandling">local handling of the rpc.</param>
    public MethodRpcAttribute(uint id, bool sendImmediately = true, RpcLocalHandling localHandling = RpcLocalHandling.After)
    {
        Id = id;
        SendImmediately = sendImmediately;
        LocalHandling = localHandling;
    }

    /// <summary>
    /// Registers all method rpc's annotated with <see cref="MethodRpcAttribute"/> in the specified <paramref name="assembly"/>.
    /// </summary>
    /// <remarks>This is called automatically on plugin assemblies so you probably don't need to call this.</remarks>
    /// <param name="assembly">The assembly to search.</param>
    /// <param name="plugin">The plugin to register the rpc to.</param>
    public static void Register(Assembly assembly, BasePlugin plugin)
    {
        if (_registeredAssemblies.Contains(assembly)) return;
        _registeredAssemblies.Add(assembly);

        var methods = assembly.GetTypes()
            .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public));

        foreach (var method in methods)
        {
            var attribute = method.GetCustomAttribute<MethodRpcAttribute>();
            if (attribute == null)
            {
                continue;
            }

            try
            {
                var customRpc = new MethodRpc(plugin, method, attribute.Id, attribute.SendOption, attribute.LocalHandling, attribute.SendImmediately);
                PluginSingleton<BetterOtherRolesPlugin>.Instance.CustomRpcManager.Register(customRpc);
            }
            catch (Exception e)
            {
                BetterOtherRolesPlugin.Logger.LogWarning($"Failed to register {method.FullDescription()}: {e}");
            }
        }
    }

    internal static void Initialize()
    {
        IL2CPPChainloader.Instance.PluginLoad += (_, assembly, plugin) => Register(assembly, plugin);
    }
}
