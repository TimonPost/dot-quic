using System;
using DotQuic.Native.Types;

namespace DotQuic.Native.Events
{
    public static class ConnectionEvents
    {
        private static readonly OnConnected _onConnected = OnConnected;
        private static readonly OnConnectionLost _onConnectionLost = OnConnectionLost;
        private static readonly OnDatagramReceived _onDatagramReceived= OnDatagramReceived;
        private static readonly OnStreamOpened _onStreamOpened= OnStreamOpened;
        private static readonly OnStreamAvailable _onStreamAvailable= OnStreamAvailable;
        private static readonly OnStreamWritable _onStreamWritable = OnStreamWritable;
        private static readonly OnStreamReadable _onStreamReadable = OnStreamReadable;
        private static readonly OnStreamStopped _onStreamStopped= OnStreamStopped;
        private static readonly OnStreamFinished _onStreamFinished = OnStreamFinished;

        public static void Initialize()
        {
            // Delegates should never bee cleaned. 
            GC.KeepAlive(_onConnected);
            GC.KeepAlive(_onConnectionLost);
            GC.KeepAlive(_onDatagramReceived);
            GC.KeepAlive(_onStreamOpened);
            GC.KeepAlive(_onStreamAvailable);
            GC.KeepAlive(_onStreamWritable);
            GC.KeepAlive(_onStreamReadable);
            GC.KeepAlive(_onStreamStopped);
            GC.KeepAlive(_onStreamFinished);

            QuinnApi.SetOnConnected(_onConnected).Unwrap();
            QuinnApi.SetOnConnectionLost(_onConnectionLost).Unwrap();

            QuinnApi.SetOnDatagramReceived(_onDatagramReceived).Unwrap();

            QuinnApi.SetOnStreamOpened(_onStreamOpened).Unwrap();
            QuinnApi.SetOnStreamAvailable(_onStreamAvailable).Unwrap();
            QuinnApi.SetOnStreamWritable(_onStreamWritable).Unwrap();
            QuinnApi.SetOnStreamReadable(_onStreamReadable).Unwrap();
            QuinnApi.SetOnStreamStopped(_onStreamStopped).Unwrap();
            QuinnApi.SetOnStreamFinished(_onStreamFinished).Unwrap();
        }
        
        /// Is triggered when a connection is fully initialized and ready to be used.
        public static event EventHandler<ConnectionIdEventArgs> ConnectionInitialized;

        /// Is triggered when a connection is lost. 
        public static event EventHandler<ConnectionIdEventArgs> ConnectionLost;

        /// Is triggered when a stream is opened. 
        public static event EventHandler<StreamTypeEventArgs> StreamOpened;

        /// Is triggered when a stream is available for accepting. 
        public static event EventHandler<StreamTypeEventArgs> StreamAvailable;

        /// Is triggered when a stream is writable.
        public static event EventHandler<StreamEventArgs> StreamWritable;

        /// Is triggered when a stream is readable. 
        public static event EventHandler<StreamEventArgs> StreamReadable;

        /// Is triggered when a stream is finished. 
        public static event EventHandler<StreamEventArgs> StreamFinished;
        
        /// Is triggered when a stream is stopped. 
        public static event EventHandler<StreamEventArgs> StreamStopped;

        /// Is triggered when a datagram is received. 
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