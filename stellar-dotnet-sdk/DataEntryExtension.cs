using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class DataEntryExtension
{
    public static DataEntryExtension FromXdr(DataEntry.DataEntryExt xdrExtension)
    {
        return new DataEntryExtension();
    }

    public DataEntry.DataEntryExt ToXdr()
    {
        return new DataEntry.DataEntryExt
        {
            Discriminant = 0
        };
    }
}