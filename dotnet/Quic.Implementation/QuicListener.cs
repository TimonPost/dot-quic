using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Quic.Native;
using Quic.Native.ApiWrappers;
using Quic.Native.Events;
using Quic.Native.Handles;

namespace Quic.Implementation
{
    /// Listens for connections from QUIC protocol clients.
    public class QuicListener : Endpoint
    {
        private readonly Dictionary<int, ConnectionHandle> _connections;

        private readonly ConnectionDriver _connectionDriver;
        private readonly ConnectionListener _connectionListener;
        private readonly CancellationToken _connectionListenerCancellationToken;

        public QuicListener(IPEndPoint ipEndpoint)
        {
            QuinnApi.Initialize();

            var serverConfig = new ServerConfig();
            QuinnApi.CreateServerEndpoint(serverConfig.Handle, out var id, out var handle).Unwrap();

            Id = id;
            Handle = handle;
            QuicSocket = new UdpClient(ipEndpoint);

            _connections = new Dictionary<int, ConnectionHandle>();
            _connectionDriver = new ConnectionDriver(id => _connections[id]);
            _connectionListenerCancellationToken = new CancellationToken();
            _connectionListener = new ConnectionListener(_connectionListenerCancellationToken, Id);

            EndpointEvents.NewConnection += OnNewConnection;
            EndpointEvents.TransmitReady += OnTransmitReady;

            StartReceivingAsync();
            _connectionDriver.StartPollingAsync();
        }

        /// <summary>
        /// Asynchronously wait for incoming connections.
        ///
        /// This function should not be called more then once at the same time. 
        /// </summary>
        /// <returns>QuicConnection</returns>
        public Task<QuicConnection> AcceptAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Listening...");
            return _connectionListener.NextAsync(cancellationToken);
        }

        /// <summary>
        /// Block wait for incoming connections.
        ///
        /// This function should not be called more then once at the same time. 
        /// </summary>
        /// <returns>QuicConnection</returns>
        public QuicConnection Accept()
        {
            Console.WriteLine("Listening...");
            return AcceptAsync(CancellationToken.None).Result;
        }

        /// <summary>
        /// Returns the connection handle for a given connection.
        ///
        /// This handle is a direct pointer into rust and should be treated with care.
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns>ConnectionHandle</returns>
        public ConnectionHandle ConnectionHandle(int connectionId) => _connections[connectionId];

        private void OnTransmitReady(object sender, TransmitEventArgs e)
        {
            if (!IsThisEndpoint(e.Id)) return;

            // TODO: maybe don't send immediately when data is transmit ready. 
            if (Id == e.Id)
                QuicSocket.Send(e.TransmitPacket.Contents, e.TransmitPacket.Contents.Length,
                    e.TransmitPacket.Destination);
        }

        private void OnNewConnection(object? sender, NewConnectionEventArgs e)
        {
            if (IsThisEndpoint(e.EndpointId))
                _connections[e.ConnectionId] = e.ConnectionHandle;
        }
    }
}