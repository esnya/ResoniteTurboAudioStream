using Elements.Assets;
using FrooxEngine;

namespace TurboAudioStream;

internal static class AudioPatchPredicates
{
    public static bool IsLocalSenderOpusStream(IAudioStream source, out OpusStream<StereoSample>? stream)
    {
        stream = source as OpusStream<StereoSample>;
        return stream?.User?.IsLocalUser == true;
    }

    public static bool IsRemoteReceiverOpusStream(
        AudioStream<StereoSample> stream,
        out OpusStream<StereoSample>? opusStream
    )
    {
        opusStream = stream as OpusStream<StereoSample>;
        return opusStream is not null && opusStream.User?.IsLocalUser != true;
    }
}
