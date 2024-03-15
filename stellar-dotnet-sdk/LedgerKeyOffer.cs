using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

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

    public override xdr.LedgerKey ToXdr()
    {
        return new xdr.LedgerKey
        {
            Discriminant =
                new LedgerEntryType { InnerValue = LedgerEntryType.LedgerEntryTypeEnum.OFFER },
            Offer = new xdr.LedgerKey.LedgerKeyOffer
            {
                SellerID = new AccountID(Seller.XdrPublicKey),
                OfferID = new Int64(OfferId)
            }
        };
    }

    public static LedgerKeyOffer FromXdr(xdr.LedgerKey.LedgerKeyOffer xdr)
    {
        var seller = KeyPair.FromXdrPublicKey(xdr.SellerID.InnerValue);
        var offerId = xdr.OfferID.InnerValue;
        return new LedgerKeyOffer(seller, offerId);
    }
}