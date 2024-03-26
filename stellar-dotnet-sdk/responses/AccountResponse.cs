using System.Collections.Generic;
using Newtonsoft.Json;

namespace stellar_dotnet_sdk.responses;

public class AccountResponse : Response, ITransactionBuilderAccount
{
    public AccountResponse(string accountId, long sequenceNumber, Balance[] balances, Dictionary<string, string> data,
        Flags flags, string homeDomain, string inflationDestination, Signer[] signers, Thresholds thresholds,
        AccountResponseLinks links)
    {
        AccountId = accountId;
        SequenceNumber = sequenceNumber;
        Balances = balances;
        Data = data;
        Flags = flags;
        HomeDomain = homeDomain;
        InflationDestination = inflationDestination;
        Signers = signers;
        Thresholds = thresholds;
        Links = links;
    }

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

    public void SetSequenceNumber(long sequenceNumber)
    {
        SequenceNumber = sequenceNumber;
    }
}