using System.Collections.Generic;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Responses.Effects;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Responses;
#nullable disable

public class AccountResponse : Response, ITransactionBuilderAccount
{
    [JsonPropertyName("subentry_count")] public int SubentryCount { get; init; }

    [JsonPropertyName("sequence_ledger")] public long? SequenceUpdatedAtLedger { get; init; }

    [JsonPropertyName("sequence_time")] public long? SequenceUpdatedAtTime { get; init; }

    [JsonPropertyName("inflation_destination")]
    public string InflationDestination { get; init; }

    [JsonPropertyName("home_domain")] public string HomeDomain { get; init; }

    [JsonPropertyName("thresholds")] public Thresholds Thresholds { get; init; }

    [JsonPropertyName("flags")] public Flags Flags { get; init; }

    [JsonPropertyName("balances")] public Balance[] Balances { get; init; }

    [JsonPropertyName("signers")] public Signer[] Signers { get; init; }

    [JsonPropertyName("_links")] public AccountResponseLinks Links { get; init; }

    [JsonPropertyName("data")] public Dictionary<string, string> Data { get; init; }

    [JsonPropertyName("account_id")] public string AccountId { get; init; }

    [JsonPropertyName("sequence")] public long SequenceNumber { get; set; }

    public long IncrementedSequenceNumber => SequenceNumber + 1;

    public KeyPair KeyPair => KeyPair.FromAccountId(AccountId);

    public IAccountId MuxedAccount => KeyPair;

    public void IncrementSequenceNumber()
    {
        SequenceNumber++;
    }

    /// <summary>
    ///     Represents account signers.
    /// </summary>
    public class Signer
    {
        [JsonPropertyName("key")] public string Key { get; init; }

        [JsonPropertyName("type")] public string Type { get; init; }

        [JsonPropertyName("weight")] public int Weight { get; init; }
    }

    public class AccountResponseLinks
    {
        [JsonPropertyName("effects")] public Link<Page<EffectResponse>> Effects { get; init; }

        [JsonPropertyName("offers")] public Link<Page<OfferResponse>> Offers { get; init; }

        [JsonPropertyName("operations")] public Link<Page<OperationResponse>> Operations { get; init; }

        [JsonPropertyName("self")] public Link<AccountResponse> Self { get; init; }

        [JsonPropertyName("transactions")] public Link<Page<TransactionResponse>> Transactions { get; init; }
    }
}