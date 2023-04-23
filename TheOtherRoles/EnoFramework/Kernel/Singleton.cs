using System;
using System.Collections.Generic;
using System.Linq;
using TheOtherRoles.EnoFramework.Utils;

namespace TheOtherRoles.EnoFramework.Kernel;

[AttributeUsage(AttributeTargets.Class)]
public class EnoSingletonAttribute : Attribute
{
    public readonly int Priority;
    public EnoSingletonAttribute(int priority = 0)
    {
        Priority = priority;
    }
}

public static class Singleton<T>
{
    public static T Instance
    {
        get => (T) Instances.Get<T>();
        set
        {
            if (value == null)
                throw new KernelException($"Cannot set singleton of {typeof(T).FullName} with null value");
            Instances.Set((T) value);
        }
    }
}

public static class Instances
{
    public static readonly Dictionary<Type, object> Singletons = new();

    public static object Get<T>()
    {
        return Singletons[typeof(T)];
    }

    public static bool Has(Type type)
    {
        return Singletons.ContainsKey(type);
    }

    public static void Set(object value)
    {
        Singletons[value.GetType()] = value;
    }

    public static void Load()
    {
        var classResults = Attributes.GetClassesByAttribute<EnoSingletonAttribute>().OrderBy(cr => cr.Attribute.Priority);
        foreach (var classResult in classResults)
        {
            Set(Attributes.GetInstance(classResult.Type));
        }
    }
}
