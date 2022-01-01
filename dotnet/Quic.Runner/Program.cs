using Quic.Implementation;
using Quic.Native;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Quic.Runner
{
    class Program
    {

        static async Task Main(string[] args)
        {
            QuinnApi.Initialize();
            
            var serverIp = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5000);
            var server = new QuicListener(serverIp);

            QuicConnection connection = await server.AcceptIncomingAsync();


            server.Poll();

            //var clientIp = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1234);


            // QuicStream stream = connection.OpenBiDirectionalStream();
            // QuicStream stream1 = connection.OpenUniDirectionalStream();
            //
            // var client = new QuickClient(clientIp);
            // client.Connect(serverIp);





            Console.ReadKey();


        }
    }
}
