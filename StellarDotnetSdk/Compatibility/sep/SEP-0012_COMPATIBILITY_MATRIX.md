# SEP-0012 (KYC API) Compatibility Matrix

**Updated:** 2026-09-30
**SDK:** StellarDotnetSdk
**SDK Version:** 12.0.0
**SEP Version:** 1.15.0
**SEP Status:** Active
**SEP URL:** https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0012.md

## SEP Summary

This SEP defines a standard way for stellar clients to upload KYC (or other)
information to anchors and other services. [SEP-6](https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0006.md) and
[SEP-31](https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0031.md) use this protocol, but it can serve as a stand-alone
service as well.

This SEP was made with these goals in mind:

- interoperability
- Allow a customer to enter their KYC information to their wallet once and use
  it across many services without re-entering information manually
- handle the most common 80% of use cases
- handle image and binary data
- support the set of fields defined in [SEP-9](https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0009.md)
- support authentication via [SEP-10](https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0010.md)
- support the provision of data for [SEP-6](https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0006.md),
  [SEP-24](https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0024.md), [SEP-31](https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0031.md), and others
- give customers control over their data by supporting complete data erasure

## Overall Coverage

**Total Coverage:** 100.0% (90/90 fields)

- ✅ **Implemented:** 90/90
- ❌ **Not Implemented:** 0/90

_Note: StellarDotnetSdk implements the **client** (wallet) side of SEP-12, counting every endpoint, request parameter, response field, status literal and field type, the request rules a client can enforce before sending (`type` whenever `transaction_id` is given, no `memo` for a `C...` account; the SDK additionally rejects a `PUT /customer` name used by both a text field and a file), and the wallet's own obligation to verify the signature and freshness of anchor callbacks. Anchor-side behaviour (storing customer data, running KYC review, sending callbacks, accepting `application/json` and `application/x-www-form-urlencoded` bodies) is outside an SDK's scope; the server-only content types are listed below as ⚙️ and not counted. `KycCallbackSignature.CreateSignatureHeader` additionally implements the anchor's "COMPUTE signature" step for anchor implementations and tests._

**Required Fields:** 100.0% (49/49)

**Optional Fields:** 100.0% (41/41)

## Implementation Status

✅ **Implemented**

### Implementation Files

- `StellarDotnetSdk/Sep/Sep0012/KycService.cs`
- `StellarDotnetSdk/Sep/Sep0012/KycCallbackSignature.cs`
- `StellarDotnetSdk/Sep/Sep0012/KycUntrustedText.cs`
- `StellarDotnetSdk/Sep/Sep0012/Requests/GetCustomerInfoRequest.cs`
- `StellarDotnetSdk/Sep/Sep0012/Requests/PutCustomerInfoRequest.cs`
- `StellarDotnetSdk/Sep/Sep0012/Requests/PutCustomerVerificationRequest.cs`
- `StellarDotnetSdk/Sep/Sep0012/Requests/PutCustomerCallbackRequest.cs`
- `StellarDotnetSdk/Sep/Sep0012/Requests/DeleteCustomerRequest.cs`
- `StellarDotnetSdk/Sep/Sep0012/Requests/PostCustomerFileRequest.cs`
- `StellarDotnetSdk/Sep/Sep0012/Requests/GetCustomerFilesRequest.cs`
- `StellarDotnetSdk/Sep/Sep0012/Requests/RequestPrinter.cs`
- `StellarDotnetSdk/Sep/Sep0012/Responses/GetCustomerInfoResponse.cs`
- `StellarDotnetSdk/Sep/Sep0012/Responses/GetCustomerInfoField.cs`
- `StellarDotnetSdk/Sep/Sep0012/Responses/GetCustomerInfoProvidedField.cs`
- `StellarDotnetSdk/Sep/Sep0012/Responses/PutCustomerInfoResponse.cs`
- `StellarDotnetSdk/Sep/Sep0012/Responses/CustomerFileResponse.cs`
- `StellarDotnetSdk/Sep/Sep0012/Responses/GetCustomerFilesResponse.cs`
- `StellarDotnetSdk/Sep/Sep0012/Responses/CustomerStatus.cs`
- `StellarDotnetSdk/Sep/Sep0012/Responses/ProvidedFieldStatus.cs`
- `StellarDotnetSdk/Sep/Sep0012/Responses/FieldType.cs`
- `StellarDotnetSdk/Sep/Sep0012/Responses/Sep12EnumJsonConverters.cs`
- `StellarDotnetSdk/Sep/Sep0012/Responses/UtcDateTimeOffsetJsonConverter.cs`
- `StellarDotnetSdk/Sep/Sep0012/Responses/KycErrorResponse.cs`
- `StellarDotnetSdk/Sep/Sep0012/Exceptions/KycServiceException.cs`
- `StellarDotnetSdk/Sep/Sep0012/Exceptions/AuthenticationRequiredException.cs`
- `StellarDotnetSdk/Sep/Sep0012/Exceptions/CustomerNotFoundException.cs`
- `StellarDotnetSdk/Sep/Sep0012/Exceptions/PayloadTooLargeException.cs`
- `StellarDotnetSdk/Sep/Sep0012/Exceptions/InvalidKycResponseException.cs`

