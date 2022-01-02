using System;
using Quic.Native.Types;

namespace Quic.Native.Events
{
    public class StreamEventArgs : EventArgs
    {
        public StreamEventArgs(int connectionId, long streamId, StreamType streamType)
        {
            ConnectionId = connectionId;
            StreamId = streamId;
            StreamType = streamType;
        }

        public int ConnectionId { get; }
        public long StreamId { get; }

        public StreamType StreamType { get; }
    }

    public class StreamTypeEventArgs : EventArgs
    {
        public StreamTypeEventArgs(int connectionId, StreamType streamType)
        {
            ConnectionId = connectionId;
            StreamType = streamType;
        }

        public int ConnectionId { get; }
        public StreamType StreamType { get; }
    }
}