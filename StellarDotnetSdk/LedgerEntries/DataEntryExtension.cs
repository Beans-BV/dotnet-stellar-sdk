using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents extensions to a data entry in the Stellar ledger. Currently a placeholder for future extension fields.
/// </summary>
public class DataEntryExtension
{
    public static DataEntryExtension FromXdr(DataEntry.DataEntryExt xdrExtension)
    {
        return new DataEntryExtension();
    }
}