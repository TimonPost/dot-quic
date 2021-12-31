using System.Net;

namespace Quick.Native.Types
{
    public readonly struct TransmitPacket
    {
        public TransmitPacket(IPEndPoint destination, byte[] contents)
        {
            Destination = destination;
            Contents = contents;
        }

        public readonly IPEndPoint Destination;
        public readonly byte[] Contents;
    }
}