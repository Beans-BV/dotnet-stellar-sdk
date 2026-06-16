# SEP-0045 (Stellar Web Authentication for Contract Accounts) Compatibility Matrix

**Updated:** 2026-06-17
**SDK:** StellarDotnetSdk
**SDK Version:** 12.0.0
**SEP Version:** 0.1.1
**SEP Status:** Draft
**SEP URL:** https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0045.md

## SEP Summary

This SEP defines the standard way for clients such as wallets or exchanges to
create authenticated web sessions on behalf of a user who holds a contract
account. A wallet may want to authenticate with any web service which requires
a contract account ownership verification, for example, to upload KYC
information to an anchor in an authenticated way as described in
[SEP-12](https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0012.md).

This SEP is based on [SEP-10](https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0010.md), but does not replace it. This SEP
only supports `C` (contract) accounts. SEP-10 only supports `G` and `M`
accounts. Services wishing to support all accounts should implement both SEPs.

## Overall Coverage

**Total Coverage:** 100.0% (35/35 applicable fields)

- ✅ **Implemented:** 35/35
- ❌ **Not Implemented:** 0/35
- ➖ **N/A (excluded):** 1 (`jwt_token_generation`)

_Note: StellarDotnetSdk implements the **client** side of SEP-45 (`ClientWebAuthContract`). The single server-side field, `jwt_token_generation` (minting the JWT returned by `POST /auth`), is marked **N/A** and excluded from coverage: issuing the JWT is an anchor-**application** responsibility implemented by no Stellar SDK. This was verified against the Flutter, Java, Python, JS, and Go SDKs — all leave token minting to the anchor server (e.g. Anchor Platform, the SEP-45 reference server). Coverage therefore reflects 100% of the client-side surface. (The Java and Python SDKs additionally expose server-side challenge **build** and **verify** primitives — distinct from JWT minting — that this SDK does not yet provide; see Recommendations.)_

**Required Fields:** 100.0% (28/28)

**Optional Fields:** 100.0% (7/7 applicable)

## Implementation Status

✅ **Implemented**

### Implementation Files

- `StellarDotnetSdk/Sep/Sep0045/ClientWebAuthContract.cs`
- `StellarDotnetSdk/Sep/Sep0045/Sep45Challenge.cs`
- `StellarDotnetSdk/Sep/Sep0045/ChallengeAuthorizationEntries.cs`
- `StellarDotnetSdk/Sep/Sep0045/ChallengeForContractsResponse.cs`
- `StellarDotnetSdk/Sep/Sep0045/SubmitChallengeForContractsResponse.cs`
- `StellarDotnetSdk/Sep/Sep0045/ClientDomainEntrySigningDelegate.cs`
- `StellarDotnetSdk/Sep/Sep0045/Exceptions/*.cs`

### Key Classes

