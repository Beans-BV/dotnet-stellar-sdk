using Newtonsoft.Json;

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
    [JsonProperty(PropertyName = "low_threshold")]
    public int LowThreshold { get; init; }

    /// <summary>
    ///     A number from 0-255 representing the threshold this account sets on all operations it performs that have a medium
    ///     threshold.
    /// </summary>
    [JsonProperty(PropertyName = "med_threshold")]
    public int MedThreshold { get; init; }

    /// <summary>
    ///     A number from 0-255 representing the threshold this account sets on all operations it performs that have a high
    ///     threshold.
    /// </summary>
    [JsonProperty(PropertyName = "high_threshold")]
    public int HighThreshold { get; init; }

    /// <summary>
    ///     Account of the inflation destination.
    /// </summary>
    [JsonProperty(PropertyName = "inflation_dest")]
    public string InflationDestination { get; init; }

    /// <summary>
    ///     Gets the home domain of an account.
    /// </summary>
    [JsonProperty(PropertyName = "home_domain")]
    public string HomeDomain { get; init; }

    /// <summary>
    ///     A signer from an account.
    /// </summary>
    [JsonProperty(PropertyName = "signer_key")]
    public string SignerKey { get; init; }

    /// <summary>
    ///     Weight of the signer.
    /// </summary>
    [JsonProperty(PropertyName = "signer_weight")]
    public int SignerWeight { get; init; }

    /// <summary>
    ///     Weight of the master key.
    /// </summary>
    [JsonProperty(PropertyName = "master_key_weight")]
    public int MasterKeyWeight { get; init; }

    /// <summary>
    ///     Indicates which flags to clear. For details about the flags, please refer to the accounts doc. The bit mask integer
    ///     subtracts
    ///     from the existing flags of the account. This allows for setting specific bits without knowledge of existing flags.
    /// </summary>
    [JsonProperty(PropertyName = "clear_flags_s")]
    public string[] ClearFlags { get; init; }

    /// <summary>
    ///     Indicates which flags to set. For details about the flags, please refer to the accounts doc. The bit mask integer
    ///     adds onto the
    ///     existing flags of the account. This allows for setting specific bits without knowledge of existing flags.
    /// </summary>
    [JsonProperty(PropertyName = "set_flags_s")]
    public string[] SetFlags { get; init; }
}