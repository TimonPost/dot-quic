using System.Net;
using System.Runtime.InteropServices;

namespace Quic.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SockaddrInV4
    {
        internal ushort port;
        internal unsafe fixed byte addr[4];
    }
    
    internal static class QuicAddressHelpers
    {
        internal static unsafe IPEndPoint ToIpEndpoint(this SockaddrInV4 inetAddress)
        {
            return new IPEndPoint(new IPAddress(MemoryMarshal.CreateReadOnlySpan<byte>(ref inetAddress.addr[0], 4)), (ushort)IPAddress.NetworkToHostOrder((short)inetAddress.port));
            
        }

        internal static unsafe SockaddrInV4 ToNative(this IPEndPoint endpoint)
        {
            SockaddrInV4 socketAddress = default;
            if (!endpoint.Address.Equals(IPAddress.Any) && !endpoint.Address.Equals(IPAddress.IPv6Any))
            {
                endpoint.Address.TryWriteBytes(MemoryMarshal.CreateSpan<byte>(ref socketAddress.addr[0], 4), out _);
            }

            SetPort(ref socketAddress, endpoint.Port);
            return socketAddress;
        }

        private static void SetPort(ref SockaddrInV4 socketAddrInet, int originalPort)
        {
            ushort convertedPort = (ushort)IPAddress.HostToNetworkOrder((ushort)originalPort);
            socketAddrInet.port = (ushort)originalPort;
        }
    }
}