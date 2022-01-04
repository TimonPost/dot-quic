using System;
using Quic.Native.Handles;

namespace Quic.Native.ApiWrappers
{
    public class StreamHelper
    {
        public static uint WriteToStream(ConnectionHandle handle, long streamId, byte[] buffer)
        {
            var bufferSpan = new ReadOnlySpan<byte>(buffer);

            unsafe
            {
                fixed (byte* bufferPtr = bufferSpan)
                {
                    QuinnApi.write_stream(
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

        public static uint ReadFromStream(ConnectionHandle handle, long streamId, byte[] buffer)
        {
            var bufferSpan = new Span<byte>(buffer);

            unsafe
            {
                fixed (byte* bufferPtr = bufferSpan)
                {
                    QuinnApi.read_stream(
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
    }
}