### Key Classes

- **`KycService`**: Client for every SEP-12 endpoint. `FromDomainAsync` discovers `KYC_SERVER` (falling back to `TRANSFER_SERVER`) from stellar.toml; requires https (http only for `localhost` or a standard-form loopback IP); responses are read with a 1 MiB cap within the client's timeout and parsed with the hardened `JsonOptions.DefaultOptions`.
- **`KycCallbackSignature`**: Verifies the `Signature` / `X-Stellar-Signature` header of anchor callbacks (Ed25519 over `<timestamp>.<host>.<body>`, with a freshness window) over the body as a string or as raw bytes; `GetSignedHost` derives the host string the anchor signs; `CreateSignatureHeader` computes the header for anchors and tests.
- **`GetCustomerInfoRequest`**: Parameters for `GET /customer`.
- **`GetCustomerInfoResponse`**: Customer status, required fields, provided fields and message; `FromJson` parses callback payloads.
- **`GetCustomerInfoField`**: A field the anchor still needs (`type`, `description`, `choices`, `optional`).
- **`GetCustomerInfoProvidedField`**: A field the anchor has received, adding `status` and `error`.
- **`PutCustomerInfoRequest`**: Customer data for `PUT /customer`: identification, SEP-9 fields and files, custom fields and files, `*_verification` values and `*_file_id` references.
- **`PutCustomerInfoResponse`**: The created or updated customer `id`.
- **`PutCustomerVerificationRequest`**: Verification values for the deprecated `PUT /customer/verification`.
- **`PutCustomerCallbackRequest`**: Callback URL and customer identification for `PUT /customer/callback`.
- **`DeleteCustomerRequest`**: Account and memo for `DELETE /customer/{account}`.
- **`PostCustomerFileRequest`**: File content, name and content type for `POST /customer/files`.
- **`GetCustomerFilesRequest`**: File or customer ID for `GET /customer/files`.
- **`CustomerFileResponse`**: Uploaded file metadata (`file_id`, `content_type`, `size`, `expires_at`, `customer_id`).
- **`GetCustomerFilesResponse`**: The list of files for `GET /customer/files`.
- **`CustomerStatus`** / **`ProvidedFieldStatus`** / **`FieldType`**: The SEP-12 status and field-type literals, matched exactly (case-sensitive; ordinals and unknown values rejected).
- **`KycServiceException`**: Base exception; carries the HTTP status, the anchor's `error` text and any `Retry-After` delay.
- **`AuthenticationRequiredException`**: 401, or 403 `authentication_required`.
- **`CustomerNotFoundException`**: 404 from `GET`/`PUT /customer`, `/customer/verification`, `/customer/callback` or `DELETE /customer` (a 404 from `/customer/files` is a plain `KycServiceException`).
- **`PayloadTooLargeException`**: 413 on a file upload.
- **`InvalidKycResponseException`**: A success response that is empty, oversized, not valid UTF-8, not JSON, has duplicate properties, or violates the SEP-12 schema.

## Coverage by Section

