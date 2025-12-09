using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Operations;

/// <summary>
///     Represents a set_options operation response.
///     Sets various options for an account, such as signers, thresholds, flags, and home domain.
/// </summary>
public class SetOptionsOperationResponse : OperationResponse
{
    public override int TypeId => 5;

    /// <summary>
    ///     The threshold value (0-255) for low security operations. Only present if modified in this operation.
    /// </summary>
    [JsonPropertyName("low_threshold")]
    public int? LowThreshold { get; init; }

    /// <summary>
    ///     The threshold value (0-255) for medium security operations. Only present if modified in this operation.
    /// </summary>
    [JsonPropertyName("med_threshold")]
    public int? MedThreshold { get; init; }

    /// <summary>
    ///     The threshold value (0-255) for high security operations. Only present if modified in this operation.
    /// </summary>
    [JsonPropertyName("high_threshold")]
    public int? HighThreshold { get; init; }

    /// <summary>
    ///     The account designated to receive inflation. Only present if set in this operation.
    /// </summary>
    [JsonPropertyName("inflation_dest")]
    public string? InflationDestination { get; init; }

    /// <summary>
    ///     The home domain of the account. Only present if set in this operation.
    /// </summary>
    [JsonPropertyName("home_domain")]
    public string? HomeDomain { get; init; }

    /// <summary>
    ///     The public key of the signer being added or modified. Only present if a signer operation is performed.
    /// </summary>
    [JsonPropertyName("signer_key")]
    public string? SignerKey { get; init; }

    /// <summary>
    ///     The weight of the signer (0-255). Only present if a signer is being added or modified.
    /// </summary>
    [JsonPropertyName("signer_weight")]
    public int? SignerWeight { get; init; }

    /// <summary>
    ///     The weight of the master key (0-255). Only present if modified in this operation.
    /// </summary>
    [JsonPropertyName("master_key_weight")]
    public int? MasterKeyWeight { get; init; }

    /// <summary>
    ///     Array of flag names to clear (e.g., ["auth_required", "auth_revocable"]). Only present if flags are being cleared.
    /// </summary>
    [JsonPropertyName("clear_flags_s")]
    public string[]? ClearFlags { get; init; }

    /// <summary>
    ///     Array of flag names to set (e.g., ["auth_required", "auth_revocable"]). Only present if flags are being set.
    /// </summary>
    [JsonPropertyName("set_flags_s")]
    public string[]? SetFlags { get; init; }
}