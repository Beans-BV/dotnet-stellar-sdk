namespace stellar_dotnet_sdk;

public class LedgerEntryCreated : LedgerEntryChange
{
    /// <summary>
    ///     The <c>LedgerEntry</c> object that was created.
    /// </summary>
    public readonly LedgerEntry CreatedEntry;

    public LedgerEntryCreated(xdr.LedgerEntry createdEntry)
    {
        CreatedEntry = LedgerEntry.FromXdr(createdEntry);
    }
}