using TheOtherRoles.EnoFw.Utils;
using UnityEngine;

namespace TheOtherRoles.EnoFw.Modules;

public static class CustomRegions
{
    public static readonly IRegionInfo[] Regions = new[]
    {
        new StaticHttpRegionInfo("Custom", StringNames.NoTranslation, TheOtherRolesPlugin.Ip.Value,
                new Il2CppReferenceArray<ServerInfo>(new[]
                    { new ServerInfo("Custom", TheOtherRolesPlugin.Ip.Value, TheOtherRolesPlugin.Port.Value, false) }))
            .CastFast<IRegionInfo>(),
        new StaticHttpRegionInfo(Colors.Cs(Color.green, "BOR [EU]"), StringNames.NoTranslation, "http://87.98.131.98",
                new Il2CppReferenceArray<ServerInfo>(new[]
                    { new ServerInfo("BOR", "http://87.98.131.98", 22023, false) }))
            .CastFast<IRegionInfo>()
    };

    /*
    public static async void LoadRegions()
    {
        Regions.Clear();
        Regions.Add(new StaticHttpRegionInfo("Custom", StringNames.NoTranslation, TheOtherRolesPlugin.Ip.Value, new Il2CppReferenceArray<ServerInfo>(new[] { new ServerInfo("Custom", TheOtherRolesPlugin.Ip.Value, TheOtherRolesPlugin.Port.Value, false) })).CastFast<IRegionInfo>());
        var regions = await ExternalResources.Get<ApiRegion[]>("CustomRegions.json");
        TheOtherRolesPlugin.Logger.LogInfo(Rpc.Serialize(regions));
        foreach (var region in regions)
        {
            Regions.Add(new StaticHttpRegionInfo(Colors.Cs(Color.green, region.Name), StringNames.NoTranslation, region.Host,
                    new Il2CppReferenceArray<ServerInfo>(new[]
                        { new ServerInfo(region.Name, region.Host, (ushort)region.Port, region.UseDtls) }))
                .CastFast<IRegionInfo>());
        }
        TheOtherRolesPlugin.Logger.LogInfo($"{Regions.Count} regions to load");
    }

    public class ApiRegion
    {
        public string Name { get; set; }
        public string Host { get; set; }
        public uint Port { get; set; }
        public bool UseDtls { get; set; }
    }
    */
}