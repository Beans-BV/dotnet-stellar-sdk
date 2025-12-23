namespace StellarDotnetSdk.Sep.Sep0024;

/// <summary>
///     Request to query the anchor's fee schedule for deposit or withdrawal operations.
///     This request allows clients to query the exact fee that would be charged for
///     a specific deposit or withdrawal operation. This endpoint is optional for anchors
///     that can fully express their fee structure in the /info response using fee_fixed,
///     fee_percent, and fee_minimum fields.
///     Authentication: Required if feeEndpointInfo.authenticationRequired is true in
///     the /info response. Provide a SEP-10 JWT token in the Jwt property.
/// </summary>
public class Sep24FeeRequest
{
    /// <summary>
    ///     Gets or sets the kind of operation (deposit or withdraw).
    /// </summary>
    public string Operation { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the type of deposit or withdrawal (SEPA, bank_account, cash, etc.).
    ///     Optional. Used when the anchor supports multiple transfer methods for an asset.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    ///     Gets or sets the asset code.
    /// </summary>
    public string AssetCode { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the amount of the asset that will be deposited/withdrawn.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    ///     Gets or sets the JWT token previously received from the anchor via the SEP-10 authentication flow.
    ///     Required if the fee endpoint requires authentication.
    /// </summary>
    public string? Jwt { get; set; }
}

