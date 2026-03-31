using System;
using System.Linq;
using StellarDotnetSdk.Xdr;
using ResultCodeEnum = StellarDotnetSdk.Xdr.ManageSellOfferResultCode.ManageSellOfferResultCodeEnum;

namespace StellarDotnetSdk.Responses.Results;

/// <summary>
///     Represents the result of a manage sell offer operation.
/// </summary>
public class ManageSellOfferResult : OperationResult
{
    /// <summary>
    ///     Creates the appropriate <see cref="ManageSellOfferResult" /> subclass from the given XDR representation.
    /// </summary>
    /// <param name="result">The XDR manage sell offer result.</param>
    /// <returns>A <see cref="ManageSellOfferResult" /> instance representing the operation outcome.</returns>
    public static ManageSellOfferResult FromXdr(Xdr.ManageSellOfferResult result)
    {
        return result.Discriminant.InnerValue switch
        {
            ResultCodeEnum.MANAGE_SELL_OFFER_SUCCESS => ManageSellOfferSuccess.FromXdr(result.Success),
            ResultCodeEnum.MANAGE_SELL_OFFER_MALFORMED => new ManageSellOfferMalformed(),
            ResultCodeEnum.MANAGE_SELL_OFFER_UNDERFUNDED => new ManageSellOfferUnderfunded(),
            ResultCodeEnum.MANAGE_SELL_OFFER_SELL_NO_TRUST => new ManageSellOfferSellNoTrust(),
            ResultCodeEnum.MANAGE_SELL_OFFER_BUY_NO_TRUST => new ManageSellOfferBuyNoTrust(),
            ResultCodeEnum.MANAGE_SELL_OFFER_SELL_NOT_AUTHORIZED => new ManageSellOfferSellNotAuthorized(),
            ResultCodeEnum.MANAGE_SELL_OFFER_BUY_NOT_AUTHORIZED => new ManageSellOfferBuyNotAuthorized(),
            ResultCodeEnum.MANAGE_SELL_OFFER_LINE_FULL => new ManageSellOfferLineFull(),
            ResultCodeEnum.MANAGE_SELL_OFFER_CROSS_SELF => new ManageSellOfferCrossSelf(),
            ResultCodeEnum.MANAGE_SELL_OFFER_SELL_NO_ISSUER => new ManageSellOfferSellNoIssuer(),
            ResultCodeEnum.MANAGE_SELL_OFFER_BUY_NO_ISSUER => new ManageSellOfferBuyNoIssuer(),
            ResultCodeEnum.MANAGE_SELL_OFFER_NOT_FOUND => new ManageSellOfferNotFound(),
            ResultCodeEnum.MANAGE_SELL_OFFER_LOW_RESERVE => new ManageSellOfferLowReserve(),
            _ => throw new ArgumentOutOfRangeException(nameof(result), "Unknown ManageOfferResult type."),
        };
    }
}

/// <summary>
///     Represents a successful manage sell offer operation result.
/// </summary>
public class ManageSellOfferSuccess : ManageSellOfferResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ManageSellOfferSuccess" /> class.
    /// </summary>
    /// <param name="offersClaimed">The offers that were claimed while executing this operation.</param>
    protected ManageSellOfferSuccess(ClaimAtom[] offersClaimed)
    {
        OffersClaimed = offersClaimed;
    }

    /// <inheritdoc />
    public override bool IsSuccess => true;

    /// <summary>
    ///     Offers that got claimed while creating this offer.
    /// </summary>
    public ClaimAtom[] OffersClaimed { get; }

    /// <summary>
    ///     Creates the appropriate <see cref="ManageSellOfferSuccess" /> subclass from the given XDR representation.
    /// </summary>
    /// <param name="result">The XDR manage offer success result.</param>
    /// <returns>A <see cref="ManageSellOfferSuccess" /> instance (Created, Updated, or Deleted).</returns>
    public static ManageSellOfferSuccess FromXdr(ManageOfferSuccessResult result)
    {
        return result.Offer.Discriminant.InnerValue switch
        {
            ManageOfferEffect.ManageOfferEffectEnum.MANAGE_OFFER_CREATED => ManageSellOfferCreated.FromXdr(result),
            ManageOfferEffect.ManageOfferEffectEnum.MANAGE_OFFER_UPDATED => ManageSellOfferUpdated.FromXdr(result),
            ManageOfferEffect.ManageOfferEffectEnum.MANAGE_OFFER_DELETED => ManageSellOfferDeleted.FromXdr(result),
            _ => throw new ArgumentOutOfRangeException(nameof(result), "Unknown ManageSellOfferSuccess type."),
        };
    }
}

/// <summary>
///     Represents a successful manage sell offer result where the offer was deleted.
/// </summary>
public class ManageSellOfferDeleted : ManageSellOfferSuccess
{
    private ManageSellOfferDeleted(ClaimAtom[] offersClaimed) : base(offersClaimed)
    {
    }

