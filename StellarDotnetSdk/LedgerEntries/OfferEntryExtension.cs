using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents extensions to an offer entry in the Stellar ledger. Currently a placeholder for future extension fields.
/// </summary>
public class OfferEntryExtension
{
    /// <summary>
    ///     Creates an <see cref="OfferEntryExtension" /> from an XDR <see cref="OfferEntry.OfferEntryExt" /> object.
    /// </summary>
    /// <param name="xdrExtension">The XDR extension object.</param>
    /// <returns>An <see cref="OfferEntryExtension" /> instance.</returns>
    public static OfferEntryExtension FromXdr(OfferEntry.OfferEntryExt xdrExtension)
    {
        return new OfferEntryExtension();
    }
}