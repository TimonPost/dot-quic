#[repr(C)]
pub struct ClientConfig {}

#[no_mangle]
pub extern "C" fn default() -> ClientConfig {
    ClientConfig {}
}
