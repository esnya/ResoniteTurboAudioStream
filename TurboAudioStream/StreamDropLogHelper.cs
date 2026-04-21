using System;
using System.Diagnostics.CodeAnalysis;
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

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Drop-log parsing must fail open so malformed async stream payloads do not break stream processing."
    )]
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
        catch (Exception)
        {
            return false;
        }
    }
}
