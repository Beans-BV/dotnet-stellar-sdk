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
    ///     The amount of <see cref="SellAsset" /> to exchange. Mutually exclusive with <see cref="BuyAmount" />.
    /// </summary>
    public decimal? SellAmount { get; init; }

    /// <summary>
    ///     The amount of <see cref="BuyAsset" /> to receive. Mutually exclusive with <see cref="SellAmount" />.
    /// </summary>
    public decimal? BuyAmount { get; init; }

    /// <summary>
    ///     Optional: the desired expiry of the quote. The anchor may choose a later <c>expires_at</c>, and should
    ///     answer <c>400 Bad Request</c> if it cannot offer an expiry on or after this time. Sent as UTC.
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
    ///     Thrown when an asset or the JWT is missing, when not exactly one amount is given, or when both delivery
    ///     methods are given.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <see cref="Context" /> is not a defined value.</exception>
    internal string ToJson()
    {
        RequestValidation.RequireNonEmpty(SellAsset, nameof(SellAsset));
        RequestValidation.RequireNonEmpty(BuyAsset, nameof(BuyAsset));
        RequestValidation.RequireNonEmpty(Jwt, nameof(Jwt));
        RequestValidation.RequireExactlyOneAmount(SellAmount, BuyAmount);
        if (!string.IsNullOrWhiteSpace(SellDeliveryMethod) && !string.IsNullOrWhiteSpace(BuyDeliveryMethod))
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
                writer.WriteString("sell_amount", RequestValidation.FormatAmount(SellAmount.Value));
            }
            else
            {
                writer.WriteString("buy_amount", RequestValidation.FormatAmount(BuyAmount!.Value));
            }

            if (ExpireAfter.HasValue)
            {
                writer.WriteString("expire_after", UtcDateTimeOffsetJsonConverter.Format(ExpireAfter.Value));
            }

            WriteOptional(writer, "sell_delivery_method", SellDeliveryMethod);
            WriteOptional(writer, "buy_delivery_method", BuyDeliveryMethod);
            WriteOptional(writer, "country_code", CountryCode);
            writer.WriteString("context", context);
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length);
    }

    private static void WriteOptional(Utf8JsonWriter writer, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            writer.WriteString(name, value);
        }
    }
}
