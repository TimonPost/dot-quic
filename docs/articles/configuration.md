A certificate is **mandatory** for using this library with [Quinn][Quinn].
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

The resulting der formatted key, and certificate can be passed to the `QuicClient` and `QuicListener`. 

### Debugging:

The FFI library uses `tracing` for logging. You can enable protocol debug logs by calling `QuinnApi.enable_log("trace")`.
The filter is any log filter in the format as described [here][tracing]

[Quinn]: https://github.com/quinn-rs/quinn
[tracing]: https://docs.rs/tracing-subscriber/latest/tracing_subscriber/filter/struct.EnvFilter.html