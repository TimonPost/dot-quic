using System;
using System.Collections.Generic;
using Quic.Native;
using Quic.Native.Events;
using Quic.Native.Handles;
using Quic.Native.Types;

namespace Quic.Implementation
{
    public class DataReceivedEventArgs : EventArgs
    {
        public QuicStream Stream { get; set; }
    }

    public class QuicConnection
    {
        private readonly ConnectionHandle _connectionHandle;
        public Dictionary<long, QuicStream> BiDirectionalQuicStreams;
        public Dictionary<long, QuicStream> UniDirectionalQuicStreams;

        public QuicConnection(ConnectionHandle connectionHandle, int connectionId)
        {
            _connectionHandle = connectionHandle;
            ConnectionId = connectionId;

            UniDirectionalQuicStreams = new Dictionary<long, QuicStream>();
            BiDirectionalQuicStreams = new Dictionary<long, QuicStream>();

            ConnectionEvents.ConnectionInitialized += OnConnectionInitialized;
            ConnectionEvents.ConnectionLost += OnConnectionLost;
            ConnectionEvents.DatagramReceived += DatagramReceived;
            ConnectionEvents.StreamAvailable += StreamAvailable;
            ConnectionEvents.StreamFinished += OnStreamFinished;
            ConnectionEvents.StreamOpened += OnStreamOpened;
            ConnectionEvents.StreamReadable += OnStreamReadable;
            ConnectionEvents.StreamStopped += OnStreamStopped;
            ConnectionEvents.StreamWritable += OnStreamWritable;
        }

        public int ConnectionId { get; set; }

        public event EventHandler<DataReceivedEventArgs> DataReceived;

        private bool IsThisConnection(int id)
        {
            return id == ConnectionId;
        }

        private void OnStreamWritable(object? sender, StreamEventArgs e)
        {
            if (!IsThisConnection(e.ConnectionId)) return;

            QuicStream stream;
            switch (e.StreamType)
            {
                case StreamType.UniDirectional:
                    stream = UniDirectionalQuicStreams[e.StreamId];
                    stream.SetWritable();
                    break;
                case StreamType.BiDirectional:
                    stream = BiDirectionalQuicStreams[e.StreamId];
                    stream.SetWritable();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            ;
        }

        private void OnStreamStopped(object? sender, StreamEventArgs e)
        {
            if (!IsThisConnection(e.ConnectionId)) return;
        }

        private void OnStreamReadable(object? sender, StreamEventArgs e)
        {
            if (!IsThisConnection(e.ConnectionId)) return;

            QuicStream stream;
            switch (e.StreamType)
            {
                case StreamType.UniDirectional:
                    stream = UniDirectionalQuicStreams[e.StreamId];
                    stream.SetReadable();
                    break;
                case StreamType.BiDirectional:
                    stream = BiDirectionalQuicStreams[e.StreamId];
                    stream.SetReadable();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            ;

            DataReceived?.Invoke(this, new DataReceivedEventArgs { Stream = stream });
        }

        private void OnStreamOpened(object? sender, StreamTypeEventArgs e)
        {
            if (!IsThisConnection(e.ConnectionId)) return;

            QuinnApi.accept_stream(_connectionHandle, (byte)e.StreamType, out var streamId).Unwrap();

            switch (e.StreamType)
            {
                case StreamType.UniDirectional:
                {
                    var newStream = new QuicStream(_connectionHandle, e.StreamType, streamId, true, false);
                    UniDirectionalQuicStreams.Add(streamId, newStream);
                    break;
                }
                case StreamType.BiDirectional:
                {
                    var newStream = new QuicStream(_connectionHandle, e.StreamType, streamId, true, true);
                    BiDirectionalQuicStreams.Add(streamId, newStream);
                    break;
                }
            }
        }

        private void OnStreamFinished(object? sender, StreamEventArgs e)
        {
            if (!IsThisConnection(e.ConnectionId)) return;
        }

        private void StreamAvailable(object? sender, StreamTypeEventArgs e)
        {
            if (!IsThisConnection(e.ConnectionId)) return;
        }

        private void DatagramReceived(object? sender, ConnectionIdEventArgs e)
        {
            if (!IsThisConnection(e.Id)) return;
        }

        private void OnConnectionLost(object? sender, ConnectionIdEventArgs e)
        {
            if (!IsThisConnection(e.Id)) return;
        }

        private void OnConnectionInitialized(object? sender, ConnectionIdEventArgs e)
        {
            if (!IsThisConnection(e.Id)) return;
        }

        public QuicStream OpenBiDirectionalStream()
        {
            QuinnApi.open_stream(_connectionHandle, StreamType.BiDirectional, out var streamId).Unwrap();

            var stream = new QuicStream(_connectionHandle, StreamType.BiDirectional, streamId, true, true);
            BiDirectionalQuicStreams.Add(streamId, stream);
            return stream;
        }

        public QuicStream OpenUniDirectionalStream()
        {
            QuinnApi.open_stream(_connectionHandle, StreamType.UniDirectional, out var streamId).Unwrap();

            var stream = new QuicStream(_connectionHandle, StreamType.UniDirectional, streamId, false, true);
            UniDirectionalQuicStreams.Add(streamId, stream);
            return stream;
        }

        public QuicStream GetBiStream(long streamId)
        {
            if (!BiDirectionalQuicStreams.TryGetValue(streamId, out var stream))
                throw new Exception($"Bidirectional stream with ID: {streamId} does not exist");

            return stream;
        }

        public QuicStream GetUniStream(long streamId)
        {
            if (!UniDirectionalQuicStreams.TryGetValue(streamId, out var stream))
                throw new Exception($"Bidirectional stream with ID: {streamId} does not exist");

            return stream;
        }
    }
}