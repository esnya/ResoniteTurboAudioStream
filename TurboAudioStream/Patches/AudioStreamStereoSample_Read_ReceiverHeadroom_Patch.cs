using System;
using System.Linq;
using System.Reflection;
using Awwdio;
using Elements.Assets;
using Elements.Core;
using FrooxEngine;
using HarmonyLib;

namespace TurboAudioStream.Patches;

[HarmonyPatch]
internal static class AudioStreamStereoSample_Read_ReceiverHeadroom_Patch
{
    private static readonly Action<AudioStream<StereoSample>, int> UpdateReadSampleCount =
        AccessTools.MethodDelegate<Action<AudioStream<StereoSample>, int>>(
            AccessTools.DeclaredMethod(typeof(AudioStream<StereoSample>), "UpdateReadSampleCount")
                ?? throw new MissingMethodException(
                    typeof(AudioStream<StereoSample>).FullName,
                    "UpdateReadSampleCount"
                )
        );

    [HarmonyTargetMethod]
    internal static MethodInfo TargetMethod()
    {
        Type closedType = typeof(AudioStream<>).MakeGenericType(typeof(StereoSample));
        MethodInfo method = closedType
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Single(m => m.Name == "Read" && m.IsGenericMethodDefinition);
        return method.MakeGenericMethod(typeof(StereoSample));
    }

    public static bool Prefix(
        AudioStream<StereoSample> __instance,
        Span<StereoSample> buffer,
        AudioSimulator simulator,
        ref CircularAudioBuffer<StereoSample> ___audioBuffer,
        ref object ____lock,
        ref long ___currentReadPosition,
        ref long ___nextReadPosition,
        ref double ____lastAudioTime,
        ref int ____lastReadSamples,
        ref bool ____activeReading,
        ref int ____missedSamples
    )
    {
        _ = simulator;

        if (
            !AudioPatchPredicates.IsRemoteReceiverOpusStream(
                __instance,
                out OpusStream<StereoSample>? opusStream
            )
        )
        {
            return true;
        }

        Engine? engine = __instance.Engine;
        if (__instance.IsDisposed || engine is null || __instance.User.IsAudioLocallyBlocked)
        {
            buffer.Clear();
            return false;
        }

        int sampleRate = engine.AudioSystem.SampleRate;
        int stockTargetBufferSize = __instance.TargetBufferSize;
        float effectiveMinimumBufferDelay = Math.Max(
            __instance.MinimumBufferDelay.Value,
            TurboAudioStreamConfig.ReceiverMinimumBufferDelayFloor
        );
        int effectiveTargetBufferSize = Math.Max(
            stockTargetBufferSize,
            TurboAudioStreamConfig.ReceiverBufferSizeFloor
        );
        bool raisedDelayFloor = effectiveMinimumBufferDelay > __instance.MinimumBufferDelay.Value;
        bool raisedBufferFloor = effectiveTargetBufferSize > stockTargetBufferSize;

        lock (____lock)
        {
            if (___audioBuffer == null || ___audioBuffer.Length != effectiveTargetBufferSize)
            {
                ___audioBuffer = ___audioBuffer is null
                    ? new CircularAudioBuffer<StereoSample>(effectiveTargetBufferSize)
                    : new CircularAudioBuffer<StereoSample>(
                        effectiveTargetBufferSize,
                        ___audioBuffer
                    );
            }

            double dspTime = engine.AudioSystem.DSPTime;
            int effectiveMinimumBufferedSamples = MathX.RoundToInt(
                sampleRate * effectiveMinimumBufferDelay
            );
            bool isNewFrame = ____lastAudioTime != dspTime;
            ____lastAudioTime = dspTime;

            if (isNewFrame)
            {
                long previousNextReadPosition = ___nextReadPosition;
                ___currentReadPosition = previousNextReadPosition;
            }

            long globalPosition = ___currentReadPosition;
            int requestedSamples = buffer.Length;
            if (!____activeReading)
            {
                requestedSamples = MathX.Max(requestedSamples, effectiveMinimumBufferedSamples);
            }

            bool hasEnoughBufferedSamples =
                ___audioBuffer.AvailableSamples(globalPosition) >= requestedSamples;
            bool wasActiveReading = ____activeReading;

            if (____activeReading || hasEnoughBufferedSamples)
            {
                UpdateReadSampleCount(__instance, buffer.Length);
                int readSamples = ___audioBuffer.Read(buffer, ref globalPosition);
                buffer[readSamples..].Clear();
                ____missedSamples += buffer.Length - readSamples;
                ____lastReadSamples = readSamples;

                if (isNewFrame)
                {
                    ___nextReadPosition = globalPosition;
                }
            }
            else
            {
                buffer.Clear();
            }

            if (!hasEnoughBufferedSamples)
            {
                __instance.World.AudioStreamUnderrun();
                if (____activeReading)
                {
                    ____missedSamples += buffer.Length;
                }

                ____activeReading = false;

                if (wasActiveReading)
                {
                    PatchTriggerLogger.Log(
                        AudioPatchFeature.ReceiverHeadroom,
                        "underrun-to-inactive",
                        opusStream!.User,
                        opusStream,
                        null
                    );
                }
            }
            else
            {
                ____activeReading = true;

                if (raisedDelayFloor || raisedBufferFloor)
                {
                    string reason =
                        $"floors-applied delayFloor={effectiveMinimumBufferDelay:0.###} bufferFloor={effectiveTargetBufferSize}";
                    PatchTriggerLogger.Log(
                        AudioPatchFeature.ReceiverHeadroom,
                        reason,
                        opusStream!.User,
                        opusStream,
                        null
                    );
                }
            }
        }

        return false;
    }
}
