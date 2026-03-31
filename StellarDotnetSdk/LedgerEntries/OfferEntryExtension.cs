using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents extensions to an offer entry in the Stellar ledger. Currently a placeholder for future extension fields.
/// </summary>
public class OfferEntryExtension
{
    public static OfferEntryExtension FromXdr(OfferEntry.OfferEntryExt xdrExtension)
    {
        return new OfferEntryExtension();
    }
}