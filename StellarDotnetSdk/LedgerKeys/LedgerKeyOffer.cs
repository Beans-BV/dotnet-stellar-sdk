using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerKeys;

/// <summary>
///     Represents a ledger key for an offer entry on the Stellar decentralized exchange.
///     Used to look up offer data from the ledger, identified by the seller and offer ID.
/// </summary>
public class LedgerKeyOffer : LedgerKey
{
    /// <summary>
    ///     Constructs a <c>LedgerKeyOffer</c> from a Stellar account ID string and offer ID.
    /// </summary>
    /// <param name="sellId">The Stellar account ID (G... public key) of the seller.</param>
    /// <param name="offerId">The unique identifier of the offer.</param>
    public LedgerKeyOffer(string sellId, long offerId) : this(KeyPair.FromAccountId(sellId), offerId)
    {
    }

    /// <summary>
    ///     Constructs a <c>LedgerKeyOffer</c> from a key pair and offer ID.
    /// </summary>
    /// <param name="seller">The key pair of the account that created the offer.</param>
    /// <param name="offerId">The unique identifier of the offer.</param>
    public LedgerKeyOffer(KeyPair seller, long offerId)
    {
        Seller = seller;
        OfferId = offerId;
    }

    /// <summary>
    ///     The key pair of the account that created the offer.
    /// </summary>
    public KeyPair Seller { get; }

    /// <summary>
    ///     The unique identifier of the offer.
    /// </summary>
    public long OfferId { get; }

    /// <summary>
    ///     Serializes this ledger key to its XDR representation.
    /// </summary>
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

    /// <summary>
    ///     Deserializes a <see cref="LedgerKeyOffer" /> from its XDR representation.
    /// </summary>
    /// <param name="xdr">The XDR ledger key offer object.</param>
    public static LedgerKeyOffer FromXdr(Xdr.LedgerKey.LedgerKeyOffer xdr)
    {
        var seller = KeyPair.FromXdrPublicKey(xdr.SellerID.InnerValue);
        var offerId = xdr.OfferID.InnerValue;
        return new LedgerKeyOffer(seller, offerId);
    }
}