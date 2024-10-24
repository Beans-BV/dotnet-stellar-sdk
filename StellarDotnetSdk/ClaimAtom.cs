using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.LiquidityPool;
using StellarDotnetSdk.Xdr;
using Asset = StellarDotnetSdk.Assets.Asset;

namespace StellarDotnetSdk;

public abstract class ClaimAtom(
    Asset assetSold,
    string amountSold,
    Asset assetBought,
    string amountBought)
{
    /// <summary>
    ///     Asset taken from the owner.
    /// </summary>
    public Asset AssetSold { get; } = assetSold;

    /// <summary>
    ///     Amount taken from the owner.
    /// </summary>
    public string AmountSold { get; } = amountSold;

    /// <summary>
    ///     Asset sent to the owner.
    /// </summary>
    public Asset AssetBought { get; } = assetBought;

    /// <summary>
    ///     Amount sent to the owner.
    /// </summary>
    public string AmountBought { get; } = amountBought;

    public static ClaimAtom FromXdr(Xdr.ClaimAtom xdrClaimAtom)
    {
        return xdrClaimAtom.Discriminant.InnerValue switch
        {
            ClaimAtomType.ClaimAtomTypeEnum.CLAIM_ATOM_TYPE_V0 => ClaimAtomV0.FromXdr(xdrClaimAtom),
            ClaimAtomType.ClaimAtomTypeEnum.CLAIM_ATOM_TYPE_ORDER_BOOK => ClaimAtomOrderBook.FromXdr(xdrClaimAtom),
            ClaimAtomType.ClaimAtomTypeEnum.CLAIM_ATOM_TYPE_LIQUIDITY_POOL => ClaimAtomLiquidityPool.FromXdr(
                xdrClaimAtom),
            _ => throw new ArgumentOutOfRangeException(nameof(xdrClaimAtom), "Unknown ClaimAtom type."),
        };
    }
}

public class ClaimAtomOrderBook : ClaimAtom
{
    private ClaimAtomOrderBook(
        KeyPair seller,
        long offerId,
        Asset assetSold,
        string amountSold,
        Asset assetBought,
        string amountBought) : base(assetSold, amountSold, assetBought, amountBought)
    {
        Seller = seller;
        OfferId = offerId;
    }

    /// <summary>
    ///     Account that owns the offer.
    /// </summary>
    public KeyPair Seller { get; }

    /// <summary>
    ///     Emitted to identify the offer.
    /// </summary>
    public long OfferId { get; }

    public static ClaimAtomOrderBook FromXdr(Xdr.ClaimAtom xdrClaimAtom)
    {
        var offer = xdrClaimAtom.OrderBook;
        return new ClaimAtomOrderBook(
            KeyPair.FromXdrPublicKey(offer.SellerID.InnerValue),
            offer.OfferID.InnerValue,
            Asset.FromXdr(offer.AssetSold),
            Amount.FromXdr(offer.AmountSold.InnerValue),
            Asset.FromXdr(offer.AssetBought),
            Amount.FromXdr(offer.AmountBought.InnerValue));
    }
}

public class ClaimAtomV0 : ClaimAtom
{
    private ClaimAtomV0(
        KeyPair seller,
        long offerId,
        Asset assetSold,
        string amountSold,
        Asset assetBought,
        string amountBought) : base(assetSold, amountSold, assetBought, amountBought)
    {
        Seller = seller;
        OfferId = offerId;
    }

    public KeyPair Seller { get; }
    public long OfferId { get; }

    public static ClaimAtomV0 FromXdr(Xdr.ClaimAtom xdrClaimAtom)
    {
        var claimOfferAtomV0Xdr = xdrClaimAtom.V0;
        return new ClaimAtomV0(
            KeyPair.FromPublicKey(claimOfferAtomV0Xdr.SellerEd25519.InnerValue),
            claimOfferAtomV0Xdr.OfferID.InnerValue,
            Asset.FromXdr(claimOfferAtomV0Xdr.AssetSold),
            Amount.FromXdr(claimOfferAtomV0Xdr.AmountSold.InnerValue),
            Asset.FromXdr(claimOfferAtomV0Xdr.AssetBought),
            Amount.FromXdr(claimOfferAtomV0Xdr.AmountBought.InnerValue));
    }
}

public class ClaimAtomLiquidityPool : ClaimAtom
{
    private ClaimAtomLiquidityPool(
        LiquidityPoolId liquidityPoolId,
        Asset assetSold,
        string amountSold,
        Asset assetBought,
        string amountBought) : base(assetSold, amountSold, assetBought, amountBought)
    {
        LiquidityPoolId = liquidityPoolId;
    }

    /// <summary>
    ///     The Liquidity Pool ID
    /// </summary>
    public LiquidityPoolId LiquidityPoolId { get; }

    /// <summary>
    ///     Get new ClaimLiquidityAtom object parsed from an XDR ClaimLiquidityAtom.
    /// </summary>
    /// <param name="claimLiquidityAtomXdr"></param>
    /// <returns></returns>
    public static ClaimAtomLiquidityPool FromXdr(Xdr.ClaimAtom xdrClaimAtom)
    {
        var claimLiquidityAtomXdr = xdrClaimAtom.LiquidityPool;
        return new ClaimAtomLiquidityPool(
            LiquidityPoolId.FromXdr(claimLiquidityAtomXdr.LiquidityPoolID),
            Asset.FromXdr(claimLiquidityAtomXdr.AssetSold),
            Amount.FromXdr(claimLiquidityAtomXdr.AmountSold.InnerValue),
            Asset.FromXdr(claimLiquidityAtomXdr.AssetBought),
            Amount.FromXdr(claimLiquidityAtomXdr.AmountBought.InnerValue));
    }
}