| Section | Coverage | Required Coverage | Implemented | Not Implemented | Total |
|---------|----------|-------------------|-------------|-----------------|-------|
| API Endpoints | 100.0% | 100.0% | 7 | 0 | 7 |
| Discovery and Authentication | 100.0% | 100.0% | 5 | 0 | 5 |
| GET /customer Request Parameters | 100.0% | 100.0% | 9 | 0 | 9 |
| PUT /customer Request Parameters | 100.0% | 100.0% | 13 | 0 | 13 |
| SEP-9 Integration | 100.0% | 100.0% | 2 | 0 | 2 |
| Customer Response Fields | 100.0% | 100.0% | 5 | 0 | 5 |
| Customer Statuses | 100.0% | 100.0% | 4 | 0 | 4 |
| Field Properties | 100.0% | 100.0% | 6 | 0 | 6 |
| Field Types | 100.0% | 100.0% | 4 | 0 | 4 |
| Provided Field Statuses | 100.0% | 100.0% | 4 | 0 | 4 |
| PUT /customer/verification | 100.0% | 100.0% | 2 | 0 | 2 |
| PUT /customer/callback | 100.0% | 100.0% | 5 | 0 | 5 |
| Callback Signature | 100.0% | 100.0% | 6 | 0 | 6 |
| DELETE /customer | 100.0% | 100.0% | 3 | 0 | 3 |
| Customer Files | 100.0% | 100.0% | 9 | 0 | 9 |
| Error Handling | 100.0% | 100.0% | 5 | 0 | 5 |
| Content Types | 100.0% | 100.0% | 1 | 0 | 1 |

## Detailed Field Comparison

### API Endpoints

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `delete_customer` | ✓ | ✅ | `KycService.DeleteCustomerAsync` | DELETE /customer/{account} - Delete all personal information about a customer |
| `get_customer` | ✓ | ✅ | `KycService.GetCustomerInfoAsync` | GET /customer - Check the status of a customer's info, or fetch the fields required to register one |
| `get_customer_files` |  | ✅ | `KycService.GetCustomerFilesAsync` | GET /customer/files - Get metadata about uploaded files (an optional endpoint for anchors) |
| `post_customer_files` |  | ✅ | `KycService.PostCustomerFileAsync` | POST /customer/files - Upload a binary file for later reference from PUT /customer (an optional endpoint for anchors) |
| `put_customer` | ✓ | ✅ | `KycService.PutCustomerInfoAsync` | PUT /customer - Idempotent upload of customer information |
| `put_customer_callback` | ✓ | ✅ | `KycService.PutCustomerCallbackAsync` | PUT /customer/callback - Register a callback URL for customer status updates |
| `put_customer_verification` |  | ✅ | `KycService.PutCustomerVerificationAsync` | PUT /customer/verification (deprecated) - Verify fields with confirmation codes; marked `[Obsolete]` in favour of `PutCustomerInfoRequest.VerificationFields` |

### Discovery and Authentication

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `jwt_authentication` | ✓ | ✅ | `*Request.Jwt` | Every request is sent with `Authorization: Bearer <JWT>`; an empty JWT, or one containing a line break, is rejected before sending |
| `kyc_server_discovery` | ✓ | ✅ | `KycService.FromDomainAsync` | Discover `KYC_SERVER` from the anchor's stellar.toml (SEP-1); a declared server that is not an absolute `https` URL raises `KycServiceException` |
| `sep10_authentication` |  | ✅ | `ClientWebAuth.JwtTokenAsync` → `*Request.Jwt` | Accepts a JWT obtained via SEP-10 for `G...`/`M...` accounts |
| `sep45_authentication` |  | ✅ | `ClientWebAuthContract.JwtTokenAsync` → `*Request.Jwt` | Accepts a JWT obtained via SEP-45 for `C...` contract accounts (SEP-12 v1.15.0) |
| `transfer_server_fallback` | ✓ | ✅ | `KycService.FromDomainAsync` | Fall back to `TRANSFER_SERVER` when `KYC_SERVER` is not declared |

### GET /customer Request Parameters

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `account` |  | ✅ | `GetCustomerInfoRequest.Account` | (Deprecated) Stellar account (`G...`, `M...` or `C...`) of the customer |
| `id` |  | ✅ | `GetCustomerInfoRequest.Id` | ID of the customer as returned by a previous PUT request |
| `lang` |  | ✅ | `GetCustomerInfoRequest.Lang` | ISO 639-1 language code for human-readable descriptions, choices and messages |
| `memo` |  | ✅ | `GetCustomerInfoRequest.Memo` | Memo that uniquely identifies a customer of a shared account |
| `memo_for_contract_account` | ✓ | ✅ | `KycService.GetCustomerInfoAsync` | A memo is rejected before sending when the account is a `C...` contract account |
| `memo_type` |  | ✅ | `GetCustomerInfoRequest.MemoType` | (Deprecated) Type of memo: `text`, `id` or `hash`; any other value is rejected before sending |
| `transaction_id` |  | ✅ | `GetCustomerInfoRequest.TransactionId` | Transaction the customer's info is associated with |
| `transaction_id_requires_type` | ✓ | ✅ | `KycService.GetCustomerInfoAsync` | A `transaction_id` without a `type` is rejected before sending |
| `type` |  | ✅ | `GetCustomerInfoRequest.Type` | Type of action the customer is being KYC'd for |

