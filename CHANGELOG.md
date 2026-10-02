# Changelog

All notable changes to this project are documented here. The format is based on
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project aims to follow
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

> **Breaking changes below require the next release to be a major version bump.**

### Added

- **Protocol 28 support** (implements [#207](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/207), [#252](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/252)):
  - XDR regenerated from stellar-xdr `v28.0` (`9c9c145`, the XDR that Protocol 28 on Mainnet was built from):
    CAP-85 `SCV_EXECUTABLE_TAG` and `CONTRACT_EXECUTABLE_EXTERNAL_REF` (`ContractExecutableExternalRef`), and
    CAP-83 `STELLAR_VALUE_EMPTY_TX_SET`. Ledger entries, transaction metas, events and simulation results that
    contain the new arms previously failed to decode. The feature-gated CAP-84 (muxed contract addresses) and
    `TEST_FEATURE` blocks are not part of Protocol 28 and are left out; the XDR generator now names every
    leftover `#ifdef` block by file and line instead of failing with xdrgen's bare parse error.
  - Generated XDR string typedefs (`StellarDotnetSdk.Xdr`'s `SCString`, `SCSymbol`, `String32`, `String64`) keep
    the raw wire bytes in a new `InnerBytes` property and round-trip them verbatim. `InnerValue` remains as a
    UTF-8 view of those bytes: the getter replaces invalid UTF-8 with U+FFFD, the setter encodes. Previously a
    non-UTF-8 string was decoded lossily and re-encoded as different bytes, which for a CAP-85 tag is the key of
    a different ledger entry. The SDK-layer wrappers built on them (`Soroban.SCString` and `Soroban.SCSymbol`,
    and data entry names read through `String64.InnerValue`) still decode lossily; that is tracked in
    [#246](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/246).
  - `SCExecutableTag` (the `SCVal` for `SCV_EXECUTABLE_TAG`, raw bytes in `InnerValue`) and
    `ContractExecutableExternalRef` (a `ContractExecutable`, raw bytes in `Tag`), each with a strict UTF-8
    accessor (`TryGetUtf8String` / `TryGetTagUtf8String`) that fails on invalid UTF-8 instead of decoding it
    lossily. The constructors copy the tag bytes they are given, and the `InnerValue` / `Tag` getters and
    `ToXdr()` return a new copy on each call, so neither the caller's array, a returned array, nor the returned
    XDR object aliases the tag.
  - `CreateContractOperation.FromExternalRef` deploys a contract from an external executable reference, with
    `string` and `byte[]` tag overloads; binary tags pass through undecoded, and a non-contract owner is rejected
    up front. Every `string` tag overload encodes strict UTF-8 and throws `ArgumentException` for an unpaired
    surrogate, rather than silently naming a different tag.
  - `StellarRpcServer.GetExternalRefWasmHash` resolves a reference to its hex-encoded Wasm hash (the form
    `ContractExecutableWasm.WasmHash` and `FromAddress` use) with a single `getLedgerEntries` call. It throws the
    new `ExternalRefNotFoundException` when the tag entry is missing or archived (`IsArchived` tells the two
    apart). An entry whose `liveUntilLedgerSeq` is at or below `latestLedger` counts as archived, because no
    transaction submitted now can read it. A response that carries undecodable entry XDR, is not exactly the
    requested persistent entry, or returns that entry without a positive `latestLedger` throws
    `ClientProtocolException`, which gains a `(message, innerException)` constructor for the first case.
    `ExternalRefNotFoundException` shows a tag, and its owner, as quoted text only when it is printable ASCII
    other than `"` and `\`, and as hex otherwise, so a tag read from the chain cannot forge or hide message
    content. Both are cut to 64 bytes plus their length. Its `OwnerContractId` and `Tag` are null only when the
    exception is constructed without them, never when the SDK throws it.
  - Encodings are verified byte-for-byte against `@stellar/stellar-sdk@17.2.0` via the known-answer vectors in
    `StellarDotnetSdk.Tests/TestData/generate-p28-kat.mjs`. New Testnet integration tests check that Stellar
    RPC parses the SDK's tag ledger key and that the host decodes an external-reference deployment; the exact
    key bytes are pinned by the known-answer vectors.
- `GetLatestLedgerResponse` now exposes the remaining `getLatestLedger` fields served by Stellar RPC:
  `CloseTime` (`long?`, unix timestamp in seconds), `HeaderXdr`, and `MetadataXdr`. `GetHealthResponse` gains the
  RPC v27.1.0 fields `LatestLedgerCloseTime` and `OldestLedgerCloseTime` (`long?`, unix seconds). The three close
  times read the quoted wire value or a bare number, including under a caller's own serializer options that bind the
  wire names but do not enable `AllowReadingFromString`. All five fields are nullable so responses from older RPC
  servers that omit them still deserialize
  ([#198](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/198), completes
  [#155](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/155) and the compatibility-matrix scope of
  [#159](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/159)).
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
- `StellarRpcServer.SimulateTransaction` accepts an optional `useUpgradedAuth` flag selecting which CAP-71
  address-credential variant a recording-mode simulation returns: `true` asks for `SorobanAddressCredentialsV2`
  (`SOROBAN_CREDENTIALS_ADDRESS_V2`), `false` for the legacy `SorobanAddressCredentials`, whose signature is not
  bound to the credential address. Signing needs no change at the call site:
  `SorobanAuthorization.AuthorizeEntry` already preserves whichever variant simulation returned and signs it
  over the matching preimage ([#187](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/187)). Testnet
  integration tests simulate with the default and with the legacy opt-out, sign the recorded entry and submit
  it, which is the end-to-end proof that the SDK's address-bound preimage is accepted by a live host.

  Leaving the flag unset means `true`, and `false` is the legacy opt-out; the default, the opt-out's replay
  trade-off and its transitional status are described under the CAP-71 default flip in **Changed**.
  **Breaking (binary):** appending the parameter changes the CLR signature of `SimulateTransaction`, so an
  application compiled against an earlier release that drops in this assembly without recompiling throws
  `MissingMethodException` at the call site. Recompiling is enough; no source change is needed
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
- **SEP-0007 (URI Scheme to facilitate delegated signing)**, new namespace `StellarDotnetSdk.Sep.Sep0007`:
  - `UriScheme` builds `web+stellar:tx` and `web+stellar:pay` request URIs with every SEP-7 parameter
    (`GenerateSignTransactionUri`, `GeneratePayOperationUri`, typed `Memo` encoding included), and parses
    and validates them (`ParseUri`, `TryParseUri`, `ValidateUri` → `Sep7Uri`/`Sep7ValidationResult`).
    Validation covers the operation, per-operation parameters, addresses, amounts, assets, memo types and
    values, `msg` length, `callback` form, `origin_domain` FQDN, `chain` nesting (at most 7 levels), and
    rejects an `origin_domain` without a `signature`, as the spec does. A URI with a lone UTF-16 surrogate
    is rejected, since UTF-8 has no encoding for it and its signature would also cover other URIs.
    `destination` also takes a SEP-2 federation address; an `amount` is a positive decimal of at most
    7 decimal places and at most the int64 stroop maximum, leading zeros allowed (`0000000000001` is 1);
    hash memos must be exactly 32 bytes; account ids must be upper case; `callback` URLs may not carry
    whitespace, invisible characters, credentials, a host with no valid IDN form, a fragment, a
    malformed percent escape, a backslash or a `.`/`..` path segment. Rejections carry a typed
    `Sep7ValidationResult.Error`, and values echoed into messages are escaped and length-clamped. Query
    values are decoded like an HTML form (`+` is a space, as `URLSearchParams` and the other Stellar SDKs
    write it), except in base64 values, where a raw `+` stays a `+`; escapes that are not valid UTF-8
    (`%FF`, `%ED%A0%80`) make the request invalid, as decoders disagree on them (`URLSearchParams` reads
    U+FFFD, .NET's own decoder keeps the escape as text).
  - `replace` support: `ParseReplacements`/`ReplacementsToString` with `Sep7Replacement`, enforcing the
    spec's balanced-identifier rule and rejecting Txrep paths SEP-7 forbids (`tx.` prefix, signatures,
    `_present`/`len`).
  - Request signing: `SignUri` and offline `VerifySignature` reproduce the spec's signed example exactly;
    `VerifyOriginDomainSignatureAsync`/`IsValidSignedUriAsync` check the signature against the origin
    domain's stellar.toml `URI_REQUEST_SIGNING_KEY` and optionally pin that key
    (`UriRequestSigningKeyChangedException`). Only the stellar.toml's root `URI_REQUEST_SIGNING_KEY` is
    read, with a non-recursive reader: the domain is chosen by whoever wrote the URI, and the general
    `StellarToml` parser overflows the stack on a few kilobytes of hostile TOML. The stellar.toml and
    callback responses are read with a 512 KiB cap and within the HTTP client's `Timeout`, body included;
    the whole stellar.toml must be valid UTF-8, as TOML requires, and a callback's answer is decoded by its
    byte order mark, else by its `Content-Type` charset if .NET supports it, else as UTF-8;
    connection failures, including ones mid-body and resilience-pipeline rejections, surface as
    `OriginDomainStellarTomlException` (so `IsValidSignedUriAsync` reports them) or, for the callback, as
    `HttpRequestException`.
  - `SignAndSubmitTransactionAsync` verifies any `origin_domain` signature before it signs, then POSTs
    the signed envelope to the request's `callback` (https only, http for loopback) or submits it to
    Horizon; `SubmitTransactionAsync`/`SubmitToCallbackAsync` hand on a transaction the wallet signed
    itself, and `SubmitToCallbackAsync` refuses the same callback URLs a parsed request cannot carry
    (whitespace, invisible characters, user info, a host with no valid IDN form, a fragment, a malformed
    percent escape, a backslash or a `.`/`..` path segment); a callback's non-2xx answer or a
    transaction Horizon rejects is returned, with `Sep7SubmitResult.IsSuccess`. With the SDK's own HTTP
    client the callback POST follows no redirects (a 3xx is returned as the callback's answer) and the
    stellar.toml fetch follows up to 5, https only, each to a fully qualified domain name (not under
    `localhost`) on the default port without user info; a caller-supplied client that follows redirects
    itself is detected on the callback path after the fact.
  - Typed exceptions under `StellarDotnetSdk.Sep.Sep0007.Exceptions` (base `Sep7Exception`), and a new
    `SEP-0007_COMPATIBILITY_MATRIX.md` (100%, 31/31 fields).
- **SEP-12 KYC API client** (`StellarDotnetSdk.Sep.Sep0012`, SEP-12 v1.15.0): `KycService` covers every
  endpoint — `GET`/`PUT /customer`, `PUT /customer/verification` (deprecated, marked `[Obsolete]`),
  `PUT /customer/callback`, `DELETE /customer/{account}`, `POST`/`GET /customer/files` — and
  `FromDomainAsync` discovers `KYC_SERVER` from stellar.toml, falling back to `TRANSFER_SERVER`, and raises
  `KycServiceException` for a declared server that is not an absolute `https` URL. Requests take a SEP-10 or
  SEP-45 JWT and must go to an `https` server, and a callback URL registered with `PUT /customer/callback`
  must be `https` too, which SEP-12 itself does not require (plain `http` only for `localhost` or a
  loopback IP in its standard form, in both cases).
  `PUT /customer` sends SEP-9 fields (via the existing `Sep0009` types), custom fields and files as
  `multipart/form-data` with every binary part last, plus `*_verification` codes and `*_file_id`
  references; the client-side rules of SEP-12 (a `type` with every `transaction_id`, no memo for a `C...`
  account) are checked before sending, and so is a name used by both a text field and a file. Customer
  statuses, provided-field statuses and field types are typed enums matched against the exact SEP-12
  literals — on the response properties and on the enum types themselves — so an unknown value or a bare
  ordinal fails the parse instead of reading as `ACCEPTED`.
  Response bodies are capped at 1 MiB, the whole exchange (body included) is bounded by the client's
  timeout, duplicate JSON properties are rejected in success and error bodies alike (the hardening issue
  [#205](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/205) asks for elsewhere), and error
  statuses map to `AuthenticationRequiredException`, `CustomerNotFoundException`,
  `PayloadTooLargeException` or `KycServiceException`, carrying the anchor's `error` text (and, on a plain
  `KycServiceException`, any `Retry-After` delay as `RetryAfterDelay`); an invalid success body, including
  one that is not valid UTF-8, raises `InvalidKycResponseException`, and server text quoted in any exception
  message is clamped and stripped of control and format characters. The internal client does not follow
  redirects; with a caller's client, a response from another origin, or to a request a redirect resent with
  another method (`POST`, `PUT` or `DELETE` turned into `GET`), is rejected. File names are percent-encoded
  per RFC 7578, and a request's `ToString()` redacts the JWT, SEP-9 and custom field values, file contents
  and names, verification codes, file references and callback-URL secrets, but prints identifiers (customer
  ID, account, memo, memo type, type, transaction ID, language, file ID, content type) with control and
  format characters replaced. The SEP-12 converters are public and attached to the response properties, so a
  consumer's source-generated `JsonSerializerContext` can use the response types with the same checks, including
  the rejection of a `null` entry in `fields` or `provided_fields`. `KycCallbackSignature` verifies the
  `Signature`/`X-Stellar-Signature` header on anchor status callbacks (Ed25519 over
  `<timestamp>.<host>.<body>`, with a freshness window, over the body as a string or raw bytes), and
  `GetSignedHost` derives the host string the anchor signs (`host:port` when the callback URL names a port);
  `GetCustomerInfoResponse.FromJson` parses the callback payload. Compatibility matrix:
  `StellarDotnetSdk/Compatibility/sep/SEP-0012_COMPATIBILITY_MATRIX.md` (100%, 90/90 fields).
- **SEP-38 (Anchor RFQ API) client**, new namespace `StellarDotnetSdk.Sep.Sep0038` (SEP-38 v2.5.0):
  - `QuoteService` covers `GET /info`, `GET /prices`, `GET /price`, `POST /quote` and `GET /quote/:id`,
    and `QuoteService.FromDomainAsync` discovers `ANCHOR_QUOTE_SERVER` from stellar.toml. The JWT is
    optional for the first three endpoints and required for the two quote endpoints; it is the token
    from the existing SEP-10 (`ClientWebAuth.JwtTokenAsync`) or SEP-45
    (`ClientWebAuthContract.JwtTokenAsync`) flow. The quote server address must be https (the constructor
    allows plain http on loopback for local development; `FromDomainAsync` never does) and carry no query,
    fragment or user information.
  - `PricesRequest`, `PriceRequest` and `QuoteRequest` (with `QuoteContext`: `Sep6`/`Sep24`/`Sep31`)
    enforce the spec's request rules before sending, failing the returned task with `ArgumentException`:
    exactly one of `SellAmount`/`BuyAmount`, one `GET /prices` side (sell or buy, the v2.3.0 buy side
    included), at most one delivery method on `POST /quote`, and only `sep6`/`sep31` for `GET /price`.
    A zero or negative amount fails the same way (`ArgumentOutOfRangeException`), and so does a string
    property set to an empty or whitespace value instead of `null`, or a JWT that is not printable
    ASCII without whitespace (`ArgumentException`). Their `ToString` redacts the JWT. Custom headers with
    an invalid name, a `Content-Length` or `Transfer-Encoding` name, or a value outside printable ASCII
    are rejected when the service is created, which takes a copy of them. A custom `Content-Type` or
    `Accept` header is ignored: requests to the quote server ask for, and send, `application/json`.
  - The response converters `ExactDecimalJsonConverter`, `UtcDateTimeOffsetJsonConverter` and
    `NonNullElementListJsonConverter<T>` (namespace `StellarDotnetSdk.Converters`) are public, so a
    consumer's source-generated `JsonSerializerContext` over the SEP-38 response types reads amounts and
    timestamps the same way the SDK does.
  - `AssetIdentifier` builds, parses and validates the Asset Identification Format (`stellar:CODE:ISSUER`,
    `stellar:native`, `iso4217:USD`), converts to and from the SDK `Asset` types, and compares by value
    (`Equals`, `==`, `!=`). The issuer must be in its canonical upper-case form; `FromAsset` converts it.
  - Amounts and prices are `decimal`. Requests send them as invariant-culture strings with their scale
    kept. Responses read them exactly, in plain or exponent form (`"1E-7"`): a value that `decimal` cannot
    hold without rounding is rejected rather than approximated. An `expires_at` without an offset is read
    as UTC, as the spec defines, not as local time.
  - Responses go through the hardened `JsonOptions` (duplicate properties rejected,
    [#205](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/205)), required fields are enforced,
    null list elements are rejected, a `GET /prices` answer must carry the list for the requested side,
    and `GET /quote/:id` must return the requested quote (its `total_price` is optional, as that
    endpoint's response table omits it). A `POST /quote` answer must carry a usable id, the requested
    asset pair, no delivery method the request did not name, and an expiry no earlier than
    `expire_after` (sent in UTC, rounded up to the whole second); prices and amounts must be greater
    than zero, and a `GET /prices` `decimals` count must not be negative. Bodies are capped at 1 MiB
    and streamed so the cap bounds memory (an error body over the
    cap is dropped, and the status still selects the exception), a leading UTF-8 byte order mark is skipped, and the whole exchange, body included, stays
    within `HttpClient.Timeout` (and `RequestTimeout` for the internal client). The internal client does
    not follow redirects, and a response whose final location, after a caller-owned client followed
    redirects, is another origin is rejected. Errors map to `BadRequestException` (400), `PermissionDeniedException` (403),
    `QuoteServerNotFoundException` (404), `UnexpectedResponseException` and
    `NoAnchorQuoteServerFoundException`, all derived from `QuoteServerException`; an
    `UnexpectedResponseException` for an error status carries the response's `Retry-After` as
    `RetryAfterDelay`. Transport failures surface as `HttpRequestException`, timeouts as
    `TaskCanceledException`.
  - `POST /quote` is not idempotent: do not configure `QuoteService` with the `ForHorizon()` or
    `ForSoroban()` presets, which retry `POST` on a 408, 429, 500, 502, 503 or 504 answer; the README
    and HTTP-retry guide now say so.
  - Compatibility matrix `StellarDotnetSdk/Compatibility/sep/SEP-0038_COMPATIBILITY_MATRIX.md`: 100%
    (75/75 fields), with the spec's prose rules listed separately.

### Changed

- **Breaking (behavioral):** CAP-71 `SOROBAN_CREDENTIALS_ADDRESS_V2` is now the default Soroban address
  credential ([#206](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/206), part of
  [#207](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/207)). `StellarRpcServer.SimulateTransaction`
  sends `useUpgradedAuth: true` unless told otherwise, so a recording-mode simulation now returns
  `SorobanAddressCredentialsV2` entries where it used to return legacy `SorobanAddressCredentials`. This takes
  up the client-default flip that SDF's CAP-71 transition plan schedules for the JS SDK at protocol 28 and
  invites other SDKs to adopt, and matches the JS and Java SDKs. It is a policy change for ecosystem
  alignment, not a correctness fix: legacy credentials remain valid on protocol 28 (CAP-71 does not deprecate
  them), and both opt-outs below keep them reachable.
  - The parameter's declared default stays `null`, meaning "the SDK's current default", which the SDK resolves
    to `true` when it builds the request rather than at the call site. C# compiles a declared default into every
    call site, so this lets a later change to the default — such as no longer sending the field once Stellar RPC
    retires it — reach callers that leave the argument unset without a recompile. It also lets a caller pass an
    optional setting of its own straight through. Code compiled against any earlier release, including
    16.0.0-beta, has to recompile anyway (see the `useUpgradedAuth` entry under **Added**) and picks up the new
    default when it does.
  - Signing needs no change. `SorobanAuthorization.AuthorizeEntry` keeps its
    `SorobanCredentialsVersion.Preserve` default because it signs an entry whose variant simulation already
    chose: a v2 simulation yields a v2 signature over the address-bound
    `ENVELOPE_TYPE_SOROBAN_AUTHORIZATION_WITH_ADDRESS` preimage, and a legacy simulation still yields a v1 one.
    Because `Preserve` signs whatever variant came back, an RPC server that ignores the flag (one older than
    v27.1.0) still yields a legacy signature; on a network at protocol 27 or later, pass
    `SorobanCredentialsVersion.V2` to `AuthorizeEntry` to require the address-bound one.
    `AuthorizeEntryWithDelegates` and `BuildWithDelegatesEntry` take no version and always emit `WITH_DELEGATES`
    credentials, whose root and delegate signatures all cover the address-bound payload
    (`BuildWithDelegatesEntry` emits them unsigned, for later signing with `AuthorizeEntry`). This SDK has no
    helper that builds an address credential from scratch (the JS and Java `authorizeInvocation`), so there is
    no other default to flip.
  - Migration: code that inspects credentials by hand must accept `SorobanAddressCredentialsV2` as well as
    `SorobanAddressCredentials` (or match on their common base, `SorobanAddressCredentialsBase`). The two are
    sibling types, not base and subclass, so an `is`/`as SorobanAddressCredentials` check silently stops
    matching v2 entries instead of failing, and only a direct cast throws `InvalidCastException`. A hand-rolled
    signer that always builds the legacy `ENVELOPE_TYPE_SOROBAN_AUTHORIZATION` preimage now produces signatures
    the network rejects for v2 entries; build the payload with
    `SorobanAuthorization.BuildAuthorizationEntryPreimageHash`, which picks the preimage from the entry. V2
    entries need a network at protocol 27 or later; RPC ignores the flag on older networks, so the default still
    yields v1 there.
  - Opt-outs, for co-signers or verifiers that cannot yet produce or check the address-bound signature: pass
    `useUpgradedAuth: false` to `SimulateTransaction` to keep receiving legacy entries, or pass
    `SorobanCredentialsVersion.V1` to `AuthorizeEntry` to sign an entry as legacy regardless of what simulation
    returned. Either way the signature gives up address binding: it can be replayed against another account that
    shares the same signing key when the invocation does not itself bind the signer's address. Explicit `false`
    is put on the wire rather than omitted (the JS and Java SDKs likewise send it explicitly), but Stellar RPC
    treats an absent field as `false`, so the explicit value does not protect the opt-out against a change of the
    server-side default.
  - The simulation opt-out is transitional: under SDF's tentative plan, RPC flips its server-side default to v2
    (planned for protocol 29), at which point `useUpgradedAuth: false` becomes a no-op and stops returning
    legacy credentials, and later disables the flag altogether (planned for protocol 30). Stellar Testnet was
    already on protocol 29 with RPC 29.0.0 in September 2026 and still defaulted to v1, so the protocol numbers
    are not firm.
- **Breaking (behavioral):** Protocol 28 XDR ([#207](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/207), [#252](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/252)).
  `SCValType`, `ContractExecutableType` and `StellarValueType` gain members (`SCV_EXECUTABLE_TAG`,
  `CONTRACT_EXECUTABLE_EXTERNAL_REF`, `STELLAR_VALUE_EMPTY_TX_SET`), so values that used to fail to decode with
  `InvalidDataException` now decode, and code that switches over these enums or over `SCVal` /
  `ContractExecutable` subclasses meets cases it has not seen. The generated `StellarDotnetSdk.Xdr` string
  typedefs' `InnerValue` is now a view over `InnerBytes`: unchanged for valid UTF-8, but a decoded non-UTF-8
  string now re-encodes to its original bytes instead of to the U+FFFD replacement. Because the text is now
  stored as UTF-8, a string with an unpaired surrogate no longer reads back as set: its wire bytes are unchanged
  (U+FFFD), but `InnerValue` now returns U+FFFD in its place, and each read decodes a new string. The bounded
  string typedefs (`SCSymbol` and `String32` at 32 bytes, `String64` at 64) now also enforce their declared
  maximum length, as every bounded opaque and array already did: encoding a longer value throws
  `ArgumentException` instead of producing XDR the network rejects, and decoding a longer length prefix throws
  `InvalidDataException`. The SDK-layer checks count characters (`ManageDataOperation` names, `SetOptions` home
  domains) or nothing (`Soroban.SCSymbol`), so a multi-byte value that passed them now fails when it is encoded.
  Source and binary compatible.
- **Breaking:** `SubmitTransactionAsyncResponse.TxStatus` deserialization is now strict. The nested
  `TransactionStatus` enum was bound by the catch-all `JsonStringEnumConverter`, which maps bare
  integers by ordinal and matches case-insensitively — so a malformed Horizon `POST /transactions_async`
  response of `"tx_status": 0` deserialized to `PENDING` — the most optimistic of the four statuses.
  (Two of the four are not observable through `Server.SubmitTransactionAsync` today: Horizon answers a
  duplicate with HTTP 409 and an unavailable core with 503, and `HandleResponse` deserializes a body
  only for 200/201/400, so `DUPLICATE` and `TRY_AGAIN_LATER` surface as
  `SubmitTransactionUnknownResponseException` and `ServiceUnavailableException` instead. The strict
  converter still governs `PENDING` and `ERROR` there, and all four wherever a caller deserializes such
  a body themselves.) `"tx_status": 99` produced the
  undefined enum value `99`, which matches none of the four members and so silently fails every
  comparison a caller writes, and `"tx_status": "pending"` was accepted although Horizon — passing the
  status through verbatim from stellar-core — emits only the four uppercase literals `PENDING`,
  `DUPLICATE`, `TRY_AGAIN_LATER`, `ERROR`. The new public `SubmitTransactionAsyncStatusJsonConverter`
  accepts exactly those literals and rejects everything else with `JsonException`, on write as well as
  read (serializing an undefined cast such as `(TransactionStatus)99` now throws instead of emitting a
  bare number). It is registered on `JsonOptions.DefaultOptions` ahead of the catch-all and also pinned
  on the property with a property-level `[JsonConverter]`, so the strict wire format holds whichever
  options instance the response is deserialized with. The pin applies on write too: serializing a
  response with the caller's own options, even a plain `new JsonSerializerOptions()`, now emits
  `"tx_status":"PENDING"` where 15.1.0 emitted the number `0`, so a response persisted that way can no
  longer be read back as a number. That pin governs the *value* grammar only: the
  duplicate-property rejection that catches a repeated `tx_status` is `JsonOptions.DefaultOptions`'
  `AllowDuplicateProperties` setting and does not travel with the type, so a caller deserializing this body
  with their own options still gets last-wins semantics. The four valid literals are unaffected
  ([#226](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/226)).
- **Breaking:** `SubmitTransactionAsyncResponse.TransactionStatus`,
  `SendTransactionResponse.SendTransactionStatus` and `TransactionInfo.TransactionStatus` now carry a
  type-level `[JsonConverter]` as well, matching `EventFilterType`. The property-level pin above only
  covers an enum reached *through* its response object; the enum travelling on its own — a caller's own
  DTO field, a persisted status column, a queue message — resolved through whatever the caller's
  `JsonSerializerOptions` provided, and a bare options instance maps a bare integer by ordinal. So
  `JsonSerializer.Deserialize<TransactionStatus>("0", new JsonSerializerOptions())` returned `PENDING`,
  and the `TransactionInfo` equivalent read `1` as `SUCCESS` — a corrupted stored record presenting
  itself as a confirmed transaction. All three now throw `JsonException` for anything but the documented
  literals under an options instance that registers no converter for the enum. Note the limit of this
  tier: System.Text.Json resolves a property-level attribute first, then the options' `Converters`
  collection, then the type-level attribute — so a caller whose own options register
  `JsonStringEnumConverter` still shadows it, and the bare enum still reads `0` as `PENDING` and `1` as
  `SUCCESS` there. Only the response *properties* (pinned with property-level `[JsonConverter]`) are
  strict under every options instance; a persisted or queued bare enum is not, and should be stored as
  the literal. This also changes the **write** direction: under a bare options instance
  `JsonSerializer.Serialize(SendTransactionStatus.PENDING, new JsonSerializerOptions())` emitted `0` and
  now emits `"PENDING"`, so a value persisted as a number by an earlier release no longer round-trips
  through the same call. Code that round-tripped one of these enums as a bare integer must
  store the literal instead. All four type-level converters — including `EventFilterTypeJsonConverter`,
  whose type-level attribute predates this change — now also implement `ReadAsPropertyName` and
  `WriteAsPropertyName`, so these enums keep working as `Dictionary` keys; a type-level converter without
  those overloads makes `Dictionary<TStatus, T>` throw `NotSupportedException`, which is not a
  `JsonException` and so escapes a caller's `catch (JsonException)` entirely. For the three status enums,
  keys change the same way as values: the overloads always write the literal, ignoring the caller's
  `DictionaryKeyPolicy`, and under options that register no converter for the enum they reject keys the
  built-in handling read back — a lowercase or mixed-case literal, or an ordinal such as `"0"` — so a
  dictionary stored under those keys no longer loads. Unlike a runtime
  `JsonStringEnumConverter`, a source-generated context with `UseStringEnumConverter = true` does not
  shadow the type attribute: there `0` read as `PENDING` before and now throws
  ([#230](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/230)).
- **Breaking:** `SimulateTransactionResponse.StateChanges`, `.Results`, `.Events` and
  `.Results[i].Auth` reject a `null`
  array *element* with `JsonException` instead of admitting it. `RespectNullableAnnotations` constrains
  the array reference, never its contents, so `"stateChanges":[null]` produced an array holding `null`
  despite the non-nullable element type — and `foreach (var c in StateChanges) if (c.Type == "created")`,
  the very loop the `[JsonRequired]` on `LedgerEntryChange.Type` was added to protect, then threw
  `NullReferenceException` at the caller instead of failing at deserialization like every other
  malformed-payload path. The same hole was open on the two sibling arrays: `"results":[null]` threw
  `NullReferenceException` on the first `Results[0].Xdr`, and — more quietly — made
  `SorobanAuthorization` answer `null`, which reads as *no authorization required*, so the documented
  assemble-and-submit flow guarded on that null and would submit the transaction with no auth entries.
  `"events":[null]` behaved the same way on first use. Note that closing this spelling does not make a
  `null` `SorobanAuthorization` trustworthy on its own: `{}`, `"results":[]`, `"results":[{}]` and
  `"results":[{"auth":null}]` all still answer `null`, and so does a simulation that genuinely needs no
  authorization. What changed is that a *malformed* payload can no longer produce that answer; telling
  "no auth required" apart from "no auth parsed" still means checking `Results` yourself.
  Assigning a null element from C# rather than
  reading it from JSON throws `ArgumentException`, not `JsonException`: that is a rejected argument, and
  raising a serialization exception from an object initializer would put it outside any `catch` a caller
  would reasonably write there. The check runs at assignment and the array is not copied, so it does not
  prevent a later write into an array the caller still holds — but serializing such an array now throws
  `JsonException` rather than silently emitting `null`, so the two directions agree. The guards are public
  converters --
  `StateChangesArrayJsonConverter`, `SimulationResultsArrayJsonConverter`,
  `SimulationEventsArrayJsonConverter` and `SorobanAuthArrayJsonConverter`, over the shared base
  `NonNullElementArrayJsonConverter<T>` -- because the System.Text.Json source generator can only construct
  a converter that is public with a public parameterless constructor. It does not fail loudly otherwise: it
  emits `SYSLIB1220` and silently drops the converter, and the property's `ArgumentException` guard would then
  surface out of `JsonSerializer.Deserialize` instead of `JsonException`, inverting the contract for exactly
  those consumers. Do not register these on `JsonSerializerOptions.Converters`: they read and write the array
  by delegating to `JsonSerializer`, so a globally registered instance resolves back to itself and recurses.
  That used to terminate the process with an uncatchable `StackOverflowException`; both directions now detect
  the registration and throw `InvalidOperationException` instead. Because these converters are necessarily
  public and look exactly like the ones this SDK *does* register globally in `JsonOptions`, the mistake was
  worth converting into an ordinary exception rather than leaving to a comment.
  **Diagnostic cost:** `JsonException.Path` is less precise for *every* failure inside these four arrays, not
  just a null element. A type mismatch inside `stateChanges` used to report `$.stateChanges[1].type`; it now
  reports `$.stateChanges`, and a failure inside `results[i].auth` reports only `$.results`. Reading the array
  delegates to a fresh serializer session that restarts the path at the array, so the converter rethrows with
  `Path` unset and System.Text.Json fills in the outer path, which stops at the array. Computing a deeper path
  isn't possible, because a converter knows its own field name but not its ancestors, so a computed path would
  be wrong for the nested `auth` array. This trade is deliberate: a converter is the only place that can raise
  `JsonException` on the wire while the `init` accessor raises `ArgumentException` on assignment, and that
  distinction was ranked above path precision. `Path` is always a true prefix of where the value lives, but the
  message recovers the rest only in part. A type mismatch keeps its array-relative locator (`$[1].type`) and a
  null element names its index. A missing required field names the field but not the element: an entry
  without `type` gives the same message at any index. For a failure inside `auth`, the message carries the
  index within `auth` but not which `results` entry holds it; only `InnerException.Path` does
  ([#229](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/229)).
- `SimulateTransactionResponse.MinResourceFee` and `.RestorePreamble.MinResourceFee` now carry
  `[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]`. Stellar RPC tags the two fields
  `json:"minResourceFee,string,omitempty"` and `json:"minResourceFee,string"` respectively, so when present the
  value arrives as a JSON *string*; the SDK read it only because `JsonOptions.DefaultOptions` sets that option
  globally. These are public response types with public `init` accessors, so a caller whose own
  `JsonSerializerOptions` bind the wire name but not the number handling — `PropertyNameCaseInsensitive`, or a
  camelCase naming policy — got a `JsonException` on a conforming reply. Options that already imply the
  allowance, such as `JsonSerializerDefaults.Web`, were never affected, and this is independent of the
  `required` added to the preamble's field in this same release, which governs an *absent* field rather than a
  string-shaped one. The attribute relaxes the read — a bare number still reads — so no payload that
  deserialized before is rejected. Note that a property-level number-handling attribute replaces the ambient
  setting in *both* directions: these two properties are now always written as bare numbers even under
  `NumberHandling = WriteAsString`, and can no longer be opted into `Strict` reads. Adding `WriteAsString` here
  instead would change the serialized shape for every caller rather than for the few who set that flag
  ([#229](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/229)).
- The assemble-and-submit example in `Examples/Soroban`, `docs/tutorials/examples/horizon/transactions.md` and
  `docs/tutorials/examples/soroban/smart-contracts.md` now probes for authorization entries on
  `Results[0].Auth` rather than on `SorobanAuthorization`, which is the guard that property's own
  documentation calls out as unsafe: reading it decodes every entry's base64 XDR, so on a malformed blob it
  throws `InvalidDataException` instead of answering `null`. Going through `Results` keeps the decode to the
  one place the value is used, so a caller can wrap that line to handle a malformed entry; `Results[0]` is
  null-checked, because the array is not copied on assignment and `SorobanAuthorization`'s own getter keeps the
  same check. This corrects the published guidance and is behaviour-preserving for every payload shape,
  including `"auth": []`. It still cannot tell "no authorization required" apart from "the reply carried no
  results" — `SorobanAuthorization` answers `null` for both, and an empty `results` is legitimate when
  simulating an operation other than `InvokeHostFunction`
  ([#229](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/229)).
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
- **Breaking:** `GetEventsRequest.EventFilter.Type` is now the new `[Flags]` enum `EventFilterType`
  (`None`/`System`/`Contract`) instead of a free `string`. Stellar RPC accepts only the literals
  `system` and `contract` in this field, joined by a bare comma for a set, and validates them
  case-sensitively without trimming — so `"diagnostic"`, `"System"`, and `"system, contract"` were all
  accepted by the SDK and rejected by the server as `filter type invalid`. A value carrying undefined
  flags (e.g. `(EventFilterType)99`) now throws `ArgumentOutOfRangeException` at the assignment rather
  than as a round-trip error. `diagnostic` is deliberately not offered: Protocol 23 removed diagnostic
  events from the `getEvents` stream, and RPC v23.0.0 onwards rejects a filter naming it, so no server
  this SDK supports accepts it. Migration: `Type = "contract"` becomes
  `Type = EventFilterType.Contract`, and `Type = "system,contract"` becomes
  `Type = EventFilterType.System | EventFilterType.Contract`. Leaving `Type` null is unchanged and
  still means "all event types". The wire value is produced by the new public
  `EventFilterTypeJsonConverter`, which is registered on `JsonOptions.DefaultOptions` ahead of the
  catch-all `JsonStringEnumConverter` and also applied directly to the property — System.Text.Json
  resolves a property-level `[JsonConverter]` first, then the options' `Converters` collection (first
  match wins), then a type-level `[JsonConverter]`, so the type-level attribute alone would have been
  outranked and would have emitted the unusable `"System, Contract"`
  ([#211](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/211)).
- **Breaking:** `SendTransactionStatusEnumJsonConverter` is now actually reachable. It was
  registered on `JsonOptions.DefaultOptions` *after* the catch-all `JsonStringEnumConverter`, and
  System.Text.Json returns the first converter in the collection whose `CanConvert` matches — so the
  standard converter, which matches every enum, shadowed it and the hand-written one never ran (its
  unit tests built their own options containing only it, so they passed regardless). Reordering it
  ahead of the catch-all tightens `SendTransactionResponse.Status` deserialization to the four
  literals Stellar RPC actually emits: a bare integer is now rejected instead of being mapped by
  ordinal — `"status": 0` previously deserialized to `PENDING`, so a malformed or adversarial
  `sendTransaction` response could present a failed submission as pending — and matching is
  case-sensitive, so `"pending"` is rejected where it was previously accepted. Non-string tokens now
  report the SDK's own `JsonException` message rather than a wrapped `InvalidOperationException` from
  the reader. The four valid literals (`PENDING`, `TRY_AGAIN_LATER`, `DUPLICATE`, `ERROR`) are
  unaffected ([#211](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/211)).
- **Breaking:** `getTransaction`/`getTransactions` statuses are now parsed by the new public
  `TransactionStatusJsonConverter` instead of the catch-all `JsonStringEnumConverter`, which is
  case-insensitive and maps bare integers by ordinal — so `"status": 1` deserialized to `SUCCESS`, on
  the endpoint callers poll to decide whether a payment settled, and `"status": 7` produced an
  undefined enum value that silently matches none of the three members. Only the exact literals
  `NOT_FOUND`, `SUCCESS` and `FAILED` are accepted now; `"success"` and any numeric form are rejected,
  and serializing an undefined cast (`(TransactionStatus)99`) throws a `JsonException` instead of
  emitting a bare number — the same guard `SendTransactionStatusEnumJsonConverter.Write` carries. Like
  `EventFilterType`, the converter is both registered on `JsonOptions.DefaultOptions` ahead of the
  catch-all and applied to `TransactionInfo.Status` with a property-level `[JsonConverter]` — which
  System.Text.Json resolves ahead of any options' converter collection, so the strictness also holds
  for consumers who deserialize these response types with their own `JsonSerializerOptions`
  (`SendTransactionResponse.Status` gained the same property-level pin). Deliberate fail-closed
  trade-off: a `getTransactions` page is deserialized as one document, so a single entry carrying a
  malformed status now fails the whole response instead of silently reporting a wrong settlement
  status.
- **Breaking:** `TransactionInfo.Status` and `SendTransactionResponse.Hash` are now
  `[JsonRequired]`, the same guard `SendTransactionResponse.Status` already carries.
  `RespectNullableAnnotations` rejects an explicit `null` but cannot reject an *absent* property, and
  an enum is a value type besides — so a `getTransaction` response carrying no `status` deserialized
  to the zero member, `NOT_FOUND`, and an absent `hash` left a non-nullable `string` holding `null`.
  Stellar RPC tags both fields without `omitempty`, so requiring them rejects nothing a conforming
  server sends. The check enforces presence, not validity (an empty `hash` is not rejected), and runs
  before any field is readable — a non-conforming reply omitting `hash` surfaces as a `JsonException`
  rather than a readable `ERROR` status.

- `SendTransactionStatusEnumJsonConverter.Write` rejects an undefined enum value with `JsonException`
  instead of writing the bare number as a string. Serializing
  `(SendTransactionStatus)99` produced `"99"`, which the converter's own `Read` then rejected, so the
  type did not round-trip; the sibling `EventFilterTypeJsonConverter` already refused the equivalent
  input ([#211](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/211)).

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
- Exception messages from `SendTransactionStatusEnumJsonConverter`, `TransactionStatusJsonConverter` and
  `EventFilterTypeJsonConverter` — including a rejected dictionary key, which `TransactionStatusJsonConverter`
  reads through its own property-name path — no longer copy the whole rejected value. All three still name the
  offending literal so a wire-format mismatch stays diagnosable, but the value is now clamped to 64 UTF-16
  code units (with its true length appended) and escaped. Previously the message grew with the payload — a
  2 MB `status` produced a 2,000,059-character `JsonException` message — and a `\r\n` in the value forged a
  line in whatever log the caller wrote it to. For the two status converters the value is server-supplied,
  so it is attacker-controlled whenever the caller does not operate the RPC endpoint; `EventFilterType` is
  request-side only and is never bound to a server response, so there the value is whatever JSON the caller
  chose to deserialize. A conforming value is unaffected: every field this applies to carries an ASCII wire
  literal by contract (`PENDING`, `NOT_FOUND`, `system,contract`). Escaping is a whitelist — printable
  ASCII survives, everything else becomes `\uXXXX`, and an astral character becomes a single
  `\UXXXXXXXX` rather than its two surrogate halves. A blacklist of "dangerous" categories cannot be
  complete: `char.IsControl` is `Cc` only, and adding `Zl`/`Zp` (U+2028/U+2029, line terminators to
  .NET's own `ReplaceLineEndings` and to JavaScript) and `Cf` (U+202E RIGHT-TO-LEFT OVERRIDE, which
  visually reverses the rest of the line without needing an ANSI escape) still leaves `Cn` (U+2065 and
  the reserved default-ignorable range, drawn as nothing), `Mn` (combining marks), `Zs` (U+00A0, which
  forges alignment in fixed-width output) and `Co` — plus a version-skew hole, since a code point that is
  `Cf` in a newer Unicode than the running framework's tables is `Cn` today and would pass. Three ASCII
  characters are also escaped: the apostrophe that delimits the quoted fragment, the quotation mark that
  would end the string in a JSON or CSV log the message is written into, and the backslash that
  introduces every escape emitted here — without the last the encoding is ambiguous, because the six
  characters `\u202e` arriving literally on the wire would render identically to a real U+202E, which
  both destroys the diagnostic value being traded for and lets any downstream that unescapes `\uXXXX`
  re-materialise the character the escaping removed. The clamp does not cut between the halves of a
  surrogate pair, so a truncated astral character is dropped whole rather than reported as a bare
  surrogate code unit.

### Removed

- **Breaking:** `SorobanSourceAccountCredentials.ToSorobanCredentialsXdr()` and
  `SorobanAddressCredentials.ToSorobanCredentialsXdr()`. Use `ToXdr()` instead (it now produces the same
  XDR via the `abstract`/`override` pair).

### Fixed

- A `getLedgerEntries` response whose `entries` array holds a `null` element now fails to deserialize with
  `JsonException` (via the new `LedgerEntryResultsArrayJsonConverter`), as the `simulateTransaction` arrays do.
  Before, `GetLedgerEntries` returned a response whose `LedgerEntries` and `LedgerKeys` getters threw
  `NullReferenceException`, and `GetAccount` threw it directly.
- `StellarRpcServer` now surfaces JSON-RPC error responses instead of discarding them
  ([#197](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/197)). Stellar RPC reports request-scoped
  failures — an out-of-range `startLedger`, a TTL ledger key queried directly, malformed parameters — as a
  JSON-RPC 2.0 error object (`code`, `message`, optional `data`) delivered with HTTP status 200 and no
  `result` member. `SorobanRpcResponse<T>` had no `Error` member, so the payload was dropped during
  deserialization and the call returned `null`, typically surfacing at the call site as an unrelated
  `NullReferenceException` with the server's own explanation unrecoverable.
  - **Breaking:** every `StellarRpcServer` method — they all route through one `SendRequest` helper — now
    throws the new `SorobanRpcException` where it previously returned `null`. The exception carries `Code`,
    `ErrorMessage` (the server's message verbatim; `Message` wraps it together with the code), and
    `ErrorData`, the optional JSON-RPC `data` member preserved verbatim as a `JsonElement?`. Code that
    null-checked the return value must catch `SorobanRpcException` instead.
  - `SorobanRpcResponse<T>` gains a nullable `Error` property of the new type `SorobanRpcErrorResponse`
    (`Code`, `Message`, `Data`), for callers that deserialize RPC envelopes themselves, and its `Result`
    is now annotated `T?` — a JSON-RPC error response carries no result, so the old non-nullable
    annotation was a promise the type could not keep.
  - An envelope with no usable result no longer returns `null` either: JSON-RPC 2.0 §5 requires exactly
    one of `result`/`error`, and a response that omits `result` — or carries it as an explicit `null` —
    now throws `ClientProtocolException`, the same type `ResponseHandler` already raises for an empty
    body, so no `StellarRpcServer` method can hand back a `null` its signature says is non-nullable.
  - HTTP-status-level failures are unchanged: 429, 503, and other error statuses still throw
    `TooManyRequestsException`, `ServiceUnavailableException`, and `HttpResponseException`. A JSON-RPC
    error is invisible to the resilience pipeline — the status is 200 — so it is never retried.
  - Every `StellarRpcServer` method now documents the full set of exceptions it can raise, not just the
    two JSON-RPC-level ones: `ServiceUnavailableException` (503), `TooManyRequestsException` (429),
    `HttpResponseException` (any other status of 300 or above) and `JsonException` (a body that is not
    valid JSON, or does not match the expected schema) were all reachable but undocumented.
    `GetAccount` additionally documents `AccountNotFoundException` and the `ArgumentException` raised for
    a malformed account id. The `SorobanRpcException` entry also records that only an error delivered
    with HTTP status 200 surfaces that way — one carried by an HTTP failure status is reported by that
    status's exception, and the JSON-RPC error object is not preserved.
- `StellarRpcServer` no longer converts two malformed Stellar RPC responses into exceptions that hide what
  actually went wrong ([#210](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/210)). Both are
  companions to the JSON-RPC error handling above and sit in the same `SendRequest` helper, so every
  method is affected.
  - **Breaking:** a body holding the JSON literal `null` deserializes to no response object at all, and
    was then dereferenced — reaching the caller as a `NullReferenceException`. It now throws
    `ClientProtocolException`, the same exception an empty body already produced. The guard sits in
    `ResponseHandler.HandleResponse` rather than in `SendRequest`, because the same hazard applied to
    every caller of that helper: its return type is non-nullable, so it was handing a `null` through a
    signature that says otherwise on the Horizon and SEP paths too (`Server.RootAsync`, every request
    builder, `Link.Follow`, `FederationServer.ResolveAddress`, and the nine `TransferServerService`
    endpoint methods). Those methods now throw where they previously returned `null`, and each one now
    documents it. `FederationServer.ResolveAddress` is the exception to the type: it wraps every failure
    other than an HTTP error status, so the malformed body reaches the caller as its own
    `ConnectionErrorException` rather than as `ClientProtocolException`.
  - **Breaking:** `SorobanRpcResponse<T>.Id` is now `string?`. JSON-RPC 2.0 §5 requires a null `id` in
    exactly one case — the server could not read the request's `id` at all, a parse error or an invalid
    request — and such a response carries the `error` explaining why. The non-nullable annotation made
    `RespectNullableAnnotations` reject it while deserializing, so the caller got a `JsonException` naming
    an SDK annotation rather than the server's own message; it now surfaces as a `SorobanRpcException`
    like any other JSON-RPC error. `JsonRpc` stays non-nullable — the specification never permits a null
    there.
- `EventFilterTypeJsonConverter.Write` now throws `JsonException` rather than
  `ArgumentOutOfRangeException` for a value carrying undefined flag bits, matching the SDK's other
  hand-written strict enum converters — `SubmitTransactionAsyncStatusJsonConverter`,
  `SendTransactionStatusEnumJsonConverter`, `TransactionStatusJsonConverter` and
  `LiquidityPoolTypeEnumJsonConverter` — so one `catch (JsonException)` around a
  `JsonSerializer.Serialize` that uses `JsonOptions.DefaultOptions` covers all five. Both halves of that
  qualifier matter. It does not extend to every converter in `JsonOptions.DefaultOptions`: the catch-all
  `JsonStringEnumConverter` registered last handles every enum without a dedicated converter and writes an
  undefined value as its bare number without throwing at all. And it is specific to those options: of the
  five, four enums — `EventFilterType`, `SubmitTransactionAsyncResponse.TransactionStatus`,
  `SendTransactionResponse.SendTransactionStatus` and `TransactionInfo.TransactionStatus` — carry a
  type-level `[JsonConverter]` and so stay strict under a bare `JsonSerializerOptions`; only
  `LiquidityPoolTypeEnum` does not, and on its own falls through to the catch-all (its response properties
  carry their own pins). Assigning an undefined value to `GetEventsRequest.EventFilter.Type` still throws
  `ArgumentOutOfRangeException` — that is a rejected *argument*, raised at assignment, and is unchanged.
  This entry is deliberately not marked breaking: the `ArgumentOutOfRangeException` it replaces never
  shipped, because `EventFilterType` and its converter are themselves new in this same unreleased section,
  so no released version ever exhibited the old behaviour.
- **Breaking:** `SimulateTransactionResponse.SorobanAuthorization` is now `[JsonIgnore]`, matching the
  `SorobanTransactionData` property beside it. Serialization reads every property, so a response
  carrying a malformed `auth` entry threw `InvalidDataException` from inside `JsonSerializer.Serialize`
  — for a caller who was only trying to log or cache the response, and from a call that has nothing to
  do with authorization. The value is derived from `Results[0].Auth`, which is serialized already, so
  nothing leaves the payload that was not already in it; round-tripping a serialized response still
  reconstructs the entries. Code that read `SorobanAuthorization` back out of serialized JSON must read
  the auth entries instead — note that these are the SDK's own CLR property names, so in a payload
  produced by `JsonSerializer.Serialize` the path is `Results[0].Auth`, not the `results[0].auth` of the
  RPC wire format ([#229](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/229)).
- **Breaking:** `SimulateTransactionResponse.LedgerEntryChange.Type` and
  `.RestorePreamble.MinResourceFee` are now `[JsonRequired]`. Both are declared in a way that cannot be
  violated by an *absent* property — a non-nullable `string` and a `long` — but nothing enforced that:
  `RespectNullableAnnotations` rejects an explicit `null` and says nothing about a missing key. A
  `stateChanges` entry with no `type` therefore left a non-nullable `string` holding `null`, so every
  `Type == "created"` comparison silently returned false; a `restorePreamble` with no `minResourceFee`
  yielded `0`, and a caller following the documented "use `MinResourceFee` and `SorobanTransactionData`
  to submit a `RestoreFootprint` operation" flow would submit it underfunded. Stellar RPC marks neither
  field `omitempty`, so requiring them rejects nothing a conforming server sends; a non-conforming reply
  now fails with `JsonException` at deserialization instead of downstream. This enforces presence, not
  membership: the empty `type` that RPC v23.0.0/v23.0.1 emitted still deserializes.
  Both are also `required` now, which carries the same guarantee to the other entry point: the attribute
  binds only the deserializer, so `new LedgerEntryChange { Before = "…" }` still compiled and still left a
  non-nullable `string` holding `null` — the state the attribute exists to make unreachable, one entry point
  over. This is source-breaking for code that constructs either type without setting the field, which the
  compiler now reports; deserialization is unaffected, and these are response types callers normally receive
  rather than build. It matches how the array properties above guard both paths, and how `Claimant` and
  `ClaimableBalanceResponse` already declare their always-present fields
  ([#229](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/229)).
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
  an explicit mapping rather than by serializing the enum.) See the breaking knock-on below
  ([#208](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/208)).
- **Breaking (behavioral):** pinning that wire spelling on `AuthMode` changes what serializing the enum
  *directly* produces. This affects only code that (de)serializes an `AuthMode` itself — the `authMode`
  request field is unaffected, being built from an explicit mapping rather than by serializing the enum,
  and no Stellar RPC response carries the field.
  - Serializing now writes `"enforce"` where a plain `JsonSerializerOptions` — and the parameterless
    `JsonSerializer.Serialize` overload — previously wrote `0`, and `"enforce"` where
    `JsonOptions.DefaultOptions` previously wrote `"ENFORCE"`.
  - Deserializing now accepts only the lowercase RPC spellings. `"ENFORCE"` and `"Enforce"` previously
    round-tripped through `JsonOptions.DefaultOptions` and now throw `JsonException`, because member-name
    matching is case-sensitive once `[JsonStringEnumMemberName]` is applied. (Reading an `AuthMode` from
    its ordinal — `0` — still works, as before.)

  Nothing in the SDK deserializes `AuthMode`, so no SDK behavior depends on the old spellings
  ([#208](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/208)).
- **Breaking:** `SimulateTransactionResponse.MinResourceFee` is now `long?` (was `uint?`). Stellar RPC
  declares `minResourceFee` as an `int64`, so a simulation quoting more than `uint.MaxValue` stroops
  (~429 XLM, reachable on large Wasm uploads and footprint restores) failed to deserialize — and took the
  *entire* `SimulateTransactionResponse` down with it, throwing `JsonException` rather than dropping the
  single field. The nested `RestorePreamble.MinResourceFee` was already `long`, so the two members are now
  consistent with each other and with the peer SDKs (Java `long`, Python `int`)
  ([#212](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/212)).
- **Breaking:** `Transaction.AddResourceFee` now takes a `long` (was `uint`) and validates the result
  instead of wrapping. A resource fee that would push the total past `uint.MaxValue` — the width of the
  transaction envelope's `fee` field — throws `OverflowException` and leaves `Fee` unchanged, and a
  negative fee throws `ArgumentOutOfRangeException`. Previously the addition wrapped modulo 2^32, so a
  large server-supplied `minResourceFee` could silently yield a *tiny* transaction fee (100 stroops +
  4294967295 stroops produced 99 stroops) with nothing bounding the value between the simulated number and
  the fee the user signed. Existing call sites that pass a `uint` still compile unchanged, since `uint`
  widens to `long` implicitly; the signature change is binary-breaking, so consumers must recompile
  ([#212](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/212)).
- **Breaking:** `SimulateTransactionResponse.RestorePreamble.SorobanTransactionData` no longer throws
  `ArgumentNullException` on every access, making the archived-entry restore workflow usable for the first
  time. The backing `transactionData` property was private and get-only with no `[JsonInclude]`, so
  System.Text.Json never populated it and the public getter always parsed `null`. It now matches the
  outer response's equivalent property, and — like that one — is typed `SorobanTransactionData?` and
  returns `null` when the preamble carries no transaction data, instead of throwing from a property
  getter outside any `try` around the awaited `SimulateTransaction` call. Consumers with nullable
  reference types enabled will need a null check (or `!`) at the use site
  (fixes [#213](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/213)).
- **Breaking:** `SimulateTransactionResponse.LedgerEntryChange.Key` is now `string?`. Stellar RPC tags
  the field `omitempty` and omits it whenever the key is empty, and a *missing* property does not
  violate a non-nullable annotation the way an explicit `null` does — so `RespectNullableAnnotations`
  never caught it and the property could be null at runtime while advertising that it could not.
  Consumers compiling with nullable reference types enabled may see new warnings where the value was
  dereferenced. Widening the annotation also relaxes the opposite path: an explicit `"key": null`, which
  `RespectNullableAnnotations` previously rejected with `JsonException` purely because the property was
  declared non-nullable, now deserializes to `null`. RPC never sends that shape — the field is
  `omitempty`, so it is either present with a value or absent entirely — but code that caught
  `JsonException` for it will no longer see one. (`LedgerEntryChange.Type` is unaffected and stays
  non-nullable: RPC tags it neither `omitempty` nor optional and its `MarshalJSON` always writes a string,
  so a conforming server always sends it. Note though that the string can be empty — RPC v23.0.0/v23.0.1
  shipped pre-allocated no-op state changes whose `type` marshalled to `""` and whose `key` was omitted,
  fixed in v23.0.2 ([stellar/stellar-rpc#506](https://github.com/stellar/stellar-rpc/pull/506)); that is
  the real payload behind `Key`'s nullability. Compare `Type` against `"created"`/`"updated"`/`"deleted"`
  rather than assuming one of the three.)
  ([#211](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/211))
- `SimulateTransactionResponse.SorobanAuthorization`, `.SorobanTransactionData`, and
  `.RestorePreamble.SorobanTransactionData` no longer leak raw XDR decoder exceptions. All three
  getters decode server-supplied base64 on every read, and a malformed blob surfaced as whichever of
  `InvalidDataException`, `IOException`/`EndOfStreamException`, `FormatException`,
  `IndexOutOfRangeException`, `ArgumentException`, or `InvalidOperationException` the decoder
  happened to raise — none of them documented, and all of them thrown from a property rather than
  from the awaited `SimulateTransaction` call, so they escaped any `try` around it. An unknown
  `SorobanCredentialsType` discriminant needs only eight bytes of `auth` to trigger this, which meant
  even a defensive `if (response.SorobanAuthorization != null)` threw instead of returning null. All
  three properties now normalize the recognized decode failures to a single documented
  `InvalidDataException` (naming the offending auth entry's index, with the original exception preserved
  as `InnerException`). The caught set is deliberately wider than the one `Sep45Challenge` uses, because
  that method stops at the generated decoder while these properties continue through the SDK's own
  `FromXdr` dispatch. It is empirical rather than proven exhaustive, so it is documented as the failure
  this API reports and not as a guarantee that nothing else can escape. They still throw rather than
  returning null, so `!= null` is not a safe presence check — test `Results?[0].Auth` or catch
  `InvalidDataException`
  ([#211](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/211)).
- **Breaking:** those same three properties changed observable exception type. A caller who wrapped them
  in `catch (FormatException)` or `catch (IOException)` to handle a malformed blob will find that clause
  no longer fires: `System.IO.InvalidDataException` derives from neither. Catch `InvalidDataException`.
- Three inputs that defeated that normalization entirely, each reachable from a hostile or
  non-conforming RPC endpoint with a handful of bytes, now normalize like the rest:
  - An `SCV_VEC` or `SCV_MAP` whose XDR *optional* body is absent. Those are the only two optional arms
    of `SCVal`; the generated decoder legitimately leaves the body null for a present-flag of `0`, and
    `SCVec.FromSCValXdr`/`SCMap.FromSCValXdr` then dereferenced it, throwing a raw
    `NullReferenceException` out of the property. Eight bytes of `auth` were enough, and the same shape
    reached `SorobanTransactionData` through a `CONTRACT_DATA` footprint key and
    `InvokeHostFunctionOperation.FromXdr` through any server-supplied envelope. Both now throw
    `ArgumentException`, which the callers' filters already cover. Note that this is fixed at the root,
    in `SCVec.FromSCValXdr`/`SCMap.FromSCValXdr`, rather than in the `simulateTransaction` response
    getters: those two are public and are reached from `Transaction.FromEnvelopeXdr` and
    `InvokeHostFunctionOperation.FromXdr` as well, so the behaviour change applies to every caller
    decoding an untrusted `SCVal`, not only to `SimulateTransactionResponse`. Catching
    `NullReferenceException` in the response getters instead would have been narrower but would have
    masked genuine SDK defects.
  - An asset code that is empty once its trailing NUL padding is stripped, or too short for its
    `ALPHANUM12` discriminant, reached via a `TRUSTLINE` footprint key or a `createContract` argument.
    This raised `AssetCodeLengthInvalidException`, which derives straight from `Exception` and so sat
    outside every hierarchy the filter named. Four zero bytes in an otherwise well-formed blob triggered
    it. It is now caught explicitly.
  - A `null` element inside `results`. The guard tested `Results.Length` but not the element, so
    `"results": [null]` passed it and then dereferenced null. It now yields `null`
    ([#211](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/211)).
- **Breaking:** `SendTransactionResponse.Status` is now `[JsonRequired]`. `RespectNullableAnnotations`
  rejects an explicit `null` but cannot reject an *absent* property, and an enum is a value type besides
  — so a `sendTransaction` response carrying no `status` at all deserialized to the zero member,
  `PENDING`, presenting a submission the server never accepted as pending. That is the same outcome the
  `"status": 0` fix above closes, reached by a simpler payload. Stellar RPC tags the field without
  `omitempty`, so requiring it rejects nothing a conforming server sends
  ([#211](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/211)).
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
- **Breaking:** `GetEventsRequest.PaginationOptions.Cursor` and `.Limit` are now auto-properties instead
  of public fields, so `getEvents` pagination finally reaches the wire. System.Text.Json ignores fields
  unless `IncludeFields` is set and `JsonOptions.DefaultOptions` does not set it, so both members
  serialized to an empty `"pagination": {}` and cursor paging was impossible: a request with
  `Limit = 2` came back with the server's default page size (100 events against Stellar RPC 28.0.0); a
  `startLedger`-plus-cursor request silently re-read the first page forever; and a cursor-only request —
  the shape Stellar RPC requires, since it rejects a cursor combined with a ledger range — failed with
  `-32602 startLedger must be positive`. One consequence is newly visible rather than fixed: a caller
  that sets both `StartLedger` and a cursor now gets `-32602 ledger ranges and cursor cannot both be
  set`, where the dropped cursor previously let the call succeed. Until
  [#197](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/197) is fixed, that error surfaces as a
  `null` result rather than an exception. The change is source-compatible for object-initializer and
  property-access callers but binary-breaking (a field load is not a property call), so consumers must
  recompile against this version. The top-level `PaginationOptions` used by `GetTransactionsRequest`
  and `GetLedgersRequest` always declared both members as properties and was never affected
  ([#214](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/214)).
- **Breaking (behavioral):** `TransactionInfo.ResultValue` — and the derived `WasmHash` /
  `CreatedContractId` — now read the Soroban return value from TransactionMeta V3 as well as V4, and return
  `null` instead of throwing when `resultMetaXdr` cannot be decoded. Previously the getter navigated only
  `V4.SorobanMeta.ReturnValue`, so a successful contract invocation recorded before Protocol 23 (a V3 meta)
  reported no return value at all — even though the sibling `TransactionMeta` property on the same class
  decoded the same payload fine — and a meta with an unknown union discriminant, truncated XDR, or invalid
  base64 threw a raw `InvalidDataException`, `EndOfStreamException` or `FormatException` from a property
  getter. Two consequences for existing callers. First, a `catch` around a `ResultValue` / `WasmHash` /
  `CreatedContractId` access no longer fires for an undecodable payload: the properties now yield `null` for
  bad base-64, malformed, truncated or over-deep XDR, an unknown discriminant, and hostile length prefixes.
  A value that decodes but that the SDK cannot map onto an `SCVal` still throws, since that is a gap in the
  SDK rather than bad input. Second, because V3's `returnValue` is a mandatory XDR field while V4's is an
  optional pointer, a void-returning invocation recorded on a V3 meta now yields a non-null `SCVoid` where it
  previously yielded `null`, so `ResultValue != null` no longer means "the invocation produced a value" for
  pre-Protocol 23 transactions. The return value is read straight off the decoded XDR rather than through the
  `TransactionMeta` property, so it still surfaces when some unrelated part of the metadata cannot be mapped
  to an SDK type. Affects `GetTransaction` and `GetTransactions` alike; the V3 regression shipped in 14.0.1
  ([#81](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/81) switched the getter from V3 to V4 instead
  of supporting both), the throw-on-undecodable behavior in 10.0.0
  ([#224](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/224)).
- **Breaking (behavioral):** `TransactionInfo.TransactionMeta` no longer reports *every* failure as absent
  metadata. It still returns `null` for each case it documents — bad base-64, malformed, truncated or
  over-deep XDR, a metadata version this SDK does not model, and a decoded structure that cannot be mapped
  onto an SDK type — but it no longer sits behind a bare `catch`, so a failure that says nothing about the
  payload now propagates instead of being swallowed: an `OutOfMemoryException` while building a large
  metadata graph, or a defect in this SDK surfacing as a `NullReferenceException`, previously reached
  callers as "this transaction has no metadata". Unlike `ResultValue`, it still reports a decoded structure
  the SDK cannot map (`InvalidOperationException`) as `null`: it is an all-or-nothing view of the metadata,
  so an unmappable part is the same answer as no metadata
  ([#224](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/224)).
  Both properties now decide "bad payload" from one shared predicate rather than two hand-maintained clause
  lists. The lists had already diverged: neither named `AssetCodeLengthInvalidException`, which derives
  straight from `Exception` rather than from `ArgumentException`, so a `resultMetaXdr` carrying a `TRUSTLINE`
  ledger-entry change with an unmappable asset code — four NUL bytes in an otherwise well-formed blob are
  enough — threw it out of the `TransactionMeta` getter instead of returning `null` as documented. The shared
  predicate is the one `SimulateTransactionResponse` already used, whose set was derived empirically from a
  fuzz of these decoders, so the properties also gain `IndexOutOfRangeException` from that list; the two
  classes now call a single internal helper rather than keeping separate copies.
  The `InvalidOperationException` carve-out in `ResultValue` is now expressed as an exclusion from the
  shared set rather than by re-listing the other types.
- SEP-45 `ClientWebAuthContract` now accepts a challenge or token response that starts with a UTF-8 byte order
  mark; it was rejected as invalid JSON. It reads response bodies through the size-bounded reader the SEP-12
  `KycService` uses, so a fix to that reader reaches both clients.
