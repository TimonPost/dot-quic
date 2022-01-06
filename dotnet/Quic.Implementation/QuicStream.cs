using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
        
        private readonly ConnectionHandle _handle;

        private readonly ManualResetEvent _readManualResetEvent;
        private readonly ManualResetEvent _writeManualResetEvent;

        public QuicStream(ConnectionHandle handle, StreamType streamType, long streamId, bool readable, bool writable)
        {
            _streamType = streamType;
            _streamId = streamId;
            _readable = readable;
            _writable = writable;
            _handle = handle;

            _readManualResetEvent = new ManualResetEvent(false);
            _writeManualResetEvent = new ManualResetEvent(false);
        }

        public override bool CanRead => _readManualResetEvent.WaitOne(10);

        public override bool CanSeek => false;
        public override bool CanWrite => _writable;
        public override long Length { get; }
        public override long Position { get; set; }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ReadAccessCheck();

            _readManualResetEvent.WaitOne();
            var read = Read(buffer);
            _readManualResetEvent.Reset();
            return read;
        }

        public override int Read(Span<byte> buffer)
        {
            var bytesRead = StreamHelper.ReadFromStream(_handle, _streamId, buffer);

            return (int)bytesRead;
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
        {
            ReadAccessCheck();

            await _readManualResetEvent.AsTask();
            var read = await new Task<int>(() => Read(buffer.Span));
            _readManualResetEvent.Reset();
            return await ValueTask.FromResult(read);
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
            
            
            StreamHelper.WriteToStream(_handle, _streamId, buffer[..count]);
        }

        public void SetReadable()
        {
            if (_readable)
                _readManualResetEvent.Set();
        }

        public void SetWritable()
        {
            if (_readable)
                _writeManualResetEvent.Set();
        }

        private void ReadAccessCheck()
        {
            if (!_readable)
                throw new Exception(
                    $"Trying to read a {_streamType} stream that can not be read from this remote endpoint.");
        }
    }
}