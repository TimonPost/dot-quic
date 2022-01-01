using Quic.Native.Handles;

namespace Quic.Implementation
{
    public class Endpoint
    {
        public EndpointHandle Handle { get; protected set; }
        public int Id { get; protected set; }
        
        public QuickSocket QuicSocket { get; protected set; }
    }
}