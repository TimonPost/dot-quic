﻿using System;
using System.Runtime.InteropServices;
using Quic.Native.ApiWrappers;

namespace Quic.Native.Types
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly ref struct QuinnResult
    {
        private readonly ResultKind ResultKind;

        public bool Erroneous()
        {
            return ResultKind == ResultKind.Error;
        }

        public bool Successful()
        {
            return ResultKind == ResultKind.Ok;
        }

        public bool IsBufferTooSmall()
        {
            return ResultKind == ResultKind.BufferToSmall;
        }

        public bool IsBufferBlocked()
        {
            return ResultKind == ResultKind.BufferBlocked;
        }

        public void Unwrap()
        {
            if (Erroneous())
                throw new Exception(QuinnFFIHelpers.LastError().Reason);

            if (IsBufferBlocked())
                throw new BufferBlockedException();
        }
    }

    public class BufferBlockedException : Exception
    {
        public override string Message => "The buffer has no data to be read.";
    }
}