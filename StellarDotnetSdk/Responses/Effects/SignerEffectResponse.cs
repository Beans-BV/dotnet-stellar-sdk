using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

public class SignerEffectResponse : EffectResponse
{
    [JsonProperty(PropertyName = "weight")]
    public int Weight { get; init; }

    [JsonProperty(PropertyName = "public_key")]
    public string PublicKey { get; init; }
}

/// <summary>
///     Represents signer_removed effect response.
/// </summary>
public class SignerRemovedEffectResponse : SignerEffectResponse
{
    public override int TypeId => 11;
}

/// <summary>
///     Represents signer_updated effect response.
/// </summary>
public class SignerUpdatedEffectResponse : SignerEffectResponse
{
    public override int TypeId => 12;
}

/// <summary>
///     Represents signer_created effect response.
/// </summary>
public class SignerCreatedEffectResponse : SignerEffectResponse
{
    public override int TypeId => 10;
}