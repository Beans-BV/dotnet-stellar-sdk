using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Operations;

/// <summary>
///     Represents an account_merge operation response.
///     Merges one account into another, transferring all XLM balance and deleting the source account.
/// </summary>
public class AccountMergeOperationResponse : OperationResponse
{
    public override int TypeId => 8;

    /// <summary>
    ///     The account that was merged (removed).
    /// </summary>
    [JsonPropertyName("account")]
    public required string Account { get; init; }

    /// <summary>
    ///     The muxed account representation of the merged account, if applicable.
    /// </summary>
    [JsonPropertyName("account_muxed")]
    public string? AccountMuxed { get; init; }

    /// <summary>
    ///     The muxed account ID of the merged account, if applicable.
    /// </summary>
    [JsonPropertyName("account_muxed_id")]
    public ulong? AccountMuxedId { get; init; }

    /// <summary>
    ///     The destination account that received the merged account's XLM balance.
    /// </summary>
    [JsonPropertyName("into")]
    public required string Into { get; init; }

    /// <summary>
    ///     The muxed account representation of the destination account, if applicable.
    /// </summary>
    [JsonPropertyName("into_muxed")]
    public string? IntoMuxed { get; init; }

    /// <summary>
    ///     The muxed account ID of the destination account, if applicable.
    /// </summary>
    [JsonPropertyName("into_muxed_id")]
    public ulong? IntoMuxedID { get; init; }
}