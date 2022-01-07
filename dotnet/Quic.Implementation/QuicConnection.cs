using System;
using System.Collections.Generic;
using Quic.Native;
using Quic.Native.Events;
using Quic.Native.Handles;
using Quic.Native.Types;

namespace Quic.Implementation
{
    /// <summary>
    /// Carries the stream form which data can be read. 
    /// </summary>
    public class DataReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// The stream that has data ready to be read.
        /// </summary>
        public QuicStream Stream { get; set; }
    }

    /// <summary>
    /// A QUIC protocol connection to some remote QUIC endpoint.
    /// </summary>
    public class QuicConnection
    {
        private readonly ConnectionHandle _connectionHandle;
        private readonly Dictionary<long, QuicStream> _biDirectionalQuicStreams;
        private readonly Dictionary<long, QuicStream> _uniDirectionalQuicStreams;
        private bool _connected;

        public QuicConnection(ConnectionHandle connectionHandle, int connectionId)
        {
            _connectionHandle = connectionHandle;
            ConnectionId = connectionId;

            _uniDirectionalQuicStreams = new Dictionary<long, QuicStream>();
            _biDirectionalQuicStreams = new Dictionary<long, QuicStream>();

            ConnectionEvents.ConnectionInitialized += OnConnectionInitialized;
            ConnectionEvents.ConnectionLost += OnConnectionLost;
            ConnectionEvents.DatagramReceived += DatagramReceived;
            ConnectionEvents.StreamAvailable += StreamAvailable;
            ConnectionEvents.StreamFinished += OnStreamFinished;
            ConnectionEvents.StreamOpened += OnStreamOpened;
            ConnectionEvents.StreamReadable += OnStreamReadable;
            ConnectionEvents.StreamStopped += OnStreamStopped;
            ConnectionEvents.StreamWritable += OnStreamWritable;

            QuinnApi.PollConnection(_connectionHandle);
        }

        /// <summary>
        /// The id of this connection.
        /// </summary>
        public int ConnectionId { get; }

        /// <summary>
        /// Returns whether the given stream is an unidirectional stream.
        /// </summary>
        /// <param name="streamId"></param>
        /// <returns>bool</returns>
        public bool IsUniStream(long streamId) => _uniDirectionalQuicStreams.ContainsKey(streamId);

        /// <summary>
        /// Returns whether the given stream is an bidirectional stream.
        /// </summary>
        /// <param name="streamId"></param>
        /// <returns>bool</returns>
        public bool IsBiStream(long streamId) => _biDirectionalQuicStreams.ContainsKey(streamId);

        /// <summary>
        /// Returns whether this connection is connected to the remote endpoint.
        /// A connection is connected if all handshaking procedures are finished.
        /// </summary>
        public bool IsConnected => _connected;

        /// <summary>
        /// Event is triggered when new data is ready to be read on a given stream.
        /// </summary>
        public event EventHandler<DataReceivedEventArgs> DataReceived;

        private bool IsThisConnection(int id) => id == ConnectionId;

        /// <summary>
        /// Opens a bidirectional stream to the remote endpoint.
        ///
        /// Exception is thrown if the stream can not be opened or the connection is not yet initialized.
        /// </summary>
        /// <returns>QuicStream</returns>
        public QuicStream OpenBiDirectionalStream()
        {
            if (!IsConnected)
                throw new Exception("Connection is not yet fully initialized.");

            QuinnApi.OpenStream(_connectionHandle, StreamType.BiDirectional, out var streamId).Unwrap();

            var stream = new QuicStream(_connectionHandle, StreamType.BiDirectional, streamId, true, true);
            _biDirectionalQuicStreams.Add(streamId, stream);
            return stream;
        }


        /// <summary>
        /// Opens a unidirectional stream to the remote endpoint.
        ///
        /// Exception is thrown if the stream can not be opened or the connection is not yet initialized.
        /// </summary>
        /// <returns>QuicStream</returns>
        public QuicStream OpenUniDirectionalStream()
        {
            if (!IsConnected)
                throw new Exception("Connection is not yet fully initialized.");

            QuinnApi.OpenStream(_connectionHandle, StreamType.UniDirectional, out var streamId).Unwrap();

            var stream = new QuicStream(_connectionHandle, StreamType.UniDirectional, streamId, false, true);
            _uniDirectionalQuicStreams.Add(streamId, stream);
            return stream;
        }


        /// <summary>
        /// Returns an bidirectional stream with the given stream id.
        ///
        /// Exception is thrown if there is no unidirectional stream with the given id. 
        /// </summary>
        /// <param name="streamId"></param>
        /// <returns>QuicStream</returns>
        public QuicStream GetBiStream(long streamId)
        {
            if (!_biDirectionalQuicStreams.TryGetValue(streamId, out var stream))
                throw new Exception($"Bidirectional stream with ID: {streamId} does not exist");

            return stream;
        }

        /// <summary>
        /// Returns an unidirectional stream with the given stream id.
        ///
        /// Exception is thrown if there is no unidirectional stream with the given id. 
        /// </summary>
        /// <param name="streamId"></param>
        /// <returns>QuicStream</returns>
        public QuicStream GetUniStream(long streamId)
        {
            if (!_uniDirectionalQuicStreams.TryGetValue(streamId, out var stream))
                throw new Exception($"Unidirectional stream with ID: {streamId} does not exist");

            return stream;
        }

        private void OnStreamWritable(object? sender, StreamEventArgs e)
        {
            if (!IsThisConnection(e.ConnectionId)) return;

            QuicStream stream;
            switch (e.StreamType)
            {
                case StreamType.UniDirectional:
                    stream = _uniDirectionalQuicStreams[e.StreamId];
                    stream.SetWritable();
                    break;
                case StreamType.BiDirectional:
                    stream = _biDirectionalQuicStreams[e.StreamId];
                    stream.SetWritable();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

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
                    stream = _uniDirectionalQuicStreams[e.StreamId];
                    stream.SetReadable();
                    break;
                case StreamType.BiDirectional:
                    stream = _biDirectionalQuicStreams[e.StreamId];
                    stream.SetReadable();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            DataReceived?.Invoke(this, new DataReceivedEventArgs { Stream = stream });
        }

        private void OnStreamOpened(object? sender, StreamTypeEventArgs e)
        {
            if (!IsThisConnection(e.ConnectionId)) return;

            QuinnApi.AcceptStream(_connectionHandle, (byte)e.StreamType, out var streamId).Unwrap();

            switch (e.StreamType)
            {
                case StreamType.UniDirectional:
                    {
                        var newStream = new QuicStream(_connectionHandle, e.StreamType, streamId, true, false);
                        _uniDirectionalQuicStreams.Add(streamId, newStream);
                        break;
                    }
                case StreamType.BiDirectional:
                    {
                        var newStream = new QuicStream(_connectionHandle, e.StreamType, streamId, true, true);
                        _biDirectionalQuicStreams.Add(streamId, newStream);
                        newStream.SetReadable();
                        DataReceived?.Invoke(null, new DataReceivedEventArgs() {Stream = newStream});
                        break;
                    }
            }
        }

        private void OnStreamFinished(object? sender, StreamEventArgs e)
        {
            if (!IsThisConnection(e.ConnectionId)) return;

            if (IsUniStream(e.StreamId))
                _uniDirectionalQuicStreams.Remove(e.StreamId);
            else if (IsBiStream(e.StreamId))
                _biDirectionalQuicStreams.Remove(e.StreamId);
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

            _connected = true;
        }

    }
}