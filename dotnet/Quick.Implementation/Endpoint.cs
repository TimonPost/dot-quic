using Quick.Native;
using Quick.Native.Handles;

namespace Quick.Implementation
{
    public class Endpoint
    {
        public EndpointHandle Handle { get; protected set; }
        public int Id { get; protected set; }
        
        public QuickSocket QuicSocket { get; protected set; }
    }
}