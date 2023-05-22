using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BetterOtherRoles.EnoFw.Kernel;

namespace BetterOtherRoles.EnoFw.Utils;

public static class Attributes
{
    public static List<AttributeMethodResult<T>> GetMethodsByAttribute<T>() where T : Attribute
    {
        var results = new List<AttributeMethodResult<T>>();

        var allClass = GetAssemblies().SelectMany(x => x.GetTypes())
            .Where(x => x is { IsClass: true });
        foreach (var aClass in allClass)
        {
            var allMethods = aClass.GetMethods()
                .Where(method =>
                    method.IsStatic && method.GetCustomAttributes(typeof(T), false).FirstOrDefault() != null).ToList();
            foreach (var aMethod in allMethods)
            {
                var attributes = aMethod.GetCustomAttributes(typeof(T)).Select(attribute => (T)attribute);
                results.AddRange(attributes.Select(attribute => new AttributeMethodResult<T>(attribute, aMethod)));
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
            var attribute = (T) classType.GetCustomAttributes(typeof(T), false).FirstOrDefault();
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
            throw new EnoFwException($"{type.FullName} does not have public parameterless constructor");
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
        public readonly T Attribute;
        public readonly MethodInfo Method;

        public AttributeMethodResult(T attribute, MethodInfo method)
        {
            Attribute = attribute;
            Method = method;
        }
    }
}
