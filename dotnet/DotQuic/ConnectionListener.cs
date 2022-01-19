using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using DotQuic.Native.Events;

namespace DotQuic
{
    internal class ConnectionListener
    {
        private readonly ConnectionDriver _connectionDriver;
        private readonly int _endpointId;
        private readonly BufferBlock<IncomingConnection> _incomingConnections;
        private readonly CancellationToken _token;

        public ConnectionListener(CancellationToken token, int endpointId, ConnectionDriver connectionDriver)
        {
            _token = token;
            _endpointId = endpointId;
            _connectionDriver = connectionDriver;
            _incomingConnections = new BufferBlock<IncomingConnection>();
            EndpointEvents.NewConnection += OnNewConnection;
        }

        public async Task<QuicConnection> NextAsync(CancellationToken token)
        {
            Console.WriteLine("Awaiting next...");
            var incoming = await _incomingConnections.ReceiveAsync(token);
            Console.WriteLine("initializing connection...");
            return await incoming.WaitAsync(); // result is set when finished.
        }

        private void OnNewConnection(object sender, NewConnectionEventArgs e)
        {
            if (_endpointId != e.EndpointId) return;

            Console.WriteLine("On new connection");
            var incoming = new IncomingConnection(e.ConnectionHandle, e.ConnectionId, _connectionDriver);
            incoming.ProcessIncoming(_token);

            _incomingConnections.Post(incoming);
        }
    }
}