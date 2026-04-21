using System;
using Elements.Core;
using FrooxEngine;

namespace TurboAudioStream;

internal static class StreamDropLogHelper
{
    public static void LogAsyncDrop(
        string reason,
        StreamMessage message,
        User? user = null,
        IWorldElement? stream = null,
        Slot? controllerSlot = null
    )
    {
        PatchTriggerLogger.Log(
            AudioPatchFeature.StreamConfigDropLog,
            $"{reason} async={message.IsAsynchronous} group={message.StreamGroup} stateVersion={message.StreamStateVersion}",
            user,
            stream,
            controllerSlot
        );
    }

    public static bool TryReadAsyncStreamId(StreamMessage message, out ulong streamId)
    {
        streamId = 0;

        try
        {
            using BitReaderStream stream = new(message.GetData());
            using BitBinaryReaderX reader = new(stream);
            streamId = reader.Read7BitEncoded();
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        catch (System.IO.EndOfStreamException)
        {
            return false;
        }
    }
}
