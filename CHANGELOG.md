# Changelog

All notable changes to this project are documented here. The format is based on
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project aims to follow
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [16.0.0-beta0] - 2026-06-25

> **First beta of the 16.0 major line.** Protocol 27 (CAP-71) Soroban authorization, SEP-45 web
> authentication for contract accounts, an HTTP retry overhaul, and stricter `System.Text.Json` / XDR
> decoding. The breaking changes below make this a major version bump.

### Added

- **Protocol 27 (CAP-71) Soroban authorization** ([#187](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/187), implements [#186](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/186)):
  - `SorobanAddressCredentialsV2` — CAP-0071-02 address-bound credentials (`SOROBAN_CREDENTIALS_ADDRESS_V2`),
    whose signature is computed over the `ENVELOPE_TYPE_SOROBAN_AUTHORIZATION_WITH_ADDRESS` preimage,
    preventing cross-account signature replay.
  - `SorobanAddressCredentialsWithDelegates`, `SorobanDelegateSignature`, and `SorobanDelegatedRoot`
    (a non-serializable view of the delegated root credential) — CAP-0071-01 delegated credentials
    (`SOROBAN_CREDENTIALS_ADDRESS_WITH_DELEGATES`).
  - `SorobanAuthorization` signing helpers: `AuthorizeEntry`, `AuthorizeEntryWithDelegates`,
    `BuildWithDelegatesEntry`, `BuildAuthorizationEntryPreimageHash`, and the lower-level
    `BuildAuthPreimageHash` / `BuildAddressAuthPreimageHash`.
  - `ISorobanEntrySigner` with the built-in `KeyPairEntrySigner` (classic Ed25519), plus a
    `SorobanCredentialsVersion` (`Preserve`/`V1`/`V2`) option that defaults to preserving the entry's
    existing credential variant (matching the JS reference SDK).
  - Signing output is verified byte-for-byte against `@stellar/stellar-sdk@16.0.0-rc.1` for the V2 and
    delegated paths via the known-answer vectors in `StellarDotnetSdk.Tests/TestData/generate-p27-auth-kat.mjs`.
- **SEP-45 (Web Authentication for Contract Accounts)** ([#190](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/190), implements [#160](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/160)):
  - `ClientWebAuthContract` — end-to-end client flow: `FromDomainAsync`, `GetChallengeAsync`,
    `ValidateChallenge`, `SignAuthorizationEntriesAsync`, `SendSignedChallengeAsync`, `JwtTokenAsync`.
  - `Sep45Challenge` — parse, validate, verify, and build challenges, including the `web_auth_verify`
    invocation hash that signers sign.
  - Hostile-server hardening: the challenge decoder caps input size (`MaxChallengeXdrBytes`, 64 KiB)
    and entry count to bound decode-time allocation.
  - Typed validation exceptions (`InvalidServerSignatureException`, `InvalidNonceException`,
    `InvalidWebAuthDomainException`, `InvalidClientDomainException`, …).
- **HTTP retry** ([#184](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/184)): new `ForSoroban()` and
  `ForHorizon()` presets; configurable `RetryHttpStatusCodes` and `RetryHttpMethods`; `Retry-After`
  support (`RespectRetryAfter`, `MaxRetryAfterDelay`); new `TooManyRequestsException` (HTTP 429) and
  `ServiceUnavailableException` (HTTP 503).
- Integration test suite, phase 1 — Testnet-backed `StellarDotnetSdk.IntegrationTests` project
  (development-only; not shipped in the NuGet package) ([#185](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/185)).
- SEP compatibility matrices ([#191](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/191)).
- **Multi-target NuGet packages: `net10.0`, `net8.0`, and `netstandard2.1`**
  ([#195](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/195), implements
  [#162](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/162)):
  - `stellar-dotnet-sdk` and `stellar-dotnet-sdk-xdr` now ship all three target frameworks; NuGet picks
    the best match automatically. `netstandard2.1` covers Unity 2022.3+/Unity 6, Tizen 5.5+, and other
    portable-library hosts (see the new "Platform support" section in the README).
  - Ed25519 backend per TFM: NSec.Cryptography 26.4.0 on `net10.0`, 25.4.0 on `net8.0`, and
    Sodium.Core 1.4.1 on `netstandard2.1`. Backend equivalence is enforced by the new cross-provider
    known-answer tests (`Ed25519CrossProviderTest`), and the full unit suite additionally runs against
    the `netstandard2.1` build via the new `StellarDotnetSdk.NetStandard21.Tests` project.
  - New `KycJsonOptions` (frozen `JsonSerializerOptions` singleton for SEP-0009 KYC types) and strict
    `DateOnlyJsonConverter` / `NullableDateOnlyJsonConverter` (ISO `yyyy-MM-dd` only; malformed values
    throw `JsonException` on deserialization).
  - On `netstandard2.1`, the SEP-0009 date properties (`BirthDate`, `IdIssueDate`, `IdExpirationDate`,
    `RegistrationDate`) are `string?` instead of `DateOnly?`; values are validated as ISO `yyyy-MM-dd`
    when the fields are submitted (throwing `ArgumentException` otherwise, including from
    `InteractiveService.DepositAsync`/`WithdrawAsync`), so the SEP-9 wire format is identical on every
    TFM.

### Changed

- **Breaking:** `SorobanCredentials.ToXdr()` is now `abstract` (was a concrete method that switched on
  the runtime type). External subclasses of `SorobanCredentials` must now override `ToXdr()`.
  ([#187](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/187))
- **Breaking:** the `ForSoroban()` / `ForHorizon()` presets now retry transient HTTP status codes
  (408/429/500/502/503/504) on POST — Stellar RPC JSON-RPC calls and Horizon `SubmitTransaction()` —
  not just connection failures. Safe because Stellar submission is idempotent (keyed by transaction
  hash + source-account sequence). ([#184](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/184))
- **Breaking:** `XdrDataInputStream` scalar reads now throw `EndOfStreamException` (was
  `IndexOutOfRangeException`) on truncated input.
  ([#189](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/189), addresses [#165](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/165))
- **Breaking:** duplicate JSON properties are now rejected (`AllowDuplicateProperties = false`) —
  duplicate keys throw `JsonException` instead of silently using the last value.
  ([#181](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/181), addresses [#166](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/166))
- **Breaking:** the shared `JsonSerializerOptions` are frozen via `MakeReadOnly()`; mutating them at
  runtime throws `InvalidOperationException`.
  ([#183](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/183), addresses [#168](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/168))
- The `SorobanCredentials`, `SorobanSourceAccountCredentials`, and `SorobanAddressCredentials` classes
  moved from `InvokeHostFunctionOperation.cs` to a new `SorobanCredentials.cs` file. They remain in the
  `StellarDotnetSdk.Operations` namespace, so `using`/fully-qualified references are unaffected.
  ([#187](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/187))
- Enable `RespectNullableAnnotations` for JSON deserialization — non-nullable reference-type properties
  are enforced during deserialization.
  ([#182](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/182), addresses [#167](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/167))
- Use `FrozenDictionary` for static lookup tables in the JSON converters.
  ([#180](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/180), addresses [#164](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/164))
- **Breaking (behavioral, `net8.0`):** the SDK no longer references the standalone `System.Text.Json`
  10.0.6 package and uses each target framework's built-in System.Text.Json instead. As a result,
  `AllowDuplicateProperties = false` and `RespectNullableAnnotations = true` on
  `JsonOptions.DefaultOptions` now apply on `net10.0` only; on `net8.0` and `netstandard2.1`,
  deserialization follows the STJ 8 defaults — the last duplicate JSON property wins and nullability
  annotations are not enforced ([#195](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/195)).
- `KeyPair.Verify` no longer swallows every exception. Malformed or attacker-supplied signatures still
  return `false` (`ArgumentException`, `FormatException`, and `CryptographicException` are caught), but
  environmental failures — e.g. a missing native libsodium — now propagate instead of being misreported
  as an invalid signature ([#195](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/195)).
- On `netstandard2.1`, the default HTTP handler is `HttpClientHandler` (`SocketsHttpHandler` on
  `net8.0`/`net10.0`), and `RetryingHttpMessageHandler` overrides the synchronous `HttpClient.Send`
  path only on `net8.0`/`net10.0` — use `SendAsync` on `netstandard2.1`
  ([#195](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/195)).

### Deprecated

- `ForSorobanPolling()` — now an `[Obsolete]` alias for `ForSoroban()`. `ForSoroban()` additionally
  retries transient HTTP status codes on POST, which the original connection-failure-only
  `ForSorobanPolling()` did not. ([#184](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/184))

### Removed

- **Breaking:** `SorobanSourceAccountCredentials.ToSorobanCredentialsXdr()` and
  `SorobanAddressCredentials.ToSorobanCredentialsXdr()`. Use `ToXdr()` instead (it now produces the same
  XDR via the `abstract`/`override` pair). ([#187](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/187))

### Fixed

- Handle `contract_credited` / `contract_debited` effects in responses.
  ([#179](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/179), fixes [#172](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/172))
- Documentation build. ([#178](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/178))

[Unreleased]: https://github.com/Beans-BV/dotnet-stellar-sdk/compare/16.0.0-beta0...HEAD
[16.0.0-beta0]: https://github.com/Beans-BV/dotnet-stellar-sdk/compare/15.1.0...16.0.0-beta0

### Fixed

- `Util.Hash` no longer leaks a `SHA256` instance on every call: it uses the static
  `SHA256.HashData` on `net8.0`/`net10.0` and a properly disposed instance on `netstandard2.1`
  ([#195](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/195)).
