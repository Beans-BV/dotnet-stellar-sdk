namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
/// </summary>
public class LedgerEntryState : LedgerEntryChange
{
    /// <summary>
    /// </summary>
    public readonly LedgerEntry State;

    public LedgerEntryState(Xdr.LedgerEntry state)
    {
        State = LedgerEntry.FromXdr(state);
    }
}