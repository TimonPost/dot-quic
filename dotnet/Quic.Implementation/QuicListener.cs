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
        private readonly ServerConfig _serverConfig;
        private readonly Dictionary<int, ConnectionHandle> _connections;

        private ManualResetEvent AwaitingConnection = new ManualResetEvent(false);
        private int _lastConnection = 0;


        public QuicListener(IPEndPoint ipEndpoint)
        {
            _serverConfig = new ServerConfig();
            var result = QuinnApi.create_endpoint(_serverConfig.Handle, out byte id, out EndpointHandle handle);
            
            if (result.Erroneous())
                throw new Exception(LastQuinnError.Retrieve().Reason);

            base.Id = id;
            base.Handle = handle;

            _connections = new Dictionary<int, ConnectionHandle>();

            QuicSocket = new QuickSocket(ipEndpoint);
            EndpointEvents.NewConnection += OnNewConnection;
            EndpointEvents.TransmitReady += OnTransmitReady;

            StartReceiving();
        }

        private byte[] buffer;

        private void StartReceiving()
        {
            Console.WriteLine("Receiving...");

            try
            {
                QuicSocket.Socket.BeginReceive(OnReceiveCallback, null);
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        private void OnReceiveCallback(IAsyncResult ar)
        {
            ar.AsyncWaitHandle.WaitOne();
            var receivedBytes = QuicSocket.Socket.EndReceive(ar, ref QuicSocket.LastAddress);
            Console.WriteLine("Processing Incoming...");
            EndpointApi.HandleDatagram(Handle, receivedBytes, QuicSocket.LastAddress);

            StartReceiving();
        }

        private void OnTransmitReady(object sender, TransmitEventArgs e)
        {
            if (Id == e.Id)
            {
                QuicSocket.Send(e.TransmitPacket.Contents, e.TransmitPacket.Destination);
            }
            else
            {

            }
        }

        private void OnNewConnection(object sender, NewConnectionEventArgs e)
        {
            _connections[e.Id] = e.ConnectionHandle;
            _lastConnection = e.Id;
            AwaitingConnection.Set();
        }
        
        public async Task<QuicConnection> AcceptIncomingAsync()
        {
            Console.WriteLine("Listening...");
            await AwaitingConnection.AsTask();
            AwaitingConnection.Reset();
            return new QuicConnection(_connections[_lastConnection], _lastConnection);
        }

        public void PollEvents()
        {
            foreach (var connection in _connections)
            {
                QuinnApi.poll_connection(connection.Value);
            }

            QuinnApi.poll_endpoint(Handle);
        }
    }
}