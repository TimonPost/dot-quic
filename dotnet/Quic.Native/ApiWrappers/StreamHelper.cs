using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quic.Native.Handles;

namespace Quic.Native.ApiWrappers
{
    public class StreamHelper
    {
        public static int ReadFromStream(ConnectionHandle handle, long streamId, byte[] buffer)
        {
            Span<byte> bufferSpan = new Span<byte>(buffer);
        
            unsafe
            { 
                fixed (byte* bufferPtr = bufferSpan)
                {
                    var result = QuinnApi.read_stream(
                        handle,
                        streamId,
                        (IntPtr)bufferPtr,
                        (UIntPtr)buffer.Length,
                        out var actualMessageLen
                    );
        
                    if (result.Erroneous())
                    {
                        return 0;
                    }

                    return actualMessageLen;
                }
            }
        }
    }
}
