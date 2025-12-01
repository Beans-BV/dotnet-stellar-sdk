# HTTP Resilience Configuration

This guide explains how to configure the SDK's built-in HTTP resilience mechanism for handling transient network failures.

> **v12.0.0 Breaking Change:** Retries are now enabled by default (3 attempts with exponential backoff). To restore pre-v12 behavior, use `HttpResilienceOptionsPresets.NoRetry()`. See [Disabling Retries](#disabling-retries) for details.

## Overview

The SDK includes automatic retry logic for transient HTTP failures. By default, both `Server` (Horizon) and `SorobanServer` use `DefaultStellarSdkHttpClient`, which includes resilience capabilities out of the box.

### Default Behavior

When using the SDK without any configuration, the following behavior is applied:

| Setting           | Default Value  |
|-------------------|----------------|
| Max Retry Count   | 3              |
| Base Delay        | 200ms          |
| Max Delay         | 5000ms         |
| Jitter            | Enabled (±20%) |
| Honor Retry-After | Yes            |
| Circuit Breaker   | Disabled       |
| Request Timeout   | None           |

### Retriable Status Codes

The following HTTP status codes trigger automatic retries:

- `408` - Request Timeout
- `425` - Too Early
- `429` - Too Many Requests
- `500` - Internal Server Error
- `502` - Bad Gateway
- `503` - Service Unavailable
- `504` - Gateway Timeout

### Retriable Exceptions

The following exceptions trigger automatic retries:

- `HttpRequestException` - Network errors
- `TimeoutException` - Request timeouts
- `TaskCanceledException` - HTTP client timeouts (not user-initiated cancellation)

## Quick Start with Presets

The SDK provides preset configurations for common use cases:

```csharp
using StellarDotnetSdk.Requests;

// Default: 3 retries, 200ms base delay (same as new HttpResilienceOptions())
var defaultOptions = HttpResilienceOptionsPresets.Default();

// No retries: restore pre-v12 behavior
var noRetryOptions = HttpResilienceOptionsPresets.NoRetry();

// Soroban polling: more retries, longer delays for tx finalization
var sorobanOptions = HttpResilienceOptionsPresets.ForSorobanPolling();

// Low latency: fast failure for trading bots
var tradingOptions = HttpResilienceOptionsPresets.LowLatency();
```

### Preset Details

| Preset                | MaxRetryCount | BaseDelay | MaxDelay | HonorRetryAfter |
|-----------------------|---------------|-----------|----------|-----------------|
| `Default()`           | 3             | 200ms     | 5s       | Yes             |
| `NoRetry()`           | 0             | -         | -        | -               |
| `ForSorobanPolling()` | 5             | 500ms     | 15s      | Yes             |
| `LowLatency()`        | 1             | 50ms      | 200ms    | No              |

## Using Default Retry Configuration

The simplest way to use the SDK is with default retry settings:

```csharp
// Horizon server with default retry (3 retries, 200ms base delay)
var server = new Server("https://horizon-testnet.stellar.org");

// Soroban server with default retry
var sorobanServer = new SorobanServer("https://soroban-testnet.stellar.org");
```

No additional configuration is needed - retries are enabled by default.

## Customizing Retry Behavior

### Using HttpResilienceOptions

You can customize retry behavior by creating an `HttpResilienceOptions` instance:

```csharp
using StellarDotnetSdk.Requests;

// Create custom resilience options
var resilienceOptions = new HttpResilienceOptions
{
    MaxRetryCount = 5,           // Retry up to 5 times
    BaseDelay = TimeSpan.FromMilliseconds(500),           // Start with 500ms delay
    MaxDelay = TimeSpan.FromMilliseconds(10000),          // Cap delay at 10 seconds
    UseJitter = true,            // Add randomness to prevent thundering herd
    HonorRetryAfterHeader = true // Respect server's Retry-After header
};

// Create HTTP client with custom resilience options
var httpClient = new DefaultStellarSdkHttpClient(resilienceOptions: resilienceOptions);

// Use with Horizon server
var server = new Server("https://horizon-testnet.stellar.org", httpClient);

// Or with Soroban server
var sorobanServer = new SorobanServer("https://soroban-testnet.stellar.org", httpClient);
```

### Disabling Retries

To disable retries entirely (restore pre-v12 behavior), use the `NoRetry()` preset:

```csharp
var httpClient = new DefaultStellarSdkHttpClient(
    resilienceOptions: HttpResilienceOptionsPresets.NoRetry());
var server = new Server("https://horizon-testnet.stellar.org", httpClient);
```

Or set `MaxRetryCount` to 0 manually:

```csharp
var noRetryOptions = new HttpResilienceOptions
{
    MaxRetryCount = 0  // Disable retries
};

var httpClient = new DefaultStellarSdkHttpClient(resilienceOptions: noRetryOptions);
var server = new Server("https://horizon-testnet.stellar.org", httpClient);
```

### Adding Custom Retriable Status Codes

You can add additional status codes that should trigger retries:

```csharp
var resilienceOptions = new HttpResilienceOptions();
resilienceOptions.AdditionalRetriableStatusCodes.Add(HttpStatusCode.Conflict);  // 409
resilienceOptions.AdditionalRetriableStatusCodes.Add(HttpStatusCode.Gone);      // 410

var httpClient = new DefaultStellarSdkHttpClient(resilienceOptions: resilienceOptions);
```

### Adding Custom Retriable Exception Types

You can add additional exception types that should trigger retries:

```csharp
var resilienceOptions = new HttpResilienceOptions();
resilienceOptions.AdditionalRetriableExceptionTypes.Add(typeof(SocketException));

var httpClient = new DefaultStellarSdkHttpClient(resilienceOptions: resilienceOptions);
```

## Circuit Breaker (Advanced)

The circuit breaker pattern prevents cascading failures by temporarily blocking requests to an unhealthy service. It's **disabled by default** and should only be enabled if you understand the implications.

> **Warning:** When the circuit is open, requests throw `BrokenCircuitException` instead of the underlying HTTP error. This changes your error handling requirements.

### Enabling Circuit Breaker

```csharp
var resilienceOptions = new HttpResilienceOptions
{
    EnableCircuitBreaker = true,
    FailureRatio = 0.5,           // Open circuit when 50% of requests fail
    MinimumThroughput = 10,       // Require at least 10 requests before evaluating
    SamplingDuration = TimeSpan.FromSeconds(30),  // Evaluate over 30-second windows
    BreakDuration = TimeSpan.FromSeconds(30)      // Stay open for 30 seconds
};

var httpClient = new DefaultStellarSdkHttpClient(resilienceOptions: resilienceOptions);
```

### Circuit Breaker Settings

| Setting                | Default | Description                                                   |
|------------------------|---------|---------------------------------------------------------------|
| `EnableCircuitBreaker` | `false` | Whether to enable circuit breaker                             |
| `FailureRatio`         | `0.5`   | Percentage of failures (0.0-1.0) that triggers circuit open   |
| `MinimumThroughput`    | `10`    | Minimum requests in sampling window before circuit can open   |
| `SamplingDuration`     | `30s`   | Time window for evaluating failure ratio                      |
| `BreakDuration`        | `30s`   | How long the circuit stays open before allowing test requests |

### Circuit Breaker States

1. **Closed** - Normal operation; requests flow through
2. **Open** - Circuit tripped; requests fail immediately with `BrokenCircuitException`
3. **Half-Open** - After break duration, allows one test request to check if service recovered

### When to Use Circuit Breaker

- ✅ High-volume applications where you need to protect against cascading failures
- ✅ Services with known availability patterns
- ✅ Applications that can gracefully degrade when circuit is open

- ❌ Low-volume applications (may not reach minimum throughput)
- ❌ Applications that can't handle `BrokenCircuitException`
- ❌ When you need every request attempt to reach the server

## Request Timeout (Advanced)

You can set a per-request timeout that applies to each attempt:

```csharp
var resilienceOptions = new HttpResilienceOptions
{
    RequestTimeout = TimeSpan.FromSeconds(10)  // Each request times out after 10s
};

var httpClient = new DefaultStellarSdkHttpClient(resilienceOptions: resilienceOptions);
```

> **Note:** This timeout applies to each individual request attempt. With retries, the total operation time can be `RequestTimeout × (MaxRetryCount + 1)` plus backoff delays.

When a request times out, it throws `TimeoutRejectedException` (from Polly). If retries are enabled, the request will be retried.

## Using a Custom Inner Handler

By default, `DefaultStellarSdkHttpClient` uses `SocketsHttpHandler` as the underlying HTTP handler. You can provide a custom `HttpMessageHandler` for scenarios such as:

- **Unit testing** - Inject a mock handler to simulate HTTP responses without network calls
- **Proxy configuration** - Use a handler configured with specific proxy settings
- **Custom certificate handling** - Implement custom SSL/TLS certificate validation
- **Platform compatibility** - Use a different handler for platforms where `SocketsHttpHandler` isn't optimal (e.g., Blazor WebAssembly)

```csharp
// Example: Using a custom handler for testing
var mockHandler = new MockHttpMessageHandler();
mockHandler.When("*").Respond(HttpStatusCode.OK);

var httpClient = new DefaultStellarSdkHttpClient(
    resilienceOptions: resilienceOptions,
    innerHandler: mockHandler
);

// Example: Using a handler with custom proxy settings
var proxyHandler = new HttpClientHandler
{
    Proxy = new WebProxy("http://proxy.example.com:8080"),
    UseProxy = true
};

var httpClientWithProxy = new DefaultStellarSdkHttpClient(
    innerHandler: proxyHandler
);
```

The custom handler is wrapped by `RetryingHttpMessageHandler`, so retry logic still applies regardless of which inner handler you provide.

## Exponential Backoff

The retry mechanism uses exponential backoff to calculate delays between retries:

```
delay = min(BaseDelay * 2^attempt, MaxDelay)
```

With jitter enabled (default), the actual delay varies by ±20%:

```
actual_delay = delay * random(0.8, 1.2)
```

### Example Delays (default settings)

| Attempt | Base Delay | With Jitter (range) |
|---------|------------|---------------------|
| 1       | 200ms      | 160-240ms           |
| 2       | 400ms      | 320-480ms           |
| 3       | 800ms      | 640-960ms           |

## Retry-After Header

When `HonorRetryAfterHeader` is enabled (default), the SDK respects the `Retry-After` header sent by servers. This header can specify:

1. **Delay in seconds**: `Retry-After: 60`
2. **HTTP date (RFC 7231 HTTP-date, RFC 1123-style)**: `Retry-After: Wed, 21 Oct 2024 07:28:00 GMT`

The delay from `Retry-After` is capped at `MaxDelay` to prevent excessive waits.

## Best Practices

### 1. Use Appropriate Retry Counts

For user-facing applications, keep retry counts low (2-3) to avoid long waits. For background jobs, higher counts (5-10) may be appropriate.

```csharp
// User-facing: quick feedback
var userOptions = new HttpResilienceOptions { MaxRetryCount = 2, BaseDelay = TimeSpan.FromMilliseconds(100) };

// Background job: more resilient
var jobOptions = new HttpResilienceOptions { MaxRetryCount = 10, BaseDelay = TimeSpan.FromMilliseconds(500) };
```

### 2. Use Presets When Possible

The built-in presets are tuned for common scenarios:

```csharp
// For Soroban transaction polling
var sorobanClient = new DefaultStellarSdkHttpClient(
    resilienceOptions: HttpResilienceOptionsPresets.ForSorobanPolling());

// For trading applications
var tradingClient = new DefaultStellarSdkHttpClient(
    resilienceOptions: HttpResilienceOptionsPresets.LowLatency());
```

### 3. Enable Jitter

Always keep jitter enabled to prevent synchronized retries from multiple clients overwhelming the server (thundering herd problem).

### 4. Honor Retry-After

Keep `HonorRetryAfterHeader` enabled to respect rate limits and avoid being blocked by the server.

### 5. Set Reasonable Max Delays

For interactive applications, cap delays at a few seconds. For batch processing, longer delays may be acceptable.

### 6. Use CancellationTokens

Always pass cancellation tokens to allow users to cancel long-running operations:

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

try
{
    var account = await server.Accounts.Account(accountId, cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Operation was cancelled");
}
```

### 7. Leave Circuit Breaker Disabled Unless Needed

Circuit breaker is powerful but changes error semantics. Only enable it if you:
- Have high request volume
- Can handle `BrokenCircuitException`
- Want fail-fast behavior during outages

## Complete Example

Here's a complete example showing custom resilience configuration:

```csharp
using StellarDotnetSdk;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Soroban;

// Configure resilience options for production use
var resilienceOptions = new HttpResilienceOptions
{
    MaxRetryCount = 5,
    BaseDelay = TimeSpan.FromMilliseconds(300),
    MaxDelay = TimeSpan.FromMilliseconds(8000),
    UseJitter = true,
    HonorRetryAfterHeader = true
};

// Create HTTP client with bearer token and resilience options
var httpClient = new DefaultStellarSdkHttpClient(
    bearerToken: "your-api-token",  // Optional
    resilienceOptions: resilienceOptions,
    innerHandler: null  // Optional: provide custom handler for testing or proxy configuration
);

// Use with Horizon
var horizonServer = new Server("https://horizon.stellar.org", httpClient);

// Use with Soroban (create a separate client if different settings needed)
var sorobanClient = new DefaultStellarSdkHttpClient(
    resilienceOptions: HttpResilienceOptionsPresets.ForSorobanPolling());
var sorobanServer = new SorobanServer("https://soroban.stellar.org", sorobanClient);

// Make requests - retries are handled automatically
try
{
    var account = await horizonServer.Accounts.Account("GABC...");
    Console.WriteLine($"Account balance: {account.Balances[0].BalanceString}");
}
catch (HttpRequestException ex)
{
    // All retries exhausted
    Console.WriteLine($"Request failed after all retries: {ex.Message}");
}
```

## Troubleshooting

### Requests Still Failing After Retries

If requests fail after all retry attempts:

1. Check network connectivity
2. Verify the server URL is correct
3. Check if the server is experiencing an outage
4. Review server status pages

### Rate Limiting Issues

If you're being rate limited frequently:

1. Reduce request frequency
2. Implement request batching where possible
3. Consider using a dedicated API key with higher limits
4. Increase `MaxDelay` to allow longer waits

### Timeouts

If operations are timing out:

1. Increase `HttpClient.Timeout` or `RequestTimeout`
2. Check network latency
3. Consider using a closer server endpoint

### BrokenCircuitException

If you're getting `BrokenCircuitException`:

1. The circuit breaker has tripped due to too many failures
2. Wait for the `BreakDuration` to pass
3. Check if the underlying service is healthy
4. Consider disabling circuit breaker if this behavior is unexpected

## Additional Resources

- [Stellar Network Status](https://status.stellar.org/)
- [Horizon API Documentation](https://developers.stellar.org/api/horizon)
- [Soroban RPC Documentation](https://developers.stellar.org/docs/data/rpc)
- [Polly Documentation](https://www.thepollyproject.org/) (used internally for resilience)
