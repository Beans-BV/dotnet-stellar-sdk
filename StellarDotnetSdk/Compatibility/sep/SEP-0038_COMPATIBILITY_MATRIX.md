# SEP-0038 (Anchor RFQ API) Compatibility Matrix

**Updated:** 2026-09-30  
**SDK:** StellarDotnetSdk  
**SDK Version:** 16.0.0  
**SEP Version:** 2.5.0  
**SEP Status:** Draft  
**SEP URL:** https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0038.md

## SEP Summary

This protocol enables anchors to accept off-chain assets in exchange for
different on-chain assets, and vice versa. Specifically, it enables anchors to
provide quotes that can be referenced within the context of existing Stellar
Ecosystem Proposals. How the exchange of assets is facilitated is outside the
scope of this document.

## Overall Coverage

**Total Coverage:** 100.0% (75/75 fields)

- ✅ **Implemented:** 75/75
- ❌ **Not Implemented:** 0/75

_Note: fields are enumerated from the SEP-0038 v2.5.0 specification itself, including the v2.3.0 buy-side `GET /prices` parameters (`buy_asset`, `buy_amount`, `sell_assets`) and the v2.4.0 `sell_delivery_method`/`buy_delivery_method` quote-response fields. The Flutter SDK's matrix (the peer reference, 58 fields, also against v2.5.0) omits those fields, and its implementation does not send or parse them; it also has no authentication, error-handling or context-value rows. This matrix covers all of them. Request-side rules the spec states in prose — exactly one of `sell_amount`/`buy_amount`, the `GET /prices` sell/buy side exclusivity, at most one delivery method on `POST /quote`, and `GET /price` accepting only `sep6`/`sep31` — are enforced client-side before a request is sent. The table counts fields; the protocol rules the spec states in prose, and how each is handled, are listed separately under [Protocol Rules](#protocol-rules) and are not part of the field count._

**Required Fields:** 100.0% (46/46)

**Optional Fields:** 100.0% (29/29)

## Implementation Status

✅ **Implemented**

### Implementation Files

- `StellarDotnetSdk/Sep/Sep0038/QuoteService.cs`
- `StellarDotnetSdk/Sep/Sep0038/AssetIdentifier.cs`
- `StellarDotnetSdk/Sep/Sep0038/Requests/QuoteContext.cs`
- `StellarDotnetSdk/Sep/Sep0038/Requests/PricesRequest.cs`
- `StellarDotnetSdk/Sep/Sep0038/Requests/PriceRequest.cs`
- `StellarDotnetSdk/Sep/Sep0038/Requests/QuoteRequest.cs`
- `StellarDotnetSdk/Sep/Sep0038/Responses/InfoResponse.cs`
- `StellarDotnetSdk/Sep/Sep0038/Responses/AssetInfo.cs`
- `StellarDotnetSdk/Sep/Sep0038/Responses/DeliveryMethod.cs`
- `StellarDotnetSdk/Sep/Sep0038/Responses/PricesResponse.cs`
- `StellarDotnetSdk/Sep/Sep0038/Responses/AssetPrice.cs`
- `StellarDotnetSdk/Sep/Sep0038/Responses/PriceResponse.cs`
- `StellarDotnetSdk/Sep/Sep0038/Responses/QuoteResponse.cs`
- `StellarDotnetSdk/Sep/Sep0038/Responses/QuoteFee.cs`
- `StellarDotnetSdk/Sep/Sep0038/Responses/QuoteFeeDetail.cs`
- `StellarDotnetSdk/Sep/Sep0038/Exceptions/*.cs`

### Key Classes

