using System;

namespace StellarDotnetSdk.Responses.Effects;

[Obsolete("Deprecated in favor of 'TrustlineFlagsUpdatedEffectResponse'")]
public class TrustlineAuthorizedToMaintainLiabilitiesEffectResponse : TrustlineAuthorizationResponse
{
    public TrustlineAuthorizedToMaintainLiabilitiesEffectResponse()
    {
    }

    public TrustlineAuthorizedToMaintainLiabilitiesEffectResponse(string trustor, string assetType, string assetCode)
        : base(trustor, assetType, assetCode)
    {
    }

    public override int TypeId => 25;
}