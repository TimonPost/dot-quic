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

## Documentation

- [Minimal Server Example](https://timonpost.github.io/dot-quic/articles/server_template.html)
- [Minimal Client Example](https://timonpost.github.io/dot-quic/articles/client_template.html)
- [API Reference](https://timonpost.github.io/dot-quic/index.html)
- [Articles/Tutorials](https://timonpost.github.io/dot-quic/articles/quic_introduction.html)

[Quinn]: https://github.com/quinn-rs/quinn
[QUIC]: https://en.wikipedia.org/wiki/QUIC
[tracing]: https://docs.rs/tracing-subscriber/latest/tracing_subscriber/filter/struct.EnvFilter.html
[dotquic]: https://www.nuget.org/packages/DotQuic/
[qunn-ffi]: https://github.com/TimonPost/quinn-ffi
[release]: https://github.com/TimonPost/quinn-ffi/releases
