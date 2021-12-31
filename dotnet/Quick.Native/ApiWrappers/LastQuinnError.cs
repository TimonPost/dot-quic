using System;
using System.Text;
using Quick.Native.Types;

namespace Quick.Native.ApiWrappers
{
    public static class LastQuinnError
    {
        public static QuinnError Retrieve()
        {
            return FillLastResult(new Span<byte>(new byte[1024]));
        }

        private static unsafe QuinnError FillLastResult(Span<byte> buffer)
        {
            fixed (byte* messageBufPtr = buffer)
            {
                var result = QuinnApi.last_error(
                    (IntPtr)messageBufPtr,
                    (UIntPtr)buffer.Length,
                    out var actualMessageLen);

                if (result.IsBufferTooSmall()) return FillLastResult(new Span<byte>(new byte[(int)actualMessageLen]));

                return new QuinnError(0, Encoding.UTF8.GetString(messageBufPtr, (int)actualMessageLen));
            }
        }
    }
}