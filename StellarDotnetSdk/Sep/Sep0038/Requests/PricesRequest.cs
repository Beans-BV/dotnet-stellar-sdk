using System;
using System.Collections.Generic;
using System.Text;

namespace StellarDotnetSdk.Sep.Sep0038.Requests;

/// <summary>
///     Parameters of SEP-38 <c>GET /prices</c>, which lists indicative prices of the assets available in exchange
///     for one asset.
/// </summary>
/// <remarks>
///     Specify one side only: either <see cref="SellAsset" /> with <see cref="SellAmount" /> (answered with
///     <c>buy_assets</c>) or <see cref="BuyAsset" /> with <see cref="BuyAmount" /> (answered with
///     <c>sell_assets</c>). Mixing the sides is rejected before the request is sent.
/// </remarks>
public sealed record PricesRequest
{
    /// <summary>
    ///     The asset to sell, in the SEP-38 Asset Identification Format (see <see cref="AssetIdentifier" />). Must
    ///     not be combined with <see cref="BuyAsset" />.
    /// </summary>
    public string? SellAsset { get; init; }

    /// <summary>
    ///     The amount of <see cref="SellAsset" /> to exchange for each of the buy assets. Required with, and only
    ///     valid with, <see cref="SellAsset" />. Must be greater than zero.
    /// </summary>
    public decimal? SellAmount { get; init; }

    /// <summary>
    ///     The asset to buy, in the SEP-38 Asset Identification Format (see <see cref="AssetIdentifier" />). Must
    ///     not be combined with <see cref="SellAsset" />.
    /// </summary>
    public string? BuyAsset { get; init; }

    /// <summary>
    ///     The amount of <see cref="BuyAsset" /> to receive for each of the sell assets. Required with, and only
    ///     valid with, <see cref="BuyAsset" />. Must be greater than zero.
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
    ///     Optional SEP-10 or SEP-45 JWT; when provided the anchor may personalize the prices.
    /// </summary>
    public string? Jwt { get; init; }

    // The compiler-generated ToString would print the bearer token; logging a request must not leak it.
    private bool PrintMembers(StringBuilder builder)
    {
        builder.Append("SellAsset = ").Append(SellAsset).Append(", ");
        builder.Append("SellAmount = ").Append(SellAmount).Append(", ");
        builder.Append("BuyAsset = ").Append(BuyAsset).Append(", ");
        builder.Append("BuyAmount = ").Append(BuyAmount).Append(", ");
        builder.Append("SellDeliveryMethod = ").Append(SellDeliveryMethod).Append(", ");
        builder.Append("BuyDeliveryMethod = ").Append(BuyDeliveryMethod).Append(", ");
        builder.Append("CountryCode = ").Append(CountryCode).Append(", ");
        builder.Append("Jwt = ").Append(RequestValidation.Redact(Jwt));
        return true;
    }

    /// <summary>
    ///     Checks the sell/buy exclusivity rules and returns the query parameters.
    /// </summary>
    /// <exception cref="ArgumentException">
    ///     Thrown when the request mixes or omits the sell and buy sides, when a string property is empty or
    ///     whitespace rather than null, or when the JWT is not printable ASCII without whitespace.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the amount is zero or negative.</exception>
    internal Dictionary<string, string> ToQueryParameters()
    {
        var hasSellAsset = RequestValidation.IsProvided(SellAsset, nameof(SellAsset));
        var hasBuyAsset = RequestValidation.IsProvided(BuyAsset, nameof(BuyAsset));
        if (hasSellAsset == hasBuyAsset)
        {
            throw new ArgumentException(
                "Exactly one of SellAsset or BuyAsset must be provided, not both and not neither.",
                hasSellAsset ? nameof(BuyAsset) : nameof(SellAsset));
        }

        var parameters = new Dictionary<string, string>();
        if (hasSellAsset)
        {
            if (!SellAmount.HasValue)
            {
                throw new ArgumentException("SellAmount is required when SellAsset is provided.", nameof(SellAmount));
            }

            if (BuyAmount.HasValue)
            {
                throw new ArgumentException("BuyAmount is only valid with BuyAsset, not with SellAsset.",
                    nameof(BuyAmount));
            }

            parameters["sell_asset"] = SellAsset!;
            parameters["sell_amount"] = RequestValidation.FormatAmount(SellAmount.Value, nameof(SellAmount));
        }
        else
        {
            if (!BuyAmount.HasValue)
            {
                throw new ArgumentException("BuyAmount is required when BuyAsset is provided.", nameof(BuyAmount));
            }

            if (SellAmount.HasValue)
            {
                throw new ArgumentException("SellAmount is only valid with SellAsset, not with BuyAsset.",
                    nameof(SellAmount));
            }

            parameters["buy_asset"] = BuyAsset!;
            parameters["buy_amount"] = RequestValidation.FormatAmount(BuyAmount.Value, nameof(BuyAmount));
        }

        AddOptional(parameters, "sell_delivery_method", SellDeliveryMethod, nameof(SellDeliveryMethod));
        AddOptional(parameters, "buy_delivery_method", BuyDeliveryMethod, nameof(BuyDeliveryMethod));
        AddOptional(parameters, "country_code", CountryCode, nameof(CountryCode));
        RequestValidation.RequireValidJwt(Jwt, nameof(Jwt), false);
        return parameters;
    }

    /// <exception cref="ArgumentException">Thrown when <paramref name="value" /> is empty or whitespace.</exception>
    internal static void AddOptional(Dictionary<string, string> parameters, string name, string? value,
        string propertyName)
    {
        if (RequestValidation.IsProvided(value, propertyName))
        {
            parameters[name] = value!;
        }
    }
}
