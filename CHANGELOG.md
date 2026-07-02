# Changelog

All notable changes to this project are documented here. The format is based on
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project aims to follow
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

> **Breaking changes below require the next release to be a major version bump.**

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
- The `SorobanCredentials`, `SorobanSourceAccountCredentials`, and `SorobanAddressCredentials` classes
  moved from `InvokeHostFunctionOperation.cs` to a new `SorobanCredentials.cs` file. They remain in the
  `StellarDotnetSdk.Operations` namespace, so `using`/fully-qualified references are unaffected.
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
  `net8.0`/`net10.0`). `RetryingHttpMessageHandler` runs the synchronous `HttpClient.Send` path
  through the full resilience pipeline on `net8.0`/`net10.0`; on the `netstandard2.1` assembly
  (which `net5`–`net7` apps also resolve) it derives from `HttpMessageHandler` instead of
  `DelegatingHandler`, so synchronous `Send` on a .NET 5+ host throws `NotSupportedException`
  instead of silently bypassing retries/circuit-breaker — use `SendAsync`. Consequently the handler
  does not expose `DelegatingHandler.InnerHandler` on `netstandard2.1`
  ([#195](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/195)).
- `KeyPair.Sign` expands/imports the Ed25519 signing key once per `KeyPair` instance (lazily,
  thread-safe) and reuses it for subsequent signatures, instead of re-deriving it on every call —
  repeated signing with the same instance is ~3–4× faster on both crypto backends
  ([#195](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/195) follow-up).
- **Breaking (behavioral):** `KeyPair` constructors and byte-array factories (`FromPublicKey`,
  `FromSecretSeed(byte[])`) now throw `ArgumentException` for wrong-length key material and
  `ArgumentNullException` for null, uniformly on all target frameworks and always at construction
  time. Previous releases surfaced NSec's `FormatException` instead.

### Removed

- **Breaking:** `SorobanSourceAccountCredentials.ToSorobanCredentialsXdr()` and
  `SorobanAddressCredentials.ToSorobanCredentialsXdr()`. Use `ToXdr()` instead (it now produces the same
  XDR via the `abstract`/`override` pair).

### Fixed

- `Util.Hash` no longer leaks a `SHA256` instance on every call: it uses the static
  `SHA256.HashData` on `net8.0`/`net10.0` and a properly disposed instance on `netstandard2.1`
  ([#195](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/195)).
- `KeyPair` construction-time validation lost in
  [#195](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/195) is restored: the
  `KeyPair(byte[], byte[]?, byte[]?)` constructor rejects public keys, private keys, and seeds that
  are not exactly 32 bytes (e.g. `FromPublicKey(new byte[16])` no longer constructs a keypair with a
  malformed account ID), and the `privateKey`/`seed` arguments are tracked separately again — a
  seed-only `KeyPair` reports `CanSign() == false` and a private-key-only `KeyPair` no longer
  exposes the private key through `SecretSeed`/`SeedBytes`.
- SEP-0009 KYC date fields (`BirthDate`, `IdIssueDate`, `IdExpirationDate`, `RegistrationDate`) are
  now validated during JSON (de)serialization on `netstandard2.1` too: the new
  `IsoDateStringJsonConverter` rejects anything but `yyyy-MM-dd` with a `JsonException` on both read
  and write, matching the `DateOnly`-based behavior on `net8.0`/`net10.0`. Previously the
  `netstandard2.1` build silently accepted and re-emitted malformed date strings through
  `KycJsonOptions` ([#195](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/195) follow-up).
- The XDR generator's blessed test snapshots are regenerated to match the
  [#195](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/195) template changes
  (`Throw.IfNull`, `ReadExactlyCompat`, `AddRangeCompat`) — the Ruby snapshot suite failed on `main`
  since that merge. The suite now normalizes line endings (so it passes on Windows and Linux alike)
  and runs in CI via a new `xdr_generator_tests.yml` workflow, so template/snapshot
  desync can no longer land silently.
- Cross-TFM behavior parity for the compatibility shims
  ([#195](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/195) follow-up):
  - `Util.HexToBytes` (all TFMs) throws `ArgumentNullException` for null and `FormatException` for
    odd-length input — the `Convert.FromHexString` contract the released package had — instead of
    `NullReferenceException` / `IndexOutOfRangeException` escaping the decode loop.
  - The `netstandard2.1` `Throw.IfNullOrEmpty` polyfill emits the BCL's
    "The value cannot be an empty string." message.
  - The `netstandard2.1` `ReadAsStringAsync` cancellation shim surfaces `TaskCanceledException`
    (with the token attached), matching the real net6+ overload, instead of the base
    `OperationCanceledException`.
  - The XDR `ReadExactlyCompat` shim throws `EndOfStreamException` with the BCL's
    "Unable to read beyond the end of the stream." message.
  - The Sodium key handles used by the `netstandard2.1` Ed25519 backend are disposed after use.
- `integration_tests.yml` installs both the `8.0.x` and `10.0.x` SDKs; the previous 8-only pin
  satisfied `global.json` only because the runner image happened to preinstall .NET 10.
- The README "Platform support" section documents that Unity 2022.3's bundled compiler cannot
  construct SDK types with `required` members (Unity 6 or an upgraded Roslyn can).
