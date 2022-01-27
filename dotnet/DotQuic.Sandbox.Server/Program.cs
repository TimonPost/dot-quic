using System;
using System.Net;
using System.Text;
using DotQuic.Native;
using DotQuic.Native.Events;

namespace DotQuic.Sandbox.Server
{
    internal class Program
    {
        private static readonly IPEndPoint serverIp = new(IPAddress.Parse("127.0.0.1"), 5000);

        public static QuicListener Server;
        private static QuicConnection _clientConnection;
        private static int _count;
        private static DateTime started;

        private static void Main(string[] args)
        {
            Server = new QuicListener(serverIp, "cert.der", "key.der");

            QuinnApi.SetLogFilter("quinn_ffi=trace");
            Server.Incoming += OnIncoming;
            Server.ConnectionClose += OnConnectionClose;

            Console.ReadKey();
        }


        private static async void OnIncoming(object? sender, NewConnectionEventArgs args)
        {
            // Do something when connection is incoming. 
            _clientConnection = await Server.AcceptAsync();
            _clientConnection.DataReceived += OnDataReceive;
            _clientConnection.StreamInitiated += OnStreamInitiated;
            _clientConnection.StreamClosed += OnStreamClosed;
        }

        private static void OnConnectionClose(object? sender, ConnectionIdEventArgs args)
        {
            // Connection is closed   
        }

        private static void OnStreamInitiated(object? sender, StreamEventArgs args)
        {
            // Do something when stream is initiated.
        }

        private static void OnStreamClosed(object? sender, StreamEventArgs args)
        {
            // Do something when stream is closed.
        }

        private static void OnDataReceive(object? sender, DataReceivedEventArgs e)
        {
            if (_count == 0) started = DateTime.Now;

            var buffer = new byte[20];

            if (e.Stream.CanRead)
            {
                var read = e.Stream.Read(buffer);

                Console.WriteLine("{0}", Encoding.UTF8.GetString(buffer[..read]));
                var response = new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes($"Ack {_count}"));

                if (e.Stream.IsBiStream) e.Stream.Write(response);

                if (_count == 200) _clientConnection.Close();

                if (_count == 4000)
                    Console.WriteLine("Packets per second: {0}", _count / (DateTime.Now - started).Seconds);

                _count++;
            }
        }
    }
}