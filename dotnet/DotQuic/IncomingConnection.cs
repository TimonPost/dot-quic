using System;
using System.Threading;
using System.Threading.Tasks;
using DotQuic.Native;
using DotQuic.Native.Events;
using DotQuic.Native.Handles;

namespace DotQuic
{
    /// <summary>
    /// Async processor of an incoming connection. 
    /// </summary>
    internal class IncomingConnection
    {
        private readonly ManualResetEvent _awaitingConnection = new(false);

        private IncomingState State = IncomingState.Listening;

        private readonly int _id;
        private readonly ConnectionDriver _connectionDriver;
        private readonly ConnectionHandle _handle;
        private Task<QuicConnection> _processingTask;

        public IncomingConnection(ConnectionHandle handle, int id, ConnectionDriver connectionDriver)
        {
            _id = id;
            _connectionDriver = connectionDriver;
            _handle = handle;

            ConnectionEvents.ConnectionInitialized += OnConnectionInitialized;
        }

        private void OnConnectionInitialized(object? sender, ConnectionIdEventArgs e)
        {
            Console.WriteLine("{0}", e);
            // Only set awaiting state if current connection and state is listening.
            if (State == IncomingState.Listening && e.Id == _id)
                _awaitingConnection.Set();
        }

        /// <summary>
        /// Starts an asynchronous task for accepting the incoming connection. 
        /// </summary>
        /// <param name="cancellationToken"></param>
        public void ProcessIncoming(CancellationToken cancellationToken)
        {
            State = IncomingState.Listening;
            _processingTask = Task.Run(async () =>
            {
                QuicConnection quicConnection = new QuicConnection(_handle, _id, _connectionDriver);
                quicConnection.SetState(IncomingState.Connecting);

                // A first poll is sometimes required to get events flowing.
                QuinnApi.PollConnection(_handle);

                await _awaitingConnection.AsTask(cancellationToken);
                quicConnection.SetState(IncomingState.Connected);
                
                return quicConnection;
            }, cancellationToken);
        }
        
        /// <summary>
        /// Waits till the connection is initialized.
        /// </summary>
        /// <returns></returns>
        public Task<QuicConnection> WaitAsync()
        {
            return _processingTask;
        }
    }
}