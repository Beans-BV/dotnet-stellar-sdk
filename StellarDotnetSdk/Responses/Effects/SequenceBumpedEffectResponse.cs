using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents sequence_bumped effect response.
/// </summary>
public class SequenceBumpedEffectResponse : EffectResponse
{
    public override int TypeId => 43;

    [JsonProperty(PropertyName = "new_seq")]
    public long NewSequence { get; init; }
}