- **`ClientWebAuthContract`**: Client-side SEP-45 web authentication for contract accounts (challenge fetch, validation, signing, JWT retrieval). Includes `FromDomainAsync` stellar.toml discovery and the end-to-end `JwtTokenAsync` flow.
- **`Sep45Challenge`**: Static helpers for challenge parsing, validation, verification, and construction (`ReadChallenge`, `VerifyServerSignature`, `ComputeAuthorizationHash`, `AppendSignature`, authorization-entry encode/decode).
- **`ChallengeAuthorizationEntries`**: Result of parsing and validating a challenge — the decoded `SorobanAuthorizationEntry` list plus every field extracted from the `web_auth_verify` args map.
- **`ChallengeForContractsResponse`**: Response from the SEP-45 GET /auth endpoint (`authorization_entries` XDR + optional `network_passphrase`; tolerant of both snake_case and camelCase field names).
- **`SubmitChallengeForContractsResponse`**: Response from the SEP-45 POST /auth endpoint (JWT token or error).
- **`ClientDomainEntrySigningDelegate`**: Async delegate for signing the client-domain authorization entry with a remote signing service (e.g., HSM).
- **`WebAuthContractException`**: Base exception type for SEP-45 errors
- **`InvalidSep45ChallengeException`**: Base class for challenge validation errors
- **`InvalidArgumentsException`**: Error when the challenge args are malformed, missing required keys, or the entries XDR is invalid
- **`InvalidClientAccountException`**: Error when the client account does not match or no client authorization entry is present
- **`InvalidClientDomainException`**: Error for client-domain inconsistencies (mismatched/aliased account, unpaired keys, or a declared but absent client-domain entry)
- **`InvalidContractIdException`**: Error when the entry is not a contract invocation or the contract id does not match the expected web auth contract
- **`InvalidEntryCountException`**: Error when fewer than two authorization entries are present
- **`InvalidFunctionNameException`**: Error when the invoked function is not `web_auth_verify`
- **`InvalidHomeDomainException`**: Error when the `home_domain` arg is not in the allowed list
- **`InvalidNetworkPassphraseException`**: Error when the challenge's network passphrase does not match the configured network
- **`InvalidNonceException`**: Error when the `nonce` arg is missing or empty
- **`InvalidServerSignatureException`**: Error when the server entry is missing, unsigned, or its signature does not verify
- **`InvalidWebAuthDomainException`**: Error when the `web_auth_domain` arg does not match the expected domain
- **`MismatchedInvocationsException`**: Error when the authorization entries do not all share the same root invocation
- **`MissingAuthorizationEntriesInChallengeResponseException`**: Error when the challenge response lacks an `authorization_entries` field
- **`MissingClientDomainException`**: Error when a client-domain signing delegate is supplied without the required client domain
- **`NoClientDomainSigningKeyFoundException`**: Error when the client domain's `SIGNING_KEY` is not found in its stellar.toml
- **`NoWebAuthContractEndpointFoundException`**: Error when `WEB_AUTH_FOR_CONTRACTS_ENDPOINT` is not found in stellar.toml
- **`NoWebAuthContractIdFoundException`**: Error when `WEB_AUTH_CONTRACT_ID` is not found in stellar.toml
- **`SubInvocationsNotAllowedException`**: Error when an authorization entry contains sub-invocations
- **`SubmitSignedChallengeForContractsErrorResponseException`**: Error response returned by the token endpoint
- **`SubmitSignedChallengeForContractsTimeoutResponseException`**: Exception when challenge submission times out
- **`SubmitSignedChallengeForContractsUnknownResponseException`**: Exception for an unknown/unparseable response from challenge submission

## Coverage by Section

| Section | Coverage | Required Coverage | Implemented | Not Implemented | Total |
|---------|----------|-------------------|-------------|-----------------|-------|
| Authentication Endpoints | 100.0% | 100.0% | 2 | 0 | 2 |
| Challenge Features | 100.0% | 100.0% | 8 | 0 | 8 |
| Client Domain Features | 100.0% | 100.0% | 5 | 0 | 5 |
| Exception Types | 100.0% | 100.0% | 8 | 0 | 8 |
| JWT Token Features | 100.0% | 100.0% | 2 | 0 | 2 |
| Validation Features | 100.0% | 100.0% | 10 | 0 | 10 |

_JWT Token Features excludes `jwt_token_generation` (server-side JWT minting), marked N/A — implemented by no Stellar SDK; see the note under Overall Coverage._

## Detailed Field Comparison

### Authentication Endpoints

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `get_auth_challenge` | ✓ | ✅ | `ClientWebAuthContract.GetChallengeAsync` | GET /auth endpoint - Returns authorization entries for contract accounts |
| `post_auth_token` | ✓ | ✅ | `ClientWebAuthContract.SendSignedChallengeAsync` | POST /auth endpoint - Validates signed authorization entries and returns JWT token |

