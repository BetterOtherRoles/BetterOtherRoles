using System;
using System.Text.Json;

namespace TheOtherRoles.EnoFw;

public static class Rpc
{
    public enum Kernel : uint
    {
        ResetVariables = 1,
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
        ShifterShift,
        SwapperSwap,
        MayorSetVoteTwice,
        MorphlingMorph,
        CamouflagerCamouflage,
        VampireSetBitten,
        PlaceGarlic,
        TrackerUsedTracker,
        DeputyUsedHandcuffs,
        DeputyPromotes,
        JackalCreatesSidekick,
        SidekickPromotes,
        ErasePlayerRoles,
        SetFutureErased,
        SetFutureShifted,
        PlaceNinjaTrace,
        PlacePortal,
        UsePortal,
        PlaceJackInTheBox,
        LightsOut,
        PlaceCamera,
        SealVent,
        ArsonistWin,
        GuesserShoot,
        LawyerSetTarget,
        LawyerPromotesToPursuer,
        SetBlanked,
        SetFutureSpelled,
        SetBloody,
        SetTiebreak,
        SetInvisible,
        ThiefStealsRole,
        SetTrap,
        TriggerTrap,
        PlaceBomb,
        DefuseBomb,
        UndertakerDragBody,
        UndertakerDropBody
    }

    public enum Module : uint
    {
        ShareRandomSeed = 150,
        ShowFailedMurderAttempt,
        ShareFriendCode,
        SetFirstKill,
        SetGuesserGm,
        ShareTimer,
        HuntedShield,
        HuntedRewindTime,
        ShareRoom,
        ShareGhostInfo,
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