using System;
using System.Runtime.InteropServices;
using Quic.Native.Handles;
using Quic.Native.Types;

namespace Quic.Native.Events
{
    public static class EndpointEvents
    {
        public static void Initialize()
        {
            QuinnApi.set_on_new_connection(OnNewConnection);
            QuinnApi.set_on_transmit(OnTransmit);
        }

        public static event EventHandler<TransmitEventArgs> TransmitReady;
        public static event EventHandler<NewConnectionEventArgs> NewConnection;

        private static void OnNewConnection(IntPtr handle, int connectionId)
        {
            Console.WriteLine("C#; OnNewConnection; Connection ID: {0}", connectionId);
            NewConnection?.Invoke(null, new NewConnectionEventArgs(new ConnectionHandle(handle), connectionId));
        }


        private static void OnTransmit(byte endpointId, IntPtr buffer, IntPtr bufferlenght, SockaddrInV4 address)
        {
            byte[] managedArray = new byte[(int)bufferlenght];
            Marshal.Copy(buffer, managedArray, 0, (int)bufferlenght);

            TransmitReady?.Invoke(null, new TransmitEventArgs(new TransmitPacket(address.ToIpEndpoint(), managedArray), (byte)endpointId));
        }
    }
}