namespace stellar_dotnet_sdk;

/// <summary>
/// </summary>
public class LedgerEntryState : LedgerEntryChange
{
    /// <summary>
    /// </summary>
    public readonly LedgerEntry State;

    public LedgerEntryState(xdr.LedgerEntry state)
    {
        State = LedgerEntry.FromXdr(state);
    }
}