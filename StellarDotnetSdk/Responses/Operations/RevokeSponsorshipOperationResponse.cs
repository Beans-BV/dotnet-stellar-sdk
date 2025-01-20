﻿using System.Text.Json;
using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Operations;
#nullable disable

/// <summary>
///     Represents AccountMerge operation response.
/// </summary>
public class RevokeSponsorshipOperationResponse : OperationResponse
{
    public override int TypeId => 18;

    [JsonPropertyName("account_id")]
    public string AccountID { get; init; }

    [JsonPropertyName("claimable_balance_id")]
    public string ClaimableBalanceID { get; init; }

    [JsonPropertyName("data_account_id")]
    public string DataAccountID { get; init; }

    [JsonPropertyName("data_name")]
    public string DataName { get; init; }

    [JsonPropertyName("offer_id")]
    public string OfferID { get; init; }

    [JsonPropertyName("trustline_account_id")]
    public string TrustlineAccountID { get; init; }

    [JsonPropertyName("trustline_asset")]
    public string TrustlineAsset { get; init; }

    [JsonPropertyName("signer_account_id")]
    public string SignerAccountID { get; init; }

    [JsonPropertyName("signer_key")]
    public string SignerKey { get; init; }
}