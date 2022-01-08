using System;
using Quic.Native.Handles;

namespace Quic.Native.Events
{
    public class NewConnectionEventArgs : EventArgs
    {
        public NewConnectionEventArgs(ConnectionHandle handle, int connectionId)
        {
            ConnectionHandle = handle;
            ConnectionId = connectionId;
        }

        public ConnectionHandle ConnectionHandle { get; }
        public int ConnectionId { get; set; }
        public int EndpointId { get; set; } = 1;
    }
}