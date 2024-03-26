namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents signer_removed effect response.
///     See: https://www.stellar.org/developers/horizon/reference/resources/effect.html
///     <seealso cref="Requests.EffectsRequestBuilder" />
///     <seealso cref="Server" />
/// </summary>
public class SignerRemovedEffectResponse : SignerEffectResponse
{
    public SignerRemovedEffectResponse()
    {
    }

    /// <inheritdoc />
    public SignerRemovedEffectResponse(int weight, string publicKey)
        : base(weight, publicKey)
    {
    }

    public override int TypeId => 11;
}