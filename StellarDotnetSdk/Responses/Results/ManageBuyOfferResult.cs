using System;
using System.Linq;
using StellarDotnetSdk.Xdr;
using ResultCodeEnum = StellarDotnetSdk.Xdr.ManageBuyOfferResultCode.ManageBuyOfferResultCodeEnum;

namespace StellarDotnetSdk.Responses.Results;

public class ManageBuyOfferResult : OperationResult
{
    public static ManageBuyOfferResult FromXdr(Xdr.ManageBuyOfferResult result)
    {
        return result.Discriminant.InnerValue switch
        {
            ResultCodeEnum.MANAGE_BUY_OFFER_SUCCESS => ManageBuyOfferSuccess.FromXdr(result.Success),
            ResultCodeEnum.MANAGE_BUY_OFFER_MALFORMED => new ManageBuyOfferMalformed(),
            ResultCodeEnum.MANAGE_BUY_OFFER_UNDERFUNDED => new ManageBuyOfferUnderfunded(),
            ResultCodeEnum.MANAGE_BUY_OFFER_SELL_NO_TRUST => new ManageBuyOfferSellNoTrust(),
            ResultCodeEnum.MANAGE_BUY_OFFER_BUY_NO_TRUST => new ManageBuyOfferBuyNoTrust(),
            ResultCodeEnum.MANAGE_BUY_OFFER_SELL_NOT_AUTHORIZED => new ManageBuyOfferSellNotAuthorized(),
            ResultCodeEnum.MANAGE_BUY_OFFER_BUY_NOT_AUTHORIZED => new ManageBuyOfferBuyNotAuthorized(),
            ResultCodeEnum.MANAGE_BUY_OFFER_LINE_FULL => new ManageBuyOfferLineFull(),
            ResultCodeEnum.MANAGE_BUY_OFFER_CROSS_SELF => new ManageBuyOfferCrossSelf(),
            ResultCodeEnum.MANAGE_BUY_OFFER_SELL_NO_ISSUER => new ManageBuyOfferSellNoIssuer(),
            ResultCodeEnum.MANAGE_BUY_OFFER_BUY_NO_ISSUER => new ManageBuyOfferBuyNoIssuer(),
            ResultCodeEnum.MANAGE_BUY_OFFER_NOT_FOUND => new ManageBuyOfferNotFound(),
            ResultCodeEnum.MANAGE_BUY_OFFER_LOW_RESERVE => new ManageBuyOfferLowReserve(),
            _ => throw new ArgumentOutOfRangeException(nameof(result), "Unknown ManageOfferResult type.")
        };
    }
}

public class ManageBuyOfferSuccess : ManageBuyOfferResult
{
    protected ManageBuyOfferSuccess(ClaimAtom[] offersClaimed)
    {
        OffersClaimed = offersClaimed;
    }

    public override bool IsSuccess => true;

    /// <summary>
    ///     Offers that got claimed while creating this offer.
    /// </summary>
    public ClaimAtom[] OffersClaimed { get; }

    public static ManageBuyOfferSuccess FromXdr(ManageOfferSuccessResult result)
    {
        return result.Offer.Discriminant.InnerValue switch
        {
            ManageOfferEffect.ManageOfferEffectEnum.MANAGE_OFFER_CREATED => ManageBuyOfferCreated.FromXdr(result),
            ManageOfferEffect.ManageOfferEffectEnum.MANAGE_OFFER_UPDATED => ManageBuyOfferUpdated.FromXdr(result),
            ManageOfferEffect.ManageOfferEffectEnum.MANAGE_OFFER_DELETED => ManageBuyOfferDeleted.FromXdr(result),
            _ => throw new ArgumentOutOfRangeException(nameof(result), "Unknown ManageBuyOfferSuccess type.")
        };
    }
}

public class ManageBuyOfferDeleted : ManageBuyOfferSuccess
{
    private ManageBuyOfferDeleted(ClaimAtom[] offersClaimed) : base(offersClaimed)
    {
    }

