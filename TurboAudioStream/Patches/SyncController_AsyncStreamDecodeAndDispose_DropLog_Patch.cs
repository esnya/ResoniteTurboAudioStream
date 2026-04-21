using System.Collections.Generic;
using FrooxEngine;
using HarmonyLib;

namespace TurboAudioStream.Patches;

[HarmonyPatch(typeof(SyncController), nameof(SyncController.AsyncStreamDecodeAndDispose))]
internal static class SyncController_AsyncStreamDecodeAndDispose_DropLog_Patch
{
    public static void Prefix(SyncController __instance, StreamMessage message)
    {
        User? user = __instance.World.TryGetUser(message.UserID);
        if (user is null)
        {
            StreamDropLogHelper.LogAsyncDrop("missing-user-or-stream", message);
            return;
        }

        if (user.StreamConfigurationVersion != message.StreamStateVersion)
        {
            StreamDropLogHelper.LogAsyncDrop("config-version-mismatch", message, user);
            return;
        }

        if (!StreamDropLogHelper.TryReadAsyncStreamId(message, out ulong streamId))
        {
            return;
        }

        try
        {
            _ = user.GetStream(streamId);
        }
        catch (KeyNotFoundException)
        {
            StreamDropLogHelper.LogAsyncDrop("missing-user-or-stream", message, user);
        }
    }
}
