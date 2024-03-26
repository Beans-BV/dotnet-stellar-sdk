namespace stellar_dotnet_sdk;

public class LedgerEntryUpdated : LedgerEntryChange
{
    /// <summary>
    ///     The <c>LedgerEntry</c> object that was updated.
    /// </summary>
    public readonly LedgerEntry UpdatedEntry;

    public LedgerEntryUpdated(xdr.LedgerEntry createdEntry)
    {
        UpdatedEntry = LedgerEntry.FromXdr(createdEntry);
    }
}