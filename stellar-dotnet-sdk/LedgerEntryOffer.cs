using System;
using stellar_dotnet_sdk.xdr;
using Int64 = stellar_dotnet_sdk.xdr.Int64;

namespace stellar_dotnet_sdk;

public class LedgerEntryOffer : LedgerEntry
{
    public KeyPair SellerID { get; set; }

    public long OfferID { get; set; }

    public long Amount { get; set; }

    public Asset Buying { get; set; }

    public Asset Selling { get; set; }

    public Price Price { get; set; }

    public uint Flags { get; set; }

    public OfferEntryExtension? OfferExtension { get; set; }

    public static LedgerEntryOffer FromXdrLedgerEntry(xdr.LedgerEntry xdrLedgerEntry)
    {
        if (xdrLedgerEntry.Data.Discriminant.InnerValue != LedgerEntryType.LedgerEntryTypeEnum.OFFER)
            throw new ArgumentException("Not an OfferEntry", nameof(xdrLedgerEntry));

        var xdrOfferEntry = xdrLedgerEntry.Data.Offer;

        var ledgerEntryOffer = new LedgerEntryOffer
        {
            SellerID = KeyPair.FromXdrPublicKey(xdrOfferEntry.SellerID.InnerValue),
            OfferID = xdrOfferEntry.OfferID.InnerValue,
            Amount = xdrOfferEntry.Amount.InnerValue,
            Buying = Asset.FromXdr(xdrOfferEntry.Buying),
            Selling = Asset.FromXdr(xdrOfferEntry.Selling),
            Price = new Price(xdrOfferEntry.Price.N.InnerValue, xdrOfferEntry.Price.D.InnerValue),
            Flags = xdrOfferEntry.Flags.InnerValue,
        };

        if (xdrOfferEntry.Ext.Discriminant != 0)
            ledgerEntryOffer.OfferExtension = OfferEntryExtension.FromXdr(xdrOfferEntry.Ext);
        ExtraFieldsFromXdr(xdrLedgerEntry, ledgerEntryOffer);
        return ledgerEntryOffer;
    }

    public OfferEntry ToXdr()
    {
        return new OfferEntry
        {
            SellerID = new AccountID(SellerID.XdrPublicKey),
            OfferID = new Int64(OfferID),
            Selling = Selling.ToXdr(),
            Buying = Buying.ToXdr(),
            Amount = new Int64(Amount),
            Price = Price.ToXdr(),
            Flags = new Uint32(Flags),
            Ext = OfferExtension?.ToXdr() ?? new OfferEntry.OfferEntryExt()
        };
    }
}