### Challenge Features

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `authorization_entry_decoding` | ✓ | ✅ | `Sep45Challenge.ReadChallenge` / `Sep45Challenge.DecodeAuthorizationEntries` | Decode base64 XDR encoded authorization entries from server |
| `authorization_entry_encoding` | ✓ | ✅ | `Sep45Challenge.EncodeAuthorizationEntries` / `ClientWebAuthContract.SignAuthorizationEntriesAsync` | Encode signed authorization entries to base64 XDR for submission |
| `auto_signature_expiration` |  | ✅ | `ClientWebAuthContract.GetLatestLedgerSequenceAsync` | Automatically fetch the latest ledger from Soroban RPC and set the signature expiration (latest + `signatureExpirationLedgers`) |
| `client_entry_signing` | ✓ | ✅ | `ClientWebAuthContract.SignAuthorizationEntriesAsync` / `Sep45Challenge.AppendSignature` | Sign client authorization entry with provided signers |
| `contract_invocation_parsing` | ✓ | ✅ | `Sep45Challenge.ReadChallenge` | Parse web_auth_verify contract invocation from authorization entries |
| `nonce_consistency` | ✓ | ✅ | `Sep45Challenge.ReadChallenge` | Verify nonce is consistent across all authorization entries (enforced by requiring an identical root invocation on every entry) |
| `server_entry_signing` | ✓ | ✅ | `Sep45Challenge.VerifyServerSignature` | Server entry is pre-signed in challenge; the client verifies that pre-signature |
| `signature_expiration_ledger` | ✓ | ✅ | `ClientWebAuthContract.SignAuthorizationEntriesAsync` / `Sep45Challenge.ComputeAuthorizationHash` | Support signature expiration ledger for replay protection |

### Client Domain Features

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `client_domain_callback_signing` |  | ✅ | `ClientWebAuthContract.SignAuthorizationEntriesAsync` / `ClientDomainEntrySigningDelegate` | Sign client domain entry via remote callback |
| `client_domain_entry` |  | ✅ | `Sep45Challenge.ReadChallenge` / `ClientWebAuthContract.SignAuthorizationEntriesAsync` | Handle client domain authorization entry in challenge |
| `client_domain_local_signing` |  | ✅ | `ClientWebAuthContract.SignAuthorizationEntriesAsync` | Sign client domain entry with local keypair (`clientDomainAccountKeyPair`) |
| `client_domain_parameter` |  | ✅ | `ClientWebAuthContract.GetChallengeAsync` | Support optional client_domain parameter in challenge request |
| `client_domain_toml_lookup` |  | ✅ | `ClientWebAuthContract.JwtTokenAsync` | Lookup client domain signing key from stellar.toml |

### Exception Types

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `challenge_request_error_exception` | ✓ | ✅ | `ChallengeForContractsRequestErrorException` | Exception for challenge request errors |
| `invalid_contract_address_exception` | ✓ | ✅ | `InvalidContractIdException` | Exception for contract address mismatch |
| `invalid_function_name_exception` | ✓ | ✅ | `InvalidFunctionNameException` | Exception for invalid function name |
| `invalid_server_signature_exception` | ✓ | ✅ | `InvalidServerSignatureException` | Exception for invalid server signature |
| `missing_client_entry_exception` | ✓ | ✅ | `InvalidClientAccountException` | Exception when client entry is missing (raised by `Sep45Challenge.ReadChallenge` as `InvalidClientAccountException`) |
| `missing_server_entry_exception` | ✓ | ✅ | `InvalidServerSignatureException` | Exception when server entry is missing (raised by `Sep45Challenge.ReadChallenge` / `ClientWebAuthContract.ValidateChallenge` as `InvalidServerSignatureException`) |
| `sub_invocations_exception` | ✓ | ✅ | `SubInvocationsNotAllowedException` | Exception when sub-invocations found |
| `submit_challenge_error_exception` | ✓ | ✅ | `SubmitSignedChallengeForContractsErrorResponseException` | Exception for challenge submission errors |

### JWT Token Features

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `complete_auth_flow` | ✓ | ✅ | `ClientWebAuthContract.JwtTokenAsync` | Execute complete authentication flow via jwtToken method |
| `jwt_token_generation` |  | ➖ N/A | ⚙️ Server (anchor app) | Generate JWT token after successful challenge validation. Server-side minting is an anchor-**application** responsibility implemented by no Stellar SDK (Flutter, Java, Python, JS, Go all verified) — excluded from coverage as N/A. The client consumes the issued JWT via `jwt_token_response`. |
| `jwt_token_response` | ✓ | ✅ | `ClientWebAuthContract.SendSignedChallengeAsync` / `SubmitChallengeForContractsResponse` | Parse JWT token from server response |

