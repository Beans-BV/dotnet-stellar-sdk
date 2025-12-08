using System;
using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Base class for trustline authorization effect responses.
/// </summary>
[Obsolete("Deprecated in favor of 'TrustlineFlagsUpdatedEffectResponse'")]
public abstract class TrustlineAuthorizationResponse : EffectResponse
{
    /// <summary>
    ///     The account ID of the trustor whose trustline was modified.
    /// </summary>
    [JsonPropertyName("trustor")]
    public string? Trustor { get; init; }

    /// <summary>
    ///     The type of the trusted asset.
    /// </summary>
    [JsonPropertyName("asset_type")]
    public string? AssetType { get; init; }

    /// <summary>
    ///     The code of the trusted asset.
    /// </summary>
    [JsonPropertyName("asset_code")]
    public string? AssetCode { get; init; }
}

/// <summary>
///     Represents the trustline_deauthorized effect response.
///     This effect occurs when a trustline is deauthorized.
/// </summary>
[Obsolete("Deprecated in favor of 'TrustlineFlagsUpdatedEffectResponse'")]
public sealed class TrustlineDeauthorizedEffectResponse : TrustlineAuthorizationResponse
{
    /// <inheritdoc />
    public override int TypeId => 24;
}

/// <summary>
///     Represents the trustline_authorized_to_maintain_liabilities effect response.
///     This effect occurs when a trustline is authorized to maintain liabilities only.
/// </summary>
[Obsolete("Deprecated in favor of 'TrustlineFlagsUpdatedEffectResponse'")]
public sealed class TrustlineAuthorizedToMaintainLiabilitiesEffectResponse : TrustlineAuthorizationResponse
{
    /// <inheritdoc />
    public override int TypeId => 25;
}

/// <summary>
///     Represents the trustline_authorized effect response.
///     This effect occurs when a trustline is fully authorized.
/// </summary>
[Obsolete("Deprecated in favor of 'TrustlineFlagsUpdatedEffectResponse'")]
public sealed class TrustlineAuthorizedEffectResponse : TrustlineAuthorizationResponse
{
    /// <inheritdoc />
    public override int TypeId => 23;
}