using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Quic.Native.ApiWrappers;
using Quic.Native.Handles;
using Quic.Native.Types;

namespace Quic.Implementation
{
    /// <summary>
    /// An initiated QUIC stream that is either unidirectional or bidirectional.
    /// Make sure to respect the directionality otherwise the stream read or write might fail.
    ///
    /// Note that not all stream methods are implemented. Restrict usage to:
    /// - Write
    /// - Read and ReadAsync
    /// - CanRead / CanWrite
    /// </summary>
    public class QuicStream : Stream
    {
        private readonly bool _readable;
        private readonly long _streamId;
        
        private readonly bool _writable;
        
        private readonly ConnectionHandle _handle;

        private readonly BufferBlock<byte> ReadableEvents;
        private readonly ManualResetEvent _writeManualResetEvent;
        
        public QuicStream(ConnectionHandle handle, StreamType streamType, long streamId, bool readable, bool writable)
        {
            StreamType = streamType;
            _streamId = streamId;
            _readable = readable;
            _writable = writable;
            _handle = handle;

            
            _writeManualResetEvent = new ManualResetEvent(false);
            ReadableEvents = new BufferBlock<byte>();
        }

        public override bool CanRead => ReadableEvents.Count != 0 && _readable;
        public override bool CanWrite => _writable;
        public StreamType StreamType { get; }

        public bool IsBiStream => StreamType == StreamType.BiDirectional;
        public bool IsUniStream => StreamType == StreamType.UniDirectional;
        
        public override int Read(byte[] buffer, int offset, int count)
        {
            return ReadAsync(buffer).Result;
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
        {
            AssertReadAccess();
            
            async Task<int> _readAsync()
            {
                // When buffer is blocked, try receiving again till next event arrives.
                while (true)
                {
                    await ReadableEvents.ReceiveAsync(cancellationToken);
                    try
                    {
                        return Read(buffer.Span);
                    }
                    catch (BufferBlockedException e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }

            var read = await _readAsync();

            return await ValueTask.FromResult(read);
        }
        
        public override int Read(Span<byte> buffer)
        {
            var bytesRead = QuinnFFIHelpers.ReadFromStream(_handle, _streamId, buffer);
            return (int)bytesRead;
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
        {
            return new ValueTask(Task.Run(() => Write(buffer.Span), cancellationToken));
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return Task.Run(() => Write(buffer, offset, count), cancellationToken);
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            AssertWriteAccess();
            QuinnFFIHelpers.WriteToStream(_handle, _streamId, buffer[..buffer.Length]);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            AssertWriteAccess();

            QuinnFFIHelpers.WriteToStream(_handle, _streamId, buffer[..count]);
        }

        /// <summary>
        /// Allows the `Read` or `ReadAsync` to continue with its work. 
        /// </summary>
        public void QueueReadEvent()
        {
            if (_readable)
                ReadableEvents.Post((byte)0);
        }

        public void SetWritable()
        {
            if (_readable)
                _writeManualResetEvent.Set();
        }

        private void AssertReadAccess()
        {
            if (!_readable)
                throw new Exception(
                    $"Trying to read a {StreamType} stream that can not be read from this remote endpoint.");
        }

        private void AssertWriteAccess()
        {
            if (!_writable)
                throw new Exception(
                    $"Trying to read a {StreamType} stream that can not be read from this remote endpoint.");
        }


        public override long Length =>
            throw new NotSupportedException("`Length` property of `QuicStream` is not supported.");
        public override bool CanSeek => throw new NotImplementedException("`Position` property of `QuicStream` is not supported.");

        public override long Position
        {
            get => throw new NotSupportedException("`Position` property of `QuicStream` is not supported.");
            set => throw new NotSupportedException("`Position` property of `QuicStream` is not supported.");
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("`Seek` method of `QuicStream` is not supported.");
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException("`SetLength` method of `QuicStream` is not supported.");
        }

        public override void Flush()
        {
            throw new NotSupportedException("`Flush` method of `QuicStream` is not supported.");
        }
    }
}