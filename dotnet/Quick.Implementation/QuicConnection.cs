using System.Collections.Generic;
using Quick.Native;
using Quick.Native.Events;
using Quick.Native.Handles;

namespace Quick.Implementation
{
    public class QuicConnection
    {
        public List<QuicStream> UniDirectionalQuicStreams;
        public List<QuicStream> BiDirectionalQuicStreams;
        private readonly ConnectionHandle _connectionHandle;

        public int ConnectionId { get; set; }

        public QuicConnection(ConnectionHandle connectionHandle, int connectionId)
        {
            _connectionHandle = connectionHandle;
            ConnectionId = connectionId;

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
            throw new System.NotImplementedException();
        }

        private void OnStreamStopped(object? sender, StreamEventArgs e)
        {
            if (!IsThisConnection(e.ConnectionId)) return;
            throw new System.NotImplementedException();
        }

        private void OnStreamReadable(object? sender, StreamEventArgs e)
        {
            if (!IsThisConnection(e.ConnectionId)) return;
            throw new System.NotImplementedException();
        }

        private void OnStreamOpened(object? sender, StreamEventArgs e)
        {
            if (IsThisConnection(e.ConnectionId)) return;
            throw new System.NotImplementedException();
        }

        private void OnStreamFinished(object? sender, StreamEventArgs e)
        {
            if (!IsThisConnection(e.ConnectionId)) return;
            throw new System.NotImplementedException();
        }

        private void StreamAvailable(object? sender, StreamEventArgs e)
        {
            if (!IsThisConnection(e.ConnectionId)) return;
            throw new System.NotImplementedException();
        }

        private void DatagramReceived(object? sender, ConnectionIdEventArgs e)
        {
            if (!IsThisConnection(e.Id)) return;
            throw new System.NotImplementedException();
        }

        private void OnConnectionLost(object? sender, ConnectionIdEventArgs e)
        {
            if (!IsThisConnection(e.Id)) return;
            throw new System.NotImplementedException();
        }

        private void OnConnectionInitialized(object? sender, ConnectionIdEventArgs e)
        {
            if (!IsThisConnection(e.Id)) return;
            throw new System.NotImplementedException();
        }

        public QuicStream OpenBiDirectionalStream()
        {
            var stream = new QuicStream(StreamType.BiDirectional, 0);
            BiDirectionalQuicStreams.Add(stream);
            return stream;
        }

        public QuicStream OpenUniDirectionalStream()
        {
            var stream = new QuicStream(StreamType.UniDirectional, 0);
            UniDirectionalQuicStreams.Add(stream);
            return stream;
        }

        public void Poll()
        {
            QuinnApi.poll_connection(_connectionHandle);
        }
    }
}