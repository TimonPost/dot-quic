using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Quic.Native;
using Quic.Native.Events;
using Quic.Native.Handles;

namespace Quic.Implementation
{
    public class QuicClient : Endpoint
    {
        private readonly ManualResetEvent _awaitingConnection = new(false);
        private int _connectionid;
        private ConnectionDriver _connectionDriver;

        public QuicClient(IPEndPoint clientIp)
        {
            var clientConfig = new ClientConfig();
            QuinnApi.CreateClientEndpoint(clientConfig.Handle, out var id, out var handle).Unwrap();

            Id = id;
            Handle = handle;
            QuicSocket = new UdpClient(clientIp);
            EndpointEvents.TransmitReady += OnTransmitReady;
            ConnectionEvents.ConnectionInitialized += OnNewConnection;
            StartReceivingAsync();
            StartPollingAsync();
        }

        private void OnNewConnection(object? sender, ConnectionIdEventArgs e)
        {
            Console.WriteLine("OnNewConnection");

            _connectionid = e.Id;

            // Set reset event for `AcceptIncomingAsync`. 
            _awaitingConnection.Set();
        }

        private void OnTransmitReady(object? sender, TransmitEventArgs e)
        {
            if (!IsThisEndpoint(e.Id)) return;
            Console.WriteLine("OnTransmit");
            // TODO: maybe don't send immediately when data is transmit ready. 
            if (Id == e.Id)
                QuicSocket.Send(e.TransmitPacket.Contents, e.TransmitPacket.Contents.Length,
                    e.TransmitPacket.Destination);
        }

        public QuicConnection Connect(IPEndPoint serverIp)
        {
            Console.WriteLine("Connecting ...");
            QuinnApi.ConnectClient(Handle, serverIp.ToNative(), out ConnectionHandle connectionHandle, out int connectionId).Unwrap();
            var newConnection = new QuicConnection(connectionHandle, connectionId);
            _connectionDriver = new ConnectionDriver((id => connectionHandle));
            _connectionDriver.StartPollingAsync();
            _awaitingConnection.WaitOne();
            Console.WriteLine("Connected");

            return newConnection;

        }
    }
}