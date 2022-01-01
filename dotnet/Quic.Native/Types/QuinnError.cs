using System.Runtime.InteropServices;

namespace Quic.Native.Types
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct QuinnError
    {
        public QuinnError(ulong code, string reason)
        {
            Reason = reason;
            Code = code;
        }

        public readonly ulong Code;
        public readonly string Reason;
    }
}