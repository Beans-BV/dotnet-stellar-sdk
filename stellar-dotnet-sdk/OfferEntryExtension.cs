using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class OfferEntryExtension
{
    public static OfferEntryExtension FromXdr(OfferEntry.OfferEntryExt xdrExtension)
    {
        return new OfferEntryExtension();
    }

    public OfferEntry.OfferEntryExt ToXdr()
    {
        return new OfferEntry.OfferEntryExt
        {
            Discriminant = 0
        };
    }
}