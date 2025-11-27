using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the data_sponsorship_updated effect response.
///     This effect occurs when a data entry's sponsor changes.
/// </summary>
public sealed class DataSponsorshipUpdatedEffectResponse : EffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 67;

    /// <summary>
    ///     The account ID of the new sponsor.
    /// </summary>
    [JsonPropertyName("new_sponsor")]
    public string? NewSponsor { get; init; }

    /// <summary>
    ///     The account ID of the former sponsor.
    /// </summary>
    [JsonPropertyName("former_sponsor")]
    public string? FormerSponsor { get; init; }

    /// <summary>
    ///     The name of the data entry.
    /// </summary>
    [JsonPropertyName("data_name")]
    public string? DataName { get; init; }
}