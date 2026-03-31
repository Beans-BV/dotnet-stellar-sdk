using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Base class for ledger entry changes.
/// </summary>
public abstract class LedgerEntryChange
{
    /// <summary>
    ///     Creates a <c>LedgerEntryChange</c> object from an <c>xdr.LedgerEntryChange</c> object.
    /// </summary>
    /// <param name="xdrLedgerEntryChange">An <c>xdr.LedgerEntryChange</c> object to be converted.</param>
    /// <returns>A <c>LedgerEntryChange</c> object.</returns>
    public static LedgerEntryChange FromXdr(Xdr.LedgerEntryChange xdrLedgerEntryChange)
    {
        return xdrLedgerEntryChange.Discriminant.InnerValue switch
        {
            LedgerEntryChangeType.LedgerEntryChangeTypeEnum.LEDGER_ENTRY_CREATED =>
                new LedgerEntryCreated(xdrLedgerEntryChange.Created),
            LedgerEntryChangeType.LedgerEntryChangeTypeEnum.LEDGER_ENTRY_UPDATED =>
                new LedgerEntryUpdated(xdrLedgerEntryChange.Updated),
            LedgerEntryChangeType.LedgerEntryChangeTypeEnum.LEDGER_ENTRY_REMOVED =>
                new LedgerEntryRemoved(xdrLedgerEntryChange.Removed),
            LedgerEntryChangeType.LedgerEntryChangeTypeEnum.LEDGER_ENTRY_STATE =>
                new LedgerEntryState(xdrLedgerEntryChange.State),
            LedgerEntryChangeType.LedgerEntryChangeTypeEnum.LEDGER_ENTRY_RESTORED =>
                new LedgerEntryRestored(xdrLedgerEntryChange.Restored),
            _ => throw new InvalidOperationException("Unknown LedgerEntryChange type."),
        };
    }

    /// <summary>
    ///     Creates a <see cref="LedgerEntryChange" /> from a base-64 encoded XDR string.
    /// </summary>
    /// <param name="xdrBase64">A base-64 encoded XDR string of a <c>LedgerEntryChange</c>.</param>
    /// <returns>A <see cref="LedgerEntryChange" /> subclass instance.</returns>
    public static LedgerEntryChange FromXdrBase64(string xdrBase64)
    {
        var bytes = Convert.FromBase64String(xdrBase64);
        var reader = new XdrDataInputStream(bytes);
        var thisXdr = Xdr.LedgerEntryChange.Decode(reader);
        return FromXdr(thisXdr);
    }
}