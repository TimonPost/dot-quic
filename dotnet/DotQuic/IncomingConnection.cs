using System.Threading;
using System.Threading.Tasks;
using DotQuic.Native.Events;
using DotQuic.Native.Handles;

namespace DotQuic
{
    /// <summary>
    ///     Async processor of an incoming connection.
    /// </summary>
    internal class IncomingConnection
    {
        private readonly ManualResetEvent _awaitingConnection = new(false);
        private readonly DeferredTaskExecutor _deferredTaskExecutor;

        private readonly EndpointHandle _endpointHandle;
        private readonly ConnectionHandle _handle;
        private readonly int _id;
        private Task<QuicConnection> _processingTask;

        private IncomingState State = IncomingState.Listening;

        public IncomingConnection(EndpointHandle endpointHandle, ConnectionHandle handle, int id,
            DeferredTaskExecutor deferredTaskExecutor)
        {
            _endpointHandle = endpointHandle;
            _id = id;
            _deferredTaskExecutor = deferredTaskExecutor;
            _handle = handle;

            ConnectionEvents.ConnectionInitialized += OnConnectionInitialized;
        }

        private void OnConnectionInitialized(object? sender, ConnectionIdEventArgs e)
        {
            // Only set awaiting state if current connection and state is listening.
            if (State == IncomingState.Listening && e.Id == _id)
                _awaitingConnection.Set();
        }

        /// <summary>
        ///     Starts an asynchronous task for accepting the incoming connection.
        /// </summary>
        /// <param name="cancellationToken"></param>
        public void ProcessIncoming(CancellationToken cancellationToken)
        {
            State = IncomingState.Listening;
            _processingTask = Task.Run(async () =>
            {
                var quicConnection = new QuicConnection(_endpointHandle, _handle, _id, _deferredTaskExecutor);
                quicConnection.SetState(IncomingState.Connecting);

                var source = new CancellationTokenSource();

                // TODO: when auto poll is disabled, we should poll manually. 
                // var task = Task.Run(async () =>
                // {
                //     while (true)
                //     {
                //         await Task.Delay(100, source.Token);
                //         QuinnApi.PollConnection(_handle);
                //     }
                // }, source.Token);

                var task2 = _awaitingConnection.AsTask(cancellationToken);

                Task.WaitAny(task2);
                source.Cancel();

                quicConnection.SetState(IncomingState.Connected);

                return quicConnection;
            }, cancellationToken);
        }

        /// <summary>
        ///     Waits till the connection is initialized.
        /// </summary>
        /// <returns></returns>
        public Task<QuicConnection> WaitAsync()
        {
            return _processingTask;
        }
    }
}