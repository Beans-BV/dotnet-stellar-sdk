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

    /// <summary>
    ///     Initializes a new instance of the <see cref="LedgerEntryUpdated" /> class from an XDR ledger entry.
    /// </summary>
    /// <param name="createdEntry">The XDR ledger entry that was updated.</param>
    public LedgerEntryUpdated(Xdr.LedgerEntry createdEntry)
    {
        UpdatedEntry = LedgerEntry.FromXdr(createdEntry);
    }
}