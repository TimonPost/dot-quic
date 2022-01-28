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
        internal CancellationTokenSource PollCancellation;

        internal CancellationTokenSource ReceiveCancellation;

        internal Endpoint()
        {
            ReceiveCancellation = new CancellationTokenSource();
            PollCancellation = new CancellationTokenSource();
        }

        /// <summary>
        /// The FFI handle to this endpoint. 
        /// </summary>
        /// <remarks>This handle is a direct pointer into rust and should be treated with care.</remarks>
        public EndpointHandle Handle { get; internal set; }
        public int Id { get; internal set; }

        internal UdpClient QuicSocket { get; set; }

        /// <summary>
        /// Disposes the endpoint and its handles. 
        /// </summary>
        public void Dispose()
        {
            Handle?.Dispose();
            QuicSocket?.Dispose();

            // TODO: dispose endpoint, configuration
        }

        /// <summary>
        /// Returns if the given endpoint id is equal to this endpoint its id.
        /// </summary>
        /// <param name="id">The endpoint id</param>
        /// <returns>If the endpoint id is equal to this endpoint its id.</returns>
        protected internal bool IsThisEndpoint(int id)
        {
            return Id == id;
        }
        
        internal void StartReceivingAsync()
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