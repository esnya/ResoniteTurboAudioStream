using System.Reflection;
using FrooxEngine;
using HarmonyLib;

namespace TurboAudioStream.Patches;

internal static class SessionIncomingMessageManager_ProcessStreamMessage_DropLog_Patch
{
    private static MethodBase TargetMethod() =>
        AccessTools.Method(typeof(SessionIncomingMessageManager), "ProcessStreamMessage")!;

    public static void Prefix(SessionIncomingMessageManager __instance, StreamMessage stream)
    {
        if (stream.IsOutdated || __instance.World.InitState != FrooxEngine.World.InitializationState.Finished)
        {
            StreamDropLogHelper.LogAsyncDrop("world-not-ready-or-outdated", stream);
        }
    }
}
