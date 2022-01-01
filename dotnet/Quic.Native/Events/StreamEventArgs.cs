using System;

namespace Quic.Native.Events
{
    public class StreamEventArgs : EventArgs
    {
        public StreamEventArgs(int connectionId, long streamId)
        {
            ConnectionId = connectionId;
            StreamId = streamId;
        }

        public int ConnectionId { get; }
        public long StreamId { get; }
    }
}