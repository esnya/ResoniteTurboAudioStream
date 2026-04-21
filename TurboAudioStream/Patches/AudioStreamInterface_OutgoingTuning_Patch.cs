using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Elements.Assets;
using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;

namespace TurboAudioStream.Patches;

/// <summary>
/// Applies configured outgoing Opus tuning for the local sender.
/// </summary>
[HarmonyPatch(typeof(AudioStreamInterface), nameof(AudioStreamInterface.SetAudioStream))]
internal static class AudioStreamInterface_OutgoingTuning_Patch
{
    public static void Prefix(IAudioStream source)
    {
        try
        {
            if (!AudioPatchPredicates.IsLocalSenderOpusStream(source, out OpusStream<StereoSample>? stream))
            {
                return;
            }

            ApplyOutgoingTuning(stream!);
            PatchTriggerLogger.Log(
                AudioPatchFeature.OutgoingTuning,
                "applied-configured-opus-tuning",
                stream!.User,
                stream,
                null
            );
        }
        catch (ReflectionTypeLoadException ex)
        {
            ResoniteMod.Error(ex.Message);
        }
        catch (TargetInvocationException ex)
        {
            ResoniteMod.Error(ex.InnerException?.Message ?? ex.Message);
        }
        catch (ArgumentException ex)
        {
            ResoniteMod.Error(ex.Message);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ApplyOutgoingTuning(OpusStream<StereoSample> stream)
    {
        stream.MinimumBufferDelay.Value = TurboAudioStreamConfig.MinimumBufferDelay;
        stream.BufferSize.Value = TurboAudioStreamConfig.BufferSize;
        stream.ApplicationType.Value = TurboAudioStreamConfig.ApplicationType;
        stream.EncoderDelay.Value = TurboAudioStreamConfig.EncoderDelay;
    }
}
