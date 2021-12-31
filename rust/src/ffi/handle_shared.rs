use std::{
    marker::PhantomData,
    ops::Deref,
    panic::{
        RefUnwindSafe,
        UnwindSafe,
    },
};

/**
A shared handle that can be accessed concurrently by multiple threads.

The interior value can be treated like `&T`.
*/
#[repr(transparent)]
pub struct HandleShared<'a, T: ?Sized>(*const T, PhantomData<&'a T>);

// The handle is semantically `&T`
unsafe impl<'a, T> Send for HandleShared<'a, T> where T: ?Sized + Send {}

// The handle is semantically `&T`
unsafe impl<'a, T> Sync for HandleShared<'a, T> where T: ?Sized + Sync {}

impl<'a, T> UnwindSafe for HandleShared<'a, T> where T: ?Sized + RefUnwindSafe {}

impl<'a, T> HandleShared<'a, T>
where
    T: Send + Sync,
{
    pub fn alloc(value: T) -> Self
    where
        T: 'static,
    {
        let v = Box::new(value);

        HandleShared(Box::into_raw(v), PhantomData)
    }
}

impl<'a, T> HandleShared<'a, T>
where
    T: Send + Sync,
{
    // There are no other live references and the handle won't be used again
    pub unsafe fn dealloc<R>(handle: Self, f: impl FnOnce(T) -> R) -> R {
        let v = Box::from_raw(handle.0 as *mut T);
        f(*v)
    }
}

impl<'a, T> Deref for HandleShared<'a, T>
where
    T: ?Sized,
{
    type Target = T;

    // "We own the interior value"
    fn deref(&self) -> &T {
        unsafe { &*self.0 }
    }
}
