using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
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
        private CancellationToken _connectionListenerCancellationToken;

        public QuicListener(IPEndPoint ipEndpoint)
        {
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
            StartPollingAsync();
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

    public enum IncomingState
    {
        Listening,
        Connecting,
        Connected
    }
    

    internal class ConnectionListener
    {
        private readonly CancellationToken _token;
        private readonly int _endpointId;
        private readonly BufferBlock<Incoming> _incomingConnections;

        public ConnectionListener(CancellationToken token, int endpointId)
        {
            _token = token;
            _endpointId = endpointId;
            _incomingConnections = new BufferBlock<Incoming>();
            EndpointEvents.NewConnection += OnNewConnection;
        }

        public async Task<QuicConnection> NextAsync(CancellationToken token)
        {
            Console.WriteLine("Awaiting next...");
            var incoming = await _incomingConnections.ReceiveAsync(token);
            Console.WriteLine("initializing connection...");
            return await incoming.ConnectionInitialized(); // result is set when finished.
        }
        
        private void OnNewConnection(object sender, NewConnectionEventArgs e)
        {
            if (_endpointId != e.EndpointId) return;

            Console.WriteLine("On new connection");
           var incoming = new Incoming(e.ConnectionHandle, e.ConnectionId);
           incoming.ProcessIncoming(_token);
            
           _incomingConnections.Post(incoming);
        }
    }

    internal class Incoming
    {
        private readonly ManualResetEvent _awaitingConnection = new(false);

        private IncomingState State = IncomingState.Listening;

        private readonly int _id;
        private readonly ConnectionHandle _handle;
        private Task<QuicConnection> _processingTask;

        public Incoming(ConnectionHandle handle, int id)
        {
            _id = id;
            _handle = handle;

            ConnectionEvents.ConnectionInitialized += OnConnectionInitialized;
        }

        private void OnConnectionInitialized(object? sender, ConnectionIdEventArgs e)
        {
            Console.WriteLine("On initizlied...");

            // Only set awaiting state if current connection and state is listening.
            if (State == IncomingState.Listening && e.Id == _id)
                _awaitingConnection.Set();
        }

        public void ProcessIncoming(CancellationToken cancellationToken)
        {
            Console.WriteLine("Processing incoming ...");
            State = IncomingState.Listening;
            _processingTask = Task.Run(async () =>
            {
                QuicConnection quicConnection = new QuicConnection(_handle, _id);
                quicConnection.SetState(IncomingState.Connecting);

                await _awaitingConnection.AsTask(cancellationToken)
                    .ContinueWith((_) =>
                    {
                        quicConnection.SetState(IncomingState.Connected);
                        
                    }, cancellationToken);
                
                return quicConnection;
            }, cancellationToken);
        }
        
        public Task<QuicConnection> ConnectionInitialized()
        {
            return _processingTask;
        }
    }
}