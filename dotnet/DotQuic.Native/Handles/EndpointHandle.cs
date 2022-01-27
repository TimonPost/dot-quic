using System;

namespace DotQuic.Native.Handles
{
    public class EndpointHandle : Handle
    {
        public EndpointHandle(IntPtr handle) : base(handle)
        {
        }

        private EndpointHandle() : base(IntPtr.Zero)
        {
        }
    }
}