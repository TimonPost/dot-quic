using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using DotQuic.Native;
using DotQuic.Native.Events;
using DotQuic.Native.Handles;

namespace DotQuic
{
    /// Listens for connections from QUIC protocol clients.
    public class QuicListener : Endpoint
    {
        private readonly ConnectionListener _connectionListener;
        private readonly CancellationToken _connectionListenerCancellationToken;
        private readonly Dictionary<int, ConnectionHandle> _connections;
        private readonly DeferredTaskExecutor _deferredTaskExecutor;

        public QuicListener(IPEndPoint ipEndpoint, string certificatePath, string privateKeyPath)
        {
            QuinnApi.Initialize();

            var serverConfig = new ServerConfig(certificatePath, privateKeyPath);
            QuinnApi.CreateServerEndpoint(serverConfig.Handle, out var id, out var handle).Unwrap();

            Id = id;
            Handle = handle;
            QuicSocket = new UdpClient(ipEndpoint);

            _connections = new Dictionary<int, ConnectionHandle>();
            _deferredTaskExecutor = new DeferredTaskExecutor(id => _connections[id]);
            _connectionListenerCancellationToken = new CancellationToken();
            _connectionListener =
                new ConnectionListener(Handle, _connectionListenerCancellationToken, Id, _deferredTaskExecutor);

            EndpointEvents.NewConnection += OnNewConnection;
            EndpointEvents.TransmitReady += OnTransmitReady;
            ConnectionEvents.ConnectionLost += OnConnectionLost;

            StartReceivingAsync();
            _deferredTaskExecutor.StartPollingAsync();
        }


        public event EventHandler<NewConnectionEventArgs> Incoming;
        public event EventHandler<ConnectionIdEventArgs> ConnectionClose;

        /// <summary>
        ///     Asynchronously wait for incoming connections.
        ///     This function should not be called more then once at the same time.
        /// </summary>
        /// <returns>QuicConnection</returns>
        public Task<QuicConnection> AcceptAsync(CancellationToken cancellationToken = new())
        {
            Console.WriteLine("Listening...");
            return _connectionListener.NextAsync(cancellationToken);
        }

        /// <summary>
        ///     Block wait for incoming connections.
        ///     This function should not be called more then once at the same time.
        /// </summary>
        /// <returns>QuicConnection</returns>
        public QuicConnection Accept()
        {
            Console.WriteLine("Listening...");
            return AcceptAsync(CancellationToken.None).Result;
        }

        /// <summary>
        ///     Returns the connection handle for a given connection.
        ///     This handle is a direct pointer into rust and should be treated with care.
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns>ConnectionHandle</returns>
        public ConnectionHandle ConnectionHandle(int connectionId)
        {
            return _connections[connectionId];
        }

        private void OnTransmitReady(object sender, TransmitEventArgs e)
        {
            if (!IsThisEndpoint(e.Id)) return;

            // TODO: maybe don't send immediately when data is transmit ready. 
            if (Id == e.Id)
                QuicSocket.Send(e.TransmitPacket.Contents, e.TransmitPacket.Contents.Length,
                    e.TransmitPacket.Destination);
        }

        private void OnConnectionLost(object? sender, ConnectionIdEventArgs e)
        {
            var handle = _connections[e.Id];
            if (_connections.Remove(e.Id))
                _deferredTaskExecutor.Schedule(() =>
                {
                    QuinnApi.FreeConnection(Handle, handle);
                    ConnectionClose?.Invoke(null, e);
                });
        }

        private void OnNewConnection(object? sender, NewConnectionEventArgs e)
        {
            if (!IsThisEndpoint(e.EndpointId)) return;

            _connections[e.ConnectionId] = e.ConnectionHandle;
            Incoming?.Invoke(null, e);
        }
    }
}