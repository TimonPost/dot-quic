using System;
using System.IO;
using System.Text;
using DotQuic.Native.Events;
using DotQuic.Native.Handles;
using DotQuic.Native.Types;

namespace DotQuic.Native
{
    public delegate void OnConnected(int connectionId);

    public delegate void OnConnectionLost(int connectionId);

    public delegate void OnConnectionClose(int connectionId, long errorCode, IntPtr reasonBytes, int reasonBytesLength);

    public delegate void OnDatagramReceived(int connectionId);

    public delegate void OnNewConnection(IntPtr handle, int connectionId, int endpointId);

    public delegate void OnStreamAvailable(int connectionId, byte streamType);

    public delegate void OnStreamFinished(int connectionId, long streamId, byte direction);

    public delegate void OnStreamOpened(int connectionId, long streamId, byte streamType);

    public delegate void OnStreamReadable(int connectionId, long streamId, byte direction);

    public delegate void OnStreamStopped(int connectionId, long streamId, byte direction);

    public delegate void OnStreamWritable(int connectionId, long streamId, byte direction);

    public delegate void OnTransmit(byte endpointId, IntPtr buffer, IntPtr bufferLength, SockaddrInV4 address);

    public delegate void OnConnectionPollable(int connectionId);


    /// <summary>
    ///     FFI into the QUINN rust QUIC protocol implementation.
    ///     This class calls into the internal class `QuinnApiFFI` which contains all ddl import functions.
    /// </summary>
    public static class QuinnApi
    {
        private static bool _isInitialized;

        public static void Initialize()
        {
            if (!_isInitialized)
            {
                ConnectionEvents.Initialize();
                EndpointEvents.Initialize();
            }

            _isInitialized = true;
        }

        public static void SetLogFilter(string filter)
        {
            unsafe
            {
                var filterBytes = Encoding.UTF8.GetBytes(filter);

                fixed (byte* filterBytesPtr = filterBytes)
                {
                    QuinnApiFFI.enable_log((IntPtr)filterBytesPtr, filterBytes.Length);
                }
            }
        }

        #region Connection

        /// <summary>
        ///     Polls the connection for any events. This function might trigger other callbacks to be called.
        /// </summary>
        /// <remarks>
        ///     Connection handle should be valid pointer for the duration of the call.
        /// </remarks>
        /// >
        public static void ConnectClient(EndpointHandle endpointHandle, string hostName, SockaddrInV4 addr,
            out ConnectionHandle connectionHandle, out int connectionId)
        {
            var hostNameBytes = Encoding.UTF8.GetBytes(hostName);

            unsafe
            {
                endpointHandle.Acquire();
                fixed (byte* hostNamePtr = hostNameBytes)
                {
                    QuinnApiFFI.connect_client(endpointHandle, (IntPtr)hostNamePtr, hostNameBytes.Length, addr,
                        out connectionHandle, out connectionId).Unwrap();
                }

                endpointHandle.Release();
            }
        }

        /// <summary>
        ///     Polls the connection for any events. This function might trigger other callbacks to be called.
        /// </summary>
        /// <remarks>
        ///     Connection handle should be valid pointer for the duration of the call.
        /// </remarks>
        public static QuinnResult PollConnection(ConnectionHandle connectionHandle)
        {
            connectionHandle.Acquire();
            var result = QuinnApiFFI.poll_connection(connectionHandle);
            connectionHandle.Release();
            return result;
        }

        /// <summary>
        ///     Frees the memory for this connection.
        /// </summary>
        /// <remarks>
        ///     * Connection handle should be valid pointer for the duration of the call.
        ///     * Connection handle should not be used after this call.
        /// </remarks>
        public static QuinnResult FreeConnection(EndpointHandle handle, ConnectionHandle connectionHandle)
        {
            handle.Acquire();
            connectionHandle.Acquire();
            var result = QuinnApiFFI.free_connection(handle, connectionHandle);
            connectionHandle.Release();
            handle.Release();
            return result;
        }

