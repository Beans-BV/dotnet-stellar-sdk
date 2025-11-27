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
    public string? Limit { get; init; }

    /// <summary>
    ///     The type of the trusted asset: "credit_alphanum4" or "credit_alphanum12".
    /// </summary>
    [JsonPropertyName("asset_type")]
    public string? AssetType { get; init; }

    /// <summary>
    ///     The code of the trusted asset.
    /// </summary>
    [JsonPropertyName("asset_code")]
    public string? AssetCode { get; init; }

    /// <summary>
    ///     The issuer of the trusted asset.
    /// </summary>
    [JsonPropertyName("asset_issuer")]
    public string? AssetIssuer { get; init; }

    /// <summary>
    ///     The trusted asset.
    /// </summary>
    public AssetTypeCreditAlphaNum? Asset =>
        AssetCode != null && AssetIssuer != null
            ? Assets.Asset.CreateNonNativeAsset(AssetCode, AssetIssuer)
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