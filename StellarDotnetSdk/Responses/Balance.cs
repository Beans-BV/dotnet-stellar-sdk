using Newtonsoft.Json;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses;
#nullable disable
/// <summary>
///     Represents account balance.
/// </summary>
public class Balance
{
    private const string LiquidityPoolSharesAssetType = "liquidity_pool_shares";

    [JsonProperty(PropertyName = "asset_type")]
    public string AssetType { get; init; }

    [JsonProperty(PropertyName = "limit")] public string Limit { get; init; }

    [JsonProperty(PropertyName = "balance")]
    public string BalanceString { get; init; }
#nullable restore

    [JsonProperty(PropertyName = "asset_code")]
    public string? AssetCode { get; init; }

    [JsonProperty(PropertyName = "asset_issuer")]
    public string? AssetIssuer { get; init; }

    [JsonIgnore]
    public Asset? Asset => AssetType != LiquidityPoolSharesAssetType
        ? Asset.Create(AssetType, AssetCode, AssetIssuer)
        : null;


    [JsonProperty(PropertyName = "buying_liabilities")]
    public string? BuyingLiabilities { get; init; }

    [JsonProperty(PropertyName = "selling_liabilities")]
    public string? SellingLiabilities { get; init; }

    [JsonProperty(PropertyName = "is_authorized")]
    public bool IsAuthorized { get; init; }

    [JsonProperty(PropertyName = "is_authorized_to_maintain_liabilities")]
    public bool IsAuthorizedToMaintainLiabilities { get; init; }

    [JsonProperty(PropertyName = "liquidity_pool_id")]
    public string? LiquidityPoolId { get; init; }
}