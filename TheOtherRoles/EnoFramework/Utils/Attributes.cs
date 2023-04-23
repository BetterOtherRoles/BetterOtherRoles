using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TheOtherRoles.EnoFramework.Kernel;

namespace TheOtherRoles.EnoFramework.Utils;

public static class Attributes
{
    public static List<AttributeMethodResult<T>> GetMethodsByAttribute<T>() where T : Attribute
    {
        var results = new List<AttributeMethodResult<T>>();
        foreach (var singleton in Instances.Singletons)
        {
            var methods = singleton.Key.GetMethods()
                .Where(method => method.GetCustomAttributes(typeof(T), false).FirstOrDefault() != null).ToList();
            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes(typeof(T)).Select(attribute => (T) attribute);
                foreach (var attribute in attributes)
                {
                    results.Add(new AttributeMethodResult<T>(attribute, method, method.IsStatic ? null : singleton.Value));
                }
            }
        }

        var classes = GetAssemblies().SelectMany(x => x.GetTypes())
            .Where(x => x is { IsClass: true, IsPublic: true } && !Instances.Has(x));
        foreach (var cClass in classes)
        {
            var methods = cClass.GetMethods()
                .Where(method => method.IsStatic && method.GetCustomAttributes(typeof(T), false).FirstOrDefault() != null).ToList();
            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes(typeof(T)).Select(attribute => (T) attribute);
                foreach (var attribute in attributes)
                {
                    results.Add(new AttributeMethodResult<T>(attribute, method));
                }
            }
        }

        return results;
    }

    public static List<AttributeClassResult<T>> GetClassesByAttribute<T>() where T : Attribute
    {
        var results = new List<AttributeClassResult<T>>();
        var classes = GetAssemblies().SelectMany(x => x.GetTypes())
            .Where(x => x is { IsClass: true });
        foreach (var classType in classes)
        {
            var attribute = (T?) classType.GetCustomAttributes(typeof(T), false).FirstOrDefault();
            if (attribute != null)
            {
                results.Add(new AttributeClassResult<T> { Attribute = attribute, Type = classType });
            }
        }

        return results;
    }

    public static object GetInstance(Type type)
    {
        var instanceField = type
            .GetFields(BindingFlags.Static | BindingFlags.Public)
            .FirstOrDefault(field => field.Name == "Instance");
        if (instanceField != null && instanceField.FieldType == type)
        {
            var val = instanceField.GetValue(null);
            if (val != null)
            {
                return val;
            }
        }

        var constructorInfo = type.GetConstructors()
            .FirstOrDefault(info => info.GetParameters().Length == 0);
        if (constructorInfo == null)
            throw new KernelException($"{type.FullName} does not have public parameterless constructor");
        var instance = constructorInfo.Invoke(Array.Empty<object>());
        if (instanceField != null && instanceField.FieldType == type && type.IsInstanceOfType(instance))
        {
            instanceField.SetValue(null, instance);
        }

        return instance;
    }

    private static List<Assembly> GetAssemblies()
    {
        var assemblies = new List<Assembly>();
        var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (assemblyFolder == null) return assemblies;
        foreach (var path in Directory.GetFiles(assemblyFolder, "*.dll"))
        {
            assemblies.Add(Assembly.LoadFrom(path));
        }

        return assemblies;
    }

    public class AttributeClassResult<T> where T : Attribute
    {
        public T Attribute;
        public Type Type;
    }

    public class AttributeMethodResult<T> where T : Attribute
    {
        public T Attribute;
        public MethodInfo MethodInfo;
        public object? Instance;

        public bool IsStatic
        {
            get
            {
                return Instance == null;
            }
        }

        public AttributeMethodResult(T attribute, MethodInfo methodInfo, object? instance = null)
        {
            Attribute = attribute;
            MethodInfo = methodInfo;
            Instance = instance;
        }

        public void Invoke()
        {
            MethodInfo.Invoke(IsStatic ? null : Instance, Array.Empty<object>());
        }
    }
}