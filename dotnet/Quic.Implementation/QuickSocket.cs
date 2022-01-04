using System;
using System.Net;
using System.Net.Sockets;

namespace Quic.Implementation
{
    public class QuickSocket
    {
        public readonly UdpClient Socket;
        public IPEndPoint LastAddress;
        
        public const int SIO_UDP_CONNRESET = -1744830452;

        public QuickSocket(IPEndPoint ipEndpoint)
        {

            Socket = new UdpClient(ipEndpoint);
            Socket.Client.IOControl((IOControlCode)SIO_UDP_CONNRESET,
                new byte[] { 0, 0, 0, 0 },
                null);
        }

        public byte[] Receive(out IPEndPoint address)
        {
            byte[] buffer = Socket.Receive(ref LastAddress);
            address = LastAddress;
            return buffer;
        }

        public void Send(byte[] buffer, IPEndPoint destination)
        {
            Socket.Send(buffer, buffer.Length, destination);
        }
    }
}