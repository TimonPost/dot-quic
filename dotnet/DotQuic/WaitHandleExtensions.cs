using System.Threading;
using System.Threading.Tasks;

namespace DotQuic
{
    internal static class WaitHandleExtensions
    {
        public static Task AsTask(this WaitHandle handle)
        {
            return AsTask(handle, new CancellationToken());
        }

        public static Task AsTask(this WaitHandle handle, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<object>();

            cancellationToken.Register(() => { tcs.SetCanceled(cancellationToken); });


            var registration = ThreadPool.RegisterWaitForSingleObject(handle, (state, timedOut) =>
            {
                var localTcs = (TaskCompletionSource<object>)state;
                if (timedOut)
                    localTcs.TrySetCanceled();
                else
                    localTcs.TrySetResult(null);
            }, tcs, Timeout.InfiniteTimeSpan, true);
            tcs.Task.ContinueWith((_, state) => ((RegisteredWaitHandle)state).Unregister(null), registration,
                TaskScheduler.Default);
            return tcs.Task;
        }
    }
}