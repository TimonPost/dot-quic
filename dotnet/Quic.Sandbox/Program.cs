using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;
using QuickNet.Utilities;
using QuicNet;
using QuicNet.Connections;
using QuicNet.Infrastructure.Frames;

namespace Quic.Sandbox
{
    class Program
    {
        static MediaFile file = new MediaFile("C:\\Users\\Timon\\Downloads\\test\\video1.mp4");
        static MediaFile outputFile = new MediaFile("test.jpg");

        private static QuicConnection ClientConnection;
        private static QuicConnection ServerConnection;

        static QuicConnection ListenServer()
        {
            Console.WriteLine("Listening...");
            QuicListener server = new QuicListener(11000);
            server.Start();

            var awaitClient = server.AcceptQuicClient();
            Console.WriteLine("Accepted Client ...");

            return awaitClient;
        }

        static  QuicConnection StartClient()
        {
            var client = new QuicClient();
            var makeConnection  = client.Connect("127.0.0.1", 11000);
            Console.WriteLine("Connected to server");
            return makeConnection;
        }

        static void Main(string[] args)
        {
            Thread serverThread = new Thread(() =>
            {
                ClientConnection = ListenServer();

                var stream = ClientConnection.CreateStream(StreamType.ServerBidirectional);

                var time = DateTime.Now;

                using (var engine = new Engine("C:\\Users\\Timon\\Downloads\\test\\ffmpeg.exe"))
                {
                    int counter = 0;
                    while (true)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(1));

                        engine.GetMetadata(file);

                        var options = new ConversionOptions { Seek = TimeSpan.FromSeconds(counter) };
                        engine.GetThumbnail(file, outputFile, options);

                        var bytes = File.ReadAllBytes(outputFile.Filename);
                        stream.Send(bytes);
                        
                        counter++;
                    }
                }
            });


            serverThread.Start();

            Thread clientThread = new Thread(() =>
            {
                ServerConnection = StartClient();

                var frames = new List<Frame>();

                ServerConnection.OnDataReceived += quicStream =>
                {
                    var imageData = quicStream.Data;

                    Console.WriteLine("Client received {0} bytes", imageData.Length);
                };

                while (true)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(500));
                    ServerConnection.ProcessFrames(frames);
                }
            });

            clientThread.Start();

            serverThread.Join();
            clientThread.Join();
        }
    }
}
