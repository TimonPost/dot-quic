using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using DotQuic.Native;
using DotQuic.Native.Events;

namespace DotQuic
{
    /// <summary>
    /// A Quic Client. 
    /// </summary>
    /// <remarks>A client connects can connect to a `QuicListener`</remarks>
    public class QuicClient : Endpoint
    {
        private DeferredTaskExecutor _deferredTaskExecutor;

        /// <summary>
        /// Creates a QUIC client.
        /// </summary>
        /// <remarks>
        /// The default client configuration contains:
        /// * only high-quality cipher suites: TLS13_AES_256_GCM_SHA384, TLS13_AES_128_GCM_SHA256, TLS13_CHACHA20_POLY1305_SHA256.
        /// * only high-quality key exchange groups: curve25519, secp256r1, secp384r1.
        /// * only TLS 1.2 and 1.3 support.
        ///
        /// For more instructions read the README, configuration section.
        /// </remarks>
        /// <param name="clientIp">The ipv4 address of this client.</param>
        /// <param name="certificatePath">The certificate must be DER-encoded X.509.</param>
        /// <param name="privateKeyPath">The private key must be DER-encoded ASN.1 in either PKCS#8 or PKCS#1 format.</param>
        public QuicClient(IPEndPoint clientIp, string certificatePath, string privateKeyPath)
        {
            QuinnApi.Initialize();

            var clientConfig = new ClientConfig(certificatePath, privateKeyPath);
            QuinnApi.CreateClientEndpoint(clientConfig.Handle, out var id, out var handle).Unwrap();

            Id = id;
            Handle = handle;
            QuicSocket = new UdpClient(clientIp);
            EndpointEvents.TransmitReady += OnTransmitReady;


            StartReceivingAsync();
        }

        /// <summary>
        ///     The underlying QUIC Connection of this client.
        /// </summary>
        /// <remarks>Use the QUIC Connection for interacting with streams. </remarks>
        public QuicConnection Connection { get; private set; }


        /// <summary>
        ///     Connect to the given server ipv4 address.
        /// </summary>
        /// <remarks>This method will block the current thread until connected.</remarks>
        /// <param name="serverIp">The IPV4 address of the server.</param>
        /// <param name="serverName">The server alternative subject name from the certificate.</param>
        /// <returns>QuicConnection</returns>
        public QuicConnection Connect(IPEndPoint serverIp, string serverName)
        {
            return ConnectAsync(serverIp, serverName, CancellationToken.None).Result;
        }

        /// <summary>
        ///     Connect asynchronously to the given server ipv4 address.
        /// </summary>
        /// <param name="serverIp">The IPV4 address of the server.</param>
        /// <param name="serverName">The server alternative subject name from the certificate.</param>
        /// <param name="token">The cancellation token, default infinite time</param>
        /// <returns>QuicConnection</returns>
        public async Task<QuicConnection> ConnectAsync(IPEndPoint serverIp, string serverName,
            CancellationToken token = new())
        {
            var waitEvent = new ManualResetEvent(false);
            ConnectionEvents.ConnectionInitialized += (sender, args) => { waitEvent.Set(); };

            QuinnApi.ConnectClient(Handle, serverName, serverIp.ToNative(), out var connectionHandle,
                out var connectionId);

            _deferredTaskExecutor = new DeferredTaskExecutor(id => connectionHandle);
            _deferredTaskExecutor.StartPollingAsync();

            Connection = new QuicConnection(Handle, connectionHandle, connectionId, _deferredTaskExecutor);

            // Wait until the handhsake is performed. 
            await waitEvent.AsTask(token);

            Connection.SetState(IncomingState.Connected);

            return Connection;
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