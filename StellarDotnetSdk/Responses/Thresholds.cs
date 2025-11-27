using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Operations have varying levels of access. This field specifies thresholds
///     for different access levels, as well as the weight of the master key.
/// </summary>
public sealed class Thresholds
{
    /// <summary>
    ///     The weight required for a valid transaction including the Allow Trust and Bump Sequence operations.
    /// </summary>
    [JsonPropertyName("low_threshold")]
    public required int LowThreshold { get; init; }

    /// <summary>
    ///     The weight required for a valid transaction including the Create Account, Payment, Path Payment,
    ///     Manage Buy Offer, Manage Sell Offer, Create Passive Sell Offer, Change Trust, Inflation,
    ///     and Manage Data operations.
    /// </summary>
    [JsonPropertyName("med_threshold")]
    public required int MedThreshold { get; init; }

    /// <summary>
    ///     The weight required for a valid transaction including the Account Merge and Set Options operations.
    /// </summary>
    [JsonPropertyName("high_threshold")]
    public required int HighThreshold { get; init; }
}