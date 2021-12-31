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
A handle that can only contain types that are `Sync` + `Send` semantically.
*/
#[repr(transparent)]
pub struct HandleSync<'a, T>(*mut T, PhantomData<&'a T>)
where
    T: ?Sized;

impl<'a, T> UnwindSafe for HandleSync<'a, T> where T: ?Sized + RefUnwindSafe {}

impl<'a, T> HandleSync<'a, T>
where
    T: Send + Sync,
{
    pub(crate) fn alloc(value: T) -> Self
    where
        T: 'static,
    {
        HandleSync(Box::into_raw(Box::new(value)), PhantomData)
    }

    // There are no other live references and the handle won't be used again
    pub(super) unsafe fn dealloc<R>(handle: Self, f: impl FnOnce(T) -> R) -> R
    where
        T: Send + Sync,
    {
        let v = Box::into_inner(Box::from_raw(handle.0));
        f(v)
    }
}

impl<'a, T> Deref for HandleSync<'a, T>
where
    T: ?Sized,
{
    type Target = T;

    fn deref(&self) -> &T {
        // We own the interior value
        unsafe { &*self.0 }
    }
}

impl<'a, T> DerefMut for HandleSync<'a, T>
where
    T: ?Sized,
{
    fn deref_mut(&mut self) -> &mut T {
        // We own the interior valu
        unsafe { &mut *self.0 }
    }
}
