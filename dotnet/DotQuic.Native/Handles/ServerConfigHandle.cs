using System;
using System.Runtime.InteropServices;

namespace DotQuic.Native.Handles
{
    public class ServerConfigHandle : SafeHandle
    {
        private ServerConfigHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid => handle == IntPtr.Zero;

        protected override bool ReleaseHandle()
        {
            if (handle == IntPtr.Zero) return true;

            var h = handle;
            handle = IntPtr.Zero;

            return true;
        }
    }
}