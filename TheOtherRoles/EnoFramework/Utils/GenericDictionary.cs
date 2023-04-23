using System.Collections.Generic;
using TheOtherRoles.EnoFramework.Kernel;

namespace TheOtherRoles.EnoFramework.Utils;

public class GenericDictionary
{
    private readonly Dictionary<string, object> _dict = new();

    public void Add<T>(string key, T value) where T : class
    {
        _dict.Add(key, value);
    }

    public T GetValue<T>(string key) where T : class
    {
        return _dict[key] as T ?? throw new KernelException("Null value in GenericDictionary");
    }
}