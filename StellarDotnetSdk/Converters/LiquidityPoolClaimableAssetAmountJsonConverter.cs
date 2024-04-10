using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Responses.Effects;

namespace StellarDotnetSdk.Converters;

public class LiquidityPoolClaimableAssetAmountJsonConverter : JsonConverter<LiquidityPoolClaimableAssetAmount>
{
    public override LiquidityPoolClaimableAssetAmount ReadJson(JsonReader reader, Type objectType,
        LiquidityPoolClaimableAssetAmount? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jt = JToken.ReadFrom(reader);

        var assetName = jt.Value<string>("asset");
        var asset = string.IsNullOrEmpty(assetName) ? null : Asset.Create(assetName);

        var amount = jt.Value<string>("amount");

        var claimableBalanceId = jt.Value<string>("claimable_balance_id");

        if (asset == null) throw new ArgumentException("JSON value for asset is missing.", nameof(asset));

        if (amount == null) throw new ArgumentException("JSON value for amount is missing.", nameof(amount));

        return new LiquidityPoolClaimableAssetAmount
        {
            Asset = asset,
            Amount = amount,
            ClaimableBalanceId = claimableBalanceId
        };
    }

    public override void WriteJson(JsonWriter writer, LiquidityPoolClaimableAssetAmount? value,
        JsonSerializer serializer)
    {
        var jo = new JObject();
        if (value?.Asset != null) jo.Add("asset", value.Asset.CanonicalName());
        if (value?.Amount != null) jo.Add("amount", value.Amount);
        if (value?.ClaimableBalanceId != null) jo.Add("claimable_balance_id", value.ClaimableBalanceId);
        jo.WriteTo(writer);
    }
}