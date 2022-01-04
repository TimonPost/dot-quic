using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Quic.Implementation;
using Quic.Native;

namespace Quic.Runner
{
    internal class Program
    {
        private static readonly IPEndPoint serverIp = new(IPAddress.Parse("127.0.0.1"), 5000);


        private static async Task Main(string[] args)
        {
            var server = new QuicListener(serverIp);

            QuinnApi.Initialize();

            var connection = await server.AcceptIncomingAsync();
            connection.DataReceived += OnDataReceive;

            var stream = connection.OpenBiDirectionalStream();

            while (true)
            {
                server.PollEvents();

                Thread.Sleep(30);

                stream.Write(new byte[] { 0, 1, 2, 3 });
            }

            //var clientIp = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1234);


            // QuicStream stream1 = connection.OpenUniDirectionalStream();


            Console.ReadKey();
        }

        private static void OnDataReceive(object? sender, DataReceivedEventArgs e)
        {
            var buffer = new byte[1024];

            if (e.Stream.CanRead)
            {
                e.Stream.Read(buffer);

                Console.WriteLine("Received {0}", Encoding.UTF8.GetString(buffer));

                Thread.Sleep(2000);

                e.Stream.Write(new ReadOnlySpan<byte>(new byte[] { 1, 2, 3 }));
            }
        }
    }
}