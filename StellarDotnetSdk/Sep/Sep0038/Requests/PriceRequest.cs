using System;
using System.Collections.Generic;
using System.Text;

namespace StellarDotnetSdk.Sep.Sep0038.Requests;

/// <summary>
///     Parameters of SEP-38 <c>GET /price</c>, which returns the indicative price for one asset pair.
/// </summary>
/// <remarks>
///     Provide exactly one of <see cref="SellAmount" /> or <see cref="BuyAmount" />. <see cref="Context" /> must be
///     <see cref="QuoteContext.Sep6" /> or <see cref="QuoteContext.Sep31" />. Both rules are checked before the
///     request is sent.
/// </remarks>
public sealed record PriceRequest
{
    /// <summary>
    ///     What the price will be used for: <see cref="QuoteContext.Sep6" /> or <see cref="QuoteContext.Sep31" />.
    /// </summary>
    public required QuoteContext Context { get; init; }

    /// <summary>
    ///     The asset the client would like to sell, in the SEP-38 Asset Identification Format (see
    ///     <see cref="AssetIdentifier" />).
    /// </summary>
    public required string SellAsset { get; init; }

    /// <summary>
    ///     The asset the client would like to receive for <see cref="SellAsset" />, in the SEP-38 Asset
    ///     Identification Format.
    /// </summary>
    public required string BuyAsset { get; init; }

    /// <summary>
    ///     The amount of <see cref="SellAsset" /> to exchange. Mutually exclusive with <see cref="BuyAmount" />. Must
    ///     be greater than zero.
    /// </summary>
    public decimal? SellAmount { get; init; }

    /// <summary>
    ///     The amount of <see cref="BuyAsset" /> to receive. Mutually exclusive with <see cref="SellAmount" />. Must
    ///     be greater than zero.
    /// </summary>
    public decimal? BuyAmount { get; init; }

    /// <summary>
    ///     Optional: one of the <c>sell_delivery_methods</c> names from <c>GET /info</c>, when the user will deliver
    ///     an off-chain asset to the anchor.
    /// </summary>
    public string? SellDeliveryMethod { get; init; }

    /// <summary>
    ///     Optional: one of the <c>buy_delivery_methods</c> names from <c>GET /info</c>, when the user will receive
    ///     an off-chain asset from the anchor.
    /// </summary>
    public string? BuyDeliveryMethod { get; init; }

    /// <summary>
    ///     Optional ISO 3166-2 or ISO 3166-1 alpha-2 code of the user's current address. Should be provided when
    ///     <c>GET /info</c> lists more than one country code for the asset.
    /// </summary>
    public string? CountryCode { get; init; }

    /// <summary>
    ///     Optional SEP-10 or SEP-45 JWT; when provided the anchor may personalize the price.
    /// </summary>
    public string? Jwt { get; init; }

    // The compiler-generated ToString would print the bearer token; logging a request must not leak it.
    private bool PrintMembers(StringBuilder builder)
    {
        builder.Append("Context = ").Append(Context).Append(", ");
        builder.Append("SellAsset = ").Append(SellAsset).Append(", ");
        builder.Append("BuyAsset = ").Append(BuyAsset).Append(", ");
        builder.Append("SellAmount = ").Append(SellAmount).Append(", ");
        builder.Append("BuyAmount = ").Append(BuyAmount).Append(", ");
        builder.Append("SellDeliveryMethod = ").Append(SellDeliveryMethod).Append(", ");
        builder.Append("BuyDeliveryMethod = ").Append(BuyDeliveryMethod).Append(", ");
        builder.Append("CountryCode = ").Append(CountryCode).Append(", ");
        builder.Append("Jwt = ").Append(RequestValidation.Redact(Jwt));
        return true;
    }

    /// <summary>
    ///     Checks the request and returns the query parameters.
    /// </summary>
    /// <exception cref="ArgumentException">
    ///     Thrown when an asset is missing, when not exactly one amount is given, when <see cref="Context" /> is
    ///     <see cref="QuoteContext.Sep24" />, when an optional string property is empty or whitespace rather than
    ///     null, or when the JWT is not printable ASCII without whitespace.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when <see cref="Context" /> is not a defined value, or the amount is zero or negative.
    /// </exception>
    internal Dictionary<string, string> ToQueryParameters()
    {
        RequestValidation.RequireNonEmpty(SellAsset, nameof(SellAsset));
        RequestValidation.RequireNonEmpty(BuyAsset, nameof(BuyAsset));
        RequestValidation.RequireExactlyOneAmount(SellAmount, BuyAmount);
        var context = RequestValidation.ToWireValue(Context);
        if (Context == QuoteContext.Sep24)
        {
            throw new ArgumentException(
                "GET /price accepts only the sep6 and sep31 contexts; SEP-24 flows use firm quotes (POST /quote).",
                nameof(Context));
        }

        var parameters = new Dictionary<string, string>
        {
            ["sell_asset"] = SellAsset,
            ["buy_asset"] = BuyAsset,
        };
        if (SellAmount.HasValue)
        {
            parameters["sell_amount"] = RequestValidation.FormatAmount(SellAmount.Value, nameof(SellAmount));
        }
        else
        {
            parameters["buy_amount"] = RequestValidation.FormatAmount(BuyAmount!.Value, nameof(BuyAmount));
        }

        RequestValidation.AddOptional(parameters, "sell_delivery_method", SellDeliveryMethod,
            nameof(SellDeliveryMethod));
        RequestValidation.AddOptional(parameters, "buy_delivery_method", BuyDeliveryMethod, nameof(BuyDeliveryMethod));
        RequestValidation.AddOptional(parameters, "country_code", CountryCode, nameof(CountryCode));
        RequestValidation.RequireValidJwt(Jwt, nameof(Jwt), false);
        parameters["context"] = context;
        return parameters;
    }
}
