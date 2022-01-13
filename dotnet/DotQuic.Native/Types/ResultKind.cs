namespace DotQuic.Native.Types
{
    public enum ResultKind : uint
    {
        Ok,
        Error,
        BufferToSmall,
        BufferBlocked,
        ArgumentNull
    }
}