using System;
using System.Reflection;
using FrooxEngine;
using HarmonyLib;

namespace TurboAudioStream.Patches;

[HarmonyPatch]
internal static class SessionIncomingMessageManager_ProcessStreamMessage_DropLog_Patch
{
    [HarmonyTargetMethod]
    internal static MethodInfo TargetMethod() =>
        AccessTools.DeclaredMethod(typeof(SessionIncomingMessageManager), "ProcessStreamMessage")
        ?? throw new MissingMethodException(
            typeof(SessionIncomingMessageManager).FullName,
            "ProcessStreamMessage"
        );

    public static void Prefix(SessionIncomingMessageManager __instance, StreamMessage stream)
    {
        if (
            stream.IsOutdated
            || __instance.World.InitState != FrooxEngine.World.InitializationState.Finished
        )
        {
            StreamDropLogHelper.LogAsyncDrop("world-not-ready-or-outdated", stream);
        }
    }
}
