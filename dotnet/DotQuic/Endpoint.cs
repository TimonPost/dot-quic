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
        public CancellationTokenSource PollCancellation;

        public CancellationTokenSource ReceiveCancellation;

        protected Endpoint()
        {
            ReceiveCancellation = new CancellationTokenSource();
            PollCancellation = new CancellationTokenSource();
        }

        public EndpointHandle Handle { get; protected set; }
        public int Id { get; protected set; }

        public UdpClient QuicSocket { get; protected set; }

        public void Dispose()
        {
            Handle?.Dispose();
            QuicSocket?.Dispose();
        }

        protected bool IsThisEndpoint(int id)
        {
            return Id == id;
        }


        public void StartReceivingAsync()
        {
            QuicSocket.BeginReceive(OnReceiveCallback, null);
        }

        private void OnReceiveCallback(IAsyncResult ar)
        {
            var receivedBytes = QuicSocket.EndReceive(ar, ref _lastAddress);

            try
            {
                if (receivedBytes.Length != 0)
                    QuinnFFIHelpers.HandleDatagram(Handle, receivedBytes, _lastAddress);
            }
            catch (Exception e)
            {
                Console.WriteLine("Handle datagram wrong");
            }

            if (!ReceiveCancellation.IsCancellationRequested)
                StartReceivingAsync();
        }
    }
}