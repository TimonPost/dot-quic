using System;
using System.Collections.Generic;
using Quic.Native;
using Quic.Native.ApiWrappers;
using Quic.Native.Events;
using Quic.Native.Handles;
using Quic.Native.Types;

namespace Quic.Implementation
{
    public class QuicConnection
    {
        public Dictionary<long, QuicStream> UniDirectionalQuicStreams;
        public Dictionary<long, QuicStream> BiDirectionalQuicStreams;
        private readonly ConnectionHandle _connectionHandle;

        public int ConnectionId { get; set; }

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

        private bool IsThisConnection(int id) => id == ConnectionId;

        private void OnStreamWritable(object? sender, StreamEventArgs e)
        {
            if (!IsThisConnection(e.ConnectionId)) return;
        }

        private void OnStreamStopped(object? sender, StreamEventArgs e)
        {
            if (!IsThisConnection(e.ConnectionId)) return;
        }

        private void OnStreamReadable(object? sender, StreamEventArgs e)
        {
            if (!IsThisConnection(e.ConnectionId)) return;

            switch (e.StreamType)
            {
                case StreamType.UniDirectional:
                    UniDirectionalQuicStreams[e.StreamId].SetReadable();
                    break;
                case StreamType.BiDirectional:
                    BiDirectionalQuicStreams[e.StreamId].SetReadable();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnStreamOpened(object? sender, StreamTypeEventArgs e)
        {
            if (!IsThisConnection(e.ConnectionId)) return;
            
            var result = QuinnApi.accept_stream(_connectionHandle, (byte)e.StreamType, out long streamId);

            if (result.Erroneous())
            {
                throw new Exception($"Could not accept stream: {LastQuinnError.Retrieve().Reason}");
            }
            else
            {
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

        // public QuicStream OpenBiDirectionalStream()
        // {
        //     var stream = new QuicStream(_connectionHandle, StreamType.BiDirectional, 0, true, true);
        //     BiDirectionalQuicStreams.Add(stream);
        //     return stream;
        // }
        //
        // public QuicStream OpenUniDirectionalStream()
        // {
        //     var stream = new QuicStream(_connectionHandle, StreamType.UniDirectional, 0, false, true);
        //     UniDirectionalQuicStreams.Add(stream);
        //     return stream;
        // }

        public void Poll()
        {
            QuinnApi.poll_connection(_connectionHandle);
        }
    }
}