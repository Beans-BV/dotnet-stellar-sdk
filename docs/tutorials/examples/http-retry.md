# HTTP Resilience Configuration

This guide explains how to configure optional HTTP retry logic for connection failures in the SDK.

> **Note:** By default, retries are **disabled** (matching the Java SDK's approach). Only connection-level failures (network errors, DNS failures) are retried when enabled. HTTP error status codes (4xx/5xx) are **never** retried automatically.

## Overview

The SDK provides an optional retry mechanism for connection failures, similar to OkHttp's `retryOnConnectionFailure(true)`. This helps handle transient network issues like DNS failures, connection timeouts, and socket errors.

### Default Behavior

When using the SDK without any configuration, **no retries are performed**:

| Setting           | Default Value |
|-------------------|---------------|
| Max Retry Count   | 0 (disabled)  |
| Base Delay        | 200ms         |
| Max Delay         | 5000ms        |
| Jitter            | Enabled       |
| Circuit Breaker   | Disabled      |
| Request Timeout   | None          |

### What Gets Retried

**Connection failures (exceptions) are retried** when retries are enabled:
- `HttpRequestException` - Network errors, DNS failures
- `TimeoutException` - Request timeouts
- `TaskCanceledException` - HTTP client timeouts (not user-initiated cancellation)

**HTTP status codes are never retried**:
- `408`, `429`, `500`, `502`, `503`, `504` - These return immediately without retry
- All 4xx and 5xx responses - Handled by your application code

This matches the Java SDK's behavior: only connection-level failures trigger retries.

## Quick Start with Presets

The SDK provides preset configurations for common use cases:

```csharp
using StellarDotnetSdk.Requests;

// Default: No retries (same as new HttpResilienceOptions())
var defaultOptions = HttpResilienceOptionsPresets.Default();

// Enable connection retries (similar to OkHttp's retryOnConnectionFailure)
var withRetries = HttpResilienceOptionsPresets.WithConnectionRetries();

// Soroban polling: more retries for network instability
var sorobanOptions = HttpResilienceOptionsPresets.ForSorobanPolling();

// Low latency: fast failure for trading bots
var tradingOptions = HttpResilienceOptionsPresets.LowLatency();
```

### Preset Details

| Preset                      | MaxRetryCount | BaseDelay | MaxDelay |
|----------------------------|---------------|-----------|----------|
| `Default()`                | 0 (disabled)  | -         | -        |
| `WithConnectionRetries()`  | 3             | 200ms     | 5s       |
| `ForSorobanPolling()`      | 5             | 500ms     | 15s      |
| `LowLatency()`             | 1             | 50ms      | 200ms    |

## Using the SDK Without Retries (Default)

By default, the SDK does not retry any requests:

```csharp
// No retries - requests fail immediately on connection errors
var server = new Server("https://horizon-testnet.stellar.org");
var sorobanServer = new SorobanServer("https://soroban-testnet.stellar.org");
```

This matches the Java SDK's default behavior. HTTP error responses (4xx/5xx) are returned immediately, and connection failures throw exceptions immediately.

## Enabling Connection Retries

To enable retries for connection failures, pass `HttpResilienceOptions` with `MaxRetryCount > 0`:

```csharp
using StellarDotnetSdk.Requests;

// Enable connection retries (3 attempts, 200ms base delay)
var resilienceOptions = HttpResilienceOptionsPresets.WithConnectionRetries();

var httpClient = new DefaultStellarSdkHttpClient(resilienceOptions: resilienceOptions);
var server = new Server("https://horizon-testnet.stellar.org", httpClient);
```

### Custom Retry Configuration

You can customize retry behavior:

```csharp
var resilienceOptions = new HttpResilienceOptions
{
    MaxRetryCount = 5,           // Retry up to 5 times
    BaseDelay = TimeSpan.FromMilliseconds(500),  // Start with 500ms delay
    MaxDelay = TimeSpan.FromMilliseconds(10000), // Cap delay at 10 seconds
    UseJitter = true             // Add randomness to prevent thundering herd
};

var httpClient = new DefaultStellarSdkHttpClient(resilienceOptions: resilienceOptions);
var server = new Server("https://horizon-testnet.stellar.org", httpClient);
```

### Adding Custom Retriable Exception Types

You can add additional exception types that should trigger retries:

```csharp
var resilienceOptions = new HttpResilienceOptions
{
    MaxRetryCount = 3
};
resilienceOptions.AdditionalRetriableExceptionTypes.Add(typeof(SocketException));

var httpClient = new DefaultStellarSdkHttpClient(resilienceOptions: resilienceOptions);
```

## Important: HTTP Status Codes Are Not Retried

Unlike some HTTP client libraries, this SDK **does not retry HTTP error status codes**. This matches the Java SDK's behavior.

```csharp
// Example: 429 Too Many Requests
var server = new Server("https://horizon-testnet.stellar.org");

try
{
    var account = await server.Accounts.Account(accountId);
}
catch (TooManyRequestsException ex)
{
    // This exception is thrown immediately - no retries
    // You must handle rate limiting in your application code
    if (ex.RetryAfter.HasValue)
    {
        await Task.Delay(TimeSpan.FromSeconds(ex.RetryAfter.Value));
        // Retry manually if needed
    }
}
```

If you need automatic retries for HTTP status codes, you can:
1. Implement custom retry logic in your application
2. Use a library like Polly to wrap SDK calls
3. Handle specific status codes in your error handling

## Circuit Breaker (Advanced)

The circuit breaker pattern prevents cascading failures by temporarily blocking requests to an unhealthy service. It's **disabled by default**.

> **Warning:** When the circuit is open, requests throw `BrokenCircuitException` instead of the underlying HTTP error.

### Enabling Circuit Breaker

```csharp
var resilienceOptions = new HttpResilienceOptions
{
    MaxRetryCount = 3,
    EnableCircuitBreaker = true,
    FailureRatio = 0.5,           // Open circuit when 50% of requests fail
    MinimumThroughput = 10,        // Require at least 10 requests before evaluating
    SamplingDuration = TimeSpan.FromSeconds(30),
    BreakDuration = TimeSpan.FromSeconds(30)
};

var httpClient = new DefaultStellarSdkHttpClient(resilienceOptions: resilienceOptions);
```

## Request Timeout (Advanced)

You can set a per-request timeout:

```csharp
var resilienceOptions = new HttpResilienceOptions
{
    MaxRetryCount = 3,
    RequestTimeout = TimeSpan.FromSeconds(10)  // Each request times out after 10s
};

var httpClient = new DefaultStellarSdkHttpClient(resilienceOptions: resilienceOptions);
```

## Exponential Backoff

When retries are enabled, the SDK uses exponential backoff:

```
delay = min(BaseDelay * 2^attempt, MaxDelay)
```

With jitter enabled (default), the actual delay varies by ±20%:

```
actual_delay = delay * random(0.8, 1.2)
```

### Example Delays (WithConnectionRetries preset)

| Attempt | Base Delay | With Jitter (range) |
|---------|------------|---------------------|
| 1       | 200ms      | 160-240ms           |
| 2       | 400ms      | 320-480ms           |
| 3       | 800ms      | 640-960ms           |

## Best Practices

### 1. Use Retries for Connection Failures Only

Retries are most useful for transient network issues. Don't enable retries if you need immediate feedback on HTTP errors.

```csharp
// Good: Enable retries for background jobs
var backgroundOptions = HttpResilienceOptionsPresets.WithConnectionRetries();

// Good: Disable retries for user-facing apps that need fast feedback
var userOptions = HttpResilienceOptionsPresets.Default(); // No retries
```

### 2. Handle HTTP Errors in Your Code

Since HTTP status codes aren't retried, handle them explicitly:

```csharp
try
{
    var account = await server.Accounts.Account(accountId);
}
catch (TooManyRequestsException ex)
{
    // Handle rate limiting
    if (ex.RetryAfter.HasValue)
    {
        await Task.Delay(TimeSpan.FromSeconds(ex.RetryAfter.Value));
        // Retry manually
    }
}
catch (HttpResponseException ex) when (ex.StatusCode == 503)
{
    // Handle service unavailable - retry manually if needed
}
```

### 3. Use Presets When Possible

The built-in presets are tuned for common scenarios:

```csharp
// For Soroban transaction polling
var sorobanClient = new DefaultStellarSdkHttpClient(
    resilienceOptions: HttpResilienceOptionsPresets.ForSorobanPolling());

// For trading applications
var tradingClient = new DefaultStellarSdkHttpClient(
    resilienceOptions: HttpResilienceOptionsPresets.LowLatency());
```

### 4. Keep Jitter Enabled

Always keep jitter enabled to prevent synchronized retries from multiple clients (thundering herd problem).

## Complete Example

```csharp
using StellarDotnetSdk;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Soroban;

// Enable connection retries for production use
var resilienceOptions = HttpResilienceOptionsPresets.WithConnectionRetries();

// Create HTTP client with bearer token and resilience options
var httpClient = new DefaultStellarSdkHttpClient(
    bearerToken: "your-api-token",  // Optional
    resilienceOptions: resilienceOptions
);

// Use with Horizon
var horizonServer = new Server("https://horizon.stellar.org", httpClient);

// Use with Soroban
var sorobanServer = new SorobanServer("https://soroban.stellar.org", httpClient);

// Make requests - connection failures are retried automatically
try
{
    var account = await horizonServer.Accounts.Account("GABC...");
    Console.WriteLine($"Account balance: {account.Balances[0].BalanceString}");
}
catch (HttpRequestException ex)
{
    // All retries exhausted for connection failures
    Console.WriteLine($"Connection failed after all retries: {ex.Message}");
}
catch (TooManyRequestsException ex)
{
    // HTTP 429 - not retried automatically
    Console.WriteLine($"Rate limited. Retry after: {ex.RetryAfter} seconds");
}
```

## Troubleshooting

### Connection Failures Still Happening

If connection failures persist after retries:
1. Check network connectivity
2. Verify DNS resolution
3. Check firewall/proxy settings
4. Increase `MaxRetryCount` if needed

### HTTP Errors Not Being Retried

This is expected behavior. HTTP status codes (4xx/5xx) are never retried automatically. Handle them in your application code.

### Rate Limiting

If you're being rate limited:
1. Handle `TooManyRequestsException` in your code
2. Check the `RetryAfter` property
3. Implement exponential backoff in your application
4. Reduce request frequency

## Additional Resources

- [Java SDK](https://github.com/lightsail-network/java-stellar-sdk)
- [Stellar Network Status](https://status.stellar.org/)
- [Horizon API Documentation](https://developers.stellar.org/api/horizon)
- [Soroban RPC Documentation](https://developers.stellar.org/docs/data/rpc)
