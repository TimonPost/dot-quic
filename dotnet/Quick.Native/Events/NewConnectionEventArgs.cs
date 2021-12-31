using System;
using Quick.Native.Handles;

namespace Quick.Native.Events
{
    public class NewConnectionEventArgs : EventArgs
    {
        public NewConnectionEventArgs(ConnectionHandle handle, int id)
        {
            ConnectionHandle = handle;
            Id = id;
        }

        public ConnectionHandle ConnectionHandle { get; }
        public int Id { get; set; }
    }
}