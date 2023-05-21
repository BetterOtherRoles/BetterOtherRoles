using System.Linq;
using UnityEngine;

namespace BetterOtherRoles.EnoFw.Utils;

public static class GameObjectExtensions
{
    public static GameObject GetChildrenRecursive(this GameObject gameObject, string[] path)
    {
        while (true)
        {
            var key = path[0];
            path = path.Skip(1).ToArray();
            var obj = gameObject.transform.Find(key);
            if (obj == null)
            {
                BetterOtherRolesPlugin.Logger.LogDebug($"Unable to find GameObject named {key} in {obj.name}");
                return null;
            }
            if (path.Length == 0) return obj.gameObject;
            gameObject = obj.gameObject;
        }
    }

    public static void LogComponents(this GameObject gameObject)
    {
        Component[] components = gameObject.GetComponents<Component>();
        foreach (var component in components)
        {
            BetterOtherRolesPlugin.Logger.LogDebug($"{component.name}: {component}");
        }
    }
}