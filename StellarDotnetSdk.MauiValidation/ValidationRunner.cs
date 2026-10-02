using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.EventSources;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Responses.Operations;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Transactions;

namespace StellarDotnetSdk.MauiValidation;

/// <summary>
///     The validation checks. Each check either returns a one-line detail (PASS) or throws (FAIL). Every line is
///     written to the console with <see cref="LogPrefix" />; on Android that lands in logcat under the
///     <c>DOTNET</c> tag. On iOS it is expected in the device console (not verified: iOS was not run).
/// </summary>
public sealed class ValidationRunner
{
    public const string LogPrefix = "STELLAR-MAUI-VALIDATION";

    private const string HorizonTestnetUrl = "https://horizon-testnet.stellar.org";
    private const string RpcTestnetUrl = "https://soroban-testnet.stellar.org";

    // Stellar Asset Contract for native XLM on Testnet.
    private const string NativeSacTestnet = "CDLZFC3SYJYDZT7K67VZ75HPJVIEUVNIXF47ZG2FB2RMQQVU2HHGCYSC";

    // RFC 8032 section 7.1, TEST 1 (empty message).
    private const string Rfc8032Seed = "9d61b19deffd5a60ba844af492ec2cc44449c5697b326919703bac031cae7f60";
    private const string Rfc8032PublicKey = "d75a980182b10ab7d54bfed3c964073a0ee172f3daa62325af021a68f707511a";
    private const string Rfc8032Signature =
        "e5564300c360ac729086e2cc806e828a84877f1eb8e5d974d873e065224901555fb8821590a33bacc61e39701cf9b46bd25bf5f0595bbe24655141438e7a100b";

    private static readonly TimeSpan NetworkTimeout = TimeSpan.FromSeconds(75);
    private static readonly TimeSpan SseMaxDelay = TimeSpan.FromSeconds(20);

    private readonly Func<bool>? _isUiThread;
    private readonly Action<string> _onLine;
    private KeyPair? _funded;
    private SseTiming? _socketsTiming;
    // The ledger of the last payment the submit check applied, kept across its retry; null when none succeeded.
    private long? _submittedLedger;

    /// <param name="onLine">Receives every result line, for display.</param>
    /// <param name="isUiThread">
    ///     Reports whether the caller is on the UI thread. When given, the SSE check asserts that it starts its stream
    ///     from the UI thread, as a button handler would. The desktop console passes nothing.
    /// </param>
    public ValidationRunner(Action<string> onLine, Func<bool>? isUiThread = null)
    {
        _onLine = onLine;
        _isUiThread = isUiThread;
    }

    /// <returns>True when every check passed.</returns>
    public async Task<bool> RunAllAsync()
    {
        var allPassed = false;
        try
        {
            Network.UseTestNetwork();
            Log($"runtime {RuntimeInformation.FrameworkDescription} rid={RuntimeInformation.RuntimeIdentifier} " +
                $"os={RuntimeInformation.OSDescription} dynamicCode={RuntimeFeature.IsDynamicCodeSupported} " +
                $"dynamicCodeCompiled={RuntimeFeature.IsDynamicCodeCompiled}");
            var checks = new (string Name, Func<Task<string>> Run)[]
            {
                ("crypto.rfc8032", () => Task.FromResult(CheckRfc8032KnownAnswer())),
                ("crypto.random-keypair", () => Task.FromResult(CheckRandomKeyPair())),
                ("horizon.friendbot-and-account", CheckFriendbotAndAccountAsync),
                ("horizon.submit-and-sse-stream", CheckSubmitAndStreamWithRetryAsync),
                ("horizon.sse-stream-sockets-handler", () => Task.FromResult(CheckSseSocketsHandler())),
                ("soroban.simulate", CheckSorobanSimulateAsync),
            };
            var passed = 0;
            foreach (var (name, run) in checks)
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    var detail = await run();
                    passed++;
                    Log($"PASS {name} ({sw.ElapsedMilliseconds} ms): {detail}");
                }
                catch (Exception ex)
                {
                    LogFailure(name, sw.ElapsedMilliseconds, ex);
                }
            }
            // Which SDK and NSec builds ran: the csproj can swap either for a published package.
            Log($"INFO assemblies {DescribeAssembly("StellarDotnetSdk")}, {DescribeAssembly("NSec.Cryptography")}");
            allPassed = passed == checks.Length;
            Log($"RESULT {(allPassed ? "PASS" : "FAIL")} {passed}/{checks.Length}");

