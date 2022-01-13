# Dot QUIC
[QUIC][QUIC] implementation for .NET with pure rust QUIC implementation by [Quinn][quinn].

**This library is very new and in active development, dont use it yet**

## Why

C# does not yet have a native, uptodate, QUIC implementation. MsQuic its API will likely be exposed in .Net 7.0 API which is expected arround November 2022. Therefore the .Net ecosystem will have to wait for some time for being able to use QUIC protocol. 

**So why write a implementation on top of rust?**

1) Rust is save, language like C# and is statically typed that guarantees memory safety with zero cost abstractions, without involving a garbage collector. 
2) MsQuic is written with unsafe C code.
3) MsQuic will be integrated in .NET 7.0 so its wasted effort trying to build just that. 
4) Quinn is a very up to date protocol that is constantly being aligned with the latest drafts.
5) Wirting QUIC in pure C# will be a lot of work, therefore levering an existing implementation could be a good solution for now.


## Main Interface

The motivation of this library is to align its API to the future implementation of QUIC in .NET which can be found in `System.Net.Quic`.

- `QuicListener`: Is like `TcpListener` and accepts incoming connections. 
- `QuicConnection`: Is a connection connected to some remote endpoint. 
- `QuicStream`: Is a stream that can be either bidirectional, or unidirectional. A `QuicConnection` has one ore more streams. 

## Examples

Server Example:
```csharp
# Create server
IPEndPoint serverIp = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5000);
QuicListener server = new QuicListener(serverIp);

# Listen for incoming connections.
QuicConnection connection = await server.AcceptAsync();
connection.DataReceived += OnDataReceive;

# Create server => client streams 
QuicStream biStream = connection.OpenBiDirectionalStream();
QuicStream uniStream = connection.OpenUniDirectionalStream();

# Send Data
biStream.Write(data);

# Receive Data
var buffer = new byte[10];
uniStream.ReadAsync(buffer);

void OnDataReceive(object? sender, DataReceivedEventArgs e) 
{
    var buffer = new byte[10];
    e.Stream.Read(buffer); // should not block
    
   if (e.Stream.IsBiStream) e.Stream.Write(buffer);  
}
```

Client Example:

```csharp
IPEndPoint connectionIp = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5001);
QuicClient client = new QuicClient(connectionIp);

client.ConnectAsync(serverIp);

# Open client => server streams. 
QuicStream biStream = client.OpenBiDirectionalStream();
QuicStream uniStream = client.OpenUniDirectionalStream();

# Send Data
biStream.Write(data);

# Receive Data
var buffer = new byte[10];
uniStream.ReadAsync(buffer);
```


## Features

- `QuicListener` that can accept incomming `QuicConnections`
- A `QuicClient` that can connect to quic endpoints. 
- Receiving data from clients
- Sending data to clients.
- Opening UNI/BIdirectional streams on both clients and server.

## Todo:
- Configuration of both server and clients. 
- Implement stram finialisation
- Implement connection termination


Cleanup:
- ConnectionHandle


[Quinn]: https://github.com/quinn-rs/quinn
[QUIC]: https://en.wikipedia.org/wiki/QUIC
