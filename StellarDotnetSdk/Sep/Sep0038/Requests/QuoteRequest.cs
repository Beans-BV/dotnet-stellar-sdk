using System;
using System.IO;
using System.Text;
using System.Text.Json;
using StellarDotnetSdk.Converters;

namespace StellarDotnetSdk.Sep.Sep0038.Requests;

/// <summary>
///     Body of SEP-38 <c>POST /quote</c>, which requests a firm quote. Requires a SEP-10 or SEP-45 JWT.
/// </summary>
/// <remarks>
///     <para>
///         Provide exactly one of <see cref="SellAmount" /> or <see cref="BuyAmount" />, and at most one of
///         <see cref="SellDeliveryMethod" /> or <see cref="BuyDeliveryMethod" />. Both rules are checked before the
///         request is sent.
///     </para>
///     <para>
///         Unless <c>GET /info</c> lists no delivery methods (or only one) for the off-chain asset, the anchor also
///         requires the delivery method for that asset. That depends on the anchor's <c>GET /info</c> response, so it
///         is left to the anchor to enforce.
///     </para>
/// </remarks>
public sealed record QuoteRequest
{
    /// <summary>
    ///     What the quote will be used for: SEP-6, SEP-24 or SEP-31.
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
    ///     Optional: the desired expiry of the quote. The anchor may choose a later <c>expires_at</c>, and should
    ///     answer <c>400 Bad Request</c> if it cannot offer an expiry on or after this time. Sent as UTC, rounded up
    ///     to the whole second so that an anchor storing whole seconds can honor it exactly; a quote expiring before
    ///     the sent time is rejected.
    /// </summary>
    public DateTimeOffset? ExpireAfter { get; init; }

    /// <summary>
    ///     One of the <c>sell_delivery_methods</c> names from <c>GET /info</c>, when the user will deliver an
    ///     off-chain asset to the anchor. Mutually exclusive with <see cref="BuyDeliveryMethod" />.
    /// </summary>
    public string? SellDeliveryMethod { get; init; }

    /// <summary>
    ///     One of the <c>buy_delivery_methods</c> names from <c>GET /info</c>, when the user will receive an
    ///     off-chain asset from the anchor. Mutually exclusive with <see cref="SellDeliveryMethod" />.
    /// </summary>
    public string? BuyDeliveryMethod { get; init; }

    /// <summary>
    ///     Optional ISO 3166-2 or ISO 3166-1 alpha-2 code of the user's current address. Should be provided when
    ///     <c>GET /info</c> lists more than one country code for the asset.
    /// </summary>
    public string? CountryCode { get; init; }

    /// <summary>
    ///     The SEP-10 or SEP-45 JWT authenticating the request, for example from
    ///     <see cref="Sep0010.ClientWebAuth.JwtTokenAsync" /> or <see cref="Sep0045.ClientWebAuthContract.JwtTokenAsync" />.
    /// </summary>
    public required string Jwt { get; init; }

    // The compiler-generated ToString would print the bearer token; logging a request must not leak it.
    private bool PrintMembers(StringBuilder builder)
    {
        builder.Append("Context = ").Append(Context).Append(", ");
        builder.Append("SellAsset = ").Append(SellAsset).Append(", ");
        builder.Append("BuyAsset = ").Append(BuyAsset).Append(", ");
        builder.Append("SellAmount = ").Append(SellAmount).Append(", ");
        builder.Append("BuyAmount = ").Append(BuyAmount).Append(", ");
        builder.Append("ExpireAfter = ").Append(ExpireAfter).Append(", ");
        builder.Append("SellDeliveryMethod = ").Append(SellDeliveryMethod).Append(", ");
        builder.Append("BuyDeliveryMethod = ").Append(BuyDeliveryMethod).Append(", ");
        builder.Append("CountryCode = ").Append(CountryCode).Append(", ");
        builder.Append("Jwt = ").Append(RequestValidation.Redact(Jwt));
        return true;
    }

    /// <summary>
    ///     Checks the request and returns the UTF-8 JSON body.
    /// </summary>
    /// <exception cref="ArgumentException">
    ///     Thrown when an asset or the JWT is missing, when the JWT is not printable ASCII without whitespace, when
    ///     not exactly one amount is given, when an optional string property is empty or whitespace rather than
    ///     null, or when both delivery methods are given.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when <see cref="Context" /> is not a defined value, or the amount is zero or negative.
    /// </exception>
    internal string ToJson()
    {
        RequestValidation.RequireNonEmpty(SellAsset, nameof(SellAsset));
        RequestValidation.RequireNonEmpty(BuyAsset, nameof(BuyAsset));
        RequestValidation.RequireValidJwt(Jwt, nameof(Jwt), true);
        RequestValidation.RequireExactlyOneAmount(SellAmount, BuyAmount);
        var hasSellDeliveryMethod = RequestValidation.IsProvided(SellDeliveryMethod, nameof(SellDeliveryMethod));
        var hasBuyDeliveryMethod = RequestValidation.IsProvided(BuyDeliveryMethod, nameof(BuyDeliveryMethod));
        RequestValidation.RequireNullOrNonBlank(CountryCode, nameof(CountryCode));
        if (hasSellDeliveryMethod && hasBuyDeliveryMethod)
        {
            throw new ArgumentException(
                "At most one of SellDeliveryMethod or BuyDeliveryMethod may be provided, not both.",
                nameof(BuyDeliveryMethod));
        }

        var context = RequestValidation.ToWireValue(Context);

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WriteString("sell_asset", SellAsset);
            writer.WriteString("buy_asset", BuyAsset);
            // Amounts go out as strings, the form the specification defines, so no JSON number parser on the
            // anchor side can turn them into a binary float.
            if (SellAmount.HasValue)
            {
                writer.WriteString("sell_amount", RequestValidation.FormatAmount(SellAmount.Value, nameof(SellAmount)));
            }
            else
            {
                writer.WriteString("buy_amount", RequestValidation.FormatAmount(BuyAmount!.Value, nameof(BuyAmount)));
            }

            if (SentExpireAfter is { } expireAfter)
            {
                writer.WriteString("expire_after", UtcDateTimeOffsetJsonConverter.Format(expireAfter));
            }

            WriteOptional(writer, "sell_delivery_method", SellDeliveryMethod);
            WriteOptional(writer, "buy_delivery_method", BuyDeliveryMethod);
            WriteOptional(writer, "country_code", CountryCode);
            writer.WriteString("context", context);
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length);
    }

    /// <summary>
    ///     <see cref="ExpireAfter" /> as sent, in UTC and rounded up to the whole second, or <see langword="null" />.
    /// </summary>
    internal DateTimeOffset? SentExpireAfter
    {
        get
        {
            if (ExpireAfter is not { } expireAfter)
            {
                return null;
            }

            // Rounded in UTC: rounding the caller's clock time could overflow it while the instant still fits.
            var utc = expireAfter.ToUniversalTime();
            var shortfall = (TimeSpan.TicksPerSecond - utc.UtcTicks % TimeSpan.TicksPerSecond) % TimeSpan.TicksPerSecond;
            // The last second of year 9999 has no next whole second, so it goes out unrounded.
            return utc.UtcTicks > DateTimeOffset.MaxValue.UtcTicks - shortfall ? utc : utc.AddTicks(shortfall);
        }
    }

    private static void WriteOptional(Utf8JsonWriter writer, string name, string? value)
    {
        // Blank values were rejected above, so null is the only absent form left.
        if (value != null)
        {
            writer.WriteString(name, value);
        }
    }
}
