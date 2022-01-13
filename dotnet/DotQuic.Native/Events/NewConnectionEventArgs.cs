using System;
using DotQuic.Native.Handles;

namespace DotQuic.Native.Events
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