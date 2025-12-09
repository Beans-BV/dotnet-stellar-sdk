using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Operations;

/// <summary>
///     Represents a revoke_sponsorship operation response.
///     This operation removes or transfers sponsorship of reserves for various ledger entries
///     (accounts, trustlines, offers, data, signers, or claimable balances).
///     Only one set of fields will be populated depending on the type of sponsorship being revoked.
/// </summary>
public class RevokeSponsorshipOperationResponse : OperationResponse
{
    public override int TypeId => 18;

    /// <summary>
    ///     The account ID whose sponsorship is being revoked.
    ///     Present when revoking sponsorship of an account.
    /// </summary>
    [JsonPropertyName("account_id")]
    public string? AccountId { get; init; }

    /// <summary>
    ///     The claimable balance ID whose sponsorship is being revoked.
    ///     Present when revoking sponsorship of a claimable balance.
    /// </summary>
    [JsonPropertyName("claimable_balance_id")]
    public string? ClaimableBalanceId { get; init; }

    /// <summary>
    ///     The account ID that holds the data entry whose sponsorship is being revoked.
    ///     Present when revoking sponsorship of a data entry.
    /// </summary>
    [JsonPropertyName("data_account_id")]
    public string? DataAccountId { get; init; }

    /// <summary>
    ///     The name of the data entry whose sponsorship is being revoked.
    ///     Present when revoking sponsorship of a data entry.
    /// </summary>
    [JsonPropertyName("data_name")]
    public string? DataName { get; init; }

    /// <summary>
    ///     The offer ID whose sponsorship is being revoked.
    ///     Present when revoking sponsorship of an offer.
    /// </summary>
    [JsonPropertyName("offer_id")]
    public string? OfferId { get; init; }

    /// <summary>
    ///     The account ID that holds the trustline whose sponsorship is being revoked.
    ///     Present when revoking sponsorship of a trustline.
    /// </summary>
    [JsonPropertyName("trustline_account_id")]
    public string? TrustlineAccountId { get; init; }

    /// <summary>
    ///     The asset of the trustline whose sponsorship is being revoked.
    ///     Present when revoking sponsorship of a trustline.
    /// </summary>
    [JsonPropertyName("trustline_asset")]
    public string? TrustlineAsset { get; init; }

    /// <summary>
    ///     The account ID that holds the signer whose sponsorship is being revoked.
    ///     Present when revoking sponsorship of a signer.
    /// </summary>
    [JsonPropertyName("signer_account_id")]
    public string? SignerAccountId { get; init; }

    /// <summary>
    ///     The signer key whose sponsorship is being revoked.
    ///     Present when revoking sponsorship of a signer.
    /// </summary>
    [JsonPropertyName("signer_key")]
    public string? SignerKey { get; init; }
}