### PUT /customer Request Parameters

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `account` |  | ✅ | `PutCustomerInfoRequest.Account` | (Deprecated) Stellar account of the customer |
| `binary_fields_last` | ✓ | ✅ | `KycService.PutCustomerInfoAsync` | Binary fields are sent after all other fields in the multipart body |
| `custom_fields` |  | ✅ | `PutCustomerInfoRequest.CustomFields` | Non-SEP-9 text fields requested by the anchor |
| `custom_files` |  | ✅ | `PutCustomerInfoRequest.CustomFiles` | Non-SEP-9 binary fields requested by the anchor |
| `file_id_references` |  | ✅ | `PutCustomerInfoRequest.FileReferences` | `<field>_file_id` references to files uploaded via POST /customer/files |
| `id` |  | ✅ | `PutCustomerInfoRequest.Id` | ID returned by a previous PUT /customer request |
| `memo` |  | ✅ | `PutCustomerInfoRequest.Memo` | Memo that uniquely identifies a customer of a shared account |
| `memo_for_contract_account` | ✓ | ✅ | `KycService.PutCustomerInfoAsync` | A memo is rejected before sending when the account is a `C...` contract account |
| `memo_type` |  | ✅ | `PutCustomerInfoRequest.MemoType` | (Deprecated) Type of memo: `text`, `id` or `hash`; any other value is rejected before sending |
| `transaction_id` |  | ✅ | `PutCustomerInfoRequest.TransactionId` | Transaction the customer's info is associated with |
| `transaction_id_requires_type` | ✓ | ✅ | `KycService.PutCustomerInfoAsync` | A `transaction_id` without a `type` is rejected before sending |
| `type` |  | ✅ | `PutCustomerInfoRequest.Type` | Type of the customer as defined by the SEP using SEP-12 |
| `verification_fields` |  | ✅ | `PutCustomerInfoRequest.VerificationFields` | `<field>_verification` values for fields in `VERIFICATION_REQUIRED` (SEP-12 v1.12.0 flow) |

### SEP-9 Integration

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `sep9_binary_fields` | ✓ | ✅ | `PutCustomerInfoRequest.KycFields` | SEP-9 binary fields (`photo_id_front`, `organization.photo_incorporation_doc`, ...) sent as file parts |
| `standard_kyc_fields` | ✓ | ✅ | `PutCustomerInfoRequest.KycFields` (`StandardKycFields`) | All SEP-9 natural-person and organization fields, including nested financial-account and card fields |

### Customer Response Fields

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `fields` |  | ✅ | `GetCustomerInfoResponse.Fields` | Fields the anchor has not yet received |
| `id` |  | ✅ | `GetCustomerInfoResponse.Id` | ID of the customer |
| `message` |  | ✅ | `GetCustomerInfoResponse.Message` | Human-readable message describing the KYC status |
| `provided_fields` |  | ✅ | `GetCustomerInfoResponse.ProvidedFields` | Fields the anchor has received |
| `status` | ✓ | ✅ | `GetCustomerInfoResponse.Status` | Status of the customer's KYC process (required; rejected if missing) |

### Customer Statuses

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `ACCEPTED` | ✓ | ✅ | `CustomerStatus.Accepted` | All required KYC fields accepted; customer validated for the `type` |
| `NEEDS_INFO` | ✓ | ✅ | `CustomerStatus.NeedsInfo` | More info needed; `fields` lists it (not enforced: a response without `fields` still parses, so the status stays readable) |
| `PROCESSING` | ✓ | ✅ | `CustomerStatus.Processing` | KYC process in flight |
| `REJECTED` | ✓ | ✅ | `CustomerStatus.Rejected` | KYC failed and will never succeed; `message` explains why (not enforced: a response without `message` still parses, so the rejection reaches the caller) |

### Field Properties

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `choices` |  | ✅ | `GetCustomerInfoField.Choices` | Array of valid values for the field; string and number elements are accepted (a number is kept as its JSON text) |
| `description` | ✓ | ✅ | `GetCustomerInfoField.Description` | Human-readable description of the field (required; rejected if missing or null) |
| `error` |  | ✅ | `GetCustomerInfoProvidedField.Error` | Description of why a provided field was rejected |
| `optional` |  | ✅ | `GetCustomerInfoField.Optional` | Whether the field may be omitted (absent or `null` means `false`) |
| `status` |  | ✅ | `GetCustomerInfoProvidedField.Status` | Validation status of a provided field |
| `type` | ✓ | ✅ | `GetCustomerInfoField.Type` | Data type of the field value (required; rejected if missing) |

