namespace StellarDotnetSdk.LedgerEntries;

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