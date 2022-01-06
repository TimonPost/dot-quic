using System;
using System.Runtime.InteropServices;
using Quic.Native.Handles;
using Quic.Native.Types;

namespace Quic.Native.Events
{
    public static class EndpointEvents
    {
        private static readonly QuinnApi.OnNewConnection _onNewConnection = OnNewConnection;
        private static readonly QuinnApi.OnTransmit _onTransmit = OnTransmit;
        private static readonly QuinnApi.OnConnectionPollable _onConnectionPollable = OnConnectionPollable;

        public static void Initialize()
        {
            // Delegates should never bee cleaned. 
            GC.KeepAlive(_onNewConnection);
            GC.KeepAlive(_onTransmit);
            GC.KeepAlive(_onConnectionPollable);

            QuinnApi.set_on_new_connection(_onNewConnection).Unwrap();
            QuinnApi.set_on_transmit(_onTransmit).Unwrap();
            QuinnApi.set_on_pollable_connection(_onConnectionPollable).Unwrap();
        }

        public static event EventHandler<TransmitEventArgs> TransmitReady;
        public static event EventHandler<NewConnectionEventArgs> NewConnection;
        public static event EventHandler<ConnectionIdEventArgs> ConnectionPollable;

        private static void OnConnectionPollable(int connectionId)
        {
            ConnectionPollable?.Invoke(null, new ConnectionIdEventArgs(connectionId));
        }

        private static void OnNewConnection(IntPtr handle, int connectionId)
        {
            NewConnection?.Invoke(null, new NewConnectionEventArgs(new ConnectionHandle(handle), connectionId));
        }


        private static void OnTransmit(byte endpointId, IntPtr buffer, IntPtr bufferlenght, SockaddrInV4 address)
        {
            var managedArray = new byte[(int)bufferlenght];
            Marshal.Copy(buffer, managedArray, 0, (int)bufferlenght);

            TransmitReady?.Invoke(null,
                new TransmitEventArgs(new TransmitPacket(address.ToIpEndpoint(), managedArray), endpointId));
        }
    }
}