        /// <summary>
        ///     Closes the connection. Connection can not be used
        /// </summary>
        public static void CloseConnection(ConnectionHandle connectionHandle, string reason, long code)
        {
            var reasonBytes = Encoding.UTF8.GetBytes(reason);
            connectionHandle.Acquire();
            unsafe
            {
                fixed (byte* reasonBytesPtr = reasonBytes)
                {
                    QuinnApiFFI.close_connection(connectionHandle, (IntPtr)reasonBytesPtr, reasonBytes.Length, code)
                        .Unwrap();
                }
            }

            connectionHandle.Release();
        }

        #endregion

        #region Configuration

        /// <summary>
        ///     Writes the given buffer into the stream.
        /// </summary>
        /// <returns></returns>
        public static void CreateServerConfig(out ServerConfigHandle serverConfig, string certificatePath,
            string privateKeyPath)
        {
            var certBytes = File.ReadAllBytes(certificatePath);
            var keyBytes = File.ReadAllBytes(privateKeyPath);

            unsafe
            {
                fixed (byte* certBytesPtr = certBytes)
                fixed (byte* keyBytesPtr = keyBytes)
                {
                    QuinnApiFFI.create_server_config(
                        out serverConfig,
                        (IntPtr)certBytesPtr,
                        certBytes.Length,
                        (IntPtr)keyBytesPtr,
                        keyBytes.Length
                    ).Unwrap();
                }
            }
        }

        public static void CreateClientConfig(out ClientConfigHandle clientConfig, string certificatePath,
            string privateKeyPath)
        {
            var certBytes = File.ReadAllBytes(certificatePath);
            var keyBytes = File.ReadAllBytes(privateKeyPath);

            unsafe
            {
                fixed (byte* certBytesPtr = certBytes)
                fixed (byte* keyBytesPtr = keyBytes)
                {
                    QuinnApiFFI.create_client_config(
                        out clientConfig,
                        (IntPtr)certBytesPtr,
                        certBytes.Length,
                        (IntPtr)keyBytesPtr,
                        keyBytes.Length
                    ).Unwrap();
                }
            }
        }

        /// <summary>
        ///     Returns the last thrown error by the protocol.
        /// </summary>
        /// <remarks>
        ///     An error will be returned if the error does not fit in the given buffer.
        ///     In that case the actual error size is passed in `actualMessageLenght`
        /// </remarks>
        /// >
        public static QuinnResult LastError(
            IntPtr messageBuf,
            UIntPtr messageBufLen,
            out UIntPtr actualMessageLen)
        {
            return QuinnApiFFI.last_error(messageBuf, messageBufLen, out actualMessageLen);
        }

        #endregion

        #region SetCallbacks

        /// <summary>
        ///     Sets the callback function for when new connections are fully initialized.
        /// </summary>
        /// <remarks>
        ///     Only one callback can be set. Make sure that the passed delegate will never be garbage collected.
        /// </remarks>
        /// >
        public static QuinnResult SetOnNewConnection(OnNewConnection callback)
        {
            return QuinnApiFFI.set_on_new_connection(callback);
        }

        /// <summary>
        ///     Sets the callback function for when new connections are incoming.
        /// </summary>
        /// <remarks>
        ///     Only one callback can be set. Make sure that the passed delegate will never be garbage collected.
        /// </remarks>
        /// >
        public static QuinnResult SetOnConnected(OnConnected callback)
        {
            return QuinnApiFFI.set_on_connected(callback);
        }

        /// <summary>
        ///     Sets the callback function for when connections are lost.
        /// </summary>
        /// <remarks>
        ///     Only one callback can be set. Make sure that the passed delegate will never be garbage collected.
        /// </remarks>
        /// >
        public static QuinnResult SetOnConnectionLost(OnConnectionLost callback)
        {
            return QuinnApiFFI.set_on_connection_lost(callback);
        }

