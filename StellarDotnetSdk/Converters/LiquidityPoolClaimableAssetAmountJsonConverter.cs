using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Responses.Effects;

namespace StellarDotnetSdk.Converters;

/// <summary>
///     JSON converter for LiquidityPoolClaimableAssetAmount.
///     Handles conversion between JSON objects and LiquidityPoolClaimableAssetAmount instances.
/// </summary>
/// <remarks>
///     Duplicate JSON property names are always rejected with a <see cref="JsonException" />, matched
///     case-insensitively. This is intentional hardening for financial fields and does not honor the
///     <see cref="JsonSerializerOptions" /> passed to <see cref="Read" /> — neither
///     <see cref="JsonSerializerOptions.AllowDuplicateProperties" /> nor
///     <see cref="JsonSerializerOptions.PropertyNameCaseInsensitive" /> changes it.
/// </remarks>
public class LiquidityPoolClaimableAssetAmountJsonConverter : JsonConverter<LiquidityPoolClaimableAssetAmount>
{
    /// <inheritdoc />
    public override LiquidityPoolClaimableAssetAmount Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        // LiquidityPoolClaimableAssetAmount is non-nullable, only check for expected token type
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException(
                $"Expected StartObject for {nameof(LiquidityPoolClaimableAssetAmount)} but found {reader.TokenType}. " +
                "LiquidityPoolClaimableAssetAmount must be a JSON object with 'asset', 'amount', and 'claimable_balance_id' properties."
            );
        }

        using var jsonDocument = JsonDocument.ParseValue(ref reader);
        var jsonObject = jsonDocument.RootElement;
        JsonDuplicatePropertyGuard.EnsureNoDuplicateProperties(jsonObject,
            nameof(LiquidityPoolClaimableAssetAmount));

        if (!jsonObject.TryGetProperty("asset", out var assetElement))
        {
            throw new JsonException($"JSON value for asset is missing in {nameof(LiquidityPoolClaimableAssetAmount)}.");
        }
        var assetName = assetElement.GetString();
        if (string.IsNullOrEmpty(assetName))
        {
            throw new JsonException($"JSON value for asset is missing in {nameof(LiquidityPoolClaimableAssetAmount)}.");
        }

        if (!jsonObject.TryGetProperty("amount", out var amountElement))
        {
            throw new JsonException($"JSON value for amount is missing in {nameof(LiquidityPoolClaimableAssetAmount)}.");
        }
        var amount = amountElement.GetString();
        if (string.IsNullOrEmpty(amount))
        {
            throw new JsonException($"JSON value for amount is missing in {nameof(LiquidityPoolClaimableAssetAmount)}.");
        }

        // claimable_balance_id is optional
        string? claimableBalanceId = null;
        if (jsonObject.TryGetProperty("claimable_balance_id", out var claimableBalanceIdElement))
        {
            claimableBalanceId = claimableBalanceIdElement.GetString();
        }

        return new LiquidityPoolClaimableAssetAmount
        {
            Asset = AssetJsonReadHelper.CreateAsset(assetName),
            Amount = amount,
            ClaimableBalanceId = claimableBalanceId,
        };
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, LiquidityPoolClaimableAssetAmount value,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        if (value.Asset != null)
        {
            writer.WriteString("asset", value.Asset.CanonicalName());
        }
        if (value.Amount != null)
        {
            writer.WriteString("amount", value.Amount);
        }
        if (value.ClaimableBalanceId != null)
        {
            writer.WriteString("claimable_balance_id", value.ClaimableBalanceId);
        }
        writer.WriteEndObject();
    }
}