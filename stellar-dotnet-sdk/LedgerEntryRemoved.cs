namespace stellar_dotnet_sdk;

/// <summary>
///     Holds the key of the removed ledger entry in the contract call.
/// </summary>
public class LedgerEntryRemoved : LedgerEntryChange
{
    /// <summary>
    ///     The <c>LedgerKey</c> object of the ledger entry that was removed.
    /// </summary>
    public readonly LedgerKey RemovedKey;

    public LedgerEntryRemoved(xdr.LedgerKey removedKey)
    {
        RemovedKey = LedgerKey.FromXdr(removedKey);
    }
}