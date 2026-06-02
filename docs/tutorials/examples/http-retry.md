# HTTP Resilience Configuration

This guide explains how to configure optional HTTP retry, circuit breaker, and timeout behavior in the SDK.

> **Note:** Retries are **disabled by default** (matching the Java SDK's approach). Nothing is retried until you opt in by passing an `HttpResilienceOptions` instance (or a preset). When enabled, the SDK retries connection-level failures (network errors, DNS failures) and — if you configure them — the transient HTTP status codes you list in `RetryHttpStatusCodes`.

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
| Retry HTTP methods | Safe only (`GET`, `HEAD`, `OPTIONS`) |
| Respect `Retry-After` | Enabled (`true`)  |
| Circuit Breaker   | Disabled              |
| Request Timeout   | None                  |

### What Gets Retried

When retries are enabled (`MaxRetryCount > 0`):

**Connection failures (exceptions) are always retried:**
- `HttpRequestException` — network errors, DNS failures
- `TimeoutException` — request timeouts
- `TaskCanceledException` — HTTP client timeouts (not user-initiated cancellation)

**HTTP status codes are retried only when you opt in** by adding them to `RetryHttpStatusCodes`. By default the set is empty, so no status code is retried. Status-code retries are additionally gated by HTTP method (see [Controlling which HTTP methods retry](#controlling-which-http-methods-retry)): by default only safe methods (`GET`, `HEAD`, `OPTIONS`) are retried.

## Quick Start with Presets

The SDK provides preset configurations for common use cases:

```csharp
using StellarDotnetSdk.Requests;

// No retries (same as new HttpResilienceOptions())
var defaultOptions = HttpResilienceOptionsPresets.Default();

// Horizon clients: retries 408/429/5xx on every call including SubmitTransaction(), honors Retry-After
var horizon = HttpResilienceOptionsPresets.ForHorizon();

// Stellar RPC (Soroban): all RPC calls are POST; status-code retries + higher budget and longer delays
var sorobanOptions = HttpResilienceOptionsPresets.ForSoroban();

// Connection failures only (no status-code retries; RetryHttpMethods stays at GET/HEAD/OPTIONS)
var connectionOnly = HttpResilienceOptionsPresets.WithConnectionRetries();

// Low latency: fast failure for trading bots (connection failures only)
var tradingOptions = HttpResilienceOptionsPresets.LowLatency();
```

### Preset Details

| Preset                     | Max Retries | Base Delay | Max Delay | Status-code retries            | `RetryHttpMethods`           | `Retry-After` |
|----------------------------|-------------|------------|-----------|--------------------------------|------------------------------|---------------|
| `Default()` / `NoRetry()`  | 0           | —          | —         | none                           | safe (GET/HEAD/OPTIONS)      | —             |
| `WithConnectionRetries()`  | 3           | 200ms      | 5s        | none (connection failures only)| safe (GET/HEAD/OPTIONS)      | n/a           |
| `ForHorizon()`             | 3           | 200ms      | 5s        | 408, 429, 500, 502, 503, 504   | safe **+ POST**              | honored       |
| `ForSoroban()`             | 5           | 500ms      | 15s       | 408, 429, 500, 502, 503, 504   | safe **+ POST**              | honored       |
| `LowLatency()`             | 1           | 50ms       | 200ms     | none (connection failures only)| safe (GET/HEAD/OPTIONS)      | n/a           |

All retrying presets use exponential backoff with jitter enabled.

`ForHorizon()` and `ForSoroban()` add `POST` to the retry-method whitelist because the Stellar wire layer is idempotent for both: Horizon queries are pure reads; `SubmitTransaction()` is keyed by transaction hash + source-account sequence number, so a resubmit either returns the cached server result or fails with `tx_bad_seq` — there is no double-spend window. The same applies to every Soroban RPC method (all POST). `PATCH`/`PUT`/`DELETE` are *not* in either preset, so future SDK methods that use them are not silently retried.

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

var resilienceOptions = HttpResilienceOptionsPresets.ForHorizon();

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

### Controlling which HTTP methods retry

Status-code retries are gated by `RetryHttpMethods` — an explicit `ISet<HttpMethod>` that defaults to the RFC-safe methods (`GET`, `HEAD`, `OPTIONS`). Other methods are never retried on a status code unless you add them to the set.

`ForHorizon()` and `ForSoroban()` add `POST` to the set because the Stellar wire layer is idempotent for both Horizon's `SubmitTransaction()` (tx hash + source sequence → no double-spend; resubmit yields the cached result or `tx_bad_seq`) and every Soroban JSON-RPC method.

If you're building options manually and need POST retried on Horizon or Soroban (e.g. you're using `WithConnectionRetries()` as a base and want to add status-code retries for `SubmitTransaction()`), add the method explicitly:

```csharp
var resilienceOptions = new HttpResilienceOptions
{
    MaxRetryCount = 3,
};
resilienceOptions.RetryHttpStatusCodes.Add(HttpStatusCode.ServiceUnavailable);
resilienceOptions.RetryHttpMethods.Add(HttpMethod.Post);
```

> **⚠️ SEP services are not blanket-safe to retry.** Specific SEP POST endpoints are non-idempotent
> by spec:
>
> - **SEP-10 `POST /auth`** — per spec: *"The Server should not provide more than one JWT for a
>   specific challenge transaction."* On transient failure, fetch a **fresh** challenge with
>   `GET /auth` and submit that — do not retry the same body.
> - **SEP-24 `POST /transactions/{deposit,withdraw}/interactive`** — each call mints a fresh
>   `transaction_id`; retrying creates duplicate transaction records. No idempotency-key mechanism
>   is defined.
> - **SEP-6 `PATCH /transactions/{id}`** — not in the SEP-6 master spec; anchor-vendor extension
>   that mutates KYC state.
>
> Do NOT wire `ForHorizon()` or `ForSoroban()` into a `ClientWebAuth` / `InteractiveService` /
> `TransferServerService` HttpClient. For SEP HttpClients, use `WithConnectionRetries()`
> (transport failures only) or build a custom `HttpResilienceOptions` whose `RetryHttpMethods`
> contains only the safe defaults.

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

### Example Delays (WithConnectionRetries / ForHorizon preset)

| Attempt | Base Delay |
|---------|------------|
| 1       | ~200ms     |
| 2       | ~400ms     |
| 3       | ~800ms     |

## Best Practices

### 1. Prefer a preset

```csharp
// Horizon clients (queries + SubmitTransaction)
var horizon = HttpResilienceOptionsPresets.ForHorizon();

// Stellar RPC (Soroban) — long-running polling
var soroban = HttpResilienceOptionsPresets.ForSoroban();

// Latency-sensitive (trading bots)
var lowLatency = HttpResilienceOptionsPresets.LowLatency();
```

### 2. Handle `tx_bad_seq` after a retried submission

If a transaction was already accepted on the original attempt and the retry surfaces `tx_bad_seq`, look up the transaction by its hash to recover the original server result instead of assuming the submission failed.

### 3. Keep jitter enabled

Always keep jitter enabled to prevent synchronized retries from multiple clients (thundering herd problem).

## Complete Example

```csharp
using System;
using System.Threading.Tasks;
using StellarDotnetSdk;
using StellarDotnetSdk.Exceptions;
using StellarDotnetSdk.Requests;

// Production-ready resilience for Horizon clients
var resilienceOptions = HttpResilienceOptionsPresets.ForHorizon();

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

Check that **all three** conditions hold: `MaxRetryCount > 0`, the status code is in `RetryHttpStatusCodes`, and the request's HTTP method is in `RetryHttpMethods` (which defaults to `GET`/`HEAD`/`OPTIONS`).

### Retries take longer than expected

A retried response with a large `Retry-After` value will delay the next attempt (capped by `MaxDelay`). Lower `MaxDelay` or set `RespectRetryAfter = false` if you prefer pure exponential backoff.

### Rate limiting

1. Use a preset that retries 429 (`ForHorizon()` / `ForSoroban()`), or add `HttpStatusCode.TooManyRequests` to `RetryHttpStatusCodes`.
2. Inspect `TooManyRequestsException.RetryAfterDelay` for manual handling.
3. Reduce request frequency.

## Additional Resources

- [Java SDK](https://github.com/lightsail-network/java-stellar-sdk)
- [Polly resilience library](https://www.pollydocs.org/)
- [Stellar Network Status](https://status.stellar.org/)
- [Horizon API Documentation](https://developers.stellar.org/docs/data/apis/horizon)
- [Stellar RPC Documentation](https://developers.stellar.org/docs/data/apis/rpc)
