using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Results;

/// <summary>
///     An offer is the building block of the offer book, they are automatically
///     claimed by payments when the price set by the owner is met.
///     For example an Offer is selling 10A where 1A is priced at 1.5B
/// </summary>
public class OfferEntry
{
    public enum OfferEntryFlags
    {
        /// <summary>
        ///     Issuer has authorized account to perform transactions with its credit.
        /// </summary>
        PASSIVE = 1
    }

    public OfferEntry(
        KeyPair seller,
        long offerId,
        Asset selling,
        Asset buying,
        string amount,
        Price price,
        OfferEntryFlags flags)
    {
        Seller = seller;
        OfferId = offerId;
        Selling = selling;
        Buying = buying;
        Amount = amount;
        Price = price;
        Flags = flags;
    }

    /// <summary>
    ///     Offer Seller.
    /// </summary>
    public KeyPair Seller { get; }

    /// <summary>
    ///     Unique Id of Offer.
    /// </summary>
    public long OfferId { get; }

    /// <summary>
    ///     Selling Asset.
    /// </summary>
    public Asset Selling { get; }

    /// <summary>
    ///     Buying Asset.
    /// </summary>
    public Asset Buying { get; }

    /// <summary>
    ///     Amount of Selling asset.
    /// </summary>
    public string Amount { get; }

    /// <summary>
    ///     Price for this offer, after fees.
    ///     Price of Selling in terms of Buying.
    ///     Price = AmountBuying / AmountSelling
    /// </summary>
    public Price Price { get; }

    /// <summary>
    ///     Flags for offer.
    /// </summary>
    public OfferEntryFlags Flags { get; }

    public static OfferEntry FromXdr(Xdr.OfferEntry entry)
    {
        return new OfferEntry(
            KeyPair.FromXdrPublicKey(entry.SellerID.InnerValue),
            entry.OfferID.InnerValue,
            Asset.FromXdr(entry.Selling),
            Asset.FromXdr(entry.Buying),
            StellarDotnetSdk.Amount.FromXdr(entry.Amount.InnerValue),
            Price.FromXdr(entry.Price),
            FlagsFromXdr(entry.Flags.InnerValue));
    }

    public static OfferEntryFlags FlagsFromXdr(uint flags)
    {
        return flags switch
        {
            0 => 0,
            1 => OfferEntryFlags.PASSIVE,
            _ => throw new ArgumentOutOfRangeException(nameof(flags), "Unknown OfferEntryFlags type.")
        };
    }
}