- **`QuoteService`**: Main service for SEP-38: `InfoAsync`, `PricesAsync`, `PriceAsync`, `PostQuoteAsync`, `GetQuoteAsync`, plus `FromDomainAsync` stellar.toml discovery. Response bodies are capped at 1 MiB and parsed with the hardened `JsonOptions` (duplicate properties rejected)
- **`AssetIdentifier`**: Builds, parses and validates the Asset Identification Format (`stellar:CODE:ISSUER`, `stellar:native`, `iso4217:USD`); converts to and from SDK `Asset` and implicitly to `string`
- **`QuoteContext`**: The `context` parameter: `Sep6`, `Sep24`, `Sep31`
- **`PricesRequest`**: Parameters for `GET /prices` (sell side or buy side)
- **`PriceRequest`**: Parameters for `GET /price`
- **`QuoteRequest`**: Body for `POST /quote` (JWT required)
- **`InfoResponse`**: Response from `GET /info`
- **`AssetInfo`**: Asset entry of `GET /info` with delivery methods and country codes
- **`DeliveryMethod`**: Sell/buy delivery method (`name`, `description`)
- **`PricesResponse`**: Response from `GET /prices` (`buy_assets` or `sell_assets`)
- **`AssetPrice`**: Asset entry of `GET /prices` with price and decimals
- **`PriceResponse`**: Response from `GET /price`
- **`QuoteResponse`**: Firm quote from `POST /quote` and `GET /quote/:id`
- **`QuoteFee`**: Fee object (`total`, `asset`, `details`)
- **`QuoteFeeDetail`**: Fee breakdown entry
- **`QuoteServerException`**: Base exception for SEP-38 errors (`StatusCode`, `Error`, `ResponseBody`, `RetryAfterDelay` from a `Retry-After` header)
- **`BadRequestException`**: HTTP 400
- **`PermissionDeniedException`**: HTTP 403
- **`QuoteServerNotFoundException`**: HTTP 404 (e.g. unknown quote id)
- **`UnexpectedResponseException`**: Other status codes, oversized bodies, or success bodies that fail validation
- **`NoAnchorQuoteServerFoundException`**: stellar.toml has no `ANCHOR_QUOTE_SERVER`

## Coverage by Section

| Section | Coverage | Required Coverage | Implemented | Not Implemented | Total |
|---------|----------|-------------------|-------------|-----------------|-------|
| Asset Fields | 100.0% | 100.0% | 4 | 0 | 4 |
| Asset Identification Format | 100.0% | 100.0% | 2 | 0 | 2 |
| Asset Price Fields | 100.0% | 100.0% | 3 | 0 | 3 |
| Context Values | 100.0% | 100.0% | 3 | 0 | 3 |
| Delivery Method Fields | 100.0% | 100.0% | 2 | 0 | 2 |
| Discovery and Authentication | 100.0% | 100.0% | 3 | 0 | 3 |
| Error Handling | 100.0% | 100.0% | 3 | 0 | 3 |
| Fee Details Fields | 100.0% | 100.0% | 3 | 0 | 3 |
| Fee Fields | 100.0% | 100.0% | 3 | 0 | 3 |
| Get Quote Endpoint | 100.0% | 100.0% | 1 | 0 | 1 |
| Get Quote Request Parameters | 100.0% | 100.0% | 1 | 0 | 1 |
| Info Endpoint | 100.0% | 100.0% | 1 | 0 | 1 |
| Info Response Fields | 100.0% | 100.0% | 1 | 0 | 1 |
| Post Quote Endpoint | 100.0% | 100.0% | 1 | 0 | 1 |
| Post Quote Request Fields | 100.0% | 100.0% | 9 | 0 | 9 |
| Price Endpoint | 100.0% | 100.0% | 1 | 0 | 1 |
| Price Request Parameters | 100.0% | 100.0% | 8 | 0 | 8 |
| Price Response Fields | 100.0% | 100.0% | 5 | 0 | 5 |
| Prices Endpoint | 100.0% | 100.0% | 1 | 0 | 1 |
| Prices Request Parameters | 100.0% | 100.0% | 7 | 0 | 7 |
| Prices Response Fields | 100.0% | 100.0% | 2 | 0 | 2 |
| Quote Response Fields | 100.0% | 100.0% | 11 | 0 | 11 |

## Detailed Field Comparison

### Asset Fields

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `asset` | ✓ | ✅ | `AssetInfo.Asset` | Asset in Asset Identification Format |
| `buy_delivery_methods` |   | ✅ | `AssetInfo.BuyDeliveryMethods` | Methods to buy/receive this off-chain asset from the anchor |
| `country_codes` |   | ✅ | `AssetInfo.CountryCodes` | ISO 3166-2 / ISO 3166-1 alpha-2 codes where the anchor operates (fiat) |
| `sell_delivery_methods` |   | ✅ | `AssetInfo.SellDeliveryMethods` | Methods to sell/deliver this off-chain asset to the anchor |

### Asset Identification Format

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `iso4217` | ✓ | ✅ | `AssetIdentifier.Iso4217` | Fiat currency scheme, `iso4217:USD` |
| `stellar` | ✓ | ✅ | `AssetIdentifier.Stellar / StellarNative / FromAsset` | Stellar asset scheme, `stellar:CODE:ISSUER` or `stellar:native` (alphanumeric codes of 1-12 characters, as the network accepts them). Request and response asset fields are plain strings, so any value an anchor lists passes through unchanged |

