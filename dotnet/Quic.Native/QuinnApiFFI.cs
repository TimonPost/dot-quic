using System;
using System.Runtime.InteropServices;
using Quic.Native.Events;
using Quic.Native.Handles;
using Quic.Native.Types;

namespace Quic.Native
{
    /// <summary>
    /// FFI into the QUINN rust QUIC protocol implementation.
    /// This class is internal because C# requires ddl imports to be either private or internal.
    /// Instead this class should be called via `QuinnApi`.
    /// </summary>
    internal static class QuinnApiFFI
    {
        private const string NativeLib = @"./Native/quinn_ffi.dll";
        
        #region Connection

        [DllImport(NativeLib, EntryPoint = nameof(connect_client), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern QuinnResult connect_client(EndpointHandle endpointHandle, SockaddrInV4 addr, out ConnectionHandle connectionHandle, out int connectionId);

        [DllImport(NativeLib, EntryPoint = nameof(poll_connection), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern QuinnResult poll_connection(ConnectionHandle connectionHandle);

        #endregion

        #region Configuration

        [DllImport(NativeLib, EntryPoint = nameof(default_server_config), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern QuinnResult default_server_config(out ServerConfigHandle serverConfig);

        [DllImport(NativeLib, EntryPoint = nameof(default_client_config), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern QuinnResult default_client_config(out ClientConfigHandle clientConfig);

        [DllImport(NativeLib, EntryPoint = nameof(last_error), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern QuinnResult last_error(
            IntPtr messageBuf,
            UIntPtr messageBufLen,
            out UIntPtr actualMessageLen);

        #endregion

        #region SetCallbacks

        [DllImport(NativeLib, EntryPoint = nameof(set_on_new_connection), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern QuinnResult set_on_new_connection(OnNewConnection callback);
        
        [DllImport(NativeLib, EntryPoint = nameof(set_on_connected), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern QuinnResult set_on_connected(OnConnected callback);
        
        [DllImport(NativeLib, EntryPoint = nameof(set_on_connection_lost), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern QuinnResult set_on_connection_lost(OnConnectionLost callback);

        [DllImport(NativeLib, EntryPoint = nameof(set_on_stream_writable), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern QuinnResult set_on_stream_writable(OnStreamWritable callback);

        [DllImport(NativeLib, EntryPoint = nameof(set_on_stream_readable), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern QuinnResult set_on_stream_readable(OnStreamReadable callback);

        [DllImport(NativeLib, EntryPoint = nameof(set_on_stream_finished), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern QuinnResult set_on_stream_finished(OnStreamFinished callback);

 
        [DllImport(NativeLib, EntryPoint = nameof(set_on_stream_stopped), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern QuinnResult set_on_stream_stopped(OnStreamStopped callback);

        [DllImport(NativeLib, EntryPoint = nameof(set_on_datagram_received), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern QuinnResult set_on_datagram_received(OnDatagramReceived callback);

        [DllImport(NativeLib, EntryPoint = nameof(set_on_stream_opened), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern QuinnResult set_on_stream_opened(OnStreamOpened callback);

        [DllImport(NativeLib, EntryPoint = nameof(set_on_stream_available), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern QuinnResult set_on_stream_available(OnStreamAvailable callback);

        [DllImport(NativeLib, EntryPoint = nameof(set_on_transmit), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern QuinnResult set_on_transmit(OnTransmit onTransmit);

        [DllImport(NativeLib, EntryPoint = nameof(set_on_pollable_connection), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern QuinnResult set_on_pollable_connection(OnConnectionPollable onTransmit);

        #endregion

        #region Endpoint

        [DllImport(NativeLib, EntryPoint = nameof(poll_endpoint), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern QuinnResult poll_endpoint(EndpointHandle connectionHandle);

        [DllImport(NativeLib, EntryPoint = nameof(create_server_endpoint), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern QuinnResult create_server_endpoint(ServerConfigHandle serverConfig, out byte endpointId,
            out EndpointHandle endpointHandle);

        [DllImport(NativeLib, EntryPoint = nameof(create_client_endpoint), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern QuinnResult create_client_endpoint(ClientConfigHandle clientConfig, out byte endpointId,
            out EndpointHandle endpointHandle);
        #endregion

        #region Data

        [DllImport(NativeLib, EntryPoint = nameof(read_stream), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern QuinnResult read_stream(ConnectionHandle handle, long streamId, IntPtr bufferPtr,
            uint bufferLength, out uint actualLength);

        [DllImport(NativeLib, EntryPoint = nameof(write_stream), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern QuinnResult write_stream(ConnectionHandle handle, long streamId, IntPtr bufferPtr,
            uint bufferLength, out uint bytesWritten);

        [DllImport(NativeLib, EntryPoint = nameof(accept_stream), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern QuinnResult
            accept_stream(ConnectionHandle handle, byte streamDirection, out long streamId);

        [DllImport(NativeLib, EntryPoint = nameof(open_stream), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern QuinnResult open_stream(ConnectionHandle connectionHandle, StreamType streamType,
            out long openedStreamId);

        [DllImport(NativeLib, EntryPoint = nameof(handle_datagram), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern QuinnResult handle_datagram(EndpointHandle handle, IntPtr buffer, UIntPtr length,
            SockaddrInV4 sockaddrInV4);

        #endregion
    }
}