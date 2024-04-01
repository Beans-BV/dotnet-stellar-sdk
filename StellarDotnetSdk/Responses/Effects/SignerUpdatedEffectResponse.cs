namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents signer_updated effect response.
///     See: https://www.stellar.org/developers/horizon/reference/resources/effect.html
///     <seealso cref="Requests.EffectsRequestBuilder" />
///     <seealso cref="Server" />
/// </summary>
public class SignerUpdatedEffectResponse : SignerEffectResponse
{
    public SignerUpdatedEffectResponse()
    {
    }

    /// <inheritdoc />
    public SignerUpdatedEffectResponse(int weight, string publicKey)
        : base(weight, publicKey)
    {
    }

    public override int TypeId => 12;
}