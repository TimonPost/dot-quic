using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace DotQuic.Native.Handles
{
    public abstract class Handle : SafeHandle
    {
        private readonly Mutex mutex = new();

        private Handle()
            : base(IntPtr.Zero, true)
        {
        }

        public Handle(IntPtr handle) : base(handle, true)
        {
            SetHandle(handle);
        }

        public bool IsAcquired { get; set; }
        public override bool IsInvalid => handle == IntPtr.Zero;

        public virtual IntPtr Acquire()
        {
            mutex.WaitOne();
            IsAcquired = true;
            return handle;
        }

        public virtual void Release()
        {
            IsAcquired = false;
            mutex.ReleaseMutex();
        }

        protected override bool ReleaseHandle()
        {
            if (handle == IntPtr.Zero) return true;

            var h = handle;
            handle = IntPtr.Zero;

            return true;
        }
    }

    public class ConnectionHandle : Handle
    {
        public ConnectionHandle(IntPtr handle) : base(handle)
        {
        }

        private ConnectionHandle() : base(IntPtr.Zero)
        {
        }
    }
}