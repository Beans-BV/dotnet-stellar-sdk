namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents the state of a ledger entry before a change was applied (used for pre-image tracking).
/// </summary>
public class LedgerEntryState : LedgerEntryChange
{
    /// <summary>
    ///     The <c>LedgerEntry</c> object representing the state before the change.
    /// </summary>
    public readonly LedgerEntry State;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LedgerEntryState" /> class from an XDR ledger entry.
    /// </summary>
    /// <param name="state">The XDR ledger entry representing the pre-change state.</param>
    public LedgerEntryState(Xdr.LedgerEntry state)
    {
        State = LedgerEntry.FromXdr(state);
    }
}