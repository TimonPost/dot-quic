using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Quic.Native;
using Quic.Native.ApiWrappers;
using Quic.Native.Handles;

namespace Quic.Implementation
{
    public abstract class Endpoint : IDisposable
    {
        private Task _pollTask;
        private IPEndPoint _lastAddress;
        public EndpointHandle Handle { get; protected set; }
        public int Id { get; protected set; }

        public UdpClient QuicSocket { get; protected set; }

        public CancellationTokenSource ReceiveCancellation;
        public CancellationTokenSource PollCancellation;

        public int PollInterval { get; set; } = 100;

        protected Endpoint()
        {
            ReceiveCancellation = new CancellationTokenSource();
            PollCancellation = new CancellationTokenSource();
        }

        protected bool IsThisEndpoint(int id) => Id == id;

        public void StartPollingAsync()
        {
            _pollTask = Task.Run(async () =>
            {
                while (!PollCancellation.IsCancellationRequested)
                {
                    await Task.Delay(PollInterval);
                    QuinnApi.PollEndpoint(Handle);
                }
            });
        }

        public void StartReceivingAsync()
        {
            Console.WriteLine("Receiving...");
            QuicSocket.BeginReceive(OnReceiveCallback, null);
        }


        private void OnReceiveCallback(IAsyncResult ar)
        {
            var receivedBytes = QuicSocket.EndReceive(ar, ref _lastAddress);
            Console.WriteLine("Processing Incoming...");
            QuinnFFIHelpers.HandleDatagram(Handle, receivedBytes, _lastAddress);

            if (!ReceiveCancellation.IsCancellationRequested)
                StartReceivingAsync();
        }

        public void Dispose()
        {
            _pollTask?.Dispose();
            Handle?.Dispose();
            QuicSocket?.Dispose();
        }
    }
}