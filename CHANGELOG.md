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
- `StellarRpcServer.SimulateTransaction` accepts an optional `useUpgradedAuth` flag, opting a simulation in to
  CAP-71 v2 authorization entries: recording mode then returns `SorobanAddressCredentialsV2`
  (`SOROBAN_CREDENTIALS_ADDRESS_V2`) instead of the legacy `SorobanAddressCredentials`, whose signature is not
  bound to the credential address and can therefore be replayed against another account. Signing needs no
  change at the call site — `SorobanAuthorization.AuthorizeEntry` already preserves whichever variant
  simulation returned and signs it over the matching preimage
  ([#187](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/187)); a new Testnet integration test simulates
  with the flag, signs the recorded v2 entry and submits it, which is the first end-to-end proof that the SDK's
  address-bound preimage is accepted by a live host.

  The flag is opt-in and unset by default, so the SDK's own *behaviour* is unchanged: omit it and Stellar RPC
  keeps returning v1 credentials. **Breaking:** its *binary* compatibility is not — appending the parameter
  changes the CLR signature of `SimulateTransaction`, so an application compiled against an earlier release that
  drops in this assembly without recompiling throws `MissingMethodException` at the call site. Recompiling is
  enough; no source change is needed. It is also transitional — RPC intends to flip its *server-side* default
  to v2 at protocol 29, at which point the flag becomes a no-op, and to stop returning v1 at protocol 30, so
  nothing should rely on omitting it to keep receiving v1
  ([#206](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/206)).
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
- `KeyPair` implements `IDisposable`: disposing releases the cached Ed25519 signing handle
  deterministically — the NSec key on `net8.0`/`net10.0` (libsodium secure memory: one mlocked region
  per signing keypair, otherwise held until finalization) is freed, and the expanded private-key copy
  on `netstandard2.1` is zeroed. After disposal `Sign`/`SignDecorated`/`SignPayloadDecorated` throw
  `ObjectDisposedException` (on every disposed keypair, including public-key-only ones); public-key
  operations and the stored seed remain usable — disposal releases signing resources, it does not
  erase the seed. Signing and disposal are serialized inside the signer, so a `Dispose` concurrent
  with an in-flight `Sign` is safe: the in-flight signature completes and stays valid, and any signing
  call that starts after disposal throws. Disposal is optional — an undisposed NSec key is still freed
  at finalization, while an undisposed `netstandard2.1` key copy is reclaimed by the GC without
  zeroing — and harmless for keypairs that never signed, though it disables `Sign` for them too
  ([#195](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/195) follow-up).

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
- **Breaking (behavioral):** `KeyPair.Sign`/`SignDecorated` on a keypair without a private key now
  throw `InvalidOperationException` instead of the base `Exception` (still caught by any existing
  `catch (Exception)`), and the message references the correctly-cased `KeyPair.FromSecretSeed`
  factory (previously `fromSecretSeed`, a leftover from the Java SDK port). These methods can also
  throw `ObjectDisposedException` now, but only after an explicit call to the new `KeyPair.Dispose`
  (see *Added*) — existing callers that never dispose are unaffected.

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

- `StellarRpcServer.SimulateTransaction` now sends the `authMode` parameter using the values Stellar RPC
  accepts (`enforce`, `record`, `record_allow_nonroot`). RPC matches this field case-sensitively against
  those three literals, so the parameter was non-functional in every release that offered it
  (14.0.0 onwards) — but it failed in two different ways, because the SDK changed JSON stacks in 15.0.0:
  - **15.0.0 through 16.0.0-beta** serialized the `AuthMode` enum under System.Text.Json's default enum
    naming, putting `ENFORCE`/`RECORD`/`RECORD_ALLOW_NONROOT` on the wire. RPC rejected each one with
    `optional 'authMode' must be one of enforce,record,record_allow_nonroot when included`. It reports
    this inside the simulation result rather than as a JSON-RPC error, so it surfaced on
    `SimulateTransactionResponse.Error` and was easily mistaken for a failed simulation.
  - **14.0.0 and 14.0.1** built the request with Newtonsoft.Json, whose default enum handling emits the
    ordinal — `"authMode":0`/`1`/`2`. RPC's `authMode` is a string field, so the request failed to
    unmarshal and RPC replied with a JSON-RPC error instead (`-32602 invalid parameters`,
    `json: cannot unmarshal number into Go struct field SimulateTransactionRequest.authMode of type
    string`). The SDK does not model JSON-RPC errors, so `SimulateTransaction` returned `null` rather
    than a response carrying `Error`, typically surfacing as a `NullReferenceException` at the call site.

  Either way, callers who passed an `AuthMode` were silently simulating nothing. Passing no `authMode`
  was, and remains, unaffected — the field is omitted entirely and RPC applies its own default.

  `AuthMode` now carries the wire spelling on the type itself (`[JsonStringEnumMemberName]` on each member
  plus a type-level `[JsonConverter]`), so serializing the enum produces the RPC form rather than only the
  one call site that remembers to convert — for `JsonOptions.DefaultOptions`, a bare
  `JsonSerializerOptions`, and the parameterless `JsonSerializer.Serialize` alike. (A caller who registers
  their own `AuthMode` converter still wins: System.Text.Json checks the options' `Converters` collection
  before a type-level attribute. That does not affect the `authMode` request field, which is now built from
  an explicit mapping rather than by serializing the enum.) Two knock-on effects, both limited to code that
  serializes `AuthMode` directly: it now writes `"enforce"` instead of `0` under a plain
  `JsonSerializerOptions`, and reading it back accepts only the lowercase spellings — `"ENFORCE"` and
  `"Enforce"` previously round-tripped through `JsonOptions.DefaultOptions` and now throw `JsonException`
  (member-name matching is case-sensitive once `[JsonStringEnumMemberName]` is applied). Nothing in the SDK
  deserializes `AuthMode`, and no Stellar RPC response carries the field
  ([#208](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/208)).
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
  human-readable `abs_before` string still looks correct. `PredicateBeforeAbsoluteTime.DateTime` now
  parses `abs_before` with the same rules as that consistency check (invariant culture; a value without
  an offset designator is interpreted as UTC) instead of the machine's current culture and local time
  zone, so an epoch-less payload resolves to the same deadline instant on every machine. Horizon always
  emits `abs_before` with an explicit offset, so values from Horizon are unaffected.
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
  and write, matching the `DateOnly`-based behavior on `net8.0`/`net10.0` — including the exception
  message for malformed date strings, which is now the same "Cannot convert JSON value '…' to an
  ISO 8601 date." text on every TFM (for non-string JSON tokens such as numbers, the exception type
  is `JsonException` everywhere but the text is System.Text.Json's own and names the target type,
  which differs per TFM). Previously the `netstandard2.1` build silently accepted and re-emitted
  malformed date strings through `KycJsonOptions`
  ([#195](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/195) follow-up).
- The XDR generator's blessed test snapshots are regenerated to match the
  [#195](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/195) template changes
  (`Throw.IfNull`, `ReadExactlyCompat`, `AddRangeCompat`) — the Ruby snapshot suite failed on `main`
  since that merge. The suite now normalizes line endings (so it passes on Windows and Linux alike)
  and runs in CI via a new `xdr_generator_tests.yml` workflow, so template/snapshot
  desync can no longer land silently.
- Cross-TFM behavior parity for the compatibility shims
  ([#195](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/195) follow-up):
  - `Util.HexToBytes` (all TFMs) throws `ArgumentNullException` for null and `FormatException` for
    odd-length input instead of `NullReferenceException` / `IndexOutOfRangeException` escaping the
    decode loop. This restores the `Convert.FromHexString` contract at the call sites
    (`LedgerKeyContractCode`, `ContractExecutableWasm.ToXdr`, `ClaimableBalanceIdUtils`) that
    [#195](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/195) switched from
    `Convert.FromHexString` to `HexToBytes` — `HexToBytes` itself never had that contract. (Note:
    `ClaimableBalanceIdUtils.FromHexString` catches everything and rethrows `ArgumentException`, so
    its own callers observe `ArgumentException` either way, exactly as they did in released
    versions.)
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
