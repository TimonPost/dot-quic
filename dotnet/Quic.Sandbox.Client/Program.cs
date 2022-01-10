using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Quic.Implementation;
using Quic.Native;

namespace Quic.Sandbox.Client
{
    internal class Program
    {
        private static readonly IPEndPoint clientIp = new(IPAddress.Parse("127.0.0.1"), 5001);
        private static readonly IPEndPoint serverIp = new(IPAddress.Parse("127.0.0.1"), 5000);

        private static int _count = 0;

        private static async Task Main(string[] args)
        {
            QuinnApi.Initialize();

            var client = new QuicClient(clientIp);
            
            var connection = await client.ConnectAsync(serverIp, CancellationToken.None);

            var stream = connection.OpenBiDirectionalStream();


            while (true)
            {
                var request = Encoding.UTF8.GetBytes($"Request {_count}");
                stream.Write(request);

                var response = new byte[20];
                var read = await stream.ReadAsync(response);
                Console.WriteLine("{0}", Encoding.UTF8.GetString(response));
                _count++;
            }


            Console.ReadKey();
        }
    }
}
