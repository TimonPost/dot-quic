using System.Net.Sockets;
using Quic.Native.Handles;

namespace Quic.Implementation
{
    public abstract class Endpoint
    {
        public EndpointHandle Handle { get; protected set; }
        public int Id { get; protected set; }

        public UdpClient QuicSocket { get; protected set; }
    }
}