# SEP-0010 (Stellar Web Authentication) Compatibility Matrix

**Updated:** 2026-06-17
**SDK:** StellarDotnetSdk
**SDK Version:** 12.0.0
**SEP Version:** 3.4.1
**SEP Status:** Active
**SEP URL:** https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0010.md

## SEP Summary

This SEP defines the standard way for clients such as wallets or exchanges to
create authenticated web sessions on behalf of a user who holds a Stellar
account. A wallet may want to authenticate with any web service which requires
a Stellar account ownership verification, for example, to upload KYC
information to an anchor in an authenticated way as described in
[SEP-12](https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0012.md).

This SEP also supports authenticating users of shared, omnibus, or pooled
Stellar accounts. Clients can use [memos](https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0010.md#memos) or
[muxed accounts](https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0010.md#muxed-accounts) to distinguish users or sub-accounts of
shared accounts.

## Overall Coverage

**Total Coverage:** 100.0% (22/22 applicable fields)

- ✅ **Implemented:** 22/22
- ❌ **Not Implemented:** 0/22
- ➖ **N/A (excluded):** 4 (`jwt_claims`, `jwt_expiration`, `jwt_token_generation`, `jwt_token_validation`)

_Note: StellarDotnetSdk implements the client (`ClientWebAuth`) and server (`ServerWebAuth`) sides of the SEP-10 **challenge** flow — building, reading, and verifying the challenge transaction and its signers. It does **not** mint or validate the session JWT: issuing the token (and constructing/checking its `sub`/`iat`/`exp` claims) is an anchor-**application** responsibility, so those four JWT fields are marked **N/A** and excluded from coverage. The SDK only parses the returned token string (`jwt_token_response`). Client-domain verification resolves the `client_domain` `SIGNING_KEY` from `stellar.toml` on the **client** side (`ClientWebAuth.JwtTokenAsync`); `ServerWebAuth` verifies the resulting signer, not the TOML._

**Required Fields:** 100.0% (16/16)

**Optional Fields:** 100.0% (6/6 applicable)

## Implementation Status

✅ **Implemented**

### Implementation Files

- `StellarDotnetSdk/Sep/Sep0010/ClientWebAuth.cs`
- `StellarDotnetSdk/Sep/Sep0010/ServerWebAuth.cs`
- `StellarDotnetSdk/Sep/Sep0010/ChallengeResponse.cs`
- `StellarDotnetSdk/Sep/Sep0010/SubmitChallengeResponse.cs`
- `StellarDotnetSdk/Sep/Sep0010/ClientDomainSigningDelegate.cs`
- `StellarDotnetSdk/Sep/Sep0010/Exceptions/*.cs`

### Key Classes

- **`ClientWebAuth`**: Client-side SEP-10 web authentication implementation (challenge fetch, validation, signing, JWT retrieval)
- **`ServerWebAuth`**: Server-side SEP-10 web authentication implementation (challenge build, read/verify, signer threshold verification)
- **`ChallengeResponse`**: Response from the SEP-0010 challenge endpoint (transaction XDR + optional network_passphrase)
- **`SubmitChallengeResponse`**: Response from the SEP-0010 token endpoint (JWT token or error)
- **`ClientDomainSigningDelegate`**: Async delegate for signing challenge transactions with a client domain's signing key (e.g., HSM or remote signing service)
- **`WebAuthException`**: Base exception type for SEP-0010 errors
- **`ChallengeValidationException`**: Base class for challenge validation errors
- **`ChallengeValidationErrorInvalidSeqNr`**: Error when challenge has invalid sequence number
- **`ChallengeValidationErrorInvalidSourceAccount`**: Error when challenge has invalid source account
- **`ChallengeValidationErrorInvalidTimeBounds`**: Error when challenge timebounds are invalid or expired
- **`ChallengeValidationErrorInvalidOperationType`**: Error when challenge contains invalid operation type
- **`ChallengeValidationErrorInvalidHomeDomain`**: Error when home domain in challenge is invalid
- **`ChallengeValidationErrorInvalidWebAuthDomain`**: Error when web auth domain is invalid
- **`ChallengeValidationErrorInvalidSignature`**: Error when challenge signature verification fails
- **`ChallengeValidationErrorInvalidNonceValue`**: Error when challenge nonce value is not 64 bytes
- **`ChallengeValidationErrorMemoAndMuxedAccount`**: Error when both memo and muxed account are present
- **`ChallengeValidationErrorInvalidMemoType`**: Error when memo type is not supported
- **`ChallengeValidationErrorInvalidMemoValue`**: Error when memo value is invalid
- **`ChallengeRequestErrorException`**: Error response from challenge request endpoint
- **`SubmitChallengeTimeoutResponseException`**: Exception when challenge submission times out
- **`SubmitChallengeUnknownResponseException`**: Exception for unknown response from challenge submission
- **`SubmitChallengeErrorResponseException`**: Exception when challenge submission returns error
- **`NoWebAuthEndpointFoundException`**: Exception when web auth endpoint not found in stellar.toml
- **`NoWebAuthServerSigningKeyFoundException`**: Exception when server signing key not found
- **`NoClientDomainSigningKeyFoundException`**: Exception when client domain signing key not found
- **`MissingClientDomainException`**: Exception when client domain is required but not provided
- **`MissingTransactionInChallengeResponseException`**: Exception when challenge response lacks transaction
- **`NoMemoForMuxedAccountsException`**: Exception when memo is used with muxed accounts
- **`InvalidWebAuthenticationException`**: Exception thrown by the server-side flow for any invalid challenge transaction

## Coverage by Section

| Section | Coverage | Required Coverage | Implemented | Not Implemented | Total |
|---------|----------|-------------------|-------------|-----------------|-------|
| Authentication Endpoints | 100.0% | 100.0% | 2 | 0 | 2 |
| Challenge Transaction Features | 100.0% | 100.0% | 9 | 0 | 9 |
| Client Domain Features | 100.0% | 100.0% | 4 | 0 | 4 |
| JWT Token Features | 100.0% | 100.0% | 1 | 0 | 1 |
| Verification Features | 100.0% | 100.0% | 6 | 0 | 6 |

_JWT Token Features excludes four N/A fields (`jwt_claims`, `jwt_expiration`, `jwt_token_generation`, `jwt_token_validation`) — JWT minting/validation is an anchor-application responsibility; see the note under Overall Coverage._

## Detailed Field Comparison

### Authentication Endpoints

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `get_auth_challenge` | ✓ | ✅ | `ClientWebAuth.GetChallengeAsync` / `ServerWebAuth.BuildChallengeTransaction` | GET /auth endpoint - Returns challenge transaction |
| `post_auth_token` | ✓ | ✅ | `ClientWebAuth.SendSignedChallengeAsync` / `ServerWebAuth.VerifyChallengeTransactionSigners` | POST /auth endpoint - Validates signed challenge and returns JWT token |

### Challenge Transaction Features

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `challenge_transaction_generation` | ✓ | ✅ | `ServerWebAuth.BuildChallengeTransaction` | Generate challenge transaction with proper structure |
| `home_domain_operation` | ✓ | ✅ | `ClientWebAuth.ValidateChallenge` / `ServerWebAuth.ReadChallengeTransaction` | First operation contains home_domain + " auth" as data name |
| `manage_data_operations` | ✓ | ✅ | `ClientWebAuth.ValidateChallenge` / `ServerWebAuth.ReadChallengeTransaction` | Challenge uses ManageData operations for auth data |
| `nonce_generation` | ✓ | ✅ | `ServerWebAuth.BuildChallengeTransaction` | Random nonce in ManageData operation value |
| `sequence_number_zero` | ✓ | ✅ | `ClientWebAuth.ValidateChallenge` / `ServerWebAuth.ReadChallengeTransaction` | Challenge transaction has sequence number 0 |
| `server_signature` | ✓ | ✅ | `ClientWebAuth.ValidateChallenge` / `ServerWebAuth.BuildChallengeTransaction` | Challenge is signed by server before sending to client |
| `timebounds_enforcement` | ✓ | ✅ | `ClientWebAuth.ValidateChallenge` / `ServerWebAuth.ReadChallengeTransaction` | Challenge transaction has timebounds for expiration |
| `transaction_envelope_format` | ✓ | ✅ | `ClientWebAuth.ValidateChallenge` / `ServerWebAuth.BuildChallengeTransaction` | Challenge uses proper Stellar transaction envelope format |
| `web_auth_domain_operation` |  | ✅ | `ClientWebAuth.ValidateChallenge` / `ServerWebAuth.BuildChallengeTransaction` | Optional operation with web_auth_domain for domain verification |

### Client Domain Features

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `client_domain_operation` |  | ✅ | `ServerWebAuth.BuildChallengeTransaction` / `ClientWebAuth.ValidateChallenge` | Add client_domain ManageData operation to challenge |
| `client_domain_parameter` |  | ✅ | `ClientWebAuth.GetChallengeAsync` | Support optional client_domain parameter in GET /auth |
| `client_domain_signature` |  | ✅ | `ClientWebAuth.SignTransaction` / `ClientDomainSigningDelegate` | Require signature from client domain account |
| `client_domain_verification` |  | ✅ | `ClientWebAuth.JwtTokenAsync` / `ServerWebAuth.VerifyChallengeTransactionSigners` | Resolve and verify the client domain. **`stellar.toml` lookup is client-side only:** `ClientWebAuth.JwtTokenAsync` resolves the `client_domain` `SIGNING_KEY` from the client domain's `stellar.toml`. `ServerWebAuth` does **not** read TOML — `VerifyChallengeTransactionSigners` / `ValidateSignatures` only verifies that the resolved client-domain signer signed the challenge. |

### JWT Token Features

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `jwt_claims` |  | ➖ N/A | ⚙️ Server (anchor app) | JWT includes the required claims (`sub`, `iat`, `exp`). Constructed by the anchor application that mints the token; the SDK has no JWT code and treats the token as opaque. |
| `jwt_expiration` |  | ➖ N/A | ⚙️ Server (anchor app) | JWT includes an expiration time. Set by the anchor application when minting; not handled by the SDK. |
| `jwt_token_generation` |  | ➖ N/A | ⚙️ Server (anchor app) | Mint the JWT after successful challenge verification. Anchor-application responsibility — no JWT minting exists anywhere in the SDK. |
| `jwt_token_response` | ✓ | ✅ | `ClientWebAuth.SendSignedChallengeAsync` / `SubmitChallengeResponse` | Parse the JWT token string from the server's JSON response (`"token"` field). |
| `jwt_token_validation` |  | ➖ N/A | ⚙️ Server (anchor app) | Validate the JWT's structure/signature. The SDK has no JWT validation; `ServerWebAuth` verifies the **challenge transaction** (a separate step the issuer runs before minting), credited under Verification Features. |

### Verification Features

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `challenge_validation` | ✓ | ✅ | `ClientWebAuth.ValidateChallenge` / `ServerWebAuth.ReadChallengeTransaction` | Validate challenge transaction structure and content |
| `home_domain_validation` | ✓ | ✅ | `ClientWebAuth.ValidateChallenge` / `ServerWebAuth.ReadChallengeTransaction` | Validate home domain in challenge matches server |
| `memo_support` |  | ✅ | `ClientWebAuth.GetChallengeAsync` / `ClientWebAuth.ValidateChallenge` | Support optional memo in challenge for muxed accounts |
| `multi_signature_support` | ✓ | ✅ | `ClientWebAuth.SignTransaction` / `ServerWebAuth.VerifyChallengeTransactionThreshold` | Support multiple signatures on challenge (client account + signers) |
| `signature_verification` | ✓ | ✅ | `ClientWebAuth.ValidateChallenge` / `ServerWebAuth.VerifyChallengeTransactionSigners` | Verify all signatures on challenge transaction |
| `timebounds_validation` | ✓ | ✅ | `ClientWebAuth.ValidateChallenge` / `ServerWebAuth.ReadChallengeTransaction` | Validate challenge is within valid time window |

## Implementation Gaps

🎉 **No gaps found!** All applicable fields are implemented.

The four JWT fields `jwt_claims`, `jwt_expiration`, `jwt_token_generation`, and `jwt_token_validation` are marked **N/A**, not gaps: minting/validating the JWT (and constructing its claims) is an anchor-application responsibility with no counterpart in the SDK, which has no JWT code and only parses the returned token string (`jwt_token_response`).

## Recommendations

✅ The SDK fully covers the SEP-10 **challenge** flow (100% of applicable fields) on both the client and server sides.

Notable strengths versus client-only SDKs:

- **Both client and server challenge handling**: StellarDotnetSdk covers the SEP-10 challenge surface with `ClientWebAuth` for wallets/clients and `ServerWebAuth` for anchors/servers, including signer-threshold verification (`VerifyChallengeTransactionThreshold`) and per-signer verification (`VerifyChallengeTransactionSigners`). Minting and validating the session JWT remains an anchor-application responsibility (see the N/A fields above).
- **Pluggable client-domain signing**: `ClientDomainSigningDelegate` enables HSM or remote signing flows for the `client_domain` key without exposing the private key to the SDK.
- **Discovery via stellar.toml**: `ClientWebAuth.FromDomainAsync` auto-discovers `WEB_AUTH_ENDPOINT` and `SIGNING_KEY` from the anchor's `stellar.toml`.
- **Configurable clock skew**: Both client and server expose a `GracePeriod` for timebounds tolerance (default 5 minutes).

## Legend

- ✅ **Implemented**: Field is implemented in SDK
- ❌ **Not Implemented**: Field is missing from SDK
- ➖ **N/A**: Not an SDK responsibility (an anchor-application concern); excluded from coverage
- ⚙️ **Server**: Server-side feature in the SEP specification
- ✓ **Required**: Field is required by SEP specification
- (blank) **Optional**: Field is optional
