namespace StellarDotnetSdk.LedgerEntries;

public class LedgerEntryCreated : LedgerEntryChange
{
    /// <summary>
    ///     The <c>LedgerEntry</c> object that was created.
    /// </summary>
    public readonly LedgerEntry CreatedEntry;

    public LedgerEntryCreated(Xdr.LedgerEntry createdEntry)
    {
        CreatedEntry = LedgerEntry.FromXdr(createdEntry);
    }
}