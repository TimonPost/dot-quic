using System;
using System.IO;
using System.Threading;
using Quic.Native;
using Quic.Native.ApiWrappers;
using Quic.Native.Handles;
using Quic.Native.Types;

namespace Quic.Implementation
{
    public class QuicStream : Stream
    {
        private readonly StreamType _streamType;
        private readonly long _streamId;
        private readonly bool _readable;
        private readonly bool _writable;
        private ConnectionHandle _handle;

        private ManualResetEvent ReadManualResetEvent;
        private bool _canRead;

        public QuicStream(ConnectionHandle handle, StreamType streamType, long streamId, bool readable, bool writable)
        {
            _streamType = streamType;
            _streamId = streamId;
            _readable = readable;
            _writable = writable;
            _handle = handle;

            ReadManualResetEvent = new ManualResetEvent(false);
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!_readable)
                throw new Exception($"Trying to read a {_streamType} stream that can not be read from this remote endpoint.");

            ReadManualResetEvent.WaitOne();

            var bytesRead = StreamHelper.ReadFromStream(_handle, _streamId, buffer);

            ReadManualResetEvent.Reset();

            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!_writable) return;

            throw new NotImplementedException();
        }

        public void SetReadable()
        {
            if (_readable)
                ReadManualResetEvent.Set();
        }

        public override bool CanRead => ReadManualResetEvent.WaitOne(10);

        public override bool CanSeek { get; }
        public override bool CanWrite { get; }
        public override long Length { get; }
        public override long Position { get; set; }
    }
}