Here follows a minimal example for a QUIC client. 

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