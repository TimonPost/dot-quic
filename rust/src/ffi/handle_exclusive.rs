use crate::ffi::thread_bound::ThreadBound;
use std::{
    marker::PhantomData,
    ops::{
        Deref,
        DerefMut,
    },
    panic::{
        RefUnwindSafe,
        UnwindSafe,
    },
};

/**
A non-shared handle that cannot be accessed by multiple threads.

The handle is bound to the thread that it was created on.
The interior value can be treated like `&mut T`.

The handle is bound to the thread that it was created on to ensure
there's no possibility for data races. Note that, if reverse PInvoke is supported
then it's possible to mutably alias the handle from the same thread if the reverse
call can re-enter the FFI using the same handle. This is technically undefined behaviour.

The handle _can_ be deallocated from a different thread than the one that created it.

Consumers must ensure a handle is not used again after it has been deallocated.
*/
#[repr(transparent)]
pub struct HandleExclusive<'a, T>(*mut ThreadBound<T>, PhantomData<&'a T>)
where
    T: ?Sized;

// The handle is semantically `&mut T`
unsafe impl<'a, T> Send for HandleExclusive<'a, T>
where
    &'a mut ThreadBound<T>: Send,
    T: 'static,
{
}

// The handle uses `ThreadBound` for synchronization
unsafe impl<'a, T> Sync for HandleExclusive<'a, T>
where
    &'a mut ThreadBound<T>: Sync,
    T: 'static,
{
}

impl<'a, T> UnwindSafe for HandleExclusive<'a, T> where T: ?Sized + RefUnwindSafe {}

impl<'a, T> HandleExclusive<'a, T>
where
    HandleExclusive<'a, T>: Send + Sync,
{
    pub(crate) fn alloc(value: T) -> Self
    where
        T: 'static,
    {
        let v = Box::new(ThreadBound::new(value));

        HandleExclusive(Box::into_raw(v), PhantomData)
    }

    // There are no other live references and the handle won't be used again
    pub(super) unsafe fn dealloc<R>(handle: Self, f: impl FnOnce(T) -> R) -> R
    where
        T: Send,
    {
        let v = Box::from_raw(handle.0);
        f(v.into_inner())
    }
}

impl<'a, T> Deref for HandleExclusive<'a, T>
where
    T: ?Sized,
{
    type Target = T;

    fn deref(&self) -> &T {
        // We own the interior value
        unsafe { &*(*self.0).get_raw_unchecked() }
    }
}

impl<'a, T> DerefMut for HandleExclusive<'a, T>
where
    T: ?Sized,
{
    fn deref_mut(&mut self) -> &mut T {
        // We own the interior valu
        unsafe { &mut *(*self.0).get_raw_unchecked() }
    }
}
