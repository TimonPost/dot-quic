use crate::ffi::{
    handle_exclusive::HandleExclusive,
    handle_shared::HandleShared,
    out::Out,
};

/**
Whether or not a value passed across an FFI boundary is null.
*/
pub trait IsNull {
    fn is_null(&self) -> bool;
}

impl<'a, T: ?Sized> IsNull for HandleExclusive<'a, T> {
    fn is_null(&self) -> bool {
        self.is_null()
    }
}

impl<'a, T: ?Sized + Sync> IsNull for HandleShared<'a, T> {
    fn is_null(&self) -> bool {
        self.is_null()
    }
}

impl<'a, T: ?Sized> IsNull for Out<'a, T> {
    fn is_null(&self) -> bool {
        self.is_null()
    }
}
