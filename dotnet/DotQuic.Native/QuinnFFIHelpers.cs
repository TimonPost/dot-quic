using System;
using System.Net;
using System.Text;
using DotQuic.Native.Handles;
using DotQuic.Native.Types;

namespace DotQuic.Native
{
    /// <summary>
    ///     Some helper functions for the Quinn Api FFI.
    /// </summary>
    public class QuinnFFIHelpers
    {
        /// <summary>
        ///     Processes a received QUIC packet.
        ///     This function calls into rust and prevents the buffer memory from changing.
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="data"></param>
        /// <param name="endpoint"></param>
        public static void HandleDatagram(EndpointHandle handle, ReadOnlySpan<byte> data, IPEndPoint endpoint)
        {
            unsafe
            {
                fixed (byte* valuePtr = data)
                {
                    QuinnApi.HandleDatagram(handle, (IntPtr)valuePtr, (UIntPtr)data.Length, endpoint.ToNative())
                        .Unwrap();
                }
            }
        }

        /// <summary>
        ///     Writes the given buffer into the stream.
        /// </summary>
        /// <param name="handle">handle to connection owning stream</param>
        /// <param name="streamId">id of the stream to write to</param>
        /// <param name="buffer">buffer to write to the stream</param>
        /// <returns>Written bytes/</returns>
        public static uint WriteToStream(ConnectionHandle handle, long streamId, ReadOnlySpan<byte> buffer)
        {
            unsafe
            {
                fixed (byte* bufferPtr = buffer)
                {
                    QuinnApi.WriteStream(
                        handle,
                        streamId,
                        (IntPtr)bufferPtr,
                        (uint)buffer.Length,
                        out var bytesWritten
                    ).Unwrap();

                    return bytesWritten;
                }
            }
        }

        /// <summary>
        ///     Reads data into the given stream and returns the the total bytes read.
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="streamId"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static uint ReadFromStream(ConnectionHandle handle, long streamId, Span<byte> buffer)
        {
            unsafe
            {
                fixed (byte* bufferPtr = buffer)
                {
                    QuinnApi.ReadStream(
                        handle,
                        streamId,
                        (IntPtr)bufferPtr,
                        (uint)buffer.Length,
                        out var actualMessageLen
                    ).Unwrap();

                    return actualMessageLen;
                }
            }
        }

        /// Returns the last occurred quinn error.
        /// 
        /// Call this method when an exception occurred from the protocol.
        /// <returns>QuinnError</returns>
        public static QuinnError LastError()
        {
            return FillLastResult(new Span<byte>(new byte[1024]));
        }

        private static unsafe QuinnError FillLastResult(Span<byte> buffer)
        {
            fixed (byte* messageBufPtr = buffer)
            {
                var result = QuinnApi.LastError(
                    (IntPtr)messageBufPtr,
                    (UIntPtr)buffer.Length,
                    out var actualMessageLen);

                if (result.IsBufferTooSmall()) return FillLastResult(new Span<byte>(new byte[(int)actualMessageLen]));

                // Rust strings are UTF8 encoded
                return new QuinnError(0, Encoding.UTF8.GetString(messageBufPtr, (int)actualMessageLen));
            }
        }
    }
}