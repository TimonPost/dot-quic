using System;
using System.Runtime.InteropServices;
using Quic.Native.ApiWrappers;

namespace Quic.Native.Types
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly ref struct QuinnResult
    {
        private readonly Kind ResultKind;

        public bool Erroneous()
        {
            return ResultKind == Kind.Error;
        }

        public bool Successful()
        {
            return ResultKind == Kind.Ok;
        }

        public bool IsBufferTooSmall()
        {
            return ResultKind == Kind.IsBufferToSmall;
        }

        public void Unwrap()
        {
            if (Erroneous())
                throw new Exception(LastQuinnError.Retrieve().Reason);
        }
    }
}