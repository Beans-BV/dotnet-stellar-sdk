using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

public class OfferEntryExtension
{
    public static OfferEntryExtension FromXdr(OfferEntry.OfferEntryExt xdrExtension)
    {
        return new OfferEntryExtension();
    }
}