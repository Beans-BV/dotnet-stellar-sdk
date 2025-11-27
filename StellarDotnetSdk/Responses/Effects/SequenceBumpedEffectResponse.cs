using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the sequence_bumped effect response.
///     This effect occurs when an account's sequence number is bumped.
/// </summary>
public sealed class SequenceBumpedEffectResponse : EffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 43;

    /// <summary>
    ///     The new sequence number of the account.
    /// </summary>
    [JsonPropertyName("new_seq")]
    public long NewSequence { get; init; }
}