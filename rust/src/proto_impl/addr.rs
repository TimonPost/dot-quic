use std::{
    net,
    net::{
        Ipv4Addr,
        SocketAddr,
        SocketAddrV4,
    },
};

#[repr(C)]
pub struct IpAddr {
    port: u16,
    address: [u8; 4],
}

impl From<SocketAddr> for IpAddr {
    fn from(addr: SocketAddr) -> Self {
        let address_bytes = match addr.ip() {
            net::IpAddr::V4(ip) => ip.octets(),
            net::IpAddr::V6(_ip) => panic!("not supported"),
        };

        IpAddr {
            port: addr.port(),
            address: address_bytes,
        }
    }
}

impl Into<SocketAddr> for IpAddr {
    fn into(self) -> SocketAddr {
        SocketAddr::V4(SocketAddrV4::new(
            Ipv4Addr::new(
                self.address[0],
                self.address[1],
                self.address[2],
                self.address[3],
            ),
            self.port,
        ))
    }
}
