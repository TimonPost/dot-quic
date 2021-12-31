using System;
using Quick.Native;
using Quick.Native.ApiWrappers;

namespace Quick.Implementation
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