using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.LiquidityPool;
using StellarDotnetSdk.Xdr;
using Asset = StellarDotnetSdk.Assets.Asset;

namespace StellarDotnetSdk;

/// <summary>
///     Represents a single trade that was executed during a path payment or offer operation on the Stellar network.
///     Each claim atom contains the assets and amounts exchanged.
/// </summary>
/// <param name="assetSold">The asset taken from the owner.</param>
/// <param name="amountSold">The amount taken from the owner.</param>
/// <param name="assetBought">The asset sent to the owner.</param>
/// <param name="amountBought">The amount sent to the owner.</param>
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

    /// <summary>
    ///     Creates a <see cref="ClaimAtom" /> from an XDR claim atom, dispatching to the correct subclass
    ///     based on the discriminant type.
    /// </summary>
    /// <param name="xdrClaimAtom">The XDR claim atom to convert.</param>
    /// <returns>A concrete <see cref="ClaimAtom" /> subclass instance.</returns>
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

/// <summary>
///     Represents a claim atom from an order book trade on the Stellar decentralized exchange.
///     Contains the seller, offer ID, and the assets and amounts exchanged.
/// </summary>
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

    /// <summary>
    ///     Creates a <see cref="ClaimAtomOrderBook" /> from an XDR claim atom containing order book data.
    /// </summary>
    /// <param name="xdrClaimAtom">The XDR claim atom to convert.</param>
    /// <returns>A new <see cref="ClaimAtomOrderBook" /> instance.</returns>
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

/// <summary>
///     Represents a V0 claim atom from a trade on the Stellar network.
///     This is the original format that identifies the seller by their Ed25519 public key
///     rather than a full account ID.
/// </summary>
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

    /// <summary>Account that owns the offer, identified by Ed25519 public key.</summary>
    public KeyPair Seller { get; }

    /// <summary>The offer ID involved in the trade.</summary>
    public long OfferId { get; }

    /// <summary>
    ///     Creates a <see cref="ClaimAtomV0" /> from an XDR claim atom containing V0 data.
    /// </summary>
    /// <param name="xdrClaimAtom">The XDR claim atom to convert.</param>
    /// <returns>A new <see cref="ClaimAtomV0" /> instance.</returns>
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

/// <summary>
///     Represents a claim atom from a liquidity pool trade on the Stellar network.
///     Contains the liquidity pool ID and the assets and amounts exchanged.
/// </summary>
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