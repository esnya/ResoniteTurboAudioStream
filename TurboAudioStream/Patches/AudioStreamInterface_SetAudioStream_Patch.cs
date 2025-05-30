using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Elements.Assets;
using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;

namespace TurboAudioStream.Patches;

/// <summary>
/// Patches AudioStreamInterface.SetAudioStream method to apply low-latency settings to OpusStream
/// </summary>
[HarmonyPatch(typeof(AudioStreamInterface), nameof(AudioStreamInterface.SetAudioStream))]
internal static class AudioStreamInterface_SetAudioStream_Patch
{
    /// <summary>
    /// Applies low-latency settings to OpusStream before SetAudioStream method execution
    /// </summary>
    public static void Prefix(IAudioStream source)
    {
        try
        {
            if (source is not OpusStream<StereoSample> stream)
            {
                return;
            }

            if (stream.User?.IsLocalUser != true)
            {
                return;
            }

            ApplyLowLatencySettings(stream);
            ResoniteMod.DebugFunc(() =>
                $"Applied low-latency settings (MinimumBufferDelay: {stream.MinimumBufferDelay.Value}, BufferSize: {stream.BufferSize.Value}, ApplicationType: {stream.ApplicationType.Value}, EncoderDelay: {stream.EncoderDelay.Value})"
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

    /// <summary>
    /// Applies low-latency settings to OpusStream
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ApplyLowLatencySettings(OpusStream<StereoSample> stream)
    {
        stream.MinimumBufferDelay.Value = TurboAudioStreamConfig.MinimumBufferDelay;
        stream.BufferSize.Value = TurboAudioStreamConfig.BufferSize;
        stream.ApplicationType.Value = TurboAudioStreamConfig.ApplicationType;
        stream.EncoderDelay.Value = TurboAudioStreamConfig.EncoderDelay;
    }
}