### Asset Price Fields

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `asset` | ✓ | ✅ | `AssetPrice.Asset` | Asset in Asset Identification Format |
| `decimals` | ✓ | ✅ | `AssetPrice.Decimals` | Number of decimals needed to represent the asset; a negative count is rejected |
| `price` | ✓ | ✅ | `AssetPrice.Price` | Indicative price of one unit of `asset` in terms of the requested asset (exact `decimal`) |

### Context Values

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `sep24` |   | ✅ | `QuoteContext.Sep24` | SEP-24 context — `POST /quote` only; `GET /price` rejects it client-side per spec |
| `sep31` | ✓ | ✅ | `QuoteContext.Sep31` | SEP-31 context |
| `sep6` | ✓ | ✅ | `QuoteContext.Sep6` | SEP-6 context |

### Delivery Method Fields

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `description` | ✓ | ✅ | `DeliveryMethod.Description` | Human-readable description of the method |
| `name` | ✓ | ✅ | `DeliveryMethod.Name` | Value to pass as `sell_delivery_method` / `buy_delivery_method` |

### Discovery and Authentication

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `ANCHOR_QUOTE_SERVER` | ✓ | ✅ | `QuoteService.FromDomainAsync` | Quote server URL discovered from stellar.toml via SEP-1 |
| `Authorization (optional)` |   | ✅ | `InfoAsync(jwt) / PricesRequest.Jwt / PriceRequest.Jwt` | Optional SEP-10/SEP-45 bearer JWT for `GET /info`, `/prices`, `/price` |
| `Authorization (required)` | ✓ | ✅ | `QuoteRequest.Jwt / GetQuoteAsync(jwt)` | Mandatory SEP-10/SEP-45 bearer JWT for `POST /quote` and `GET /quote/:id` |

### Error Handling

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `400 Bad Request` | ✓ | ✅ | `BadRequestException` | Invalid request |
| `403 Permission Denied` | ✓ | ✅ | `PermissionDeniedException` | Missing or rejected JWT |
| `error` | ✓ | ✅ | `QuoteServerException.Error` | Human-readable error description in the response body |

_Note: these rows are the specification's Errors section: its status table names only `400` and `403`, and the `error` row is the field its error body must carry. The client also maps `404 Not Found` to `QuoteServerNotFoundException`, and exposes a `Retry-After` header on an error response (typically `429` or `503`) as `QuoteServerException.RetryAfterDelay`. Neither is a specification field, so neither is counted in the coverage above._

### Fee Details Fields

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `amount` | ✓ | ✅ | `QuoteFeeDetail.Amount` | Amount of this fee component (exact `decimal`) |
| `description` |   | ✅ | `QuoteFeeDetail.Description` | Description of the fee component |
| `name` | ✓ | ✅ | `QuoteFeeDetail.Name` | Name of the fee component |

### Fee Fields

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `asset` | ✓ | ✅ | `QuoteFee.Asset` | Asset in which the fee is applied |
| `details` |   | ✅ | `QuoteFee.Details` | Breakdown of the fee |
| `total` | ✓ | ✅ | `QuoteFee.Total` | Total fee (exact `decimal`) |

### Get Quote Endpoint

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `get_quote_endpoint` | ✓ | ✅ | `GetQuoteAsync` | GET /quote/:id - Fetch a previously-provided firm quote |

### Get Quote Request Parameters

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `id` | ✓ | ✅ | `GetQuoteAsync(quoteId)` | Quote id, escaped as a single path segment (`.` and `..` rejected) |

### Info Endpoint

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `info_endpoint` | ✓ | ✅ | `InfoAsync` | GET /info - Supported Stellar and off-chain assets |

### Info Response Fields

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `assets` | ✓ | ✅ | `InfoResponse.Assets` | Assets available for trading |

### Post Quote Endpoint

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `post_quote_endpoint` | ✓ | ✅ | `PostQuoteAsync` | POST /quote - Request a firm quote |

