using System;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Represents a claimable balance on the Stellar network.
///     A claimable balance is an amount of an asset that has been transferred from one account
///     to be claimed by one or more potential claimants based on specified conditions.
/// </summary>
public sealed class ClaimableBalanceResponse : Response
{
    /// <summary>
    ///     The unique identifier for this claimable balance.
    ///     This is a hex-encoded preimage of the claimable balance.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    ///     A cursor value for use in pagination.
    /// </summary>
    [JsonPropertyName("paging_token")]
    public required string PagingToken { get; init; }

    /// <summary>
    ///     The asset held in this claimable balance in canonical form (Code:Issuer or "native").
    /// </summary>
    [JsonPropertyName("asset")]
    public required string Asset { get; init; }

    /// <summary>
    ///     The amount of the asset in this claimable balance.
    ///     Represented as a string to preserve precision.
    /// </summary>
    [JsonPropertyName("amount")]
    public required string Amount { get; init; }

    /// <summary>
    ///     The account ID that is sponsoring this claimable balance's base reserve.
    /// </summary>
    [JsonPropertyName("sponsor")]
    public string? Sponsor { get; init; }

    /// <summary>
    ///     The sequence number of the last ledger in which this balance was modified.
    /// </summary>
    [JsonPropertyName("last_modified_ledger")]
    public required long LastModifiedLedger { get; init; }

    /// <summary>
    ///     The time this claimable balance was last modified.
    /// </summary>
    [JsonPropertyName("last_modified_time")]
    public required DateTimeOffset LastModifiedTime { get; init; }

    /// <summary>
    ///     The list of accounts that can claim this balance and the conditions under which they can do so.
    /// </summary>
    [JsonPropertyName("claimants")]
    public required Claimant[] Claimants { get; init; }

    /// <summary>
    ///     Links to related resources for this claimable balance.
    /// </summary>
    [JsonPropertyName("_links")]
    public required ClaimableBalanceResponseLinks Links { get; init; }

    /// <summary>
    ///     Flags denote the enabling/disabling of certain claimable balance issuer privileges.
    /// </summary>
    [JsonPropertyName("flags")]
    public required ClaimableBalanceFlags Flags { get; init; }

    /// <summary>
    ///     Represents claimable balance flags.
    /// </summary>
    public sealed class ClaimableBalanceFlags
    {
        /// <summary>
        ///     When set to true, this claimable balance can be clawed back.
        /// </summary>
        [JsonPropertyName("clawback_enabled")]
        public required bool ClawbackEnabled { get; init; }
    }

    /// <summary>
    ///     Links to related resources for a claimable balance.
    /// </summary>
    public sealed class ClaimableBalanceResponseLinks
    {
        /// <summary>
        ///     Link to the operations related to this claimable balance.
        /// </summary>
        [JsonPropertyName("operations")]
        public required Link<Page<OperationResponse>> Operations { get; init; }

        /// <summary>
        ///     Link to this claimable balance.
        /// </summary>
        [JsonPropertyName("self")]
        public required Link<ClaimableBalanceResponse> Self { get; init; }

        /// <summary>
        ///     Link to the transactions related to this claimable balance.
        /// </summary>
        [JsonPropertyName("transactions")]
        public required Link<Page<TransactionResponse>> Transactions { get; init; }
    }
}