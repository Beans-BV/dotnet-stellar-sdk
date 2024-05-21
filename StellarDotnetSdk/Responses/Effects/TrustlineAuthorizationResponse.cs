using System;
using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

public class TrustlineAuthorizationResponse : EffectResponse
{
    [JsonPropertyName("trustor")]
    public string Trustor { get; init; }

    [JsonPropertyName("asset_type")]
    public string AssetType { get; init; }

    [JsonPropertyName("asset_code")]
    public string AssetCode { get; init; }
}

/// <summary>
///     Represents trustline_deauthorized effect response.
/// </summary>
[Obsolete("Deprecated in favor of 'TrustlineFlagsUpdatedEffectResponse'")]
public class TrustlineDeauthorizedEffectResponse : TrustlineAuthorizationResponse
{
    public override int TypeId => 24;
}

[Obsolete("Deprecated in favor of 'TrustlineFlagsUpdatedEffectResponse'")]
public class TrustlineAuthorizedToMaintainLiabilitiesEffectResponse : TrustlineAuthorizationResponse
{
    public override int TypeId => 25;
}

/// <summary>
///     Represents trustline_authorized effect response.
/// </summary>
[Obsolete("Deprecated in favor of 'TrustlineFlagsUpdatedEffectResponse'")]
public class TrustlineAuthorizedEffectResponse : TrustlineAuthorizationResponse
{
    public override int TypeId => 23;
}