using System;

namespace Quick.Native.Events
{
    public class ConnectionIdEventArgs : EventArgs
    {
        public ConnectionIdEventArgs(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }
}