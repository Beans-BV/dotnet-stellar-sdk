using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the data_sponsorship_created effect response.
///     This effect occurs when a data entry becomes sponsored.
/// </summary>
public sealed class DataSponsorshipCreatedEffectResponse : EffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 66;

    /// <summary>
    ///     The account ID of the sponsor.
    /// </summary>
    [JsonPropertyName("sponsor")]
    public required string Sponsor { get; init; }

    /// <summary>
    ///     The name of the sponsored data entry.
    /// </summary>
    [JsonPropertyName("data_name")]
    public required string DataName { get; init; }
}