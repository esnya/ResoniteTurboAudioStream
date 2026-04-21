using FrooxEngine;
using HarmonyLib;

namespace TurboAudioStream.Patches;

[HarmonyPatch(typeof(SyncController), nameof(SyncController.ApplyStreams))]
internal static class SyncController_ApplyStreams_DropLog_Patch
{
    public static void Prefix(SyncController __instance, StreamMessage message)
    {
        if (message.StreamTime - __instance.World.Time.WorldTime < -4.0)
        {
            StreamDropLogHelper.LogAsyncDrop("world-not-ready-or-outdated", message);
            return;
        }

        User? user = __instance.World.TryGetUser(message.UserID);
        if (user is null)
        {
            StreamDropLogHelper.LogAsyncDrop("missing-user-or-stream", message);
            return;
        }

        if (message.StreamGroup == ushort.MaxValue || user.StreamConfigurationVersion != message.StreamStateVersion)
        {
            string reason =
                user.StreamConfigurationVersion != message.StreamStateVersion
                    ? "config-version-mismatch"
                    : "missing-user-or-stream";
            StreamDropLogHelper.LogAsyncDrop(reason, message, user);
        }
    }
}
