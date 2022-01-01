using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Quick.Native;
using Quick.Native.ApiWrappers;
using Quick.Native.Events;
using Quick.Native.Handles;

namespace Quick.Implementation
{
    public class QuicListener : Endpoint
    {
        private readonly ServerConfig _serverConfig;
        private readonly Dictionary<int, ConnectionHandle> _connections;

        private SemaphoreSlim _signal = new SemaphoreSlim(0, 1);
        private int _lastConnection = 0;


        public QuicListener(IPEndPoint ipEndpoint)
        {
            _serverConfig = new ServerConfig();
            var result = QuinnApi.create_endpoint(_serverConfig.Handle, out int id, out EndpointHandle handle);
            
            if (result.Erroneous())
                throw new Exception(LastQuinnError.Retrieve().Reason);

            base.Id = id;
            base.Handle = handle;

            _connections = new Dictionary<int, ConnectionHandle>();

            QuicSocket = new QuickSocket(ipEndpoint);
            EndpointEvents.NewConnection += OnNewConnection;
            EndpointEvents.TransmitReady += OnTransmitReady;
        }

        private void OnTransmitReady(object sender, TransmitEventArgs e)
        {
            if (Id == e.Id)
            {
                Console.WriteLine("c#; On Transmit: endpoint: {0} dest: {1}, length: {2}", e.Id,
                    e.TransmitPacket.Destination, e.TransmitPacket.Contents.Length);
            }
        }

        private void OnNewConnection(object sender, NewConnectionEventArgs e)
        {
            _connections[e.Id] = e.ConnectionHandle;
            _lastConnection = e.Id;
            _signal.Release();
        }
        
        public void Recieve()
        {
            Console.WriteLine("Receiving...");
            var buffer = QuicSocket.Receive(out var address);
            Console.WriteLine("Processing Incoming...");
            EndpointApi.HandleDatagram(Handle, buffer, address);
        }

        public async Task<QuicConnection> AcceptIncomingAsync()
        {
            Console.WriteLine("Listening...");

            Recieve();
            await _signal.WaitAsync();
            _signal.Dispose();
            _signal = new SemaphoreSlim(0, 1);
            return new QuicConnection(_connections[_lastConnection], _lastConnection);
        }

        public void Poll()
        {
            foreach (var connection in _connections)
            {
                QuinnApi.poll_connection(connection.Value);
            }

            QuinnApi.poll_endpoint(Handle);
        }
    }
}