using System;
using System.Collections.Concurrent;
using Elements.Core;
using FrooxEngine;
using ResoniteModLoader;

namespace TurboAudioStream;

internal static class PatchTriggerLogger
{
    private static readonly ConcurrentDictionary<string, DateTime> LastLogTimes = new();

    public static void Log(
        AudioPatchFeature feature,
        string reason,
        User? user = null,
        IWorldElement? stream = null,
        Slot? controllerSlot = null
    )
    {
        if (!TurboAudioStreamConfig.EnablePatchTriggerLogging)
        {
            return;
        }

        string userId = user?.UserID ?? "<null>";
        string streamId = stream?.ReferenceID.ToString() ?? "<null>";
        string slotLabel = controllerSlot is null
            ? "<null>"
            : $"{controllerSlot.Name} ({controllerSlot.ReferenceID})";
        string key = $"{feature}|{reason}|{userId}|{streamId}|{slotLabel}";
        DateTime now = DateTime.UtcNow;
        TimeSpan cooldown = TimeSpan.FromSeconds(
            Math.Max(0f, TurboAudioStreamConfig.PatchLogCooldownSeconds)
        );

        if (
            LastLogTimes.TryGetValue(key, out DateTime lastLogTime)
            && now - lastLogTime < cooldown
        )
        {
            return;
        }

        LastLogTimes[key] = now;
        ResoniteMod.Msg(
            $"TurboAudioStream[Patch] feature={feature} reason={reason} user={user?.UserName ?? "<null>"} streamId={streamId} controllerSlot={slotLabel}"
        );
    }
}
