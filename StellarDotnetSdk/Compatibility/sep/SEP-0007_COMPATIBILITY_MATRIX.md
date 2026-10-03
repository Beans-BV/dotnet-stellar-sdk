# SEP-0007 (URI Scheme to facilitate delegated signing) Compatibility Matrix

**Updated:** 2026-09-29
**SDK:** StellarDotnetSdk
**SDK Version:** 16.0.0
**SEP Version:** 2.1.0
**SEP Status:** Active
**SEP URL:** https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0007.md

## SEP Summary

This Stellar Ecosystem Proposal introduces a URI Scheme that can be used to
generate a URI that will serve as a request to sign a transaction. The URI
(request) will typically be signed by the user's trusted wallet where she
stores her secret key(s).

## Overall Coverage

**Total Coverage:** 100.0% (31/31 fields)

- ✅ **Implemented:** 31/31
- ❌ **Not Implemented:** 0/31

_Note: the field set is the one used for the SEP-7 matrices of the other Stellar SDKs (e.g. the Flutter SDK's 31 fields in six sections), so coverage figures are directly comparable. Behaviour this SDK adds on top of that field set — `replace` grammar enforcement, request submission, signing-key pinning — is listed under Recommendations._

**Required Fields:** 100.0% (18/18)

**Optional Fields:** 100.0% (13/13)

## Implementation Status

✅ **Implemented**

### Implementation Files

- `StellarDotnetSdk/Sep/Sep0007/UriScheme.cs`
- `StellarDotnetSdk/Sep/Sep0007/Sep7Uri.cs`
- `StellarDotnetSdk/Sep/Sep0007/Sep7UriParser.cs`
- `StellarDotnetSdk/Sep/Sep0007/Sep7StellarTomlReader.cs`
- `StellarDotnetSdk/Sep/Sep0007/Sep7Replacement.cs`
- `StellarDotnetSdk/Sep/Sep0007/Sep7OperationType.cs`
- `StellarDotnetSdk/Sep/Sep0007/Sep7Parameters.cs`
- `StellarDotnetSdk/Sep/Sep0007/Sep7MemoType.cs`
- `StellarDotnetSdk/Sep/Sep0007/Sep7ValidationResult.cs`
- `StellarDotnetSdk/Sep/Sep0007/Sep7SubmitResult.cs`
- `StellarDotnetSdk/Sep/Sep0007/Exceptions/*.cs`

### Key Classes

- **`UriScheme`**: Builds (`GenerateSignTransactionUri`, `GeneratePayOperationUri`), parses and validates (`ParseUri`, `TryParseUri`, `ValidateUri`), signs (`SignUri`) and verifies (`VerifySignature`, `VerifyOriginDomainSignatureAsync`, `IsValidSignedUriAsync`) SEP-7 URIs; parses and builds `replace` values (`ParseReplacements`, `ReplacementsToString`); signs and hands a `tx` request on to its callback or Horizon (`SignAndSubmitTransactionAsync`, `SubmitTransactionAsync`, `SubmitToCallbackAsync`)
- **`Sep7Uri`**: A parsed, validated request — typed accessors for every parameter plus `GetTransaction`, `GetNetwork`, `GetMemo`, `GetAsset`, and the parsed `Replacements` and `ChainedUri`
- **`Sep7Replacement`**: One `replace` entry (reference identifier, SEP-11 Txrep path, hint)
- **`Sep7OperationType`**: The `tx` and `pay` operations
- **`Sep7Parameters`** / **`Sep7MemoType`**: Parameter-name and `memo_type` constants
- **`Sep7ValidationResult`**: Non-throwing validation outcome with the failure reason
- **`Sep7SubmitResult`**: Outcome of handing on a signed transaction (callback status and body, or the Horizon response)
- **`Sep7Exception`**: Base exception type for SEP-7 errors
- **`InvalidSep7UriException`**: Error when a URI is structurally invalid (the message carries the reason)
- **`InvalidOriginDomainException`**: Error when `origin_domain` is not a fully qualified domain name
- **`MissingOriginDomainException`**: Error when a signature is created or verified for a URI without `origin_domain`
- **`MissingSignatureException`**: Error when a URI has `origin_domain` but no `signature`
- **`OriginDomainStellarTomlException`**: Error when the origin domain's stellar.toml cannot be fetched or parsed
- **`NoUriRequestSigningKeyFoundException`**: Error when the stellar.toml has no `URI_REQUEST_SIGNING_KEY`
- **`InvalidUriRequestSigningKeyException`**: Error when the published `URI_REQUEST_SIGNING_KEY` is not a valid account id
- **`UriRequestSigningKeyChangedException`**: Error when the published key differs from the key the wallet pinned earlier
- **`InvalidSep7SignatureException`**: Error when the request signature does not verify against the published key

## Coverage by Section

| Section | Coverage | Required Coverage | Implemented | Not Implemented | Total |
|---------|----------|-------------------|-------------|-----------------|-------|
| Common Parameters | 100.0% | 100.0% | 4 | 0 | 4 |
| PAY Operation Parameters | 100.0% | 100.0% | 6 | 0 | 6 |
| Signature Features | 100.0% | 100.0% | 3 | 0 | 3 |
| TX Operation Parameters | 100.0% | 100.0% | 5 | 0 | 5 |
| URI Operations | 100.0% | 100.0% | 2 | 0 | 2 |
| Validation Features | 100.0% | 100.0% | 11 | 0 | 11 |

## Detailed Field Comparison

### Common Parameters

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `msg` |  | ✅ | `Sep7Parameters.Msg` / `Sep7Uri.Message` | Message for the user (max 300 characters) |
| `network_passphrase` |  | ✅ | `Sep7Parameters.NetworkPassphrase` / `Sep7Uri.NetworkPassphrase` / `Sep7Uri.GetNetwork` | Network passphrase for the transaction (public network when absent) |
| `origin_domain` |  | ✅ | `Sep7Parameters.OriginDomain` / `Sep7Uri.OriginDomain` | Fully qualified domain name of the service originating the request |
| `signature` |  | ✅ | `Sep7Parameters.Signature` / `Sep7Uri.Signature` | Signature of the URL for verification |

### PAY Operation Parameters

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `amount` |  | ✅ | `Sep7Parameters.Amount` / `Sep7Uri.Amount` | Amount to send |
| `asset_code` |  | ✅ | `Sep7Parameters.AssetCode` / `Sep7Uri.AssetCode` / `Sep7Uri.GetAsset` | Asset code for the payment (e.g., USD, BTC) |
| `asset_issuer` |  | ✅ | `Sep7Parameters.AssetIssuer` / `Sep7Uri.AssetIssuer` / `Sep7Uri.GetAsset` | Stellar account ID of asset issuer |
| `destination` | ✓ | ✅ | `Sep7Parameters.Destination` / `Sep7Uri.Destination` | Stellar account ID or payment address to receive payment |
| `memo` |  | ✅ | `Sep7Parameters.Memo` / `Sep7Uri.Memo` / `Sep7Uri.GetMemo` | Memo value to attach to transaction (hash memos base64-encoded, as the spec requires) |
| `memo_type` |  | ✅ | `Sep7Parameters.MemoType` / `Sep7MemoType` / `Sep7Uri.MemoType` | Type of memo (MEMO_TEXT, MEMO_ID, MEMO_HASH, MEMO_RETURN) |

### Signature Features

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `sign_uri` | ✓ | ✅ | `UriScheme.SignUri` | Sign a SEP-0007 URI with a keypair |
| `verify_signature` | ✓ | ✅ | `UriScheme.VerifySignature` | Verify URI signature with a public key |
| `verify_signed_uri` | ✓ | ✅ | `UriScheme.VerifyOriginDomainSignatureAsync` / `UriScheme.IsValidSignedUriAsync` | Verify signed URI by fetching signing key from origin domain TOML |

### TX Operation Parameters

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `callback` |  | ✅ | `Sep7Parameters.Callback` / `Sep7Uri.Callback` / `Sep7Uri.CallbackUrl` / `UriScheme.SubmitToCallbackAsync` | URL for transaction submission callback (also accepted on `pay`, as the spec allows) |
| `chain` |  | ✅ | `Sep7Parameters.Chain` / `Sep7Uri.Chain` / `Sep7Uri.ChainedUri` | Nested SEP-0007 URL for transaction chaining |
| `pubkey` |  | ✅ | `Sep7Parameters.Pubkey` / `Sep7Uri.PublicKey` | Stellar public key to specify which key should sign |
| `replace` |  | ✅ | `Sep7Parameters.Replace` / `Sep7Uri.Replacements` / `UriScheme.ParseReplacements` / `UriScheme.ReplacementsToString` | URL-encoded field replacement using Txrep (SEP-0011) format |
| `xdr` | ✓ | ✅ | `Sep7Parameters.Xdr` / `Sep7Uri.Xdr` / `Sep7Uri.GetTransaction` | Base64 encoded TransactionEnvelope XDR |

### URI Operations

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `pay` | ✓ | ✅ | `UriScheme.GeneratePayOperationUri` | Payment operation - Request to pay a specific address |
| `tx` | ✓ | ✅ | `UriScheme.GenerateSignTransactionUri` | Transaction operation - Request to sign a transaction |

### Validation Features

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `validate_asset_code` | ✓ | ✅ | `UriScheme.ValidateUri` / `UriScheme.ParseUri` | Validate asset code length and format (1–12 alphanumerics, paired with `asset_issuer`) |
| `validate_chain_nesting` | ✓ | ✅ | `UriScheme.ValidateUri` / `UriScheme.ParseUri` | Validate chain parameter nesting depth (max 7 levels) |
| `validate_destination_parameter` | ✓ | ✅ | `UriScheme.ValidateUri` / `UriScheme.ParseUri` | Validate destination parameter for pay operation |
| `validate_memo_type` | ✓ | ✅ | `UriScheme.ValidateUri` / `UriScheme.ParseUri` | Validate memo type is one of allowed types |
| `validate_memo_value` | ✓ | ✅ | `UriScheme.ValidateUri` / `UriScheme.ParseUri` | Validate memo value based on memo type (hash memos exactly 32 bytes) |
| `validate_message_length` | ✓ | ✅ | `UriScheme.ValidateUri` / `UriScheme.ParseUri` | Validate message parameter length (max 300 chars) |
| `validate_operation_type` | ✓ | ✅ | `UriScheme.ValidateUri` / `UriScheme.ParseUri` | Validate operation type is tx or pay |
| `validate_origin_domain` | ✓ | ✅ | `UriScheme.ValidateUri` / `UriScheme.ParseUri` | Validate origin_domain is fully qualified domain name |
| `validate_stellar_address` | ✓ | ✅ | `UriScheme.ValidateUri` / `UriScheme.ParseUri` | Validate Stellar addresses (account IDs, muxed accounts, contract IDs) |
| `validate_uri_scheme` | ✓ | ✅ | `UriScheme.ValidateUri` / `UriScheme.ParseUri` | Validate that URI starts with web+stellar: |
| `validate_xdr_parameter` | ✓ | ✅ | `UriScheme.ValidateUri` / `UriScheme.ParseUri` | Validate XDR parameter for tx operation |

## Implementation Gaps

🎉 **No gaps found!** All fields are implemented.

## Recommendations

✅ The SDK has full compatibility with SEP-0007!

The fields above are the SDK's side of the spec. Some SEP-7 rules are the wallet's to follow with them, and the SDK leaves them to the application: showing the request, its `msg`, `network_passphrase` and verified `origin_domain` to the user and getting their approval; alerting the user when `UriRequestSigningKeyChangedException` is raised; and treating a request without `origin_domain` as unverified. `SignAndSubmitTransactionAsync` does enforce the rules that can be enforced in code: it refuses a request whose `origin_domain` signature does not verify.

Notable strengths:

- **Spec-exact request signatures**: `SignUri` reproduces the spec's signed example byte-for-byte, as well as the typescript-wallet-sdk signing vector. Verification works on the received string as-is (it is never re-encoded). Query values are decoded like an HTML form, as the other Stellar SDKs do, so a `+` written for a space by the typescript-wallet-sdk reads as a space (a testnet `network_passphrase` included); in the base64 values (`xdr`, `signature`, hash memos) a raw `+` stays a `+`; a URI holding a lone UTF-16 surrogate, or escapes that are not valid UTF-8 (on which decoders disagree: `URLSearchParams` reads U+FFFD, .NET's own decoder keeps the escape as text), is rejected, so a signature covers exactly one request and every wallet reads the same values.
- **Strict `replace` grammar**: `ParseReplacements` enforces the spec rule that reference identifiers are balanced across the `;`. It also rejects paths SEP-7 forbids (the `tx.` prefix, signatures, the `_present`/`len` metadata fields), malformed Txrep paths, and duplicate fields or hints. Only the first `:` separates a hint from its identifier, so hints containing `:` are kept whole.
- **Spec-faithful limits**: `chain` nesting is capped at exactly 7 levels. `msg` may be exactly 300 characters, counted before URL-encoding. Hash memos are generated as base64 of their 32 raw bytes.
- **Signing-key pinning**: `VerifyOriginDomainSignatureAsync` accepts the key the wallet cached for the domain, raises `UriRequestSigningKeyChangedException` when the published key changes (the spec's "must alert the user"), and returns the verified key for caching.
- **Unverified requests are not signed**: a request with an `origin_domain` but no `signature` is invalid (`ParseUri`, `TryParseUri`, `ValidateUri`), and `SignAndSubmitTransactionAsync` verifies the `origin_domain` signature before it signs anything, as the spec's wallet steps require.
- **Hardened network access**: The origin domain is chosen by whoever wrote the URI, so its stellar.toml is read with a non-recursive reader of the root table instead of the general TOML parser, which a few kilobytes of hostile TOML crash with a stack overflow. The stellar.toml must be valid UTF-8 throughout, as TOML requires; a callback's answer is decoded by its byte order mark, else its `Content-Type` charset if .NET supports it, else as UTF-8. The stellar.toml fetch and callback responses are read with a 512 KiB cap, also while streaming, and within the HTTP client's timeout, body included. `origin_domain` must be a fully qualified domain name, not under the loopback-only `localhost` TLD, before it is placed in the stellar.toml URL. With the SDK's own HTTP client, the stellar.toml request follows at most 5 redirects, each https to a fully qualified domain name on the default port without user info (so a redirect cannot reach an IP address, a single-label or `localhost` host, or another port that `origin_domain` itself could not name; a domain whose DNS points at a private address is still reached, so a server verifying untrusted requests should filter addresses in its own HTTP client), and the callback POST follows none, so the signed transaction goes to the checked URL only. Callbacks must be https, with plain http allowed only for loopback, and `SubmitToCallbackAsync` applies the same whitespace, invisible-character, user-info, IDN-host, fragment, malformed-percent-escape, backslash and dot-segment checks as the URI parser. Failures to verify an `origin_domain`, transport failures included, are typed `Sep7Exception`s.
- **End-to-end `tx` handling**: `SignAndSubmitTransactionAsync` checks `pubkey`, verifies any `origin_domain`, signs for `network_passphrase`, then POSTs the signed envelope form-encoded to `callback` or submits it to Horizon. `SubmitTransactionAsync` covers requests whose `replace` fields the wallet filled in.

Optional future work:

- **Applying `replace` values**: Filling the requested Txrep fields into the transaction needs SEP-11 (Txrep), which this SDK does not implement yet. Until then, wallets edit the transaction from `Sep7Uri.GetTransaction()` themselves.
- **Track the spec**: revisit this matrix if SEP-7 adds operations or callback types.

## Legend

- ✅ **Implemented**: Field is implemented in SDK
- ❌ **Not Implemented**: Field is missing from SDK
- ⚙️ **Server**: Server-side only feature (not applicable to client SDKs)
- ✓ **Required**: Field is required by SEP specification
- (blank) **Optional**: Field is optional
