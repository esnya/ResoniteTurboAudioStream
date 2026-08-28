using FrooxEngine;
using HarmonyLib;

namespace TurboAudioStream.Patches;

[HarmonyPatch(typeof(SyncController), nameof(SyncController.AsyncStreamDecodeAndDispose))]
internal static class SyncController_AsyncStreamDecodeAndDispose_FreshnessGuard_Patch
{
    public static bool Prefix(SyncController __instance, StreamMessage message)
    {
        if (!message.IsOutdated)
        {
            return true;
        }

        User? user = __instance.World.TryGetUser(message.UserID);
        PatchTriggerLogger.Log(
            AudioPatchFeature.AsyncFreshnessGuard,
            "outdated-before-decode",
            user,
            null,
            null
        );
        message.Dispose();
        return false;
    }
}
