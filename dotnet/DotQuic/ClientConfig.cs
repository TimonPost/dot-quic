using DotQuic.Native;
using DotQuic.Native.Handles;

namespace DotQuic
{
    /// <summary>
    /// Configuration for the client endpoint. 
    /// </summary>
    internal class ClientConfig
    {
        public ClientConfig(string certificatePath, string privateKeyPath)
        {
            QuinnApi.CreateClientConfig(out var handle, certificatePath, privateKeyPath);
            Handle = handle;
        }

        /// <summary>
        /// The FFI handle to the client endpoint configuration.
        /// </summary>
        public ClientConfigHandle Handle { get; }
    }
}