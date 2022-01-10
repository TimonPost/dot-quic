using System;
using System.Threading;
using System.Threading.Tasks;
using Quic.Native;
using Quic.Native.Events;
using Quic.Native.Handles;

namespace Quic.Implementation
{
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
            // Only set awaiting state if current connection and state is listening.
            if (State == IncomingState.Listening && e.Id == _id)
                _awaitingConnection.Set();
        }

        public void ProcessIncoming(CancellationToken cancellationToken)
        {
            State = IncomingState.Listening;
            _processingTask = Task.Run(async () =>
            {
                QuicConnection quicConnection = new QuicConnection(_handle, _id);
                quicConnection.SetState(IncomingState.Connecting);

                // A first poll is sometimes required to get events flowing.
                QuinnApi.PollConnection(_handle);

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