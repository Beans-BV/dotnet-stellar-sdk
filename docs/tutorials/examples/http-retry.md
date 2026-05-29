# HTTP Resilience Configuration

This guide explains how to configure optional HTTP retry, circuit breaker, and timeout behavior in the SDK.

> **Note:** Retries are **disabled by default** (matching the Java SDK's approach). Nothing is retried until you opt in by passing an `HttpResilienceOptions` instance (or a preset). When enabled, the SDK retries connection-level failures (network errors, DNS failures) and — if you configure them — the transient HTTP status codes you list in `RetryHttpStatusCodes`.

> **Behavior change (vs. earlier 2.x releases):** the SDK can now retry HTTP error status codes (e.g. 408/429/500/502/503/504), honor the `Retry-After` response header, and gate retries by HTTP method. Earlier versions retried connection failures only and never retried status codes. The `ForSorobanPolling()` preset now also retries those status codes for safe methods — see [Behavior changes](#behavior-changes-when-upgrading).

## Overview

The SDK provides an opt-in resilience pipeline (built on [Polly](https://www.pollydocs.org/)) for transient failures:

- **Connection failures** — `HttpRequestException`, timeouts, and HTTP-client `TaskCanceledException` (not user cancellation), similar to OkHttp's `retryOnConnectionFailure(true)`.
- **HTTP status codes** — only the codes you add to `RetryHttpStatusCodes` (empty by default), and only for safe methods unless you opt in to unsafe ones.

### Default Behavior

When using the SDK without any configuration, **no retries are performed**:

| Setting           | Default Value         |
|-------------------|-----------------------|
| Max Retry Count   | 0 (disabled)          |
| Base Delay        | 200ms                 |
| Max Delay         | 5000ms                |
| Jitter            | Enabled               |
| Status-code retries | None (`RetryHttpStatusCodes` empty) |
| Retry unsafe methods | Disabled (`false`) |
| Respect `Retry-After` | Enabled (`true`)  |
| Circuit Breaker   | Disabled              |
| Request Timeout   | None                  |

### What Gets Retried

When retries are enabled (`MaxRetryCount > 0`):

**Connection failures (exceptions) are always retried:**
- `HttpRequestException` — network errors, DNS failures
- `TimeoutException` — request timeouts
- `TaskCanceledException` — HTTP client timeouts (not user-initiated cancellation)

**HTTP status codes are retried only when you opt in** by adding them to `RetryHttpStatusCodes`. By default the set is empty, so no status code is retried. Status-code retries are additionally gated by HTTP method (see [Unsafe HTTP methods](#unsafe-http-methods)): by default only safe methods (`GET`, `HEAD`, `OPTIONS`) are retried.

## Quick Start with Presets

The SDK provides preset configurations for common use cases:

```csharp
using StellarDotnetSdk.Requests;

// No retries (same as new HttpResilienceOptions())
var defaultOptions = HttpResilienceOptionsPresets.Default();

// Industry-standard retries: 408/429/5xx for safe methods, honors Retry-After
var standard = HttpResilienceOptionsPresets.WithStandardRetries();

// Connection failures only (no status-code retries)
var connectionOnly = HttpResilienceOptionsPresets.WithConnectionRetries();

// Soroban polling: status-code retries + higher budget and longer delays
var sorobanOptions = HttpResilienceOptionsPresets.ForSorobanPolling();

// Low latency: fast failure for trading bots (connection failures only)
var tradingOptions = HttpResilienceOptionsPresets.LowLatency();
```

### Preset Details

| Preset                     | Max Retries | Base Delay | Max Delay | Status-code retries            | `Retry-After` |
|----------------------------|-------------|------------|-----------|--------------------------------|---------------|
| `Default()` / `NoRetry()`  | 0           | —          | —         | none                           | —             |
| `WithConnectionRetries()`  | 3           | 200ms      | 5s        | none (connection failures only)| n/a           |
| `WithStandardRetries()`    | 3           | 200ms      | 5s        | 408, 429, 500, 502, 503, 504 (safe methods) | honored |
| `ForSorobanPolling()`      | 5           | 500ms      | 15s       | 408, 429, 500, 502, 503, 504 (safe methods) | honored |
| `LowLatency()`             | 1           | 50ms       | 200ms     | none (connection failures only)| n/a           |

All retrying presets use exponential backoff with jitter enabled.

`WithStandardRetries()` matches the .NET industry standard (`Microsoft.Extensions.Http.Resilience.AddStandardResilienceHandler`) and is recommended for general-purpose Horizon/Soroban clients.

## Using the SDK Without Retries (Default)

By default, the SDK does not retry any requests:

```csharp
// No retries - requests fail immediately on connection errors,
// and HTTP error responses are surfaced immediately.
var server = new Server("https://horizon-testnet.stellar.org");
var sorobanServer = new StellarRpcServer("https://soroban-testnet.stellar.org");
```

## Enabling Retries

To enable retries, pass `HttpResilienceOptions` (or a preset) with `MaxRetryCount > 0`:

```csharp
using StellarDotnetSdk.Requests;

var resilienceOptions = HttpResilienceOptionsPresets.WithStandardRetries();

var httpClient = new DefaultStellarSdkHttpClient(resilienceOptions: resilienceOptions);
var server = new Server("https://horizon-testnet.stellar.org", httpClient);
```

## Retrying HTTP Status Codes

To retry HTTP error status codes, set `MaxRetryCount > 0` **and** add the codes to `RetryHttpStatusCodes`. (Setting status codes without a positive `MaxRetryCount` has no effect — there is no retry loop to enter.)

```csharp
using System.Net;
using StellarDotnetSdk.Requests;

var resilienceOptions = new HttpResilienceOptions
{
    MaxRetryCount = 3,
    RespectRetryAfter = true, // honor Retry-After on retried responses (default true)
};
resilienceOptions.RetryHttpStatusCodes.Add(HttpStatusCode.TooManyRequests);   // 429
resilienceOptions.RetryHttpStatusCodes.Add(HttpStatusCode.ServiceUnavailable); // 503

var httpClient = new DefaultStellarSdkHttpClient(resilienceOptions: resilienceOptions);
var server = new Server("https://horizon-testnet.stellar.org", httpClient);
```

The retry pipeline observes responses **inside** the HTTP handler chain, so it triggers *before* a status code is translated into a typed exception such as `TooManyRequestsException` or `ServiceUnavailableException`. If all retries are exhausted, the last response is surfaced as the usual typed exception.

### Unsafe HTTP methods

By default, status-code retries apply only to **safe** methods (`GET`, `HEAD`, `OPTIONS`). Unsafe methods (`POST`, `PUT`, `PATCH`, `DELETE`) are not retried, because replaying them can cause duplicate side effects (for example, submitting a transaction twice).

To retry unsafe methods anyway — only if your endpoint is idempotent or tolerates duplicates (e.g. via idempotency keys) — set `RetryUnsafeHttpMethods`:

```csharp
var resilienceOptions = new HttpResilienceOptions
{
    MaxRetryCount = 3,
    RetryUnsafeHttpMethods = true, // retry POST/PUT/PATCH/DELETE on configured status codes
};
resilienceOptions.RetryHttpStatusCodes.Add(HttpStatusCode.ServiceUnavailable);
```

### Honoring `Retry-After`

When `RespectRetryAfter` is `true` (the default) and a retried response carries a `Retry-After` header, the SDK uses that value as the next delay, **capped by `MaxDelay`**. Both the delay-seconds and HTTP-date forms (RFC 7231 §7.1.3) are supported. If no `Retry-After` header is present, the SDK falls back to exponential backoff.

The `TooManyRequestsException` and `ServiceUnavailableException` types also expose the value directly for manual handling:

```csharp
try
{
    var account = await server.Accounts.Account(accountId);
}
catch (TooManyRequestsException ex)
{
    // RetryAfter is whole seconds (int?); RetryAfterDelay is a TimeSpan? parsed from
    // the raw header at full precision and supports the HTTP-date form too.
    var delay = ex.RetryAfterDelay ?? TimeSpan.FromSeconds(5);
    await Task.Delay(delay);
}
```

**A note on `Retry-After` for Stellar services.** Horizon sends `Retry-After` only on HTTP 429 (always as an integer number of seconds); on HTTP 503 it relies on the client's configured backoff. Stellar RPC (Soroban) does not send `Retry-After` at all — overload is reported via HTTP 503/504 or JSON-RPC error bodies, so the pipeline falls back to exponential backoff. The parser still accepts the RFC 7231 HTTP-date form because upstream proxies and CDNs (Cloudflare, nginx, API gateways) may rewrite the header.

## Custom Retry Configuration

```csharp
var resilienceOptions = new HttpResilienceOptions
{
    MaxRetryCount = 5,                           // Retry up to 5 times
    BaseDelay = TimeSpan.FromMilliseconds(500),  // Start with 500ms delay
    MaxDelay = TimeSpan.FromSeconds(10),         // Cap delay at 10 seconds
    UseJitter = true,                            // Add randomness to prevent thundering herd
};

var httpClient = new DefaultStellarSdkHttpClient(resilienceOptions: resilienceOptions);
var server = new Server("https://horizon-testnet.stellar.org", httpClient);
```

### Adding Custom Retriable Exception Types

You can add additional exception types that should trigger retries (in addition to the defaults):

```csharp
var resilienceOptions = new HttpResilienceOptions
{
    MaxRetryCount = 3,
};
resilienceOptions.AdditionalRetriableExceptionTypes.Add(typeof(SocketException));

var httpClient = new DefaultStellarSdkHttpClient(resilienceOptions: resilienceOptions);
```

> **Scope:** `AdditionalRetriableExceptionTypes` applies only to exceptions thrown from within the HTTP handler chain (`HttpClient.SendAsync`). It does **not** apply to typed exceptions thrown after a response is received (`TooManyRequestsException`, `ServiceUnavailableException`, `HttpResponseException`). To retry those, use `RetryHttpStatusCodes` instead.

## Circuit Breaker (Advanced)

The circuit breaker prevents cascading failures by temporarily blocking requests to an unhealthy service. It's **disabled by default**.

> **Warning:** When the circuit is open, requests throw `BrokenCircuitException` instead of the underlying HTTP error. The circuit breaker counts the same outcomes the retry pipeline handles — including any configured `RetryHttpStatusCodes` — as failures, so a sustained burst of 429/503 responses can open it.

```csharp
var resilienceOptions = new HttpResilienceOptions
{
    MaxRetryCount = 3,
    EnableCircuitBreaker = true,
    FailureRatio = 0.5,           // Open circuit when 50% of requests fail
    MinimumThroughput = 10,        // Require at least 10 requests before evaluating
    SamplingDuration = TimeSpan.FromSeconds(30),
    BreakDuration = TimeSpan.FromSeconds(30),
};

var httpClient = new DefaultStellarSdkHttpClient(resilienceOptions: resilienceOptions);
```

## Request Timeout (Advanced)

You can set an overall timeout for the whole operation. It is applied as the outermost strategy, so it covers
all retry attempts and their backoff delays — not each attempt individually:

```csharp
var resilienceOptions = new HttpResilienceOptions
{
    MaxRetryCount = 3,
    RequestTimeout = TimeSpan.FromSeconds(10),  // The whole operation (all retries) must finish within 10s
};

var httpClient = new DefaultStellarSdkHttpClient(resilienceOptions: resilienceOptions);
```

## Exponential Backoff

When retries are enabled and no `Retry-After` header applies, the SDK uses exponential backoff:

```
delay = min(BaseDelay * 2^attempt, MaxDelay)
```

With jitter enabled (default), the actual delay is randomized to avoid synchronized retries (thundering herd).

### Example Delays (WithConnectionRetries / WithStandardRetries preset)

| Attempt | Base Delay |
|---------|------------|
| 1       | ~200ms     |
| 2       | ~400ms     |
| 3       | ~800ms     |

## Best Practices

### 1. Prefer a preset

```csharp
// General purpose, recommended
var standard = HttpResilienceOptionsPresets.WithStandardRetries();

// Long-running Soroban polling
var soroban = HttpResilienceOptionsPresets.ForSorobanPolling();

// Latency-sensitive (trading bots)
var lowLatency = HttpResilienceOptionsPresets.LowLatency();
```

### 2. Be careful retrying unsafe methods

Leave `RetryUnsafeHttpMethods = false` unless the endpoint is idempotent. Retrying a `POST` (such as a transaction submission) can produce duplicate side effects.

### 3. Keep jitter enabled

Always keep jitter enabled to prevent synchronized retries from multiple clients (thundering herd problem).

## Behavior changes when upgrading

If you are upgrading from a version that retried connection failures only:

- **`ForSorobanPolling()`** now also retries the transient HTTP status codes (408/429/500/502/503/504) for safe methods and honors `Retry-After`, in addition to connection failures. If you want the previous connection-only behavior, use `WithConnectionRetries()` (or build a custom `HttpResilienceOptions` with an empty `RetryHttpStatusCodes`).
- Default behavior is unchanged: with no `HttpResilienceOptions`, nothing is retried.

## Complete Example

```csharp
using System;
using System.Threading.Tasks;
using StellarDotnetSdk;
using StellarDotnetSdk.Exceptions;
using StellarDotnetSdk.Requests;

// Industry-standard retries for production use
var resilienceOptions = HttpResilienceOptionsPresets.WithStandardRetries();

var httpClient = new DefaultStellarSdkHttpClient(
    bearerToken: "your-api-token",  // Optional
    resilienceOptions: resilienceOptions
);

var horizonServer = new Server("https://horizon.stellar.org", httpClient);

try
{
    var account = await horizonServer.Accounts.Account("GABC...");
    Console.WriteLine($"Account balance: {account.Balances[0].BalanceString}");
}
catch (TooManyRequestsException ex)
{
    // Surfaced only after retries are exhausted (or when 429 is not in RetryHttpStatusCodes)
    var delay = ex.RetryAfterDelay ?? TimeSpan.FromSeconds(5);
    Console.WriteLine($"Rate limited. Suggested wait: {delay}.");
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Connection failed after all retries: {ex.Message}");
}
```

## Troubleshooting

### HTTP status codes are not being retried

Check that **both** conditions hold: `MaxRetryCount > 0` and the status code is in `RetryHttpStatusCodes`. Also confirm the request uses a safe method, or that `RetryUnsafeHttpMethods = true`.

### Retries take longer than expected

A retried response with a large `Retry-After` value will delay the next attempt (capped by `MaxDelay`). Lower `MaxDelay` or set `RespectRetryAfter = false` if you prefer pure exponential backoff.

### Rate limiting

1. Use a preset that retries 429 (`WithStandardRetries()` / `ForSorobanPolling()`), or add `HttpStatusCode.TooManyRequests` to `RetryHttpStatusCodes`.
2. Inspect `TooManyRequestsException.RetryAfterDelay` for manual handling.
3. Reduce request frequency.

## Additional Resources

- [Java SDK](https://github.com/lightsail-network/java-stellar-sdk)
- [Polly resilience library](https://www.pollydocs.org/)
- [Stellar Network Status](https://status.stellar.org/)
- [Horizon API Documentation](https://developers.stellar.org/docs/data/apis/horizon)
- [Stellar RPC Documentation](https://developers.stellar.org/docs/data/apis/rpc)
