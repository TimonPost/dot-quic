use std::{
    marker::PhantomData,
    panic::{
        RefUnwindSafe,
        UnwindSafe,
    },
    slice,
};

/**
An initialized parameter passed by shared reference.
*/
#[repr(transparent)]
pub struct Ref<'a, T: ?Sized>(*const T, PhantomData<&'a T>);

impl<'a, T: ?Sized + RefUnwindSafe> UnwindSafe for Ref<'a, T> {}

// The handle is semantically `&T`
unsafe impl<'a, T: ?Sized> Send for Ref<'a, T> where &'a T: Send {}
// The handle uses `ThreadBound` for synchronization"
unsafe impl<'a, T: ?Sized> Sync for Ref<'a, T> where &'a T: Sync {}

impl<'a, T: ?Sized> Ref<'a, T> {
    // The pointer must be nonnull and will remain valid
    pub unsafe fn as_ref(&self) -> &T {
        &*self.0
    }
}

impl<'a> Ref<'a, u8> {
    // The pointer must be nonnull, the length is correct, and will remain valid
    pub unsafe fn as_bytes(&self, len: usize) -> &[u8] {
        slice::from_raw_parts(self.0, len)
    }
}

/**
An initialized parameter passed by exclusive reference.
*/
#[repr(transparent)]
pub struct RefMut<'a, T: ?Sized>(*mut T, PhantomData<&'a mut T>);

impl<'a, T: ?Sized + RefUnwindSafe> UnwindSafe for RefMut<'a, T> {}

// The handle is semantically `&mut T`
unsafe impl<'a, T: ?Sized> Send for RefMut<'a, T> where &'a mut T: Send {}
// The handle uses `ThreadBound` for synchronization
unsafe impl<'a, T: ?Sized> Sync for RefMut<'a, T> where &'a mut T: Sync {}

impl<'a, T: ?Sized> RefMut<'a, T> {
    // The pointer must be nonnull and will remain valid
    pub fn as_mut(&mut self) -> &mut T {
        unsafe { &mut *self.0 }
    }
}

impl<'a> RefMut<'a, u8> {
    // The pointer must be nonnull, the length is correct, and will remain valid
    pub fn as_bytes_mut(&mut self, len: usize) -> &mut [u8] {
        unsafe { slice::from_raw_parts_mut(self.0, len) }
    }
}
