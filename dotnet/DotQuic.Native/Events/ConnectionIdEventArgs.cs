using System;

namespace DotQuic.Native.Events
{
    public class ConnectionIdEventArgs : EventArgs
    {
        public ConnectionIdEventArgs(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }

    public class ConnectionCloseEventArgs : EventArgs
    {
        public ConnectionCloseEventArgs(int id)
        {
            Id = id;
        }

        public int Id { get; set; }
    }
}