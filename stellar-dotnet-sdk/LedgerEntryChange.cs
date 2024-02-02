using System;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public abstract class LedgerEntryChange
{
    public LedgerEntry ChangedEntry { get; set; }

    public xdr.LedgerEntryChange ToXdr()
    {
        return this switch
        {
            LedgerEntryChangeCreated created => created.ToXdrLedgerEntryChange(),
            LedgerEntryChangeUpdated updated => updated.ToXdrLedgerEntryChange(),
            LedgerEntryChangeState state => state.ToXdrLedgerEntryChange(),
            LedgerEntryChangeRemoved removed => removed.ToXdrLedgerEntryChange(),
            _ => throw new InvalidOperationException("Unknown LedgerEntryChange type")
        };
    }

    public static LedgerEntryChange FromXdr(xdr.LedgerEntryChange xdrChange)
    {
        return xdrChange.Discriminant.InnerValue switch
        {
            LedgerEntryChangeType.LedgerEntryChangeTypeEnum.LEDGER_ENTRY_CREATED => LedgerEntryChangeCreated
                .FromXdrLedgerEntryChange(xdrChange),
            LedgerEntryChangeType.LedgerEntryChangeTypeEnum.LEDGER_ENTRY_UPDATED => LedgerEntryChangeUpdated
                .FromXdrLedgerEntryChange(xdrChange),
            LedgerEntryChangeType.LedgerEntryChangeTypeEnum.LEDGER_ENTRY_REMOVED => LedgerEntryChangeRemoved
                .FromXdrLedgerEntryChange(xdrChange),
            LedgerEntryChangeType.LedgerEntryChangeTypeEnum.LEDGER_ENTRY_STATE => LedgerEntryChangeState
                .FromXdrLedgerEntryChange(xdrChange),
            _ => throw new InvalidOperationException("Unknown LedgerEntryChange type")
        };
    }
}