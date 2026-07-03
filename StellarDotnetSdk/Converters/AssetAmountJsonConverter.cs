using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Converters;

/// <summary>
///     JSON converter for AssetAmount.
///     Handles conversion between JSON objects and AssetAmount instances.
/// </summary>
/// <remarks>
///     Duplicate JSON property names are always rejected with a <see cref="JsonException" />, matched
///     case-insensitively. This is intentional hardening for financial fields and does not honor the
///     <see cref="JsonSerializerOptions" /> passed to <see cref="Read" /> — neither
///     <see cref="JsonSerializerOptions.AllowDuplicateProperties" /> nor
///     <see cref="JsonSerializerOptions.PropertyNameCaseInsensitive" /> changes it.
/// </remarks>
public class AssetAmountJsonConverter : JsonConverter<AssetAmount>
{
    /// <inheritdoc />
    public override AssetAmount Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // AssetAmount is non-nullable, only check for expected token type
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException(
                $"Expected StartObject for {nameof(AssetAmount)} but found {reader.TokenType}. " +
                "AssetAmount must be a JSON object with 'asset' and 'amount' properties."
            );
        }

        using var jsonDocument = JsonDocument.ParseValue(ref reader);
        var jsonObject = jsonDocument.RootElement;
        JsonDuplicatePropertyGuard.EnsureNoDuplicateProperties(jsonObject, nameof(AssetAmount));

        if (!jsonObject.TryGetProperty("asset", out var assetElement))
        {
            throw new JsonException($"JSON value for asset is missing in {nameof(AssetAmount)}.");
        }
        var assetName = assetElement.GetString();
        if (string.IsNullOrEmpty(assetName))
        {
            throw new JsonException($"JSON value for asset is missing in {nameof(AssetAmount)}.");
        }

        if (!jsonObject.TryGetProperty("amount", out var amountElement))
        {
            throw new JsonException($"JSON value for amount is missing in {nameof(AssetAmount)}.");
        }
        var amount = amountElement.GetString();
        if (amount == null)
        {
            throw new JsonException($"JSON value for amount is missing in {nameof(AssetAmount)}.");
        }

        return new AssetAmount(AssetJsonReadHelper.CreateAsset(assetName), amount);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, AssetAmount value, JsonSerializerOptions options)
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
        writer.WriteEndObject();
    }
}