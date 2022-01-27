using System;

namespace DotQuic.Native.Handles
{
    public class ServerConfigHandle : Handle
    {
        public ServerConfigHandle(IntPtr handle) : base(handle)
        {
            SetHandle(handle);
        }

        private ServerConfigHandle() : base(IntPtr.Zero)
        {
        }
    }
}