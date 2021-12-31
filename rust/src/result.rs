use std::panic::{UnwindSafe, catch_unwind};
use std::borrow::{BorrowMut, Borrow};
use std::cell::RefCell;

/** A container for the last result type returned by an FFI call on a given thread. */
thread_local! {
    static LAST_RESULT: RefCell<Option<LastResult>> = RefCell::new(None);
}

struct LastResult {
    value: QuinnResult,
    err: Option<String>,
}

/**
An indicator of success or failure in an FFI call.

If the result is not success, a descriptive error stack can be obtained.
*/
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
enum QuinnResult {
    Ok,
    Error
}

impl QuinnResult {
    /**
    Attempt to get a human-readable error MESSAGE for a result.

    If the result is successful then this method returns `None`.
    */
    fn as_err(&self) -> Option<&'static str> {
        match *self {
            QuinnResult::Ok => None,
            QuinnResult::Error => None
        }
    }

    /**
    Call a function that returns a `FlareResult`, setting the thread-local last result.

    This method will also catch panics, so the function to call must be unwind safe.
    */
    pub(super) fn catch(f: impl FnOnce() -> Self + UnwindSafe) -> Self {
        LAST_RESULT.with(|mut last_result| {
            {
                *last_result.borrow_mut() = None;
            }

            match catch_unwind(f) {
                Ok(quinn_result) => {
                    let extract_err = || quinn_result.as_err().map(Into::into);

                    // Always set the last result so it matches what's returned.
                    // This `Ok` branch doesn't necessarily mean the result is ok,
                    // only that there wasn't a panic.
                    last_result
                        .borrow_mut()
                        .map_mut(|last_result| {
                            last_result.value = quinn_result;
                            last_result.err.or_else_mut(extract_err);
                        })
                        .get_or_insert_with(|| LastResult {
                            value: quinn_result,
                            err: extract_err(),
                        })
                        .value
                }
                Err(e) => {
                    let extract_panic =
                        || error::extract_panic(&e)
                            .map(|s| format!("internal panic with '{}'", s));

                    // Set the last error to the panic MESSAGE if it's not already set
                    last_result
                        .borrow_mut()
                        .map_mut(|last_result| {
                            last_result.err.or_else_mut(extract_panic);
                        })
                        .get_or_insert_with(|| LastResult {
                            value: QuinnResult::Error,
                            err: extract_panic(),
                        })
                        .value
                }
            }
        })
    }

    /** Access the last result returned on the calling thread. */
    fn with_last_result<R>(f: impl Fn(Option<(QuinnResult, Option<&str>)>) -> R) -> R {
        LAST_RESULT.with(|last_result| {
            let last_result = last_result.borrow();
            let last_result = last_result.as_ref().map(|last_result| {
                let msg = last_result.err.as_ref().map(|msg| msg.as_ref());
                (last_result.value, msg)
            });

            f(last_result)
        })
    }
}