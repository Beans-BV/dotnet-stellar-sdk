using System;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

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
    public static LedgerEntryChange FromXdr(xdr.LedgerEntryChange xdrLedgerEntryChange)
    {
        return xdrLedgerEntryChange.Discriminant.InnerValue switch
        {
            LedgerEntryChangeType.LedgerEntryChangeTypeEnum.LEDGER_ENTRY_CREATED => new LedgerEntryCreated
                (xdrLedgerEntryChange.Created),
            LedgerEntryChangeType.LedgerEntryChangeTypeEnum.LEDGER_ENTRY_UPDATED => new LedgerEntryUpdated
                (xdrLedgerEntryChange.Updated),
            LedgerEntryChangeType.LedgerEntryChangeTypeEnum.LEDGER_ENTRY_REMOVED => new LedgerEntryRemoved(
                xdrLedgerEntryChange.Removed),
            LedgerEntryChangeType.LedgerEntryChangeTypeEnum.LEDGER_ENTRY_STATE => new LedgerEntryState
                (xdrLedgerEntryChange.State),
            _ => throw new InvalidOperationException("Unknown LedgerEntryChange type.")
        };
    }
}