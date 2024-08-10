using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerKeys;

public class LedgerKeyOffer : LedgerKey
{
    public LedgerKeyOffer(string sellId, long offerId) : this(KeyPair.FromAccountId(sellId), offerId)
    {
    }

    public LedgerKeyOffer(KeyPair seller, long offerId)
    {
        Seller = seller;
        OfferId = offerId;
    }

    public KeyPair Seller { get; }
    public long OfferId { get; }

    public override Xdr.LedgerKey ToXdr()
    {
        return new Xdr.LedgerKey
        {
            Discriminant =
                new LedgerEntryType { InnerValue = LedgerEntryType.LedgerEntryTypeEnum.OFFER },
            Offer = new Xdr.LedgerKey.LedgerKeyOffer
            {
                SellerID = new AccountID(Seller.XdrPublicKey),
                OfferID = new Int64(OfferId),
            },
        };
    }

    public static LedgerKeyOffer FromXdr(Xdr.LedgerKey.LedgerKeyOffer xdr)
    {
        var seller = KeyPair.FromXdrPublicKey(xdr.SellerID.InnerValue);
        var offerId = xdr.OfferID.InnerValue;
        return new LedgerKeyOffer(seller, offerId);
    }
}