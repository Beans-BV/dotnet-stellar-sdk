using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Compatibility;

namespace StellarDotnetSdk.Tests.Compatibility;

/// <summary>
///     Cross-TFM parity tests for the netstandard2.1 compatibility helpers: the polyfills must surface the
///     same exception types and messages as the BCL members they stand in for, so consumers observe identical
///     behavior no matter which assembly NuGet resolves. These run on every test leg — on net8.0/net10.0 they
///     pin the BCL behavior the polyfills mirror; on the netstandard2.1 leg they exercise the polyfills.
/// </summary>
[TestClass]
public class CompatibilityParityTest
{
    [TestMethod]
    public void ThrowIfNullOrEmpty_WithEmptyString_MatchesBclMessageAndParamName()
    {
        var ex = Assert.ThrowsException<ArgumentException>(() => Throw.IfNullOrEmpty("", "value"));

        Assert.AreEqual("value", ex.ParamName);
        StringAssert.StartsWith(ex.Message, "The value cannot be an empty string.");
    }

    [TestMethod]
    public void ThrowIfNullOrEmpty_WithNull_ThrowsArgumentNullException()
    {
        var ex = Assert.ThrowsException<ArgumentNullException>(() => Throw.IfNullOrEmpty(null, "value"));

        Assert.AreEqual("value", ex.ParamName);
    }

#if TEST_SDK_NETSTANDARD21
    /// <summary>
    ///     The netstandard2.1 ReadAsStringAsync shim must surface cancellation as TaskCanceledException —
    ///     the type the real HttpContent.ReadAsStringAsync(CancellationToken) overload throws on net8.0+ —
    ///     so a consumer's <c>catch (TaskCanceledException)</c> behaves identically across TFMs.
    /// </summary>
    [TestMethod]
    public async Task ReadAsStringAsyncShim_PreCanceledToken_ThrowsTaskCanceledException()
    {
        using var content = new StringContent("body");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var ex = await Assert.ThrowsExceptionAsync<TaskCanceledException>(() =>
            HttpContentExtensions.ReadAsStringAsync(content, cts.Token));
        Assert.AreEqual(cts.Token, ex.CancellationToken);
    }

    /// <summary>
    ///     Same parity for the mid-read cancellation path: the shim races the body read against the token,
    ///     and losing that race must also surface as TaskCanceledException, not the base
    ///     OperationCanceledException.
    /// </summary>
    [TestMethod]
    public async Task ReadAsStringAsyncShim_CanceledMidRead_ThrowsTaskCanceledException()
    {
        using var content = new NeverCompletingContent();
        using var cts = new CancellationTokenSource();

        var readTask = HttpContentExtensions.ReadAsStringAsync(content, cts.Token);
        cts.Cancel();

        var ex = await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => readTask);
        Assert.AreEqual(cts.Token, ex.CancellationToken);
    }

    private sealed class NeverCompletingContent : HttpContent
    {
        private readonly TaskCompletionSource<bool> _never = new();

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            return _never.Task;
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }
#endif
}
