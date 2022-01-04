using System;
using System.IO;
using System.Threading;
using Quic.Native.ApiWrappers;
using Quic.Native.Handles;
using Quic.Native.Types;

namespace Quic.Implementation
{
    public class QuicStream : Stream
    {
        private readonly bool _readable;
        private readonly long _streamId;
        private readonly StreamType _streamType;
        private readonly bool _writable;

        private bool _canRead;
        private readonly ConnectionHandle _handle;

        private readonly ManualResetEvent ReadManualResetEvent;
        private readonly ManualResetEvent WriteManualResetEvent;

        public QuicStream(ConnectionHandle handle, StreamType streamType, long streamId, bool readable, bool writable)
        {
            _streamType = streamType;
            _streamId = streamId;
            _readable = readable;
            _writable = writable;
            _handle = handle;

            ReadManualResetEvent = new ManualResetEvent(false);
            WriteManualResetEvent = new ManualResetEvent(false);
        }

        public override bool CanRead => ReadManualResetEvent.WaitOne(10);

        public override bool CanSeek { get; }
        public override bool CanWrite { get; }
        public override long Length { get; }
        public override long Position { get; set; }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!_readable)
                throw new Exception(
                    $"Trying to read a {_streamType} stream that can not be read from this remote endpoint.");

            ReadManualResetEvent.WaitOne();

            var bytesRead = StreamHelper.ReadFromStream(_handle, _streamId, buffer);

            ReadManualResetEvent.Reset();

            return (int)bytesRead;
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

            //ReadManualResetEvent.WaitOne();

            StreamHelper.WriteToStream(_handle, _streamId, buffer);

            //ReadManualResetEvent.Reset();
        }

        public void SetReadable()
        {
            if (_readable)
                ReadManualResetEvent.Set();
        }

        public void SetWritable()
        {
            if (_readable)
                WriteManualResetEvent.Set();
        }
    }
}