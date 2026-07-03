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
- **Breaking:** the SDK references the standalone `System.Text.Json` 10.0.6 package on `net8.0` and
  `netstandard2.1` (`net10.0` uses the built-in STJ 10), so `AllowDuplicateProperties = false` and
  `RespectNullableAnnotations = true` on `JsonOptions.DefaultOptions` / `KycJsonOptions.Default`
  apply on **every** target framework: a duplicate JSON property on a POCO-mapped field now throws
  `JsonException` instead of last-write-wins (fields parsed by the SDK's hand-written converters,
  e.g. `Reserve`/`Asset`/`AssetAmount`, are outside this option's reach but are now guarded
  separately — see the Security entry below), and explicit `null` for a non-nullable member also
  throws `JsonException`. For `KycJsonOptions.Default`, duplicate-property rejection is new on every
  TFM including `net10.0` (it previously enforced only nullability, and only on `net10.0`). This is
  breaking in two ways — payloads that previously deserialized on `net8.0`/`netstandard2.1`
  (duplicate keys, or explicit `null` for a non-nullable member) are now rejected, and consumers on
  `net8.0`/`netstandard2.1` inherit a transitive `System.Text.Json >= 10.0.6` floor plus its own
  dependencies (`System.Text.Encodings.Web` and `System.IO.Pipelines` 10.0.6 on both TFMs; on
  `netstandard2.1` also `Microsoft.Bcl.AsyncInterfaces` 10.0.6). `netstandard2.1` is the only target
  that previously pinned `System.Text.Json` 8.0.5, and relative to that 8.0.5 closure `System.IO.Pipelines`
  is net-new — one more DLL for consumers who vendor dependencies by hand, e.g. Unity. (`net8.0` never
  referenced 8.0.5: it resolved the built-in framework `System.Text.Json` before this package reference.)
  ([#195](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/195) follow-up).
- `KeyPair.Verify` no longer swallows every exception. Malformed or attacker-supplied signatures still
  return `false` (`ArgumentException`, `FormatException`, and `CryptographicException` are caught), but
  environmental failures — e.g. a missing native libsodium — now propagate instead of being misreported
  as an invalid signature ([#195](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/195)).
- On `netstandard2.1`, the default HTTP handler is `HttpClientHandler` (`SocketsHttpHandler` on
  `net8.0`/`net10.0`), and `RetryingHttpMessageHandler` overrides the synchronous `HttpClient.Send`
  path only on `net8.0`/`net10.0` — use `SendAsync` on `netstandard2.1`
  ([#195](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/195)).

### Security

- **Breaking:** converters that hand-parse JSON now reject objects that define the same property more
  than once (throwing `JsonException`, matched case-insensitively), on every target framework — payloads
  with duplicate keys that previously deserialized last-wins are now rejected. The serializer-level
  `AllowDuplicateProperties = false` guard on `JsonOptions.DefaultOptions` is enforced by the built-in
  object mapper only, so fields read manually by a converter were last-wins: a malformed or adversarial
  Horizon response could silently override a financial field by repeating its key. Hardened converters:
  `AssetAmount`, `Reserve`, `LiquidityPoolClaimableAssetAmount`, `Asset` (asset-code/issuer
  substitution), `Predicate` (claimable-balance time locks, checked at every nesting level), the
  HATEOAS `Link` converter (pagination `href`), and the SEP-45 `ChallengeForContractsResponse` converter
  (the adversarial `authorization_entries` blob the client signs — it already rejected duplicates inline
  and now shares the `JsonDuplicatePropertyGuard` helper). The polymorphic `OperationResponse`/`EffectResponse`
  converters read the `type_i` discriminator by hand and re-deserialize the payload through the object
  mapper; the mapper rejects duplicates of the mapped payload fields, but because `type_i` is a read-only
  property the mapper never binds a duplicated discriminator would otherwise slip through, so these two
  converters now apply the same guard to the whole object and reject any duplicate — discriminator
  included — before dispatching.

### Removed

- **Breaking:** `SorobanSourceAccountCredentials.ToSorobanCredentialsXdr()` and
  `SorobanAddressCredentials.ToSorobanCredentialsXdr()`. Use `ToXdr()` instead (it now produces the same
  XDR via the `abstract`/`override` pair).

### Fixed

- `PredicateJsonConverter` no longer leaks `FormatException`/`OverflowException` for malformed
  `rel_before`/`abs_before_epoch` values — every malformed predicate now throws `JsonException`, the
  SDK's documented deserialization failure mode. It also rejects `and`/`or` predicate arrays that do
  not contain exactly 2 elements (stellar-core validates `ClaimPredicate` AND/OR to exactly 2 children
  at ledger close, so Horizon never emits any other arity; extra elements were previously dropped
  silently) and validates the arity before deserializing any element, so an oversized array is no
  longer fully materialized. Time-bound values are now also range- and consistency-checked: a negative
  `rel_before`/`abs_before_epoch` is rejected (Stellar time bounds are unsigned), and a payload that
  supplies both `abs_before` and `abs_before_epoch` with disagreeing instants is rejected rather than
  silently preferring the epoch — a spoofed epoch can no longer shift a claim deadline while the
  human-readable `abs_before` string still looks correct.
- The `Asset` and `Reserve` converters now skip unrecognized properties with object/array values
  whole. Previously the reader descended into such values and treated their nested keys as top-level
  properties. With the new duplicate-property guard in place that surfaced as a misleading
  duplicate-property rejection of an otherwise-valid payload; in releases without that guard (≤ 15.1.0)
  a nested key reusing a top-level name — e.g. `amount` inside a `_links` object — could instead
  silently overwrite the top-level financial field. Skipping unrecognized values whole closes both.
- **Breaking:** the `Asset`, `AssetAmount`, `Reserve`, and `LiquidityPoolClaimableAssetAmount` converters
  now throw `JsonException` — the documented System.Text.Json deserialization failure mode — for missing,
  `null`, empty, or malformed `asset`/`amount`/`asset_code`/`asset_issuer` values, where they previously
  leaked `ArgumentException` (or `AssetCodeLengthInvalidException` for an out-of-range asset code). A
  consumer can now catch every malformed-response failure from `JsonSerializer.Deserialize` with a single
  `catch (JsonException)`; code that specifically caught `ArgumentException` from these converters must
  catch `JsonException` instead. (The `Asset.Create`/`Asset.CreateNonNativeAsset` factory methods, when
  called directly, still throw `ArgumentException`/`AssetCodeLengthInvalidException`.)
- `Util.Hash` no longer leaks a `SHA256` instance on every call: it uses the static
  `SHA256.HashData` on `net8.0`/`net10.0` and a properly disposed instance on `netstandard2.1`
  ([#195](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/195)).
