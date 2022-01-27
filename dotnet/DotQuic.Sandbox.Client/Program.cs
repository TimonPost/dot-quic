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
        private static readonly IPEndPoint clientIp = new(IPAddress.Parse("127.0.0.1"), 5001);
        private static readonly IPEndPoint serverIp = new(IPAddress.Parse("127.0.0.1"), 5000);

        private static int _count;
        private static QuicConnection _connection;
        private static QuicStream _stream;

        private static async Task Main(string[] args)
        {
            var client = new QuicClient(clientIp, "cert.der",
                "key.der");

            _connection = await client.ConnectAsync(serverIp, "localhost", CancellationToken.None);
            _stream = _connection.OpenBiDirectionalStream();

            QuinnApi.SetLogFilter("quinn_ffi=trace");

            //StartWithEvent();
            await StartWithLoop();

            Console.ReadKey();
        }


        private static async Task StartWithLoop()
        {
            var response = new byte[20];

            while (true)
            {
                var request = Encoding.UTF8.GetBytes($"Request {_count}");
                _stream.Write(request);

                var read = await _stream.ReadAsync(response);
                Console.WriteLine("{0}", Encoding.UTF8.GetString(response));

                _count++;

                if (_count == 200)
                {
                    // _connection.Close();
                    // break;
                }
            }
        }

        private static void StartWithEvent()
        {
            _connection.DataReceived += OnDatagramReceived;
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