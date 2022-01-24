using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DotQuic;
using DotQuic.Native.Events;

namespace TestApp
{
    internal class Program
    {
        private static readonly IPEndPoint serverIp = new(IPAddress.Parse("127.0.0.1"), 5000);

        public static QuicListener Server;

        private static int _count;
        private static DateTime started;

        private static async Task Main(string[] args)
        {
            Server = new QuicListener(serverIp, "cert.der", "key.der");
            Server.Incoming += OnIncoming;

            Console.ReadKey();
        }

        private static async void OnIncoming(object? sender, NewConnectionEventArgs e)
        {
            // Do something when connection is incoming. 
            var connection = await Server.AcceptAsync();
            connection.DataReceived += OnDataReceive;
            connection.StreamInitiated += OnStreamInitiated;
            connection.StreamClosed += OnStreamClosed;
        }

        private static void OnStreamInitiated(object? sender, StreamEventArgs e)
        {
            // Do something when stream is initiated.
        }

        private static void OnStreamClosed(object? sender, StreamEventArgs e)
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

                if (_count == 4000)
                    Console.WriteLine("Packets per second: {0}", _count / (DateTime.Now - started).Seconds);

                _count++;
            }
        }
    }
}
