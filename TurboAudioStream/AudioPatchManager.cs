using System;
using System.Collections.Generic;
using HarmonyLib;
using TurboAudioStream.Patches;

namespace TurboAudioStream;

internal static class AudioPatchManager
{
    private sealed class PatchRegistration(
        string harmonyId,
        Func<bool> isEnabled,
        IReadOnlyList<Type> patchTypes
    )
    {
        public Harmony Harmony { get; } = new(harmonyId);
        public Func<bool> IsEnabled { get; } = isEnabled;
        public IReadOnlyList<Type> PatchTypes { get; } = patchTypes;
        public bool IsPatched { get; set; }
    }

    private static readonly Dictionary<AudioPatchFeature, PatchRegistration> Registrations = new()
    {
        [AudioPatchFeature.OutgoingTuning] = new(
            "TurboAudioStream.OutgoingTuning",
            () => TurboAudioStreamConfig.EnableOutgoingTuningPatch,
            [typeof(AudioStreamInterface_OutgoingTuning_Patch)]
        ),
        [AudioPatchFeature.BindRepair] = new(
            "TurboAudioStream.BindRepair",
            () => TurboAudioStreamConfig.EnableBindRepairPatch,
            [typeof(AudioStreamInterface_BindRepair_Patch)]
        ),
        [AudioPatchFeature.AsyncFreshnessGuard] = new(
            "TurboAudioStream.AsyncFreshnessGuard",
            () => TurboAudioStreamConfig.EnableAsyncFreshnessGuardPatch,
            [typeof(SyncController_AsyncStreamDecodeAndDispose_FreshnessGuard_Patch)]
        ),
        [AudioPatchFeature.StreamConfigDropLog] = new(
            "TurboAudioStream.StreamConfigDropLog",
            () => TurboAudioStreamConfig.EnableStreamConfigDropLogPatch,
            [
                typeof(SyncController_AsyncStreamDecodeAndDispose_DropLog_Patch),
                typeof(SyncController_ApplyStreams_DropLog_Patch),
                typeof(SessionIncomingMessageManager_ProcessStreamMessage_DropLog_Patch),
            ]
        ),
        [AudioPatchFeature.ReceiverHeadroom] = new(
            "TurboAudioStream.ReceiverHeadroom",
            () => TurboAudioStreamConfig.EnableReceiverHeadroomPatch,
            [typeof(AudioStreamStereoSample_Read_ReceiverHeadroom_Patch)]
        ),
    };

    public static void Synchronize()
    {
        foreach ((_, PatchRegistration registration) in Registrations)
        {
            bool shouldBeEnabled = registration.IsEnabled();

            if (shouldBeEnabled && !registration.IsPatched)
            {
                foreach (Type patchType in registration.PatchTypes)
                {
                    _ = registration.Harmony.CreateClassProcessor(patchType).Patch();
                }

                registration.IsPatched = true;
            }
            else if (!shouldBeEnabled && registration.IsPatched)
            {
                registration.Harmony.UnpatchAll(registration.Harmony.Id);
                registration.IsPatched = false;
            }
        }
    }

    public static void UnpatchAll()
    {
        foreach ((_, PatchRegistration registration) in Registrations)
        {
            registration.Harmony.UnpatchAll(registration.Harmony.Id);
            registration.IsPatched = false;
        }
    }
}
