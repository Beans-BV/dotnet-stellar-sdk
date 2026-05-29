<p align="center">
    <img height="200" src="https://raw.githubusercontent.com/Beans-BV/dotnet-stellar-sdk/main/docfx/images/logo.svg">
    <h3 align="center">dotnet-stellar-sdk</h3>
    <p align="center">
        Stellar API SDK for .NET
        <br /> 
        <a href="https://github.com/Beans-BV/dotnet-stellar-sdk/actions/workflows/pack_and_test.yml">
            <img src="https://github.com/Beans-BV/dotnet-stellar-sdk/actions/workflows/pack_and_test.yml/badge.svg?branch=main">
        </a>
        <a href="https://www.nuget.org/packages/stellar-dotnet-sdk">
            <img src="https://img.shields.io/nuget/v/stellar-dotnet-sdk.svg" />
        </a>
        <a href="https://www.nuget.org/packages/stellar-dotnet-sdk">
            <img src="https://img.shields.io/nuget/dt/stellar-dotnet-sdk.svg" />
        </a>
        <br />
        <a href="https://github.com/Beans-BV/dotnet-stellar-sdk/issues/new?template=Bug_report.md">Report Bug</a> · 
        <a href="https://github.com/Beans-BV/dotnet-stellar-sdk/issues/new?template=Feature_request.md">Request Feature</a> · 
        <a href="https://github.com/Beans-BV/dotnet-stellar-sdk/security/policy">Report Security Vulnerability</a> 
    </p>
</p>

## About The Project

`dotnet-stellar-sdk` is a .NET library for communicating with
a [Stellar Horizon server](https://github.com/stellar/go/tree/master/services/horizon)
or [Stellar RPC server](https://developers.stellar.org/docs/data/apis/rpc).

It is used for building Stellar apps.

_This project originated as a full port of the
official [Java SDK API](https://github.com/lightsail-network/java-stellar-sdk)._

_The SEP (Stellar Ecosystem Proposal) protocol implementations were ported from the
[Flutter SDK](https://github.com/Soneso/stellar_flutter_sdk)._

## Features

- **Industry-standard HTTP resilience** - Opt-in retries for 408/429/5xx (matches `Microsoft.Extensions.Http.Resilience.AddStandardResilienceHandler`), with `Retry-After` honored and unsafe HTTP methods protected by default
- **Configurable** - Customize retry count, delays, jitter, status codes, and method filter
- **Full Stellar Support** - Works with both Horizon API and Stellar RPC servers

## Installation

The `stellar-dotnet-sdk` library is bundled in a NuGet package.

- [NuGet Package](https://www.nuget.org/packages/stellar-dotnet-sdk)

### Visual Studio

- Using the [console](https://docs.microsoft.com/en-us/nuget/consume-packages/install-use-packages-powershell)
    - Run `Install-Package stellar-dotnet-sdk` in the console.
- Using
  the [NuGet Package Manager](https://docs.microsoft.com/en-us/nuget/consume-packages/install-use-packages-visual-studio)
    - Search this package [NuGet Package](https://www.nuget.org/packages/stellar-dotnet-sdk) and install it.

### JetBrains Rider

- [Using NuGet in Rider](https://www.jetbrains.com/help/rider/Using_NuGet.html)

### Other

- [Ways to install a NuGet package](https://docs.microsoft.com/en-us/nuget/consume-packages/overview-and-workflow#ways-to-install-a-nuget-package)

### Examples

The SDK includes numerous example applications showcasing its features. Explore these standalone projects:

- [Horizon Examples](https://github.com/Beans-BV/dotnet-stellar-sdk/tree/master/Examples/Horizon/HorizonExamples.cs)
- [Soroban Examples](https://github.com/Beans-BV/dotnet-stellar-sdk/tree/master/Examples/Soroban/SorobanExamples.cs)

### HTTP retry & resilience

The SDK ships with opt-in retry support that matches the .NET industry standard
(`Microsoft.Extensions.Http.Resilience.AddStandardResilienceHandler`):

```csharp
var resilience = HttpResilienceOptionsPresets.WithStandardRetries();
var server = new Server("https://horizon-testnet.stellar.org", resilience, bearerToken: null);
```

`WithStandardRetries()` retries HTTP 408, 429, 500, 502, 503, 504 for safe methods
(GET, HEAD, OPTIONS) with 3 attempts, exponential backoff with jitter, and respects the
`Retry-After` response header. POST/PUT/PATCH/DELETE are NOT retried by default — set
`options.RetryUnsafeHttpMethods = true` to opt in (only if your endpoint is idempotent
or tolerates duplicates, e.g., via idempotency keys).

Other presets:

- `WithConnectionRetries()` — retries only transport-level failures (`HttpRequestException`,
  timeouts). No status-code retries. Use this when you want connection robustness only.
- `ForSorobanPolling()` — tuned for long-running Soroban operations; same status-code
  retries as `WithStandardRetries()` plus a higher retry budget and longer delays.
- `LowLatency()` — minimal retries and short delays for trading bots and similar workloads.
- `NoRetry()` — no retries at all.

> **Behavior change (upgrading):** `ForSorobanPolling()` now also retries the transient HTTP
> status codes (408/429/500/502/503/504) for safe methods and honors `Retry-After`; previously
> it retried connection failures only. Use `WithConnectionRetries()` (or a custom
> `HttpResilienceOptions` with an empty `RetryHttpStatusCodes`) to keep the old connection-only
> behavior.

The retry pipeline observes responses inside the HTTP handler chain, so it triggers
*before* status codes are translated into typed exceptions like `TooManyRequestsException`
and `ServiceUnavailableException`. Those exceptions also expose a
`RetryAfterDelay` (`TimeSpan?`) accessor for direct inspection when needed.

**A note on `Retry-After` for Stellar services.** Horizon sends `Retry-After` only on
HTTP 429 (always as an integer number of seconds); on HTTP 503 it relies on the client's
configured backoff. Stellar RPC (Soroban) does not send `Retry-After` at all — overload
is reported via HTTP 503/504 or JSON-RPC error bodies, so the retry pipeline falls back
to exponential backoff. The parser still accepts the RFC 7231 HTTP-date form because
upstream proxies and CDNs (Cloudflare, nginx, API gateways) may rewrite the header.

## Documentation

Documentation is available [here](https://beans-bv.github.io/dotnet-stellar-sdk/).

## Community & Support

- [Stellar Stack Exchange](https://stellar.stackexchange.com/)
- [Keybase Team](https://keybase.io/team/stellar_dotnet)
- [Stellar Developers on Discord](https://discord.com/invite/stellardev)

## Contributing

For information on how to contribute, please refer to
our [contribution guide](https://github.com/Beans-BV/dotnet-stellar-sdk/blob/master/CONTRIBUTING.md).

## License

`dotnet-stellar-sdk` is licensed under an Apache-2.0 license. See
the [LICENSE](https://github.com/Beans-BV/dotnet-stellar-sdk/blob/master/LICENSE.txt) file for details.