    /// <summary>
    ///     Creates a new <see cref="ManageSellOfferDeleted" /> from the given XDR representation.
    /// </summary>
    /// <param name="successResult">The XDR manage offer success result.</param>
    /// <returns>A new <see cref="ManageSellOfferDeleted" /> instance.</returns>
    public static ManageSellOfferDeleted FromXdr(ManageOfferSuccessResult successResult)
    {
        return new ManageSellOfferDeleted(successResult.OffersClaimed.Select(ClaimAtom.FromXdr).ToArray());
    }
}

/// <summary>
///     Represents a successful manage sell offer result where a new offer was created.
/// </summary>
public class ManageSellOfferCreated : ManageSellOfferSuccess
{
    private ManageSellOfferCreated(OfferEntry offer, ClaimAtom[] offersClaimed) : base(offersClaimed)
    {
        Offer = offer;
    }

    /// <summary>
    ///     The offer that was created.
    /// </summary>
    public OfferEntry Offer { get; }

    /// <summary>
    ///     Creates a new <see cref="ManageSellOfferCreated" /> from the given XDR representation.
    /// </summary>
    /// <param name="successResult">The XDR manage offer success result.</param>
    /// <returns>A new <see cref="ManageSellOfferCreated" /> instance.</returns>
    public static ManageSellOfferCreated FromXdr(ManageOfferSuccessResult successResult)
    {
        return new ManageSellOfferCreated(
            OfferEntry.FromXdr(successResult.Offer.Offer),
            successResult.OffersClaimed.Select(ClaimAtom.FromXdr).ToArray());
    }
}

/// <summary>
///     Represents a successful manage sell offer result where the offer was updated.
/// </summary>
public class ManageSellOfferUpdated : ManageSellOfferSuccess
{
    private ManageSellOfferUpdated(OfferEntry offer, ClaimAtom[] offersClaimed) : base(offersClaimed)
    {
        Offer = offer;
    }

    /// <summary>
    ///     The offer that was updated.
    /// </summary>
    public OfferEntry Offer { get; }

    /// <summary>
    ///     Creates a new <see cref="ManageSellOfferUpdated" /> from the given XDR representation.
    /// </summary>
    /// <param name="successResult">The XDR manage offer success result.</param>
    /// <returns>A new <see cref="ManageSellOfferUpdated" /> instance.</returns>
    public static ManageSellOfferUpdated FromXdr(ManageOfferSuccessResult successResult)
    {
        return new ManageSellOfferUpdated(
            OfferEntry.FromXdr(successResult.Offer.Offer),
            successResult.OffersClaimed.Select(ClaimAtom.FromXdr).ToArray());
    }
}

/// <summary>
///     The input is incorrect and would result in an invalid offer.
/// </summary>
public class ManageSellOfferMalformed : ManageSellOfferResult;

/// <summary>
///     The account creating the offer does not have a trustline for the asset it is selling.
/// </summary>
public class ManageSellOfferSellNoTrust : ManageSellOfferResult;

/// <summary>
///     The account creating the offer does not have a trustline for the asset it is buying.
/// </summary>
public class ManageSellOfferBuyNoTrust : ManageSellOfferResult;

/// <summary>
///     The account creating the offer is not authorized to sell this asset.
/// </summary>
public class ManageSellOfferSellNotAuthorized : ManageSellOfferResult;

/// <summary>
///     The account creating the offer is not authorized to buy this asset.
/// </summary>
public class ManageSellOfferBuyNotAuthorized : ManageSellOfferResult;

/// <summary>
///     The account creating the offer does not have sufficient limits to receive buying and still satisfy its buying
///     liabilities.
/// </summary>
public class ManageSellOfferLineFull : ManageSellOfferResult;

/// <summary>
///     The account creating the offer does not have sufficient limits to send selling and still satisfy its selling
///     liabilities. Note that if selling XLM then the account must additionally maintain its minimum XLM reserve, which is
///     calculated assuming this offer will not completely execute immediately.
/// </summary>
public class ManageSellOfferUnderfunded : ManageSellOfferResult;

/// <summary>
///     The account has opposite offer of equal or lesser price active, so the account creating this offer would
///     immediately cross itself.
/// </summary>
public class ManageSellOfferCrossSelf : ManageSellOfferResult;

/// <summary>
///     TODO Missing on official docs
/// </summary>
public class ManageSellOfferSellNoIssuer : ManageSellOfferResult;

/// <summary>
///     TODO Missing on official docs
/// </summary>
public class ManageSellOfferBuyNoIssuer : ManageSellOfferResult;

/// <summary>
///     An offer with that offerID cannot be found.
/// </summary>
public class ManageSellOfferNotFound : ManageSellOfferResult;

/// <summary>
///     The account creating this offer does not have enough XLM to satisfy the minimum XLM reserve increase caused by
///     adding a subentry and still satisfy its XLM selling liabilities. For every offer an account creates, the minimum
///     amount of XLM that account must hold will increase.
/// </summary>
public class ManageSellOfferLowReserve : ManageSellOfferResult;