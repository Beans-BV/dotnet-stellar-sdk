using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Operations;

#nullable disable
/// <inheritdoc />
/// <summary>
///     This operation sets the options for an account.
/// </summary>
public class SetOptionsOperationResponse : OperationResponse
{
    public override int TypeId => 5;

    /// <summary>
    ///     A number from 0-255 representing the threshold this account sets on all operations it performs that have a low
    ///     threshold.
    /// </summary>
    [JsonPropertyName("low_threshold")]
    public int LowThreshold { get; init; }

    /// <summary>
    ///     A number from 0-255 representing the threshold this account sets on all operations it performs that have a medium
    ///     threshold.
    /// </summary>
    [JsonPropertyName("med_threshold")]
    public int MedThreshold { get; init; }

    /// <summary>
    ///     A number from 0-255 representing the threshold this account sets on all operations it performs that have a high
    ///     threshold.
    /// </summary>
    [JsonPropertyName("high_threshold")]
    public int HighThreshold { get; init; }

    /// <summary>
    ///     Account of the inflation destination.
    /// </summary>
    [JsonPropertyName("inflation_dest")]
    public string InflationDestination { get; init; }

    /// <summary>
    ///     Gets the home domain of an account.
    /// </summary>
    [JsonPropertyName("home_domain")]
    public string HomeDomain { get; init; }

    /// <summary>
    ///     A signer from an account.
    /// </summary>
    [JsonPropertyName("signer_key")]
    public string SignerKey { get; init; }

    /// <summary>
    ///     Weight of the signer.
    /// </summary>
    [JsonPropertyName("signer_weight")]
    public int SignerWeight { get; init; }

    /// <summary>
    ///     Weight of the master key.
    /// </summary>
    [JsonPropertyName("master_key_weight")]
    public int MasterKeyWeight { get; init; }

    /// <summary>
    ///     Indicates which flags to clear. For details about the flags, please refer to the accounts doc. The bit mask integer
    ///     subtracts
    ///     from the existing flags of the account. This allows for setting specific bits without knowledge of existing flags.
    /// </summary>
    [JsonPropertyName("clear_flags_s")]
    public string[] ClearFlags { get; init; }

    /// <summary>
    ///     Indicates which flags to set. For details about the flags, please refer to the accounts doc. The bit mask integer
    ///     adds onto the
    ///     existing flags of the account. This allows for setting specific bits without knowledge of existing flags.
    /// </summary>
    [JsonPropertyName("set_flags_s")]
    public string[] SetFlags { get; init; }
}