### Post Quote Request Fields

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `buy_amount` |   | ✅ | `QuoteRequest.BuyAmount` | Amount of `buy_asset` (exactly one of `sell_amount`/`buy_amount`, validated client-side) |
| `buy_asset` | ✓ | ✅ | `QuoteRequest.BuyAsset` | Asset to buy |
| `buy_delivery_method` |   | ✅ | `QuoteRequest.BuyDeliveryMethod` | Delivery method for receiving an off-chain asset (at most one delivery method, validated client-side; see [Protocol Rules](#protocol-rules) for when one is required) |
| `context` | ✓ | ✅ | `QuoteRequest.Context` | `sep6`, `sep24` or `sep31` |
| `country_code` |   | ✅ | `QuoteRequest.CountryCode` | ISO 3166-2 / ISO 3166-1 alpha-2 code of the user |
| `expire_after` |   | ✅ | `QuoteRequest.ExpireAfter` | Desired expiry, sent as UTC ISO 8601 |
| `sell_amount` |   | ✅ | `QuoteRequest.SellAmount` | Amount of `sell_asset` (exactly one of `sell_amount`/`buy_amount`, validated client-side) |
| `sell_asset` | ✓ | ✅ | `QuoteRequest.SellAsset` | Asset to sell |
| `sell_delivery_method` |   | ✅ | `QuoteRequest.SellDeliveryMethod` | Delivery method for delivering an off-chain asset |

### Price Endpoint

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `price_endpoint` | ✓ | ✅ | `PriceAsync` | GET /price - Indicative price for one asset pair |

### Price Request Parameters

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `buy_amount` |   | ✅ | `PriceRequest.BuyAmount` | Amount of `buy_asset` (exactly one of `sell_amount`/`buy_amount`, validated client-side) |
| `buy_asset` | ✓ | ✅ | `PriceRequest.BuyAsset` | Asset to buy |
| `buy_delivery_method` |   | ✅ | `PriceRequest.BuyDeliveryMethod` | Delivery method for receiving an off-chain asset |
| `context` | ✓ | ✅ | `PriceRequest.Context` | `sep6` or `sep31` |
| `country_code` |   | ✅ | `PriceRequest.CountryCode` | ISO 3166-2 / ISO 3166-1 alpha-2 code of the user |
| `sell_amount` |   | ✅ | `PriceRequest.SellAmount` | Amount of `sell_asset` (exactly one of `sell_amount`/`buy_amount`, validated client-side) |
| `sell_asset` | ✓ | ✅ | `PriceRequest.SellAsset` | Asset to sell |
| `sell_delivery_method` |   | ✅ | `PriceRequest.SellDeliveryMethod` | Delivery method for delivering an off-chain asset |

### Price Response Fields

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `buy_amount` | ✓ | ✅ | `PriceResponse.BuyAmount` | Amount of `buy_asset` the anchor will provide (exact `decimal`) |
| `fee` | ✓ | ✅ | `PriceResponse.Fee` | Fee object |
| `price` | ✓ | ✅ | `PriceResponse.Price` | Conversion price excluding fees (exact `decimal`) |
| `sell_amount` | ✓ | ✅ | `PriceResponse.SellAmount` | Amount of `sell_asset` the anchor will exchange (exact `decimal`) |
| `total_price` | ✓ | ✅ | `PriceResponse.TotalPrice` | Conversion price including fees (exact `decimal`) |

### Prices Endpoint

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `prices_endpoint` | ✓ | ✅ | `PricesAsync` | GET /prices - Indicative prices for all assets exchangeable for one asset |

### Prices Request Parameters

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `buy_amount` |   | ✅ | `PricesRequest.BuyAmount` | Amount of `buy_asset`; required with, and only valid with, `buy_asset` (v2.3.0) |
| `buy_asset` |   | ✅ | `PricesRequest.BuyAsset` | Asset to buy; exactly one of `sell_asset`/`buy_asset` (v2.3.0) |
| `buy_delivery_method` |   | ✅ | `PricesRequest.BuyDeliveryMethod` | Delivery method for receiving an off-chain asset |
| `country_code` |   | ✅ | `PricesRequest.CountryCode` | ISO 3166-2 / ISO 3166-1 alpha-2 code of the user |
| `sell_amount` |   | ✅ | `PricesRequest.SellAmount` | Amount of `sell_asset`; required with, and only valid with, `sell_asset` |
| `sell_asset` |   | ✅ | `PricesRequest.SellAsset` | Asset to sell; exactly one of `sell_asset`/`buy_asset` |
| `sell_delivery_method` |   | ✅ | `PricesRequest.SellDeliveryMethod` | Delivery method for delivering an off-chain asset |

### Prices Response Fields

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `buy_assets` |   | ✅ | `PricesResponse.BuyAssets` | Prices when `sell_asset` was requested |
| `sell_assets` |   | ✅ | `PricesResponse.SellAssets` | Prices when `buy_asset` was requested (v2.3.0) |

### Quote Response Fields

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `buy_amount` | ✓ | ✅ | `QuoteResponse.BuyAmount` | Amount of `buy_asset` (exact `decimal`) |
| `buy_asset` | ✓ | ✅ | `QuoteResponse.BuyAsset` | Asset bought |
| `buy_delivery_method` |   | ✅ | `QuoteResponse.BuyDeliveryMethod` | Echo of the requested buy delivery method (v2.4.0) |
| `expires_at` | ✓ | ✅ | `QuoteResponse.ExpiresAt` | Expiry; an offset-less value is read as UTC, as the spec defines |
| `fee` | ✓ | ✅ | `QuoteResponse.Fee` | Fee object |
| `id` | ✓ | ✅ | `QuoteResponse.Id` | Quote id, used as `quote_id` in SEP-6/24/31; on `GET /quote/:id`, checked against the requested id |
| `price` | ✓ | ✅ | `QuoteResponse.Price` | Conversion price excluding fees (exact `decimal`) |
| `sell_amount` | ✓ | ✅ | `QuoteResponse.SellAmount` | Amount of `sell_asset` (exact `decimal`) |
| `sell_asset` | ✓ | ✅ | `QuoteResponse.SellAsset` | Asset sold |
| `sell_delivery_method` |   | ✅ | `QuoteResponse.SellDeliveryMethod` | Echo of the requested sell delivery method (v2.4.0) |
| `total_price` | ✓ | ✅ | `QuoteResponse.TotalPrice` | Conversion price including fees (exact `decimal`); required on `POST /quote`, optional on `GET /quote/:id`, whose response table omits it |

## Protocol Rules

Rules the specification states in prose rather than as fields. Not counted in the field coverage above.

| Rule | Spec | Status | Handling |
|------|------|--------|----------|
| Exactly one of `sell_amount`/`buy_amount` (`GET /price`, `POST /quote`) | MUST | ✅ | `ArgumentException` before sending |
| One `GET /prices` side; its amount only with its asset | MUST | ✅ | `ArgumentException` before sending |
| `GET /prices` answers the sell side with `buy_assets` and the buy side with `sell_assets` | Response shape | ✅ | `PricesAsync` rejects a response without the list the request calls for |
| `GET /price` context is `sep6` or `sep31` | MUST | ✅ | `ArgumentException` before sending |
| At most one delivery method on `POST /quote` | MUST | ✅ | `ArgumentException` before sending |
| A delivery method must be given for the off-chain asset unless `GET /info` lists at most one | MUST (conditional) | ➖ Caller | Depends on the anchor's `GET /info` answer, which `PostQuoteAsync` does not have; documented on `QuoteRequest`, and the anchor rejects a request that omits it |
| `GET /quote/:id` returns the requested `id` | Response shape | ✅ | Mismatch raises `UnexpectedResponseException` |
| JWT on `POST /quote` and `GET /quote/:id`; optional elsewhere | MUST | ✅ | `ArgumentException` when missing on the two quote endpoints |
| `expire_after` / `expires_at` are UTC ISO 8601 | Format | ✅ | Sent with `Z`; an offset-less `expires_at` is read as UTC |
| Amounts and prices are strings | Format | ✅ | Sent as invariant strings; read exactly (plain or exponent form), never rounded |
| Firm quote held in reserve until `expires_at`; quotes referenced elsewhere stay available past it | Server | ⚙️ | Anchor behaviour |
| `price * buy_amount = sell_amount`; fee details sum to the total | Server | ⚙️ | Anchor arithmetic (SHOULD); values are returned exactly so a caller can check them |
| KYC before quoting; the anchor may deny quotes | Server | ⚙️ | Anchor behaviour; surfaces as `PermissionDeniedException` (403) |

## Implementation Gaps

🎉 **No gaps found!** All fields are implemented.

## Recommendations

✅ The SDK has full compatibility with SEP-0038!

## Legend

- ✅ **Implemented**: Field is implemented in SDK
- ❌ **Not Implemented**: Field is missing from SDK
- ⚙️ **Server**: Server-side only feature (not applicable to client SDKs)
- ➖ **Caller**: Client-side rule the SDK cannot check on its own; documented for the caller
- ✓ **Required**: Field is required by SEP specification
- (blank) **Optional**: Field is optional
