using System;

namespace Quic.Native.Events
{
    public static class ConnectionEvents
    { 
        public static void Initialize()
        {
            QuinnApi.set_on_connected(OnConnected);
            QuinnApi.set_on_connection_lost(OnConnectionLost);

            QuinnApi.set_on_datagram_received(OnDatagramReceived);
      
            QuinnApi.set_on_stream_opened(OnStreamOpened);
            QuinnApi.set_on_stream_available(OnStreamAvailable);
            QuinnApi.set_on_stream_writable(OnStreamWritable);
            QuinnApi.set_on_stream_readable(OnStreamReadable);
            QuinnApi.set_on_stream_stopped(OnStreamStopped);
            QuinnApi.set_on_stream_finished(OnStreamFinished);
        }


        public static event EventHandler<ConnectionIdEventArgs> ConnectionInitialized;
        public static event EventHandler<ConnectionIdEventArgs> ConnectionLost;

        public static event EventHandler<StreamEventArgs> StreamOpened;
        public static event EventHandler<StreamEventArgs> StreamAvailable;
        public static event EventHandler<StreamEventArgs> StreamWritable;
        public static event EventHandler<StreamEventArgs> StreamReadable;
        public static event EventHandler<StreamEventArgs> StreamFinished;
        public static event EventHandler<StreamEventArgs> StreamStopped;
        public static event EventHandler<ConnectionIdEventArgs> DatagramReceived;

      
        public static void OnConnected(int connectionId)
        {
            Console.WriteLine("C#; OnConnected; Connection ID: {0}", connectionId);
            ConnectionInitialized?.Invoke(null, new ConnectionIdEventArgs(connectionId));
        }

        private static void OnConnectionLost(int connectionId)
        {
            Console.WriteLine("C#; OnConnectionLost; Connection ID: {0}", connectionId);
            ConnectionLost?.Invoke(null, new ConnectionIdEventArgs(connectionId));
        }

        private static void OnStreamOpened(int connectionId, byte streamType)
        {
            Console.WriteLine("C#; OnStreamOpened; Connection ID: {0} Stream Type: {1}", connectionId, streamType);
            StreamOpened?.Invoke(null, new StreamEventArgs(connectionId, streamType));
        }

        private static void OnStreamAvailable(int connectionId, byte streamType)
        {
            Console.WriteLine("C#; OnStreamOpened; Connection ID: {0} Stream Type: {1}", connectionId, streamType);
            StreamAvailable?.Invoke(null, new StreamEventArgs(connectionId, streamType));
        }
        
        private static void OnStreamWritable(int connectionId, long streamId)
        {
            Console.WriteLine("C#; OnStreamWritable; Connection ID: {0} Stream: {1}", connectionId, streamId);
            StreamWritable?.Invoke(null, new StreamEventArgs(connectionId, streamId));
        }

        private static void OnStreamReadable(int connectionId, long streamId)
        {
            Console.WriteLine("C#; OnStreamReadable; Connection ID: {0} Stream: {1}", connectionId, streamId);
            StreamReadable?.Invoke(null, new StreamEventArgs(connectionId, streamId));
        }

        private static void OnStreamStopped(int connectionId, long streamId)
        {
            Console.WriteLine("C#; OnStreamStopped; Connection ID: {0} Stream: {1}", connectionId, streamId);
            StreamStopped?.Invoke(null, new StreamEventArgs(connectionId, streamId));
        }

        private static void OnStreamFinished(int connectionId, long streamId)
        {
            Console.WriteLine("C#; OnStreamFinished; Connection ID: {0} Stream: {1}", connectionId, streamId);
            StreamFinished?.Invoke(null, new StreamEventArgs(connectionId, streamId));
        }

        private static void OnDatagramReceived(int connectionId)
        {
            Console.WriteLine("C#; OnDatagramReceived; Connection ID: {0}", connectionId);
            DatagramReceived?.Invoke(null, new ConnectionIdEventArgs(connectionId));
        }
    }
}
