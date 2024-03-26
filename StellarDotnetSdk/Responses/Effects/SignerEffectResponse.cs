using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;

/// <inheritdoc />
public class SignerEffectResponse : EffectResponse
{
    public SignerEffectResponse()
    {
    }

    public SignerEffectResponse(int weight, string publicKey)
    {
        Weight = weight;
        PublicKey = publicKey;
    }

    [JsonProperty(PropertyName = "weight")]
    public int Weight { get; private set; }

    [JsonProperty(PropertyName = "public_key")]
    public string PublicKey { get; private set; }
}