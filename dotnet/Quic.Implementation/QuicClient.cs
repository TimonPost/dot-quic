using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Quic.Native;
using Quic.Native.Events;
using Quic.Native.Handles;

namespace Quic.Implementation
{
    public class QuicClient : Endpoint
    {
        private ConnectionDriver _connectionDriver;
        private QuicConnection _innerConnection;

        public QuicClient(IPEndPoint clientIp)
        {
            var clientConfig = new ClientConfig();
            QuinnApi.CreateClientEndpoint(clientConfig.Handle, out var id, out var handle).Unwrap();

            Id = id;
            Handle = handle;
            QuicSocket = new UdpClient(clientIp);
            EndpointEvents.TransmitReady += OnTransmitReady;
            StartReceivingAsync();
        }

        /// <summary>
        /// The underlying QUIC Connection.
        /// </summary>
        public QuicConnection Connection => _innerConnection;
       

        /// <summary>
        /// Connect to the given server ip.
        /// This method will block the current thread until connected.
        /// </summary>
        /// <returns>QuicConnection</returns>
        public QuicConnection Connect(IPEndPoint serverIp)
        {
           return ConnectAsync(serverIp, CancellationToken.None).Result;
        }

        /// <summary>
        /// Connect asynchronously to the given server ip. 
        /// </summary>
        /// <returns>QuicConnection</returns>
        public async Task<QuicConnection> ConnectAsync(IPEndPoint serverIp, CancellationToken token = new())
        {
            QuinnApi.ConnectClient(Handle, serverIp.ToNative(), out ConnectionHandle connectionHandle, out int connectionId).Unwrap();

            _connectionDriver = new ConnectionDriver((id => connectionHandle));
            _connectionDriver.StartPollingAsync();

            var incoming = new IncomingConnection(connectionHandle, connectionId, _connectionDriver);
            incoming.ProcessIncoming(token);
            _innerConnection = await incoming.WaitAsync();

            return _innerConnection;
        }

        private void OnTransmitReady(object? sender, TransmitEventArgs e)
        {
            if (!IsThisEndpoint(e.Id)) return;

            // TODO: maybe don't send immediately when data is transmit ready. 
            if (Id == e.Id)
                QuicSocket.Send(e.TransmitPacket.Contents, e.TransmitPacket.Contents.Length,
                    e.TransmitPacket.Destination);
        }
    }
}