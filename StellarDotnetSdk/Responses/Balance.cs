using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses;
#nullable disable
/// <summary>
///     Represents account balance.
/// </summary>
public class Balance
{
    private const string LiquidityPoolSharesAssetType = "liquidity_pool_shares";

    [JsonPropertyName("asset_type")]
    public string AssetType { get; init; }

    [JsonPropertyName("limit")]
    public string Limit { get; init; }

    [JsonPropertyName("balance")]
    public string BalanceString { get; init; }
#nullable restore

    [JsonPropertyName("asset_code")]
    public string? AssetCode { get; init; }

    [JsonPropertyName("asset_issuer")]
    public string? AssetIssuer { get; init; }

    [JsonIgnore]
    public Asset? Asset => AssetType != LiquidityPoolSharesAssetType
        ? Asset.Create(AssetType, AssetCode, AssetIssuer)
        : null;


    [JsonPropertyName("buying_liabilities")]
    public string? BuyingLiabilities { get; init; }

    [JsonPropertyName("selling_liabilities")]
    public string? SellingLiabilities { get; init; }

    [JsonPropertyName("is_authorized")]
    public bool IsAuthorized { get; init; }

    [JsonPropertyName("is_authorized_to_maintain_liabilities")]
    public bool IsAuthorizedToMaintainLiabilities { get; init; }

    [JsonPropertyName("liquidity_pool_id")]
    public string? LiquidityPoolId { get; init; }
}