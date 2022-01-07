using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Quic.Native;
using Quic.Native.Events;
using Quic.Native.Handles;

namespace Quic.Implementation
{
    internal struct PollTask
    {
        public PollTask(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }

    /// <summary>
    /// Polls connections if they should be polled. 
    /// </summary>
    internal class ConnectionDriver : IDisposable
    {
        // Dont mutate any state of quic listener. 
        private readonly Func<int, ConnectionHandle> _getConnectionHandle;
        private readonly BufferBlock<PollTask> _pollTasks;
        private readonly CancellationTokenSource Source;
        private Task _connectionPollTask;

        public ConnectionDriver(Func<int, ConnectionHandle> getConnectionHandle)
        {
            _getConnectionHandle = getConnectionHandle;
            _pollTasks = new BufferBlock<PollTask>();
            Source = new CancellationTokenSource();
            
            EndpointEvents.ConnectionPollable += OnConnectionPollable;

        }

        /// Runs a task that polls a connection when it is ready to be polled.
        /// Rust will invoke a callback into C# that triggers the `OnConnectionPollable` method.
        /// Therefore this task will only poll when it is necessarily. 
        public void StartPollingAsync()
        {
            _connectionPollTask = Task.Run(async () =>
            {
                while (!Source.IsCancellationRequested)
                {
                    // Wait for poll task
                    var task = await _pollTasks.ReceiveAsync(Source.Token);
                    QuinnApi.PollConnection(_getConnectionHandle(task.Id));
                }
            });
        }
        
        private void OnConnectionPollable(object? sender, ConnectionIdEventArgs e)
        {
            _pollTasks.SendAsync(new PollTask(e.Id));
        }

        public void Dispose()
        {
            Source.Cancel();
            Source.Dispose();
            _connectionPollTask?.Dispose();
        }
    }
}