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
    /// <summary>
    ///  Listener for connections from QUIC protocol clients.
    /// </summary>
    public class QuicListener : Endpoint
    {
        private readonly ConnectionListener _connectionListener;
        private readonly CancellationToken _connectionListenerCancellationToken;
        private readonly Dictionary<int, ConnectionHandle> _connections;
        private readonly DeferredTaskExecutor _deferredTaskExecutor;

        /// <summary>
        /// Creates a QUIC Connection Listener.
        /// </summary>
        /// <remarks>
        /// The default server configuration contains:
        /// * only high-quality cipher suites: TLS13_AES_256_GCM_SHA384, TLS13_AES_128_GCM_SHA256, TLS13_CHACHA20_POLY1305_SHA256.
        /// * only high-quality key exchange groups: curve25519, secp256r1, secp384r1.
        /// * only TLS 1.2 and 1.3 support.
        ///
        /// For more instructions read the README, configuration section.
        /// </remarks>
        /// <param name="listenerIp">The ipv4 address of this server.</param>
        /// <param name="certificatePath">The certificate must be DER-encoded X.509.</param>
        /// <param name="privateKeyPath">The private key must be DER-encoded ASN.1 in either PKCS#8 or PKCS#1 format.</param>
        public QuicListener(IPEndPoint listenerIp, string certificatePath, string privateKeyPath)
        {
            QuinnApi.Initialize();

            var serverConfig = new ServerConfig(certificatePath, privateKeyPath);
            QuinnApi.CreateServerEndpoint(serverConfig.Handle, out var id, out var handle).Unwrap();

            Id = id;
            Handle = handle;
            QuicSocket = new UdpClient(listenerIp);

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

        /// <summary>
        /// Event that is triggered when a new connection is initialized, and ready to be used. 
        /// </summary>
        public event EventHandler<NewConnectionEventArgs> Incoming;

        /// <summary>
        /// Event that is triggered when a connection is closed.
        /// </summary>
        public event EventHandler<ConnectionIdEventArgs> ConnectionClose;

        /// <summary>
        ///     Asynchronously wait for incoming connections.
        /// </summary>
        /// <remarks>This function should not be called more then once at the same time.</remarks>
        /// <returns cref="QuicConnection">QuicConnection</returns>
        public Task<QuicConnection> AcceptAsync(CancellationToken cancellationToken = new())
        {
            return _connectionListener.NextAsync(cancellationToken);
        }

        /// <summary>
        ///     Synchronously wait for incoming connections.     
        /// </summary>
        /// <remarks>This function should not be called more then once at the same time.</remarks>
        /// <returns cref="QuicConnection">QuicConnection</returns>
        public QuicConnection Accept()
        {
            return AcceptAsync(CancellationToken.None).Result;
        }

        /// <summary>
        ///     Returns the FFI connection handle for a given connection.
        /// </summary>
        /// <remarks>This handle is a direct pointer into rust and should be treated with care.</remarks>
        /// <param name="connectionId">The connection id to fetch the handle from.</param>
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