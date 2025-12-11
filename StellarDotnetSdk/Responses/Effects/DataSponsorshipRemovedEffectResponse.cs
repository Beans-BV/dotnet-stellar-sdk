using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the data_sponsorship_removed effect response.
///     This effect occurs when a data entry's sponsorship is removed.
/// </summary>
public sealed class DataSponsorshipRemovedEffectResponse : EffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 68;

    /// <summary>
    ///     The account ID of the former sponsor.
    /// </summary>
    [JsonPropertyName("former_sponsor")]
    public required string FormerSponsor { get; init; }

    /// <summary>
    ///     The name of the data entry.
    /// </summary>
    [JsonPropertyName("data_name")]
    public required string DataName { get; init; }
}