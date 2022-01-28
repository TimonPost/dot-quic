# API Documentation

Thanks for checking out this library!

Over here you can find all the information required to setup a QUIC application with DotQuic.

## API Overview

The motivation of this library is to align its API to the future implementation of QUIC in .NET which can be found in `System.Net.Quic`.

- `QuicListener`: Is like `TcpListener` and accepts incoming connections. 
- `QuinnClient`: It is like `TcpClient`.
- `QuicConnection`: Is a connection connected to some remote endpoint. 
- `QuicStream`: Is a stream that can be either bidirectional, or unidirectional. A `QuicConnection` has one ore more streams. 
- Functions that are postfixed `Async` can be awaited otherwise they will likely be blocking. 

Either events or direct function calls such as `server.Incomming` vs `server.AcceptAsync()` respectively can be used to interact with the protocol. The same principle applies to `Read` vs `OnDataReceive`.