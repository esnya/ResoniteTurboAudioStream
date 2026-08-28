using POpusCodec.Enums;

namespace TurboAudioStream;

/// <summary>
/// Configuration class for TurboAudioStream low-latency audio settings.
/// </summary>
internal static class TurboAudioStreamConfig
{
    public const float DefaultMinimumBufferDelay = 0.2f;
    public const int DefaultBufferSize = 24000;
    public const OpusApplicationType DefaultApplicationType = OpusApplicationType.Audio;
    public const Delay DefaultEncoderDelay = Delay.Delay20ms;
    public const bool DefaultEnableOutgoingTuningPatch = true;
    public const bool DefaultEnableBindRepairPatch = false;
    public const bool DefaultEnableAsyncFreshnessGuardPatch = false;
    public const bool DefaultEnableStreamConfigDropLogPatch = false;
    public const bool DefaultEnableReceiverHeadroomPatch = false;
    public const bool DefaultEnablePatchTriggerLogging = false;
    public const float DefaultPatchLogCooldownSeconds = 5f;
    public const float DefaultReceiverMinimumBufferDelayFloor = 0.05f;
    public const int DefaultReceiverBufferSizeFloor = 12000;

    public static float MinimumBufferDelay = DefaultMinimumBufferDelay;
    public static int BufferSize = DefaultBufferSize;
    public static OpusApplicationType ApplicationType = DefaultApplicationType;
    public static Delay EncoderDelay = DefaultEncoderDelay;
    public static bool EnableOutgoingTuningPatch = DefaultEnableOutgoingTuningPatch;
    public static bool EnableBindRepairPatch = DefaultEnableBindRepairPatch;
    public static bool EnableAsyncFreshnessGuardPatch = DefaultEnableAsyncFreshnessGuardPatch;
    public static bool EnableStreamConfigDropLogPatch = DefaultEnableStreamConfigDropLogPatch;
    public static bool EnableReceiverHeadroomPatch = DefaultEnableReceiverHeadroomPatch;
    public static bool EnablePatchTriggerLogging = DefaultEnablePatchTriggerLogging;
    public static float PatchLogCooldownSeconds = DefaultPatchLogCooldownSeconds;
    public static float ReceiverMinimumBufferDelayFloor = DefaultReceiverMinimumBufferDelayFloor;
    public static int ReceiverBufferSizeFloor = DefaultReceiverBufferSizeFloor;
}
