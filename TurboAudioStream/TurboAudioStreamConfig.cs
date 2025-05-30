using POpusCodec.Enums;

namespace TurboAudioStream;

/// <summary>
/// Configuration class for TurboAudioStream low-latency audio settings
/// </summary>
internal static class TurboAudioStreamConfig
{
    public const float DefaultMinimumBufferDelay = 0.2f;
    public const int DefaultBufferSize = 24000;
    public const OpusApplicationType DefaultApplicationType = OpusApplicationType.Audio;
    public const Delay DefaultEncoderDelay = Delay.Delay20ms;

    public static float MinimumBufferDelay = DefaultMinimumBufferDelay;
    public static int BufferSize = DefaultBufferSize;
    public static OpusApplicationType ApplicationType = DefaultApplicationType;
    public static Delay EncoderDelay = DefaultEncoderDelay;
}
