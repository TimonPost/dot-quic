using System;
using Quic.Native.Events;
using Quic.Native.Handles;
using Quic.Native.Types;

namespace Quic.Native
{
    public delegate void OnConnected(int connectionId);

    public delegate void OnConnectionLost(int connectionId);

    public delegate void OnDatagramReceived(int connectionId);

    public delegate void OnNewConnection(IntPtr handle, int connectionId);

    public delegate void OnStreamAvailable(int connectionId, byte streamType);

    public delegate void OnStreamFinished(int connectionId, long streamId, byte direction);

    public delegate void OnStreamOpened(int connectionId, byte streamType);

    public delegate void OnStreamReadable(int connectionId, long streamId, byte direction);

    public delegate void OnStreamStopped(int connectionId, long streamId, byte direction);

    public delegate void OnStreamWritable(int connectionId, long streamId, byte direction);

    public delegate void OnTransmit(byte endpointId, IntPtr buffer, IntPtr bufferLength, SockaddrInV4 address);

    public delegate void OnConnectionPollable(int connectionId);


    /// <summary>
    /// FFI into the QUINN rust QUIC protocol implementation.
    /// 
    /// This class calls into the internal class `QuinnApiFFI` which contains all ddl import functions.
    /// </summary>
    public static class QuinnApi
    {
        public static void Initialize()
        {
            ConnectionEvents.Initialize();
            EndpointEvents.Initialize();
        }

        #region Connection
        /// <summary>
        ///  Polls the connection for any events. This function might trigger other callbacks to be called.
        /// </summary>
        /// <remarks>
        /// Connection handle should be valid pointer for the duration of the call. 
        /// </remarks>> 
        public static QuinnResult ConnectClient(EndpointHandle endpointHandle, SockaddrInV4 addr, out ConnectionHandle connectionHandle, out int connectionId)
        {
            return QuinnApiFFI.connect_client(endpointHandle, addr, out connectionHandle, out connectionId);
        }

        /// <summary>
        ///  Polls the connection for any events. This function might trigger other callbacks to be called.
        /// </summary>
        /// <remarks>
        /// Connection handle should be valid pointer for the duration of the call. 
        /// </remarks>> 
        public static QuinnResult PollConnection(ConnectionHandle connectionHandle)
        {
            return QuinnApiFFI.poll_connection(connectionHandle);
        }

        #endregion

        #region Configuration

        public static QuinnResult DefaultServerConfig(out ServerConfigHandle serverConfig)
        {
            return QuinnApiFFI.default_server_config(out serverConfig);
        }

       public static QuinnResult DefaultClientConfig(out ClientConfigHandle clientConfig)
       {
           return QuinnApiFFI.default_client_config(out clientConfig);
       }

       /// <summary>
       ///  Returns the last thrown error by the protocol. 
       /// </summary>
       /// <remarks>
       /// An error will be returned if the error does not fit in the given buffer.
       /// In that case the actual error size is passed in `actualMessageLenght`
       /// </remarks>> 
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
        ///  Sets the callback function for when new connections are fully initialized.
        /// </summary>
        /// <remarks>
        /// Only one callback can be set. Make sure that the passed delegate will never be garbage collected. 
        /// </remarks>> 
        public static QuinnResult SetOnNewConnection(OnNewConnection callback)
        {
            return QuinnApiFFI.set_on_new_connection(callback);
        }

        /// <summary>
        ///  Sets the callback function for when new connections are incoming.
        /// </summary>
        /// <remarks>
        /// Only one callback can be set. Make sure that the passed delegate will never be garbage collected. 
        /// </remarks>> 
        public static QuinnResult SetOnConnected(OnConnected callback)
        {
            return QuinnApiFFI.set_on_connected(callback);
        }

        /// <summary>
        ///  Sets the callback function for when connections are lost.
        /// </summary>
        /// <remarks>
        /// Only one callback can be set. Make sure that the passed delegate will never be garbage collected. 
        /// </remarks>> 
        public static QuinnResult SetOnConnectionLost(OnConnectionLost callback)
        {
            return QuinnApiFFI.set_on_connection_lost(callback);
        }

        /// <summary>
        ///  Sets the callback function for when a stream becomes writable. 
        /// </summary>
        /// <remarks>
        /// Only one callback can be set. Make sure that the passed delegate will never be garbage collected. 
        /// </remarks>> 
        public static QuinnResult SetOnStreamWritable(OnStreamWritable callback)
        {
            return QuinnApiFFI.set_on_stream_writable(callback);
        }

        /// <summary>
        ///  Sets the callback function for when a stream becomes readable. 
        /// </summary>
        /// <remarks>
        /// Only one callback can be set. Make sure that the passed delegate will never be garbage collected. 
        /// </remarks>> 
        public static QuinnResult SetOnStreamReadable(OnStreamReadable callback)
        {
            return QuinnApiFFI.set_on_stream_readable(callback);
        }

        /// <summary>
        ///  Sets the callback function for when a stream is finished. 
        /// </summary>
        /// <remarks>
        /// Only one callback can be set. Make sure that the passed delegate will never be garbage collected. 
        /// </remarks>> 

        public static QuinnResult SetOnStreamFinished(OnStreamFinished callback)
        {
            return QuinnApiFFI.set_on_stream_finished(callback);
        }

        /// <summary>
        ///  Sets the callback function for when a stream is stopped. 
        /// </summary>
        /// <remarks>
        /// Only one callback can be set. Make sure that the passed delegate will never be garbage collected. 
        /// </remarks>> 
        public static QuinnResult SetOnStreamStopped(OnStreamStopped callback)
        {
            return QuinnApiFFI.set_on_stream_stopped(callback);
        }

