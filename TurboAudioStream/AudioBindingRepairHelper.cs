using Elements.Assets;
using FrooxEngine;

namespace TurboAudioStream;

internal static class AudioBindingRepairHelper
{
    public static bool TryRepair(
        AudioStreamInterface audioStreamInterface,
        IAudioStream source,
        out AudioStreamController? controller
    )
    {
        controller = audioStreamInterface.Slot.GetComponentInChildren<AudioStreamController>();
        if (controller is null)
        {
            return false;
        }

        bool repaired = false;

        if (audioStreamInterface.Source.Target != controller.Stream)
        {
            audioStreamInterface.Source.Target = controller.Stream;
            repaired = true;
        }

        if (
            source is OpusStream<StereoSample> opusStream
            && audioStreamInterface.Bitrate.Target is null
        )
        {
            audioStreamInterface.Bitrate.Target = opusStream.BitRate;
            repaired = true;
        }

        AudioOutput? audioOutput = controller.AudioOutput.Target;
        if (audioOutput is not null)
        {
            if (audioStreamInterface.Volume.Target is null)
            {
                audioStreamInterface.Volume.Target = audioOutput.Volume;
                repaired = true;
            }

            if (audioStreamInterface.Spatialize.Target is null)
            {
                audioStreamInterface.Spatialize.Target = audioOutput.Spatialize;
                repaired = true;
            }
        }

        if (controller.Stream.Target != source)
        {
            controller.Stream.Target = source;
            repaired = true;
        }

        return repaired;
    }
}
