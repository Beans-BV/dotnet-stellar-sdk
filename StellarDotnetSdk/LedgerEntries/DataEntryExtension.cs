using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents extensions to a data entry in the Stellar ledger. Currently a placeholder for future extension fields.
/// </summary>
public class DataEntryExtension
{
    /// <summary>
    ///     Creates a <see cref="DataEntryExtension" /> from an XDR <see cref="DataEntry.DataEntryExt" /> object.
    /// </summary>
    /// <param name="xdrExtension">The XDR extension object.</param>
    /// <returns>A <see cref="DataEntryExtension" /> instance.</returns>
    public static DataEntryExtension FromXdr(DataEntry.DataEntryExt xdrExtension)
    {
        return new DataEntryExtension();
    }
}