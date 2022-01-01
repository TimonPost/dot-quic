using System;
using Quic.Native.Types;

namespace Quic.Native.Events
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