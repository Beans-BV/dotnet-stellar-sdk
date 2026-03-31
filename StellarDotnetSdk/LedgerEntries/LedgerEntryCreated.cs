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

    /// <summary>
    ///     Initializes a new instance of the <see cref="LedgerEntryCreated" /> class from an XDR ledger entry.
    /// </summary>
    /// <param name="createdEntry">The XDR ledger entry that was created.</param>
    public LedgerEntryCreated(Xdr.LedgerEntry createdEntry)
    {
        CreatedEntry = LedgerEntry.FromXdr(createdEntry);
    }
}