        /// <summary>
        ///  Sets the callback function for when a datagram is received. 
        /// </summary>
        /// <remarks>
        /// Only one callback can be set. Make sure that the passed delegate will never be garbage collected. 
        /// </remarks>> 
        public static QuinnResult SetOnDatagramReceived(OnDatagramReceived callback)
        {
            return QuinnApiFFI.set_on_datagram_received(callback);
        }

        /// <summary>
        ///  Sets the callback function for when a stream is opened. 
        /// </summary>
        /// <remarks>
        /// Only one callback can be set. Make sure that the passed delegate will never be garbage collected.
        /// A stream is readable when first opened!
        /// </remarks>> 
        public static QuinnResult SetOnStreamOpened(OnStreamOpened callback)
        {
            return QuinnApiFFI.set_on_stream_opened(callback);
        }

        /// <summary>
        ///  Sets the callback function for when a stream becomes available. 
        /// </summary>
        /// <remarks>
        /// Only one callback can be set. Make sure that the passed delegate will never be garbage collected. 
        /// </remarks>>
        public static QuinnResult SetOnStreamAvailable(OnStreamAvailable callback)
        {
            return QuinnApiFFI.set_on_stream_available(callback);
        }

        /// <summary>
        ///  Sets the callback function for when a transmit is ready. 
        /// </summary>
        /// <remarks>
        /// Only one callback can be set. Make sure that the passed delegate will never be garbage collected. 
        /// </remarks>> 
        public static QuinnResult SetOnTransmit(OnTransmit callback)
        {
            return QuinnApiFFI.set_on_transmit(callback);
        }

        /// <summary>
        ///  Sets the callback function for when a connection can be polled. 
        /// </summary>
        /// <remarks>
        /// Only one callback can be set. Make sure that the passed delegate will never be garbage collected. 
        /// </remarks>> 
        public static QuinnResult SetOnPollableConnection(OnConnectionPollable callback)
        {
            return QuinnApiFFI.set_on_pollable_connection(callback);
        }

        #endregion

        #region Endpoint

        /// <summary>
        ///  Polls the endpoint for any events. This function might trigger other callbacks to be called.
        /// </summary>
        /// <remarks>
        /// Endpoint handle should be valid pointer for the duration of the call. 
        /// </remarks>> 
        public static QuinnResult PollEndpoint(EndpointHandle connectionHandle)
        {
            return QuinnApiFFI.poll_endpoint(connectionHandle);
        }

        /// <summary>
        ///  Creates a server endpoint with the given server config. 
        /// </summary>
        /// <remarks>
        /// The server config handle is valid for the duration of the call.
        /// </remarks>> 
        public static QuinnResult CreateServerEndpoint(ServerConfigHandle serverConfig, out byte endpointId,
            out EndpointHandle endpointHandle)
        {
            return QuinnApiFFI.create_server_endpoint(serverConfig, out endpointId, out endpointHandle);
        }

        /// <summary>
        ///  Creates a client endpoint with the given client config. 
        /// </summary>
        /// <remarks>
        /// The client config handle is valid for the duration of the call.
        /// </remarks>> 
        public static QuinnResult CreateClientEndpoint(ClientConfigHandle clientConfig, out byte endpointId,
            out EndpointHandle endpointHandle)
        {
            return QuinnApiFFI.create_client_endpoint(clientConfig, out endpointId, out endpointHandle);
        }
        #endregion

        #region Data

        /// <summary>
        ///  Reads data from a stream with the given id, into the given buffer. 
        /// </summary>
        /// <remarks>
        /// The connection handle is valid for the duration of the call.
        /// Buffer should be fixed and should not be deallocated.
        /// </remarks>> 

        public static QuinnResult ReadStream(ConnectionHandle handle, long streamId, IntPtr bufferPtr,
            uint bufferLength, out uint actualLength)
        {
            return QuinnApiFFI.read_stream(handle, streamId, bufferPtr, bufferLength, out actualLength);
        }

        /// <summary>
        ///  Writes the given data to a stream with the given id. 
        /// </summary>
        /// <remarks>
        /// The connection handle is valid for the duration of the call.
        /// Buffer should be fixed and should not be deallocated.
        /// </remarks>> 
        public static QuinnResult WriteStream(ConnectionHandle handle, long streamId, IntPtr bufferPtr,
            uint bufferLength, out uint bytesWritten)
        {
            return QuinnApiFFI.write_stream(handle, streamId, bufferPtr, bufferLength, out bytesWritten);
        }

        /// <summary>
        ///  Accepts an incoming stream and returns its stream id. 
        /// </summary>
        /// <remarks>
        /// The connection handle is valid for the duration of the call.
        /// </remarks>> 
        public static QuinnResult
            AcceptStream(ConnectionHandle handle, byte streamDirection, out long streamId)
        {
            return QuinnApiFFI.AcceptStream(handle, streamDirection, out streamId);
        }

        /// <summary>
        ///  Opens a stream of the given directionality. 
        /// </summary>
        /// <remarks>
        /// The connection handle is valid for the duration of the call.
        /// </remarks>> 
        public static QuinnResult OpenStream(ConnectionHandle connectionHandle, StreamType streamType,
            out long openedStreamId)
        {
            return QuinnApiFFI.open_stream(connectionHandle, streamType, out openedStreamId);
        }

        /// <summary>
        ///  Processes an incoming QUIC data packet. 
        /// </summary>
        /// <remarks>
        /// The connection handle is valid for the duration of the call.
        /// Buffer should be fixed and should not be deallocated.
        /// </remarks>> 
        public static QuinnResult HandleDatagram(EndpointHandle handle, IntPtr buffer, UIntPtr length,
            SockaddrInV4 sockaddrInV4)
        {
            return QuinnApiFFI.handle_datagram(handle, buffer, length, sockaddrInV4);
        }

        #endregion
    }
}