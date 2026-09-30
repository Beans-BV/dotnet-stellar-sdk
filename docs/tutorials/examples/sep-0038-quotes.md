# SEP-0038: Anchor RFQ (quotes)

This guide shows how to get prices and firm quotes from an anchor with the SEP-0038 client in `StellarDotnetSdk.Sep.Sep0038`. A firm quote's `Id` is what you pass as `quote_id` to a SEP-6, SEP-24 or SEP-31 transaction when the user exchanges one asset for another (for example USD cash for USDC).

The examples run against the Stellar test anchor, `testanchor.stellar.org`, which exchanges `iso4217:USD` (delivered by `WIRE`, country `US`) for testnet USDC.

## Connecting to the quote server

The anchor declares its quote server as `ANCHOR_QUOTE_SERVER` in its stellar.toml:

```csharp
using System.Net.Http;
using StellarDotnetSdk.Sep.Sep0038;

// Reuse one HttpClient across the application; its Timeout bounds every quote call, body included.
var httpClient = new HttpClient();
var quotes = await QuoteService.FromDomainAsync("testanchor.stellar.org", httpClient: httpClient);
```

`FromDomainAsync` throws `NoAnchorQuoteServerFoundException` when the anchor does not advertise SEP-38, or advertises an address the client refuses (anything but `https`, or a URL with a query, fragment or user information).

## Authenticating

`GET /info`, `GET /prices` and `GET /price` work without a token; the anchor may personalize them when one is sent. `POST /quote` and `GET /quote/:id` require one. Use the JWT from SEP-10 (classic `G...`/`M...` accounts) or SEP-45 (contract accounts):

```csharp
using StellarDotnetSdk;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Sep.Sep0010;

var userKeyPair = KeyPair.FromSecretSeed(userSecretSeed);
var webAuth = await ClientWebAuth.FromDomainAsync("testanchor.stellar.org", Network.Test(), httpClient: httpClient);
var jwt = await webAuth.JwtTokenAsync(userKeyPair.AccountId, new[] { userKeyPair });
```

## Assets

SEP-38 identifies assets as `stellar:CODE:ISSUER` (or `stellar:native`) and `iso4217:CODE`. `AssetIdentifier` builds, validates and parses these, and converts implicitly to the `string` the request types take:

```csharp
var usdc = AssetIdentifier.Stellar("USDC", "GBBD47IF6LWK7P7MDEVSCWR7DPUWV3NY3DTQEVFL4NAT4AQH3ZLLFLA5");
var usd = AssetIdentifier.Iso4217("USD");

var parsed = AssetIdentifier.Parse("stellar:USDC:GBBD47IF6LWK7P7MDEVSCWR7DPUWV3NY3DTQEVFL4NAT4AQH3ZLLFLA5");
var sdkAsset = parsed.ToAsset();
```

## What the anchor offers

```csharp
var info = await quotes.InfoAsync();
foreach (var asset in info.Assets)
{
    Console.WriteLine(asset.Asset);
    foreach (var method in asset.SellDeliveryMethods ?? [])
    {
        Console.WriteLine($"  deliver via {method.Name}: {method.Description}");
    }
}
```

## Indicative prices

`GET /prices` lists prices for every asset exchangeable for one asset. Give either the sell side or the buy side, not both; the response carries `BuyAssets` for a sell-side request and `SellAssets` for a buy-side one:

```csharp
using StellarDotnetSdk.Sep.Sep0038.Requests;

var prices = await quotes.PricesAsync(new PricesRequest
{
    SellAsset = usd,
    SellAmount = 100m,
    SellDeliveryMethod = "WIRE",
    CountryCode = "US",
});

foreach (var offer in prices.BuyAssets!)
{
    // The price of one unit of offer.Asset, in USD.
    Console.WriteLine($"{offer.Asset}: {offer.Price} ({offer.Decimals} decimals)");
}
```

`GET /price` prices one pair, including the fee. Give exactly one of `SellAmount` or `BuyAmount`; the context is `Sep6` or `Sep31`:

```csharp
var price = await quotes.PriceAsync(new PriceRequest
{
    Context = QuoteContext.Sep6,
    SellAsset = usd,
    BuyAsset = usdc,
    SellAmount = 100m,
    SellDeliveryMethod = "WIRE",
    CountryCode = "US",
});

Console.WriteLine(
    $"{price.SellAmount} USD -> {price.BuyAmount} USDC at {price.TotalPrice} (fee {price.Fee.Total} {price.Fee.Asset})");
```

## Firm quotes

When `GET /info` lists more than one delivery method for the off-chain asset, SEP-38 requires the client to name the one the user chose; the anchor enforces it, so pass it:

```csharp
var quote = await quotes.PostQuoteAsync(new QuoteRequest
{
    Context = QuoteContext.Sep6,
    SellAsset = usd,
    BuyAsset = usdc,
    SellAmount = 100m,
    SellDeliveryMethod = "WIRE",
    CountryCode = "US",
    ExpireAfter = DateTimeOffset.UtcNow.AddMinutes(10),
    Jwt = jwt,
});

Console.WriteLine($"Quote {quote.Id} expires at {quote.ExpiresAt:u}");

// Later, for example to show the user what they agreed to:
var sameQuote = await quotes.GetQuoteAsync(quote.Id, jwt);
```

Pass `quote.Id` as `QuoteId` of a SEP-24 `DepositRequest` or `WithdrawRequest`, or of a SEP-6 `DepositExchangeRequest` or `WithdrawExchangeRequest`.

`POST /quote` is not idempotent: each call reserves a new firm quote. Do not configure the service with `HttpResilienceOptionsPresets.ForHorizon()` or `ForSoroban()`, which retry `POST` on a 408, 429 or 5xx answer; see [HTTP retries](http-retry.md).

## Amounts and errors

Amounts and prices are `decimal`. They are sent as invariant-culture strings and read exactly: a value that `decimal` cannot represent without rounding fails with `UnexpectedResponseException` instead of being approximated.

Request rules the specification states (one amount, one `GET /prices` side, at most one delivery method on `POST /quote`) throw `ArgumentException` before anything is sent. Anchor errors map to exceptions derived from `QuoteServerException`; transport failures surface as `HttpRequestException`, and timeouts as `TaskCanceledException`:

```csharp
using StellarDotnetSdk.Sep.Sep0038.Exceptions;

try
{
    await quotes.GetQuoteAsync(quote.Id, jwt);
}
catch (NotFoundException)
{
    // 404: no such quote for this account (anchors also answer 404 for an unknown asset on GET /price).
}
catch (QuoteServerException ex)
{
    // BadRequestException (400), PermissionDeniedException (403), UnexpectedResponseException (anything else).
    // ex.Error is the anchor's message, exactly as sent: escape it before logging or display. ex.Message carries
    // an escaped, length-bounded copy. ex.RetryAfterDelay is the anchor's Retry-After (typically on 429 or 503).
    Console.WriteLine($"{ex.StatusCode}: {ex.Message}");
}
```
