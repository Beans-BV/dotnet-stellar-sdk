namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents signer_created effect response.
///     See: https://www.stellar.org/developers/horizon/reference/resources/effect.html
///     <seealso cref="Requests.EffectsRequestBuilder" />
///     <seealso cref="Server" />
/// </summary>
public class SignerCreatedEffectResponse : SignerEffectResponse
{
    public SignerCreatedEffectResponse()
    {
    }

    /// <inheritdoc />
    public SignerCreatedEffectResponse(int weight, string publicKey)
        : base(weight, publicKey)
    {
    }

    public override int TypeId => 10;
}