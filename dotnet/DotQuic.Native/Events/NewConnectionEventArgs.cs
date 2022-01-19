using System;
using DotQuic.Native.Handles;

namespace DotQuic.Native.Events
{
    public class NewConnectionEventArgs : EventArgs
    {
        public NewConnectionEventArgs(ConnectionHandle handle, int connectionId, int endpointId)
        {
            ConnectionHandle = handle;
            ConnectionId = connectionId;
            EndpointId = endpointId;
        }

        public ConnectionHandle ConnectionHandle { get; }
        public int ConnectionId { get; set; }
        public int EndpointId { get; set; }
    }
}