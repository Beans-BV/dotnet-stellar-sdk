namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents a ledger entry change where a previously archived Soroban entry was restored.
/// </summary>
public class LedgerEntryRestored : LedgerEntryChange
{
    /// <summary>
    ///     The <c>LedgerEntry</c> object that was restored.
    /// </summary>
    public readonly LedgerEntry RestoredEntry;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LedgerEntryRestored" /> class from an XDR ledger entry.
    /// </summary>
    /// <param name="restoredEntry">The XDR ledger entry that was restored.</param>
    public LedgerEntryRestored(Xdr.LedgerEntry restoredEntry)
    {
        RestoredEntry = LedgerEntry.FromXdr(restoredEntry);
    }
}