### Validation Features

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `account_validation` | ✓ | ✅ | `ClientWebAuthContract.ValidateChallenge` | Validate account argument matches client contract account |
| `client_entry_presence` | ✓ | ✅ | `Sep45Challenge.ReadChallenge` | Validate client authorization entry is present |
| `contract_address_validation` | ✓ | ✅ | `Sep45Challenge.ReadChallenge` | Validate contract address matches WEB_AUTH_CONTRACT_ID from stellar.toml |
| `function_name_validation` | ✓ | ✅ | `Sep45Challenge.ReadChallenge` | Validate function name is web_auth_verify |
| `home_domain_validation` | ✓ | ✅ | `Sep45Challenge.ReadChallenge` | Validate home_domain argument matches expected domain |
| `network_passphrase_validation` |  | ✅ | `ClientWebAuthContract.JwtTokenAsync` | Validate network passphrase if provided in response |
| `server_entry_presence` | ✓ | ✅ | `Sep45Challenge.ReadChallenge` | Validate server authorization entry is present |
| `server_signature_verification` | ✓ | ✅ | `Sep45Challenge.VerifyServerSignature` / `ClientWebAuthContract.ValidateChallenge` | Verify server signature on server authorization entry |
| `sub_invocations_check` | ✓ | ✅ | `Sep45Challenge.ReadChallenge` | Reject authorization entries with sub-invocations |
| `web_auth_domain_validation` | ✓ | ✅ | `Sep45Challenge.ReadChallenge` | Validate web_auth_domain argument matches server domain |

## Implementation Gaps

🎉 **No gaps found!** All applicable client-side fields are implemented.

`jwt_token_generation` (server-side JWT minting) is marked **N/A**, not a gap: issuing the JWT is an anchor-application responsibility that no Stellar SDK implements (verified across the Flutter, Java, Python, JS, and Go SDKs — each leaves token minting to the anchor server). StellarDotnetSdk implements the SEP-45 client, which consumes the issued token (`jwt_token_response`).

## Recommendations

✅ The SDK has full **client-side** compatibility with SEP-45 (100% of applicable fields).

Notable strengths:

- **End-to-end client flow**: `ClientWebAuthContract.JwtTokenAsync` runs the full handshake — fetch challenge, validate, sign, submit, return JWT — while `GetChallengeAsync`, `ValidateChallenge`, `SignAuthorizationEntriesAsync`, and `SendSignedChallengeAsync` are exposed individually for finer control.
- **Strict challenge validation**: `Sep45Challenge.ReadChallenge` enforces the full SEP-45 rule set (entry count, no sub-invocations, contract id, `web_auth_verify` function name, args shape, address credentials, a shared root invocation across entries, allowed `home_domain`, `web_auth_domain`/`web_auth_domain_account` binding, and paired client-domain keys), with hardening against malformed/oversized XDR.
- **Pluggable client-domain signing**: `ClientDomainEntrySigningDelegate` enables HSM or remote signing of the client-domain entry, alongside local-keypair signing.
- **Discovery via stellar.toml**: `ClientWebAuthContract.FromDomainAsync` auto-discovers `WEB_AUTH_FOR_CONTRACTS_ENDPOINT`, `WEB_AUTH_CONTRACT_ID`, and `SIGNING_KEY`.

Optional future work:

- **Server-side challenge primitives**: The Java (`Sep45Challenge.buildChallengeAuthorizationEntries` / `verifyChallengeAuthorizationEntries`) and Python (`build_challenge_authorization_entries` / `verify_challenge_authorization_entries`) SDKs additionally expose server-side helpers that **build** the challenge (signing the server entry) and **verify** a client-signed challenge by simulating the contract's `__check_auth` over Soroban RPC. This SDK is client-only and does not yet provide these. Note this is distinct from JWT minting — even those SDKs stop at "signatures valid" and leave issuing the JWT to the anchor application. Adding a `ServerWebAuthContract` with build/verify helpers (mirroring `ServerWebAuth` for SEP-10) would close the remaining gap versus the most capable peers.
- **Track the spec**: SEP-45 is still a Draft (v0.1.1); revisit this matrix as the specification evolves.

## Legend

- ✅ **Implemented**: Field is implemented in SDK
- ❌ **Not Implemented**: Field is missing from SDK
- ➖ **N/A**: Not an SDK responsibility (an anchor-application concern); excluded from coverage
- ⚙️ **Server**: Server-side feature in the SEP specification
- ✓ **Required**: Field is required by SEP specification
- (blank) **Optional**: Field is optional
