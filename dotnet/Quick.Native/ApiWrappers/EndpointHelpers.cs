using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Quick.Native.Handles;

namespace Quick.Native.ApiWrappers
{
    public ref struct Transmit
    {
        public Transmit(byte[] contents, IPEndPoint ipEndPoint)
        {
            Contents = contents;
            IpEndPoint = ipEndPoint;
        }

        public byte[] Contents { get; set; }
        public IPEndPoint IpEndPoint { get; set; }
    }

    public class EndpointApi
    {
        public static void HandleDatagram(EndpointHandle handle, ReadOnlySpan<byte> data, IPEndPoint endpoint)
        {
            unsafe
            {
                fixed (byte* valuePtr = data)
                {
                    QuinnApi.handle_datagram(handle, (IntPtr)valuePtr, (UIntPtr)data.Length, endpoint.ToNative());
                }
            }
        }

        // public Transmit PollTransmit(EndpointHandle handle)
        // {
        //     Span<byte> buffer = new Span<byte>(new byte[1024]);
        //
        //     unsafe
        //     {
        //         fixed (byte* messageBufPtr = buffer)
        //         {
        //             var result = QuinnApi.poll_transmit(
        //                 handle,
        //                 (IntPtr)messageBufPtr,
        //                 (UIntPtr)buffer.Length,
        //                 out var actualMessageLen,
        //                 out SockaddrInV4 destination
        //             );
        //
        //             if (result.Erroneous())
        //             {
        //                 return new Transmit();
        //             }
        //
        //             return new Transmit(buffer.Slice(0, (int)actualMessageLen).ToArray(), QuicAddressHelpers.ToIpEndpoint(ref destination));
        //         }
        //     }
        // }
    }
}
