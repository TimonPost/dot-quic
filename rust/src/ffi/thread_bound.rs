use crate::ffi::deferred_cleanup::{
    get_thread_id,
    ThreadId,
};
use std::{
    cell::UnsafeCell,
    panic::{
        RefUnwindSafe,
        UnwindSafe,
    },
};

/**
A value that's bound to the thread it's created on.
*/
pub struct ThreadBound<T: ?Sized> {
    thread_id: ThreadId,
    inner: UnsafeCell<T>,
}

impl<T> ThreadBound<T> {
    pub(super) fn new(inner: T) -> Self {
        ThreadBound {
            thread_id: get_thread_id(),
            inner: UnsafeCell::new(inner),
        }
    }
}

/*
We don't need to check the thread id when moving out of the inner
value so long as the inner value is itself `Send`. This allows
the .NET runtime to potentially finalize a value on another thread.
*/
impl<T: Send> ThreadBound<T> {
    pub(super) fn into_inner(self) -> T {
        self.inner.into_inner()
    }
}

impl<T: ?Sized> ThreadBound<T> {
    fn check(&self) {
        let _current = get_thread_id();

        // if self.thread_id != current {
        //     panic!("attempted to access resource from a different thread");
        // }
    }

    pub(super) fn get_raw_unchecked(&self) -> *mut T {
        self.check();
        self.inner.get()
    }
}

impl<T: ?Sized + UnwindSafe> UnwindSafe for ThreadBound<T> {}
impl<T: ?Sized + RefUnwindSafe> RefUnwindSafe for ThreadBound<T> {}

// The inner value is safe to send to another thread
unsafe impl<T: ?Sized + Send> Send for ThreadBound<T> {}

// The inner value can't actually be accessed concurrently
unsafe impl<T: ?Sized> Sync for ThreadBound<T> {}
