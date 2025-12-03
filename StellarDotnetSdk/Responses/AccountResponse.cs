using System.Collections.Generic;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Accounts;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Represents an account on the Stellar network.
///     Contains account balances, signers, thresholds, and other account state.
/// </summary>
public sealed class AccountResponse : Response, ITransactionBuilderAccount, IPagingToken
{
    /// <summary>
    ///     A unique identifier for this account.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    ///     The number of subentries (trustlines, offers, signers, data entries) on this account.
    ///     Each subentry increases the account's minimum balance requirement.
    /// </summary>
    [JsonPropertyName("subentry_count")]
    public required int SubentryCount { get; init; }

    /// <summary>
    ///     The unsigned 32-bit ledger number of the sequence number's age.
    /// </summary>
    [JsonPropertyName("sequence_ledger")]
    public long? SequenceUpdatedAtLedger { get; init; }

    /// <summary>
    ///     The unsigned 64-bit UNIX timestamp of the sequence number's age.
    /// </summary>
    [JsonPropertyName("sequence_time")]
    public long? SequenceUpdatedAtTime { get; init; }

    /// <summary>
    ///     The account ID where inflation payouts are sent.
    ///     Note: Inflation is no longer active on the network.
    /// </summary>
    [JsonPropertyName("inflation_destination")]
    public string? InflationDestination { get; init; }

    /// <summary>
    ///     The domain that hosts this account's stellar.toml file.
    /// </summary>
    [JsonPropertyName("home_domain")]
    public string? HomeDomain { get; init; }

    /// <summary>
    ///     The thresholds required for different operation types.
    /// </summary>
    [JsonPropertyName("thresholds")]
    public required Thresholds Thresholds { get; init; }

    /// <summary>
    ///     The authorization flags set on this account.
    /// </summary>
    [JsonPropertyName("flags")]
    public required Flags Flags { get; init; }

    /// <summary>
    ///     The balances held by this account, one for each trusted asset.
    /// </summary>
    [JsonPropertyName("balances")]
    public required Balance[] Balances { get; init; }

    /// <summary>
    ///     The signers authorized to sign transactions for this account.
    /// </summary>
    [JsonPropertyName("signers")]
    public required Signer[] Signers { get; init; }

    /// <summary>
    ///     Links to related resources for this account.
    /// </summary>
    [JsonPropertyName("_links")]
    public required AccountResponseLinks Links { get; init; }

    /// <summary>
    ///     Key-value data entries attached to this account.
    ///     Values are base64-encoded.
    /// </summary>
    [JsonPropertyName("data")]
    public required Dictionary<string, string> Data { get; init; }

    /// <summary>
    ///     The number of reserves sponsored by this account.
    /// </summary>
    [JsonPropertyName("num_sponsoring")]
    public required int NumberSponsoring { get; init; }

    /// <summary>
    ///     The number of reserves sponsored for this account.
    /// </summary>
    [JsonPropertyName("num_sponsored")]
    public required int NumberSponsored { get; init; }

    /// <summary>
    ///     The sequence number of the last ledger in which this account was modified.
    /// </summary>
    [JsonPropertyName("last_modified_ledger")]
    public required long LastModifiedLedger { get; init; }

    /// <summary>
    ///     An ISO 8601 formatted string of when this account was last modified.
    /// </summary>
    [JsonPropertyName("last_modified_time")]
    public required string LastModifiedTime { get; init; }

    /// <inheritdoc />
    [JsonPropertyName("paging_token")]
    public required string PagingToken { get; init; }

    /// <summary>
    ///     The public key of this account (G...).
    /// </summary>
    [JsonPropertyName("account_id")]
    public required string AccountId { get; init; }

    /// <summary>
    ///     The current sequence number for this account.
    ///     Each transaction must use a sequence number one higher than the last.
    /// </summary>
    [JsonPropertyName("sequence")]
    public required long SequenceNumber { get; set; }

    /// <summary>
    ///     The next sequence number to use for a transaction.
    /// </summary>
    public long IncrementedSequenceNumber => SequenceNumber + 1;

    /// <summary>
    ///     The KeyPair for this account.
    /// </summary>
    public KeyPair KeyPair => KeyPair.FromAccountId(AccountId);

    /// <summary>
    ///     The account identifier for transaction building.
    /// </summary>
    public IAccountId MuxedAccount => KeyPair;

    /// <summary>
    ///     Increments the sequence number. Called after building a transaction.
    /// </summary>
    public void IncrementSequenceNumber()
    {
        SequenceNumber++;
    }

    /// <summary>
    ///     Represents a signer authorized to sign transactions for an account.
    /// </summary>
    public sealed class Signer
    {
        /// <summary>
        ///     The public key or pre-authorized transaction hash of this signer.
        /// </summary>
        [JsonPropertyName("key")]
        public required string Key { get; init; }

        /// <summary>
        ///     The type of signer: "ed25519_public_key", "sha256_hash", or "preauth_tx".
        /// </summary>
        [JsonPropertyName("type")]
        public required string Type { get; init; }

        /// <summary>
        ///     The weight of this signer (0-255).
        ///     A weight of 0 removes the signer.
        /// </summary>
        [JsonPropertyName("weight")]
        public required int Weight { get; init; }
    }
}