namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents trustline_updated effect response.
///     See: https://www.stellar.org/developers/horizon/reference/resources/effect.html
///     <seealso cref="Requests.EffectsRequestBuilder" />
///     <seealso cref="Server" />
/// </summary>
public class TrustlineUpdatedEffectResponse : TrustlineCUDResponse
{
    public TrustlineUpdatedEffectResponse()
    {
    }

    /// <inheritdoc />
    public TrustlineUpdatedEffectResponse(string limit, string assetType, string assetCode, string assetIssuer)
        : base(limit, assetType, assetCode, assetIssuer)
    {
    }

    public override int TypeId => 22;
}