### Field Types

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `binary` | ✓ | ✅ | `FieldType.Binary` | Binary data such as a photo or document |
| `date` | ✓ | ✅ | `FieldType.Date` | Date value |
| `number` | ✓ | ✅ | `FieldType.Number` | Numeric value |
| `string` | ✓ | ✅ | `FieldType.String` | Text value |

### Provided Field Statuses

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `ACCEPTED` | ✓ | ✅ | `ProvidedFieldStatus.Accepted` | The field has been validated |
| `PROCESSING` | ✓ | ✅ | `ProvidedFieldStatus.Processing` | The field is being validated |
| `REJECTED` | ✓ | ✅ | `ProvidedFieldStatus.Rejected` | The field did not pass validation |
| `VERIFICATION_REQUIRED` | ✓ | ✅ | `ProvidedFieldStatus.VerificationRequired` | The field must be verified (e.g. with a confirmation code) |

### PUT /customer/verification

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `*_verification` | ✓ | ✅ | `PutCustomerVerificationRequest.VerificationFields` | One or more SEP-9 fields suffixed with `_verification` |
| `id` | ✓ | ✅ | `PutCustomerVerificationRequest.Id` | ID of the customer as returned by a previous PUT request |

### PUT /customer/callback

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `account` |  | ✅ | `PutCustomerCallbackRequest.Account` | (Deprecated) Stellar account of the customer |
| `id` |  | ✅ | `PutCustomerCallbackRequest.Id` | ID of the customer |
| `memo` |  | ✅ | `PutCustomerCallbackRequest.Memo` | Memo identifying a customer of a shared account |
| `memo_type` |  | ✅ | `PutCustomerCallbackRequest.MemoType` | (Deprecated) Type of memo |
| `url` | ✓ | ✅ | `PutCustomerCallbackRequest.Url` | Callback URL the anchor POSTs status changes to (validated as absolute https; http only for `localhost` or a standard-form loopback IP) |

### Callback Signature

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `callback_payload_parsing` | ✓ | ✅ | `GetCustomerInfoResponse.FromJson` | Parse the POSTed GET /customer payload with the hardened JSON settings |
| `host_binding` | ✓ | ✅ | `KycCallbackSignature.Verify` (`host`), `KycCallbackSignature.GetSignedHost` | Signature covers the wallet host, preventing relay to another wallet; `GetSignedHost` spells it as the reference Anchor Platform signs it (`host:port` when the callback URL names a port) |
| `signature_header` | ✓ | ✅ | `KycCallbackSignature.Verify` / `SignatureHeaderName` | Parse `Signature: t=<timestamp>, s=<base64 signature>` (duplicate keys rejected); the caller reads the header from its web framework and passes its value |
| `signature_verification` | ✓ | ✅ | `KycCallbackSignature.Verify` | Verify the Ed25519 signature over `<timestamp>.<host>.<body>` with the anchor's `SIGNING_KEY`; a `byte[]` overload verifies the body exactly as received |
| `timestamp_freshness` | ✓ | ✅ | `KycCallbackSignature.Verify` (`maxAge`, default 2 minutes) | Discard callbacks outside the freshness window (within it, a repeated delivery verifies again; deduplicate if needed) |
| `x_stellar_signature_header` |  | ✅ | `KycCallbackSignature.DeprecatedSignatureHeaderName` | Deprecated `X-Stellar-Signature` header, same format |

### DELETE /customer

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `account` | ✓ | ✅ | `DeleteCustomerRequest.Account` | Account (`G...` or `C...`) of the customer to delete, sent as an escaped path segment (`.` and `..` rejected) |
| `memo` |  | ✅ | `DeleteCustomerRequest.Memo` | Memo identifying a customer of a shared account, sent in the request body |
| `memo_type` |  | ✅ | `DeleteCustomerRequest.MemoType` | (Deprecated) Type of memo, sent in the request body |

