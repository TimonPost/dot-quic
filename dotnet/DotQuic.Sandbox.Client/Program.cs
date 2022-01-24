using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotQuic.Native;

namespace DotQuic.Sandbox.Client
{
    internal class Program
    {
        private static readonly IPEndPoint clientIp = new(IPAddress.Parse("172.21.120.67"), 5001);
        private static readonly IPEndPoint serverIp = new(IPAddress.Parse("192.168.0.114"), 5000);

        private static int _count;

        private static async Task Main(string[] args)
        {
            var client = new QuicClient(clientIp, "cert.der",
                "key.der");
            

            var connection = await client.ConnectAsync(serverIp, "localhost", CancellationToken.None);

            //connection.DataReceived += OnDatagramReceived;

            var _stream = connection.OpenBiDirectionalStream();

            var response = new byte[20];

            while (true)
            {
                var request = Encoding.UTF8.GetBytes($"Request {_count}");
                _stream.Write(request);

                var read = await _stream.ReadAsync(response);
                Console.WriteLine("{0}", Encoding.UTF8.GetString(response));

                _count++;
            }

            Console.ReadKey();
        }

        private static void OnDatagramReceived(object? sender, DataReceivedEventArgs e)
        {
            var response = new byte[20];
           
            try
            {
                var read = e.Stream.Read(response);
                Console.WriteLine("{0}", Encoding.UTF8.GetString(response));

                var request = Encoding.UTF8.GetBytes($"Request {_count}");
                e.Stream.Write(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine(e);
            }

            _count++;
        }
    }
}