    public static ManageBuyOfferDeleted FromXdr(ManageOfferSuccessResult successResult)
    {
        return new ManageBuyOfferDeleted(successResult.OffersClaimed.Select(ClaimAtom.FromXdr).ToArray());
    }
}

public class ManageBuyOfferUpdated : ManageBuyOfferSuccess
{
    private ManageBuyOfferUpdated(OfferEntry offer, ClaimAtom[] offersClaimed) : base(offersClaimed)
    {
        Offer = offer;
    }

    /// <summary>
    ///     The offer that was updated.
    /// </summary>
    public OfferEntry Offer { get; }

    public static ManageBuyOfferUpdated FromXdr(ManageOfferSuccessResult successResult)
    {
        return new ManageBuyOfferUpdated(
            OfferEntry.FromXdr(successResult.Offer.Offer),
            successResult.OffersClaimed.Select(ClaimAtom.FromXdr).ToArray());
    }
}

public class ManageBuyOfferCreated : ManageBuyOfferSuccess
{
    private ManageBuyOfferCreated(OfferEntry offer, ClaimAtom[] offersClaimed) : base(offersClaimed)
    {
        Offer = offer;
    }

    /// <summary>
    ///     The offer that was created.
    /// </summary>
    public OfferEntry Offer { get; }

    public static ManageBuyOfferCreated FromXdr(ManageOfferSuccessResult successResult)
    {
        return new ManageBuyOfferCreated(
            OfferEntry.FromXdr(successResult.Offer.Offer),
            successResult.OffersClaimed.Select(ClaimAtom.FromXdr).ToArray());
    }
}

/// <summary>
///     The input is incorrect and would result in an invalid offer.
/// </summary>
public class ManageBuyOfferMalformed : ManageBuyOfferResult;

/// <summary>
///     The account creating the offer does not have a trustline for the asset it is selling.
/// </summary>
public class ManageBuyOfferSellNoTrust : ManageBuyOfferResult;

/// <summary>
///     The account creating the offer does not have a trustline for the asset it is buying.
/// </summary>
public class ManageBuyOfferBuyNoTrust : ManageBuyOfferResult;

/// <summary>
///     The account creating the offer is not authorized to sell this asset.
/// </summary>
public class ManageBuyOfferBuyNotAuthorized : ManageBuyOfferResult;

/// <summary>
///     The account creating the offer is not authorized to buy this asset.
/// </summary>
public class ManageBuyOfferSellNotAuthorized : ManageBuyOfferResult;

/// <summary>
///     The account creating the offer does not have sufficient limits to receive buying and still satisfy its buying
///     liabilities.
/// </summary>
public class ManageBuyOfferLineFull : ManageBuyOfferResult;

/// <summary>
///     The account creating the offer does not have sufficient limits to send selling and still satisfy its selling
///     liabilities. Note that if selling XLM then the account must additionally maintain its minimum XLM reserve, which is
///     calculated assuming this offer will not completely execute immediately.
/// </summary>
public class ManageBuyOfferUnderfunded : ManageBuyOfferResult;

/// <summary>
///     The account has opposite offer of equal or lesser price active, so the account creating this offer would
///     immediately cross itself.
/// </summary>
public class ManageBuyOfferCrossSelf : ManageBuyOfferResult;

/// <summary>
///     TODO Missing on official docs
/// </summary>
public class ManageBuyOfferSellNoIssuer : ManageBuyOfferResult;

/// <summary>
///     TODO Missing on official docs
/// </summary>
public class ManageBuyOfferBuyNoIssuer : ManageBuyOfferResult;

/// <summary>
///     An offer with that offerID cannot be found.
/// </summary>
public class ManageBuyOfferNotFound : ManageBuyOfferResult;

/// <summary>
///     The account creating this offer does not have enough XLM to satisfy the minimum XLM reserve increase caused by
///     adding a subentry and still satisfy its XLM selling liabilities. For every offer an account creates, the minimum
///     amount of XLM that account must hold will increase.
/// </summary>
public class ManageBuyOfferLowReserve : ManageBuyOfferResult;