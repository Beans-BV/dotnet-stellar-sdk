using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;
using Asset = StellarDotnetSdk.Assets.Asset;

namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents an offer ledger entry on the Stellar decentralized exchange (DEX).
/// </summary>
public class LedgerEntryOffer : LedgerEntry
{
    private LedgerEntryOffer(
        KeyPair sellerId,
        long offerId,
        long amount,
        Asset buying,
        Asset selling,
        Price price,
        uint flags)
    {
        SellerId = sellerId;
        OfferId = offerId;
        Amount = amount;
        Buying = buying;
        Selling = selling;
        Price = price;
        Flags = flags;
    }

    /// <summary>
    ///     The account ID of the offer creator (seller).
    /// </summary>
    public KeyPair SellerId { get; }

    /// <summary>
    ///     The unique identifier of this offer.
    /// </summary>
    public long OfferId { get; }

    /// <summary>
    ///     The amount of the <see cref="Selling" /> asset being offered, in stroops.
    /// </summary>
    public long Amount { get; }

    /// <summary>
    ///     The asset the offer creator wants to buy.
    /// </summary>
    public Asset Buying { get; }

    /// <summary>
    ///     The asset the offer creator is selling.
    /// </summary>
    public Asset Selling { get; }

    /// <summary>
    ///     The price as a ratio of selling to buying: price = selling / buying.
    /// </summary>
    public Price Price { get; }

    /// <summary>
    ///     Offer flags (e.g. PASSIVE flag for passive offers).
    /// </summary>
    public uint Flags { get; }

    /// <summary>
    ///     Extension fields for this offer entry, if present.
    /// </summary>
    public OfferEntryExtension? OfferExtension { get; private set; }

    /// <summary>
    ///     Creates the corresponding LedgerEntryOffer object from a <see cref="Xdr.LedgerEntry.LedgerEntryData" /> object.
    /// </summary>
    /// <param name="xdrLedgerEntryData">A <see cref="Xdr.LedgerEntry.LedgerEntryData" /> object.</param>
    /// <returns>A LedgerEntryOffer object.</returns>
    /// <exception cref="ArgumentException">Throws when the parameter is not a valid OfferEntry.</exception>
    public static LedgerEntryOffer FromXdrLedgerEntryData(Xdr.LedgerEntry.LedgerEntryData xdrLedgerEntryData)
    {
        if (xdrLedgerEntryData.Discriminant.InnerValue != LedgerEntryType.LedgerEntryTypeEnum.OFFER)
        {
            throw new ArgumentException("Not an OfferEntry", nameof(xdrLedgerEntryData));
        }

        return FromXdr(xdrLedgerEntryData.Offer);
    }

    private static LedgerEntryOffer FromXdr(OfferEntry xdrOfferEntry)
    {
        var ledgerEntryOffer = new LedgerEntryOffer(
            KeyPair.FromXdrPublicKey(xdrOfferEntry.SellerID.InnerValue),
            xdrOfferEntry.OfferID.InnerValue,
            xdrOfferEntry.Amount.InnerValue,
            Asset.FromXdr(xdrOfferEntry.Buying),
            Asset.FromXdr(xdrOfferEntry.Selling),
            new Price(xdrOfferEntry.Price.N.InnerValue, xdrOfferEntry.Price.D.InnerValue),
            xdrOfferEntry.Flags.InnerValue);

        if (xdrOfferEntry.Ext.Discriminant != 0)
        {
            ledgerEntryOffer.OfferExtension = OfferEntryExtension.FromXdr(xdrOfferEntry.Ext);
        }

        return ledgerEntryOffer;
    }
}