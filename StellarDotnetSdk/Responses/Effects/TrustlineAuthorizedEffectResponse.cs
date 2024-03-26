using System;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents trustline_authorized effect response.
///     See: https://www.stellar.org/developers/horizon/reference/resources/effect.html
///     <seealso cref="Requests.EffectsRequestBuilder" />
///     <seealso cref="Server" />
/// </summary>
[Obsolete("Deprecated in favor of 'TrustlineFlagsUpdatedEffectResponse'")]
public class TrustlineAuthorizedEffectResponse : TrustlineAuthorizationResponse
{
    public TrustlineAuthorizedEffectResponse()
    {
    }

    /// <inheritdoc />
    public TrustlineAuthorizedEffectResponse(string trustor, string assetType, string assetCode)
        : base(trustor, assetType, assetCode)
    {
    }

    public override int TypeId => 23;
}