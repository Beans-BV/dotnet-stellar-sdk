using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class LedgerEntryChangeUpdated : LedgerEntryChange
{
    public xdr.LedgerEntryChange ToXdrLedgerEntryChange()
    {
        return new xdr.LedgerEntryChange
        {
            Discriminant =
                LedgerEntryChangeType.Create(LedgerEntryChangeType.LedgerEntryChangeTypeEnum.LEDGER_ENTRY_UPDATED),
            Updated = ChangedEntry.ToXdr()
        };
    }

    public static LedgerEntryChange FromXdrLedgerEntryChange(xdr.LedgerEntryChange xdrChange)
    {
        return new LedgerEntryChangeUpdated
        {
            ChangedEntry = LedgerEntry.FromXdr(xdrChange.Updated)
        };
    }
}