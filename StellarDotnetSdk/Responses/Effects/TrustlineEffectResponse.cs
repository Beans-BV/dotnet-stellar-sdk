using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Base class for trustline-related effect responses.
/// </summary>
public abstract class TrustlineEffectResponse : EffectResponse
{
    /// <summary>
    ///     The trust limit for the asset.
    /// </summary>
    [JsonPropertyName("limit")]
    public required string Limit { get; init; }

    /// <summary>
    ///     The type of the trusted asset: "credit_alphanum4", "credit_alphanum12" or "liquidity_pool_shares".
    /// </summary>
    [JsonPropertyName("asset_type")]
    public required string AssetType { get; init; }

    /// <summary>
    ///     The code of the trusted asset. Null for native XLM or "liquidity_pool_shares".
    /// </summary>
    [JsonPropertyName("asset_code")]
    public string? AssetCode { get; init; }

    /// <summary>
    ///     The unique identifier of the liquidity pool, if the asset type is "liquidity pool shares".
    ///     Null for regular asset types.
    /// </summary>
    [JsonPropertyName("liquidity_pool_id")]
    public string? LiquidityPoolId { get; init; }

    /// <summary>
    ///     The issuer of the trusted asset. Null for native XLM or "liquidity_pool_shares".
    /// </summary>
    [JsonPropertyName("asset_issuer")]
    public string? AssetIssuer { get; init; }

    /// <summary>
    ///     The trusted asset.
    /// </summary>
    [JsonIgnore]
    public Asset? Asset => AssetType != "liquidity_pool_shares"
        ? Asset.Create(AssetType, AssetCode, AssetIssuer)
        : null;
}

/// <summary>
///     Represents the trustline_created effect response.
///     This effect occurs when a new trustline is established.
/// </summary>
public sealed class TrustlineCreatedEffectResponse : TrustlineEffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 20;
}

/// <summary>
///     Represents the trustline_removed effect response.
///     This effect occurs when a trustline is removed.
/// </summary>
public sealed class TrustlineRemovedEffectResponse : TrustlineEffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 21;
}

/// <summary>
///     Represents the trustline_updated effect response.
///     This effect occurs when a trustline is updated.
/// </summary>
public sealed class TrustlineUpdatedEffectResponse : TrustlineEffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 22;
}