using System;
using System.Runtime.InteropServices;

namespace Quic.Native.Handles
{
    public class ConnectionHandle : SafeHandle
    {
        private ConnectionHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public ConnectionHandle(IntPtr handle)
            : this()
        {
            this.SetHandle(handle);
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