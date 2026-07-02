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

- **Built-in HTTP resilience for Stellar** - Opt-in retries for 408/429/5xx tuned for Horizon (`ForHorizon()`) and Stellar RPC / Soroban (`ForSoroban()`), with `Retry-After` honored (capped by `MaxRetryAfterDelay`, default 1 minute)
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

## Platform support

The `stellar-dotnet-sdk` and `stellar-dotnet-sdk-xdr` packages multi-target the following frameworks:

| Target framework | Typical platforms | Crypto backend |
|------------------|-------------------|----------------|
| `net10.0` | .NET 10 apps | NSec |
| `net8.0` | .NET 8 apps | NSec |
| `netstandard2.1` | Unity 2022.3+, Unity 6, Tizen 5.5+, portable libraries | Sodium.Core |

NuGet resolves the best matching assembly for your project automatically.

### TFM-specific API notes

- **SEP-0009 date fields** (`BirthDate`, `IdIssueDate`, `IdExpirationDate`, `RegistrationDate`): `DateOnly?` on `net8.0` / `net10.0`; `string?` (ISO `yyyy-MM-dd`) on `netstandard2.1`. JSON wire format is identical across TFMs.
- **Synchronous `HttpClient.Send` resilience**: `RetryingHttpMessageHandler` overrides sync `Send` only on `net8.0` / `net10.0`. Use `SendAsync` on `netstandard2.1`.

### Examples

The SDK includes numerous example applications showcasing its features. Explore these standalone projects:

- [Horizon Examples](https://github.com/Beans-BV/dotnet-stellar-sdk/tree/master/Examples/Horizon/HorizonExamples.cs)
- [Soroban Examples](https://github.com/Beans-BV/dotnet-stellar-sdk/tree/master/Examples/Soroban/SorobanExamples.cs)

### HTTP retry & resilience

The SDK ships with opt-in retry support tuned for Stellar's HTTP surfaces (Horizon and Stellar RPC).
Pick the preset that matches the server you're calling:

```csharp
// For Horizon — GET queries and SubmitTransaction()
var resilience = HttpResilienceOptionsPresets.ForHorizon();
var server = new Server("https://horizon-testnet.stellar.org", resilience, bearerToken: null);

// For Stellar RPC (Soroban)
var rpcResilience = HttpResilienceOptionsPresets.ForSoroban();
var rpcClient = new DefaultStellarSdkHttpClient(resilienceOptions: rpcResilience);
var rpcServer = new StellarRpcServer("https://soroban-testnet.stellar.org", rpcClient);
```

Both retry HTTP 408, 429, 500, 502, 503, 504 on the Stellar requests they're scoped to (see the
matrix below) with exponential backoff, jitter, and `Retry-After` honored (capped by
`MaxRetryAfterDelay`, default 1 minute). They differ in retry budget and delay range.

#### Which Stellar operations are retried?

`HttpResilienceOptions.RetryHttpMethods` is an explicit whitelist. By default it contains only the
RFC-safe methods (`GET`, `HEAD`, `OPTIONS`). Each preset opts in to the additional methods it needs.

| Stellar operation | HTTP method | Retried by `ForHorizon()` | Retried by `ForSoroban()` |
|---|---|---|---|
| Horizon queries (`Server.Accounts`, `Ledgers`, `Operations`, `Effects`, `Root`, …) | GET | ✅ | ✅ |
| `Server.SubmitTransaction()` / `SubmitTransactionAsync()` | POST | ✅ | n/a |
| Every `StellarRpcServer` method — read (`getLatestLedger`, `simulateTransaction`, `getEvents`, …) or write (`sendTransaction`) | POST | n/a | ✅ |
| SEP-6 `PATCH /transactions/{id}` | PATCH | ❌ | ❌ |

Retrying `SubmitTransaction()` is safe on Stellar even though it is HTTP POST: every envelope is
uniquely keyed by transaction hash plus the source account's sequence number, so a resubmit either
returns the cached server result or fails with `tx_bad_seq` — there is no double-spend window. If
the original submission had already committed when the transient failure occurred, the retry
surfaces `tx_bad_seq` and your code should look up the transaction by its hash to recover the
original result.

> **⚠️ Do not wire `ForHorizon()` or `ForSoroban()` into SEP service clients**
> (`ClientWebAuth`, `InteractiveService`, `TransferServerService`, `StellarToml` with a custom
> `HttpClient`). Specific SEP POST endpoints are **non-idempotent by spec** and silently retrying
> them creates real problems:
>
> - **SEP-10 `POST /auth`** — the spec says: *"The Server should not provide more than one JWT for
>   a specific challenge transaction."* The challenge is one-shot. On transient failure, request a
>   **fresh** challenge — don't resubmit the same body.
> - **SEP-24 `POST /transactions/{deposit,withdraw}/interactive`** — each call mints a fresh
>   `transaction_id`. Retrying creates a duplicate transaction record and an orphaned interactive
>   URL. The spec defines no idempotency-key mechanism.
> - **SEP-6 `PATCH /transactions/{id}`** — not in the SEP-6 master spec; anchor-vendor extension
>   that mutates KYC state. Treat as non-idempotent.
>
> For SEP HttpClients, use `WithConnectionRetries()` (transport retries only) or build a custom
> `HttpResilienceOptions` whose `RetryHttpMethods` contains only `GET`/`HEAD`/`OPTIONS`. Note that
> transport (connection-failure) retries apply to **all** HTTP methods: a POST whose response was
> lost may already have been processed server-side, so even `WithConnectionRetries()` carries a
> small replay window. If that window is unacceptable, use `NoRetry()` and recover at the
> application level (e.g. request a fresh SEP-10 challenge).

#### Presets

- `ForHorizon()` — for the Stellar Horizon API. Retries the transient HTTP status codes on
  Horizon's GET queries and `SubmitTransaction()` POST. 3 retries, 200ms–5s exponential backoff
  with jitter, honors `Retry-After`. PATCH/PUT/DELETE are not retried.
- `ForSoroban()` — for Stellar RPC (Soroban). Same status-code set as `ForHorizon()`; adds POST
  to the retry-method whitelist (every JSON-RPC call is POST). Higher retry budget (5) and longer
  delays (up to 15s) — tuned for long-running polling workflows like `getTransaction(hash)`. For
  latency-sensitive one-off calls (e.g. `simulateTransaction`), override `MaxRetryCount`/`MaxDelay`.
- `WithConnectionRetries()` — transport-level retries only (`HttpRequestException` and timeouts
  thrown inside the handler chain; an `HttpClient.Timeout` cancellation is never retried). No
  status-code retries, but transport retries apply to **all** HTTP methods — see the SEP warning
  above for the replay window this implies on one-shot endpoints.
- `LowLatency()` — minimal retries and short delays (trading bots, latency-sensitive workloads).
- `NoRetry()` — no retries at all.

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
