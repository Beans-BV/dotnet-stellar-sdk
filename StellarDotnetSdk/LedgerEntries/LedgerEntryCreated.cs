namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents a ledger entry change where a new entry was created.
/// </summary>
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