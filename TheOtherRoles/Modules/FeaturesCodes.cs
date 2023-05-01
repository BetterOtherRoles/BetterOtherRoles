using System.Collections.Generic;
using System.Linq;

namespace TheOtherRoles.Modules;

public static class FeaturesCodes
{

    private static List<string> Keys => TheOtherRolesPlugin.FeaturesCodes.Value.Split("|").ToList();
    
    private static bool Has(string key)
    {
        return Keys.Contains(key);
    }

    private static void Add(string key)
    {
        if (Keys.Contains(key)) return;
        var keys = Keys;
        keys.Add(key);
        TheOtherRolesPlugin.FeaturesCodes.Value = string.Join("|", keys);
    }
}