using DotQuic.Native;
using DotQuic.Native.Handles;

namespace DotQuic
{
    public class ServerConfig
    {
        public ServerConfig(string certificatePath, string privateKeyPath)
        {
            QuinnApi.CreateServerConfig(out var handle, certificatePath, privateKeyPath);
            Handle = handle;
        }

        public ServerConfigHandle Handle { get; }
    }

    public class ClientConfig
    {
        public ClientConfig(string certificatePath, string privateKeyPath)
        {
            QuinnApi.CreateClientConfig(out var handle, certificatePath, privateKeyPath);
            Handle = handle;
        }

        public ClientConfigHandle Handle { get; }
    }
}