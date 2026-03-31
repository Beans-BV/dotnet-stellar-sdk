namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents a ledger entry change where an existing entry was updated.
/// </summary>
public class LedgerEntryUpdated : LedgerEntryChange
{
    /// <summary>
    ///     The <c>LedgerEntry</c> object that was updated.
    /// </summary>
    public readonly LedgerEntry UpdatedEntry;

    public LedgerEntryUpdated(Xdr.LedgerEntry createdEntry)
    {
        UpdatedEntry = LedgerEntry.FromXdr(createdEntry);
    }
}