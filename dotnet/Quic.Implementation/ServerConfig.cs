using System;
using Quic.Native;
using Quic.Native.ApiWrappers;

namespace Quic.Implementation
{
    public class ServerConfig
    {
        public ServerConfig()
        {
            QuinnApi.DefaultServerConfig(out var handle).Unwrap();
            Handle = handle;
        }

        public ServerConfigHandle Handle { get; }
    }

    public class ClientConfig
    {
        public ClientConfig()
        {
            QuinnApi.DefaultClientConfig(out var handle).Unwrap();
            Handle = handle;
        }

        public ClientConfigHandle Handle { get; }
    }
}