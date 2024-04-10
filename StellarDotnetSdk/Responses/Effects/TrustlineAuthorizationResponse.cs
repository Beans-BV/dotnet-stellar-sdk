using System;
using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

public class TrustlineAuthorizationResponse : EffectResponse
{
    [JsonProperty(PropertyName = "trustor")]
    public string Trustor { get; init; }

    [JsonProperty(PropertyName = "asset_type")]
    public string AssetType { get; init; }

    [JsonProperty(PropertyName = "asset_code")]
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