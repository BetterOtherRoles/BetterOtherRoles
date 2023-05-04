using System;
using System.Text.Json;

namespace TheOtherRoles.EnoFw;

public static class Rpc
{
    public enum Kernel : uint
    {
        ResetVariables,
        ShareOptions,
        ForceEnd,
        WorkaroundSetRoles,
        SetRole,
        SetModifier,
        VersionHandshake,
        UseUncheckedVent,
        UncheckedMurderPlayer,
        UncheckedExilePlayer,
        UncheckedCmdReportDeadBody,
        DynamicMapOption,
        SetGameStarting,
        ShareGameMode,
    }
    
    public enum Role : uint
    {
        EngineerFixLights = 50,
        EngineerFixSubmergedOxygen,
        EngineerUsedRepair,
        CleanBody,
        TimeMasterRewindTime,
        TimeMasterShield,
        MedicSetShielded,
        SetFutureShielded,
        ShieldedMurderAttempt,
    }
    
    public static string Serialize<T>(T data)
    {
        return JsonSerializer.Serialize(data);
    }

    public static T Deserialize<T>(string data)
    {
        return JsonSerializer.Deserialize<T>(data) ?? throw new Exception("Deserialization error");
    }
}