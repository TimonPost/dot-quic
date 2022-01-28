using DotQuic.Native;
using DotQuic.Native.Handles;

namespace DotQuic
{
    /// <summary>
    /// The FFI handle to the server endpoint configuration.
    /// </summary>
    internal class ServerConfig
    {
        public ServerConfig(string certificatePath, string privateKeyPath)
        {
            QuinnApi.CreateServerConfig(out var handle, certificatePath, privateKeyPath);
            Handle = handle;
        }

        /// <summary>
        /// The FFI handle to the client endpoint configuration.
        /// </summary>
        public ServerConfigHandle Handle { get; }
    }
}