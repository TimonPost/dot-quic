using System;
using DotQuic.Native.Types;

namespace DotQuic.Native.Events
{
    public class TransmitEventArgs : EventArgs
    {
        public TransmitEventArgs(TransmitPacket handle, int id)
        {
            TransmitPacket = handle;
            Id = id;
        }

        public TransmitPacket TransmitPacket { get; }
        public int Id { get; set; }
    }
}