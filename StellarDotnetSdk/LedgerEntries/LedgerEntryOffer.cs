using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;
using Asset = StellarDotnetSdk.Assets.Asset;

namespace StellarDotnetSdk.LedgerEntries;

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

    public KeyPair SellerId { get; }

    public long OfferId { get; }

    public long Amount { get; }

    public Asset Buying { get; }

    public Asset Selling { get; }

    public Price Price { get; }

    public uint Flags { get; }

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