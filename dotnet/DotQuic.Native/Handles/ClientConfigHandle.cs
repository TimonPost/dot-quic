using System;

namespace DotQuic.Native.Handles
{
    public class ClientConfigHandle : Handle
    {
        public ClientConfigHandle(IntPtr handle) : base(handle)
        {
        }

        private ClientConfigHandle() : base(IntPtr.Zero)
        {
        }
    }
}