﻿using System;
using System.Net;
using System.Runtime.InteropServices;
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
            QuinnApi.Initialize();

            var server = new QuicListener(serverIp);
            
            var connection = await server.AcceptAsync(new CancellationToken());
            connection.DataReceived += OnDataReceive;

            Console.ReadKey();
        }

        private static int _count = 0;

        private static void OnDataReceive(object? sender, DataReceivedEventArgs e)
        {
            var buffer = new byte[10];

            if (e.Stream.CanRead)
            {
                var read = e.Stream.Read(buffer);

                Console.WriteLine("{0}", Encoding.UTF8.GetString(buffer[..read]));
                var response = new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes($"Ack {_count}"));
                e.Stream.Position = 0;
                if (e.Stream.IsBiStream) e.Stream.Write(response);

                try
                {
                    Console.WriteLine("written: {0}", Encoding.UTF8.GetString(response));
                }
                catch(Exception ex)
                {

                }

                _count++;
            }
        }
    }
}