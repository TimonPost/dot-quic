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
    /// </summary>
    public class QuicStream : Stream
    {
        private readonly bool _readable;
        private readonly long _streamId;
        
        private readonly bool _writable;
        
        private readonly ConnectionHandle _handle;

        private BufferBlock<byte> ReadableEvents;
        private readonly ManualResetEvent _writeManualResetEvent;

        public StreamType StreamType { get; }

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

        public override bool CanRead => ReadableEvents.Count != 0;

        public override bool CanSeek => false;
        public override bool CanWrite => _writable;
        public override long Length { get; }
        public override long Position { get; set; }
        public bool IsBiStream => StreamType == StreamType.BiDirectional;
        public bool IsUniStream => StreamType == StreamType.UniDirectional;

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return ReadAsync(buffer).Result;
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
        {
            ReadAccessCheck();


            async Task<int> _readAsync()
            {
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
            Position = 0;
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

        private void ReadAccessCheck()
        {
            if (!_readable)
                throw new Exception(
                    $"Trying to read a {StreamType} stream that can not be read from this remote endpoint.");
        }
    }
}