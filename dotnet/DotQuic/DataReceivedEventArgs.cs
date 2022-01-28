using System;

namespace DotQuic
{
    /// <summary>
    ///     Carries the stream form which data can be read.
    /// </summary>
    public class DataReceivedEventArgs : EventArgs
    {
        /// <summary>
        ///     The stream that has data ready to be read.
        /// </summary>
        public QuicStream Stream { get; internal set; }
    }
}