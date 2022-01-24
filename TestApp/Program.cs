using System;
using System.Net;
using DotQuic;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            QuicListener listener = new QuicListener(IPEndPoint.Parse("127.0.0.2:5000"), "cert.der", "key.der");
        }
    }
}
