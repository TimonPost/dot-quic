using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using DotQuic.Native;
using DotQuic.Native.Handles;
using DotQuic.Native.Types;

namespace DotQuic
{
    /// <summary>
    ///     An initiated QUIC stream that is either unidirectional or bidirectional.
    /// </summary>
    /// <remarks>
    /// * Use a `QuicConnection` to initiate streams, or receive stream from peers. 
    /// * Make sure to respect the directionality otherwise the stream read or write might fail.
    /// * Note that not all stream methods are implemented (see docs for defined behaviour). 
    /// </remarks>
    public class QuicStream : Stream
    {
        private readonly ConnectionHandle _handle;
        private readonly bool _readable;
        private readonly long _streamId;

        private readonly bool _writable;
        private readonly ManualResetEvent _writeManualResetEvent;

        private readonly BufferBlock<byte> _readableEvents;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="handle">The connection handle to which the stream belongs.</param>
        /// <param name="streamType">The stream type.</param>
        /// <param name="streamId">The id of this stream.</param>
        /// <param name="readable">If the stream can be read from.</param>
        /// <param name="writable">If the stream can be written to.</param>
        internal QuicStream(ConnectionHandle handle, StreamType streamType, long streamId, bool readable, bool writable)
        {
            StreamType = streamType;
            _streamId = streamId;
            _readable = readable;
            _writable = writable;
            _handle = handle;


            _writeManualResetEvent = new ManualResetEvent(false);
            _readableEvents = new BufferBlock<byte>();
        }

        /// <summary>
        /// Returns if the stream is readable.
        /// </summary>
        public override bool CanRead => _readableEvents.Count != 0 && _readable;

        /// <summary>
        /// Returns if the stream writable.
        /// </summary>
        public override bool CanWrite => _writable;

        /// <summary>
        /// Returns the QUIC stream type. 
        /// </summary>
        public StreamType StreamType { get; }

        /// <summary>
        /// Returns if this is a bidirectional stream.
        /// </summary>
        public bool IsBiStream => StreamType == StreamType.BiDirectional;

        /// <summary>
        ///  Returns if this is a unidirectional stream.
        /// </summary>
        public bool IsUniStream => StreamType == StreamType.UniDirectional;

        /// Not implemented!
        public override long Length =>
            throw new NotSupportedException("`Length` property of `QuicStream` is not supported.");

        /// Not implemented!
        public override bool CanSeek =>
            throw new NotImplementedException("`Position` property of `QuicStream` is not supported.");

        /// Not implemented!
        public override long Position
        {
            get => throw new NotSupportedException("`Position` property of `QuicStream` is not supported.");
            set => throw new NotSupportedException("`Position` property of `QuicStream` is not supported.");
        }

        /// <summary>
        /// Read data into the given buffer synchronously. 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return ReadAsync(buffer).Result;
        }

        /// <summary>
        /// Read data into the given buffer asynchronously. 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = new())
        {
            AssertReadAccess();

            async Task<int> _readAsync()
            {
                // When buffer is blocked, try receiving again till next event arrives.
                while (true)
                {
                    await _readableEvents.ReceiveAsync(cancellationToken);
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

        /// <summary>
        ///     Reads data into the given buffer.     
        /// </summary>
        /// <param name="buffer"></param>
        /// <exception cref="BufferBlockedException">Might throw an exception if buffer is blocked.</exception>
        /// <returns></returns>
        public override int Read(Span<byte> buffer)
        {
            var bytesRead = QuinnFFIHelpers.ReadFromStream(_handle, _streamId, buffer);
            return (int)bytesRead;
        }

        /// <summary>
        /// Writes asynchronously to this stream.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = new())
        {
            return new ValueTask(Task.Run(() => Write(buffer.Span), cancellationToken));
        }

        /// <summary>
        /// Writes the given buffer asynchronously to this stream.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="count"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return Task.Run(() => Write(buffer, offset, count), cancellationToken);
        }

        /// <summary>
        /// Writes the given buffer synchronously to this stream.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            AssertWriteAccess();
            QuinnFFIHelpers.WriteToStream(_handle, _streamId, buffer[..buffer.Length]);
        }

        /// <summary>
        /// Writes the given buffer synchronously to this stream.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public override void Write(byte[] buffer, int offset, int count)
        {
            AssertWriteAccess();

            QuinnFFIHelpers.WriteToStream(_handle, _streamId, buffer[..count]);
        }

        /// <summary>
        ///     Allows the `Read` or `ReadAsync` to continue with its work.
        /// </summary>
        internal void QueueReadEvent()
        {
            if (_readable)
                _readableEvents.Post((byte)0);
        }

        internal void SetWritable()
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

        /// Not implemented!
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("`Seek` method of `QuicStream` is not supported.");
        }

        /// Not implemented!
        public override void SetLength(long value)
        {
            throw new NotSupportedException("`SetLength` method of `QuicStream` is not supported.");
        }

        /// Not implemented!
        public override void Flush()
        {
            throw new NotSupportedException("`Flush` method of `QuicStream` is not supported.");
        }
    }
}