        /// <summary>
        ///     Sets the callback function for when a stream becomes writable.
        /// </summary>
        /// <remarks>
        ///     Only one callback can be set. Make sure that the passed delegate will never be garbage collected.
        /// </remarks>
        /// >
        public static QuinnResult SetOnStreamWritable(OnStreamWritable callback)
        {
            return QuinnApiFFI.set_on_stream_writable(callback);
        }

        /// <summary>
        ///     Sets the callback function for when a stream becomes readable.
        /// </summary>
        /// <remarks>
        ///     Only one callback can be set. Make sure that the passed delegate will never be garbage collected.
        /// </remarks>
        /// >
        public static QuinnResult SetOnStreamReadable(OnStreamReadable callback)
        {
            return QuinnApiFFI.set_on_stream_readable(callback);
        }

        /// <summary>
        ///     Sets the callback function for when a stream is finished.
        /// </summary>
        /// <remarks>
        ///     Only one callback can be set. Make sure that the passed delegate will never be garbage collected.
        /// </remarks>
        /// >
        public static QuinnResult SetOnStreamFinished(OnStreamFinished callback)
        {
            return QuinnApiFFI.set_on_stream_finished(callback);
        }

        /// <summary>
        ///     Sets the callback function for when a stream is stopped.
        /// </summary>
        /// <remarks>
        ///     Only one callback can be set. Make sure that the passed delegate will never be garbage collected.
        /// </remarks>
        /// >
        public static QuinnResult SetOnStreamStopped(OnStreamStopped callback)
        {
            return QuinnApiFFI.set_on_stream_stopped(callback);
        }

        /// <summary>
        ///     Sets the callback function for when a datagram is received.
        /// </summary>
        /// <remarks>
        ///     Only one callback can be set. Make sure that the passed delegate will never be garbage collected.
        /// </remarks>
        /// >
        public static QuinnResult SetOnDatagramReceived(OnDatagramReceived callback)
        {
            return QuinnApiFFI.set_on_datagram_received(callback);
        }

        /// <summary>
        ///     Sets the callback function for when a stream is opened.
        /// </summary>
        /// <remarks>
        ///     Only one callback can be set. Make sure that the passed delegate will never be garbage collected.
        ///     A stream is readable when first opened!
        /// </remarks>
        /// >
        public static QuinnResult SetOnStreamOpened(OnStreamOpened callback)
        {
            return QuinnApiFFI.set_on_stream_opened(callback);
        }

        /// <summary>
        ///     Sets the callback function for when a stream becomes available.
        /// </summary>
        /// <remarks>
        ///     Only one callback can be set. Make sure that the passed delegate will never be garbage collected.
        /// </remarks>
        /// >
        public static QuinnResult SetOnStreamAvailable(OnStreamAvailable callback)
        {
            return QuinnApiFFI.set_on_stream_available(callback);
        }

        /// <summary>
        ///     Sets the callback function for when a transmit is ready.
        /// </summary>
        /// <remarks>
        ///     Only one callback can be set. Make sure that the passed delegate will never be garbage collected.
        /// </remarks>
        /// >
        public static QuinnResult SetOnTransmit(OnTransmit callback)
        {
            return QuinnApiFFI.set_on_transmit(callback);
        }

        /// <summary>
        ///     Sets the callback function for when a connection can be polled.
        /// </summary>
        /// <remarks>
        ///     Only one callback can be set. Make sure that the passed delegate will never be garbage collected.
        /// </remarks>
        /// >
        public static QuinnResult SetOnPollableConnection(OnConnectionPollable callback)
        {
            return QuinnApiFFI.set_on_pollable_connection(callback);
        }

        #endregion

        #region Endpoint

        /// <summary>
        ///     Polls the endpoint for any events. This function might trigger other callbacks to be called.
        /// </summary>
        /// <remarks>
        ///     Endpoint handle should be valid pointer for the duration of the call.
        /// </remarks>
        /// >
        public static QuinnResult PollEndpoint(EndpointHandle endpointHandle)
        {
            endpointHandle.Acquire();
            var result = QuinnApiFFI.poll_endpoint(endpointHandle);
            endpointHandle.Release();
            return result;
        }

