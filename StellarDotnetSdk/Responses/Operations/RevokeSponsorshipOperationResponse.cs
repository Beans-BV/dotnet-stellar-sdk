using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Operations;
#nullable disable

/// <summary>
///     Represents AccountMerge operation response.
/// </summary>
public class RevokeSponsorshipOperationResponse : OperationResponse
{
    public override int TypeId => 18;

    [JsonProperty(PropertyName = "account_id")]
    public string AccountId { get; init; }

    [JsonProperty(PropertyName = "claimable_balance_id")]
    public string ClaimableBalanceId { get; init; }

    [JsonProperty(PropertyName = "data_account_id")]
    public string DataAccountId { get; init; }

    [JsonProperty(PropertyName = "data_name")]
    public string DataName { get; init; }

    [JsonProperty(PropertyName = "offer_id")]
    public string OfferId { get; init; }

    [JsonProperty(PropertyName = "trustline_account_id")]
    public string TrustlineAccountId { get; init; }

    [JsonProperty(PropertyName = "trustline_asset")]
    public string TrustlineAsset { get; init; }

    [JsonProperty(PropertyName = "signer_account_id")]
    public string SignerAccountId { get; init; }

    [JsonProperty(PropertyName = "signer_key")]
    public string SignerKey { get; init; }
}