### Customer Files

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `content_type` | ✓ | ✅ | `CustomerFileResponse.ContentType` | `Content-Type` of the uploaded file |
| `customer_id` |  | ✅ | `CustomerFileResponse.CustomerId` | Customer the file is associated with (`null` if none yet) |
| `expires_at` |  | ✅ | `CustomerFileResponse.ExpiresAt` | UTC ISO 8601 time at which an unreferenced file is discarded (a value without a zone designator is read as UTC) |
| `file` | ✓ | ✅ | `PostCustomerFileRequest.File` | File content, sent as the `file` part of a multipart body (with `FileName`/`ContentType` options; non-ASCII characters, quotes, backslashes and `%` in a file name are percent-encoded per RFC 7578) |
| `file_id` | ✓ | ✅ | `CustomerFileResponse.FileId` | Unique identifier of the uploaded file |
| `files` | ✓ | ✅ | `GetCustomerFilesResponse.Files` | List of file objects returned by GET /customer/files |
| `get_by_customer_id` |  | ✅ | `GetCustomerFilesRequest.CustomerId` | Query all files of a customer |
| `get_by_file_id` |  | ✅ | `GetCustomerFilesRequest.FileId` | Query a single file |
| `size` | ✓ | ✅ | `CustomerFileResponse.Size` | Size of the file in bytes |

### Error Handling

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `bad_request_400` | ✓ | ✅ | `KycServiceException` | Invalid request; carries the anchor's error |
| `error_response_key` | ✓ | ✅ | `KycServiceException.ErrorMessage` | Anchor error text from the `error` key (duplicate keys rejected, text sanitized in `Message`) |
| `not_found_404` | ✓ | ✅ | `CustomerNotFoundException` | Unknown customer `id`, or no information on the customer |
| `payload_too_large_413` | ✓ | ✅ | `PayloadTooLargeException` | Uploaded file exceeds the server's limit |
| `unauthorized_401` | ✓ | ✅ | `AuthenticationRequiredException` | Client not authenticated (401, or 403 `authentication_required`) |

### Content Types

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `multipart_form_data` | ✓ | ✅ | `KycService` | All request bodies are `multipart/form-data`, the encoding SEP-12 requires for binary data and one every endpoint accepts |
| `application_json` |  | ⚙️ | — | Servers must also accept JSON bodies; a client chooses one encoding, so this is not counted |
| `application_x_www_form_urlencoded` |  | ⚙️ | — | Servers must also accept URL-encoded bodies; a client chooses one encoding, so this is not counted |

## Implementation Gaps

🎉 **No gaps found!** All fields are implemented.

## Recommendations

✅ The SDK has full compatibility with SEP-12!

Notable strengths:

- **Beyond the peer reference**: in addition to the Flutter SDK's surface, the SDK implements callback signature verification (`KycCallbackSignature`), the v1.12.0 `*_verification` flow on `PUT /customer`, `*_file_id` references, and typed statuses and field types.
- **Hardened parsing**: response bodies are capped at 1 MiB (declared lengths are refused before reading, undeclared ones stop streaming at the limit) and decoded as strict UTF-8, so malformed bytes are rejected rather than replaced with U+FFFD; duplicate JSON properties are rejected in success **and** error bodies (issue #205's concern, applied from the start); status and field-type literals are matched exactly — on the response properties and on the enum types themselves — so an ordinal or an unknown value cannot be misread as `ACCEPTED`; `null` dictionary and array entries are rejected; the whole exchange, body included, is bounded by the client's timeout; a response from a different origin after a redirect is rejected.
- **Safe requests**: the KYC server and a registered callback URL must use https (http only for `localhost` or a loopback IP in its standard form, never for a server discovered from stellar.toml) and the internal client does not follow redirects; form-data part names must be printable ASCII without quotes or backslashes, and file names are percent-encoded; the account in `DELETE /customer/{account}` is escaped as a path segment and may not be `.` or `..`; server-supplied text (error text, and field names echoed in parse errors) is clamped, with control, format and line-separator characters in any Unicode plane replaced, before it reaches an exception message; `ToString()` of a request redacts the JWT and customer data and reduces a callback URL to its origin, while printing identifiers (customer ID, account, memo).
- **Discovery and authentication**: `KycService.FromDomainAsync` follows the SEP's `KYC_SERVER` → `TRANSFER_SERVER` rule, and every request takes a SEP-10 or SEP-45 JWT.

## Legend

- ✅ **Implemented**: Field is implemented in SDK
- ❌ **Not Implemented**: Field is missing from SDK
- ⚙️ **Server**: Server-side only feature (not applicable to client SDKs)
- ✓ **Required**: Field is required by SEP specification
- (blank) **Optional**: Field is optional
