using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Elements.Assets;
using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;

namespace TurboAudioStream.Patches;

/// <summary>
/// Patches UserAudioStream.OnChanges to apply Opus settings before audio input subscription.
/// </summary>
[HarmonyPatch(typeof(UserAudioStream<StereoSample>), "OnChanges")]
internal static class UserAudioStream_OnChanges_Patch
{
    private sealed class BindingState
    {
        public AudioStream<StereoSample>? Stream { get; set; }

        public AudioInput? AudioInput { get; set; }

        public int DeviceIndex { get; set; } = -1;

        public bool UseFilteredData { get; set; }
    }

    private readonly struct EffectiveBinding(
        AudioStream<StereoSample>? stream,
        AudioInput? audioInput,
        int deviceIndex,
        bool useFilteredData
    )
    {
        public AudioStream<StereoSample>? Stream { get; } = stream;

        public AudioInput? AudioInput { get; } = audioInput;

        public int DeviceIndex { get; } = deviceIndex;

        public bool UseFilteredData { get; } = useFilteredData;
    }

    private static readonly ConditionalWeakTable<
        UserAudioStream<StereoSample>,
        BindingState
    > BindingStates = [];

    private static readonly FieldInfo? LastDeviceIndexField = AccessTools.Field(
        typeof(UserAudioStream<StereoSample>),
        "lastDeviceIndex"
    );

    private static readonly FieldInfo? LastFilteredDataField = AccessTools.Field(
        typeof(UserAudioStream<StereoSample>),
        "lastFilteredData"
    );

    private static readonly MethodInfo? UnregisterEventsMethod = AccessTools.Method(
        typeof(UserAudioStream<StereoSample>),
        "UnregisterEvents"
    );

    private static bool reportedMissingPatchMember;

    /// <summary>
    /// Applies low-latency settings before the original method can subscribe to audio input events.
    /// </summary>
    public static bool Prefix(UserAudioStream<StereoSample> __instance)
    {
        try
        {
            EffectiveBinding binding = GetEffectiveBinding(__instance);
            ApplyLowLatencySettings(binding.Stream);
            ForceRebindWhenStreamOrInputChanged(__instance, binding);
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
        catch (InvalidOperationException ex)
        {
            ResoniteMod.Error(ex.Message);
        }

        return true;
    }

    /// <summary>
    /// Records the actual binding identity after the original OnChanges finishes.
    /// </summary>
    public static void Postfix(UserAudioStream<StereoSample> __instance)
    {
        try
        {
            EffectiveBinding binding = GetEffectiveBinding(__instance);
            BindingState state = GetBindingState(__instance);
            state.Stream = binding.Stream;
            state.AudioInput = binding.AudioInput;
            state.DeviceIndex = binding.DeviceIndex;
            state.UseFilteredData = binding.UseFilteredData;
        }
        catch (ArgumentException ex)
        {
            ResoniteMod.Error(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            ResoniteMod.Error(ex.Message);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ApplyLowLatencySettings(AudioStream<StereoSample>? stream)
    {
        if (stream is not OpusStream<StereoSample> opusStream)
        {
            return;
        }

        if (opusStream.User?.IsLocalUser != true)
        {
            return;
        }

        opusStream.MinimumBufferDelay.Value = TurboAudioStreamConfig.MinimumBufferDelay;
        opusStream.ApplicationType.Value = TurboAudioStreamConfig.ApplicationType;
        opusStream.EncoderDelay.Value = TurboAudioStreamConfig.EncoderDelay;
        ResoniteMod.DebugFunc(() =>
            $"Prepared low-latency stream settings before audio input bind (MinimumBufferDelay: {opusStream.MinimumBufferDelay.Value}, ApplicationType: {opusStream.ApplicationType.Value}, EncoderDelay: {opusStream.EncoderDelay.Value}, BitRate: {opusStream.BitRate.Value})"
        );
    }

    private static void ForceRebindWhenStreamOrInputChanged(
        UserAudioStream<StereoSample> instance,
        EffectiveBinding binding
    )
    {
        if (
            LastDeviceIndexField is null
            || LastFilteredDataField is null
            || UnregisterEventsMethod is null
        )
        {
            ReportMissingPatchMember();
            return;
        }

        if (LastDeviceIndexField.GetValue(instance) is not int lastDeviceIndex)
        {
            throw new InvalidOperationException("Cannot read UserAudioStream.lastDeviceIndex.");
        }

        if (LastFilteredDataField.GetValue(instance) is not bool lastFilteredData)
        {
            throw new InvalidOperationException("Cannot read UserAudioStream.lastFilteredData.");
        }

        var vanillaWillRebind =
            lastDeviceIndex != binding.DeviceIndex || lastFilteredData != binding.UseFilteredData;
        if (vanillaWillRebind)
        {
            return;
        }

        BindingState state = GetBindingState(instance);
        var streamOrInputChanged =
            !ReferenceEquals(state.Stream, binding.Stream)
            || !ReferenceEquals(state.AudioInput, binding.AudioInput);

        if (!streamOrInputChanged)
        {
            return;
        }

        _ = UnregisterEventsMethod.Invoke(instance, null);
        LastDeviceIndexField.SetValue(instance, -1);
    }

    private static EffectiveBinding GetEffectiveBinding(UserAudioStream<StereoSample> instance)
    {
        AudioStream<StereoSample>? stream = instance.Stream.Target;
        var useFilteredData = instance.UseFilteredData.Value;
        AudioSystem audioSystem = instance.AudioSystem;
        var deviceIndex = instance.TargetDeviceIndex ?? -1;

        deviceIndex = instance.User != instance.LocalUser
            ? -1
            : deviceIndex >= 0 ? MathX.Clamp(deviceIndex, 0, audioSystem.AudioInputs.Count) : audioSystem.DefaultAudioInputIndex;

        AudioInput? audioInput =
            deviceIndex >= 0 && deviceIndex < audioSystem.AudioInputs.Count
                ? audioSystem.AudioInputs[deviceIndex]
                : null;

        return new EffectiveBinding(stream, audioInput, deviceIndex, useFilteredData);
    }

    private static void ReportMissingPatchMember()
    {
        if (reportedMissingPatchMember)
        {
            return;
        }

        reportedMissingPatchMember = true;
        ResoniteMod.Error(
            "UserAudioStream.OnChanges patch could not find required private members. Stream/input rebind enhancement is disabled."
        );
    }

    private static BindingState GetBindingState(UserAudioStream<StereoSample> instance)
    {
        return BindingStates.GetValue(instance, _ => new BindingState());
    }
}
