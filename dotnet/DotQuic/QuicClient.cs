using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using DotQuic.Native;
using DotQuic.Native.Events;

namespace DotQuic
{
    public class QuicClient : Endpoint
    {
        private DeferredTaskExecutor _deferredTaskExecutor;

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
        ///     The underlying QUIC Connection.
        /// </summary>
        public QuicConnection Connection { get; private set; }


        /// <summary>
        ///     Connect to the given server ip.
        ///     This method will block the current thread until connected.
        /// </summary>
        /// <returns>QuicConnection</returns>
        public QuicConnection Connect(IPEndPoint serverIp, string serverName)
        {
            return ConnectAsync(serverIp, serverName, CancellationToken.None).Result;
        }

        /// <summary>
        ///     Connect asynchronously to the given server ip.
        /// </summary>
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