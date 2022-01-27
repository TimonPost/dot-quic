using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using DotQuic.Native.Events;
using DotQuic.Native.Handles;

namespace DotQuic
{
    internal class ConnectionListener
    {
        private readonly DeferredTaskExecutor _deferredTaskExecutor;
        private readonly EndpointHandle _endpointHandle;
        private readonly int _endpointId;
        private readonly BufferBlock<IncomingConnection> _incomingConnections;
        private readonly CancellationToken _token;

        public ConnectionListener(EndpointHandle endpointHandle, CancellationToken token, int endpointId,
            DeferredTaskExecutor deferredTaskExecutor)
        {
            _endpointHandle = endpointHandle;
            _token = token;
            _endpointId = endpointId;
            _deferredTaskExecutor = deferredTaskExecutor;
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
            var incoming = new IncomingConnection(_endpointHandle, e.ConnectionHandle, e.ConnectionId,
                _deferredTaskExecutor);
            incoming.ProcessIncoming(_token);

            _incomingConnections.Post(incoming);
        }
    }
}