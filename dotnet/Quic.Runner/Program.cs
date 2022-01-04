using Quic.Implementation;
using Quic.Native;
using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Quic.Native.Events;

namespace Quic.Runner
{
    class Program
    {
        static IPEndPoint serverIp = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5000);
       

        static async Task Main(string[] args)
        {
            QuicListener server = new QuicListener(serverIp);

            QuinnApi.Initialize();
            
            QuicConnection connection = await server.AcceptIncomingAsync();
            connection.DataReceived += OnDataReceive;

            while (true)
            {
                server.PollEvents();
                Thread.Sleep(30);
            }

            //var clientIp = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1234);


            // QuicStream stream = connection.OpenBiDirectionalStream();
            // QuicStream stream1 = connection.OpenUniDirectionalStream();
            //
            // var client = new QuickClient(clientIp);
            // client.Connect(serverIp);

            
            Console.ReadKey();
        }

        private static void OnDataReceive(object? sender, DataReceivedEventArgs e)
        {
            var buffer = new byte[1024];

            if (e.Stream.CanRead)
            {
                e.Stream.Read(buffer);

                Console.WriteLine("Received {0}", Encoding.UTF8.GetString(buffer));
            }
        }
    }
}
