using System;
using Quic.Native.Types;

namespace Quic.Native.Events
{
    public static class ConnectionEvents
    {
        public static void Initialize()
        {
            QuinnApi.set_on_connected(OnConnected).Unwrap();
            QuinnApi.set_on_connection_lost(OnConnectionLost).Unwrap();

            QuinnApi.set_on_datagram_received(OnDatagramReceived).Unwrap();

            QuinnApi.set_on_stream_opened(OnStreamOpened).Unwrap();
            QuinnApi.set_on_stream_available(OnStreamAvailable).Unwrap();
            QuinnApi.set_on_stream_writable(OnStreamWritable).Unwrap();
            QuinnApi.set_on_stream_readable(OnStreamReadable).Unwrap();
            QuinnApi.set_on_stream_stopped(OnStreamStopped).Unwrap();
            QuinnApi.set_on_stream_finished(OnStreamFinished).Unwrap();
        }


        public static event EventHandler<ConnectionIdEventArgs> ConnectionInitialized;
        public static event EventHandler<ConnectionIdEventArgs> ConnectionLost;

        public static event EventHandler<StreamTypeEventArgs> StreamOpened;
        public static event EventHandler<StreamTypeEventArgs> StreamAvailable;
        public static event EventHandler<StreamEventArgs> StreamWritable;
        public static event EventHandler<StreamEventArgs> StreamReadable;
        public static event EventHandler<StreamEventArgs> StreamFinished;
        public static event EventHandler<StreamEventArgs> StreamStopped;
        public static event EventHandler<ConnectionIdEventArgs> DatagramReceived;


        public static void OnConnected(int connectionId)
        {
            ConnectionInitialized?.Invoke(null, new ConnectionIdEventArgs(connectionId));
        }

        private static void OnConnectionLost(int connectionId)
        {
            ConnectionLost?.Invoke(null, new ConnectionIdEventArgs(connectionId));
        }

        private static void OnStreamOpened(int connectionId, byte streamType)
        {
            StreamOpened?.Invoke(null, new StreamTypeEventArgs(connectionId, (StreamType)streamType));
        }

        private static void OnStreamAvailable(int connectionId, byte streamType)
        {
            StreamAvailable?.Invoke(null, new StreamTypeEventArgs(connectionId, (StreamType)streamType));
        }

        private static void OnStreamWritable(int connectionId, long streamId, byte streamType)
        {
            StreamWritable?.Invoke(null, new StreamEventArgs(connectionId, streamId, (StreamType)streamType));
        }

        private static void OnStreamReadable(int connectionId, long streamId, byte streamType)
        {
            StreamReadable?.Invoke(null, new StreamEventArgs(connectionId, streamId, (StreamType)streamType));
        }

        private static void OnStreamStopped(int connectionId, long streamId, byte streamType)
        {
            StreamStopped?.Invoke(null, new StreamEventArgs(connectionId, streamId, (StreamType)streamType));
        }

        private static void OnStreamFinished(int connectionId, long streamId, byte streamType)
        {
            StreamFinished?.Invoke(null, new StreamEventArgs(connectionId, streamId, (StreamType)streamType));
        }

        private static void OnDatagramReceived(int connectionId)
        {
            DatagramReceived?.Invoke(null, new ConnectionIdEventArgs(connectionId));
        }
    }
}