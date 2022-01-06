using System;
using System.Net;
using Quic.Native.Handles;

namespace Quic.Native.ApiWrappers
{
    public class EndpointApi
    {
        public static void HandleDatagram(EndpointHandle handle, ReadOnlySpan<byte> data, IPEndPoint endpoint)
        {
            unsafe
            {
                fixed (byte* valuePtr = data)
                {
                    QuinnApi.handle_datagram(handle, (IntPtr)valuePtr, (UIntPtr)data.Length, endpoint.ToNative()).Unwrap();
                }
            }
        }
    }
}