            // Informational: how quickly each HTTP handler delivers streamed SSE bytes on this platform. Run on the
            // thread pool and in parallel: this measures buffering, not the UI-thread rules checked above.
            var handlers = CreateSseHandlers().ToList();
            var latencies = await Task.WhenAll(handlers.Select(h => Task.Run(() => MeasureSseFirstEventAsync(h.Handler))));
            for (var i = 0; i < handlers.Count; i++)
            {
                Log($"INFO sse-latency {handlers[i].Name}: {latencies[i]}");
            }
        }
        catch (Exception ex)
        {
            allPassed = false;
            LogFailure("runner", 0, ex);
        }
        finally
        {
            if (_funded != null)
            {
                AsDisposableOrNull(_funded)?.Dispose();
            }
            // Per-run state: a later run on this instance must not reuse the disposed account or the old timing.
            _funded = null;
            _socketsTiming = null;
            _submittedLedger = null;
            Log("DONE");
        }
        return allPassed;
    }

    private static IEnumerable<(string Name, HttpMessageHandler Handler)> CreateSseHandlers()
    {
        yield return ("HttpClientHandler (platform default)", new HttpClientHandler());
        yield return ("SocketsHttpHandler", new SocketsHttpHandler());
#if ANDROID
        yield return ("AndroidMessageHandler", new Xamarin.Android.Net.AndroidMessageHandler());
#endif
    }

    /// <summary>
    ///     Opens Horizon's ledger stream (a new ledger every ~5 s) with a bare HttpClient and reports when the first
    ///     ledger event's bytes arrive. Horizon closes an SSE connection after 10 events or about 55 s, so a result
    ///     near 50 s means the handler buffered the response until the connection closed.
    /// </summary>
    private static async Task<string> MeasureSseFirstEventAsync(HttpMessageHandler handler)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            using var client = new HttpClient(handler);
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(70));
            var (response, stream) = await OpenSseAsync(client, $"{HorizonTestnetUrl}/ledgers?cursor=now", cts.Token);
            using var responseLifetime = response;
            await using var streamLifetime = stream;
            var headersMs = sw.ElapsedMilliseconds;
            var buffer = new byte[4096];
            var text = new System.Text.StringBuilder();
            long firstBytesMs = -1;
            int read;
            while ((read = await stream.ReadAsync(buffer, cts.Token)) > 0)
            {
                if (firstBytesMs < 0)
                {
                    firstBytesMs = sw.ElapsedMilliseconds;
                }
                text.Append(System.Text.Encoding.UTF8.GetString(buffer, 0, read));
                if (text.ToString().Contains("\nid: "))
                {
                    return $"headers {headersMs} ms, first bytes {firstBytesMs} ms, first ledger event {sw.ElapsedMilliseconds} ms " +
                           $"(content-encoding: {string.Join(",", response.Content.Headers.ContentEncoding)})";
                }
            }
            return $"stream ended after {sw.ElapsedMilliseconds} ms without a ledger event";
        }
        catch (Exception ex)
        {
            return $"{ex.GetType().FullName} after {sw.ElapsedMilliseconds} ms: {ex.Message}";
        }
    }

    /// <summary>Sends an SSE GET request and returns the response with its body stream, headers read.</summary>
    private static async Task<(HttpResponseMessage Response, Stream Stream)> OpenSseAsync(HttpClient client, string url,
        CancellationToken ct)
    {
        client.Timeout = Timeout.InfiniteTimeSpan;
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Accept.ParseAdd("text/event-stream");
        var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        return (response, await response.Content.ReadAsStreamAsync(ct));
    }

    private static string DescribeAssembly(string name)
    {
        var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == name);
        var version = assembly?.GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false)
            .OfType<System.Reflection.AssemblyInformationalVersionAttribute>().FirstOrDefault()?.InformationalVersion;
        return $"{name} {version ?? assembly?.GetName().Version?.ToString() ?? "not loaded"}";
    }

    private void Log(string line)
    {
        Console.WriteLine($"{LogPrefix} {line}");
        _onLine(line);
    }

    /// <summary>
    ///     Logs a FAIL line and then the full exception, one prefixed line per line of text: logcat stores each line
    ///     of a multi-line message as its own entry, and only prefixed entries are collected by run-android.sh.
    /// </summary>
    private void LogFailure(string name, long elapsedMs, Exception ex)
    {
        Log($"FAIL {name} ({elapsedMs} ms): {ex.GetType().FullName}: {ex.Message.ReplaceLineEndings(" ")}");
        foreach (var line in ex.ToString().ReplaceLineEndings("\n").Split('\n'))
        {
            Console.WriteLine($"{LogPrefix} DETAIL {name}: {line}");
        }
    }

    /// <summary>
    ///     Returns <paramref name="keyPair" /> as <see cref="IDisposable" /> when the SDK in use makes it disposable,
    ///     and null otherwise. Published packages up to 15.1.0 do not, and the app can be built against them
    ///     (see the csproj).
    /// </summary>
    private static IDisposable? AsDisposableOrNull(KeyPair keyPair)
    {
        return (object)keyPair as IDisposable;
    }

    private KeyPair RequireFunded()
    {
        return _funded ?? throw new InvalidOperationException("no funded account (friendbot check failed)");
    }

    /// <summary>Deterministic check that the native backend loaded and produces the RFC 8032 vector.</summary>
    private static string CheckRfc8032KnownAnswer()
    {
        var keyPair = KeyPair.FromSecretSeed(Convert.FromHexString(Rfc8032Seed));
        using var keyPairLifetime = AsDisposableOrNull(keyPair);
        Expect(Convert.ToHexString(keyPair.PublicKey).Equals(Rfc8032PublicKey, StringComparison.OrdinalIgnoreCase),
            "public key does not match RFC 8032");
        var signature = keyPair.Sign([]);
        Expect(Convert.ToHexString(signature).Equals(Rfc8032Signature, StringComparison.OrdinalIgnoreCase),
            "signature does not match RFC 8032");
        Expect(keyPair.Verify([], signature), "known-good signature failed to verify");
        return "public key and signature match RFC 8032 test 1";
    }

    private static string CheckRandomKeyPair()
    {
        var keyPair = KeyPair.Random();
        using var keyPairLifetime = AsDisposableOrNull(keyPair);
        var data = "stellar maui validation"u8.ToArray();
        var signature = keyPair.Sign(data);
        Expect(keyPair.Verify(data, signature), "fresh signature failed to verify");
        signature[0] ^= 0x01;
        Expect(!keyPair.Verify(data, signature), "tampered signature verified");
        var watchOnly = KeyPair.FromAccountId(keyPair.AccountId);
        using var watchOnlyLifetime = AsDisposableOrNull(watchOnly);
        Expect(!watchOnly.CanSign(), "public-only key pair claims it can sign");
        // The StrKey string round trip is what this checks, so the seed goes through a string on purpose.
        var roundTripped = KeyPair.FromSecretSeed(keyPair.SecretSeed!);
        using var roundTrippedLifetime = AsDisposableOrNull(roundTripped);
        Expect(roundTripped.AccountId == keyPair.AccountId, "StrKey seed round trip changed the account");
        return $"sign/verify/tamper/StrKey ok for {keyPair.AccountId}";
    }

    private async Task<string> CheckFriendbotAndAccountAsync()
    {
        using var server = new Server(HorizonTestnetUrl);
        var keyPair = KeyPair.Random();
        var keyPairLifetime = AsDisposableOrNull(keyPair);
        try
        {
            await server.TestNetFriendBot.FundAccount(keyPair.AccountId).Execute().WaitAsync(NetworkTimeout);
            var account = await server.Accounts.Account(keyPair.AccountId).WaitAsync(NetworkTimeout);
            Expect(account.AccountId == keyPair.AccountId, $"Horizon returned account {account.AccountId}");
            Expect(account.SequenceNumber > 0, $"funded account has sequence {account.SequenceNumber}");
            var native = account.Balances.Single(b => b.AssetType == "native");
            Expect(decimal.Parse(native.BalanceString, CultureInfo.InvariantCulture) > 0,
                "funded account has no native balance");
            _funded = keyPair;
            keyPairLifetime = null;
            return $"funded {keyPair.AccountId}, sequence {account.SequenceNumber}, balance {native.BalanceString}";
        }
        finally
        {
            keyPairLifetime?.Dispose();
        }
    }

    /// <summary>
    ///     Runs <see cref="CheckSubmitAndStreamAsync" />, and once more if its timing was inconclusive: a slow Testnet
    ///     submit says nothing about the SDK, and the rule that catches it cannot be relaxed (see
    ///     <see cref="ExpectPromptDelivery" />). A second inconclusive measurement fails the check.
    /// </summary>
    private async Task<string> CheckSubmitAndStreamWithRetryAsync()
    {
        try
        {
            return await CheckSubmitAndStreamAsync();
        }
        catch (InconclusiveException ex)
        {
            Log($"INFO horizon.submit-and-sse-stream measuring again: {ex.Message.ReplaceLineEndings(" ")}");
            _socketsTiming = null;
            return await CheckSubmitAndStreamAsync();
        }
    }

    /// <summary>
    ///     Opens an SSE payments stream on the funded account, then submits a signed CreateAccount transaction from
    ///     it and waits for the stream to deliver that operation. The same stream is also opened with
    ///     <see cref="SocketsHttpHandler" />; its timing is judged by <see cref="CheckSseSocketsHandler" />.
    /// </summary>
    private async Task<string> CheckSubmitAndStreamAsync()
    {
        var source = RequireFunded();
        using var server = new Server(HorizonTestnetUrl);
        var destination = KeyPair.Random();
        using var destinationLifetime = AsDisposableOrNull(destination);
        // LaunchDarkly does not dispose a handler it is given.
        using var socketsHandler = new SocketsHttpHandler();
        using var rawCts = new CancellationTokenSource(NetworkTimeout);
        var sw = Stopwatch.StartNew();
        // Delivery times are taken in the listeners, not after the waits below.
        var streamed = new TaskCompletionSource<(OperationResponse Op, long Ms)>(TaskCreationOptions.RunContinuationsAsynchronously);
        var opened = new TaskCompletionSource<long>(TaskCreationOptions.RunContinuationsAsynchronously);
        var socketsStreamed = new TaskCompletionSource<long>(TaskCreationOptions.RunContinuationsAsynchronously);
        var socketsOpened = new TaskCompletionSource<long>(TaskCreationOptions.RunContinuationsAsynchronously);
        // Connection history, reported with the result: SSE problems on mobile show up as reconnects or delays.
        var history = new System.Collections.Concurrent.ConcurrentQueue<string>();
        IEventSource? eventSource = null;
        IEventSource? socketsSource = null;
        var connect = Task.CompletedTask;
        var socketsConnect = Task.CompletedTask;
        try
        {
            eventSource = server.Payments.ForAccount(source.AccountId).Cursor("now").Stream((_, op) =>
            {
                if (op is CreateAccountOperationResponse created && created.Account == destination.AccountId)
                {
                    streamed.TrySetResult((op, sw.ElapsedMilliseconds));
                }
            });
            eventSource.StateChange += (_, e) =>
            {
                history.Enqueue($"{sw.ElapsedMilliseconds}ms:{e.NewState}");
                if (e.NewState == EventSource.EventSourceState.OPEN)
                {
                    opened.TrySetResult(sw.ElapsedMilliseconds);
                }
            };
            eventSource.Message += (_, e) =>
                history.Enqueue($"{sw.ElapsedMilliseconds}ms:message {e.Data?[..Math.Min(e.Data.Length, 24)]}");
            eventSource.Error += (_, e) =>
                history.Enqueue($"{sw.ElapsedMilliseconds}ms:error {e.Exception?.GetType().Name}: {e.Exception?.Message}");
            // Started from the UI thread in the MAUI app, as a button handler would. LaunchDarkly then reads the
            // response on thread-pool threads, so this covers starting a stream there, not reading on it.
            if (_isUiThread != null)
            {
                Expect(_isUiThread(), "the stream was not started from the UI thread");
            }
            connect = eventSource.Connect();
            // The same SDK stream with SocketsHttpHandler instead of the platform default handler: the
            // consumer-side workaround for the Android delay (docs/maui-compatibility.md §3.3).
            var socketsBuilder = server.Payments.ForAccount(source.AccountId).Cursor("now");
            socketsBuilder.EventSource = new SseEventSource(socketsBuilder.BuildUri(),
                config => config.MessageHandler(socketsHandler));
            socketsSource = socketsBuilder.Stream((_, op) =>
            {
                if (op is CreateAccountOperationResponse created && created.Account == destination.AccountId)
                {
                    socketsStreamed.TrySetResult(sw.ElapsedMilliseconds);
                }
            });
            socketsSource.StateChange += (_, e) =>
            {
                if (e.NewState == EventSource.EventSourceState.OPEN)
                {
                    socketsOpened.TrySetResult(sw.ElapsedMilliseconds);
                }
            };
            socketsConnect = socketsSource.Connect();
            // Same URL read with a bare HttpClient, to tell handler/network buffering apart from EventSource
            // parsing. Comparison only: these readers never fail the check.
            var paymentsUrl = $"{HorizonTestnetUrl}/accounts/{source.AccountId}/payments?cursor=now";
            var rawBytesOpened = new TaskCompletionSource<long>(TaskCreationOptions.RunContinuationsAsynchronously);
            var rawLinesOpened = new TaskCompletionSource<long>(TaskCreationOptions.RunContinuationsAsynchronously);
            var rawLinesSocketsOpened = new TaskCompletionSource<long>(TaskCreationOptions.RunContinuationsAsynchronously);
            var rawBytes = Task.Run(() =>
                ReadSseUntilAsync(paymentsUrl, destination.AccountId, sw, false, false, rawBytesOpened, rawCts.Token));
            var rawLines = Task.Run(() =>
                ReadSseUntilAsync(paymentsUrl, destination.AccountId, sw, true, false, rawLinesOpened, rawCts.Token));
            var rawLinesSockets = Task.Run(() =>
                ReadSseUntilAsync(paymentsUrl, destination.AccountId, sw, true, true, rawLinesSocketsOpened, rawCts.Token));

            // Submit only after the default stream is open, so cursor "now" precedes the submitted ledger. The
            // SocketsHttpHandler stream and the comparison readers use cursor "now" too, so they get a short grace
            // period to open (they usually open within 1-3 s; the wait is capped at 5 s): a stream that fails to open
            // is judged by its own check, and a reader reports when it opened.
            var openedMs = await opened.Task.WaitAsync(NetworkTimeout);
            await Task.WhenAny(
                Task.WhenAll(socketsOpened.Task, rawBytesOpened.Task, rawLinesOpened.Task, rawLinesSocketsOpened.Task),
                Task.Delay(TimeSpan.FromSeconds(5)));
            var account = await server.Accounts.Account(source.AccountId).WaitAsync(NetworkTimeout);
            // Time bounds, so a submit this check gave up on cannot be applied long afterwards. Five minutes is longer
            // than the submit timeout, to tolerate a device clock that is somewhat off; a submit that times out here
            // can still be applied within that window.
            var tx = new TransactionBuilder(account)
                .AddOperation(new CreateAccountOperation(destination, "2", source))
                .AddTimeBounds(new TimeBounds(0L, DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds()))
                .Build();
            tx.Sign(source);
            // The payment can land in a ledger from here on, so a reader that opens later can miss it.
            var submitStartMs = sw.ElapsedMilliseconds;
            var response = await server.SubmitTransaction(tx).WaitAsync(NetworkTimeout)
                           ?? throw new InvalidOperationException("SubmitTransaction returned null");
            Expect(response.IsSuccess, $"submit failed: result {response.ResultXdr}");
            _submittedLedger = Math.Max(_submittedLedger ?? 0, response.Ledger!.Value);
            var submittedMs = sw.ElapsedMilliseconds;

            // Wait for the default stream, then give the SocketsHttpHandler stream until the same deadline its own
            // check applies. Record its result and the comparison readers before reading the default result, so
            // that they survive a default stream that never delivers.
            await Task.WhenAny(streamed.Task, Task.Delay(NetworkTimeout));
            var socketsDeadline = submittedMs + (long)SseMaxDelay.TotalMilliseconds - sw.ElapsedMilliseconds;
            await Task.WhenAny(socketsStreamed.Task, Task.Delay(TimeSpan.FromMilliseconds(Math.Max(0, socketsDeadline))));
            _socketsTiming = new SseTiming(
                socketsOpened.Task.IsCompletedSuccessfully ? socketsOpened.Task.Result : null,
                submittedMs,
                socketsStreamed.Task.IsCompletedSuccessfully ? socketsStreamed.Task.Result : null);
            history.Enqueue(
                $"SDK stream with SocketsHttpHandler delivered it {(_socketsTiming.EventMs is { } ms ? $"at {ms} ms" : "never (not within the limit)")}");
            // Each reader's open time is read after its result, when the reader has finished and set it.
            var rawBytesResult = await rawBytes;
            var rawLinesResult = await rawLines;
            var rawLinesSocketsResult = await rawLinesSockets;
            history.Enqueue($"raw HttpClient Stream.ReadAsync {DescribeOpen(rawBytesOpened, submitStartMs)} saw it {rawBytesResult}");
            history.Enqueue($"raw HttpClient StreamReader.ReadLineAsync {DescribeOpen(rawLinesOpened, submitStartMs)} saw it {rawLinesResult}");
            history.Enqueue($"raw SocketsHttpHandler StreamReader.ReadLineAsync {DescribeOpen(rawLinesSocketsOpened, submitStartMs)} saw it {rawLinesSocketsResult}");
            // Judged for both streams before anything else, so that a slow submit is measured again (see
            // CheckSubmitAndStreamWithRetryAsync) before either check fails on it, delivered or not.
            var timingDetail = $"stream open {openedMs} ms, submit done {submittedMs} ms; states [{string.Join(", ", history)}]";
            ExpectConclusive(openedMs, submittedMs, timingDetail);
            if (_socketsTiming.OpenedMs is { } socketsOpenedMs)
            {
                ExpectConclusive(socketsOpenedMs, submittedMs, timingDetail);
            }
            // Zero timeout: the result if the stream already delivered, a TimeoutException if it did not.
            var (op, eventMs) = await streamed.Task.WaitAsync(TimeSpan.Zero);
            var detail = $"submitted {response.Hash} (ledger {response.Ledger}); SSE delivered {op.Type} op {op.Id}; " +
                         $"stream open {openedMs} ms, submit done {submittedMs} ms, event {eventMs} ms; " +
                         $"states [{string.Join(", ", history)}]";
            ExpectPromptDelivery("SSE event", openedMs, submittedMs, eventMs, detail);
            return detail;
        }
        catch (TimeoutException ex)
        {
            throw new TimeoutException($"{ex.Message} States [{string.Join(", ", history)}]", ex);
        }
        finally
        {
            rawCts.Cancel();
            eventSource?.Shutdown();
            eventSource?.Dispose();
            socketsSource?.Shutdown();
            socketsSource?.Dispose();
            await Task.WhenAny(Task.WhenAll(connect, socketsConnect), Task.Delay(TimeSpan.FromSeconds(5)));
        }
    }

    /// <summary>
    ///     Judges the <see cref="SocketsHttpHandler" /> stream that <see cref="CheckSubmitAndStreamAsync" /> opened
    ///     alongside the default one, with the same rules.
    /// </summary>
    private string CheckSseSocketsHandler()
    {
        var timing = _socketsTiming ?? throw new InvalidOperationException("no measurement (the submit-and-sse-stream check failed before the submit)");
        var openedMs = timing.OpenedMs ?? throw new InvalidOperationException("the SocketsHttpHandler stream never opened");
        var eventMs = timing.EventMs ?? throw new InvalidOperationException("the SocketsHttpHandler stream did not deliver the operation");
        var detail = $"stream open {openedMs} ms, submit done {timing.SubmittedMs} ms, event {eventMs} ms";
        ExpectPromptDelivery("SSE event (SocketsHttpHandler)", openedMs, timing.SubmittedMs, eventMs, detail);
        return detail;
    }

    /// <summary>
    ///     Fails unless the event arrived within <see cref="SseMaxDelay" /> of the submit, and the submit came within
    ///     <see cref="SseMaxDelay" /> of the stream opening. Horizon closes a quiet stream about 55 s after it opens,
    ///     and a held-back event is delivered at that close; the second rule keeps a slow submit from making such a
    ///     delivery look prompt.
    /// </summary>
    /// <remarks>
    ///     The two rules share <see cref="SseMaxDelay" /> on purpose. A longer window for the submit would let a
    ///     held-back event pass: with a submit 40 s after the stream opened, an event delivered at the close about
    ///     55 s in is only 15 s after the submit. The first window must stay well under 55 s minus the second.
    /// </remarks>
    private static void ExpectPromptDelivery(string what, long openedMs, long submittedMs, long eventMs, string detail)
    {
        ExpectConclusive(openedMs, submittedMs, detail);
        Expect(eventMs - submittedMs <= SseMaxDelay.TotalMilliseconds,
            $"{what} arrived {eventMs - submittedMs} ms after submit (limit {SseMaxDelay.TotalMilliseconds} ms): {detail}");
    }

    /// <summary>
    ///     Reads an SSE URL with a bare HttpClient until <paramref name="needle" /> appears and describes when it was
    ///     seen. <paramref name="isLineReader" /> reads with <see cref="StreamReader.ReadLineAsync(CancellationToken)" />
    ///     (as LaunchDarkly.EventSource 3.x does) instead of raw <see cref="Stream.ReadAsync(Memory{byte}, CancellationToken)" />,
    ///     and also reports when the event's <c>id:</c> line arrived. <paramref name="isSocketsHandler" /> uses
    ///     <see cref="SocketsHttpHandler" /> instead of the platform default handler. <paramref name="opened" /> gets the
    ///     time the response headers arrived, or -1 if they never did. Never throws.
    /// </summary>
    private static async Task<string> ReadSseUntilAsync(string url, string needle, Stopwatch sw, bool isLineReader,
        bool isSocketsHandler, TaskCompletionSource<long> opened, CancellationToken ct)
    {
        try
        {
            using var client = isSocketsHandler ? new HttpClient(new SocketsHttpHandler()) : new HttpClient();
            var (response, stream) = await OpenSseAsync(client, url, ct);
            opened.TrySetResult(sw.ElapsedMilliseconds);
            using var responseLifetime = response;
            await using var streamLifetime = stream;
            if (isLineReader)
            {
                using var reader = new StreamReader(stream);
                long idLineMs = -1;
                string? line;
                while ((line = await reader.ReadLineAsync(ct)) != null)
                {
                    if (line.StartsWith("id: ", StringComparison.Ordinal))
                    {
                        idLineMs = sw.ElapsedMilliseconds;
                    }
                    if (line.Contains(needle))
                    {
                        return $"at {sw.ElapsedMilliseconds} ms (its id: line at {idLineMs} ms)";
                    }
                }
                return "never (stream ended)";
            }
            var buffer = new byte[4096];
            var text = new System.Text.StringBuilder();
            int read;
            while ((read = await stream.ReadAsync(buffer, ct)) > 0)
            {
                text.Append(System.Text.Encoding.UTF8.GetString(buffer, 0, read));
                if (text.ToString().Contains(needle))
                {
                    return $"at {sw.ElapsedMilliseconds} ms";
                }
            }
            return "never (stream ended)";
        }
        catch (OperationCanceledException)
        {
            return "never (timed out)";
        }
        catch (Exception ex)
        {
            return $"never ({ex.GetType().Name}: {ex.Message})";
        }
        finally
        {
            opened.TrySetResult(-1);
        }
    }

    /// <summary>
    ///     When a comparison reader opened. One that opened after the submit started can have missed the payment,
    ///     because it reads from cursor "now", so a "never" from it may be a miss rather than buffering.
    /// </summary>
    private static string DescribeOpen(TaskCompletionSource<long> opened, long submitStartMs)
    {
        if (opened.Task.Result < 0)
        {
            return "(never opened)";
        }
        return opened.Task.Result > submitStartMs
            ? $"(opened at {opened.Task.Result} ms, after the submit started: it may have missed the payment)"
            : $"(opened at {opened.Task.Result} ms)";
    }

    /// <summary>
    ///     Simulates the native SAC <c>balance(address)</c> call for the funded account and checks the decoded
    ///     <c>i128</c> against the account's native balance from Horizon. Horizon and the RPC node ingest ledgers
    ///     independently and can be a ledger apart right after the previous check's submit. Horizon's account is
    ///     re-read (briefly) until it shows that submit, and the simulation is repeated until the RPC node has reached
    ///     the ledger that last changed the account. An RPC node that does not report its ledger fails the check,
    ///     because the comparison would then prove nothing.
    /// </summary>
    private async Task<string> CheckSorobanSimulateAsync()
    {
        var source = RequireFunded();
        using var horizon = new Server(HorizonTestnetUrl);
        using var rpc = new StellarRpcServer(RpcTestnetUrl);
        var account = await horizon.Accounts.Account(source.AccountId).WaitAsync(NetworkTimeout);
        // Horizon's servers ingest independently too: a lagging one returns the account from before the payment.
        for (var attempt = 1; attempt < 5 && account.LastModifiedLedger < _submittedLedger; attempt++)
        {
            await Task.Delay(TimeSpan.FromSeconds(2));
            account = await horizon.Accounts.Account(source.AccountId).WaitAsync(NetworkTimeout);
        }
        Expect(!(account.LastModifiedLedger < _submittedLedger),
            $"Horizon returned the account at ledger {account.LastModifiedLedger}, before the payment in ledger {_submittedLedger}");
        var invoke = new InvokeContractOperation(
            NativeSacTestnet, "balance", [new ScAccountId(source.AccountId)], source);
        var tx = new TransactionBuilder(account).AddOperation(invoke).Build();
        var simulation = await rpc.SimulateTransaction(tx).WaitAsync(NetworkTimeout);
        for (var attempt = 1; attempt < 5 && simulation.LatestLedger < account.LastModifiedLedger; attempt++)
        {
            await Task.Delay(TimeSpan.FromSeconds(2));
            simulation = await rpc.SimulateTransaction(tx).WaitAsync(NetworkTimeout);
        }
        Expect(simulation.Error == null, $"simulation error: {simulation.Error}");
        Expect(simulation.LatestLedger >= account.LastModifiedLedger,
            $"RPC node at ledger {simulation.LatestLedger?.ToString() ?? "(not reported)"}, behind Horizon's ledger " +
            $"{account.LastModifiedLedger} for the account");
        var results = simulation.Results ?? [];
        Expect(results.Length == 1, $"simulation returned {results.Length} results, expected 1");
        var value = SCVal.FromXdrBase64(results[0].Xdr
                                        ?? throw new InvalidOperationException("simulation result has no XDR"));
        var balance = value as SCInt128
                      ?? throw new InvalidOperationException($"balance() returned {value.GetType().Name}, expected an i128");
        // Nothing else touches this account, and both sources have seen its last change, so the SAC balance
        // (stroops) equals Horizon's native balance.
        var native = account.Balances.Single(b => b.AssetType == "native");
        var expectedStroops = decimal.Parse(native.BalanceString, CultureInfo.InvariantCulture) * 10_000_000m;
        Expect(balance.Hi == 0 && balance.Lo == expectedStroops,
            $"balance() = (hi {balance.Hi}, lo {balance.Lo}), Horizon says {native.BalanceString} XLM = {expectedStroops:0} stroops");
        return $"balance() = {balance.Lo} stroops, matches Horizon; minResourceFee {simulation.MinResourceFee}, " +
               $"latestLedger {simulation.LatestLedger}";
    }

    private static void ExpectConclusive(long openedMs, long submittedMs, string detail)
    {
        if (submittedMs - openedMs > SseMaxDelay.TotalMilliseconds)
        {
            throw new InconclusiveException(
                $"inconclusive: submit finished {submittedMs - openedMs} ms after the stream opened (limit {SseMaxDelay.TotalMilliseconds} ms): {detail}");
        }
    }

    private static void Expect(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    /// <summary>The measurement cannot tell a prompt stream from a late one; says nothing about the SDK.</summary>
    private sealed class InconclusiveException(string message) : InvalidOperationException(message);

    /// <summary>Times on the check's stopwatch, in ms; null when the event did not happen.</summary>
    private sealed record SseTiming(long? OpenedMs, long SubmittedMs, long? EventMs);
}
