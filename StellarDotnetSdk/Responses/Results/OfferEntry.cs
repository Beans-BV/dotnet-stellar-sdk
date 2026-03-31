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
    /// <summary>
    ///     Defines the flags that can be set on an offer entry.
    /// </summary>
    public enum OfferEntryFlags
    {
        /// <summary>
        ///     Issuer has authorized account to perform transactions with its credit.
        /// </summary>
        PASSIVE = 1,
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="OfferEntry" /> class.
    /// </summary>
    /// <param name="seller">The account that created the offer.</param>
    /// <param name="offerId">The unique identifier of the offer.</param>
    /// <param name="selling">The asset being sold.</param>
    /// <param name="buying">The asset being bought.</param>
    /// <param name="amount">The amount of the selling asset.</param>
    /// <param name="price">The price of selling in terms of buying.</param>
    /// <param name="flags">The flags set on the offer.</param>
    public OfferEntry(
        KeyPair seller,
        long offerId,
        Asset selling,
        Asset buying,
        string amount,
        StellarDotnetSdk.Price price,
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
    public StellarDotnetSdk.Price Price { get; }

    /// <summary>
    ///     Flags for offer.
    /// </summary>
    public OfferEntryFlags Flags { get; }

    /// <summary>
    ///     Creates a new <see cref="OfferEntry" /> from the given XDR representation.
    /// </summary>
    /// <param name="entry">The XDR offer entry.</param>
    /// <returns>A new <see cref="OfferEntry" /> instance.</returns>
    public static OfferEntry FromXdr(Xdr.OfferEntry entry)
    {
        return new OfferEntry(
            KeyPair.FromXdrPublicKey(entry.SellerID.InnerValue),
            entry.OfferID.InnerValue,
            Asset.FromXdr(entry.Selling),
            Asset.FromXdr(entry.Buying),
            StellarDotnetSdk.Amount.FromXdr(entry.Amount.InnerValue),
            StellarDotnetSdk.Price.FromXdr(entry.Price),
            FlagsFromXdr(entry.Flags.InnerValue));
    }

    /// <summary>
    ///     Converts a raw XDR flags value to the corresponding <see cref="OfferEntryFlags" /> enum.
    /// </summary>
    /// <param name="flags">The raw XDR flags value.</param>
    /// <returns>The corresponding <see cref="OfferEntryFlags" /> value.</returns>
    public static OfferEntryFlags FlagsFromXdr(uint flags)
    {
        return flags switch
        {
            0 => 0,
            1 => OfferEntryFlags.PASSIVE,
            _ => throw new ArgumentOutOfRangeException(nameof(flags), "Unknown OfferEntryFlags type."),
        };
    }
}