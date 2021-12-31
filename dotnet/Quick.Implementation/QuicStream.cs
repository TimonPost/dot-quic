using System;
using System.IO;

namespace Quick.Implementation
{
    public class QuicStream : Stream
    {
        private readonly StreamType _streamType;
        private readonly int _streamId;

        public QuicStream(StreamType streamType, int streamId)
        {
            _streamType = streamType;
            _streamId = streamId;
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_streamType == StreamType.UniDirectional)
                return 0;

            throw new NotImplementedException();
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
            
            throw new NotImplementedException();
        }

        public override bool CanRead { get; }
        public override bool CanSeek { get; }
        public override bool CanWrite { get; }
        public override long Length { get; }
        public override long Position { get; set; }
    }
}