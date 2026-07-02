#if NETSTANDARD2_1
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace StellarDotnetSdk.Compatibility;

internal static class HttpContentExtensions
{
    public static async Task<string> ReadAsStringAsync(this HttpContent content, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            // Match the real ReadAsStringAsync(CancellationToken) overload on net8.0+, which surfaces
            // cancellation as TaskCanceledException (with the token attached), not the base
            // OperationCanceledException that ThrowIfCancellationRequested would produce.
            return await Task.FromCanceled<string>(cancellationToken).ConfigureAwait(false);
        }

        if (!cancellationToken.CanBeCanceled)
        {
            return await content.ReadAsStringAsync().ConfigureAwait(false);
        }

        // HttpContent.ReadAsStringAsync() has no token overload before .NET 6, so race the read against
        // a cancellation signal to avoid waiting for the full body when the caller cancels mid-read.
        var readTask = content.ReadAsStringAsync();
        var cancellationTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using (cancellationToken.Register(static state => ((TaskCompletionSource<bool>)state!).TrySetResult(true), cancellationTcs))
        {
            var completed = await Task.WhenAny(readTask, cancellationTcs.Task).ConfigureAwait(false);
            if (completed != readTask)
            {
                // Observe the eventual read result/exception so it is not left unobserved.
                _ = readTask.ContinueWith(static t => _ = t.Exception, TaskContinuationOptions.OnlyOnFaulted);
                // TaskCanceledException for type parity with the net8.0+ overload (see above).
                return await Task.FromCanceled<string>(cancellationToken).ConfigureAwait(false);
            }
        }

        return await readTask.ConfigureAwait(false);
    }
}
#endif
