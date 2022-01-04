using System;
using System.Collections.Generic;
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
    public class QuicListener : Endpoint
    {
        private readonly Dictionary<int, ConnectionHandle> _connections;
        private IPEndPoint _lastAddress;
        private int _lastIncommingConnection;

        private readonly ManualResetEvent AwaitingConnection = new(false);


        public QuicListener(IPEndPoint ipEndpoint)
        {
            var serverConfig = new ServerConfig();
            QuinnApi.create_endpoint(serverConfig.Handle, out var id, out var handle).Unwrap();

            Id = id;
            Handle = handle;
            QuicSocket = new UdpClient(ipEndpoint);

            _connections = new Dictionary<int, ConnectionHandle>();

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
            _lastIncommingConnection = e.Id;
            AwaitingConnection.Set();
        }

        public async Task<QuicConnection> AcceptIncomingAsync()
        {
            Console.WriteLine("Listening...");
            await AwaitingConnection.AsTask();
            AwaitingConnection.Reset();
            return new QuicConnection(_connections[_lastIncommingConnection], _lastIncommingConnection);
        }

        public void PollEvents()
        {
            foreach (var connection in _connections) QuinnApi.poll_connection(connection.Value).Unwrap();

            QuinnApi.poll_endpoint(Handle).Unwrap();
        }
    }
}