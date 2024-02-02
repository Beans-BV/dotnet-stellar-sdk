using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class LedgerEntryChangeState : LedgerEntryChange
{
    public xdr.LedgerEntryChange ToXdrLedgerEntryChange()
    {
        return new xdr.LedgerEntryChange
        {
            Discriminant =
                LedgerEntryChangeType.Create(LedgerEntryChangeType.LedgerEntryChangeTypeEnum.LEDGER_ENTRY_STATE),
            State = ChangedEntry.ToXdr()
        };
    }

    public static LedgerEntryChange FromXdrLedgerEntryChange(xdr.LedgerEntryChange xdrChange)
    {
        return new LedgerEntryChangeState
        {
            ChangedEntry = LedgerEntry.FromXdr(xdrChange.State)
        };
    }
}