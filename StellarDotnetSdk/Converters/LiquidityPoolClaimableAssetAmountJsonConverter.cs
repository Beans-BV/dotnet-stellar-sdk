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
public class LiquidityPoolClaimableAssetAmountJsonConverter : JsonConverter<LiquidityPoolClaimableAssetAmount>
{
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

        using (var jsonDocument = JsonDocument.ParseValue(ref reader))
        {
            var jsonObject = jsonDocument.RootElement;
            var assetName = jsonObject.GetProperty("asset").GetString();
            var asset = string.IsNullOrEmpty(assetName) ? null : Asset.Create(assetName);

            var amount = jsonObject.GetProperty("amount").GetString();
            var claimableBalanceId = jsonObject.GetProperty("claimable_balance_id").GetString();

            if (asset == null)
            {
                throw new ArgumentException("JSON value for asset is missing.", nameof(asset));
            }
            if (amount == null)
            {
                throw new ArgumentException("JSON value for amount is missing.", nameof(amount));
            }

            return new LiquidityPoolClaimableAssetAmount
            {
                Asset = asset,
                Amount = amount,
                ClaimableBalanceId = claimableBalanceId,
            };
        }
    }

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