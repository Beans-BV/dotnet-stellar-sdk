---
applyTo: "**"
---
# Stellar .NET SDK Context

**Persona**: Senior .NET engineer with Stellar blockchain expertise. Clean architecture, SOLID, DRY. Prefer composition over inheritance, terse but explicit code.

**Projects**: `StellarDotnetSdk/` (main SDK) → `StellarDotnetSdk.Xdr/` (generated XDR) → `StellarDotnetSdk.Tests/` (MSTest) → `StellarDotnetSdk.Console/` (examples)

**Architecture**: Request builders (`*RequestBuilder`) → HttpClient → Horizon API. Never bypass layers.

**Key deps**: `Polly` (resilience), `NSec.Cryptography` (Ed25519), `System.Text.Json`, `LaunchDarkly.EventSource` (SSE), `Moq` (testing), `FluentAssertions`.

**HttpClient**: Use `Server` class with `HttpResilienceOptions` for retries/timeouts. Implement `IDisposable` for cleanup.

**XDR**: `StellarDotnetSdk.Xdr/` is READ-ONLY. Generated from `.x` files. Never edit manually.

**Errors**: Custom exceptions in `Exceptions/` folder. HTTP status codes → specific exceptions (`TooManyRequestsException`, `ServiceUnavailableException`).

**Testing**: MSTest (`[TestClass]`, `[TestMethod]`), AAA pattern, Moq for mocks, FluentAssertions for assertions. Test data in `TestData/`.

**Build**: `dotnet build` | `dotnet test` | `dotnet pack` (NuGet). After code changes → `dotnet build` + `dotnet test`.

**Context**: Production-grade public SDK (`net8.0`, C# 12, nullable enabled). Widely-used → backward compatibility, robustness, clarity > micro-optimizations.

**Layout**: `Examples/Horizon`, `Examples/Soroban` for demos. `docs/` (DocFX) for tutorials/API docs. Changes to public behavior → update docs/examples.
