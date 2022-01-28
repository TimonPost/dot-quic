Here follows a minimal example for a QUIC server. 

```csharp
void Main() {
    IPEndPoint serverIp = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5000);
    QuicListener server = new QuicListener(serverIp, "cert.der", "key.der");
    
    // It is also possible to use `Accept(Async)` for incoming connections instead of using events. 
    
    server.Incoming += OnIncoming;   
    server.ConnectionClose += OnConnectionClose;
}

async void OnIncoming(object? sender, NewConnectionEventArgs e)
{
    // Do something when connection is incoming. 
    var connection = await Server.AcceptAsync(new CancellationToken());

    // It is also possible use Read(Async) instead of using `OnDataReceive`. 

    connection.DataReceived += OnDataReceive;
    connection.StreamInitiated += OnStreamInitiated;
    connection.StreamClosed += OnStreamClosed;
}

void OnStreamInitiated(object? sender, StreamEventArgs e) { /* Do something when stream is initiated. */ }
void OnStreamClosed(object? sender, StreamEventArgs e) { /* Do something when stream is closed.*/ }
void OnConnectionClose(object? sender, ConnectionIdEventArgs args) { /* Connection is closed */  }

private static void OnDataReceive(object? sender, DataReceivedEventArgs e)
{
    var read = e.Stream.Read(buffer); // Async supported
    var read = e.Stream.Write(buffer); // Async supported
}
```