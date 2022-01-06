using System;
using System.Collections.Generic;
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
    struct PollTask
    {
        public int Id { get; set; }
    }

    public class ConnectionDriver : IDisposable
    {
        private readonly QuicListener _serverEndpoint;
        private readonly BufferBlock<PollTask> _pollTasks;
        private readonly CancellationTokenSource Source;
        private readonly Task _connectionPollTask;

        public ConnectionDriver(QuicListener serverEndpoint)
        {
            _serverEndpoint = serverEndpoint;
            _pollTasks = new BufferBlock<PollTask>();
            Source = new CancellationTokenSource();
            
            EndpointEvents.ConnectionPollable += OnConnectionPollable;

            _connectionPollTask = Task.Run(async () =>
            {
                while (!Source.IsCancellationRequested)
                {
                    var task = await _pollTasks.ReceiveAsync(Source.Token);
                    QuinnApi.poll_connection(serverEndpoint.ConnectionHandle(task.Id));
                }
            });
        }
        
        private void OnConnectionPollable(object? sender, ConnectionIdEventArgs e)
        {
            Console.WriteLine("Connection Pollable");
            _pollTasks.SendAsync(new PollTask() {Id = e.Id});
        }

        public void Dispose()
        {
            Source.Cancel();
            Source.Dispose();
            _connectionPollTask?.Dispose();
        }
    }

    public class QuicListener : Endpoint
    {
        private readonly Dictionary<int, ConnectionHandle> _connections;
        
        private IPEndPoint _lastAddress;
        private int _lastIncomingConnection;

        private readonly ManualResetEvent _awaitingConnection = new(false);

        private ConnectionDriver _connectionDriver;

        public QuicListener(IPEndPoint ipEndpoint)
        {
            var serverConfig = new ServerConfig();
            QuinnApi.create_server_endpoint(serverConfig.Handle, out var id, out var handle).Unwrap();

            Id = id;
            Handle = handle;
            QuicSocket = new UdpClient(ipEndpoint);

            _connections = new Dictionary<int, ConnectionHandle>();
            _connectionDriver = new ConnectionDriver(this);

            EndpointEvents.NewConnection += OnNewConnection;
            EndpointEvents.TransmitReady += OnTransmitReady;
           
            StartReceiving();
        }

        private void StartReceiving()
        {
            Console.WriteLine("Receiving...");
            QuicSocket.BeginReceive(OnReceiveCallback, null);
        }


        private void OnReceiveCallback(IAsyncResult ar)
        {
            ar.AsyncWaitHandle.WaitOne();
            var receivedBytes = QuicSocket.EndReceive(ar, ref _lastAddress);
            Console.WriteLine("Processing Incoming...");
            EndpointApi.HandleDatagram(Handle, receivedBytes, _lastAddress);

            StartReceiving();
        }

        private void OnTransmitReady(object sender, TransmitEventArgs e)
        {
            if (Id == e.Id)
                QuicSocket.Send(e.TransmitPacket.Contents, e.TransmitPacket.Contents.Length,
                    e.TransmitPacket.Destination);
        }

        private void OnNewConnection(object sender, NewConnectionEventArgs e)
        {
            _connections[e.Id] = e.ConnectionHandle;

            _lastIncomingConnection = e.Id;
            _awaitingConnection.Set();
        }

        public async Task<QuicConnection> AcceptIncomingAsync()
        {
            Console.WriteLine("Listening...");
            await _awaitingConnection.AsTask();
            _awaitingConnection.Reset();
            return new QuicConnection(_connections[_lastIncomingConnection], _lastIncomingConnection);
        }

        public ConnectionHandle ConnectionHandle(int connectionId) => _connections[connectionId];
        
        public void PollEvents()
        {
            QuinnApi.poll_endpoint(Handle).Unwrap();

            // foreach (var connectionHandle in _connections)
            // {
            //     try
            //     {
            //         QuinnApi.poll_connection(connectionHandle.Value).Unwrap();
            //     }
            //     catch (Exception e)
            //     {
            //         Console.WriteLine(e);
            //     }
            // }
        }
    }
}