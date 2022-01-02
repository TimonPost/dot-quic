using System;
using System.Runtime.InteropServices;
using Quic.Native.Events;
using Quic.Native.Handles;
using Quic.Native.Types;

namespace Quic.Native
{
    public static class QuinnApi
    {
        public static void Initialize()
        {
            ConnectionEvents.Initialize();
            EndpointEvents.Initialize();
        }

        const string NativeLib = "./Native/quinn_ffi.dll";

        [DllImport(NativeLib, EntryPoint = nameof(create_endpoint), ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern QuinnResult create_endpoint(ServerConfigHandle serverConfig, out byte endpointId, out EndpointHandle endpointHandle);

        [DllImport(NativeLib, EntryPoint = nameof(default_server_config), ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern QuinnResult default_server_config(out ServerConfigHandle serverConfig);


        [DllImport(NativeLib, EntryPoint = nameof(last_error), ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern QuinnResult last_error(
            IntPtr messageBuf,
            UIntPtr messageBufLen,
            out UIntPtr actualMessageLen);

        [DllImport(NativeLib, EntryPoint = nameof(poll_transmit), ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern QuinnResult poll_transmit(
            EndpointHandle endpointHandle,
            IntPtr messageBuf,
            UIntPtr messageBufLen,
            out UIntPtr actualMessageLen,
            out SockaddrInV4 destinationAddr
        );

        [DllImport(NativeLib, EntryPoint = nameof(handle_datagram), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern QuinnResult handle_datagram(EndpointHandle handle, IntPtr buffer, UIntPtr length,
            SockaddrInV4 sockaddrInV4);

        [DllImport(NativeLib, EntryPoint = nameof(set_on_new_connection), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern QuinnResult set_on_new_connection(OnNewConnection callback);

        [DllImport(NativeLib, EntryPoint = nameof(set_on_connected), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern QuinnResult set_on_connected(OnConnected callback);
        
        [DllImport(NativeLib, EntryPoint = nameof(set_on_connection_lost), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern QuinnResult set_on_connection_lost(OnConnectionLost callback);

        [DllImport(NativeLib, EntryPoint = nameof(set_on_stream_writable), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern QuinnResult set_on_stream_writable(OnStreamWritable callback);
        
        [DllImport(NativeLib, EntryPoint = nameof(set_on_stream_readable), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern QuinnResult set_on_stream_readable(OnStreamReadable callback);

        [DllImport(NativeLib, EntryPoint = nameof(set_on_stream_finished), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern QuinnResult set_on_stream_finished(OnStreamFinished callback);

        [DllImport(NativeLib, EntryPoint = nameof(set_on_stream_stopped), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern QuinnResult set_on_stream_stopped(OnStreamStopped callback);

        [DllImport(NativeLib, EntryPoint = nameof(on_stream_available), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern QuinnResult on_stream_available(OnStreamAvailable callback);

        [DllImport(NativeLib, EntryPoint = nameof(set_on_datagram_received), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern QuinnResult set_on_datagram_received(OnDatagramReceived callback);

        [DllImport(NativeLib, EntryPoint = nameof(set_on_stream_opened), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern QuinnResult set_on_stream_opened(OnStreamOpened callback);
        
        [DllImport(NativeLib, EntryPoint = nameof(set_on_stream_available), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern QuinnResult set_on_stream_available(OnStreamAvailable callback);

        [DllImport(NativeLib, EntryPoint = nameof(set_on_transmit), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern void set_on_transmit(OnTransmit onTransmit);

        [DllImport(NativeLib, EntryPoint = nameof(trigger_connection), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern void trigger_connection();

        [DllImport(NativeLib, EntryPoint = nameof(get_connection), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern void get_connection(ConnectionHandle handle);

        [DllImport(NativeLib, EntryPoint = nameof(poll_connection), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern void poll_connection(ConnectionHandle connectionHandle);

        [DllImport(NativeLib, EntryPoint = nameof(poll_endpoint), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern void poll_endpoint(EndpointHandle connectionHandle);

        [DllImport(NativeLib, EntryPoint = nameof(read_stream), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern QuinnResult read_stream(ConnectionHandle handle, long streamId, IntPtr bufferPtr, UIntPtr bufferLength, out int actualLength);

        [DllImport(NativeLib, EntryPoint = nameof(accept_stream), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern QuinnResult accept_stream(ConnectionHandle handle, byte streamDirection, out long streamId);


        public delegate void OnNewConnection(IntPtr handle, int connectionId);
        public delegate void OnConnected(int connectionId);
        public delegate void OnConnectionLost(int connectionId);
        public delegate void OnStreamReadable(int connectionId, long streamId, byte direction);
        public delegate void OnStreamWritable(int connectionId, long streamId, byte direction);
        public delegate void OnStreamFinished(int connectionId, long streamId, byte direction);
        public delegate void OnStreamStopped(int connectionId, long streamId, byte direction);
        public delegate void OnStreamAvailable(int connectionId, byte streamType);
        public delegate void OnStreamOpened(int connectionId, byte streamType);
        public delegate void OnDatagramReceived(int connectionId);
        public delegate void OnTransmit(byte endpointId, IntPtr buffer, IntPtr bufferLength, SockaddrInV4 address);

      
    }
}