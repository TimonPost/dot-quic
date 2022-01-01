using System;
using Quic.Native;
using Quic.Native.ApiWrappers;

namespace Quic.Implementation
{
    public class ServerConfig
    {
        public ServerConfig()
        {
            var result = QuinnApi.default_server_config(out var handle);
            Handle = handle;

            if (result.Erroneous())
                throw new Exception(LastQuinnError.Retrieve().Reason);
        }

        public ServerConfigHandle Handle { get; }
    }
}