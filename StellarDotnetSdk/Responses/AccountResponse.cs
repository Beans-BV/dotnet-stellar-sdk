using System.Collections.Generic;
using Newtonsoft.Json;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Responses.Effects;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Responses;
#nullable disable

public class AccountResponse : Response, ITransactionBuilderAccount
{
    [JsonProperty(PropertyName = "subentry_count")]
    public int SubentryCount { get; init; }

    [JsonProperty(PropertyName = "sequence_ledger")]
    public long? SequenceUpdatedAtLedger { get; init; }

    [JsonProperty(PropertyName = "sequence_time")]
    public long? SequenceUpdatedAtTime { get; init; }

    [JsonProperty(PropertyName = "inflation_destination")]
    public string InflationDestination { get; init; }

    [JsonProperty(PropertyName = "home_domain")]
    public string HomeDomain { get; init; }

    [JsonProperty(PropertyName = "thresholds")]
    public Thresholds Thresholds { get; init; }

    [JsonProperty(PropertyName = "flags")] public Flags Flags { get; init; }

    [JsonProperty(PropertyName = "balances")]
    public Balance[] Balances { get; init; }

    [JsonProperty(PropertyName = "signers")]
    public Signer[] Signers { get; init; }

    [JsonProperty(PropertyName = "_links")]
    public AccountResponseLinks Links { get; init; }

    [JsonProperty("Data")] public Dictionary<string, string> Data { get; init; }

    [JsonProperty(PropertyName = "account_id")]
    public string AccountId { get; init; }

    [JsonProperty(PropertyName = "sequence")]
    public long SequenceNumber { get; set; }

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
        [JsonProperty(PropertyName = "key")] public string Key { get; init; }

        [JsonProperty(PropertyName = "type")] public string Type { get; init; }

        [JsonProperty(PropertyName = "weight")]
        public int Weight { get; init; }
    }

    public class AccountResponseLinks
    {
        [JsonProperty(PropertyName = "effects")]
        public Link<Page<EffectResponse>> Effects { get; init; }

        [JsonProperty(PropertyName = "offers")]
        public Link<Page<OfferResponse>> Offers { get; init; }

        [JsonProperty(PropertyName = "operations")]
        public Link<Page<OperationResponse>> Operations { get; init; }

        [JsonProperty(PropertyName = "self")] public Link<AccountResponse> Self { get; init; }

        [JsonProperty(PropertyName = "transactions")]
        public Link<Page<TransactionResponse>> Transactions { get; init; }
    }
}