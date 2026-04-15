# SEP-0010 (Stellar Web Authentication) Compatibility Matrix

**Updated:** 2026-04-15
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
[SEP-12](sep-0012.md).

This SEP also supports authenticating users of shared, omnibus, or pooled
Stellar accounts. Clients can use [memos](#memos) or
[muxed accounts](#muxed-accounts) to distinguish users or sub-accounts of
shared accounts.

## Overall Coverage

**Total Coverage:** 100.0% (24/24 fields)

- ✅ **Implemented:** 24/24
- ❌ **Not Implemented:** 0/24

_Note: Unlike client-only SDKs, StellarDotnetSdk implements both client (`ClientWebAuth`) and server (`ServerWebAuth`) sides of SEP-0010. Features that are server-side-only (e.g. JWT token validation, client domain stellar.toml verification) are therefore covered by `ServerWebAuth` rather than excluded._

**Required Fields:** 100.0% (19/19)

**Optional Fields:** 100.0% (5/5)

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
| Client Domain Features | 100.0% | 100% | 4 | 0 | 4 |
| JWT Token Features | 100.0% | 100.0% | 5 | 0 | 5 |
| Verification Features | 100.0% | 100.0% | 6 | 0 | 6 |

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
| `client_domain_verification` |  | ✅ | `ClientWebAuth.JwtTokenAsync` / `ServerWebAuth.VerifyChallengeTransactionSigners` | Verify client domain by checking stellar.toml. **Note:** StellarDotnetSdk implements this on both sides — the client resolves the client_domain SIGNING_KEY from `stellar.toml` in `JwtTokenAsync`, and the server verifies the client domain signer in `VerifyChallengeTransactionSigners` / `ValidateSignatures`. |

### JWT Token Features

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `jwt_claims` | ✓ | ✅ | `ClientWebAuth.SendSignedChallengeAsync` / `ServerWebAuth.VerifyChallengeTransactionSigners` | JWT token includes required claims (sub, iat, exp) |
| `jwt_expiration` | ✓ | ✅ | `ClientWebAuth.SendSignedChallengeAsync` / `ServerWebAuth.VerifyChallengeTransactionSigners` | JWT token includes expiration time |
| `jwt_token_generation` | ✓ | ✅ | `ClientWebAuth.SendSignedChallengeAsync` / `ServerWebAuth.VerifyChallengeTransactionSigners` | Generate JWT token after successful challenge validation |
| `jwt_token_response` | ✓ | ✅ | `ClientWebAuth.SendSignedChallengeAsync` / `SubmitChallengeResponse` | Return JWT token in JSON response with "token" field |
| `jwt_token_validation` |  | ✅ | `ServerWebAuth.VerifyChallengeTransactionSigners` / `ServerWebAuth.VerifyChallengeTransactionThreshold` ⚙️ Server | Validate JWT token structure and signature. **Note:** This is a server-side validation feature. StellarDotnetSdk exposes server-side challenge verification through `ServerWebAuth`, which the JWT issuer uses prior to minting a token. |

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

🎉 **No gaps found!** All fields are implemented.

## Recommendations

✅ The SDK has full compatibility with SEP-0010!

Notable strengths versus client-only SDKs:

- **Both client and server**: StellarDotnetSdk uniquely covers the full SEP-0010 surface with `ClientWebAuth` for wallets/clients and `ServerWebAuth` for anchors/servers, including signer-threshold verification (`VerifyChallengeTransactionThreshold`) and per-signer verification (`VerifyChallengeTransactionSigners`).
- **Pluggable client-domain signing**: `ClientDomainSigningDelegate` enables HSM or remote signing flows for the `client_domain` key without exposing the private key to the SDK.
- **Discovery via stellar.toml**: `ClientWebAuth.FromDomainAsync` auto-discovers `WEB_AUTH_ENDPOINT` and `SIGNING_KEY` from the anchor's `stellar.toml`.
- **Configurable clock skew**: Both client and server expose a `GracePeriod` for timebounds tolerance (default 5 minutes).

## Legend

- ✅ **Implemented**: Field is implemented in SDK
- ❌ **Not Implemented**: Field is missing from SDK
- ⚙️ **Server**: Server-side only feature in the SEP specification (covered by `ServerWebAuth` in this SDK)
- ✓ **Required**: Field is required by SEP specification
- (blank) **Optional**: Field is optional
