using UnityEngine;

namespace TheOtherRoles.Modules;

public enum CustomRpc
{
    ShareRandomSeeds = 10,
    ShowFailedMurderAttempt = 11,
    ShareFriendCode = 12,
    
    UndertakerDragBody = 13,
    UndertakerDropBody = 14,
}

public static class Vector3Extensions
{
    public static string Serialize(this Vector3 v)
    {
        return $"{v.x}|{v.y}|{v.z}";
    }

    public static Vector3 Deserialize(string rawData)
    {
        var data = rawData.Split("|");
        return new Vector3(float.Parse(data[0]), float.Parse(data[1]), float.Parse(data[2]));
    }
}

public static class Vector2Extensions
{
    public static string Serialize(this Vector2 v)
    {
        return $"{v.x}|{v.y}";
    }
    
    public static Vector2 Deserialize(string rawData)
    {
        var data = rawData.Split("|");
        return new Vector2(float.Parse(data[0]), float.Parse(data[1]));
    }
}