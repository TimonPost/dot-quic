using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Quic.Native.Events;

namespace Quic.Implementation
{
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
}