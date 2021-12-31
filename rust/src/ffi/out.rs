use std::{
    marker::PhantomData,
    panic::{
        RefUnwindSafe,
        UnwindSafe,
    },
    ptr,
    slice,
};

/**
An uninitialized, assignable out parameter.
*/
#[repr(transparent)]
pub struct Out<'a, T: ?Sized>(*mut T, PhantomData<&'a mut T>);

impl<'a, T: ?Sized + RefUnwindSafe> UnwindSafe for Out<'a, T> {}

// The handle is semantically `&mut T`
unsafe impl<'a, T: ?Sized> Send for Out<'a, T> where &'a mut T: Send {}
// The handle uses `ThreadBound` for synchronization
unsafe impl<'a, T: ?Sized> Sync for Out<'a, T> where &'a mut T: Sync {}

impl<'a, T> Out<'a, T> {
    pub fn new(value: *mut T) -> Out<'a, T> {
        Out(value, PhantomData)
    }

    //The pointer must be nonnull and valid for writes
    pub unsafe fn init(&mut self, value: T) {
        ptr::write(self.0, value);
    }
}

impl<'a> Out<'a, u8> {
    // The pointer must be nonnull, not overlap the slice, must be valid for the length of the slice, and valid for writes
    pub unsafe fn init_bytes(&mut self, value: &[u8]) {
        ptr::copy_nonoverlapping(value.as_ptr(), self.0, value.len());
    }

    // The slice must never be read from and must be valid for the length of the slice
    pub unsafe fn as_uninit_bytes_mut(&mut self, len: usize) -> &mut [u8] {
        slice::from_raw_parts_mut(self.0, len)
    }
}
