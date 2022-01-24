using System;
using System.Collections.Generic;
using DotQuic.Native;
using DotQuic.Native.Events;
using DotQuic.Native.Handles;
using DotQuic.Native.Types;

namespace DotQuic
{
    /// <summary>
    ///     Carries the stream form which data can be read.
    /// </summary>
    public class DataReceivedEventArgs : EventArgs
    {
        /// <summary>
        ///     The stream that has data ready to be read.
        /// </summary>
        public QuicStream Stream { get; set; }
    }

    /// <summary>
    ///     A QUIC protocol connection to some remote QUIC endpoint.
    /// </summary>
    public class QuicConnection
    {
        private readonly Dictionary<long, QuicStream> _biDirectionalQuicStreams;
        private readonly ConnectionDriver _connectionDriver;
        private readonly Dictionary<long, QuicStream> _uniDirectionalQuicStreams;
        private IncomingState ConnectionState;

        public QuicConnection(ConnectionHandle connectionHandle, int connectionId, ConnectionDriver connectionDriver)
        {
            _connectionDriver = connectionDriver;
            ConnectionHandle = connectionHandle;
            ConnectionId = connectionId;

            _uniDirectionalQuicStreams = new Dictionary<long, QuicStream>();
            _biDirectionalQuicStreams = new Dictionary<long, QuicStream>();

            ConnectionEvents.ConnectionLost += OnConnectionLost;
            ConnectionEvents.DatagramReceived += OnDatagramReceived;

            ConnectionEvents.StreamAvailable += OnStreamAvailable;
            ConnectionEvents.StreamOpened += OnStreamOpened;

            ConnectionEvents.StreamFinished += OnStreamFinished;
            ConnectionEvents.StreamReadable += OnStreamReadable;
            ConnectionEvents.StreamStopped += OnStreamStopped;
            ConnectionEvents.StreamWritable += OnStreamWritable;
        }

        public ConnectionHandle ConnectionHandle { get; }


        /// <summary>
        ///     The id of this connection.
        /// </summary>
        public int ConnectionId { get; }

        /// <summary>
        ///     Returns whether this connection is connected to the remote endpoint.
        ///     A connection is connected if all handshaking procedures are finished.
        /// </summary>
        public bool IsConnected => ConnectionState == IncomingState.Connected;

        /// <summary>
        ///     Returns whether the given stream is an unidirectional stream.
        /// </summary>
        /// <param name="streamId"></param>
        /// <returns>bool</returns>
        public bool IsUniStream(long streamId)
        {
            return _uniDirectionalQuicStreams.ContainsKey(streamId);
        }

        /// <summary>
        ///     Returns whether the given stream is an bidirectional stream.
        /// </summary>
        /// <param name="streamId"></param>
        /// <returns>bool</returns>
        public bool IsBiStream(long streamId)
        {
            return _biDirectionalQuicStreams.ContainsKey(streamId);
        }

        /// <summary>
        ///     Event is triggered when new data is ready to be read on a given stream.
        /// </summary>
        public event EventHandler<DataReceivedEventArgs> DataReceived;

        public event EventHandler<StreamEventArgs> StreamInitiated;
        public event EventHandler<StreamEventArgs> StreamClosed;

        private bool IsThisConnection(int id)
        {
            return id == ConnectionId;
        }

        /// <summary>
        ///     Opens a bidirectional stream to the remote endpoint.
        ///     Exception is thrown if the stream can not be opened or the connection is not yet initialized.
        /// </summary>
        /// <returns>QuicStream</returns>
        public QuicStream OpenBiDirectionalStream()
        {
            if (!IsConnected)
                throw new Exception("Connection is not yet fully initialized.");

            QuinnApi.OpenStream(ConnectionHandle, StreamType.BiDirectional, out var streamId).Unwrap();

            var stream = new QuicStream(ConnectionHandle, StreamType.BiDirectional, streamId, true, true);

            _biDirectionalQuicStreams.Add(streamId, stream);
            return stream;
        }


        /// <summary>
        ///     Opens a unidirectional stream to the remote endpoint.
        ///     Exception is thrown if the stream can not be opened or the connection is not yet initialized.
        /// </summary>
        /// <returns>QuicStream</returns>
        public QuicStream OpenUniDirectionalStream()
        {
            if (!IsConnected)
                throw new Exception("Connection is not yet fully initialized.");

            QuinnApi.OpenStream(ConnectionHandle, StreamType.UniDirectional, out var streamId).Unwrap();

            var stream = new QuicStream(ConnectionHandle, StreamType.UniDirectional, streamId, false, true);
            _uniDirectionalQuicStreams.Add(streamId, stream);
            return stream;
        }


        /// <summary>
        ///     Returns an bidirectional stream with the given stream id.
        ///     Exception is thrown if there is no unidirectional stream with the given id.
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
        ///     Returns an unidirectional stream with the given stream id.
        ///     Exception is thrown if there is no unidirectional stream with the given id.
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
                    stream.QueueReadEvent();
                    break;
                case StreamType.BiDirectional:
                    stream = _biDirectionalQuicStreams[e.StreamId];
                    stream.QueueReadEvent();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _connectionDriver.Schedule(() => DataReceived?.Invoke(this, new DataReceivedEventArgs { Stream = stream }));
        }

        private void OnStreamOpened(object? sender, StreamEventArgs e)
        {
            if (!IsThisConnection(e.ConnectionId)) return;

            QuicStream newStream = null;

            switch (e.StreamType)
            {
                case StreamType.UniDirectional:
                {
                    newStream = new QuicStream(ConnectionHandle, e.StreamType, e.StreamId, true, false);
                    _uniDirectionalQuicStreams.Add(e.StreamId, newStream);
                    break;
                }
                case StreamType.BiDirectional:
                {
                    newStream = new QuicStream(ConnectionHandle, e.StreamType, e.StreamId, true, true);
                    _biDirectionalQuicStreams.Add(e.StreamId, newStream);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException($"{e.StreamType}");
            }

            newStream.QueueReadEvent();
            _connectionDriver.Schedule(() =>
            {
                StreamInitiated?.Invoke(null, new StreamEventArgs(e.ConnectionId, e.StreamId, e.StreamType));
                DataReceived?.Invoke(null, new DataReceivedEventArgs { Stream = newStream });
            });
        }

        private void OnStreamFinished(object? sender, StreamEventArgs e)
        {
            if (!IsThisConnection(e.ConnectionId)) return;

            if (IsUniStream(e.StreamId))
                _uniDirectionalQuicStreams.Remove(e.StreamId);
            else if (IsBiStream(e.StreamId))
                _biDirectionalQuicStreams.Remove(e.StreamId);

            StreamClosed?.Invoke(null, e);
        }


        private void OnStreamAvailable(object? sender, StreamTypeEventArgs e)
        {
            if (!IsThisConnection(e.ConnectionId)) return;
        }

        private void OnDatagramReceived(object? sender, ConnectionIdEventArgs e)
        {
            if (!IsThisConnection(e.Id)) return;
        }

        private void OnConnectionLost(object? sender, ConnectionIdEventArgs e)
        {
            if (!IsThisConnection(e.Id)) return;
        }
        
        public void SetState(IncomingState state)
        {
            ConnectionState = state;
        }
    }
}