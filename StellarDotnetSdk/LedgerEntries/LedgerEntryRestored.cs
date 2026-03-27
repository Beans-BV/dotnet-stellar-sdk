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

    public LedgerEntryRestored(Xdr.LedgerEntry restoredEntry)
    {
        RestoredEntry = LedgerEntry.FromXdr(restoredEntry);
    }
}