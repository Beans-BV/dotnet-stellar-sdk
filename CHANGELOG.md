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
  - `SorobanAddressCredentialsWithDelegates` and `SorobanDelegateSignature` — CAP-0071-01 delegated
    credentials (`SOROBAN_CREDENTIALS_ADDRESS_WITH_DELEGATES`).
  - `SorobanAuthorization` signing helpers: `AuthorizeEntry`, `AuthorizeEntryWithDelegates`,
    `BuildWithDelegatesEntry`, `BuildAuthorizationEntryPreimageHash`, and the lower-level
    `BuildAuthPreimageHash` / `BuildAddressAuthPreimageHash`.
  - `ISorobanEntrySigner` with the built-in `KeyPairEntrySigner` (classic Ed25519), plus a
    `SorobanCredentialsVersion` (`Preserve`/`V1`/`V2`) option that defaults to preserving the entry's
    existing credential variant (matching the JS reference SDK).
  - Signing output is verified byte-for-byte against `@stellar/stellar-sdk@16.0.0-rc.1` for the V2 and
    delegated paths via the known-answer vectors in `StellarDotnetSdk.Tests/TestData/generate-p27-auth-kat.mjs`.

### Changed

- **Breaking:** `SorobanCredentials.ToXdr()` is now `abstract` (was a concrete method that switched on
  the runtime type). External subclasses of `SorobanCredentials` must now override `ToXdr()`.
- The `SorobanCredentials`, `SorobanSourceAccountCredentials`, and `SorobanAddressCredentials` classes
  moved from `InvokeHostFunctionOperation.cs` to a new `SorobanCredentials.cs` file. They remain in the
  `StellarDotnetSdk.Operations` namespace, so `using`/fully-qualified references are unaffected.

### Removed

- **Breaking:** `SorobanSourceAccountCredentials.ToSorobanCredentialsXdr()` and
  `SorobanAddressCredentials.ToSorobanCredentialsXdr()`. Use `ToXdr()` instead (it now produces the same
  XDR via the `abstract`/`override` pair).
