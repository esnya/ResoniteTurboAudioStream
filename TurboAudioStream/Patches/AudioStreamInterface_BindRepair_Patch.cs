using FrooxEngine;
using HarmonyLib;

namespace TurboAudioStream.Patches;

[HarmonyPatch(typeof(AudioStreamInterface), nameof(AudioStreamInterface.SetAudioStream))]
internal static class AudioStreamInterface_BindRepair_Patch
{
    public static void Prefix(
        AudioStreamInterface __instance,
        IAudioStream source,
        int bitrate,
        float volume,
        bool spatialize
    )
    {
        if (__instance.Source.Target is not null)
        {
            return;
        }

        if (
            !AudioBindingRepairHelper.TryRepair(
                __instance,
                source,
                out AudioStreamController? controller
            )
        )
        {
            return;
        }

        User? user = source.User;
        PatchTriggerLogger.Log(
            AudioPatchFeature.BindRepair,
            $"repaired-interface-bind bitrate={bitrate} volume={volume:0.###} spatialize={spatialize}",
            user,
            source,
            controller?.Slot
        );
    }
}
