using System.Net;
using System.Net.Sockets;

namespace Quick.Implementation
{
    public class QuickSocket
    {
        private readonly UdpClient _socket;
        public IPEndPoint LastAddress;

        public QuickSocket(IPEndPoint ipEndpoint)
        {
            _socket = new UdpClient(ipEndpoint);
        }

        public byte[] Receive(out IPEndPoint address)
        {
            byte[] buffer = _socket.Receive(ref LastAddress);
            address = LastAddress;
            return buffer;
        }

        public void Send(byte[] buffer)
        {
            _socket.Client.Send(buffer);
        }
    }
}