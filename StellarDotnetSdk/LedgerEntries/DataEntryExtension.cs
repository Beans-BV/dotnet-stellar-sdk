using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

public class DataEntryExtension
{
    public static DataEntryExtension FromXdr(DataEntry.DataEntryExt xdrExtension)
    {
        return new DataEntryExtension();
    }
}