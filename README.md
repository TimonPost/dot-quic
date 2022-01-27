# Dot Quic - QUIC for .NET
[QUIC][QUIC] implementation for .NET with pure rust, up-to-date, QUIC implementation by [Quinn][quinn].

**This library is very new and in active development, dont use it yet**

## Setup
[Dot Quic][DotQuic] does requires a build of [quinn FFI Rust library][qunn-ffi]

1. Add [Dot Quic][dotquic] as dependency to your project (nuget). 
2. Find the right [release build][release] for your platform, and download the dll. 
3. Put the dll into project `bin/Debug` or `bin/Release` file. 

## Why

C# does not yet have a native, up-to-date, QUIC implementation. MsQuic its API will likely be exposed in .Net 7.0 API which is expected around November 2022. Therefore the .Net ecosystem will have to wait for some time for being able to use QUIC protocol. 

**So why write a implementation on top of rust?**

1) Rust is save language like C# and is statically typed that guarantees memory safety with zero cost abstractions, without involving a garbage collector. 
2) MsQuic is written with unsafe C code.
3) MsQuic will be integrated in .NET 7.0 so its wasted effort trying to build just that. 
4) Quinn is a very up to date protocol that is constantly being aligned with the latest drafts.
5) Wirting QUIC in pure C# will be a lot of work, therefore levering an existing implementation could be a good solution for now.


## Main Interface

The motivation of this library is to align its API to the future implementation of QUIC in .NET which can be found in `System.Net.Quic`.

- `QuicListener`: Is like `TcpListener` and accepts incoming connections. 
- `QuinnClient`: It is like `TcpClient`.
- `QuicConnection`: Is a connection connected to some remote endpoint. 
- `QuicStream`: Is a stream that can be either bidirectional, or unidirectional. A `QuicConnection` has one ore more streams. 
- Functions that are postfixed `Async` can be awaited otherwise they will likely be blocking. 

Either events or direct function calls such as `server.Incomming` vs `server.AcceptAsync()` respectively can be used to interact with the protocol. The same principle applies to `Read` vs `OnDataReceive`.

## Configuration
<details>
  <summary>Examples</summary>

A certificate is mandatory for using this library with [Quinn][Quinn].
Use openssl, keytool, lets encrypt to generate one. 

```txt
// Generate x509 ASN.1 PKSI#1 pem encoded certificate and RSA pem encoded private key.
openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout key.pem -out cert.pem -subj "/CN=exampleCA/OU=Example Org/O=Example Company/L=San Francisco/ST=California/C=US" -addext "subjectAltName=DNS:localhost" -addext "basicConstraints=CA:FALSE"

// Convert PEM to DER encoded certificate
openssl x509 -outform der -in cert.pem -out cert.der

// Convert PEM to DER encoded private key. 
openssl rsa -in key.pem -outform DER -out key.der
```
1. The certificate **MUST** be DER-encoded X.509.
2. The private key **MUST** be DER-encoded ASN.1 in either PKCS#8 or PKCS#1 format.

### Debugging:

The FFI library uses `tracing` for logging. You can enable protocol debug logs by calling `QuinnApi.enable_log("trace")`.
The filter is any log filter in the format as described [here][tracing]

</details>

## Examples
<details>
  <summary>Examples</summary>

Server Example:
```csharp
# Create server
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

Client Example:

```csharp
void Main() {
    IPEndPoint connectionIp = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5001);
    QuicClient client = new QuicClient(connectionIp, "cert.der", "key.der");

    client.Connect(serverIp); // Async supported

    // Open client => server streams. 
    QuicStream biStream = client.OpenBiDirectionalStream();
    QuicStream uniStream = client.OpenUniDirectionalStream();

    biStream.Write(data); // Async supported
    uniStream.Read(buffer); // Async supported
}
```
</details>


[Quinn]: https://github.com/quinn-rs/quinn
[QUIC]: https://en.wikipedia.org/wiki/QUIC
[tracing]: https://docs.rs/tracing-subscriber/latest/tracing_subscriber/filter/struct.EnvFilter.html
[dotquic]: https://www.nuget.org/packages/DotQuic/
[qunn-ffi]: https://github.com/TimonPost/quinn-ffi
[release]: https://github.com/TimonPost/quinn-ffi/releases