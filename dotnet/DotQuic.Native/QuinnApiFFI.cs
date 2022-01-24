using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text.RegularExpressions;
using DotQuic.Native.Handles;
using DotQuic.Native.Types;

namespace DotQuic.Native
{

    /// <summary>
    ///     FFI into the QUINN rust QUIC protocol implementation.
    ///     This class is internal because C# requires ddl imports to be either private or internal.
    ///     Instead this class should be called via `QuinnApi`.
    /// </summary>
    internal static class QuinnApiFFI
    {
        private static IntPtr LibraryHandel = IntPtr.Zero;
        private const string NativeLib = "quinn_ffi";

        static QuinnApiFFI()
        {
            NativeLibrary.SetDllImportResolver(typeof(QuinnApiFFI).Assembly, ImportResolver);
        }

        private static IntPtr ImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (LibraryHandel == IntPtr.Zero && libraryName == NativeLib)
            {
                Regex r = new Regex("quinn_ffi-nightly-.*.(dll|so)");

                try
                {
                    Console.WriteLine("{0}", Directory.GetCurrentDirectory());
                    var files = Directory.GetFiles(Directory.GetCurrentDirectory())
                        .First(path => r.IsMatch(path));

                    NativeLibrary.TryLoad(files, assembly, searchPath, out LibraryHandel);
                }
                catch (InvalidOperationException e)
                {
                    throw new QuinnFFILibraryNotFoundException();
                }
            }

            return LibraryHandel;
        }

        #region Connection

        [DllImport(NativeLib, EntryPoint = nameof(connect_client), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern QuinnResult connect_client(EndpointHandle endpointHandle, IntPtr hostNameBytes,
            int hostNameLength, SockaddrInV4 addr, out ConnectionHandle connectionHandle, out int connectionId);

        [DllImport(NativeLib, EntryPoint = nameof(poll_connection), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        internal static extern QuinnResult poll_connection(ConnectionHandle connectionHandle);

        #endregion

        #region Configuration

        [DllImport(NativeLib, EntryPoint = nameof(enable_log), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern void enable_log(IntPtr logFilterBytes, int bufferLength);


        [DllImport(NativeLib, EntryPoint = nameof(create_test_certificate), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern void create_test_certificate(IntPtr certPathBytesPtr, int length, IntPtr keyPathBytesPtr,
            int keyPathBytesLenght);

        [DllImport(NativeLib, EntryPoint = nameof(create_server_config), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern QuinnResult create_server_config(out ServerConfigHandle serverConfig, IntPtr certPath,
            int certPathLength, IntPtr keyPath, int keyPathLength);

        [DllImport(NativeLib, EntryPoint = nameof(create_client_config), ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern QuinnResult create_client_config(out ClientConfigHandle clientConfig, IntPtr certPath,
            int certPathLength, IntPtr keyPath, int keyPathLength);

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
        [SuppressUnmanagedCodeSecurity]
        internal static extern QuinnResult handle_datagram(EndpointHandle handle, IntPtr buffer, UIntPtr length,
            SockaddrInV4 sockaddrInV4);

        #endregion
    }

    internal class QuinnFFILibraryNotFoundException : Exception
    {
        public override string Message =>
            @"
            The source library that contains the rust interface for QUINN could not be found in the application folder. 

            Make sure:
            1. Download the appropriated release from github: https://github.com/TimonPost/quinn-ffi/releases
            2. Put the binary in the /bin/Debug or /bin/Release folder (depending your configuration)
            3. !!DONT RENAME the binary!!         
                
            What is a appropriated release:
            1. Run 'rustup default' to retrieve your active rust toolchain. 
            2. Make sure it is the nightly version. Make sure it matches the supported releases.
            3. Find the binary on the releases page with this toolchain in its name. e.g: `quinn_ffi-nightly-x86_64-pc-windows-msvc.dll` if your running windows on msvc architecture. 
            ";
    }
}