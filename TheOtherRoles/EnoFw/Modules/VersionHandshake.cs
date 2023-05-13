using System;
using System.Timers;
using HarmonyLib;
using Hazel;
using TheOtherRoles.Modules;
using TheOtherRoles.Patches;
using TheOtherRoles.Players;

namespace TheOtherRoles.EnoFw.Modules;

public static class VersionHandshake
{
    private const byte VersionHandshakeRpcId = 221;
    private static Timer _retryTimer;

    public static void Share(bool deferred = false)
    {
        if (!deferred && (!AmongUsClient.Instance || CachedPlayer.LocalPlayer == null))
        {
            TheOtherRolesPlugin.Logger.LogDebug("defer share");
            DeferShare();
            return;
        }
        
        TheOtherRolesPlugin.Logger.LogDebug("Sharing BetterOtherRoles version...");
        TheOtherRolesPlugin.Logger.LogDebug($"My client id: {CachedPlayer.LocalPlayer.PlayerControl.OwnerId}");
        TheOtherRolesPlugin.Logger.LogDebug($"My friend code: {AmongUsClient.Instance.GetClient(CachedPlayer.LocalPlayer.PlayerControl.OwnerId).FriendCode}");

        var writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
            VersionHandshakeRpcId, SendOption.Reliable, -1);
        writer.Write((byte)TheOtherRolesPlugin.Version.Major);
        writer.Write((byte)TheOtherRolesPlugin.Version.Minor);
        writer.Write((byte)TheOtherRolesPlugin.Version.Build);
        writer.Write(AmongUsClient.Instance.AmHost ? GameStartManagerPatch.timer : -1f);
        writer.WritePacked(AmongUsClient.Instance.ClientId);
        writer.Write(AmongUsClient.Instance.GetClient(CachedPlayer.LocalPlayer.PlayerControl.OwnerId).FriendCode);
        writer.Write((byte)(TheOtherRolesPlugin.Version.Revision < 0 ? 0xFF : TheOtherRolesPlugin.Version.Revision));
        writer.Write(CustomGuid.Guid.ToByteArray());
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        ReceiveVersionHandshake(
            TheOtherRolesPlugin.Version.Major,
            TheOtherRolesPlugin.Version.Minor,
            TheOtherRolesPlugin.Version.Build,
            TheOtherRolesPlugin.Version.Revision,
            CustomGuid.Guid,
            AmongUsClient.Instance.ClientId,
            AmongUsClient.Instance.GetClient(CachedPlayer.LocalPlayer.PlayerControl.OwnerId).FriendCode);
    }

    private static void DeferShare()
    {
        _retryTimer = new Timer(1000);

        _retryTimer.Elapsed += DeferredHandshake;

        _retryTimer.AutoReset = true;
        _retryTimer.Enabled = true;
    }

    private static void DeferredHandshake(object source, ElapsedEventArgs e)
    {
        if (CachedPlayer.LocalPlayer == null || AmongUsClient.Instance == null) return;
        _retryTimer.AutoReset = false;
        _retryTimer.Enabled = false;
        Share(true);
    }

    private static void ReceiveVersionHandshake(int major, int minor, int build, int revision, Guid guid, int clientId, string friendCode)
    {
        TheOtherRolesPlugin.Logger.LogDebug($"ClientId: {clientId} && FriendCode: {friendCode}");
        var version = revision < 0 ? new Version(major, minor, build) : new Version(major, minor, build, revision);

        GameStartManagerPatch.PlayerVersions[clientId] = new PlayerVersion(version, guid, friendCode);
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    internal static class PlayerControlHandleRpcPatch
    {
        private static void Postfix([HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
        {
            if (callId != VersionHandshakeRpcId) return;
            var major = reader.ReadByte();
            var minor = reader.ReadByte();
            var patch = reader.ReadByte();
            var timer = reader.ReadSingle();
            if (!AmongUsClient.Instance.AmHost && timer >= 0f)
            {
                GameStartManagerPatch.timer = timer;
            }

            var versionOwnerId = reader.ReadPackedInt32();
            var friendCode = reader.ReadString();
            byte revision = 0xFF;
            Guid guid;
            if (reader.Length - reader.Position >= 17)
            {
                revision = reader.ReadByte();
                byte[] guidBytes = reader.ReadBytes(16);
                guid = new Guid(guidBytes);
            }
            else
            {
                guid = new Guid(new byte[16]);
            }
            ReceiveVersionHandshake(major, minor, patch, revision == 0xFF ? -1 : revision, guid, versionOwnerId, friendCode);
        }
    }
    
    public class PlayerVersion
    {
        public readonly Version Version;
        public readonly Guid Guid;
        public readonly string FriendCode;

        public PlayerVersion(Version version, Guid guid, string friendCode)
        {
            Version = version;
            Guid = guid;
            FriendCode = friendCode;
        }

        public bool GuidMatches()
        {
            return CustomGuid.Guid.Equals(Guid);
        }
    }
}