        /// <summary>
        ///     Creates a server endpoint with the given server config.
        /// </summary>
        /// <remarks>
        ///     The server config handle is valid for the duration of the call.
        /// </remarks>
        public static QuinnResult CreateServerEndpoint(ServerConfigHandle serverConfig, out byte endpointId,
            out EndpointHandle endpointHandle)
        {
            serverConfig.Acquire();
            var result = QuinnApiFFI.create_server_endpoint(serverConfig, out endpointId, out endpointHandle);
            serverConfig.Release();
            return result;
        }

        /// <summary>
        ///     Creates a client endpoint with the given client config.
        /// </summary>
        /// <remarks>
        ///     The client config handle is valid for the duration of the call.
        /// </remarks>
        public static QuinnResult CreateClientEndpoint(ClientConfigHandle clientConfig, out byte endpointId,
            out EndpointHandle endpointHandle)
        {
            clientConfig.Acquire();
            var result = QuinnApiFFI.create_client_endpoint(clientConfig, out endpointId, out endpointHandle);
            clientConfig.Release();
            return result;
        }

        #endregion

        #region Data

        /// <summary>
        ///     Reads data from a stream with the given id, into the given buffer.
        /// </summary>
        /// <remarks>
        ///     The connection handle is valid for the duration of the call.
        ///     Buffer should be fixed and should not be deallocated.
        /// </remarks>
        /// >
        public static QuinnResult ReadStream(ConnectionHandle handle, long streamId, IntPtr bufferPtr,
            uint bufferLength, out uint actualLength)
        {
            handle.Acquire();
            var result = QuinnApiFFI.read_stream(handle, streamId, bufferPtr, bufferLength, out actualLength);
            handle.Release();
            return result;
        }

        /// <summary>
        ///     Writes the given data to a stream with the given id.
        /// </summary>
        /// <remarks>
        ///     The connection handle is valid for the duration of the call.
        ///     Buffer should be fixed and should not be deallocated.
        /// </remarks>
        /// >
        public static QuinnResult WriteStream(ConnectionHandle handle, long streamId, IntPtr bufferPtr,
            uint bufferLength, out uint bytesWritten)
        {
            handle.Acquire();
            var result = QuinnApiFFI.write_stream(handle, streamId, bufferPtr, bufferLength, out bytesWritten);
            handle.Release();
            return result;
        }

        /// <summary>
        ///     Accepts an incoming stream and returns its stream id.
        /// </summary>
        /// <remarks>
        ///     The connection handle is valid for the duration of the call.
        /// </remarks>
        /// >
        public static QuinnResult
            AcceptStream(ConnectionHandle handle, byte streamDirection, out long streamId)
        {
            handle.Acquire();
            var result = QuinnApiFFI.accept_stream(handle, streamDirection, out streamId);
            handle.Release();
            return result;
        }

        /// <summary>
        ///     Opens a stream of the given directionality.
        /// </summary>
        /// <remarks>
        ///     The connection handle is valid for the duration of the call.
        /// </remarks>
        /// >
        public static QuinnResult OpenStream(ConnectionHandle connectionHandle, StreamType streamType,
            out long openedStreamId)
        {
            connectionHandle.Acquire();
            var result = QuinnApiFFI.open_stream(connectionHandle, streamType, out openedStreamId);
            connectionHandle.Release();
            return result;
        }

        /// <summary>
        ///     Processes an incoming QUIC data packet.
        /// </summary>
        /// <remarks>
        ///     The connection handle is valid for the duration of the call.
        ///     Buffer should be fixed and should not be deallocated.
        /// </remarks>
        /// >
        public static QuinnResult HandleDatagram(EndpointHandle handle, IntPtr buffer, UIntPtr length,
            SockaddrInV4 sockaddrInV4)
        {
            handle.Acquire();
            var result = QuinnApiFFI.handle_datagram(handle, buffer, length, sockaddrInV4);
            handle.Release();
            return result;
        }

        #endregion
    }
}