using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using DotQuic.Native;
using DotQuic.Native.Handles;

namespace DotQuic
{
    public abstract class Endpoint : IDisposable
    {
        private IPEndPoint _lastAddress;
        public EndpointHandle Handle { get; protected set; }
        public int Id { get; protected set; }

        public UdpClient QuicSocket { get; protected set; }

        public CancellationTokenSource ReceiveCancellation;
        public CancellationTokenSource PollCancellation;
        
        protected Endpoint()
        {
            ReceiveCancellation = new CancellationTokenSource();
            PollCancellation = new CancellationTokenSource();
        }

        protected bool IsThisEndpoint(int id) => Id == id;
        

        public void StartReceivingAsync()
        {
            QuicSocket.BeginReceive(OnReceiveCallback, null);
        }


        private void OnReceiveCallback(IAsyncResult ar)
        {
            var receivedBytes = QuicSocket.EndReceive(ar, ref _lastAddress);
            QuinnFFIHelpers.HandleDatagram(Handle, receivedBytes, _lastAddress);

            if (!ReceiveCancellation.IsCancellationRequested)
                StartReceivingAsync();
        }

        public void Dispose()
        {
            Handle?.Dispose();
            QuicSocket?.Dispose();
        }
    }
}